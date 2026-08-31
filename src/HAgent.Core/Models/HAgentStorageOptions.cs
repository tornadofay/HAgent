using System;
using System.IO;

namespace HAgent.Models
{
    public enum HAgentStorageType
    {
        File,
        SqlServer,
        MySql
    }

    public sealed class HAgentDatabaseStorageOptions
    {
        public HAgentDatabaseStorageOptions()
        {
            ServerName = string.Empty;
            Port = 0;
            UserName = string.Empty;
            PasswordSecretId = string.Empty;
        }

        public string ServerName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string PasswordSecretId { get; set; }

        public int GetEffectivePort(HAgentStorageType storageType)
        {
            if (Port > 0)
                return Port;
            return storageType == HAgentStorageType.MySql ? 3306 : 1433;
        }
    }

    /// <summary>
    /// HAgent-owned persistence configuration. This configuration describes only HAgent's internal storage backend.
    /// Database credentials are intentionally excluded and belong to the secret/runtime connection boundary.
    /// </summary>
    public sealed class HAgentStorageOptions
    {
        public HAgentStorageOptions()
        {
            StorageType = HAgentStorageType.File;
            ApplicationName = string.Empty;
            RootPath = AppContext.BaseDirectory;
            DatabaseName = string.Empty;
            ServerName = string.Empty;
            Port = 0;
            UserName = string.Empty;
            PasswordSecretId = string.Empty;
            SqlServer = new HAgentDatabaseStorageOptions();
            MySql = new HAgentDatabaseStorageOptions();
        }

        public HAgentStorageType StorageType { get; set; }
        public string ApplicationName { get; set; }
        public string RootPath { get; set; }

        // Legacy shared database settings retained only so older storage.json files can still be read.
        // Runtime resolution must use the backend-specific profile properties below.
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string PasswordSecretId { get; set; }

        public HAgentDatabaseStorageOptions SqlServer { get; set; }
        public HAgentDatabaseStorageOptions MySql { get; set; }

        public HAgentDatabaseStorageOptions GetDatabaseProfile(HAgentStorageType storageType)
        {
            switch (storageType)
            {
                case HAgentStorageType.SqlServer:
                    if (SqlServer == null) SqlServer = new HAgentDatabaseStorageOptions();
                    return SqlServer;
                case HAgentStorageType.MySql:
                    if (MySql == null) MySql = new HAgentDatabaseStorageOptions();
                    return MySql;
                default:
                    return null;
            }
        }

        public string GetEffectiveDatabaseName()
        {
            return BuildDatabaseName(ApplicationName);
        }

        public int GetEffectivePort()
        {
            var profile = GetDatabaseProfile(StorageType);
            return profile == null ? 0 : profile.GetEffectivePort(StorageType);
        }

        public string GetEffectiveRootPath()
        {
            var root = string.IsNullOrWhiteSpace(RootPath) ? AppContext.BaseDirectory : RootPath;
            return Path.Combine(root, "HAgentData");
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApplicationName))
                throw new ArgumentException("Application name is required.", nameof(ApplicationName));

            if (StorageType == HAgentStorageType.File)
            {
                if (string.IsNullOrWhiteSpace(GetEffectiveRootPath()))
                    throw new ArgumentException("A storage root path is required.", nameof(RootPath));
                return;
            }

            var profile = GetDatabaseProfile(StorageType);
            if (profile == null || string.IsNullOrWhiteSpace(profile.ServerName))
                throw new ArgumentException("Server name is required for database storage.", nameof(profile.ServerName));

            var port = profile.GetEffectivePort(StorageType);
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(profile.Port), "Database port must be between 1 and 65535.");
        }

        public static string BuildDatabaseName(string applicationName)
        {
            return SanitizeDatabaseName(applicationName) + "-ai";
        }

        private static string SanitizeDatabaseName(string value)
        {
            var source = (value ?? string.Empty).Trim().ToLowerInvariant();
            var buffer = new char[source.Length];
            var count = 0;
            foreach (var c in source)
            {
                if (char.IsLetterOrDigit(c))
                    buffer[count++] = c;
                else if (c == '-' || c == '_' || char.IsWhiteSpace(c))
                    buffer[count++] = '-';
            }

            var result = new string(buffer, 0, count).Trim('-');
            while (result.Contains("--")) result = result.Replace("--", "-");
            if (string.IsNullOrWhiteSpace(result)) result = "hagent";
            return result;
        }
    }
}

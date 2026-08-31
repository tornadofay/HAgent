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
        }

        public HAgentStorageType StorageType { get; set; }
        public string ApplicationName { get; set; }
        public string RootPath { get; set; }
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string PasswordSecretId { get; set; }

        public string GetEffectiveDatabaseName()
        {
            if (!string.IsNullOrWhiteSpace(DatabaseName))
                return SanitizeDatabaseName(DatabaseName);
            return BuildDatabaseName(ApplicationName);
        }

        public int GetEffectivePort()
        {
            if (Port > 0)
                return Port;
            return StorageType == HAgentStorageType.MySql ? 3306 : 1433;
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

            if (string.IsNullOrWhiteSpace(ServerName))
                throw new ArgumentException("Server name is required for database storage.", nameof(ServerName));
            if (GetEffectivePort() < 1 || GetEffectivePort() > 65535)
                throw new ArgumentOutOfRangeException(nameof(Port), "Database port must be between 1 and 65535.");
            if (string.IsNullOrWhiteSpace(GetEffectiveDatabaseName()))
                throw new ArgumentException("Database name is required for database storage.", nameof(DatabaseName));
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

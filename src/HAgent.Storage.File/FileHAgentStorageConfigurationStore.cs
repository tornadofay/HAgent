using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Storage.File
{
    /// <summary>
    /// Persists HAgent internal-storage configuration metadata. Database passwords are never stored here.
    /// </summary>
    public sealed class FileHAgentStorageConfigurationStore
    {
        private readonly string _path;
        private static readonly JsonSerializerOptions Options = CreateOptions();

        public FileHAgentStorageConfigurationStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Storage configuration path is required.", nameof(path));
            _path = path;
        }

        public string Path { get { return _path; } }

        public Task<HAgentStorageOptions> LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!System.IO.File.Exists(_path))
                return Task.FromResult<HAgentStorageOptions>(null);

            try
            {
                var json = System.IO.File.ReadAllText(_path);
                var options = JsonSerializer.Deserialize<HAgentStorageOptions>(json, Options);
                if (options != null)
                    MigrateLegacySelectedProfile(options);
                return Task.FromResult(options);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("HAgent storage configuration could not be read.", ex);
            }
        }

        public Task SaveAsync(HAgentStorageOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var directory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            var tempPath = _path + ".tmp";
            System.IO.File.WriteAllText(tempPath, JsonSerializer.Serialize(options, Options));
            if (System.IO.File.Exists(_path)) System.IO.File.Replace(tempPath, _path, _path + ".bak", true);
            else System.IO.File.Move(tempPath, _path);

            var savedJson = System.IO.File.ReadAllText(_path);
            var savedOptions = JsonSerializer.Deserialize<HAgentStorageOptions>(savedJson, Options);
            if (savedOptions == null || savedOptions.StorageType != options.StorageType)
                throw new InvalidDataException("HAgent storage configuration was not persisted with the selected storage backend.");

            if (options.StorageType != HAgentStorageType.File)
            {
                var expected = options.GetDatabaseProfile(options.StorageType);
                var actual = savedOptions.GetDatabaseProfile(savedOptions.StorageType);
                if (!ProfilesEqual(expected, actual))
                    throw new InvalidDataException("HAgent database connection profile was not persisted correctly for the selected storage backend.");
            }

            return Task.CompletedTask;
        }

        private static void MigrateLegacySelectedProfile(HAgentStorageOptions options)
        {
            if (options == null || options.StorageType == HAgentStorageType.File)
                return;

            var profile = options.GetDatabaseProfile(options.StorageType);
            if (profile == null || !string.IsNullOrWhiteSpace(profile.ServerName))
                return;

            if (string.IsNullOrWhiteSpace(options.ServerName))
                return;

            profile.ServerName = options.ServerName;
            profile.Port = options.Port;
            profile.UserName = options.UserName ?? string.Empty;
            profile.PasswordSecretId = options.PasswordSecretId ?? string.Empty;
        }

        private static bool ProfilesEqual(HAgentDatabaseStorageOptions left, HAgentDatabaseStorageOptions right)
        {
            if (left == null || right == null) return left == right;
            return string.Equals(left.ServerName, right.ServerName, StringComparison.Ordinal)
                && left.Port == right.Port
                && string.Equals(left.UserName, right.UserName, StringComparison.Ordinal)
                && string.Equals(left.PasswordSecretId, right.PasswordSecretId, StringComparison.Ordinal);
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}

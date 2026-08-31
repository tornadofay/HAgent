using System;
using System.IO;
using System.Text.Json;
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
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

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
                return Task.FromResult(JsonSerializer.Deserialize<HAgentStorageOptions>(json, Options));
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
            return Task.CompletedTask;
        }
    }
}

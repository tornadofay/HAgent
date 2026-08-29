using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.WinForms.UI;

namespace HAgent.Storage.File
{
    public sealed class UiPermissionStore
    {
        private readonly string _path;
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        public UiPermissionStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));
            _path = path;
        }

        public Task<UiAutomationPermissions> LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!System.IO.File.Exists(_path))
                return Task.FromResult(new UiAutomationPermissions());

            var json = System.IO.File.ReadAllText(_path);
            var value = JsonSerializer.Deserialize<UiAutomationPermissions>(json, Options);
            if (value == null) value = new UiAutomationPermissions();
            value.Validate();
            return Task.FromResult(value);
        }

        public Task SaveAsync(UiAutomationPermissions permissions, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            var temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonSerializer.Serialize(permissions, Options));
            if (System.IO.File.Exists(_path)) System.IO.File.Replace(temp, _path, null, true);
            else System.IO.File.Move(temp, _path);
            return Task.CompletedTask;
        }
    }
}

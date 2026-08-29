using System;
using System.IO;
using System.Text.Json;

namespace HAgent.WinForms.UI
{
    /// <summary>
    /// Lightweight local persistence for the WinForms automatic UI policy.
    /// This stores policy only; application-specific authorization remains a host concern.
    /// </summary>
    public sealed class UiPermissionStore
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };
        private readonly string _path;

        public UiPermissionStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));
            _path = path;
        }

        public UiAutomationPermissions Load()
        {
            try
            {
                if (!System.IO.File.Exists(_path)) return new UiAutomationPermissions();
                var json = System.IO.File.ReadAllText(_path);
                var value = JsonSerializer.Deserialize<UiAutomationPermissions>(json, Options);
                if (value == null) return new UiAutomationPermissions();
                value.Validate();
                return value;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("HAgent UI permission policy could not be read.", ex);
            }
        }

        public void Save(UiAutomationPermissions permissions)
        {
            if (permissions == null) throw new ArgumentNullException(nameof(permissions));
            permissions.Validate();
            var folder = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
            var temp = _path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonSerializer.Serialize(permissions, Options));
            if (System.IO.File.Exists(_path)) System.IO.File.Replace(temp, _path, _path + ".bak", true);
            else System.IO.File.Move(temp, _path);
        }
    }
}

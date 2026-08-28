using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

namespace HAgent.Storage.File
{
    public sealed class ProtectedDataSecretStore : ISecretStore
    {
        private readonly string _directory;
        private const string Prefix = "HAG1";

        public ProtectedDataSecretStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Directory is required.", nameof(directory));
            _directory = directory;
        }

        public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken))
        {
            var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(secret ?? string.Empty), null, DataProtectionScope.CurrentUser);
            System.IO.Directory.CreateDirectory(_directory);
            System.IO.File.WriteAllText(PathFor(id), Prefix + Convert.ToBase64String(bytes), Encoding.UTF8);
            return Task.CompletedTask;
        }

        public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = PathFor(id);
            if (!System.IO.File.Exists(path)) return Task.FromResult(string.Empty);
            var text = System.IO.File.ReadAllText(path, Encoding.UTF8);
            if (!text.StartsWith(Prefix, StringComparison.Ordinal)) return Task.FromResult(string.Empty);
            var bytes = ProtectedData.Unprotect(Convert.FromBase64String(text.Substring(Prefix.Length)), null, DataProtectionScope.CurrentUser);
            return Task.FromResult(Encoding.UTF8.GetString(bytes));
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = PathFor(id);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            return Task.CompletedTask;
        }

        private string PathFor(string id)
        {
            foreach (var c in id)
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_') throw new ArgumentException("Invalid secret id.", nameof(id));
            return System.IO.Path.Combine(_directory, id + ".secret");
        }
    }
}

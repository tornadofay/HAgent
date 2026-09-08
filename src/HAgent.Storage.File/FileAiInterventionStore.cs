using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Storage.File
{
    /// <summary>
    /// Durable local intervention store. State is rewritten atomically so a request has one current persisted record.
    /// The file contains metadata/state only; runtime handlers and authorization callbacks are never serialized.
    /// </summary>
    public sealed class FileAiInterventionStore : IAiInterventionStore, IDisposable
    {
        private readonly string _path;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public FileAiInterventionStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Intervention file path is required.", nameof(path));
            _path = path;
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }

        public string Path => _path;

        public async Task<AiInterventionRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("Request ID is required.", nameof(requestId));
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                return records.FirstOrDefault(x => string.Equals(x.RequestId, requestId, StringComparison.OrdinalIgnoreCase))?.Clone();
            }
            finally { _gate.Release(); }
        }

        public async Task<IReadOnlyList<AiInterventionRequest>> SearchAsync(
            AiInterventionQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new AiInterventionQuery();
            query.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var result = records
                    .Where(x => !query.Status.HasValue || x.Status == query.Status.Value)
                    .Where(x => !query.TargetKind.HasValue || x.TargetKind == query.TargetKind.Value)
                    .Where(x => string.IsNullOrWhiteSpace(query.TargetId) ||
                                string.Equals(x.ResourceId, query.TargetId, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(x.ExecutionId, query.TargetId, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(x.ToolId, query.TargetId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(query.MaxResults)
                    .Select(x => x.Clone())
                    .ToList()
                    .AsReadOnly();
                return result;
            }
            finally { _gate.Release(); }
        }

        public async Task CreateAsync(AiInterventionRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                if (records.Any(x => string.Equals(x.RequestId, request.RequestId, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Intervention request already exists: " + request.RequestId);
                records.Add(request.Clone());
                await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public async Task<bool> TryUpdateAsync(
            AiInterventionRequest request,
            long expectedVersion,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var index = records.FindIndex(x => string.Equals(x.RequestId, request.RequestId, StringComparison.OrdinalIgnoreCase));
                if (index < 0) return false;
                if (records[index].Version != expectedVersion) return false;
                records[index] = request.Clone();
                await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
                return true;
            }
            finally { _gate.Release(); }
        }

        private async Task<List<AiInterventionRequest>> ReadAllAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_path)) return new List<AiInterventionRequest>();
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                var records = await JsonSerializer.DeserializeAsync<List<AiInterventionRequest>>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
                return records ?? new List<AiInterventionRequest>();
            }
        }

        private async Task WriteAllAsync(List<AiInterventionRequest> records, CancellationToken cancellationToken)
        {
            var tempPath = _path + ".tmp";
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    await JsonSerializer.SerializeAsync(stream, records, JsonOptions, cancellationToken).ConfigureAwait(false);

                if (File.Exists(_path)) File.Delete(_path);
                File.Move(tempPath, _path);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public void Dispose() { _gate.Dispose(); }
    }
}

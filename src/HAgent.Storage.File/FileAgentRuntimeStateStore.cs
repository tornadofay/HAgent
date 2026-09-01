using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Storage.File
{
    public sealed class FileAgentRuntimeStateStore : IAgentRuntimeStateStore, IDisposable
    {
        private readonly string _path;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };

        public FileAgentRuntimeStateStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Runtime state file path is required.", nameof(path));
            _path = path;
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }

        public string Path => _path;

        public async Task SaveAsync(AgentRuntimeStateRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateRecord(record);
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var replaced = false;
                for (var i = 0; i < records.Count; i++)
                {
                    if (!string.Equals(records[i].InstanceId, record.InstanceId, StringComparison.OrdinalIgnoreCase)) continue;
                    records[i] = record;
                    replaced = true;
                    break;
                }
                if (!replaced) records.Add(record);
                await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public async Task<AgentRuntimeStateRecord> GetAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(instanceId));
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                for (var i = 0; i < records.Count; i++)
                    if (string.Equals(records[i].InstanceId, instanceId, StringComparison.OrdinalIgnoreCase))
                        return records[i];
                return null;
            }
            finally { _gate.Release(); }
        }

        public async Task<IReadOnlyList<AgentRuntimeStateRecord>> SearchAsync(AgentRuntimeStateQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new AgentRuntimeStateQuery();
            var maxResults = query.GetEffectiveMaxResults();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var result = new List<AgentRuntimeStateRecord>();
                for (var i = 0; i < records.Count && result.Count < maxResults; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!Matches(records[i], query)) continue;
                    result.Add(records[i]);
                }
                return result.AsReadOnly();
            }
            finally { _gate.Release(); }
        }

        public async Task DeleteAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(instanceId));
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                records.RemoveAll(x => string.Equals(x.InstanceId, instanceId, StringComparison.OrdinalIgnoreCase));
                await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        private async Task<List<AgentRuntimeStateRecord>> ReadAllAsync(CancellationToken cancellationToken)
        {
            var records = new List<AgentRuntimeStateRecord>();
            if (!System.IO.File.Exists(_path)) return records;

            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var reader = new StreamReader(stream))
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null) break;
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var record = JsonSerializer.Deserialize<AgentRuntimeStateRecord>(line, JsonOptions);
                        if (record != null && !string.IsNullOrWhiteSpace(record.InstanceId)) records.Add(record);
                    }
                    catch (JsonException) { }
                }
            }
            return records;
        }

        private async Task WriteAllAsync(IReadOnlyList<AgentRuntimeStateRecord> records, CancellationToken cancellationToken)
        {
            var tempPath = _path + ".tmp";
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var writer = new StreamWriter(stream))
                {
                    for (var i = 0; i < records.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await writer.WriteLineAsync(JsonSerializer.Serialize(records[i], JsonOptions)).ConfigureAwait(false);
                    }
                }

                if (System.IO.File.Exists(_path)) System.IO.File.Delete(_path);
                System.IO.File.Move(tempPath, _path);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
            }
        }

        private static bool Matches(AgentRuntimeStateRecord record, AgentRuntimeStateQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.HostInstanceId) && !string.Equals(record.HostInstanceId, query.HostInstanceId, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(query.UserId) && !string.Equals(record.UserId, query.UserId, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(query.WorkspaceId) && !string.Equals(record.WorkspaceId, query.WorkspaceId, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(query.SessionId) && !string.Equals(record.SessionId, query.SessionId, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(query.ProfileId) && !string.Equals(record.ProfileId, query.ProfileId, StringComparison.OrdinalIgnoreCase)) return false;
            if (query.Scope.HasValue && record.Scope != query.Scope.Value) return false;
            return true;
        }

        private static void ValidateRecord(AgentRuntimeStateRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(record.InstanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(record));
            if (string.IsNullOrWhiteSpace(record.ProfileId)) throw new ArgumentException("Runtime profile ID is required.", nameof(record));
        }

        public void Dispose() { _gate.Dispose(); }
    }
}

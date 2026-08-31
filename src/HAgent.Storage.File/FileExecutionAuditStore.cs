using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Storage.File
{
    public sealed class FileExecutionAuditStore : IExecutionAuditStore, IDisposable
    {
        private readonly string _path;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };

        public FileExecutionAuditStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Audit file path is required.", nameof(path));
            _path = path;
            var directory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) System.IO.Directory.CreateDirectory(directory);
        }

        public string Path => _path;

        public async Task AppendAsync(AgentExecutionAuditRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new System.IO.FileStream(_path, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read, 4096, true))
                using (var writer = new System.IO.StreamWriter(stream))
                    await writer.WriteLineAsync(json).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public async Task<IReadOnlyList<AgentExecutionAuditRecord>> SearchAsync(ExecutionAuditQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new ExecutionAuditQuery();
            var maxResults = query.GetEffectiveMaxResults();
            var result = new List<AgentExecutionAuditRecord>();
            if (!System.IO.File.Exists(_path)) return result.AsReadOnly();

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new System.IO.FileStream(_path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 4096, true))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    while (result.Count < maxResults)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var line = await reader.ReadLineAsync().ConfigureAwait(false);
                        if (line == null) break;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        AgentExecutionAuditRecord record;
                        try { record = JsonSerializer.Deserialize<AgentExecutionAuditRecord>(line, JsonOptions); }
                        catch (JsonException) { continue; }
                        if (record == null || !Matches(record, query)) continue;
                        result.Add(record);
                    }
                }
            }
            finally { _gate.Release(); }

            return result.AsReadOnly();
        }

        public async Task TrimAsync(int maxRecords, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (maxRecords < 1) throw new ArgumentOutOfRangeException(nameof(maxRecords));
            if (!System.IO.File.Exists(_path)) return;

            var records = new List<AgentExecutionAuditRecord>();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new System.IO.FileStream(_path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 4096, true))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var line = await reader.ReadLineAsync().ConfigureAwait(false);
                        if (line == null) break;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        AgentExecutionAuditRecord record;
                        try { record = JsonSerializer.Deserialize<AgentExecutionAuditRecord>(line, JsonOptions); }
                        catch (JsonException) { continue; }
                        if (record == null) continue;
                        records.Add(record);
                        if (records.Count > maxRecords)
                            records.RemoveAt(0);
                    }
                }

                var tempPath = _path + ".tmp";
                try
                {
                    using (var output = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 4096, true))
                    using (var writer = new System.IO.StreamWriter(output))
                    {
                        foreach (var record in records)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await writer.WriteLineAsync(JsonSerializer.Serialize(record, JsonOptions)).ConfigureAwait(false);
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
            finally { _gate.Release(); }
        }

        private static bool Matches(AgentExecutionAuditRecord record, ExecutionAuditQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.ExecutionId) && !string.Equals(record.ExecutionId, query.ExecutionId, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(query.CorrelationId) && !string.Equals(record.CorrelationId, query.CorrelationId, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(query.AgentId) && !string.Equals(record.AgentId, query.AgentId, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        public void Dispose() { _gate.Dispose(); }
    }
}

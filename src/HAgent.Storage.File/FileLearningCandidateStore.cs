using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Storage.File
{
    public sealed class FileLearningCandidateStore : IAiLearningCandidateStore, IDisposable
    {
        private readonly string _path;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = false };

        public FileLearningCandidateStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Learning candidate file path is required.", nameof(path));
            _path = path;
            var directory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }

        public string Path => _path;

        public async Task SaveAsync(AiLearningCandidateRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var replaced = false;
                for (var i = 0; i < records.Count; i++)
                {
                    if (!string.Equals(records[i].CandidateId, record.CandidateId, StringComparison.OrdinalIgnoreCase)) continue;
                    records[i] = Clone(record);
                    replaced = true;
                    break;
                }
                if (!replaced) records.Add(Clone(record));
                await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public async Task<AiLearningCandidateRecord> GetAsync(string candidateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(candidateId)) return null;
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                foreach (var record in records)
                    if (string.Equals(record.CandidateId, candidateId, StringComparison.OrdinalIgnoreCase))
                        return record.IsExpired() ? null : Clone(record);
                return null;
            }
            finally { _gate.Release(); }
        }

        public async Task<IReadOnlyList<AiLearningCandidateRecord>> QueryAsync(AiLearningCandidateQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new AiLearningCandidateQuery();
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var result = new List<AiLearningCandidateRecord>();
                var max = Math.Max(1, Math.Min(query.MaxResults, 1000));
                foreach (var record in records)
                {
                    if (query.Status.HasValue && record.Status != query.Status.Value) continue;
                    if (query.CandidateType.HasValue && record.CandidateType != query.CandidateType.Value) continue;
                    if (!string.IsNullOrWhiteSpace(query.ProposedScope) && !string.Equals(record.ProposedScope, query.ProposedScope, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!string.IsNullOrWhiteSpace(query.SourceAgentProfileId) && !string.Equals(record.SourceAgentProfileId, query.SourceAgentProfileId, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!query.IncludeExpired && record.IsExpired()) continue;
                    result.Add(Clone(record));
                }
                result.Sort((x, y) => y.UpdatedAt.CompareTo(x.UpdatedAt));
                if (result.Count > max) result.RemoveRange(max, result.Count - max);
                return result.AsReadOnly();
            }
            finally { _gate.Release(); }
        }

        public async Task<AiLearningCandidateRecord> TryUpdateAsync(AiLearningCandidateRecord record, long expectedRevision, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                for (var i = 0; i < records.Count; i++)
                {
                    if (!string.Equals(records[i].CandidateId, record.CandidateId, StringComparison.OrdinalIgnoreCase)) continue;
                    if (records[i].Revision != expectedRevision)
                        throw new InvalidOperationException("Learning candidate revision is stale. Expected " + expectedRevision + " but found " + records[i].Revision + ".");
                    records[i] = Clone(record);
                    await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
                    return Clone(record);
                }
                throw new InvalidOperationException("Learning candidate does not exist: " + record.CandidateId + ".");
            }
            finally { _gate.Release(); }
        }

        public async Task<int> PurgeExpiredAsync(DateTimeOffset? at = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var now = at ?? DateTimeOffset.UtcNow;
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var records = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                var before = records.Count;
                records.RemoveAll(x => x.IsExpired(now));
                if (records.Count != before) await WriteAllAsync(records, cancellationToken).ConfigureAwait(false);
                return before - records.Count;
            }
            finally { _gate.Release(); }
        }

        private async Task<List<AiLearningCandidateRecord>> ReadAllAsync(CancellationToken cancellationToken)
        {
            var records = new List<AiLearningCandidateRecord>();
            if (!File.Exists(_path)) return records;
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true))
            using (var reader = new StreamReader(stream))
            {
                var lineNumber = 0;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null) break;
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        var record = JsonSerializer.Deserialize<AiLearningCandidateRecord>(line, _jsonOptions);
                        if (record == null)
                            throw new InvalidDataException("Learning candidate record is null at line " + lineNumber + ".");
                        record.Validate();
                        records.Add(record);
                    }
                    catch (JsonException ex)
                    {
                        throw new InvalidDataException("Learning candidate store contains invalid JSON at line " + lineNumber + ".", ex);
                    }
                }
            }
            return records;
        }

        private async Task WriteAllAsync(IList<AiLearningCandidateRecord> records, CancellationToken cancellationToken)
        {
            var tempPath = _path + ".tmp";
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var record in records)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteLineAsync(JsonSerializer.Serialize(record, _jsonOptions)).ConfigureAwait(false);
                }
            }

            if (File.Exists(_path))
                File.Replace(tempPath, _path, null);
            else
                File.Move(tempPath, _path);
        }

        private static AiLearningCandidateRecord Clone(AiLearningCandidateRecord source)
        {
            return JsonSerializer.Deserialize<AiLearningCandidateRecord>(JsonSerializer.Serialize(source));
        }

        public void Dispose() { _gate.Dispose(); }
    }
}

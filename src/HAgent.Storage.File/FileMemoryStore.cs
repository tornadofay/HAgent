using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Storage.File
{
    /// <summary>
    /// Lightweight append-oriented memory store.
    /// Uses one JSON object per line so searches do not require loading the whole store into memory.
    /// </summary>
    public sealed class FileMemoryStore : IMemoryStore, IDisposable
    {
        private readonly string _path;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        public FileMemoryStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Memory file path is required.", nameof(path));
            _path = path;

            var directory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory))
                System.IO.Directory.CreateDirectory(directory);
        }

        public string Path => _path;

        public async Task AddAsync(MemoryEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(entry.Id)) entry.Id = Guid.NewGuid().ToString("N");
            if (entry.Metadata == null) entry.Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (entry.CreatedAt == default(DateTimeOffset)) entry.CreatedAt = DateTimeOffset.UtcNow;

            var json = JsonSerializer.Serialize(entry, _jsonOptions);
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new System.IO.FileStream(_path, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read, 4096, true))
                using (var writer = new System.IO.StreamWriter(stream))
                {
                    await writer.WriteLineAsync(json).ConfigureAwait(false);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query ?? new MemoryQuery();
            var maxResults = query.MaxResults <= 0 ? 10 : Math.Min(query.MaxResults, 1000);
            var terms = SplitTerms(query.Text);
            var matches = new List<ScoredEntry>();

            if (!System.IO.File.Exists(_path)) return matches.ConvertAll(x => x.Entry).AsReadOnly();

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var stream = new System.IO.FileStream(_path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 4096, true))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var line = await reader.ReadLineAsync().ConfigureAwait(false);
                        if (line == null) break;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        MemoryEntry entry;
                        try
                        {
                            entry = JsonSerializer.Deserialize<MemoryEntry>(line, _jsonOptions);
                        }
                        catch (JsonException)
                        {
                            continue;
                        }

                        if (!MatchesFilter(entry, query)) continue;
                        var score = Score(entry, terms);
                        if (terms.Count > 0 && score <= 0) continue;

                        matches.Add(new ScoredEntry(entry, score));
                        matches.Sort(Compare);
                        if (matches.Count > maxResults)
                            matches.RemoveAt(matches.Count - 1);
                    }
                }
            }
            finally
            {
                _gate.Release();
            }

            var result = new List<MemoryEntry>(matches.Count);
            foreach (var match in matches) result.Add(match.Entry);
            return result.AsReadOnly();
        }

        public async Task RemoveAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(memoryId) || !System.IO.File.Exists(_path)) return;
            await RewriteAsync(delegate(MemoryEntry entry) {
                return !string.Equals(entry.Id, memoryId, StringComparison.OrdinalIgnoreCase);
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task ClearAsync(string scope, string ownerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!System.IO.File.Exists(_path)) return;
            MemoryScope parsed;
            var hasScope = Enum.TryParse(scope, true, out parsed);
            await RewriteAsync(delegate(MemoryEntry entry) {
                var scopeMatch = !hasScope || entry.Scope == parsed;
                var ownerMatch = string.IsNullOrWhiteSpace(ownerId) || string.Equals(entry.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase);
                return !(scopeMatch && ownerMatch);
            }, cancellationToken).ConfigureAwait(false);
        }

        private async Task RewriteAsync(Func<MemoryEntry, bool> keep, CancellationToken cancellationToken)
        {
            var tempPath = _path + ".tmp";
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using (var input = System.IO.File.Exists(_path)
                    ? new System.IO.FileStream(_path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 4096, true)
                    : null)
                using (var reader = input == null ? null : new System.IO.StreamReader(input))
                using (var output = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 4096, true))
                using (var writer = new System.IO.StreamWriter(output))
                {
                    if (reader != null)
                    {
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var line = await reader.ReadLineAsync().ConfigureAwait(false);
                            if (line == null) break;
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            MemoryEntry entry;
                            try
                            {
                                entry = JsonSerializer.Deserialize<MemoryEntry>(line, _jsonOptions);
                            }
                            catch (JsonException)
                            {
                                continue;
                            }

                            if (keep(entry))
                                await writer.WriteLineAsync(JsonSerializer.Serialize(entry, _jsonOptions)).ConfigureAwait(false);
                        }
                    }
                }

                System.IO.File.Delete(_path);
                System.IO.File.Move(tempPath, _path);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
                _gate.Release();
            }
        }

        private static bool MatchesFilter(MemoryEntry entry, MemoryQuery query)
        {
            if (entry == null) return false;
            if (query.Scope != null && entry.Scope != query.Scope.Value) return false;
            if (!string.IsNullOrWhiteSpace(query.OwnerId) && !string.Equals(entry.OwnerId, query.OwnerId, StringComparison.OrdinalIgnoreCase)) return false;
            if (query.Metadata == null || query.Metadata.Count == 0) return true;
            if (entry.Metadata == null) return false;

            foreach (var pair in query.Metadata)
            {
                string value;
                if (!entry.Metadata.TryGetValue(pair.Key, out value) || !string.Equals(value, pair.Value, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        private static int Score(MemoryEntry entry, IReadOnlyList<string> terms)
        {
            if (terms.Count == 0) return 1;
            var content = entry.Content ?? string.Empty;
            var metadata = entry.Metadata == null ? string.Empty : string.Join(" ", entry.Metadata.Values);
            var haystack = (content + " " + metadata).ToLowerInvariant();
            var score = 0;
            foreach (var term in terms)
            {
                var index = 0;
                while ((index = haystack.IndexOf(term, index, StringComparison.Ordinal)) >= 0)
                {
                    score++;
                    index += Math.Max(1, term.Length);
                }
            }
            return score;
        }

        private static List<string> SplitTerms(string text)
        {
            var terms = new List<string>();
            foreach (var part in (text ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.Length < 2) continue;
                var normalized = part.ToLowerInvariant();
                if (!terms.Contains(normalized)) terms.Add(normalized);
            }
            return terms;
        }

        private static int Compare(ScoredEntry x, ScoredEntry y)
        {
            var score = y.Score.CompareTo(x.Score);
            return score != 0 ? score : y.Entry.CreatedAt.CompareTo(x.Entry.CreatedAt);
        }

        private sealed class ScoredEntry
        {
            public ScoredEntry(MemoryEntry entry, int score) { Entry = entry; Score = score; }
            public MemoryEntry Entry { get; private set; }
            public int Score { get; private set; }
        }

        public void Dispose()
        {
            _gate.Dispose();
        }
    }
}

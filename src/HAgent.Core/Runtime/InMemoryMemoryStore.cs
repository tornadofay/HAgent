using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Small, dependency-free memory store intended for development and low-memory applications.
    /// Search is text/metadata based; no embedding model or GPU is required.
    /// </summary>
    public sealed class InMemoryMemoryStore : IMemoryStore
    {
        private readonly ConcurrentDictionary<string, MemoryEntry> _entries = new ConcurrentDictionary<string, MemoryEntry>(StringComparer.OrdinalIgnoreCase);

        public Task AddAsync(MemoryEntry entry, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (string.IsNullOrWhiteSpace(entry.Id)) entry.Id = Guid.NewGuid().ToString("N");
            _entries[entry.Id] = entry;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            query = query ?? new MemoryQuery();
            var terms = SplitTerms(query.Text);
            var maxResults = query.MaxResults <= 0 ? 10 : query.MaxResults;

            var matches = _entries.Values
                .Where(x => query.Scope == null || x.Scope == query.Scope.Value)
                .Where(x => string.IsNullOrWhiteSpace(query.OwnerId) || string.Equals(x.OwnerId, query.OwnerId, StringComparison.OrdinalIgnoreCase))
                .Where(x => MetadataMatches(x, query.Metadata))
                .Select(x => new { Entry = x, Score = Score(x, terms) })
                .Where(x => terms.Count == 0 || x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Entry.CreatedAt)
                .Take(maxResults)
                .Select(x => x.Entry)
                .ToList();

            return Task.FromResult((IReadOnlyList<MemoryEntry>)matches.AsReadOnly());
        }

        public Task RemoveAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(memoryId)) _entries.TryRemove(memoryId, out _);
            return Task.CompletedTask;
        }

        public Task ClearAsync(string scope, string ownerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            MemoryScope parsed;
            var hasScope = Enum.TryParse(scope, true, out parsed);

            foreach (var pair in _entries)
            {
                var matchScope = !hasScope || pair.Value.Scope == parsed;
                var matchOwner = string.IsNullOrWhiteSpace(ownerId) || string.Equals(pair.Value.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase);
                if (matchScope && matchOwner)
                    _entries.TryRemove(pair.Key, out _);
            }
            return Task.CompletedTask;
        }

        private static bool MetadataMatches(MemoryEntry entry, IDictionary<string, string> metadata)
        {
            if (metadata == null || metadata.Count == 0) return true;
            if (entry.Metadata == null) return false;

            foreach (var pair in metadata)
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
                    index += term.Length;
                }
            }
            return score;
        }

        private static List<string> SplitTerms(string text)
        {
            return (text ?? string.Empty)
                .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length >= 2)
                .Select(x => x.ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

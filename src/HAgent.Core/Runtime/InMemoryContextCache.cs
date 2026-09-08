using System;
using System.Collections.Concurrent;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Thread-safe in-memory cache for reusable context snapshots.
    /// Cache identity includes component, scope, configuration, resource, and freshness versions.
    /// </summary>
    public sealed class InMemoryContextCache : IContextCache
    {
        private readonly ConcurrentDictionary<string, ContextCacheEntry> _entries =
            new ConcurrentDictionary<string, ContextCacheEntry>(StringComparer.Ordinal);

        public bool TryGet(ContextCacheKey key, DateTimeOffset now, out ContextSnapshot snapshot)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            key.Validate();

            ContextCacheEntry entry;
            if (_entries.TryGetValue(key.ToCanonicalString(), out entry) &&
                entry != null && entry.ExpiresAt > now)
            {
                snapshot = entry.Snapshot;
                return true;
            }

            Invalidate(key);
            snapshot = null;
            return false;
        }

        public void Set(ContextCacheKey key, ContextSnapshot snapshot, DateTimeOffset expiresAt)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            key.Validate();
            if (expiresAt == default(DateTimeOffset))
                throw new ArgumentException("Context cache expiration is required.", nameof(expiresAt));

            _entries[key.ToCanonicalString()] = new ContextCacheEntry(key, snapshot, expiresAt);
        }

        public void Invalidate(ContextCacheKey key)
        {
            if (key == null) return;
            key.Validate();
            ContextCacheEntry ignored;
            _entries.TryRemove(key.ToCanonicalString(), out ignored);
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}

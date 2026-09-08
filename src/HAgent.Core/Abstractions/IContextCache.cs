using System;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Generic cache boundary for reusable context snapshots.
    /// Implementations must respect the complete cache key and must not expose mutable execution state.
    /// </summary>
    public interface IContextCache
    {
        bool TryGet(ContextCacheKey key, DateTimeOffset now, out ContextSnapshot snapshot);

        void Set(ContextCacheKey key, ContextSnapshot snapshot, DateTimeOffset expiresAt);

        void Invalidate(ContextCacheKey key);

        void Clear();
    }
}

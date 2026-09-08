using System;

namespace HAgent.Models
{
    /// <summary>
    /// Reusable context snapshot stored with an explicit cache identity and expiration boundary.
    /// </summary>
    public sealed class ContextCacheEntry
    {
        public ContextCacheEntry(ContextCacheKey key, ContextSnapshot snapshot, DateTimeOffset expiresAt)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            key.Validate();
            if (expiresAt == default(DateTimeOffset))
                throw new ArgumentException("Context cache expiration is required.", nameof(expiresAt));

            Key = key.Clone();
            Snapshot = snapshot;
            ExpiresAt = expiresAt;
            Version = 1;
        }

        public ContextCacheKey Key { get; private set; }
        public ContextSnapshot Snapshot { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public int Version { get; private set; }
    }
}

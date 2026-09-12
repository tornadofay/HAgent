using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryAiResourceReliabilityStore : IAiResourceReliabilityStore
    {
        private readonly ConcurrentDictionary<string, AiResourceReliabilityRecord> _records =
            new ConcurrentDictionary<string, AiResourceReliabilityRecord>(StringComparer.OrdinalIgnoreCase);

        public Task<AiResourceReliabilityRecord> GetAsync(
            AiResourceReliabilityIdentity identity,
            CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            cancellationToken.ThrowIfCancellationRequested();

            AiResourceReliabilityRecord record;
            _records.TryGetValue(identity.ToStableKey(), out record);
            return Task.FromResult(record == null ? null : record.Clone());
        }

        public Task<bool> TryCreateAsync(
            AiResourceReliabilityRecord record,
            CancellationToken cancellationToken)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            var clone = record.Clone();
            return Task.FromResult(_records.TryAdd(clone.Identity.ToStableKey(), clone));
        }

        public Task<bool> TryUpdateAsync(
            AiResourceReliabilityRecord record,
            long expectedRevision,
            CancellationToken cancellationToken)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (expectedRevision < 0) throw new ArgumentOutOfRangeException(nameof(expectedRevision));
            record.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var key = record.Identity.ToStableKey();
            AiResourceReliabilityRecord current;
            if (!_records.TryGetValue(key, out current))
                return Task.FromResult(false);
            if (current.Revision != expectedRevision)
                return Task.FromResult(false);

            var clone = record.Clone();
            return Task.FromResult(_records.TryUpdate(key, clone, current));
        }
    }
}

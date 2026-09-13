using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryAiLearnedResourceLifecycleStore : IAiLearnedResourceLifecycleStore
    {
        private readonly ConcurrentDictionary<string, AiLearnedResourceLifecycleRecord> _records =
            new ConcurrentDictionary<string, AiLearnedResourceLifecycleRecord>(StringComparer.OrdinalIgnoreCase);

        public Task<AiLearnedResourceLifecycleRecord> GetAsync(
            AiResourceReliabilityIdentity identity,
            CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            identity.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            AiLearnedResourceLifecycleRecord record;
            _records.TryGetValue(identity.ToStableKey(), out record);
            return Task.FromResult(record == null ? null : record.Clone());
        }

        public Task<bool> TryCreateAsync(
            AiLearnedResourceLifecycleRecord record,
            CancellationToken cancellationToken)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_records.TryAdd(record.Identity.ToStableKey(), record.Clone()));
        }

        public Task<bool> TryUpdateAsync(
            AiLearnedResourceLifecycleRecord record,
            long expectedRevision,
            CancellationToken cancellationToken)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (expectedRevision < 0) throw new ArgumentOutOfRangeException(nameof(expectedRevision));
            record.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var key = record.Identity.ToStableKey();
            AiLearnedResourceLifecycleRecord current;
            if (!_records.TryGetValue(key, out current))
                return Task.FromResult(false);
            if (current.Revision != expectedRevision)
                return Task.FromResult(false);

            return Task.FromResult(_records.TryUpdate(key, record.Clone(), current));
        }
    }
}

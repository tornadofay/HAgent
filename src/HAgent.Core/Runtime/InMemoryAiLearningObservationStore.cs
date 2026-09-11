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
    /// Thread-safe bounded in-memory observation store for deterministic hosts and tests.
    /// </summary>
    public sealed class InMemoryAiLearningObservationStore : IAiLearningObservationStore
    {
        private readonly ConcurrentQueue<AiLearningExecutionObservation> _items = new ConcurrentQueue<AiLearningExecutionObservation>();
        private readonly int _capacity;

        public InMemoryAiLearningObservationStore(int capacity = 10000)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
        }

        public Task AppendAsync(AiLearningExecutionObservation observation, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (observation == null) throw new ArgumentNullException(nameof(observation));
            observation.Validate();
            _items.Enqueue(observation.Clone());
            while (_items.Count > _capacity)
            {
                AiLearningExecutionObservation discarded;
                if (!_items.TryDequeue(out discarded))
                    break;
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AiLearningExecutionObservation>> QueryAsync(
            AiLearningObservationQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (query == null) throw new ArgumentNullException(nameof(query));
            query.Validate();

            var items = _items.ToArray();
            IEnumerable<AiLearningExecutionObservation> filtered = items;
            if (!string.IsNullOrWhiteSpace(query.ExecutionId))
                filtered = filtered.Where(x => string.Equals(x.ExecutionId, query.ExecutionId, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(query.Kind))
                filtered = filtered.Where(x => string.Equals(x.Kind, query.Kind, StringComparison.OrdinalIgnoreCase));

            var result = filtered
                .OrderByDescending(x => x.CapturedAt)
                .Take(query.MaxResults)
                .Select(x => x.Clone())
                .ToList();
            return Task.FromResult((IReadOnlyList<AiLearningExecutionObservation>)result);
        }
    }
}

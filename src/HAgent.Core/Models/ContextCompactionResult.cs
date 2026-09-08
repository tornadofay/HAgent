using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Immutable-from-caller compacted context plus safe inclusion/exclusion diagnostics.
    /// </summary>
    public sealed class ContextCompactionResult
    {
        private readonly ContextSnapshot _snapshot;
        private readonly IReadOnlyList<ContextCompactionDecision> _decisions;

        public ContextCompactionResult(
            ContextSnapshot snapshot,
            IReadOnlyList<ContextCompactionDecision> decisions,
            int candidateCount)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (decisions == null) throw new ArgumentNullException(nameof(decisions));
            if (candidateCount < snapshot.UsedItems)
                throw new ArgumentOutOfRangeException(nameof(candidateCount));

            var clones = new List<ContextCompactionDecision>(decisions.Count);
            foreach (var decision in decisions)
            {
                if (decision == null)
                    throw new ArgumentException("Compaction decisions cannot contain null values.", nameof(decisions));
                clones.Add(decision.Clone());
            }

            _snapshot = snapshot;
            _decisions = new ReadOnlyCollection<ContextCompactionDecision>(clones);
            IncludedCount = snapshot.UsedItems;
            ExcludedCount = candidateCount - IncludedCount;
            WasTruncated = ExcludedCount > 0;
            Version = 1;
        }

        public ContextSnapshot Snapshot { get { return _snapshot; } }

        public IReadOnlyList<ContextCompactionDecision> Decisions
        {
            get
            {
                var clones = new List<ContextCompactionDecision>(_decisions.Count);
                foreach (var decision in _decisions)
                    clones.Add(decision.Clone());
                return new ReadOnlyCollection<ContextCompactionDecision>(clones);
            }
        }

        public int IncludedCount { get; private set; }
        public int ExcludedCount { get; private set; }
        public bool WasTruncated { get; private set; }
        public int Version { get; private set; }
    }
}

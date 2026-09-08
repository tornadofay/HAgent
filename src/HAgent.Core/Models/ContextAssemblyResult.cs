using System.Collections.Generic;

namespace HAgent.Models
{
    /// <summary>
    /// Final provider-neutral context result after policy admission, ranking/deduplication, and compaction.
    /// </summary>
    public sealed class ContextAssemblyResult
    {
        public ContextAssemblyResult(
            ContextSnapshot snapshot,
            IReadOnlyList<ContextAdmissionDecision> admissionDecisions,
            ContextCompactionResult compaction)
        {
            Snapshot = snapshot;
            AdmissionDecisions = admissionDecisions;
            Compaction = compaction;
        }

        public ContextSnapshot Snapshot { get; private set; }
        public IReadOnlyList<ContextAdmissionDecision> AdmissionDecisions { get; private set; }
        public ContextCompactionResult Compaction { get; private set; }
    }
}

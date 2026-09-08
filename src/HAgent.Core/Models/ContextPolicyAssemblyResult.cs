using System.Collections.Generic;

namespace HAgent.Models
{
    /// <summary>
    /// Result of policy-aware context assembly. Decisions contain metadata only and never payload content.
    /// </summary>
    public sealed class ContextPolicyAssemblyResult
    {
        public ContextPolicyAssemblyResult(
            ContextSnapshot snapshot,
            IReadOnlyList<ContextAdmissionDecision> decisions)
        {
            Snapshot = snapshot;
            Decisions = decisions;
        }

        public ContextSnapshot Snapshot { get; private set; }
        public IReadOnlyList<ContextAdmissionDecision> Decisions { get; private set; }
    }
}

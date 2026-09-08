using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Policy-filtered provider-neutral candidates before ranking and final bounded assembly.
    /// Decisions contain metadata only and never payload content.
    /// </summary>
    public sealed class ContextPolicyRetrievalResult
    {
        public ContextPolicyRetrievalResult(
            IReadOnlyList<ContextItem> candidates,
            IReadOnlyList<ContextAdmissionDecision> decisions)
        {
            var candidateClones = new List<ContextItem>();
            foreach (var candidate in candidates ?? new List<ContextItem>())
                if (candidate != null) candidateClones.Add(candidate.Clone());

            var decisionClones = new List<ContextAdmissionDecision>();
            foreach (var decision in decisions ?? new List<ContextAdmissionDecision>())
                if (decision != null) decisionClones.Add(decision.Clone());

            Candidates = new ReadOnlyCollection<ContextItem>(candidateClones);
            Decisions = new ReadOnlyCollection<ContextAdmissionDecision>(decisionClones);
        }

        public IReadOnlyList<ContextItem> Candidates { get; private set; }
        public IReadOnlyList<ContextAdmissionDecision> Decisions { get; private set; }
    }
}

using System;

namespace HAgent.Models
{
    /// <summary>
    /// Explicit target and diagnostic behavior for deterministic context compaction.
    /// </summary>
    public sealed class ContextCompactionOptions
    {
        public ContextCompactionOptions()
        {
            TargetBudget = new ContextBudget();
            ContinueAfterExcludedCandidate = true;
            CaptureDiagnostics = true;
        }

        public ContextBudget TargetBudget { get; set; }
        public bool ContinueAfterExcludedCandidate { get; set; }
        public bool CaptureDiagnostics { get; set; }

        public ContextCompactionOptions Clone()
        {
            return new ContextCompactionOptions
            {
                TargetBudget = TargetBudget == null ? null : TargetBudget.Clone(),
                ContinueAfterExcludedCandidate = ContinueAfterExcludedCandidate,
                CaptureDiagnostics = CaptureDiagnostics
            };
        }

        public void Validate()
        {
            if (TargetBudget == null)
                throw new ArgumentException("Context compaction target budget is required.", nameof(TargetBudget));
            TargetBudget.Validate();
        }
    }
}

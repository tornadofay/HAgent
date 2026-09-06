using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiCandidateDecision
    {
        Accepted,
        Rejected
    }

    public sealed class AiExecutionCandidateEvaluation
    {
        public AiExecutionCandidateEvaluation()
        {
            TargetId = string.Empty;
            Decision = AiCandidateDecision.Rejected;
            Score = 0d;
            Reasons = new List<string>();
        }

        public string TargetId { get; set; }
        public AiCandidateDecision Decision { get; set; }
        public double Score { get; set; }
        public IList<string> Reasons { get; private set; }
    }

    public sealed class AiExecutionPlan
    {
        public AiExecutionPlan()
        {
            SelectedTarget = null;
            Evaluations = new List<AiExecutionCandidateEvaluation>();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public AiExecutionTarget SelectedTarget { get; set; }
        public IList<AiExecutionCandidateEvaluation> Evaluations { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public bool HasSelection
        {
            get { return SelectedTarget != null; }
        }
    }
}

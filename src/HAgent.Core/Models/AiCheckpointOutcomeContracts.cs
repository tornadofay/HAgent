using System;

namespace HAgent.Models
{
    public enum AiPlanOutcomeStatus
    {
        Completed = 0,
        Failed = 1,
        UnknownOutcome = 2,
        Cancelled = 3,
        Superseded = 4
    }

    public enum AiCheckpointStatus
    {
        Reached = 0,
        Superseded = 1
    }

    public sealed class AiPlanCheckpoint
    {
        public string Id { get; private set; }
        public string PlanId { get; private set; }
        public string PlanStepId { get; private set; }
        public long PlanRevision { get; private set; }
        public AiCheckpointStatus Status { get; private set; }
        public string Boundary { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset CapturedAt { get; private set; }

        public AiPlanCheckpoint(
            string id,
            string planId,
            string planStepId,
            long planRevision,
            AiCheckpointStatus status,
            string boundary,
            string evidence,
            DateTimeOffset capturedAt)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Checkpoint ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(planId)) throw new ArgumentException("Checkpoint plan ID is required.", nameof(planId));
            if (string.IsNullOrWhiteSpace(planStepId)) throw new ArgumentException("Checkpoint step ID is required.", nameof(planStepId));
            if (planRevision < 1L) throw new ArgumentOutOfRangeException(nameof(planRevision));
            if (!Enum.IsDefined(typeof(AiCheckpointStatus), status)) throw new ArgumentException("Checkpoint status is not supported.", nameof(status));
            if (string.IsNullOrWhiteSpace(boundary)) throw new ArgumentException("Checkpoint boundary is required.", nameof(boundary));
            if (capturedAt == default(DateTimeOffset)) throw new ArgumentException("Checkpoint timestamp is required.", nameof(capturedAt));

            Id = id.Trim();
            PlanId = planId.Trim();
            PlanStepId = planStepId.Trim();
            PlanRevision = planRevision;
            Status = status;
            Boundary = boundary.Trim();
            Evidence = evidence == null ? string.Empty : evidence.Trim();
            CapturedAt = capturedAt;
        }
    }

    public sealed class AiPlanStepOutcome
    {
        public string Id { get; private set; }
        public string PlanId { get; private set; }
        public string PlanStepId { get; private set; }
        public long PlanRevision { get; private set; }
        public AiPlanOutcomeStatus Status { get; private set; }
        public string Reason { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset RecordedAt { get; private set; }

        public AiPlanStepOutcome(
            string id,
            string planId,
            string planStepId,
            long planRevision,
            AiPlanOutcomeStatus status,
            string reason,
            string evidence,
            DateTimeOffset recordedAt)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Outcome ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(planId)) throw new ArgumentException("Outcome plan ID is required.", nameof(planId));
            if (string.IsNullOrWhiteSpace(planStepId)) throw new ArgumentException("Outcome step ID is required.", nameof(planStepId));
            if (planRevision < 1L) throw new ArgumentOutOfRangeException(nameof(planRevision));
            if (!Enum.IsDefined(typeof(AiPlanOutcomeStatus), status)) throw new ArgumentException("Outcome status is not supported.", nameof(status));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Outcome reason is required.", nameof(reason));
            if (recordedAt == default(DateTimeOffset)) throw new ArgumentException("Outcome timestamp is required.", nameof(recordedAt));
            if (status == AiPlanOutcomeStatus.Completed && string.IsNullOrWhiteSpace(evidence))
                throw new ArgumentException("Completed outcomes require evidence.", nameof(evidence));

            Id = id.Trim();
            PlanId = planId.Trim();
            PlanStepId = planStepId.Trim();
            PlanRevision = planRevision;
            Status = status;
            Reason = reason.Trim();
            Evidence = evidence == null ? string.Empty : evidence.Trim();
            RecordedAt = recordedAt;
        }
    }
}

using System;

namespace HAgent.Models
{
    public enum AiPlanOperationStatus
    {
        Requested = 0,
        Completed = 1,
        Failed = 2,
        UnknownOutcome = 3,
        Cancelled = 4,
        Superseded = 5
    }

    public enum AiPlanRetryDecisionKind
    {
        RetryAllowed = 0,
        ReconciliationRequired = 1,
        RetryDenied = 2
    }

    public sealed class AiPlanStepOperation
    {
        public string Id { get; private set; }
        public string PlanId { get; private set; }
        public string PlanStepId { get; private set; }
        public long PlanRevision { get; private set; }
        public int Attempt { get; private set; }
        public AiPlanOperationStatus Status { get; private set; }
        public string HostCorrelationId { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset RecordedAt { get; private set; }

        public AiPlanStepOperation(
            string id,
            string planId,
            string planStepId,
            long planRevision,
            int attempt,
            AiPlanOperationStatus status,
            string hostCorrelationId,
            string evidence,
            DateTimeOffset recordedAt)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Operation ID is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(planId)) throw new ArgumentException("Operation plan ID is required.", nameof(planId));
            if (string.IsNullOrWhiteSpace(planStepId)) throw new ArgumentException("Operation step ID is required.", nameof(planStepId));
            if (planRevision < 1L) throw new ArgumentOutOfRangeException(nameof(planRevision));
            if (attempt < 1) throw new ArgumentOutOfRangeException(nameof(attempt));
            if (!Enum.IsDefined(typeof(AiPlanOperationStatus), status)) throw new ArgumentException("Operation status is not supported.", nameof(status));
            if (status == AiPlanOperationStatus.Requested && string.IsNullOrWhiteSpace(hostCorrelationId))
                throw new ArgumentException("Requested operations require host correlation identity.", nameof(hostCorrelationId));
            if (recordedAt == default(DateTimeOffset)) throw new ArgumentException("Operation timestamp is required.", nameof(recordedAt));

            Id = id.Trim();
            PlanId = planId.Trim();
            PlanStepId = planStepId.Trim();
            PlanRevision = planRevision;
            Attempt = attempt;
            Status = status;
            HostCorrelationId = hostCorrelationId == null ? string.Empty : hostCorrelationId.Trim();
            Evidence = evidence == null ? string.Empty : evidence.Trim();
            RecordedAt = recordedAt;
        }
    }

    public sealed class AiPlanRetryDecision
    {
        public AiPlanRetryDecisionKind Kind { get; private set; }
        public AiPlanOperationStatus SourceStatus { get; private set; }
        public string Reason { get; private set; }
        public DateTimeOffset DecidedAt { get; private set; }

        private AiPlanRetryDecision(
            AiPlanRetryDecisionKind kind,
            AiPlanOperationStatus sourceStatus,
            string reason,
            DateTimeOffset decidedAt)
        {
            Kind = kind;
            SourceStatus = sourceStatus;
            Reason = reason;
            DecidedAt = decidedAt;
        }

        public static AiPlanRetryDecision Evaluate(
            AiPlanStepOperation operation,
            bool hostConfirmsRetrySafe,
            DateTimeOffset decidedAt)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (decidedAt == default(DateTimeOffset)) throw new ArgumentException("Retry decision timestamp is required.", nameof(decidedAt));

            switch (operation.Status)
            {
                case AiPlanOperationStatus.UnknownOutcome:
                    return new AiPlanRetryDecision(
                        AiPlanRetryDecisionKind.ReconciliationRequired,
                        operation.Status,
                        "External outcome is unknown; reconcile before retrying.",
                        decidedAt);
                case AiPlanOperationStatus.Failed:
                    return new AiPlanRetryDecision(
                        hostConfirmsRetrySafe ? AiPlanRetryDecisionKind.RetryAllowed : AiPlanRetryDecisionKind.RetryDenied,
                        operation.Status,
                        hostConfirmsRetrySafe ? "Host confirmed that retry is safe." : "Retry safety was not established by the host.",
                        decidedAt);
                case AiPlanOperationStatus.Requested:
                    return new AiPlanRetryDecision(
                        AiPlanRetryDecisionKind.ReconciliationRequired,
                        operation.Status,
                        "The operation remains in requested state; reconcile its external effect before retrying.",
                        decidedAt);
                default:
                    return new AiPlanRetryDecision(
                        AiPlanRetryDecisionKind.RetryDenied,
                        operation.Status,
                        "The operation is not in a retryable failed state.",
                        decidedAt);
            }
        }
    }
}

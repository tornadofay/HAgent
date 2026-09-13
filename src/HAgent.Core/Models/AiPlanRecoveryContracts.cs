using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiPlanRecoveryReason
    {
        Restart = 0,
        ProcessCrash = 1,
        RuntimeRecreation = 2
    }

    public enum AiPlanRecoveryStepDisposition
    {
        Retryable = 0,
        Unknown = 1,
        Blocked = 2,
        RequiresHostReview = 3,
        AlreadyTerminal = 4
    }

    public sealed class AiPlanRecoveryStepDecision
    {
        public string PlanStepId { get; private set; }
        public AiPlanStepStatus PreviousStatus { get; private set; }
        public AiPlanRecoveryStepDisposition Disposition { get; private set; }
        public string OperationId { get; private set; }
        public string Reason { get; private set; }
        public string Evidence { get; private set; }

        public AiPlanRecoveryStepDecision(
            string planStepId,
            AiPlanStepStatus previousStatus,
            AiPlanRecoveryStepDisposition disposition,
            string operationId,
            string reason,
            string evidence)
        {
            if (string.IsNullOrWhiteSpace(planStepId)) throw new ArgumentException("Recovery step ID is required.", nameof(planStepId));
            if (!Enum.IsDefined(typeof(AiPlanStepStatus), previousStatus)) throw new ArgumentException("Previous step status is not supported.", nameof(previousStatus));
            if (!Enum.IsDefined(typeof(AiPlanRecoveryStepDisposition), disposition)) throw new ArgumentException("Recovery disposition is not supported.", nameof(disposition));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Recovery reason is required.", nameof(reason));

            PlanStepId = planStepId.Trim();
            PreviousStatus = previousStatus;
            Disposition = disposition;
            OperationId = operationId == null ? string.Empty : operationId.Trim();
            Reason = reason.Trim();
            Evidence = evidence == null ? string.Empty : evidence.Trim();
        }
    }

    public sealed class AiPlanRecoveryRecord
    {
        public string PlanId { get; private set; }
        public long PlanRevision { get; private set; }
        public string PreviousRuntimeInstanceId { get; private set; }
        public string CurrentRuntimeInstanceId { get; private set; }
        public long PreviousExecutionRevision { get; private set; }
        public long CurrentExecutionRevision { get; private set; }
        public AiPlanRecoveryReason Reason { get; private set; }
        public bool PreviousAuthorityInvalidated { get; private set; }
        public IList<AiPlanRecoveryStepDecision> StepDecisions { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset RecordedAt { get; private set; }

        public AiPlanRecoveryRecord(
            string planId,
            long planRevision,
            string previousRuntimeInstanceId,
            string currentRuntimeInstanceId,
            long previousExecutionRevision,
            long currentExecutionRevision,
            AiPlanRecoveryReason reason,
            bool previousAuthorityInvalidated,
            IEnumerable<AiPlanRecoveryStepDecision> stepDecisions,
            string evidence,
            DateTimeOffset recordedAt)
        {
            if (string.IsNullOrWhiteSpace(planId)) throw new ArgumentException("Recovery plan ID is required.", nameof(planId));
            if (planRevision < 0L) throw new ArgumentOutOfRangeException(nameof(planRevision));
            if (string.IsNullOrWhiteSpace(previousRuntimeInstanceId)) throw new ArgumentException("Previous runtime instance ID is required.", nameof(previousRuntimeInstanceId));
            if (string.IsNullOrWhiteSpace(currentRuntimeInstanceId)) throw new ArgumentException("Current runtime instance ID is required.", nameof(currentRuntimeInstanceId));
            if (string.Equals(previousRuntimeInstanceId.Trim(), currentRuntimeInstanceId.Trim(), StringComparison.Ordinal)) throw new ArgumentException("Recovery requires a new runtime instance identity.", nameof(currentRuntimeInstanceId));
            if (previousExecutionRevision < 0L) throw new ArgumentOutOfRangeException(nameof(previousExecutionRevision));
            if (currentExecutionRevision <= previousExecutionRevision) throw new ArgumentException("Current execution revision must advance beyond the previous execution revision.", nameof(currentExecutionRevision));
            if (!Enum.IsDefined(typeof(AiPlanRecoveryReason), reason)) throw new ArgumentException("Recovery reason is not supported.", nameof(reason));
            if (recordedAt == default(DateTimeOffset)) throw new ArgumentException("Recovery timestamp is required.", nameof(recordedAt));

            PlanId = planId.Trim();
            PlanRevision = planRevision;
            PreviousRuntimeInstanceId = previousRuntimeInstanceId.Trim();
            CurrentRuntimeInstanceId = currentRuntimeInstanceId.Trim();
            PreviousExecutionRevision = previousExecutionRevision;
            CurrentExecutionRevision = currentExecutionRevision;
            Reason = reason;
            PreviousAuthorityInvalidated = previousAuthorityInvalidated;
            StepDecisions = new List<AiPlanRecoveryStepDecision>(stepDecisions ?? new List<AiPlanRecoveryStepDecision>());
            Evidence = evidence == null ? string.Empty : evidence.Trim();
            RecordedAt = recordedAt;
        }
    }

    public static class AiPlanRecoveryEvaluator
    {
        public static AiPlanRecoveryRecord Recover(
            AiPlan plan,
            IEnumerable<AiPlanStepOperation> latestOperations,
            string previousRuntimeInstanceId,
            string currentRuntimeInstanceId,
            long previousExecutionRevision,
            long currentExecutionRevision,
            AiPlanRecoveryReason reason,
            DateTimeOffset recordedAt)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            plan.Validate();
            if (latestOperations == null) latestOperations = new List<AiPlanStepOperation>();
            if (string.IsNullOrWhiteSpace(previousRuntimeInstanceId)) throw new ArgumentException("Previous runtime instance ID is required.", nameof(previousRuntimeInstanceId));
            if (string.IsNullOrWhiteSpace(currentRuntimeInstanceId)) throw new ArgumentException("Current runtime instance ID is required.", nameof(currentRuntimeInstanceId));
            if (string.Equals(previousRuntimeInstanceId.Trim(), currentRuntimeInstanceId.Trim(), StringComparison.Ordinal)) throw new ArgumentException("Recovery requires a new runtime instance identity.", nameof(currentRuntimeInstanceId));
            if (currentExecutionRevision <= previousExecutionRevision) throw new ArgumentException("Current execution revision must advance beyond the previous execution revision.", nameof(currentExecutionRevision));

            var operationsByStep = new Dictionary<string, AiPlanStepOperation>(StringComparer.OrdinalIgnoreCase);
            foreach (var operation in latestOperations)
            {
                if (operation == null) continue;
                if (!string.Equals(operation.PlanId, plan.Id, StringComparison.Ordinal)) continue;
                if (operation.PlanRevision != plan.Revision) continue;
                operationsByStep[operation.PlanStepId] = operation;
            }

            var decisions = new List<AiPlanRecoveryStepDecision>();
            foreach (var step in plan.Steps)
            {
                AiPlanStepOperation operation;
                operationsByStep.TryGetValue(step.Id, out operation);
                var disposition = GetDisposition(step.Status, operation);
                var reasonText = GetReason(step.Status, operation, disposition);
                decisions.Add(new AiPlanRecoveryStepDecision(
                    step.Id,
                    step.Status,
                    disposition,
                    operation == null ? string.Empty : operation.Id,
                    reasonText,
                    operation == null ? string.Empty : operation.Evidence));
            }

            var evidence = "Recovered plan revision " + plan.Revision + " under a new runtime/execution authority; prior in-flight authority is invalidated.";
            return new AiPlanRecoveryRecord(
                plan.Id,
                plan.Revision,
                previousRuntimeInstanceId,
                currentRuntimeInstanceId,
                previousExecutionRevision,
                currentExecutionRevision,
                reason,
                true,
                decisions,
                evidence,
                recordedAt);
        }

        private static AiPlanRecoveryStepDisposition GetDisposition(AiPlanStepStatus status, AiPlanStepOperation operation)
        {
            if (status == AiPlanStepStatus.Completed || status == AiPlanStepStatus.Skipped || status == AiPlanStepStatus.Cancelled || status == AiPlanStepStatus.Superseded)
                return AiPlanRecoveryStepDisposition.AlreadyTerminal;
            if (operation != null && operation.Status == AiPlanOperationStatus.UnknownOutcome)
                return AiPlanRecoveryStepDisposition.RequiresHostReview;
            if (operation != null && operation.Status == AiPlanOperationStatus.Requested)
                return AiPlanRecoveryStepDisposition.RequiresHostReview;
            if (status == AiPlanStepStatus.Failed || status == AiPlanStepStatus.Running || status == AiPlanStepStatus.Ready || status == AiPlanStepStatus.Pending)
                return AiPlanRecoveryStepDisposition.Retryable;
            return AiPlanRecoveryStepDisposition.Blocked;
        }

        private static string GetReason(AiPlanStepStatus status, AiPlanStepOperation operation, AiPlanRecoveryStepDisposition disposition)
        {
            if (disposition == AiPlanRecoveryStepDisposition.AlreadyTerminal) return "Step already has a terminal durable status; do not revive previous execution authority.";
            if (operation != null && operation.Status == AiPlanOperationStatus.UnknownOutcome) return "External operation outcome is unknown; host reconciliation is required before retry.";
            if (operation != null && operation.Status == AiPlanOperationStatus.Requested) return "Operation was requested before restart; reconcile external effect before retry.";
            if (status == AiPlanStepStatus.Running) return "Previous execution was in flight; previous authority is invalid and the step is safely retryable only under current retry policy.";
            if (status == AiPlanStepStatus.Failed) return "Previous execution failed; current retry policy may evaluate a new attempt.";
            if (status == AiPlanStepStatus.Pending || status == AiPlanStepStatus.Ready) return "Step was not terminal; current runtime may resume from this durable boundary.";
            return "Step requires explicit recovery handling.";
        }
    }
}

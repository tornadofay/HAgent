using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class PlanRetryIdempotencyContractsTests
    {
        [Fact]
        public void Operation_PreservesStableIdentityAcrossAttempt()
        {
            var operation = new AiPlanStepOperation("operation-1", "plan-1", "step-1", 3L, 2, AiPlanOperationStatus.Failed, "host-correlation-1", "Host observed failure.", DateTimeOffset.UtcNow);
            Assert.Equal("operation-1", operation.Id);
            Assert.Equal("plan-1", operation.PlanId);
            Assert.Equal("step-1", operation.PlanStepId);
            Assert.Equal(3L, operation.PlanRevision);
            Assert.Equal(2, operation.Attempt);
            Assert.Equal(AiPlanOperationStatus.Failed, operation.Status);
        }

        [Fact]
        public void RequestedOperation_RequiresHostCorrelation()
        {
            Assert.Throws<ArgumentException>(() => new AiPlanStepOperation("operation-1", "plan-1", "step-1", 1L, 1, AiPlanOperationStatus.Requested, string.Empty, "Requested.", DateTimeOffset.UtcNow));
        }

        [Fact]
        public void FailedOperation_AllowsRetryOnlyWhenHostConfirmsSafety()
        {
            var operation = new AiPlanStepOperation("operation-1", "plan-1", "step-1", 1L, 1, AiPlanOperationStatus.Failed, "host-1", "Rejected before side effect.", DateTimeOffset.UtcNow);
            var denied = AiPlanRetryDecision.Evaluate(operation, false, DateTimeOffset.UtcNow);
            var allowed = AiPlanRetryDecision.Evaluate(operation, true, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRetryDecisionKind.RetryDenied, denied.Kind);
            Assert.Equal(AiPlanRetryDecisionKind.RetryAllowed, allowed.Kind);
        }

        [Fact]
        public void UnknownOutcome_RequiresReconciliationAndNeverBecomesRetryAllowed()
        {
            var operation = new AiPlanStepOperation("operation-unknown", "plan-1", "step-1", 1L, 1, AiPlanOperationStatus.UnknownOutcome, "host-1", "Timed out after request submission.", DateTimeOffset.UtcNow);
            var decision = AiPlanRetryDecision.Evaluate(operation, true, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRetryDecisionKind.ReconciliationRequired, decision.Kind);
            Assert.Equal(AiPlanOperationStatus.UnknownOutcome, decision.SourceStatus);
        }

        [Fact]
        public void RequestedOperation_RequiresReconciliationBeforeRetry()
        {
            var operation = new AiPlanStepOperation("operation-requested", "plan-1", "step-1", 1L, 1, AiPlanOperationStatus.Requested, "host-1", "Request accepted for processing.", DateTimeOffset.UtcNow);
            var decision = AiPlanRetryDecision.Evaluate(operation, true, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRetryDecisionKind.ReconciliationRequired, decision.Kind);
        }

        [Fact]
        public void CompletedAndCancelledOperations_AreNotRetryable()
        {
            var completed = new AiPlanStepOperation("operation-completed", "plan-1", "step-1", 1L, 1, AiPlanOperationStatus.Completed, "host-1", "Observed completion.", DateTimeOffset.UtcNow);
            var cancelled = new AiPlanStepOperation("operation-cancelled", "plan-1", "step-1", 1L, 1, AiPlanOperationStatus.Cancelled, "host-1", "Cancelled before completion.", DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRetryDecisionKind.RetryDenied, AiPlanRetryDecision.Evaluate(completed, true, DateTimeOffset.UtcNow).Kind);
            Assert.Equal(AiPlanRetryDecisionKind.RetryDenied, AiPlanRetryDecision.Evaluate(cancelled, true, DateTimeOffset.UtcNow).Kind);
        }
    }
}

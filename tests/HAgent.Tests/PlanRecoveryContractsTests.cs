using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class PlanRecoveryContractsTests
    {
        [Fact]
        public void Recovery_PreservesPlanRevisionAndInvalidatesPreviousAuthority()
        {
            var plan = CreatePlan();
            var record = AiPlanRecoveryEvaluator.Recover(plan, null, "runtime-old", "runtime-new", 7L, 8L, AiPlanRecoveryReason.Restart, DateTimeOffset.UtcNow);

            Assert.Equal(plan.Id, record.PlanId);
            Assert.Equal(plan.Revision, record.PlanRevision);
            Assert.True(record.PreviousAuthorityInvalidated);
            Assert.Equal("runtime-old", record.PreviousRuntimeInstanceId);
            Assert.Equal("runtime-new", record.CurrentRuntimeInstanceId);
        }

        [Fact]
        public void Recovery_RunningStepBecomesRetryableUnderNewAuthority()
        {
            var plan = CreatePlan();
            var record = AiPlanRecoveryEvaluator.Recover(plan, null, "runtime-old", "runtime-new", 7L, 8L, AiPlanRecoveryReason.ProcessCrash, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRecoveryStepDisposition.Retryable, record.StepDecisions[0].Disposition);
        }

        [Fact]
        public void Recovery_UnknownExternalOutcomeRequiresHostReview()
        {
            var plan = CreatePlan();
            var operation = new AiPlanStepOperation("operation-unknown", plan.Id, plan.Steps[0].Id, plan.Revision, 1, AiPlanOperationStatus.UnknownOutcome, "host-1", "Timed out after submission.", DateTimeOffset.UtcNow);

            var record = AiPlanRecoveryEvaluator.Recover(plan, new[] { operation }, "runtime-old", "runtime-new", 7L, 8L, AiPlanRecoveryReason.Restart, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRecoveryStepDisposition.RequiresHostReview, record.StepDecisions[0].Disposition);
            Assert.Equal("operation-unknown", record.StepDecisions[0].OperationId);
        }

        [Fact]
        public void Recovery_RequestedExternalOutcomeRequiresHostReview()
        {
            var plan = CreatePlan();
            var operation = new AiPlanStepOperation("operation-requested", plan.Id, plan.Steps[0].Id, plan.Revision, 1, AiPlanOperationStatus.Requested, "host-1", "Request was accepted.", DateTimeOffset.UtcNow);

            var record = AiPlanRecoveryEvaluator.Recover(plan, new[] { operation }, "runtime-old", "runtime-new", 7L, 8L, AiPlanRecoveryReason.RuntimeRecreation, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRecoveryStepDisposition.RequiresHostReview, record.StepDecisions[0].Disposition);
        }

        [Fact]
        public void Recovery_TerminalStepIsNeverRevived()
        {
            var plan = CreatePlan();
            plan.Steps[0].Status = AiPlanStepStatus.Completed;

            var record = AiPlanRecoveryEvaluator.Recover(plan, null, "runtime-old", "runtime-new", 7L, 8L, AiPlanRecoveryReason.Restart, DateTimeOffset.UtcNow);

            Assert.Equal(AiPlanRecoveryStepDisposition.AlreadyTerminal, record.StepDecisions[0].Disposition);
        }

        [Fact]
        public void Recovery_RejectsSameRuntimeIdentityAndNonAdvancingExecutionRevision()
        {
            var plan = CreatePlan();

            Assert.Throws<ArgumentException>(() => AiPlanRecoveryEvaluator.Recover(plan, null, "runtime-1", "runtime-1", 7L, 8L, AiPlanRecoveryReason.Restart, DateTimeOffset.UtcNow));
            Assert.Throws<ArgumentException>(() => AiPlanRecoveryEvaluator.Recover(plan, null, "runtime-1", "runtime-2", 7L, 7L, AiPlanRecoveryReason.Restart, DateTimeOffset.UtcNow));
        }

        private static AiPlan CreatePlan()
        {
            var plan = new AiPlan
            {
                Id = "plan-recovery",
                GoalId = "goal-recovery",
                IntentionId = "intention-recovery",
                Title = "Recovery test plan",
                Status = AiPlanStatus.Active,
                Revision = 3L,
                RevisionReason = "Recovery contract test"
            };
            plan.Provenance.Source = "Test";
            plan.Provenance.Authority = AiGoalAuthority.HostSupplied;
            plan.Steps.Add(new AiPlanStep
            {
                Id = "step-recovery",
                PlanId = plan.Id,
                Sequence = 1,
                Title = "Recover step",
                Status = AiPlanStepStatus.Running,
                Revision = 3L
            });
            plan.Steps[0].Provenance.Source = "Test";
            plan.Steps[0].Provenance.Authority = AiGoalAuthority.HostSupplied;
            plan.Validate();
            return plan;
        }
    }
}

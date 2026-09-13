using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class CheckpointOutcomeContractsTests
    {
        [Fact]
        public void Checkpoint_PreservesBoundaryAndRevision()
        {
            var time = DateTimeOffset.UtcNow;
            var checkpoint = new AiPlanCheckpoint("checkpoint-1", "plan-1", "step-2", 3L, AiCheckpointStatus.Reached, "After validation", "Validation completed.", time);
            Assert.Equal("plan-1", checkpoint.PlanId);
            Assert.Equal("step-2", checkpoint.PlanStepId);
            Assert.Equal(3L, checkpoint.PlanRevision);
            Assert.Equal("After validation", checkpoint.Boundary);
            Assert.Equal(time, checkpoint.CapturedAt);
        }

        [Fact]
        public void Outcome_SupportsAllRequiredTerminalStatuses()
        {
            var statuses = new[] { AiPlanOutcomeStatus.Completed, AiPlanOutcomeStatus.Failed, AiPlanOutcomeStatus.UnknownOutcome, AiPlanOutcomeStatus.Cancelled, AiPlanOutcomeStatus.Superseded };
            foreach (var status in statuses)
            {
                var outcome = new AiPlanStepOutcome(Guid.NewGuid().ToString("N"), "plan-1", "step-1", 2L, status, "Recorded by host boundary.", status == AiPlanOutcomeStatus.Completed ? "Observed completion." : string.Empty, DateTimeOffset.UtcNow);
                Assert.Equal(status, outcome.Status);
                Assert.Equal(2L, outcome.PlanRevision);
            }
        }

        [Fact]
        public void CompletedOutcome_RequiresEvidence()
        {
            Assert.Throws<ArgumentException>(() => new AiPlanStepOutcome("outcome-1", "plan-1", "step-1", 1L, AiPlanOutcomeStatus.Completed, "Completed.", string.Empty, DateTimeOffset.UtcNow));
        }

        [Fact]
        public void UnknownOutcome_IsNotCompleted()
        {
            var outcome = new AiPlanStepOutcome("outcome-unknown", "plan-1", "step-1", 4L, AiPlanOutcomeStatus.UnknownOutcome, "External result could not be verified.", "Timeout after submission.", DateTimeOffset.UtcNow);
            Assert.Equal(AiPlanOutcomeStatus.UnknownOutcome, outcome.Status);
            Assert.NotEqual(AiPlanOutcomeStatus.Completed, outcome.Status);
        }
    }
}

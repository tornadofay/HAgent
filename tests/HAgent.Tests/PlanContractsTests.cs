using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class PlanContractsTests
    {
        [Fact]
        public void Plan_CapturesGoalIntentionRevisionAndStepMetadata()
        {
            var plan = new AiPlan
            {
                GoalId = "goal-1",
                IntentionId = "intention-1",
                Title = "Complete intention",
                Status = AiPlanStatus.Active,
                Revision = 2L,
                RevisionReason = "New evidence changed the approach.",
                Provenance = new AiGoalProvenance { Authority = AiGoalAuthority.AgentInferred, Source = "Planner" }
            };
            var step = new AiPlanStep
            {
                PlanId = plan.Id,
                Sequence = 1,
                Title = "First step",
                Status = AiPlanStepStatus.Ready,
                Provenance = plan.Provenance.Clone(),
                Revision = 1L
            };
            step.Preconditions.Add("Resource available");
            step.ExpectedEffects.Add("Step completes");
            plan.Steps.Add(step);

            plan.Validate();

            Assert.Equal("goal-1", plan.GoalId);
            Assert.Equal("intention-1", plan.IntentionId);
            Assert.Equal(2L, plan.Revision);
            Assert.Equal("New evidence changed the approach.", plan.RevisionReason);
            Assert.Equal(plan.Id, step.PlanId);
            Assert.Equal(1, step.Sequence);
            Assert.Equal(AiPlanStepStatus.Ready, step.Status);
        }

        [Fact]
        public void Plan_SupportsExplicitStepDependencies()
        {
            var plan = new AiPlan
            {
                GoalId = "goal-1",
                IntentionId = "intention-1",
                Title = "Dependency plan",
                Provenance = new AiGoalProvenance { Authority = AiGoalAuthority.HostSupplied, Source = "Host" }
            };
            var first = new AiPlanStep
            {
                PlanId = plan.Id,
                Sequence = 1,
                Title = "Prepare",
                Provenance = plan.Provenance.Clone()
            };
            var second = new AiPlanStep
            {
                PlanId = plan.Id,
                Sequence = 2,
                Title = "Use prepared state",
                Provenance = plan.Provenance.Clone()
            };
            second.DependsOnStepIds.Add(first.Id);
            second.FailureConditions.Add("Prepared state unavailable");
            second.CompletionCriteria.Add("Operation confirmed");
            plan.Steps.Add(first);
            plan.Steps.Add(second);

            plan.Validate();

            Assert.Equal(first.Id, second.DependsOnStepIds[0]);
            Assert.Equal("Prepared state unavailable", second.FailureConditions[0]);
            Assert.Equal("Operation confirmed", second.CompletionCriteria[0]);
        }

        [Fact]
        public void Plan_Clone_DetachesNestedCollections()
        {
            var plan = new AiPlan
            {
                GoalId = "goal-1",
                IntentionId = "intention-1",
                Title = "Clone test",
                Provenance = new AiGoalProvenance { Authority = AiGoalAuthority.HostSupplied, Source = "Host" }
            };
            plan.Preconditions.Add("Original");
            var step = new AiPlanStep
            {
                PlanId = plan.Id,
                Sequence = 1,
                Title = "Original step",
                Provenance = plan.Provenance.Clone()
            };
            step.DependsOnStepIds.Add("dependency-1");
            plan.Steps.Add(step);

            var clone = plan.Clone();
            clone.Preconditions[0] = "Changed";
            clone.Steps[0].Title = "Changed step";
            clone.Steps[0].DependsOnStepIds[0] = "dependency-2";

            Assert.Equal("Original", plan.Preconditions[0]);
            Assert.Equal("Original step", plan.Steps[0].Title);
            Assert.Equal("dependency-1", plan.Steps[0].DependsOnStepIds[0]);
        }

        [Fact]
        public void Plan_RequiresRevisionReason_WhenRevisionIsPositive()
        {
            var plan = new AiPlan
            {
                GoalId = "goal-1",
                IntentionId = "intention-1",
                Title = "Revision test",
                Revision = 1L,
                Provenance = new AiGoalProvenance { Authority = AiGoalAuthority.AgentInferred, Source = "Planner" }
            };

            Assert.Throws<System.ArgumentException>(() => plan.Validate());
        }
    }
}

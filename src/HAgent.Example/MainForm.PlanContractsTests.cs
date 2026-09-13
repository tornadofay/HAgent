using System;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddPlanContractsTab()
        {
            AddApiTab(
                "PLAN CONTRACTS",
                "Run plan contract test",
                "Verifies 0.9591 Slice 2 durable Plan and PlanStep contracts: goal/intention linkage, revision metadata, ordered steps, explicit dependencies, execution semantics, and detached cloning.",
                "Plans remain separate durable structures beneath goals and intentions. Each step carries explicit status, ordering, dependencies, preconditions, assumptions, expected effects, completion criteria, and failure conditions.",
                "Durable plan contract verification.",
                TestPlanContracts,
                "Durable plan contract boundary",
                "Provider-free contract exercise; no external provider or persistence backend is contacted.");
        }

        private Task TestPlanContracts(string message)
        {
            var plan = new AiPlan
            {
                GoalId = "goal-example",
                IntentionId = "intention-example",
                Title = string.IsNullOrWhiteSpace(message) ? "Example plan" : message,
                Description = "A bounded durable plan for an adopted intention.",
                Status = AiPlanStatus.Active,
                Revision = 2L,
                RevisionReason = "Plan revised after new evidence.",
                Provenance = new AiGoalProvenance
                {
                    Authority = AiGoalAuthority.AgentInferred,
                    Source = "Example planning strategy",
                    Evidence = "Current intention remains valid."
                }
            };
            plan.Preconditions.Add("Required capability is available.");
            plan.Assumptions.Add("Host constraints remain unchanged.");
            plan.ExpectedEffects.Add("Goal progress is advanced.");
            plan.FailureConditions.Add("Required capability becomes unavailable.");
            plan.CompletionCriteria.Add("All required steps complete.");

            var prepare = new AiPlanStep
            {
                PlanId = plan.Id,
                Sequence = 1,
                Title = "Prepare",
                Description = "Prepare the required state.",
                Status = AiPlanStepStatus.Completed,
                Provenance = plan.Provenance.Clone(),
                Revision = 1L
            };
            prepare.CompletionCriteria.Add("Preparation confirmed.");

            var execute = new AiPlanStep
            {
                PlanId = plan.Id,
                Sequence = 2,
                Title = "Execute",
                Description = "Perform the next bounded operation.",
                Status = AiPlanStepStatus.Ready,
                Provenance = plan.Provenance.Clone(),
                Revision = 1L
            };
            execute.DependsOnStepIds.Add(prepare.Id);
            execute.Preconditions.Add("Preparation is complete.");
            execute.Assumptions.Add("The selected capability remains valid.");
            execute.ExpectedEffects.Add("The intended operation is completed.");
            execute.FailureConditions.Add("The required capability is unavailable.");
            execute.CompletionCriteria.Add("Operation completion is observed.");

            plan.Steps.Add(prepare);
            plan.Steps.Add(execute);
            plan.Validate();

            var clone = plan.Clone();
            clone.Steps[0].Title = "Changed clone";

            if (plan.Steps.Count != 2 || execute.DependsOnStepIds.Count != 1 || execute.DependsOnStepIds[0] != prepare.Id)
                throw new InvalidOperationException("Plan step ordering/dependency contract was not preserved.");
            if (plan.Steps[0].Title == "Changed clone")
                throw new InvalidOperationException("Plan clone did not detach nested step state.");
            if (plan.Revision != 2L || string.IsNullOrWhiteSpace(plan.RevisionReason))
                throw new InvalidOperationException("Plan revision metadata was not preserved.");

            Write("PLAN CONTRACTS",
                "Contract test succeeded." + Environment.NewLine +
                "Plan ID: " + plan.Id + Environment.NewLine +
                "Goal ID: " + plan.GoalId + Environment.NewLine +
                "Intention ID: " + plan.IntentionId + Environment.NewLine +
                "Plan status: " + plan.Status + Environment.NewLine +
                "Plan revision: " + plan.Revision + Environment.NewLine +
                "Step count: " + plan.Steps.Count + Environment.NewLine +
                "Dependency preserved: yes" + Environment.NewLine +
                "Nested clone detached: yes");

            return Task.CompletedTask;
        }
    }
}

using System;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddCheckpointOutcomeContractsTab()
        {
            AddApiTab(
                "CHECKPOINT & OUTCOME CONTRACTS",
                "Run checkpoint/outcome contract test",
                "Verifies 0.9591 Slice 3 checkpoint boundaries and durable terminal outcome semantics, including explicit unknown external outcomes.",
                "Checkpoints preserve plan revision and boundary evidence. Outcomes distinguish completion, failure, unknown outcome, cancellation, and supersession without treating uncertainty as success.",
                "Checkpoint and outcome contract verification.",
                TestCheckpointOutcomeContracts,
                "Durable checkpoint/outcome contract boundary",
                "Provider-free contract exercise; no external provider or persistence backend is contacted.");

            AddPlanRetryIdempotencyContractsTab();
        }

        private Task TestCheckpointOutcomeContracts(string message)
        {
            var now = DateTimeOffset.UtcNow;
            var checkpoint = new AiPlanCheckpoint(
                "checkpoint-example",
                "plan-example",
                "step-2",
                3L,
                AiCheckpointStatus.Reached,
                string.IsNullOrWhiteSpace(message) ? "After validation" : message,
                "Validation completed at a durable safe point.",
                now);

            var completed = new AiPlanStepOutcome(
                "outcome-completed",
                "plan-example",
                "step-2",
                3L,
                AiPlanOutcomeStatus.Completed,
                "Step completed and was observed at the checkpoint boundary.",
                "Observed completion evidence.",
                now.AddSeconds(1));

            var unknown = new AiPlanStepOutcome(
                "outcome-unknown",
                "plan-example",
                "step-3",
                3L,
                AiPlanOutcomeStatus.UnknownOutcome,
                "External side effect result could not be verified.",
                "Timeout after request submission.",
                now.AddSeconds(2));

            if (checkpoint.PlanRevision != 3L || checkpoint.PlanStepId != "step-2")
                throw new InvalidOperationException("Checkpoint did not preserve its plan revision or step boundary.");
            if (completed.Status != AiPlanOutcomeStatus.Completed || string.IsNullOrWhiteSpace(completed.Evidence))
                throw new InvalidOperationException("Completed outcome did not preserve completion evidence.");
            if (unknown.Status != AiPlanOutcomeStatus.UnknownOutcome || unknown.Status == AiPlanOutcomeStatus.Completed)
                throw new InvalidOperationException("Unknown external outcome was incorrectly treated as success.");

            Write("CHECKPOINT & OUTCOME CONTRACTS",
                "Contract test succeeded." + Environment.NewLine +
                "Checkpoint ID: " + checkpoint.Id + Environment.NewLine +
                "Plan revision: " + checkpoint.PlanRevision + Environment.NewLine +
                "Checkpoint boundary: " + checkpoint.Boundary + Environment.NewLine +
                "Completed outcome: " + completed.Status + Environment.NewLine +
                "Unknown outcome: " + unknown.Status + Environment.NewLine +
                "Unknown outcome treated as success: no");

            return Task.CompletedTask;
        }
    }
}

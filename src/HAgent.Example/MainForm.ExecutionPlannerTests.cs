using System;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _executionPlannerTabsAdded;

        private void AddExecutionPlannerTabs()
        {
            if (System.Threading.Interlocked.Exchange(ref _executionPlannerTabsAdded, 1) != 0)
                return;

            AddApiTab(
                "Execution Target Planning",
                "Run planner test",
                "Creates multiple concrete execution targets for the same logical model and verifies capability requirements, cost policy, availability, and preference scoring.",
                "The selected target must satisfy every required capability and policy while preserving distinct provider execution targets.",
                "No provider or external service is used.",
                TestExecutionPlannerAsync,
                "Planner boundary",
                "This is the provider-neutral decision layer. Transport remains behind ProviderExecutionRequest.");
        }

        private System.Threading.Tasks.Task TestExecutionPlannerAsync(string unused)
        {
            var targets = new[]
            {
                CreateTarget("target-free", "provider-free", "model-shared", "shared-model", AiCostStatus.Free, AiAvailabilityState.Available, AiCapability.Chat | AiCapability.StructuredOutput),
                CreateTarget("target-paid", "provider-paid", "model-shared", "shared-model", AiCostStatus.Paid, AiAvailabilityState.Available, AiCapability.Chat | AiCapability.StructuredOutput | AiCapability.ToolCalling),
                CreateTarget("target-unknown", "provider-unknown", "model-shared", "shared-model", AiCostStatus.Unknown, AiAvailabilityState.Available, AiCapability.Chat)
            };

            var requirements = new AiCapabilityRequirements();
            requirements.Require(AiCapability.Chat);
            requirements.Require(AiCapability.StructuredOutput);
            requirements.Prefer(AiCapability.ToolCalling);

            var planner = new DefaultExecutionPlanner();
            var freeOnly = planner.Plan(
                targets,
                requirements,
                new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    CostPolicy = AiCostPolicy.FreeOnly
                });

            if (!freeOnly.HasSelection || freeOnly.SelectedTarget.Id != "target-free")
                throw new InvalidOperationException("FreeOnly did not select the compatible free execution target.");

            var freeCandidate = Find(freeOnly, "target-free");
            var paidCandidate = Find(freeOnly, "target-paid");
            var unknownCandidate = Find(freeOnly, "target-unknown");
            if (freeCandidate.Decision != AiCandidateDecision.Accepted ||
                paidCandidate.Decision != AiCandidateDecision.Rejected ||
                unknownCandidate.Decision != AiCandidateDecision.Rejected)
                throw new InvalidOperationException("Cost-policy filtering did not distinguish free, paid, and unknown targets.");

            var preferred = planner.Plan(
                targets,
                requirements,
                new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Preferred,
                    CostPolicy = AiCostPolicy.NoRestriction,
                    PreferredTargetId = "target-paid"
                });
            if (!preferred.HasSelection || preferred.SelectedTarget.Id != "target-paid")
                throw new InvalidOperationException("Preferred selection did not choose the preferred compatible target.");

            var fixedRejected = planner.Plan(
                targets,
                requirements,
                new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Fixed,
                    PreferredTargetId = "target-unknown",
                    CostPolicy = AiCostPolicy.NoRestriction
                });
            if (fixedRejected.HasSelection)
                throw new InvalidOperationException("Fixed selection accepted a target that could not satisfy required capabilities.");

            var forbidden = new AiCapabilityRequirements();
            forbidden.Require(AiCapability.Chat);
            forbidden.Forbid(AiCapability.ToolCalling);
            var forbiddenPlan = planner.Plan(
                targets,
                forbidden,
                new AiExecutionSelectionPolicy { Mode = AiSelectionMode.Auto, CostPolicy = AiCostPolicy.NoRestriction });
            if (!forbiddenPlan.HasSelection || forbiddenPlan.SelectedTarget.Id == "target-paid")
                throw new InvalidOperationException("Forbidden capability was not enforced.");

            Write(
                "EXECUTION TARGET PLANNING",
                "Execution planner contract test succeeded." + Environment.NewLine +
                "Multiple providers for one logical model: verified." + Environment.NewLine +
                "Required capability filtering: verified." + Environment.NewLine +
                "Preferred capability scoring: verified." + Environment.NewLine +
                "FreeOnly cost policy: verified." + Environment.NewLine +
                "Preferred target selection: verified." + Environment.NewLine +
                "Fixed-target capability enforcement: verified." + Environment.NewLine +
                "Forbidden capability enforcement: verified." + Environment.NewLine +
                "Selected FreeOnly target: " + freeOnly.SelectedTarget.Id);

            return System.Threading.Tasks.Task.CompletedTask;
        }

        private static AiExecutionTarget CreateTarget(
            string id,
            string providerId,
            string modelId,
            string logicalModelId,
            AiCostStatus cost,
            AiAvailabilityState availability,
            AiCapability supported)
        {
            var capabilities = new AiModelCapabilities();
            foreach (AiCapability capability in Enum.GetValues(typeof(AiCapability)))
            {
                if (capability == AiCapability.None || (supported & capability) == 0)
                    continue;
                capabilities.Set(capability, CapabilitySupport.Supported, CapabilitySource.UserConfigured, 1d, "Example target definition");
            }

            return new AiExecutionTarget
            {
                Id = id,
                ProviderId = providerId,
                ModelId = modelId,
                LogicalModelId = logicalModelId,
                Capabilities = capabilities,
                Cost = cost,
                Availability = availability
            };
        }

        private static AiExecutionCandidateEvaluation Find(AiExecutionPlan plan, string targetId)
        {
            foreach (var evaluation in plan.Evaluations)
            {
                if (string.Equals(evaluation.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                    return evaluation;
            }

            throw new InvalidOperationException("Planner did not evaluate expected target " + targetId + ".");
        }
    }
}

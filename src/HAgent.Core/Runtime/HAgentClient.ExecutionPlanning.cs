using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        private sealed class PlannedExecutionTarget
        {
            public AiAgent Agent { get; set; }
            public AiProvider Provider { get; set; }
            public AiExecutionTarget Target { get; set; }
            public IAiProviderAdapter Adapter { get; set; }
        }

        private async Task<PlannedExecutionTarget> PlanExecutionTargetAsync(
            string agentId,
            AiCapabilityRequirements requirements,
            CancellationToken cancellationToken)
        {
            var agents = await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false);
            var agent = agents.FirstOrDefault(x => string.Equals(x.Id, agentId, StringComparison.OrdinalIgnoreCase));
            if (agent == null)
                throw new InvalidOperationException("Agent was not found: " + agentId);
            if (!agent.Enabled)
                throw new InvalidOperationException("Agent is disabled: " + agent.Name);

            var providers = await _store.GetProvidersAsync(cancellationToken).ConfigureAwait(false);
            var policy = agent.ExecutionSelection == null
                ? new AiExecutionSelectionPolicy()
                : agent.ExecutionSelection.Clone();
            var effectiveRequirements = requirements == null
                ? (agent.CapabilityRequirements == null ? new AiCapabilityRequirements() : agent.CapabilityRequirements.Clone())
                : requirements.Clone();

            policy.Validate();
            var targets = await _executionTargetCatalog
                .GetTargetsAsync(providers, cancellationToken)
                .ConfigureAwait(false);
            var plan = _executionPlanner.Plan(targets, effectiveRequirements, policy);
            if (!plan.HasSelection)
                throw new InvalidOperationException(
                    "No compatible execution target was selected for agent '" + agent.Name + "'. " +
                    BuildClientPlannerFailureSummary(plan));

            var target = plan.SelectedTarget;
            var provider = providers.FirstOrDefault(x =>
                x != null &&
                string.Equals(x.Id, target.ProviderId, StringComparison.OrdinalIgnoreCase) &&
                x.Enabled);
            if (provider == null)
                throw new InvalidOperationException("The selected execution target references an unavailable provider: " + target.ProviderId);

            var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
            if (adapter == null)
                throw new InvalidOperationException("No registered adapter can handle provider '" + provider.Name + "' of kind '" + provider.Kind + "'.");

            return new PlannedExecutionTarget
            {
                Agent = agent,
                Provider = provider,
                Target = target,
                Adapter = adapter
            };
        }

        private static string BuildClientPlannerFailureSummary(AiExecutionPlan plan)
        {
            if (plan == null || plan.Evaluations == null || plan.Evaluations.Count == 0)
                return "No execution candidates were available.";

            var reasons = plan.Evaluations
                .Where(x => x != null)
                .Select(x => string.Join("; ", x.Reasons ?? new List<string>()))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(3)
                .ToArray();
            return reasons.Length == 0
                ? "All candidates were rejected."
                : "Candidate diagnostics: " + string.Join(" | ", reasons);
        }
    }
}

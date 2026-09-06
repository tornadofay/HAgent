using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class DefaultExecutionPlanner : IExecutionPlanner
    {
        public AiExecutionPlan Plan(
            IReadOnlyCollection<AiExecutionTarget> candidates,
            AiCapabilityRequirements requirements,
            AiExecutionSelectionPolicy policy)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            policy.Validate();

            var plan = new AiExecutionPlan();
            var accepted = new List<Tuple<AiExecutionTarget, AiExecutionCandidateEvaluation>>();

            foreach (var target in candidates)
            {
                if (target == null) continue;
                var evaluation = Evaluate(target, requirements, policy);
                plan.Evaluations.Add(evaluation);
                if (evaluation.Decision == AiCandidateDecision.Accepted)
                    accepted.Add(Tuple.Create(target, evaluation));
            }

            if (accepted.Count == 0)
                return plan;

            var selected = accepted
                .OrderByDescending(x => x.Item2.Score)
                .ThenBy(x => x.Item1.Id, StringComparer.OrdinalIgnoreCase)
                .First();

            plan.SelectedTarget = selected.Item1;
            return plan;
        }

        private static AiExecutionCandidateEvaluation Evaluate(
            AiExecutionTarget target,
            AiCapabilityRequirements requirements,
            AiExecutionSelectionPolicy policy)
        {
            var result = new AiExecutionCandidateEvaluation { TargetId = target.Id };
            try
            {
                target.Validate();
            }
            catch (Exception ex)
            {
                result.Reasons.Add("Target invalid: " + ex.Message);
                return result;
            }

            if (policy.Mode == AiSelectionMode.Fixed && !string.Equals(policy.PreferredTargetId, target.Id, StringComparison.OrdinalIgnoreCase))
            {
                result.Reasons.Add("Target is not the fixed execution target.");
                return result;
            }

            if (!string.IsNullOrWhiteSpace(policy.PreferredProviderId) &&
                string.Equals(policy.PreferredProviderId, target.ProviderId, StringComparison.OrdinalIgnoreCase))
            {
                result.Score += 1000d;
                result.Reasons.Add("Preferred provider match.");
            }

            if (!string.IsNullOrWhiteSpace(policy.PreferredLogicalModelId) &&
                string.Equals(policy.PreferredLogicalModelId, target.LogicalModelId, StringComparison.OrdinalIgnoreCase))
            {
                result.Score += 500d;
                result.Reasons.Add("Preferred logical model match.");
            }

            if (policy.Mode == AiSelectionMode.Preferred &&
                string.Equals(policy.PreferredTargetId, target.Id, StringComparison.OrdinalIgnoreCase))
            {
                result.Score += 2000d;
                result.Reasons.Add("Preferred target match.");
            }

            if (!EvaluateCost(target.Cost, policy.CostPolicy, result))
                return result;

            if (target.Availability == AiAvailabilityState.Unavailable)
            {
                result.Reasons.Add("Target is unavailable.");
                return result;
            }

            if (target.Availability == AiAvailabilityState.Available)
            {
                result.Score += 100d;
                result.Reasons.Add("Target is available.");
            }
            else if (target.Availability == AiAvailabilityState.Degraded)
            {
                result.Score += 10d;
                result.Reasons.Add("Target is degraded; selection remains permitted.");
            }
            else
            {
                result.Score += 1d;
                result.Reasons.Add("Availability is unknown.");
            }

            foreach (var requirement in requirements.Items)
            {
                if (requirement == null || requirement.Capability == AiCapability.None)
                    continue;

                var support = target.Capabilities.Get(requirement.Capability);
                switch (requirement.Strength)
                {
                    case CapabilityRequirementStrength.Required:
                        if (support != CapabilitySupport.Supported)
                        {
                            result.Reasons.Add("Required capability is " + support + ".");
                            return result;
                        }
                        result.Score += 100d;
                        break;

                    case CapabilityRequirementStrength.Preferred:
                        if (support == CapabilitySupport.Supported)
                        {
                            result.Score += 25d;
                            result.Reasons.Add("Preferred capability supported.");
                        }
                        else if (support == CapabilitySupport.Unknown)
                        {
                            result.Score += 0d;
                            result.Reasons.Add("Preferred capability unknown.");
                        }
                        break;

                    case CapabilityRequirementStrength.Forbidden:
                        if (support == CapabilitySupport.Supported)
                        {
                            result.Reasons.Add("Forbidden capability is supported.");
                            return result;
                        }
                        break;

                    case CapabilityRequirementStrength.Optional:
                        if (support == CapabilitySupport.Supported)
                            result.Score += 5d;
                        break;
                }
            }

            result.Decision = AiCandidateDecision.Accepted;
            result.Reasons.Add("All required constraints satisfied.");
            return result;
        }

        private static bool EvaluateCost(AiCostStatus cost, AiCostPolicy policy, AiExecutionCandidateEvaluation result)
        {
            if (policy == AiCostPolicy.NoRestriction)
            {
                result.Score += cost == AiCostStatus.Free ? 20d : 0d;
                return true;
            }

            if (policy == AiCostPolicy.FreeOnly)
            {
                if (cost != AiCostStatus.Free && cost != AiCostStatus.FreeWithinQuota)
                {
                    result.Reasons.Add("Cost policy requires known free/free-within-quota target.");
                    return false;
                }

                result.Score += cost == AiCostStatus.Free ? 100d : 80d;
                result.Reasons.Add("Free-only policy satisfied.");
                return true;
            }

            if (cost == AiCostStatus.Free || cost == AiCostStatus.FreeWithinQuota)
                result.Score += 100d;
            else if (cost == AiCostStatus.Unknown)
                result.Score += 1d;

            result.Reasons.Add("Free-preferred policy evaluated.");
            return true;
        }
    }
}

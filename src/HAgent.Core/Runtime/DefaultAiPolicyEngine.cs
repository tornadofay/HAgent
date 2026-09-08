using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class DefaultAiPolicyEngine : IAiPolicyEngine
    {
        private readonly AiPolicySet _policy;

        public DefaultAiPolicyEngine(AiPolicySet policy = null)
        {
            _policy = policy == null ? new AiPolicySet() : policy.Clone();
            _policy.Validate();
        }

        public string PolicyVersion
        {
            get { return _policy.Version; }
        }

        public AiPolicySet GetPolicySnapshot()
        {
            return _policy.Clone();
        }

        public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.Validate();

            var costDecision = EvaluateCost(context);
            if (costDecision != null)
                return costDecision;

            var candidates = (_policy.Rules ?? new List<AiPolicyRule>())
                .Where(x => x != null && Matches(x, context))
                .Select(x => new RuleMatch(x, GetScopeSpecificity(x.Scope), GetMatchSpecificity(x)))
                .OrderByDescending(x => x.Rule.Priority)
                .ThenByDescending(x => x.ScopeSpecificity)
                .ThenByDescending(x => x.MatchSpecificity)
                .ThenByDescending(x => GetOutcomeRestrictiveness(x.Rule.Outcome))
                .ThenBy(x => x.Rule.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (candidates.Count == 0)
                return new AiPolicyDecision
                {
                    Outcome = AiPolicyOutcome.NotApplicable,
                    PolicyVersion = PolicyVersion,
                    Reason = "No matching policy rule applies."
                };

            var selected = candidates[0].Rule;
            return new AiPolicyDecision
            {
                Outcome = selected.Outcome,
                PolicyVersion = PolicyVersion,
                RuleId = selected.Id,
                RuleName = selected.Name,
                Scope = selected.Scope,
                Priority = selected.Priority,
                Reason = string.IsNullOrWhiteSpace(selected.Reason)
                    ? "Matched policy rule '" + selected.Id + "'."
                    : selected.Reason
            };
        }

        private AiPolicyDecision EvaluateCost(AiPolicyEvaluationContext context)
        {
            if (context.RequestedCostPolicy != AiCostPolicy.FreeOnly)
                return null;

            if (context.CostStatus == AiCostStatus.Free || context.CostStatus == AiCostStatus.FreeWithinQuota)
                return null;

            return new AiPolicyDecision
            {
                Outcome = AiPolicyOutcome.Deny,
                PolicyVersion = PolicyVersion,
                RuleId = "builtin.cost.free-only",
                RuleName = "FreeOnly cost guard",
                Scope = AiPolicyScopeKind.ExecutionTarget,
                Priority = int.MaxValue,
                Reason = context.CostStatus == AiCostStatus.Unknown
                    ? "FreeOnly requires the execution target cost to be known as Free or FreeWithinQuota."
                    : "FreeOnly rejects a paid execution target.",
                IsBuiltIn = true
            };
        }

        private static bool Matches(AiPolicyRule rule, AiPolicyEvaluationContext context)
        {
            if (!ScopeMatches(rule, context)) return false;
            if (!MatchesValues(rule.Operations, context.Operation)) return false;
            if (!MatchesValues(rule.ResourceTypes, context.ResourceType)) return false;
            if (!MatchesValues(rule.ResourceIds, context.ResourceId)) return false;
            if (!MatchesValues(rule.ToolIds, context.ToolId)) return false;
            if (!MatchesValues(rule.ProviderIds, context.ProviderId)) return false;
            if (!MatchesValues(rule.ExecutionTargetIds, context.ExecutionTargetId)) return false;

            foreach (var condition in rule.Attributes ?? new Dictionary<string, string>())
            {
                string actual;
                if (!context.Attributes.TryGetValue(condition.Key, out actual)) return false;
                if (!string.Equals(actual ?? string.Empty, condition.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase)) return false;
            }
            return true;
        }

        private static bool ScopeMatches(AiPolicyRule rule, AiPolicyEvaluationContext context)
        {
            switch (rule.Scope)
            {
                case AiPolicyScopeKind.System:
                    return true;
                case AiPolicyScopeKind.Tenant:
                    return context.Identity != null && string.Equals(context.Identity.TenantId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.User:
                    return context.Identity != null && string.Equals(context.Identity.UserId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Workspace:
                    return context.Identity != null && string.Equals(context.Identity.WorkspaceId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Agent:
                    return string.Equals(context.AgentProfileId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Runtime:
                    return string.Equals(context.RuntimeInstanceId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Execution:
                    return string.Equals(context.ExecutionId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Resource:
                    return string.Equals(context.ResourceId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Tool:
                    return string.Equals(context.ToolId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.Provider:
                    return string.Equals(context.ProviderId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                case AiPolicyScopeKind.ExecutionTarget:
                    return string.Equals(context.ExecutionTargetId, rule.ScopeId, StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        private static bool MatchesValues(IList<string> values, string actual)
        {
            if (values == null || values.Count == 0)
                return true;
            foreach (var value in values)
                if (string.Equals(value, actual, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static int GetScopeSpecificity(AiPolicyScopeKind scope)
        {
            switch (scope)
            {
                case AiPolicyScopeKind.Execution: return 110;
                case AiPolicyScopeKind.Runtime: return 100;
                case AiPolicyScopeKind.Agent: return 90;
                case AiPolicyScopeKind.Resource: return 85;
                case AiPolicyScopeKind.Tool: return 85;
                case AiPolicyScopeKind.Workspace: return 80;
                case AiPolicyScopeKind.User: return 70;
                case AiPolicyScopeKind.Tenant: return 60;
                case AiPolicyScopeKind.ExecutionTarget: return 55;
                case AiPolicyScopeKind.Provider: return 50;
                case AiPolicyScopeKind.System: return 10;
                default: return 0;
            }
        }

        private static int GetMatchSpecificity(AiPolicyRule rule)
        {
            var score = 0;
            if (rule.Operations != null && rule.Operations.Count > 0) score += 8;
            if (rule.ResourceTypes != null && rule.ResourceTypes.Count > 0) score += 4;
            if (rule.ResourceIds != null && rule.ResourceIds.Count > 0) score += 8;
            if (rule.ToolIds != null && rule.ToolIds.Count > 0) score += 8;
            if (rule.ProviderIds != null && rule.ProviderIds.Count > 0) score += 6;
            if (rule.ExecutionTargetIds != null && rule.ExecutionTargetIds.Count > 0) score += 8;
            if (rule.Attributes != null) score += rule.Attributes.Count;
            return score;
        }

        private static int GetOutcomeRestrictiveness(AiPolicyOutcome outcome)
        {
            switch (outcome)
            {
                case AiPolicyOutcome.Deny: return 50;
                case AiPolicyOutcome.RequireApproval: return 40;
                case AiPolicyOutcome.Defer: return 30;
                case AiPolicyOutcome.Allow: return 20;
                default: return 0;
            }
        }

        private sealed class RuleMatch
        {
            public RuleMatch(AiPolicyRule rule, int scopeSpecificity, int matchSpecificity)
            {
                Rule = rule;
                ScopeSpecificity = scopeSpecificity;
                MatchSpecificity = matchSpecificity;
            }

            public AiPolicyRule Rule { get; private set; }
            public int ScopeSpecificity { get; private set; }
            public int MatchSpecificity { get; private set; }
        }
    }
}

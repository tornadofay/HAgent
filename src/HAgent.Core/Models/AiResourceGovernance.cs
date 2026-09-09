using System;
using System.Collections.Generic;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public sealed class AiResourceGovernanceRequest
    {
        public AiResourceGovernanceRequest()
        {
            Operation = string.Empty;
            ResourceType = string.Empty;
            ResourceId = string.Empty;
            Scope = AgentResourceScope.Global;
            ResourceOwnerId = string.Empty;
            AgentProfileId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            Identity = new AgentIdentityContext();
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Operation { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string ResourceOwnerId { get; set; }
        public string AgentProfileId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public AgentIdentityContext Identity { get; set; }
        public IDictionary<string, string> Attributes { get; private set; }

        public AiResourceGovernanceRequest Clone()
        {
            var clone = new AiResourceGovernanceRequest
            {
                Operation = Operation,
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Scope = Scope,
                ResourceOwnerId = ResourceOwnerId,
                AgentProfileId = AgentProfileId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                Identity = Identity == null ? new AgentIdentityContext() : Identity.Clone()
            };

            foreach (var pair in Attributes ?? new Dictionary<string, string>())
                clone.Attributes[pair.Key] = pair.Value;
            return clone;
        }

        public void Validate()
        {
            Require(Operation, nameof(Operation), 256);
            Require(ResourceType, nameof(ResourceType), 256);
            Require(ResourceId, nameof(ResourceId), 512, false);
            Require(ResourceOwnerId, nameof(ResourceOwnerId), 2048, false);
            Require(AgentProfileId, nameof(AgentProfileId), 512, false);
            Require(RuntimeInstanceId, nameof(RuntimeInstanceId), 512, false);
            Require(ExecutionId, nameof(ExecutionId), 512, false);
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentOutOfRangeException(nameof(Scope));
            if (Scope != AgentResourceScope.Global && string.IsNullOrWhiteSpace(ResourceOwnerId))
                throw new ArgumentException("Non-global resource governance requests require the authoritative resource owner ID.", nameof(ResourceOwnerId));
            if (Identity != null)
                Identity.Validate();
            if (Attributes == null)
                throw new ArgumentNullException(nameof(Attributes));
            if (Attributes.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Attributes));
            foreach (var pair in Attributes)
            {
                Require(pair.Key, nameof(Attributes), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentOutOfRangeException(nameof(Attributes));
            }
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiResourceGovernanceDecision
    {
        public AiResourceGovernanceDecision()
        {
            ResourceType = string.Empty;
            ResourceId = string.Empty;
            ResourceOwnerId = string.Empty;
            ExpectedOwnerId = string.Empty;
            AgentProfileId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            Reason = string.Empty;
            OwnershipMatches = false;
            Allowed = false;
            RequiresApproval = false;
            ResourceCapabilityState = AiResourceCapabilityState.Enabled;
            ResourceCapabilitySource = AiResourceCapabilitySource.Default;
        }

        public bool Allowed { get; set; }
        public bool RequiresApproval { get; set; }
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string ResourceOwnerId { get; set; }
        public string ExpectedOwnerId { get; set; }
        public bool OwnershipMatches { get; set; }
        public string AgentProfileId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public AiResourceCapabilityState ResourceCapabilityState { get; set; }
        public AiResourceCapabilitySource ResourceCapabilitySource { get; set; }
        public AiPolicyDecision PolicyDecision { get; set; }
        public string Reason { get; set; }

        public AiResourceGovernanceDecision Clone()
        {
            return new AiResourceGovernanceDecision
            {
                Allowed = Allowed,
                RequiresApproval = RequiresApproval,
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Scope = Scope,
                ResourceOwnerId = ResourceOwnerId,
                ExpectedOwnerId = ExpectedOwnerId,
                OwnershipMatches = OwnershipMatches,
                AgentProfileId = AgentProfileId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                ResourceCapabilityState = ResourceCapabilityState,
                ResourceCapabilitySource = ResourceCapabilitySource,
                PolicyDecision = ClonePolicyDecision(PolicyDecision),
                Reason = Reason
            };
        }

        public void Validate()
        {
            Require(ResourceType, nameof(ResourceType), 256);
            Require(ResourceId, nameof(ResourceId), 512, false);
            Require(ResourceOwnerId, nameof(ResourceOwnerId), 2048, false);
            Require(ExpectedOwnerId, nameof(ExpectedOwnerId), 2048);
            Require(AgentProfileId, nameof(AgentProfileId), 512, false);
            Require(RuntimeInstanceId, nameof(RuntimeInstanceId), 512, false);
            Require(ExecutionId, nameof(ExecutionId), 512, false);
            Require(Reason, nameof(Reason), 2048, false);
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentOutOfRangeException(nameof(Scope));
            if (Scope != AgentResourceScope.Global && string.IsNullOrWhiteSpace(ResourceOwnerId))
                throw new ArgumentException("Non-global resource governance decisions require the authoritative resource owner ID.", nameof(ResourceOwnerId));
            if (!Enum.IsDefined(typeof(AiResourceCapabilityState), ResourceCapabilityState) || ResourceCapabilityState == AiResourceCapabilityState.Inherit)
                throw new ArgumentOutOfRangeException(nameof(ResourceCapabilityState));
            if (!Enum.IsDefined(typeof(AiResourceCapabilitySource), ResourceCapabilitySource))
                throw new ArgumentOutOfRangeException(nameof(ResourceCapabilitySource));
            if (PolicyDecision != null && string.IsNullOrWhiteSpace(PolicyDecision.PolicyVersion))
                throw new ArgumentException("A supplied resource governance policy decision must identify its policy version.", nameof(PolicyDecision));
            if (Allowed && (!OwnershipMatches || ResourceCapabilityState == AiResourceCapabilityState.Disabled || RequiresApproval))
                throw new ArgumentException("An allowed resource governance decision must satisfy ownership, capability, and approval constraints.", nameof(Allowed));
        }

        private static AiPolicyDecision ClonePolicyDecision(AiPolicyDecision decision)
        {
            if (decision == null) return null;
            return new AiPolicyDecision
            {
                Outcome = decision.Outcome,
                PolicyVersion = decision.PolicyVersion,
                RuleId = decision.RuleId,
                RuleName = decision.RuleName,
                Scope = decision.Scope,
                Priority = decision.Priority,
                Reason = decision.Reason,
                IsBuiltIn = decision.IsBuiltIn
            };
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiResourceGovernanceEvaluator
    {
        private readonly IAiPolicyEngine _policyEngine;
        private readonly AiResourceCapabilitySnapshot _resourceCapabilities;

        public AiResourceGovernanceEvaluator(
            IAiPolicyEngine policyEngine,
            AiResourceCapabilitySnapshot resourceCapabilities)
        {
            if (policyEngine == null) throw new ArgumentNullException(nameof(policyEngine));
            if (resourceCapabilities == null) throw new ArgumentNullException(nameof(resourceCapabilities));
            _policyEngine = policyEngine;
            _resourceCapabilities = resourceCapabilities.Clone();
            _resourceCapabilities.Validate();
        }

        public AiResourceGovernanceDecision Evaluate(AiResourceGovernanceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var snapshot = request.Clone();
            snapshot.Validate();

            var expectedOwnerId = AgentResourceOwnership.GetOwnerId(snapshot.Scope, snapshot.Identity, snapshot.ResourceId);
            var ownershipMatches = snapshot.Scope == AgentResourceScope.Global && string.IsNullOrWhiteSpace(snapshot.ResourceOwnerId)
                ? true
                : string.Equals(snapshot.ResourceOwnerId, expectedOwnerId, StringComparison.Ordinal);

            var decision = new AiResourceGovernanceDecision
            {
                Allowed = true,
                ResourceType = snapshot.ResourceType,
                ResourceId = snapshot.ResourceId ?? string.Empty,
                Scope = snapshot.Scope,
                ResourceOwnerId = snapshot.ResourceOwnerId ?? string.Empty,
                ExpectedOwnerId = expectedOwnerId,
                OwnershipMatches = ownershipMatches,
                AgentProfileId = snapshot.AgentProfileId ?? string.Empty,
                RuntimeInstanceId = snapshot.RuntimeInstanceId ?? string.Empty,
                ExecutionId = snapshot.ExecutionId ?? string.Empty,
                ResourceCapabilityState = _resourceCapabilities.GetState(snapshot.ResourceType, snapshot.ResourceId),
                ResourceCapabilitySource = _resourceCapabilities.GetSource(snapshot.ResourceType, snapshot.ResourceId),
                Reason = "Resource is admitted."
            };

            if (!ownershipMatches)
            {
                decision.Allowed = false;
                decision.Reason = "Resource ownership does not match the canonical identity-derived owner.";
                decision.Validate();
                return decision;
            }

            if (decision.ResourceCapabilityState == AiResourceCapabilityState.Disabled)
            {
                decision.Allowed = false;
                decision.Reason = "Resource is disabled by effective capability configuration.";
                decision.Validate();
                return decision;
            }

            decision.PolicyDecision = _policyEngine.Evaluate(ToPolicyContext(snapshot, expectedOwnerId));
            if (decision.PolicyDecision == null)
                throw new InvalidOperationException("The policy engine returned no resource governance decision.");

            if (decision.PolicyDecision.IsDenied || decision.PolicyDecision.IsDeferred || decision.PolicyDecision.RequiresApproval)
            {
                decision.Allowed = false;
                decision.RequiresApproval = decision.PolicyDecision.RequiresApproval;
                decision.Reason = BuildPolicyReason(decision.PolicyDecision);
                decision.Validate();
                return decision;
            }

            if (decision.PolicyDecision.Outcome == AiPolicyOutcome.NotApplicable)
            {
                decision.Allowed = false;
                decision.Reason = "No applicable policy rule authorizes the requested resource operation.";
                decision.Validate();
                return decision;
            }

            decision.Validate();
            return decision;
        }

        private static AiPolicyEvaluationContext ToPolicyContext(AiResourceGovernanceRequest request, string expectedOwnerId)
        {
            var context = new AiPolicyEvaluationContext
            {
                Operation = request.Operation,
                ResourceType = request.ResourceType,
                ResourceId = request.ResourceId ?? string.Empty,
                AgentProfileId = request.AgentProfileId ?? string.Empty,
                RuntimeInstanceId = request.RuntimeInstanceId ?? string.Empty,
                ExecutionId = request.ExecutionId ?? string.Empty,
                Identity = request.Identity == null ? new AgentIdentityContext() : request.Identity.Clone()
            };
            context.Attributes["resource.scope"] = request.Scope.ToString();
            context.Attributes["resource.expectedOwnerId"] = expectedOwnerId;
            context.Attributes["resource.ownerId"] = request.ResourceOwnerId ?? string.Empty;
            foreach (var pair in request.Attributes ?? new Dictionary<string, string>())
                context.Attributes[pair.Key] = pair.Value;
            return context;
        }

        private static string BuildPolicyReason(AiPolicyDecision decision)
        {
            var reason = string.IsNullOrWhiteSpace(decision.Reason) ? "No policy reason was supplied." : decision.Reason;
            if (decision.RequiresApproval)
                return "Resource admission requires approval by policy. " + reason;
            if (decision.IsDeferred)
                return "Resource admission was deferred by policy. " + reason;
            return "Resource admission was denied by policy. " + reason;
        }
    }
}

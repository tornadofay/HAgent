using System;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ResourceGovernanceTests
    {
        [Fact]
        public void EffectiveCapability_RecordsProfileRuntimeAndDefaultSources()
        {
            var profile = new AiResourceCapabilityPolicy();
            profile.Set("knowledge", "kb-1", AiResourceCapabilityState.Disabled);
            profile.Set("skill", AiResourceCapabilityState.Disabled);

            var runtime = new AiResourceCapabilityPolicy();
            runtime.Set("skill", AiResourceCapabilityState.Enabled);

            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile, runtime);

            Assert.Equal(AiResourceCapabilityState.Disabled, snapshot.GetState("knowledge", "kb-1"));
            Assert.Equal(AiResourceCapabilitySource.Profile, snapshot.GetSource("knowledge", "kb-1"));
            Assert.Equal(AiResourceCapabilityState.Enabled, snapshot.GetState("skill", "skill-1"));
            Assert.Equal(AiResourceCapabilitySource.RuntimeOverride, snapshot.GetSource("skill", "skill-1"));
            Assert.Equal(AiResourceCapabilityState.Enabled, snapshot.GetState("memory", "memory-1"));
            Assert.Equal(AiResourceCapabilitySource.Default, snapshot.GetSource("memory", "memory-1"));
        }

        [Fact]
        public void Governance_DeniesDisabledResourceBeforePolicyEvaluation()
        {
            var policy = new AiPolicySet();
            policy.Rules.Add(Allow("resource.read", "knowledge", "kb-1"));
            var evaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(Disabled("knowledge", "kb-1")));

            var decision = evaluator.Evaluate(Request("resource.read", "knowledge", "kb-1"));

            Assert.False(decision.Allowed);
            Assert.Equal(AiResourceCapabilityState.Disabled, decision.ResourceCapabilityState);
            Assert.Equal(AiResourceCapabilitySource.Profile, decision.ResourceCapabilitySource);
            Assert.Null(decision.PolicyDecision);
        }

        [Fact]
        public void Governance_DeniesCrossOwnerResource()
        {
            var identity = new AgentIdentityContext("deployment-1", "tenant-1", userId: "user-1");
            var expected = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var evaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(new AiPolicySet()),
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));

            var request = Request("resource.read", "knowledge", "kb-1");
            request.Scope = AgentResourceScope.User;
            request.Identity = identity;
            request.ResourceOwnerId = expected + "-other";

            var decision = evaluator.Evaluate(request);

            Assert.False(decision.Allowed);
            Assert.False(decision.OwnershipMatches);
            Assert.Equal(expected, decision.ExpectedOwnerId);
        }

        [Fact]
        public void Governance_AllowsOnlyWhenCapabilityOwnershipAndPolicyAllAllow()
        {
            var identity = new AgentIdentityContext("deployment-1", "tenant-1", userId: "user-1");
            var owner = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);
            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-knowledge-read",
                Name = "Allow knowledge read",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "resource.read" },
                ResourceTypes = { "knowledge" },
                ResourceIds = { "kb-1" },
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-1"
            });

            var evaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));

            var request = Request("resource.read", "knowledge", "kb-1");
            request.Scope = AgentResourceScope.User;
            request.Identity = identity;
            request.ResourceOwnerId = owner;

            var decision = evaluator.Evaluate(request);

            Assert.True(decision.Allowed);
            Assert.Equal(AiPolicyOutcome.Allow, decision.PolicyDecision.Outcome);
            Assert.True(decision.OwnershipMatches);
            Assert.Equal(AiResourceCapabilitySource.Default, decision.ResourceCapabilitySource);
            decision.Validate();
        }

        [Fact]
        public void Governance_TreatsApprovalAndNoApplicablePolicyAsNonAdmitted()
        {
            var approvalPolicy = new AiPolicySet();
            approvalPolicy.Rules.Add(new AiPolicyRule
            {
                Id = "review-knowledge",
                Name = "Review knowledge",
                Outcome = AiPolicyOutcome.RequireApproval,
                Operations = { "resource.read" },
                ResourceTypes = { "knowledge" },
                ResourceIds = { "kb-1" }
            });

            var approvalEvaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(approvalPolicy),
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));
            var approval = approvalEvaluator.Evaluate(Request("resource.read", "knowledge", "kb-1"));

            Assert.False(approval.Allowed);
            Assert.True(approval.RequiresApproval);
            Assert.Equal(AiPolicyOutcome.RequireApproval, approval.PolicyDecision.Outcome);

            var noPolicyEvaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(new AiPolicySet()),
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));
            var noPolicy = noPolicyEvaluator.Evaluate(Request("resource.read", "knowledge", "kb-1"));

            Assert.False(noPolicy.Allowed);
            Assert.False(noPolicy.RequiresApproval);
            Assert.Equal(AiPolicyOutcome.NotApplicable, noPolicy.PolicyDecision.Outcome);
        }

        [Fact]
        public void EffectiveCapabilitySnapshot_RemainsDetachedFromSourcePolicies()
        {
            var profile = new AiResourceCapabilityPolicy();
            profile.Set("knowledge", "kb-1", AiResourceCapabilityState.Disabled);
            var runtime = new AiResourceCapabilityPolicy();
            var snapshot = AiResourceCapabilitySnapshot.Resolve(profile, runtime);

            profile.Set("knowledge", "kb-1", AiResourceCapabilityState.Enabled);
            runtime.Set("knowledge", "kb-1", AiResourceCapabilityState.Enabled);

            Assert.Equal(AiResourceCapabilityState.Disabled, snapshot.GetState("knowledge", "kb-1"));
            Assert.Equal(AiResourceCapabilitySource.Profile, snapshot.GetSource("knowledge", "kb-1"));
        }

        private static AiResourceGovernanceRequest Request(string operation, string type, string id)
        {
            return new AiResourceGovernanceRequest
            {
                Operation = operation,
                ResourceType = type,
                ResourceId = id,
                Identity = new AgentIdentityContext("deployment-1")
            };
        }

        private static AiResourceCapabilityPolicy Disabled(string type, string id)
        {
            var policy = new AiResourceCapabilityPolicy();
            policy.Set(type, id, AiResourceCapabilityState.Disabled);
            return policy;
        }

        private static AiPolicyRule Allow(string operation, string resourceType, string resourceId)
        {
            return new AiPolicyRule
            {
                Id = "allow-rule",
                Name = "Allow rule",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { operation },
                ResourceTypes = { resourceType },
                ResourceIds = { resourceId }
            };
        }
    }
}

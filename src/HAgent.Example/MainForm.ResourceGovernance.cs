using System;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddResourceGovernanceTab()
        {
            AddApiTab(
                "Resource Governance",
                "Run resource governance test",
                "Verifies the canonical resource admission boundary: identity-derived ownership, profile/runtime capability state, policy authorization, approval requirements, and fail-closed behavior.",
                "A resource should be admitted only when ownership, effective capability, and policy authorization all permit the operation. Disabled or cross-owner resources must be rejected before resource use.",
                "No provider request is sent. The scenario uses only provider-neutral HAgent.Core public contracts.",
                TestResourceGovernanceAsync,
                "Governance boundary",
                "Capability enablement and policy authorization are separate concerns. The governance decision composes them without granting authority to model output or resource content.");
        }

        private Task TestResourceGovernanceAsync(string unused)
        {
            var identity = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-42",
                userId: "user-42",
                workspaceId: "workspace-42");
            var ownerId = AgentResourceOwnership.GetOwnerId(AgentResourceScope.User, identity);

            var profileCapabilities = new AiResourceCapabilityPolicy();
            profileCapabilities.Set("knowledge", "knowledge-private-42", AiResourceCapabilityState.Disabled);
            profileCapabilities.Set("skill", AiResourceCapabilityState.Enabled);

            var runtimeOverrides = new AiResourceCapabilityPolicy();
            runtimeOverrides.Set("knowledge", "knowledge-private-42", AiResourceCapabilityState.Enabled);

            var effective = AiResourceCapabilitySnapshot.Resolve(profileCapabilities, runtimeOverrides);
            var policy = new AiPolicySet { Version = "resource-governance-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "allow-user-knowledge-read",
                Name = "Allow user knowledge read",
                Scope = AiPolicyScopeKind.User,
                ScopeId = "user-42",
                Outcome = AiPolicyOutcome.Allow,
                Operations = { "resource.read" },
                ResourceTypes = { "knowledge" },
                ResourceIds = { "knowledge-private-42" }
            });

            var evaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                effective);

            var allowedRequest = new AiResourceGovernanceRequest
            {
                Operation = "resource.read",
                ResourceType = "knowledge",
                ResourceId = "knowledge-private-42",
                Scope = AgentResourceScope.User,
                ResourceOwnerId = ownerId,
                AgentProfileId = "agent-42",
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                Identity = identity
            };
            var allowed = evaluator.Evaluate(allowedRequest);
            if (!allowed.Allowed || allowed.ResourceCapabilityState != AiResourceCapabilityState.Enabled ||
                allowed.ResourceCapabilitySource != AiResourceCapabilitySource.RuntimeOverride ||
                !allowed.OwnershipMatches || allowed.PolicyDecision == null ||
                allowed.PolicyDecision.Outcome != AiPolicyOutcome.Allow)
                throw new InvalidOperationException("Allowed resource governance decision was not composed correctly.");

            var wrongOwner = allowedRequest.Clone();
            wrongOwner.ResourceOwnerId = ownerId + "-other";
            var deniedOwner = evaluator.Evaluate(wrongOwner);
            if (deniedOwner.Allowed || deniedOwner.OwnershipMatches || deniedOwner.PolicyDecision != null)
                throw new InvalidOperationException("Cross-owner resource access was not rejected before policy evaluation.");

            var approvalPolicy = new AiPolicySet { Version = "resource-governance-approval-42" };
            approvalPolicy.Rules.Add(new AiPolicyRule
            {
                Id = "review-skill-read",
                Name = "Review skill read",
                Outcome = AiPolicyOutcome.RequireApproval,
                Operations = { "resource.read" },
                ResourceTypes = { "skill" },
                ResourceIds = { "skill-42" }
            });
            var approvalEvaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(approvalPolicy),
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));
            var approvalRequest = new AiResourceGovernanceRequest
            {
                Operation = "resource.read",
                ResourceType = "skill",
                ResourceId = "skill-42",
                Scope = AgentResourceScope.User,
                ResourceOwnerId = ownerId,
                Identity = identity
            };
            var approval = approvalEvaluator.Evaluate(approvalRequest);
            if (approval.Allowed || !approval.RequiresApproval || approval.PolicyDecision == null ||
                approval.PolicyDecision.Outcome != AiPolicyOutcome.RequireApproval)
                throw new InvalidOperationException("Policy approval requirement was not preserved as a non-admitted decision.");

            var disabledProfile = new AiResourceCapabilityPolicy();
            disabledProfile.Set("knowledge", "knowledge-disabled-42", AiResourceCapabilityState.Disabled);
            var disabledEvaluator = new AiResourceGovernanceEvaluator(
                new DefaultAiPolicyEngine(policy),
                AiResourceCapabilitySnapshot.Resolve(disabledProfile));
            var disabledRequest = allowedRequest.Clone();
            disabledRequest.ResourceId = "knowledge-disabled-42";
            disabledRequest.ResourceOwnerId = ownerId;
            var disabled = disabledEvaluator.Evaluate(disabledRequest);
            if (disabled.Allowed || disabled.ResourceCapabilityState != AiResourceCapabilityState.Disabled || disabled.PolicyDecision != null)
                throw new InvalidOperationException("Disabled capability was not enforced before policy authorization.");

            Write(
                "RESOURCE GOVERNANCE",
                "Resource governance succeeded." + Environment.NewLine +
                "Identity-derived owner: verified." + Environment.NewLine +
                "Profile capability overridden at runtime: verified." + Environment.NewLine +
                "Effective capability source: RuntimeOverride." + Environment.NewLine +
                "Policy authorization: Allow." + Environment.NewLine +
                "Cross-owner access: denied before policy evaluation." + Environment.NewLine +
                "Approval requirement: preserved as non-admitted." + Environment.NewLine +
                "Disabled resource: rejected before policy evaluation." + Environment.NewLine +
                "Authoritative resource mutation: none.");

            return Task.CompletedTask;
        }
    }
}

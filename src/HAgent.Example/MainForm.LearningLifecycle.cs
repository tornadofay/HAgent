using System;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningLifecycleTab()
        {
            AddApiTab(
                "Learning Lifecycle",
                "Run governed learning lifecycle",
                "Verifies the canonical candidate gate: learning policy, Learning Mode, unified promotion authorization, review routing, and terminal admission state.",
                "Candidates are proposed first. The lifecycle gate may reject, route to review, or approve a candidate; it never publishes Knowledge or Skills itself.",
                "Uses deterministic in-memory policies only; no model, network provider, or resource repository is required.",
                TestLearningLifecycleAsync,
                "Learning lifecycle",
                "Promotion is a later resource-write step. This example verifies only the governed candidate lifecycle boundary.");
        }

        private Task TestLearningLifecycleAsync(string unused)
        {
            var learningPolicy = new AiLearningPolicy
            {
                Id = "example-learning-policy",
                Version = 2
            };
            learningPolicy.Rules.Add(new AiLearningPolicyRule
            {
                Id = "example-skill-rule",
                CandidateType = AiLearningCandidateType.Skill,
                AllowedScopes = { "Agent" },
                Priority = 100,
                MinimumConfidence = 0.85m,
                EvidenceRequirement = AiLearningEvidenceRequirement.Required,
                ProvenanceRequirement = AiLearningProvenanceRequirement.Required,
                ContradictionRequirement = AiLearningContradictionRequirement.RequireClear,
                EvaluationRequirement = AiLearningEvaluationRequirement.RequiredPassed,
                RetentionClass = "Standard",
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired
            });

            var authorizationSet = new AiPolicySet { Version = "example-auth-policy" };
            var authorizationRule = new AiPolicyRule
            {
                Id = "example-learning-auth",
                Name = "Learning promotion authorization",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "learning-agent-example",
                Priority = 100,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "Deterministic Example authorization."
            };
            authorizationRule.Operations.Add("learning.promote");
            authorizationRule.ResourceTypes.Add("learning-candidate");
            authorizationRule.Attributes["candidateType"] = "Skill";
            authorizationRule.Attributes["proposedScope"] = "Agent";
            authorizationRule.Attributes["confidenceBand"] = "High";
            authorizationRule.Attributes["evidenceState"] = "Strong";
            authorizationRule.Attributes["provenanceState"] = "Complete";
            authorizationRule.Attributes["contradictionState"] = "None";
            authorizationRule.Attributes["retentionClass"] = "Standard";
            authorizationSet.Rules.Add(authorizationRule);

            var authorization = new DefaultAiPolicyEngine(authorizationSet);
            var approved = CreateSkillCandidate("approved-candidate");
            var approvedDecision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                approved,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                new AgentIdentityContext(tenantId: "tenant-example", userId: "user-example", workspaceId: "workspace-example"));

            if (approved.Status != AiLearningCandidateStatus.Approved || !approvedDecision.CanProceedToPromotion)
                throw new InvalidOperationException("AutomaticWithPolicy candidate was not admitted to Approved.");

            var review = CreateSkillCandidate("review-candidate");
            var reviewDecision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                review,
                learningPolicy,
                AiLearningMode.SuggestOnly,
                authorization,
                new AgentIdentityContext(tenantId: "tenant-example", userId: "user-example", workspaceId: "workspace-example"));

            if (review.Status != AiLearningCandidateStatus.PendingReview || !reviewDecision.RequiresReview)
                throw new InvalidOperationException("SuggestOnly candidate was not routed to PendingReview.");

            var deniedSet = authorizationSet.Clone();
            deniedSet.Rules[0].Outcome = AiPolicyOutcome.Deny;
            deniedSet.Rules[0].Reason = "Deterministic denial for Example.";
            var deniedCandidate = CreateSkillCandidate("denied-candidate");
            var deniedDecision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                deniedCandidate,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                new DefaultAiPolicyEngine(deniedSet),
                new AgentIdentityContext(tenantId: "tenant-example", userId: "user-example", workspaceId: "workspace-example"));

            if (deniedCandidate.Status != AiLearningCandidateStatus.Rejected || deniedDecision.CanProceedToPromotion)
                throw new InvalidOperationException("Unified policy denial did not reject the candidate.");

            var notEvaluated = CreateSkillCandidate("not-evaluated-candidate");
            notEvaluated.EvaluationState = "Failed";
            var notEvaluatedDecision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                notEvaluated,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                new AgentIdentityContext(tenantId: "tenant-example", userId: "user-example", workspaceId: "workspace-example"));

            if (notEvaluated.Status != AiLearningCandidateStatus.Rejected || notEvaluatedDecision.CanProceedToPromotion)
                throw new InvalidOperationException("Failed evaluation bypassed the learning policy gate.");

            var disabled = CreateSkillCandidate("disabled-candidate");
            var disabledDecision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                disabled,
                learningPolicy,
                AiLearningMode.Disabled,
                authorization,
                new AgentIdentityContext(tenantId: "tenant-example", userId: "user-example", workspaceId: "workspace-example"));

            if (disabled.Status != AiLearningCandidateStatus.Rejected || disabledDecision.CanProceedToPromotion)
                throw new InvalidOperationException("Disabled Learning Mode still admitted a candidate.");

            Write(
                "LEARNING LIFECYCLE",
                "Contract test succeeded." + Environment.NewLine +
                "AutomaticWithPolicy -> Approved: verified." + Environment.NewLine +
                "Approved candidate marked eligible for later promotion: verified." + Environment.NewLine +
                "SuggestOnly -> PendingReview: verified." + Environment.NewLine +
                "Unified policy denial -> Rejected: verified." + Environment.NewLine +
                "Evaluation failure blocks admission: verified." + Environment.NewLine +
                "Disabled Learning Mode -> Rejected: verified." + Environment.NewLine +
                "Authoritative resource publication is not performed by this lifecycle gate: verified.");

            return Task.CompletedTask;
        }

        private static SkillCandidate CreateSkillCandidate(string id)
        {
            return new SkillCandidate(
                new AiSkillDefinition
                {
                    Id = "example-skill-" + id,
                    Version = 1,
                    Scope = AgentResourceScope.Agent,
                    OwnerId = "learning-agent-example",
                    Name = "Example learned skill",
                    Description = "Draft candidate skill.",
                    Status = AiSkillLifecycleStatus.Draft,
                    Provenance = new AiSkillProvenance
                    {
                        Source = "Learning Lifecycle Example",
                        SourceExecutionId = "example-execution-1",
                        SourceRuntimeInstanceId = "example-runtime-1",
                        Evidence = "Deterministic Example evidence.",
                        Confidence = 0.95m
                    }
                },
                new AiLearningCandidate
                {
                    Id = id,
                    Type = AiLearningCandidateType.Skill,
                    ProposedScope = "Agent",
                    Provenance = "Complete",
                    Evidence = "Strong",
                    SourceExecutionId = "example-execution-1",
                    SourceRuntimeInstanceId = "example-runtime-1",
                    SourceAgentProfileId = "learning-agent-example"
                })
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };
        }
    }
}

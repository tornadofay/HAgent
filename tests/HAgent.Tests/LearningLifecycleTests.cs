using System;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class LearningLifecycleTests
    {
        [Fact]
        public void AutomaticWithPolicyMovesCandidateToApproved()
        {
            var candidate = CreateSkillCandidate();
            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Allow);

            var decision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                candidate,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                Identity());

            Assert.Equal(AiLearningCandidateStatus.Approved, candidate.Status);
            Assert.Equal(AiLearningCandidateStatus.Approved, decision.TargetStatus);
            Assert.True(decision.CanProceedToPromotion);
            Assert.False(decision.RequiresReview);
            Assert.Equal("learning-rule-1", decision.RuleId);
            Assert.Equal(1, decision.PolicyVersion);
        }

        [Fact]
        public void SuggestOnlyAlwaysRequiresReviewAfterLearningPolicyPasses()
        {
            var candidate = CreateSkillCandidate();
            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Allow);

            var decision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                candidate,
                learningPolicy,
                AiLearningMode.SuggestOnly,
                authorization,
                Identity());

            Assert.Equal(AiLearningCandidateStatus.PendingReview, candidate.Status);
            Assert.Equal(AiLearningCandidateStatus.PendingReview, decision.TargetStatus);
            Assert.True(decision.RequiresReview);
            Assert.False(decision.CanProceedToPromotion);
        }

        [Fact]
        public void UnifiedPolicyDenialRejectsCandidate()
        {
            var candidate = CreateSkillCandidate();
            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Deny);

            var decision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                candidate,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                Identity());

            Assert.Equal(AiLearningCandidateStatus.Rejected, candidate.Status);
            Assert.Equal(AiPolicyOutcome.Deny, decision.AuthorizationDecision.Outcome);
            Assert.False(decision.CanProceedToPromotion);
        }

        [Fact]
        public void LearningPolicyFailureRejectsBeforeUnifiedAuthorization()
        {
            var candidate = CreateSkillCandidate();
            candidate.EvaluationState = "Failed";
            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Allow);

            var decision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                candidate,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                Identity());

            Assert.Equal(AiLearningCandidateStatus.Rejected, candidate.Status);
            Assert.False(decision.CanProceedToPromotion);
            Assert.Contains("evaluation", decision.Reason, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(AiPolicyOutcome.Deny, decision.AuthorizationDecision.Outcome);
        }

        [Fact]
        public void DisabledLearningRejectsCandidateWithoutPromotion()
        {
            var candidate = CreateSkillCandidate();
            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Allow);

            var decision = AiLearningLifecycleCoordinator.EvaluateAndApply(
                candidate,
                learningPolicy,
                AiLearningMode.Disabled,
                authorization,
                Identity());

            Assert.Equal(AiLearningCandidateStatus.Rejected, candidate.Status);
            Assert.False(decision.CanProceedToPromotion);
            Assert.Equal(AiLearningPromotionAuthorization.NotPermitted, decision.PromotionAuthorization);
        }

        [Fact]
        public void LifecycleGateRequiresProposedCandidate()
        {
            var candidate = CreateSkillCandidate();
            candidate.Lifecycle.ApplyPolicyDecision(new AiPolicyDecision
            {
                Outcome = AiPolicyOutcome.Allow,
                PolicyVersion = "test",
                RuleId = "seed"
            });

            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Allow);

            Assert.Throws<InvalidOperationException>(() => AiLearningLifecycleCoordinator.Evaluate(
                candidate,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                Identity()));
        }

        [Fact]
        public void LifecycleGateRequiresIdentityForAuthorizationBoundary()
        {
            var candidate = CreateSkillCandidate();
            var learningPolicy = CreateLearningPolicy(AiLearningPromotionAuthorization.UnifiedPolicyRequired);
            var authorization = CreateAuthorizationEngine(AiPolicyOutcome.Allow);

            Assert.Throws<ArgumentNullException>(() => AiLearningLifecycleCoordinator.Evaluate(
                candidate,
                learningPolicy,
                AiLearningMode.AutomaticWithPolicy,
                authorization,
                null));

            Assert.Equal(AiLearningCandidateStatus.Proposed, candidate.Status);
        }

        private static SkillCandidate CreateSkillCandidate()
        {
            var skill = new AiSkillDefinition
            {
                Id = "learned-skill-1",
                Version = 1,
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-42",
                Name = "Learned skill",
                Description = "Candidate skill",
                Status = AiSkillLifecycleStatus.Draft,
                Provenance = new AiSkillProvenance
                {
                    Source = "test",
                    SourceExecutionId = "execution-1",
                    SourceRuntimeInstanceId = "runtime-1",
                    Evidence = "test evidence",
                    Confidence = 0.95m
                }
            };

            return new SkillCandidate(
                skill,
                new AiLearningCandidate
                {
                    Id = "candidate-1",
                    Type = AiLearningCandidateType.Skill,
                    ProposedScope = "Agent",
                    Provenance = "Complete",
                    Evidence = "Strong",
                    SourceExecutionId = "execution-1",
                    SourceRuntimeInstanceId = "runtime-1",
                    SourceAgentProfileId = "agent-42"
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

        private static AiLearningPolicy CreateLearningPolicy(AiLearningPromotionAuthorization authorization)
        {
            var policy = new AiLearningPolicy
            {
                Id = "learning-policy-1",
                Version = 1
            };
            policy.Rules.Add(new AiLearningPolicyRule
            {
                Id = "learning-rule-1",
                CandidateType = AiLearningCandidateType.Skill,
                Priority = 10,
                MinimumConfidence = 0.80m,
                EvidenceRequirement = AiLearningEvidenceRequirement.Required,
                ProvenanceRequirement = AiLearningProvenanceRequirement.Required,
                ContradictionRequirement = AiLearningContradictionRequirement.RequireClear,
                EvaluationRequirement = AiLearningEvaluationRequirement.RequiredPassed,
                RetentionClass = "Standard",
                PromotionAuthorization = authorization,
                AllowedScopes = { "Agent" }
            });
            return policy;
        }

        private static IAiPolicyEngine CreateAuthorizationEngine(AiPolicyOutcome outcome)
        {
            var set = new AiPolicySet { Version = "auth-1" };
            var rule = new AiPolicyRule
            {
                Id = "learning-auth",
                Name = "learning auth",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "agent-42",
                Priority = 10,
                Outcome = outcome,
                Reason = "test"
            };
            rule.Operations.Add("learning.promote");
            rule.ResourceTypes.Add("learning-candidate");
            rule.Attributes["candidateType"] = "Skill";
            rule.Attributes["proposedScope"] = "Agent";
            rule.Attributes["confidenceBand"] = "High";
            rule.Attributes["evidenceState"] = "Strong";
            rule.Attributes["provenanceState"] = "Complete";
            rule.Attributes["contradictionState"] = "None";
            rule.Attributes["retentionClass"] = "Standard";
            set.Rules.Add(rule);
            return new DefaultAiPolicyEngine(set);
        }

        private static AgentIdentityContext Identity()
        {
            return new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42", workspaceId: "workspace-42");
        }
    }
}

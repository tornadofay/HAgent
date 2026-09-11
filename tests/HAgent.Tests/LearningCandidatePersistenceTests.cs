using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class LearningCandidatePersistenceTests
    {
        [Fact]
        public void CaptureAndRestorePreservesTypedCandidateAndLifecycleRevision()
        {
            var candidate = CreatePendingSkillCandidate();
            var admission = new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = "learning-policy",
                PolicyVersion = 1,
                RuleId = "learning-rule",
                LearningMode = AiLearningMode.SuggestOnly,
                TargetStatus = AiLearningCandidateStatus.PendingReview,
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                AuthorizationDecision = new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval, PolicyVersion = "1", RuleId = "learning-rule" },
                RequiresReview = true,
                CanProceedToPromotion = false,
                Reason = "review"
            };
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = "Standard", RetentionDays = 30 });

            var record = AiLearningCandidatePersistence.Capture(candidate, admission, retention, DateTimeOffset.UtcNow);
            var restored = AiLearningCandidatePersistence.Restore(record);

            Assert.Equal(candidate.Id, record.CandidateId);
            Assert.Equal(AiLearningCandidateStatus.PendingReview, record.Status);
            Assert.Equal(1, record.Revision);
            Assert.Equal(AiLearningCandidateStatus.PendingReview, restored.Status);
            Assert.IsType<SkillCandidate>(restored);
            Assert.Equal(((SkillCandidate)candidate).Skill.Id, ((SkillCandidate)restored).Skill.Id);
        }

        [Fact]
        public async Task StoreRejectsStaleReviewUpdate()
        {
            var candidate = CreatePendingSkillCandidate();
            var admission = PendingDecision(candidate);
            var retention = StandardRetention();
            var store = new InMemoryAiLearningCandidateStore();
            var record = AiLearningCandidatePersistence.Capture(candidate, admission, retention, DateTimeOffset.UtcNow);
            await store.SaveAsync(record);

            var current = await store.GetAsync(record.CandidateId);
            var approved = AiLearningCandidatePersistence.ApplyReview(
                current,
                AiLearningCandidateReviewAction.Approve,
                Identity(),
                AllowReviewDecision(),
                "approved",
                DateTimeOffset.UtcNow);
            await store.TryUpdateAsync(approved, current.Revision);

            var stale = AiLearningCandidatePersistence.ApplyReview(
                current,
                AiLearningCandidateReviewAction.Reject,
                Identity(),
                AllowReviewDecision(),
                "stale",
                DateTimeOffset.UtcNow);

            await Assert.ThrowsAsync<InvalidOperationException>(() => store.TryUpdateAsync(stale, current.Revision));
            var persisted = await store.GetAsync(record.CandidateId);
            Assert.Equal(AiLearningCandidateStatus.Approved, persisted.Status);
            Assert.Equal(2, persisted.Revision);
        }

        [Fact]
        public async Task ReviewServiceRequiresAuthorizationAndRecordsEvidence()
        {
            var candidate = CreatePendingSkillCandidate();
            var record = AiLearningCandidatePersistence.Capture(candidate, PendingDecision(candidate), StandardRetention(), DateTimeOffset.UtcNow);
            var store = new InMemoryAiLearningCandidateStore();
            await store.SaveAsync(record);

            var deniedSet = new AiPolicySet { Version = "review-1" };
            deniedSet.Rules.Add(CreateReviewRule(AiPolicyOutcome.Deny));
            var deniedService = new AiLearningCandidateReviewService(store, new DefaultAiPolicyEngine(deniedSet));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => deniedService.ReviewAsync(record.CandidateId, AiLearningCandidateReviewAction.Approve, Identity(), "no"));

            var allowedSet = new AiPolicySet { Version = "review-2" };
            allowedSet.Rules.Add(CreateReviewRule(AiPolicyOutcome.Allow));
            var allowedService = new AiLearningCandidateReviewService(store, new DefaultAiPolicyEngine(allowedSet));
            var updated = await allowedService.ReviewAsync(record.CandidateId, AiLearningCandidateReviewAction.Approve, Identity(), "operator-approved");

            Assert.Equal(AiLearningCandidateStatus.Approved, updated.Status);
            Assert.Equal(2, updated.Revision);
            Assert.Equal("Approve", updated.LastReviewAction);
            Assert.Equal("Allow", updated.LastReviewOutcome);
            Assert.Equal("operator-approved", updated.LastReviewReason);
            Assert.False(string.IsNullOrWhiteSpace(updated.LastReviewerIdentityJson));
        }

        [Fact]
        public async Task RetentionExpiryIsDeterministicAndPurgeable()
        {
            var candidate = CreatePendingSkillCandidate();
            var capturedAt = DateTimeOffset.UtcNow.AddDays(-2);
            var admission = PendingDecision(candidate);
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = "Immediate", RetentionDays = 1 });
            var record = AiLearningCandidatePersistence.Capture(candidate, admission, retention, capturedAt);
            var store = new InMemoryAiLearningCandidateStore();
            await store.SaveAsync(record);

            var visible = await store.GetAsync(record.CandidateId);
            Assert.Null(visible);
            Assert.Equal(1, await store.PurgeExpiredAsync(DateTimeOffset.UtcNow));
        }

        private static SkillCandidate CreatePendingSkillCandidate()
        {
            var lifecycle = new AiLearningCandidate
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = AiLearningCandidateType.Skill,
                ProposedScope = "Agent",
                Provenance = "Complete",
                Evidence = "Strong",
                SourceExecutionId = "execution-1",
                SourceRuntimeInstanceId = "runtime-1",
                SourceAgentProfileId = "agent-42"
            };
            var candidate = new SkillCandidate(
                new AiSkillDefinition
                {
                    Id = "persisted-skill",
                    Version = 1,
                    Scope = AgentResourceScope.Agent,
                    OwnerId = "agent-42",
                    Name = "Persisted skill",
                    Description = "Pending candidate",
                    Status = AiSkillLifecycleStatus.Draft,
                    Provenance = new AiSkillProvenance { Source = "test", Evidence = "evidence", Confidence = 0.95m }
                },
                lifecycle)
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };
            lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval, PolicyVersion = "1", RuleId = "learning-rule" });
            return candidate;
        }

        private static AiLearningLifecycleDecision PendingDecision(SkillCandidate candidate)
        {
            return new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = "learning-policy",
                PolicyVersion = 1,
                RuleId = "learning-rule",
                LearningMode = AiLearningMode.SuggestOnly,
                TargetStatus = AiLearningCandidateStatus.PendingReview,
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                AuthorizationDecision = new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval, PolicyVersion = "1", RuleId = "learning-rule" },
                RequiresReview = true,
                Reason = "review"
            };
        }

        private static AiLearningCandidateRetentionPolicy StandardRetention()
        {
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = "Standard", RetentionDays = 30 });
            return retention;
        }

        private static AgentIdentityContext Identity()
        {
            return new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42", workspaceId: "workspace-42");
        }

        private static AiPolicyRule CreateReviewRule(AiPolicyOutcome outcome)
        {
            var rule = new AiPolicyRule
            {
                Id = "learning-review",
                Name = "Learning review authorization",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "agent-42",
                Priority = 100,
                Outcome = outcome,
                Reason = outcome == AiPolicyOutcome.Allow ? "review allowed" : "review denied"
            };
            rule.Operations.Add("learning.review");
            rule.ResourceTypes.Add("learning-candidate");
            rule.ResourceIds.Add("*");
            rule.Attributes["candidateType"] = "Skill";
            rule.Attributes["proposedScope"] = "Agent";
            rule.Attributes["currentStatus"] = "PendingReview";
            rule.Attributes["reviewAction"] = "Approve";
            return rule;
        }

        private static AiPolicyDecision AllowReviewDecision()
        {
            return new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "review", RuleId = "learning-review", Reason = "allowed" };
        }
    }
}

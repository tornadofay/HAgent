using System;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearningCandidatePersistenceTests
    {
        [Fact]
        public void CaptureAndRestorePreservesTypedCandidateAndLifecycleRevision()
        {
            var candidate = CreatePendingSkillCandidate();
            var capturedAt = DateTimeOffset.UtcNow;
            var record = AiLearningCandidatePersistence.Capture(candidate, PendingDecision(candidate), StandardRetention(), capturedAt);

            var restored = AiLearningCandidatePersistence.Restore(record);

            Assert.Equal(candidate.Id, restored.Id);
            Assert.Equal(AiLearningCandidateType.Skill, restored.Type);
            Assert.Equal(AiLearningCandidateStatus.PendingReview, restored.Status);
            Assert.Equal(record.Revision, restored.Lifecycle.Revision);
            Assert.Equal(candidate.ProposedScope, restored.ProposedScope);
            Assert.Equal(candidate.RetentionClass, restored.RetentionClass);
        }

        [Fact]
        public async Task FileStorePersistsAcrossInstancesAndFailsClosedOnCorruptRecord()
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "hagent-learning-candidate-" + Guid.NewGuid().ToString("N"));
            var path = System.IO.Path.Combine(root, "candidates.jsonl");
            try
            {
                var candidate = CreatePendingSkillCandidate();
                var record = AiLearningCandidatePersistence.Capture(candidate, PendingDecision(candidate), StandardRetention(), DateTimeOffset.UtcNow);

                using (var first = new HAgent.Storage.File.FileLearningCandidateStore(path))
                {
                    await first.SaveAsync(record);
                }

                using (var second = new HAgent.Storage.File.FileLearningCandidateStore(path))
                {
                    var recovered = await second.GetAsync(record.CandidateId);
                    Assert.NotNull(recovered);
                    Assert.Equal(record.CandidateId, recovered.CandidateId);
                    Assert.Equal(record.PayloadJson, recovered.PayloadJson);
                }

                await System.IO.File.AppendAllTextAsync(path, "{not-json}\n");
                using (var corrupt = new HAgent.Storage.File.FileLearningCandidateStore(path))
                {
                    await Assert.ThrowsAsync<System.IO.InvalidDataException>(() => corrupt.GetAsync(record.CandidateId));
                }
            }
            finally
            {
                if (System.IO.Directory.Exists(root))
                    System.IO.Directory.Delete(root, true);
            }
        }

        [Fact]
        public async Task StoreRejectsStaleReviewUpdate()
        {
            var candidate = CreatePendingSkillCandidate();
            var record = AiLearningCandidatePersistence.Capture(candidate, PendingDecision(candidate), StandardRetention(), DateTimeOffset.UtcNow);
            var store = new InMemoryAiLearningCandidateStore();
            await store.SaveAsync(record);

            var approved = AiLearningCandidatePersistence.ApplyReview(
                record,
                AiLearningCandidateReviewAction.Approve,
                Identity(),
                AllowReviewDecision(),
                "approved",
                DateTimeOffset.UtcNow);

            await store.TryUpdateAsync(approved, record.Revision);
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.TryUpdateAsync(approved, record.Revision));
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
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => deniedService.ReviewAsync(record.CandidateId, AiLearningCandidateReviewAction.Approve, Identity(), "operator-denied"));

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
        public void ZeroDayRetentionCreatesImmediatelyExpiredCandidate()
        {
            var candidate = CreatePendingSkillCandidate();
            var capturedAt = DateTimeOffset.UtcNow;
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = "Standard", RetentionDays = 0 });

            var record = AiLearningCandidatePersistence.Capture(candidate, PendingDecision(candidate), retention, capturedAt);

            Assert.Equal(capturedAt, record.ExpiresAt);
            Assert.True(record.IsExpired(capturedAt));
        }

        [Fact]
        public async Task RetentionExpiryIsDeterministicAndPurgeable()
        {
            var candidate = CreatePendingSkillCandidate();
            var capturedAt = DateTimeOffset.UtcNow.AddDays(-2);
            var admission = PendingDecision(candidate);
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = "Standard", RetentionDays = 1 });
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
            return new AgentIdentityContext { AgentProfileId = "agent-42", AgentInstanceId = "instance-1", TenantId = "tenant-1" };
        }

        private static AiPolicyDecision AllowReviewDecision()
        {
            return new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "review-2", RuleId = "review-rule", Reason = "allowed" };
        }

        private static AiPolicyRule CreateReviewRule(AiPolicyOutcome outcome)
        {
            var rule = new AiPolicyRule
            {
                Id = "review-rule",
                Effect = outcome,
                Operation = "learning.review",
                ResourceType = "learning-candidate",
                AgentId = "agent-42"
            };
            rule.ResourceIds.Add("candidate");
            rule.Attributes["candidateType"] = "Skill";
            rule.Attributes["proposedScope"] = "Agent";
            rule.Attributes["currentStatus"] = "PendingReview";
            rule.Attributes["reviewAction"] = "Approve";
            return rule;
        }
    }
}

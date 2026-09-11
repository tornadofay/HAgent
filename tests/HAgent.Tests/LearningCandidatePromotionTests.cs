using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearningCandidatePromotionTests
    {
        [Fact]
        public async Task PromotesMemoryThroughExistingStoreAndMarksCandidatePromoted()
        {
            var candidate = CreateMemoryCandidate();
            var candidateStore = new InMemoryAiLearningCandidateStore();
            var memoryStore = new InMemoryMemoryStore();
            var record = CaptureApproved(candidate);
            await candidateStore.SaveAsync(record);

            var service = CreateService(candidateStore, memoryStore, new TestKnowledgeTarget(), new TestSkillTarget(), CreateAllowEngine(candidate.Id));
            var result = await service.PromoteAsync(candidate.Id, Identity());

            Assert.Equal(AiLearningCandidateStatus.Promoted, result.CandidateStatus);
            Assert.Equal(2, result.CandidateRevision);
            Assert.Equal("memory", result.AuthoritativeResourceType);
            Assert.NotEqual(candidate.Memory.Id, result.AuthoritativeResourceId);

            var memories = await memoryStore.SearchAsync(new MemoryQuery { MaxResults = 10, OwnerId = candidate.Memory.OwnerId });
            Assert.Single(memories);
            Assert.Equal(candidate.Id, memories[0].Metadata["learning.candidateId"]);
        }

        [Fact]
        public async Task PromotesKnowledgeAsPublishedVersionWithoutMutatingCandidatePayload()
        {
            var candidate = CreateKnowledgeCandidate(1);
            var candidateStore = new InMemoryAiLearningCandidateStore();
            await candidateStore.SaveAsync(CaptureApproved(candidate));
            var target = new TestKnowledgeTarget();
            var service = CreateService(candidateStore, new InMemoryMemoryStore(), target, new TestSkillTarget(), CreateAllowEngine(candidate.Id));

            var result = await service.PromoteAsync(candidate.Id, Identity());
            var published = await target.GetAsync(candidate.Knowledge.Id, CancellationToken.None);

            Assert.Equal("knowledge", result.AuthoritativeResourceType);
            Assert.Equal(1, result.AuthoritativeResourceVersion);
            Assert.Equal(AiKnowledgeLifecycleStatus.Published, published.Status);
            Assert.Equal(1, published.Version);
            Assert.Equal(AiKnowledgeLifecycleStatus.Draft, candidate.Knowledge.Status);
        }

        [Fact]
        public async Task RejectsStaleKnowledgeVersionWithoutPromotingCandidate()
        {
            var candidate = CreateKnowledgeCandidate(1);
            var candidateStore = new InMemoryAiLearningCandidateStore();
            await candidateStore.SaveAsync(CaptureApproved(candidate));
            var target = new TestKnowledgeTarget();
            await target.PublishAsync(CreatePublishedKnowledge(candidate.Knowledge.Id, 2), PromotionContext(candidate.Id), CancellationToken.None);
            var service = CreateService(candidateStore, new InMemoryMemoryStore(), target, new TestSkillTarget(), CreateAllowEngine(candidate.Id));

            await Assert.ThrowsAsync<AiLearningPromotionConflictException>(() => service.PromoteAsync(candidate.Id, Identity()));
            var persisted = await candidateStore.GetAsync(candidate.Id);
            Assert.Equal(AiLearningCandidateStatus.Approved, persisted.Status);
            Assert.Equal(1, persisted.Revision);
        }

        [Fact]
        public async Task RejectsStaleSkillVersionWithoutPromotingCandidate()
        {
            var candidate = CreateSkillCandidate(1);
            var candidateStore = new InMemoryAiLearningCandidateStore();
            await candidateStore.SaveAsync(CaptureApproved(candidate));
            var target = new TestSkillTarget();
            await target.PublishAsync(CreatePublishedSkill(candidate.Skill.Id, 2), PromotionContext(candidate.Id), CancellationToken.None);
            var service = CreateService(candidateStore, new InMemoryMemoryStore(), new TestKnowledgeTarget(), target, CreateAllowEngine(candidate.Id));

            await Assert.ThrowsAsync<AiLearningPromotionConflictException>(() => service.PromoteAsync(candidate.Id, Identity()));
            var persisted = await candidateStore.GetAsync(candidate.Id);
            Assert.Equal(AiLearningCandidateStatus.Approved, persisted.Status);
            Assert.Equal(1, persisted.Revision);
        }

        [Fact]
        public async Task DeniedPromotionPublishesNothing()
        {
            var candidate = CreateMemoryCandidate();
            var candidateStore = new InMemoryAiLearningCandidateStore();
            var memoryStore = new InMemoryMemoryStore();
            await candidateStore.SaveAsync(CaptureApproved(candidate));
            var service = CreateService(candidateStore, memoryStore, new TestKnowledgeTarget(), new TestSkillTarget(), CreateDenyEngine(candidate.Id));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.PromoteAsync(candidate.Id, Identity()));
            var memories = await memoryStore.SearchAsync(new MemoryQuery { MaxResults = 10, OwnerId = candidate.Memory.OwnerId });
            Assert.Empty(memories);
            var persisted = await candidateStore.GetAsync(candidate.Id);
            Assert.Equal(AiLearningCandidateStatus.Approved, persisted.Status);
            Assert.Equal(1, persisted.Revision);
        }

        private static AiLearningPromotionService CreateService(
            IAiLearningCandidateStore candidates,
            IMemoryStore memory,
            IAiKnowledgePromotionTarget knowledge,
            IAiSkillPromotionTarget skill,
            IAiPolicyEngine policy)
        {
            return new AiLearningPromotionService(candidates, memory, knowledge, skill, policy);
        }

        private static AiLearningCandidateRecord CaptureApproved(AiLearningTypedCandidate candidate)
        {
            var admission = new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = "learning-policy",
                PolicyVersion = 1,
                RuleId = "learning-rule",
                LearningMode = AiLearningMode.AutomaticWithPolicy,
                TargetStatus = AiLearningCandidateStatus.Approved,
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                AuthorizationDecision = new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "1", RuleId = "learning-rule" },
                RequiresReview = false,
                CanProceedToPromotion = true,
                Reason = "approved"
            };
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = candidate.RetentionClass, RetentionDays = 30 });
            return AiLearningCandidatePersistence.Capture(candidate, admission, retention, DateTimeOffset.UtcNow);
        }

        private static MemoryCandidate CreateMemoryCandidate()
        {
            var lifecycle = NewLifecycle(AiLearningCandidateType.Memory, "Agent");
            var memory = new MemoryEntry
            {
                Id = "candidate-memory",
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Fact,
                Family = AiMemoryFamily.Semantic,
                TypeId = "semantic.fact",
                OwnerId = "agent-42",
                Content = "Promote this fact",
                Provenance = new AiMemoryProvenance { Kind = AiMemoryProvenanceKind.ModelGenerated, Source = "test", Evidence = "evidence", Confidence = 0.95m },
                CreatedAt = DateTimeOffset.UtcNow,
                OccurredAt = DateTimeOffset.UtcNow
            };
            return new MemoryCandidate(memory, lifecycle)
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "Clear",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };
        }

        private static KnowledgeCandidate CreateKnowledgeCandidate(long version)
        {
            var lifecycle = NewLifecycle(AiLearningCandidateType.Knowledge, "Agent");
            var resource = new AiKnowledgeResource
            {
                Id = "learned-knowledge",
                Kind = AiKnowledgeResourceKind.Knowledge,
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-42",
                Title = "Learned knowledge",
                Summary = "Candidate summary",
                Content = "Candidate content",
                Status = AiKnowledgeLifecycleStatus.Draft,
                Version = version,
                Source = "test",
                Provenance = new AiKnowledgeProvenance { Kind = AiKnowledgeProvenanceKind.ModelGenerated, Source = "test", Evidence = "evidence", Confidence = 0.95m },
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            return new KnowledgeCandidate(resource, lifecycle)
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "Clear",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };
        }

        private static SkillCandidate CreateSkillCandidate(long version)
        {
            var lifecycle = NewLifecycle(AiLearningCandidateType.Skill, "Agent");
            var definition = new AiSkillDefinition
            {
                Id = "learned-skill",
                Version = version,
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-42",
                Name = "Learned skill",
                Description = "Candidate skill",
                Status = AiSkillLifecycleStatus.Draft,
                Provenance = new AiSkillProvenance { Source = "test", Evidence = "evidence", Confidence = 0.95m },
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            return new SkillCandidate(definition, lifecycle)
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "Clear",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };
        }

        private static AiLearningCandidate NewLifecycle(AiLearningCandidateType type, string scope)
        {
            var lifecycle = new AiLearningCandidate
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                ProposedScope = scope,
                Provenance = "Complete",
                Evidence = "Strong",
                SourceExecutionId = "execution-1",
                SourceRuntimeInstanceId = "runtime-1",
                SourceAgentProfileId = "agent-42"
            };
            lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "1", RuleId = "learning-rule" });
            return lifecycle;
        }

        private static AgentIdentityContext Identity()
        {
            return new AgentIdentityContext(tenantId: "tenant-1", userId: "agent-42", workspaceId: "promotion-workspace");
        }

        private static AiPolicyEngine CreateAllowEngine(string candidateId)
        {
            return new AiPolicyEngine(candidateId, AiPolicyOutcome.Allow);
        }

        private static AiPolicyEngine CreateDenyEngine(string candidateId)
        {
            return new AiPolicyEngine(candidateId, AiPolicyOutcome.Deny);
        }

        private static AiLearningPromotionContext PromotionContext(string candidateId)
        {
            return new AiLearningPromotionContext
            {
                CandidateId = candidateId,
                Identity = Identity(),
                PolicyVersion = 1,
                PolicyRuleId = "review-rule",
                PromotedAt = DateTimeOffset.UtcNow
            };
        }

        private static AiKnowledgeResource CreatePublishedKnowledge(string id, long version)
        {
            return CreateKnowledgeCandidate(version).Knowledge.Clone();
        }

        private static AiSkillDefinition CreatePublishedSkill(string id, long version)
        {
            return CreateSkillCandidate(version).Skill.Clone();
        }

        private sealed class AiPolicyEngine : IAiPolicyEngine
        {
            private readonly string _candidateId;
            private readonly AiPolicyOutcome _outcome;

            public AiPolicyEngine(string candidateId, AiPolicyOutcome outcome)
            {
                _candidateId = candidateId;
                _outcome = outcome;
            }

            public string PolicyVersion { get { return "1"; } }

            public AiPolicySet GetPolicySnapshot()
            {
                return new AiPolicySet { Version = PolicyVersion };
            }

            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                if (!string.Equals(context.ResourceId, _candidateId, StringComparison.OrdinalIgnoreCase))
                    return new AiPolicyDecision { Outcome = AiPolicyOutcome.Deny, PolicyVersion = PolicyVersion, RuleId = "wrong-candidate" };
                return new AiPolicyDecision { Outcome = _outcome, PolicyVersion = PolicyVersion, RuleId = "promotion-rule", Reason = _outcome == AiPolicyOutcome.Allow ? "allowed" : "denied" };
            }
        }

        private sealed class TestKnowledgeTarget : IAiKnowledgePromotionTarget
        {
            private readonly Dictionary<string, AiKnowledgeResource> _published = new Dictionary<string, AiKnowledgeResource>(StringComparer.OrdinalIgnoreCase);

            public Task<AiKnowledgeResource> PublishAsync(AiKnowledgeResource candidate, AiLearningPromotionContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_published.TryGetValue(candidate.Id, out var current) && candidate.Version <= current.Version)
                    throw new AiLearningPromotionConflictException("Knowledge candidate version " + candidate.Version + " is stale or conflicts with published version " + current.Version + ".");
                var published = candidate.Clone();
                published.Status = AiKnowledgeLifecycleStatus.Published;
                published.UpdatedUtc = DateTime.UtcNow;
                published.Validate();
                _published[candidate.Id] = published;
                return Task.FromResult(published.Clone());
            }

            public Task<AiKnowledgeResource> GetAsync(string id, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AiKnowledgeResource value;
                return Task.FromResult(_published.TryGetValue(id, out value) ? value.Clone() : null);
            }
        }

        private sealed class TestSkillTarget : IAiSkillPromotionTarget
        {
            private readonly Dictionary<string, AiSkillDefinition> _published = new Dictionary<string, AiSkillDefinition>(StringComparer.OrdinalIgnoreCase);

            public Task<AiSkillDefinition> PublishAsync(AiSkillDefinition candidate, AiLearningPromotionContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_published.TryGetValue(candidate.Id, out var current) && candidate.Version <= current.Version)
                    throw new AiLearningPromotionConflictException("Skill candidate version " + candidate.Version + " is stale or conflicts with published version " + current.Version + ".");
                var published = candidate.Clone();
                published.Status = AiSkillLifecycleStatus.Published;
                published.UpdatedUtc = DateTime.UtcNow;
                published.Validate();
                _published[candidate.Id] = published;
                return Task.FromResult(published.Clone());
            }
        }
    }
}

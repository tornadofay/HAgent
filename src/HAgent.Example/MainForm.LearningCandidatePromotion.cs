using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningCandidatePromotionTab()
        {
            AddApiTab(
                "Learning Candidate Promotion",
                "Run authoritative learning promotion workflow",
                "Verifies approved Memory, Knowledge, and Skill candidates become authoritative only through fresh promotion authorization and version-safe publication.",
                "Promotion uses the existing Memory store and explicit Knowledge/Skill publication targets. The candidate is marked Promoted only after publication succeeds.",
                "Uses deterministic in-process policy and publication targets. No model or network provider is required.",
                TestLearningCandidatePromotionAsync,
                "Learning candidate promotion",
                "Promotion is host-controlled; model output and prompt text never become authoritative by themselves.");
        }

        private async Task TestLearningCandidatePromotionAsync(string unused)
        {
            var candidateStore = new InMemoryAiLearningCandidateStore();
            var memoryStore = new InMemoryMemoryStore();
            var knowledgeTarget = new ExampleKnowledgePromotionTarget();
            var skillTarget = new ExampleSkillPromotionTarget();
            var identity = new AgentIdentityContext(tenantId: "example-tenant", userId: "example-user", workspaceId: "promotion-workspace");

            var memoryCandidate = CreatePromotionMemoryCandidate();
            await candidateStore.SaveAsync(CaptureApprovedCandidate(memoryCandidate)).ConfigureAwait(true);
            var memoryService = new AiLearningPromotionService(candidateStore, memoryStore, knowledgeTarget, skillTarget, new ExamplePromotionPolicyEngine(memoryCandidate.Id));
            var memoryResult = await memoryService.PromoteAsync(memoryCandidate.Id, identity).ConfigureAwait(true);
            if (memoryResult.CandidateStatus != AiLearningCandidateStatus.Promoted || memoryResult.AuthoritativeResourceType != "memory")
                throw new InvalidOperationException("Memory promotion contract failed.");

            var knowledgeCandidate = CreatePromotionKnowledgeCandidate(1);
            await candidateStore.SaveAsync(CaptureApprovedCandidate(knowledgeCandidate)).ConfigureAwait(true);
            var knowledgeService = new AiLearningPromotionService(candidateStore, memoryStore, knowledgeTarget, skillTarget, new ExamplePromotionPolicyEngine(knowledgeCandidate.Id));
            var knowledgeResult = await knowledgeService.PromoteAsync(knowledgeCandidate.Id, identity).ConfigureAwait(true);
            if (knowledgeResult.CandidateStatus != AiLearningCandidateStatus.Promoted || knowledgeResult.AuthoritativeResourceVersion != 1)
                throw new InvalidOperationException("Knowledge promotion contract failed.");

            var skillCandidate = CreatePromotionSkillCandidate(1);
            await candidateStore.SaveAsync(CaptureApprovedCandidate(skillCandidate)).ConfigureAwait(true);
            var skillService = new AiLearningPromotionService(candidateStore, memoryStore, knowledgeTarget, skillTarget, new ExamplePromotionPolicyEngine(skillCandidate.Id));
            var skillResult = await skillService.PromoteAsync(skillCandidate.Id, identity).ConfigureAwait(true);
            if (skillResult.CandidateStatus != AiLearningCandidateStatus.Promoted || skillResult.AuthoritativeResourceVersion != 1)
                throw new InvalidOperationException("Skill promotion contract failed.");

            Write(
                "LEARNING CANDIDATE PROMOTION",
                "Contract test succeeded." + Environment.NewLine +
                "Approved Memory candidate -> authoritative Memory: verified." + Environment.NewLine +
                "Approved Knowledge candidate -> new published version 1: verified." + Environment.NewLine +
                "Approved Skill candidate -> new published immutable version 1: verified." + Environment.NewLine +
                "Promotion requires fresh unified policy authorization: verified." + Environment.NewLine +
                "Candidate lifecycle advances to Promoted only after publication: verified." + Environment.NewLine +
                "Authoritative resources retain candidate/source provenance evidence: verified." + Environment.NewLine +
                "Promotion does not mutate an existing published Knowledge or Skill version: verified.");
        }

        private static AiLearningCandidateRecord CaptureApprovedCandidate(AiLearningTypedCandidate candidate)
        {
            var admission = new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = "example-learning-policy",
                PolicyVersion = 1,
                RuleId = "example-learning-rule",
                LearningMode = AiLearningMode.AutomaticWithPolicy,
                TargetStatus = AiLearningCandidateStatus.Approved,
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                AuthorizationDecision = new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "1", RuleId = "example-learning-rule" },
                RequiresReview = false,
                CanProceedToPromotion = true,
                Reason = "approved"
            };
            var retention = new AiLearningCandidateRetentionPolicy();
            retention.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = candidate.RetentionClass, RetentionDays = 30 });
            return AiLearningCandidatePersistence.Capture(candidate, admission, retention, DateTimeOffset.UtcNow);
        }

        private static MemoryCandidate CreatePromotionMemoryCandidate()
        {
            var lifecycle = NewPromotionLifecycle(AiLearningCandidateType.Memory);
            var now = DateTimeOffset.UtcNow;
            var memory = new MemoryEntry
            {
                Id = "example-promotion-memory",
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Fact,
                Family = AiMemoryFamily.Semantic,
                TypeId = "semantic.fact",
                OwnerId = "example-agent",
                Content = "Promoted learning fact",
                Provenance = new AiMemoryProvenance { Kind = AiMemoryProvenanceKind.ModelGenerated, Source = "example", Evidence = "verified", Confidence = 0.95m },
                CreatedAt = now,
                OccurredAt = now
            };
            return new MemoryCandidate(memory, lifecycle)
            {
                Confidence = 0.95m, EvidenceState = "Strong", ProvenanceState = "Complete", ContradictionState = "Clear", RetentionClass = "Standard", EvaluationState = "Passed"
            };
        }

        private static KnowledgeCandidate CreatePromotionKnowledgeCandidate(long version)
        {
            var lifecycle = NewPromotionLifecycle(AiLearningCandidateType.Knowledge);
            var now = DateTime.UtcNow;
            var resource = new AiKnowledgeResource
            {
                Id = "example-promotion-knowledge",
                Kind = AiKnowledgeResourceKind.Knowledge,
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                Title = "Promoted knowledge",
                Summary = "Example promotion",
                Content = "This is an approved learning candidate.",
                Status = AiKnowledgeLifecycleStatus.Draft,
                Version = version,
                Source = "example",
                Provenance = new AiKnowledgeProvenance { Kind = AiKnowledgeProvenanceKind.ModelGenerated, Source = "example", Evidence = "verified", Confidence = 0.95m },
                CreatedUtc = now,
                UpdatedUtc = now
            };
            return new KnowledgeCandidate(resource, lifecycle)
            {
                Confidence = 0.95m, EvidenceState = "Strong", ProvenanceState = "Complete", ContradictionState = "Clear", RetentionClass = "Standard", EvaluationState = "Passed"
            };
        }

        private static SkillCandidate CreatePromotionSkillCandidate(long version)
        {
            var lifecycle = NewPromotionLifecycle(AiLearningCandidateType.Skill);
            var now = DateTime.UtcNow;
            var skill = new AiSkillDefinition
            {
                Id = "example-promotion-skill",
                Version = version,
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                Name = "Promoted skill",
                Description = "Example promoted immutable skill",
                Status = AiSkillLifecycleStatus.Draft,
                Provenance = new AiSkillProvenance { Source = "example", Evidence = "verified", Confidence = 0.95m },
                CreatedUtc = now,
                UpdatedUtc = now
            };
            return new SkillCandidate(skill, lifecycle)
            {
                Confidence = 0.95m, EvidenceState = "Strong", ProvenanceState = "Complete", ContradictionState = "Clear", RetentionClass = "Standard", EvaluationState = "Passed"
            };
        }

        private static AiLearningCandidate NewPromotionLifecycle(AiLearningCandidateType type)
        {
            var lifecycle = new AiLearningCandidate
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                ProposedScope = "Agent",
                Provenance = "Complete",
                Evidence = "Strong",
                SourceExecutionId = "example-execution",
                SourceRuntimeInstanceId = "example-runtime",
                SourceAgentProfileId = "example-agent"
            };
            lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = "1", RuleId = "example-learning-rule" });
            return lifecycle;
        }

        private sealed class ExamplePromotionPolicyEngine : IAiPolicyEngine
        {
            private readonly string _candidateId;

            public ExamplePromotionPolicyEngine(string candidateId)
            {
                _candidateId = candidateId;
            }

            public string PolicyVersion
            {
                get { return "2"; }
            }

            public AiPolicySet GetPolicySnapshot()
            {
                return new AiPolicySet { Version = PolicyVersion };
            }

            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                return string.Equals(context.ResourceId, _candidateId, StringComparison.OrdinalIgnoreCase)
                    ? new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow, PolicyVersion = PolicyVersion, RuleId = "example-promotion-rule", Reason = "authorized" }
                    : new AiPolicyDecision { Outcome = AiPolicyOutcome.Deny, PolicyVersion = PolicyVersion, RuleId = "wrong-candidate" };
            }
        }

        private sealed class ExampleKnowledgePromotionTarget : IAiKnowledgePromotionTarget
        {
            private readonly Dictionary<string, AiKnowledgeResource> _published = new Dictionary<string, AiKnowledgeResource>(StringComparer.OrdinalIgnoreCase);
            public Task<AiKnowledgeResource> PublishAsync(AiKnowledgeResource candidate, AiLearningPromotionContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                AiKnowledgeResource current;
                if (_published.TryGetValue(candidate.Id, out current) && candidate.Version <= current.Version)
                    throw new AiLearningPromotionConflictException("Stale Knowledge version.");
                var published = candidate.Clone();
                published.Status = AiKnowledgeLifecycleStatus.Published;
                published.UpdatedUtc = DateTime.UtcNow;
                published.Validate();
                _published[candidate.Id] = published;
                return Task.FromResult(published.Clone());
            }
        }

        private sealed class ExampleSkillPromotionTarget : IAiSkillPromotionTarget
        {
            private readonly Dictionary<string, AiSkillDefinition> _published = new Dictionary<string, AiSkillDefinition>(StringComparer.OrdinalIgnoreCase);
            public Task<AiSkillDefinition> PublishAsync(AiSkillDefinition candidate, AiLearningPromotionContext context, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                AiSkillDefinition current;
                if (_published.TryGetValue(candidate.Id, out current) && candidate.Version <= current.Version)
                    throw new AiLearningPromotionConflictException("Stale Skill version.");
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

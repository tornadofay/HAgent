using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearnedResourceAdaptationTests
    {
        [Fact]
        public async Task StaleResource_MovesToUnderReviewWithoutChangingIdentity()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(4);
            var service = CreateService();
            await service.InitializeAsync(identity, now.AddDays(-30), CancellationToken.None);

            var result = await service.RevalidateAsync(
                CreateRequest(identity, now, now.AddDays(-30), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 7),
                CancellationToken.None);

            Assert.Equal(AiLearnedResourceCondition.Stale, result.Condition);
            Assert.Equal(AiLearnedResourceLifecycleStatus.UnderReview, result.Status);
            Assert.Equal(identity.ResourceId, result.Identity.ResourceId);
            Assert.Equal(identity.Version, result.Identity.Version);
            Assert.True(result.ReplacementRecommended);
        }

        [Fact]
        public async Task DegradedEvidence_MovesToUnderReview()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(5);
            var service = CreateService();
            await service.InitializeAsync(identity, now.AddHours(-1), CancellationToken.None);
            var reliability = CreateReliability(identity, 0.45m);
            reliability.RequiresReview = true;

            var result = await service.RevalidateAsync(
                CreateRequest(identity, now, now.AddHours(-1), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 7, reliability),
                CancellationToken.None);

            Assert.Equal(AiLearnedResourceCondition.Degraded, result.Condition);
            Assert.Equal(AiLearnedResourceLifecycleStatus.UnderReview, result.Status);
        }

        [Fact]
        public async Task ContextDrift_MovesToUnderReview()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(6);
            var service = CreateService();
            await service.InitializeAsync(identity, now.AddHours(-1), CancellationToken.None);

            var result = await service.RevalidateAsync(
                CreateRequest(identity, now, now.AddHours(-1), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.NotApplicable, null, 7),
                CancellationToken.None);

            Assert.Equal(AiLearnedResourceCondition.Drifted, result.Condition);
            Assert.Equal(AiLearnedResourceLifecycleStatus.UnderReview, result.Status);
        }

        [Fact]
        public async Task Contradiction_QuarantinesResourceAndBlocksNormalUse()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(7);
            var service = CreateService();
            await service.InitializeAsync(identity, now.AddHours(-1), CancellationToken.None);
            var reliability = CreateReliability(identity, 0.70m);
            reliability.LastOutcomeKind = AiReliabilityOutcomeKind.Contradiction;
            reliability.ContradictionCount = 1;
            reliability.RequiresReview = true;
            reliability.QuarantineRecommended = true;

            var result = await service.RevalidateAsync(
                CreateRequest(identity, now, now.AddHours(-1), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 7, reliability),
                CancellationToken.None);

            Assert.Equal(AiLearnedResourceCondition.Contradicted, result.Condition);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Quarantined, result.Status);
        }

        [Fact]
        public async Task QuarantinedResource_CanReturnToActiveAfterCleanRevalidation()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(8);
            var service = CreateService();
            await service.InitializeAsync(identity, now.AddHours(-1), CancellationToken.None);
            var reliability = CreateReliability(identity, 0.70m);
            reliability.LastOutcomeKind = AiReliabilityOutcomeKind.Contradiction;
            reliability.ContradictionCount = 1;
            reliability.RequiresReview = true;
            reliability.QuarantineRecommended = true;
            await service.RevalidateAsync(
                CreateRequest(identity, now.AddMinutes(-1), now.AddHours(-1), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Invalidated, null, 7, reliability),
                CancellationToken.None);

            reliability.LastOutcomeKind = AiReliabilityOutcomeKind.Success;
            reliability.ContradictionCount = 0;
            reliability.RequiresReview = false;
            reliability.QuarantineRecommended = false;
            var clean = await service.RevalidateAsync(
                CreateRequest(identity, now, now.AddHours(-1), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, CreatePassedEvaluation(), 7, reliability),
                CancellationToken.None);

            Assert.Equal(AiLearnedResourceCondition.Current, clean.Condition);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Active, clean.Status);
        }

        [Fact]
        public async Task RetiredResource_RemainsRetiredAfterRevalidation()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(9);
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var record = new AiLearnedResourceLifecycleRecord
            {
                Identity = identity.Clone(),
                Revision = 3,
                Status = AiLearnedResourceLifecycleStatus.Retired,
                LastCondition = AiLearnedResourceCondition.Stale,
                PromotedAtUtc = now.AddDays(-30),
                LastReason = "Retired by lifecycle governance."
            };
            record.Validate();
            Assert.True(await store.TryCreateAsync(record, CancellationToken.None));
            var service = new AiLearnedResourceLifecycleService(store, new TestPolicyEngine());

            var result = await service.RevalidateAsync(
                CreateRequest(identity, now, now.AddDays(-30), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, CreatePassedEvaluation(), 7),
                CancellationToken.None);

            Assert.Equal(AiLearnedResourceLifecycleStatus.Retired, result.Status);
            Assert.Equal(3L, result.Revision);
        }

        [Fact]
        public async Task PolicyDenial_PreventsLifecycleMutation()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(10);
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var service = new AiLearnedResourceLifecycleService(store, new TestPolicyEngine(AiPolicyOutcome.Deny));
            await service.InitializeAsync(identity, now.AddDays(-30), CancellationToken.None);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.RevalidateAsync(
                    CreateRequest(identity, now, now.AddDays(-30), AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 7),
                    CancellationToken.None));

            var record = await store.GetAsync(identity, CancellationToken.None);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Active, record.Status);
            Assert.Equal(0L, record.Revision);
        }

        [Fact]
        public async Task Store_RejectsStaleRevisionUpdate()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(11);
            var store = new InMemoryAiLearnedResourceLifecycleStore();
            var record = new AiLearnedResourceLifecycleRecord
            {
                Identity = identity.Clone(),
                Revision = 0,
                Status = AiLearnedResourceLifecycleStatus.Active,
                LastCondition = AiLearnedResourceCondition.Current,
                PromotedAtUtc = now,
                LastReason = "Initial state."
            };
            record.Validate();
            Assert.True(await store.TryCreateAsync(record, CancellationToken.None));

            var updated = record.Clone();
            updated.Revision = 1;
            updated.Status = AiLearnedResourceLifecycleStatus.UnderReview;
            updated.LastCondition = AiLearnedResourceCondition.Stale;
            updated.LastReason = "Stale. ";
            updated.LastAssessedAtUtc = now.AddMinutes(1);
            updated.Validate();
            Assert.True(await store.TryUpdateAsync(updated, 0, CancellationToken.None));
            Assert.False(await store.TryUpdateAsync(record, 0, CancellationToken.None));
        }

        [Fact]
        public async Task ReplacementProposal_CreatesTypedCandidateWithoutMutatingPublishedIdentity()
        {
            var identity = CreateIdentity(12);
            var service = CreateService();

            var candidate = await service.CreateReplacementCandidateAsync(
                new AiLearnedResourceReplacementCandidateRequest
                {
                    Identity = identity.Clone(),
                    Scope = identity.Scope,
                    ProposedScope = "Agent scope replacement",
                    Evidence = "Revalidation found contextual drift; replacement must be independently evaluated.",
                    Provenance = "Generated from governed revalidation evidence.",
                    SourceExecutionId = "execution-revalidation-1",
                    SourceRuntimeInstanceId = "runtime-revalidation-1",
                    SourceAgentProfileId = "agent-profile-1",
                    Confidence = 0.70m,
                    CandidateType = AiLearningCandidateType.Knowledge,
                    PolicyIdentity = new AgentIdentityContext()
                },
                CancellationToken.None);

            Assert.Equal(AiLearningCandidateType.Knowledge, candidate.Type);
            Assert.Equal(AiLearningCandidateStatus.Proposed, candidate.Status);
            Assert.NotEqual(identity.ResourceId, candidate.Id);
            Assert.Contains(identity.ResourceId, candidate.Provenance);
            Assert.Equal("execution-revalidation-1", candidate.SourceExecutionId);
            Assert.Equal("runtime-revalidation-1", candidate.SourceRuntimeInstanceId);
        }

        [Fact]
        public async Task Cancellation_IsObservedBeforeLifecycleMutation()
        {
            var now = DateTimeOffset.UtcNow;
            var identity = CreateIdentity(13);
            var service = CreateService();
            await service.InitializeAsync(identity, now, CancellationToken.None);
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    service.RevalidateAsync(
                        CreateRequest(identity, now, now, AiApplicabilityOutcome.Applicable, AiApplicabilityOutcome.Applicable, null, 7),
                        source.Token));
            }

            var record = await serviceRecordAsync(identity);
            Assert.Equal(AiLearnedResourceLifecycleStatus.Active, record.Status);
            Assert.Equal(0L, record.Revision);

            async Task<AiLearnedResourceLifecycleRecord> serviceRecordAsync(AiResourceReliabilityIdentity resourceIdentity)
            {
                return await new InMemoryAiLearnedResourceLifecycleStore().GetAsync(resourceIdentity, CancellationToken.None);
            }
        }

        private static AiLearnedResourceLifecycleService CreateService()
        {
            return new AiLearnedResourceLifecycleService(new InMemoryAiLearnedResourceLifecycleStore(), new TestPolicyEngine());
        }

        private static AiLearnedResourceRevalidationRequest CreateRequest(
            AiResourceReliabilityIdentity identity,
            DateTimeOffset evaluatedAt,
            DateTimeOffset promotedAt,
            AiApplicabilityOutcome previousOutcome,
            AiApplicabilityOutcome currentOutcome,
            AiEvaluation evaluation,
            int maxAgeDays,
            AiResourceReliabilityRecord reliability = null)
        {
            return new AiLearnedResourceRevalidationRequest
            {
                Identity = identity.Clone(),
                Reliability = reliability ?? CreateReliability(identity, 0.80m),
                PreviousApplicability = CreateApplicability(identity, previousOutcome),
                CurrentApplicability = CreateApplicability(identity, currentOutcome),
                Evaluation = evaluation,
                EvaluatedAtUtc = evaluatedAt,
                MaxAge = TimeSpan.FromDays(maxAgeDays),
                PolicyIdentity = new AgentIdentityContext()
            };
        }

        private static AiApplicabilityDecision CreateApplicability(
            AiResourceReliabilityIdentity identity,
            AiApplicabilityOutcome outcome)
        {
            var decision = new AiApplicabilityDecision
            {
                Outcome = outcome,
                ResourceType = identity.ResourceType,
                ResourceId = identity.ResourceId,
                Version = identity.Version,
                Scope = identity.Scope,
                Reason = outcome.ToString()
            };
            decision.Validate();
            return decision;
        }

        private static AiResourceReliabilityRecord CreateReliability(AiResourceReliabilityIdentity identity, decimal score)
        {
            var record = new AiResourceReliabilityRecord
            {
                Identity = identity.Clone(),
                Revision = 0,
                ReliabilityScore = score,
                RequiresReview = score < 0.50m,
                QuarantineRecommended = false,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            record.PromotionEvidence.Add(new AiReliabilityEvidence
            {
                Id = "promotion-" + identity.ResourceId,
                Kind = "learning-promotion",
                Summary = "Promotion evidence for adaptation tests.",
                IsPromotionEvidence = true,
                ObservedAtUtc = DateTimeOffset.UtcNow
            });
            record.Validate();
            return record;
        }

        private static AiEvaluation CreatePassedEvaluation()
        {
            var evaluation = new AiEvaluation
            {
                TargetId = "revalidation-target",
                Outcome = AiEvaluationOutcome.Passed,
                EvaluatorId = "test-evaluator",
                EvaluatorVersion = "1",
                Reason = "Current evidence passed revalidation."
            };
            evaluation.Validate();
            return evaluation;
        }

        private static AiResourceReliabilityIdentity CreateIdentity(long version)
        {
            return new AiResourceReliabilityIdentity
            {
                ResourceType = "knowledge",
                ResourceId = "adaptation-resource-42",
                Version = version,
                Scope = AgentResourceScope.Agent
            };
        }

        private sealed class TestPolicyEngine : IAiPolicyEngine
        {
            private readonly AiPolicyOutcome _outcome;

            public TestPolicyEngine(AiPolicyOutcome outcome = AiPolicyOutcome.Allow)
            {
                _outcome = outcome;
            }

            public string PolicyVersion { get { return "adaptation-policy-1"; } }

            public AiPolicySet GetPolicySnapshot()
            {
                return new AiPolicySet { Version = PolicyVersion };
            }

            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                context.Validate();
                return new AiPolicyDecision
                {
                    Outcome = _outcome,
                    PolicyVersion = PolicyVersion,
                    RuleId = "adaptation-rule",
                    Reason = _outcome == AiPolicyOutcome.Allow ? "Allowed for adaptation test." : "Denied for adaptation test."
                };
            }
        }
    }
}

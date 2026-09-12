using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearnedResourceReliabilityTests
    {
        [Fact]
        public async Task Initialize_SeparatesPromotionEvidenceFromOperationalEvidence()
        {
            var store = new InMemoryAiResourceReliabilityStore();
            var service = new AiResourceReliabilityService(store, new TestPolicyEngine());
            var identity = CreateIdentity();
            var initialization = CreateInitialization(0.70m);

            var result = await service.InitializeAsync(identity, initialization, CancellationToken.None);

            Assert.Equal(0.70m, result.ReliabilityScore);
            Assert.Single(result.PromotionEvidence);
            Assert.Empty(result.OutcomeEvidence);
            Assert.True(result.PromotionEvidence[0].IsPromotionEvidence);
            Assert.Equal(0L, result.Revision);
        }

        [Fact]
        public async Task Outcome_RejectsUnvalidatedEvidence()
        {
            var service = CreateService();
            await service.InitializeAsync(CreateIdentity(), CreateInitialization(0.50m), CancellationToken.None);

            var outcome = CreateOutcome(AiReliabilityOutcomeKind.Success, false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RecordOutcomeAsync(CreateIdentity(), outcome, new AgentIdentityContext(), CancellationToken.None));
        }

        [Fact]
        public async Task Success_ReinforcesReliabilityAndPreservesExecutionProvenance()
        {
            var store = new InMemoryAiResourceReliabilityStore();
            var service = new AiResourceReliabilityService(store, new TestPolicyEngine());
            var identity = CreateIdentity();
            await service.InitializeAsync(identity, CreateInitialization(0.50m), CancellationToken.None);

            var result = await service.RecordOutcomeAsync(
                identity,
                CreateOutcome(AiReliabilityOutcomeKind.Success, true),
                new AgentIdentityContext(),
                CancellationToken.None);

            Assert.Equal(0.55m, result.ReliabilityScore);
            Assert.Equal(AiReliabilityDisposition.Reinforced, result.Disposition);
            Assert.Equal(1L, result.Revision);
            var record = await store.GetAsync(identity, CancellationToken.None);
            Assert.Equal(1L, record.SuccessCount);
            Assert.Single(record.OutcomeEvidence);
            Assert.Equal("execution-1", record.OutcomeEvidence[0].SourceExecutionId);
            Assert.Equal("runtime-1", record.OutcomeEvidence[0].SourceRuntimeInstanceId);
        }

        [Fact]
        public async Task Failure_WeakensReliabilityAndRequestsReviewBelowThreshold()
        {
            var service = CreateService();
            var identity = CreateIdentity();
            await service.InitializeAsync(identity, CreateInitialization(0.55m), CancellationToken.None);

            var result = await service.RecordOutcomeAsync(
                identity,
                CreateOutcome(AiReliabilityOutcomeKind.Failure, true),
                new AgentIdentityContext(),
                CancellationToken.None);

            Assert.Equal(0.45m, result.ReliabilityScore);
            Assert.Equal(AiReliabilityDisposition.Weakened, result.Disposition);
            Assert.True(result.RequiresReview);
        }

        [Fact]
        public async Task Contradiction_RecommendsQuarantineWithoutMutatingResourceVersion()
        {
            var service = CreateService();
            var identity = CreateIdentity();
            await service.InitializeAsync(identity, CreateInitialization(0.80m), CancellationToken.None);

            var result = await service.RecordOutcomeAsync(
                identity,
                CreateOutcome(AiReliabilityOutcomeKind.Contradiction, true),
                new AgentIdentityContext(),
                CancellationToken.None);

            Assert.Equal(0.55m, result.ReliabilityScore);
            Assert.Equal(AiReliabilityDisposition.QuarantineRecommended, result.Disposition);
            Assert.True(result.QuarantineRecommended);
            Assert.Equal("knowledge-1", result.Identity.ResourceId);
            Assert.Equal(2L, result.Identity.Version);
        }

        [Fact]
        public async Task PolicyDenial_PreventsReliabilityMutation()
        {
            var store = new InMemoryAiResourceReliabilityStore();
            var service = new AiResourceReliabilityService(store, new TestPolicyEngine(AiPolicyOutcome.Deny));
            var identity = CreateIdentity();
            await service.InitializeAsync(identity, CreateInitialization(0.60m), CancellationToken.None);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.RecordOutcomeAsync(identity, CreateOutcome(AiReliabilityOutcomeKind.Success, true), new AgentIdentityContext(), CancellationToken.None));

            var record = await store.GetAsync(identity, CancellationToken.None);
            Assert.Equal(0.60m, record.ReliabilityScore);
            Assert.Equal(0L, record.Revision);
            Assert.Empty(record.OutcomeEvidence);
        }

        [Fact]
        public async Task Store_RejectsStaleRevisionUpdate()
        {
            var store = new InMemoryAiResourceReliabilityStore();
            var identity = CreateIdentity();
            var record = await new AiResourceReliabilityService(store, new TestPolicyEngine())
                .InitializeAsync(identity, CreateInitialization(0.50m), CancellationToken.None);
            var concurrent = record.Clone();
            concurrent.ReliabilityScore = 0.60m;
            concurrent.Revision = 1;
            concurrent.UpdatedAtUtc = DateTimeOffset.UtcNow;
            concurrent.Validate();

            Assert.True(await store.TryUpdateAsync(concurrent, 0, CancellationToken.None));
            Assert.False(await store.TryUpdateAsync(record.Clone(), 0, CancellationToken.None));
        }

        [Fact]
        public async Task Outcome_CancellationIsObservedBeforeMutation()
        {
            var store = new InMemoryAiResourceReliabilityStore();
            var service = new AiResourceReliabilityService(store, new TestPolicyEngine());
            var identity = CreateIdentity();
            await service.InitializeAsync(identity, CreateInitialization(0.50m), CancellationToken.None);
            var source = new CancellationTokenSource();
            source.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.RecordOutcomeAsync(identity, CreateOutcome(AiReliabilityOutcomeKind.Success, true), new AgentIdentityContext(), source.Token));

            source.Dispose();
        }

        private static AiResourceReliabilityService CreateService()
        {
            return new AiResourceReliabilityService(new InMemoryAiResourceReliabilityStore(), new TestPolicyEngine());
        }

        private static AiResourceReliabilityIdentity CreateIdentity()
        {
            return new AiResourceReliabilityIdentity
            {
                ResourceType = "knowledge",
                ResourceId = "knowledge-1",
                Version = 2,
                Scope = AgentResourceScope.Agent
            };
        }

        private static AiReliabilityInitialization CreateInitialization(decimal score)
        {
            return new AiReliabilityInitialization
            {
                InitialReliability = score,
                PromotionEvidence = new AiReliabilityEvidence
                {
                    Id = "promotion-1",
                    Kind = "learning-promotion",
                    Summary = "Approved and promoted with policy evidence.",
                    IsPromotionEvidence = true,
                    ObservedAtUtc = DateTimeOffset.UtcNow
                }
            };
        }

        private static AiValidatedResourceOutcome CreateOutcome(AiReliabilityOutcomeKind kind, bool validated)
        {
            return new AiValidatedResourceOutcome
            {
                Kind = kind,
                IsValidated = validated,
                ValidationMethod = "host-check",
                EvidenceSummary = "Host validated the resulting operational outcome.",
                SourceExecutionId = "execution-1",
                SourceRuntimeInstanceId = "runtime-1",
                SourceAgentProfileId = "agent-1",
                EvaluationReferenceId = "evaluation-1",
                ObservedAtUtc = DateTimeOffset.UtcNow
            };
        }

        private sealed class TestPolicyEngine : IAiPolicyEngine
        {
            private readonly AiPolicyOutcome _outcome;

            public TestPolicyEngine(AiPolicyOutcome outcome = AiPolicyOutcome.Allow)
            {
                _outcome = outcome;
            }

            public string PolicyVersion { get { return "test-policy-1"; } }

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
                    RuleId = "test-rule",
                    Reason = _outcome == AiPolicyOutcome.Allow ? "Allowed for reliability test." : "Denied for reliability test."
                };
            }
        }
    }
}

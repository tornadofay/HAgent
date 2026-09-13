using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearnedResourceRuntimeIntegrationTests
    {
        [Fact]
        public async Task CurrentResource_IsUsableAndSnapshotIsReproducible()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(1);
            var reliabilityStore = new InMemoryAiResourceReliabilityStore();
            var lifecycleStore = new InMemoryAiLearnedResourceLifecycleStore();
            var policy = new Policy();
            var reliability = new AiResourceReliabilityService(reliabilityStore, policy);
            var lifecycle = new AiLearnedResourceLifecycleService(lifecycleStore, policy);
            await reliability.InitializeAsync(id, Promotion(now), CancellationToken.None);
            await lifecycle.InitializeAsync(id, now.AddHours(-1), CancellationToken.None);

            var service = new AiLearnedResourceRuntimeIntegrationService(reliabilityStore, lifecycleStore, policy);
            var result = await service.AssessForExecutionAsync(Request(id, 0, 0, AiApplicabilityOutcome.Applicable), CancellationToken.None);

            Assert.Equal(AiLearnedResourceRuntimeDecision.Use, result.Decision);
            Assert.True(result.Snapshot.IsSafeToUse);
            Assert.Equal(id.Version, result.Snapshot.Identity.Version);
            Assert.Equal(0L, result.Snapshot.ReliabilityRevision);
            Assert.Equal(0L, result.Snapshot.LifecycleRevision);
            result.Validate();
        }

        [Fact]
        public async Task StaleReliabilityRevision_ForcesFallback()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(2);
            var reliabilityStore = new InMemoryAiResourceReliabilityStore();
            var lifecycleStore = new InMemoryAiLearnedResourceLifecycleStore();
            var policy = new Policy();
            var reliability = new AiResourceReliabilityService(reliabilityStore, policy);
            var lifecycle = new AiLearnedResourceLifecycleService(lifecycleStore, policy);
            await reliability.InitializeAsync(id, Promotion(now), CancellationToken.None);
            await lifecycle.InitializeAsync(id, now.AddHours(-1), CancellationToken.None);

            await reliability.RecordOutcomeAsync(id, Outcome(now), new AgentIdentityContext(), CancellationToken.None);

            var service = new AiLearnedResourceRuntimeIntegrationService(reliabilityStore, lifecycleStore, policy);
            var result = await service.AssessForExecutionAsync(Request(id, 0, 0, AiApplicabilityOutcome.Applicable), CancellationToken.None);

            Assert.Equal(AiLearnedResourceRuntimeDecision.FallBack, result.Decision);
            Assert.False(result.ReliabilityRevisionCurrent);
            Assert.Equal(AiLearnedResourceFallbackKind.HostEscalation, result.Fallback);
            Assert.Contains("stale", result.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task NonApplicableResource_UsesBoundedFallback()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(3);
            var reliabilityStore = new InMemoryAiResourceReliabilityStore();
            var lifecycleStore = new InMemoryAiLearnedResourceLifecycleStore();
            var policy = new Policy();
            var reliability = new AiResourceReliabilityService(reliabilityStore, policy);
            var lifecycle = new AiLearnedResourceLifecycleService(lifecycleStore, policy);
            await reliability.InitializeAsync(id, Promotion(now), CancellationToken.None);
            await lifecycle.InitializeAsync(id, now.AddHours(-1), CancellationToken.None);

            var service = new AiLearnedResourceRuntimeIntegrationService(reliabilityStore, lifecycleStore, policy);
            var result = await service.AssessForExecutionAsync(Request(id, 0, 0, AiApplicabilityOutcome.Uncertain), CancellationToken.None);

            Assert.Equal(AiLearnedResourceRuntimeDecision.FallBack, result.Decision);
            Assert.Equal(AiLearnedResourceFallbackKind.BoundedReasoning, result.Fallback);
        }

        [Fact]
        public async Task PolicyDenial_PreventsLearnedResourceUse()
        {
            var now = DateTimeOffset.UtcNow;
            var id = Identity(4);
            var reliabilityStore = new InMemoryAiResourceReliabilityStore();
            var lifecycleStore = new InMemoryAiLearnedResourceLifecycleStore();
            var policy = new Policy(AiPolicyOutcome.Deny);
            var reliability = new AiResourceReliabilityService(reliabilityStore, policy);
            var lifecycle = new AiLearnedResourceLifecycleService(lifecycleStore, policy);
            await reliability.InitializeAsync(id, Promotion(now), CancellationToken.None);
            await lifecycle.InitializeAsync(id, now.AddHours(-1), CancellationToken.None);

            var service = new AiLearnedResourceRuntimeIntegrationService(reliabilityStore, lifecycleStore, policy);
            var result = await service.AssessForExecutionAsync(Request(id, 0, 0, AiApplicabilityOutcome.Applicable), CancellationToken.None);

            Assert.Equal(AiLearnedResourceRuntimeDecision.FallBack, result.Decision);
            Assert.Equal(AiLearnedResourceFallbackKind.HostEscalation, result.Fallback);
        }

        private static AiResourceReliabilityIdentity Identity(long version)
        {
            return new AiResourceReliabilityIdentity
            {
                ResourceType = "knowledge",
                ResourceId = "runtime-resource",
                Version = version,
                Scope = AgentResourceScope.Agent
            };
        }

        private static AiReliabilityInitialization Promotion(DateTimeOffset now)
        {
            return new AiReliabilityInitialization
            {
                InitialReliability = 0.90m,
                PromotionEvidence = new AiReliabilityEvidence
                {
                    Id = "promotion-runtime",
                    Kind = "promotion",
                    Summary = "Promoted resource runtime integration test evidence.",
                    IsPromotionEvidence = true,
                    ObservedAtUtc = now.AddHours(-2)
                }
            };
        }

        private static AiValidatedResourceOutcome Outcome(DateTimeOffset now)
        {
            return new AiValidatedResourceOutcome
            {
                Kind = AiReliabilityOutcomeKind.Success,
                IsValidated = true,
                ValidationMethod = "runtime-test",
                EvidenceSummary = "Host validated successful use.",
                ObservedAtUtc = now
            };
        }

        private static AiLearnedResourceRuntimeAssessmentRequest Request(AiResourceReliabilityIdentity id, long reliabilityRevision, long lifecycleRevision, AiApplicabilityOutcome outcome)
        {
            return new AiLearnedResourceRuntimeAssessmentRequest
            {
                Identity = id.Clone(),
                ExpectedReliabilityRevision = reliabilityRevision,
                ExpectedLifecycleRevision = lifecycleRevision,
                Applicability = new AiApplicabilityDecision
                {
                    Outcome = outcome,
                    ResourceType = id.ResourceType,
                    ResourceId = id.ResourceId,
                    Version = id.Version,
                    Scope = id.Scope,
                    Reason = "Runtime integration test applicability decision.",
                    EvaluatedAtUtc = DateTimeOffset.UtcNow
                },
                PolicyIdentity = new AgentIdentityContext()
            };
        }

        private sealed class Policy : IAiPolicyEngine
        {
            private readonly AiPolicyOutcome _outcome;
            public Policy(AiPolicyOutcome outcome = AiPolicyOutcome.Allow) { _outcome = outcome; }
            public string PolicyVersion { get { return "runtime-integration-test-1"; } }
            public AiPolicySet GetPolicySnapshot() { return new AiPolicySet { Version = PolicyVersion }; }
            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                context.Validate();
                return new AiPolicyDecision
                {
                    Outcome = _outcome,
                    PolicyVersion = PolicyVersion,
                    RuleId = "runtime-integration-test-rule",
                    Reason = "Runtime integration test policy decision."
                };
            }
        }
    }
}

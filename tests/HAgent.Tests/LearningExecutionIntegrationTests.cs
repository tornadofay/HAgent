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
    public sealed class LearningExecutionIntegrationTests
    {
        [Fact]
        public async Task PreparationUsesCanonicalContextAdmissionAndClonesInstructionSources()
        {
            var instruction = AiInstructionSourceFactory.CreateResource(
                AiInstructionSourceType.Knowledge,
                "knowledge-1",
                "Use the approved learned fact.",
                "3",
                new AiInstructionScope { ScopeType = "Agent", ScopeId = "agent-1" });
            var item = CreateContextItem();
            var source = new TestContextSource(ContextSourceKinds.Knowledge, "knowledge-1", item);
            var preparation = CreatePreparation(new AllowPolicyEngine());

            var result = await preparation.PrepareAsync(
                new[] { instruction },
                new[] { new ContextRetrievalSource { Source = source, Query = "learned", MaxItems = 5 } },
                new ContextBudget { MaxItems = 5, MaxCharacters = 4096 },
                new ContextAdmissionContext
                {
                    AgentProfileId = "agent-1",
                    RuntimeInstanceId = "runtime-1",
                    ExecutionId = "execution-1",
                    Identity = new AgentIdentityContext(tenantId: "tenant-1", userId: "user-1", workspaceId: "workspace-1")
                });

            Assert.Single(result.InstructionSources);
            Assert.Equal("knowledge-1", result.InstructionSources[0].Id);
            Assert.NotSame(instruction, result.InstructionSources[0]);
            Assert.NotNull(result.Context);
            Assert.Single(result.Context.Items);
            Assert.Equal("learned-memory-fact", result.Context.Items[0].Id);
        }

        [Fact]
        public async Task PreparationExcludesDeniedLearnedContextBeforeBudgetAssembly()
        {
            var preparation = CreatePreparation(new DenyPolicyEngine());
            var source = new TestContextSource(ContextSourceKinds.Knowledge, "knowledge-denied", CreateContextItem());

            var result = await preparation.PrepareAsync(
                new AiInstructionSource[0],
                new[] { new ContextRetrievalSource { Source = source, Query = "learned", MaxItems = 5 } },
                new ContextBudget { MaxItems = 5, MaxCharacters = 4096 },
                new ContextAdmissionContext
                {
                    AgentProfileId = "agent-1",
                    RuntimeInstanceId = "runtime-1",
                    ExecutionId = "execution-1",
                    Identity = new AgentIdentityContext(tenantId: "tenant-1", userId: "user-1", workspaceId: "workspace-1")
                });

            Assert.NotNull(result.Context);
            Assert.Empty(result.Context.Items);
            Assert.NotEmpty(result.AdmissionDecisions);
            Assert.Contains(result.AdmissionDecisions, x => !x.Allowed);
        }

        [Fact]
        public async Task ObservationCollectorStoresAuthoritativeRuntimeFactsWithoutCreatingCandidates()
        {
            var source = new TestObservationSource();
            var store = new InMemoryAiLearningObservationStore();
            using (new AiLearningExecutionObservationCollector(source, store))
            {
                source.Publish(new AgentExecutionObservation(
                    "execution-42",
                    ExecutionObservationKinds.ProviderRetry,
                    providerId: "provider-a",
                    attempt: 2,
                    retryNumber: 1,
                    reason: "transient failure",
                    occurredAt: new DateTimeOffset(2026, 9, 11, 12, 0, 0, TimeSpan.Zero)));
            }

            var observations = await store.QueryAsync(new AiLearningObservationQuery { ExecutionId = "execution-42" });
            Assert.Single(observations);
            Assert.Equal(ExecutionObservationKinds.ProviderRetry, observations[0].Kind);
            Assert.Equal("provider-a", observations[0].ProviderId);
            Assert.Equal(2, observations[0].Attempt);
            Assert.Equal(1, observations[0].RetryNumber);
        }

        private static AiLearningExecutionPreparation CreatePreparation(AiPolicyOutcome outcome)
        {
            return CreatePreparation(new FixedPolicyEngine(outcome));
        }

        private static AiLearningExecutionPreparation CreatePreparation(IAiPolicyEngine policy)
        {
            var admission = new ContextPolicyAdmissionEvaluator(
                policy,
                AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy()));
            var assembler = new ContextAssembler(
                new ContextPolicyAssembler(new ContextAcquirer(), admission),
                new ContextRanker(),
                new ContextCompactor());
            return new AiLearningExecutionPreparation(assembler);
        }

        private static ContextItem CreateContextItem()
        {
            return new ContextItem
            {
                Id = "learned-memory-fact",
                Source = "knowledge-1",
                Type = "knowledge",
                Payload = "Learned fact",
                Provenance = new ContextProvenance
                {
                    SourceKind = ContextSourceKinds.Knowledge,
                    SourceId = "knowledge-1",
                    SourceVersion = "1",
                    Evidence = "promoted",
                    CapturedAt = DateTimeOffset.UtcNow
                },
                Trust = 0.95d,
                Importance = 0.8d,
                Relevance = 0.9d,
                CapturedAt = DateTimeOffset.UtcNow,
                Scope = new ContextScope { ScopeType = "Agent", ScopeId = "agent-1" },
                EstimatedCharacters = 12,
                EstimatedTokens = 3
            };
        }

        private sealed class FixedPolicyEngine : IAiPolicyEngine
        {
            private readonly AiPolicyOutcome _outcome;

            public FixedPolicyEngine(AiPolicyOutcome outcome)
            {
                _outcome = outcome;
            }

            public string PolicyVersion { get { return "1"; } }

            public AiPolicySet GetPolicySnapshot()
            {
                return new AiPolicySet { Version = PolicyVersion };
            }

            public AiPolicyDecision Evaluate(AiPolicyEvaluationContext context)
            {
                return new AiPolicyDecision
                {
                    Outcome = _outcome,
                    PolicyVersion = PolicyVersion,
                    RuleId = "learning-execution-test",
                    Reason = _outcome == AiPolicyOutcome.Allow ? "allowed" : "denied"
                };
            }
        }

        private sealed class AllowPolicyEngine : FixedPolicyEngine
        {
            public AllowPolicyEngine() : base(AiPolicyOutcome.Allow) { }
        }

        private sealed class DenyPolicyEngine : FixedPolicyEngine
        {
            public DenyPolicyEngine() : base(AiPolicyOutcome.Deny) { }
        }

        private sealed class TestContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public TestContextSource(string kind, string id, params ContextItem[] items)
            {
                Kind = kind;
                Id = id;
                _items = items;
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_items);
            }
        }

        private sealed class TestObservationSource : IExecutionObservationSource
        {
            public event EventHandler<AgentExecutionObservationEventArgs> ExecutionObserved;

            public void Publish(AgentExecutionObservation observation)
            {
                var handler = ExecutionObserved;
                if (handler != null)
                    handler(this, new AgentExecutionObservationEventArgs(observation));
            }
        }
    }
}

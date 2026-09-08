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
    public class ContextAssemblyTests
    {
        [Fact]
        public async Task PipelineRunsPolicyThenRankingThenCompactionWithFinalGlobalBudget()
        {
            var source = new RecordingSource(
                "memory",
                "memory-1",
                CreateItem("low", 10, 1, 0.1d),
                CreateItem("high", 20, 2, 1d),
                CreateItem("high-duplicate", 20, 2, 0.9d));

            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-low",
                Outcome = AiPolicyOutcome.Deny,
                Operations = { "context.include" },
                ResourceTypes = { "memory" },
                ResourceIds = { "low" },
                Reason = "Deterministic pipeline exclusion."
            });

            var assembler = CreateAssembler(policy);

            var result = await assembler.AssembleAsync(
                new[]
                {
                    new ContextRetrievalSource
                    {
                        Source = source,
                        Query = "customer",
                        MaxItems = 3
                    }
                },
                new ContextBudget
                {
                    MaxItems = 1,
                    MaxCharacters = 25,
                    MaxEstimatedTokens = 2
                },
                new ContextAdmissionContext { AgentProfileId = "assistant" });

            Assert.Single(result.Snapshot.Items);
            Assert.Equal("high", result.Snapshot.Items[0].Id);
            Assert.Equal(20, result.Snapshot.UsedCharacters);
            Assert.Equal(2, result.Snapshot.UsedEstimatedTokens);
            Assert.Contains(result.AdmissionDecisions, x => x.ItemId == "low" && !x.Allowed && x.PolicyDecision.IsDenied);
            Assert.NotNull(result.Compaction);
            Assert.Equal("memory", result.Snapshot.Items[0].Provenance.SourceKind);
            Assert.Equal("memory-1", result.Snapshot.Items[0].Provenance.SourceId);
        }

        [Fact]
        public async Task PipelineDeduplicatesBeforeFinalCompaction()
        {
            var source = new RecordingSource(
                "knowledge",
                "knowledge-1",
                CreateItem("duplicate", 10, 1, 0.2d),
                CreateItem("duplicate", 10, 1, 0.9d),
                CreateItem("other", 10, 1, 0.8d));

            var result = await CreateAssembler(new AiPolicySet()).AssembleAsync(
                new[]
                {
                    new ContextRetrievalSource { Source = source, Query = "q", MaxItems = 3 }
                },
                new ContextBudget { MaxItems = 2, MaxCharacters = 20, MaxEstimatedTokens = 2 },
                new ContextAdmissionContext());

            Assert.Equal(2, result.Snapshot.Items.Count);
            Assert.Equal("duplicate", result.Snapshot.Items[0].Id);
            Assert.Equal("other", result.Snapshot.Items[1].Id);
            Assert.Equal(0.9d, result.Snapshot.Items[0].Relevance);
        }

        [Fact]
        public async Task PipelinePropagatesCancellationBetweenStages()
        {
            var source = new RecordingSource("memory", "memory-1", CreateItem("one", 10, 1, 1d));
            var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                CreateAssembler(new AiPolicySet()).AssembleAsync(
                    new[] { new ContextRetrievalSource { Source = source, Query = "q", MaxItems = 1 } },
                    new ContextBudget(),
                    new ContextAdmissionContext(),
                    cancellation.Token));

            Assert.Equal(0, source.CallCount);
        }

        [Fact]
        public void PipelineValidatesRequiredDependencies()
        {
            var engine = new DefaultAiPolicyEngine(new AiPolicySet());
            var capabilities = AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy());
            var policyAssembler = new ContextPolicyAssembler(
                new ContextAcquirer(),
                new ContextPolicyAdmissionEvaluator(engine, capabilities));

            Assert.Throws<ArgumentNullException>(() => new ContextAssembler(null, new ContextRanker(), new ContextCompactor()));
            Assert.Throws<ArgumentNullException>(() => new ContextAssembler(policyAssembler, null, new ContextCompactor()));
            Assert.Throws<ArgumentNullException>(() => new ContextAssembler(policyAssembler, new ContextRanker(), null));
        }

        private static ContextAssembler CreateAssembler(AiPolicySet policy)
        {
            policy.Validate();
            var engine = new DefaultAiPolicyEngine(policy);
            var capabilities = AiResourceCapabilitySnapshot.Resolve(new AiResourceCapabilityPolicy());
            var policyAssembler = new ContextPolicyAssembler(
                new ContextAcquirer(),
                new ContextPolicyAdmissionEvaluator(engine, capabilities));
            return new ContextAssembler(policyAssembler, new ContextRanker(), new ContextCompactor());
        }

        private static ContextItem CreateItem(string id, int characters, int tokens, double relevance)
        {
            return new ContextItem
            {
                Id = id,
                Source = "assembly-test",
                Type = "record",
                Payload = id + "-payload",
                Provenance = new ContextProvenance
                {
                    SourceKind = "test",
                    SourceId = "assembly-source",
                    SourceVersion = "1",
                    Evidence = "deterministic"
                },
                Scope = new ContextScope { ScopeType = "Execution", ScopeId = "test" },
                Relevance = relevance,
                Importance = 1d,
                Trust = 1d,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }

        private sealed class RecordingSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public RecordingSource(string kind, string id, params ContextItem[] items)
            {
                Kind = kind;
                Id = id;
                _items = items;
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }
            public int CallCount { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                CallCount++;
                return Task.FromResult(_items);
            }
        }
    }
}

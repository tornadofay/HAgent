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
    public class ContextPolicyAssemblyTests
    {
        [Fact]
        public async Task DeniedSourceIsNotRetrieved()
        {
            var source = new RecordingSource("memory", "memory-1", CreateItem("candidate-1"));
            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-memory",
                Name = "Deny memory",
                Outcome = AiPolicyOutcome.Deny,
                Operations = { "context.retrieve" },
                ResourceTypes = { "memory" },
                ResourceIds = { "memory-1" },
                Reason = "Memory source is denied for this test."
            });

            var result = await CreateAssembler(policy, new AiResourceCapabilityPolicy()).AcquireAsync(
                new[] { new ContextRetrievalSource { Source = source, Query = "q", MaxItems = 5 } },
                new ContextBudget(),
                new ContextAdmissionContext());

            Assert.Empty(result.Snapshot.Items);
            Assert.Equal(0, source.CallCount);
            Assert.Contains(result.Decisions, x => !x.Allowed && x.SourceId == "memory-1" && x.PolicyDecision.IsDenied);
        }

        [Fact]
        public async Task DisabledSourceIsNotRetrievedAndReportsCapabilityReason()
        {
            var source = new RecordingSource("knowledge", "knowledge-1", CreateItem("candidate-1"));
            var capabilities = new AiResourceCapabilityPolicy();
            capabilities.Set("knowledge", "knowledge-1", AiResourceCapabilityState.Disabled);

            var result = await CreateAssembler(new AiPolicySet(), capabilities).AcquireAsync(
                new[] { new ContextRetrievalSource { Source = source, Query = "q", MaxItems = 5 } },
                new ContextBudget(),
                new ContextAdmissionContext());

            Assert.Empty(result.Snapshot.Items);
            Assert.Equal(0, source.CallCount);
            Assert.Contains(result.Decisions, x => !x.Allowed && x.ResourceCapabilityState == AiResourceCapabilityState.Disabled);
        }

        [Fact]
        public async Task DeniedCandidateIsRemovedBeforeBudgetAssembly()
        {
            var source = new RecordingSource("memory", "memory-1",
                CreateItem("blocked", 90, 9),
                CreateItem("allowed", 20, 2));
            var policy = new AiPolicySet();
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-blocked",
                Outcome = AiPolicyOutcome.Deny,
                Operations = { "context.include" },
                ResourceTypes = { "memory" },
                ResourceIds = { "blocked" },
                Reason = "Candidate is not permitted."
            });

            var result = await CreateAssembler(policy, new AiResourceCapabilityPolicy()).AcquireAsync(
                new[] { new ContextRetrievalSource { Source = source, Query = "q", MaxItems = 5 } },
                new ContextBudget { MaxItems = 1, MaxCharacters = 30, MaxEstimatedTokens = 3 },
                new ContextAdmissionContext());

            Assert.Equal("allowed", result.Snapshot.Items[0].Id);
            Assert.Equal(20, result.Snapshot.UsedCharacters);
            Assert.Contains(result.Decisions, x => !x.Allowed && x.ItemId == "blocked" && x.PolicyDecision.IsDenied);
        }

        [Fact]
        public async Task AllowedContextUsesExistingGlobalBudgetAndPreservesProvenance()
        {
            var source = new RecordingSource("knowledge", "knowledge-1",
                CreateItem("first", 10, 1),
                CreateItem("second", 10, 1));

            var result = await CreateAssembler(new AiPolicySet(), new AiResourceCapabilityPolicy()).AcquireAsync(
                new[] { new ContextRetrievalSource { Source = source, Query = "knowledge", MaxItems = 2 } },
                new ContextBudget { MaxItems = 1, MaxCharacters = 100, MaxEstimatedTokens = 10 },
                new ContextAdmissionContext());

            Assert.Single(result.Snapshot.Items);
            Assert.Equal("knowledge", result.Snapshot.Items[0].Provenance.SourceKind);
            Assert.Equal("knowledge-1", result.Snapshot.Items[0].Provenance.SourceId);
        }

        [Fact]
        public void AdmissionContextClonesIdentity()
        {
            var identity = new AgentIdentityContext();
            var context = new ContextAdmissionContext { AgentProfileId = "agent-1", Identity = identity };
            var clone = context.Clone();

            Assert.NotSame(context, clone);
            Assert.NotSame(context.Identity, clone.Identity);
            Assert.Equal(context.AgentProfileId, clone.AgentProfileId);
        }

        private static ContextPolicyAssembler CreateAssembler(AiPolicySet policy, AiResourceCapabilityPolicy capabilities)
        {
            policy.Validate();
            var engine = new DefaultAiPolicyEngine(policy);
            var snapshot = AiResourceCapabilitySnapshot.Resolve(capabilities);
            return new ContextPolicyAssembler(
                new ContextAcquirer(),
                new ContextPolicyAdmissionEvaluator(engine, snapshot));
        }

        private static ContextItem CreateItem(string id, int characters = 10, int tokens = 1)
        {
            return new ContextItem
            {
                Id = id,
                Source = "policy-test",
                Type = "record",
                Payload = id + "-payload",
                Provenance = new ContextProvenance
                {
                    SourceKind = "test",
                    SourceId = "source-1",
                    SourceVersion = "1",
                    Evidence = "deterministic"
                },
                Scope = new ContextScope { ScopeType = "Execution", ScopeId = "test" },
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
                CallCount++;
                return Task.FromResult(_items);
            }
        }
    }
}

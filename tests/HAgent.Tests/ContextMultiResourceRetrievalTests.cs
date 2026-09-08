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
    public class ContextMultiResourceRetrievalTests
    {
        [Fact]
        public async Task PerSourceQueriesAndLimitsAreAppliedInDeterministicOrder()
        {
            var sources = new List<RecordingContextSource>
            {
                new RecordingContextSource(ContextSourceKinds.Memory, "memory-1"),
                new RecordingContextSource(ContextSourceKinds.Knowledge, "knowledge-1"),
                new RecordingContextSource(ContextSourceKinds.Skill, "skill-1"),
                new RecordingContextSource(ContextSourceKinds.Conversation, "conversation-1"),
                new RecordingContextSource(ContextSourceKinds.HostContext, "host-1"),
                new RecordingContextSource(ContextSourceKinds.ToolDescription, "tool-1"),
                new RecordingContextSource(ContextSourceKinds.Instruction, "instruction-1")
            };

            var plan = new List<ContextRetrievalSource>();
            for (var i = 0; i < sources.Count; i++)
            {
                plan.Add(new ContextRetrievalSource
                {
                    Source = sources[i],
                    Query = "query-" + (i + 1),
                    MaxItems = 1
                });
            }

            var snapshot = await new ContextAcquirer().AcquireAsync(
                plan,
                new ContextBudget
                {
                    MaxItems = 7,
                    MaxCharacters = 1000,
                    MaxEstimatedTokens = 100
                });

            Assert.Equal(7, snapshot.Items.Count);
            Assert.Equal(
                "memory-1,knowledge-1,skill-1,conversation-1,host-1,tool-1,instruction-1",
                string.Join(",", GetIds(snapshot)));

            for (var i = 0; i < sources.Count; i++)
            {
                Assert.Equal("query-" + (i + 1), sources[i].LastRequest.Query);
                Assert.Equal(1, sources[i].LastRequest.MaxItems);
            }
        }

        [Fact]
        public async Task GlobalItemBudgetLimitsLaterSourcesAfterPerSourceLimits()
        {
            var first = new RecordingContextSource(ContextSourceKinds.Memory, "memory-1", "memory-2");
            var second = new RecordingContextSource(ContextSourceKinds.Knowledge, "knowledge-1");

            var snapshot = await new ContextAcquirer().AcquireAsync(
                new ContextRetrievalSource[]
                {
                    new ContextRetrievalSource { Source = first, Query = "memory", MaxItems = 5 },
                    new ContextRetrievalSource { Source = second, Query = "knowledge", MaxItems = 5 }
                },
                new ContextBudget
                {
                    MaxItems = 2,
                    MaxCharacters = 1000,
                    MaxEstimatedTokens = 100
                });

            Assert.Equal("memory-1,memory-2", string.Join(",", GetIds(snapshot)));
            Assert.Equal(2, first.LastRequest.MaxItems);
            Assert.Null(second.LastRequest);
        }

        [Fact]
        public async Task GlobalCharacterAndTokenBudgetsApplyAcrossSourceCategories()
        {
            var memory = new RecordingContextSource(ContextSourceKinds.Memory, CreateItem("memory", 60, 6));
            var knowledge = new RecordingContextSource(ContextSourceKinds.Knowledge, CreateItem("knowledge", 30, 3));

            var snapshot = await new ContextAcquirer().AcquireAsync(
                new ContextRetrievalSource[]
                {
                    new ContextRetrievalSource { Source = memory, Query = "memory", MaxItems = 1 },
                    new ContextRetrievalSource { Source = knowledge, Query = "knowledge", MaxItems = 1 }
                },
                new ContextBudget
                {
                    MaxItems = 2,
                    MaxCharacters = 50,
                    MaxEstimatedTokens = 5
                });

            Assert.Equal("knowledge", string.Join(",", GetIds(snapshot)));
            Assert.Equal(30, snapshot.UsedCharacters);
            Assert.Equal(3, snapshot.UsedEstimatedTokens);
        }

        [Fact]
        public async Task RetrievalSourceValidationRejectsUnboundedPerSourceRequests()
        {
            var source = new RecordingContextSource(ContextSourceKinds.Memory, "memory-1");
            var retrieval = new ContextRetrievalSource
            {
                Source = source,
                Query = new string('q', 4097),
                MaxItems = 1
            };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                new ContextAcquirer().AcquireAsync(
                    new[] { retrieval },
                    new ContextBudget()));
        }

        private static string[] GetIds(ContextSnapshot snapshot)
        {
            var ids = new List<string>();
            foreach (var item in snapshot.Items)
                ids.Add(item.Id);
            return ids.ToArray();
        }

        private static ContextItem CreateItem(string id, int characters, int tokens)
        {
            return new ContextItem
            {
                Id = id,
                Source = id + "-source",
                Type = "record",
                Payload = id + "-payload",
                Provenance = new ContextProvenance
                {
                    SourceKind = id,
                    SourceId = id + "-source",
                    SourceVersion = "1",
                    Evidence = "deterministic"
                },
                Scope = new ContextScope
                {
                    ScopeType = "Execution",
                    ScopeId = "test"
                },
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }

        private sealed class RecordingContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public RecordingContextSource(string kind, params string[] itemIds)
            {
                Id = kind + "-source";
                Kind = kind;
                var items = new List<ContextItem>();
                foreach (var itemId in itemIds)
                    items.Add(CreateItem(itemId, 10, 1));
                _items = items;
            }

            public RecordingContextSource(string kind, ContextItem item)
                : this(kind, item.Id)
            {
                _items = new[] { item };
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }
            public ContextSourceRequest LastRequest { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
                ContextSourceRequest request,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                request.Validate();
                LastRequest = request.Clone();
                return Task.FromResult(_items);
            }
        }
    }
}

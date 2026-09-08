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
    public class ContextAcquisitionTests
    {
        [Fact]
        public async Task AcquireAsync_EnforcesItemCharacterAndTokenBudgetsInSourceOrder()
        {
            var sources = new List<IContextSource>
            {
                new TestContextSource("first", "Host", new[]
                {
                    CreateItem("a", 40, 10),
                    CreateItem("too-large", 100, 10),
                    CreateItem("b", 30, 8)
                }),
                new TestContextSource("second", "Memory", new[]
                {
                    CreateItem("c", 30, 12),
                    CreateItem("d", 30, 12)
                })
            };

            var snapshot = await new ContextAcquirer().AcquireAsync(
                sources,
                new ContextSourceRequest { Query = "customer", MaxItems = 10 },
                new ContextBudget { MaxItems = 3, MaxCharacters = 100, MaxEstimatedTokens = 30 },
                CancellationToken.None);

            Assert.Equal(3, snapshot.UsedItems);
            Assert.Equal(100, snapshot.UsedCharacters);
            Assert.Equal(30, snapshot.UsedEstimatedTokens);
            Assert.Equal(0, snapshot.RemainingItems);
            Assert.Equal(0, snapshot.RemainingCharacters);
            Assert.Equal(0, snapshot.RemainingEstimatedTokens);

            var items = snapshot.Items;
            Assert.Equal("a", items[0].Id);
            Assert.Equal("b", items[1].Id);
            Assert.Equal("c", items[2].Id);
            Assert.Equal(2, snapshot.SourceCount);
        }

        [Fact]
        public async Task AcquireAsync_DoesNotAcceptUnknownTokenEstimatesWhenTokenBudgetIsHard()
        {
            var source = new TestContextSource("source", "Host", new[]
            {
                CreateItem("unknown", 10, null),
                CreateItem("known", 10, 5)
            });

            var snapshot = await new ContextAcquirer().AcquireAsync(
                new[] { source },
                new ContextSourceRequest { MaxItems = 10 },
                new ContextBudget { MaxItems = 10, MaxCharacters = 100, MaxEstimatedTokens = 10 },
                CancellationToken.None);

            var items = snapshot.Items;
            Assert.Single(items);
            Assert.Equal("known", items[0].Id);
            Assert.Equal(5, snapshot.UsedEstimatedTokens);
        }

        [Fact]
        public async Task AcquireAsync_ClonesRequestAndSnapshotItems()
        {
            var source = new MutatingContextSource(CreateItem("item-42", 10, 2));
            var request = new ContextSourceRequest { Query = "original", MaxItems = 5 };
            var item = source.Item;

            var snapshot = await new ContextAcquirer().AcquireAsync(
                new[] { source },
                request,
                new ContextBudget { MaxItems = 5, MaxCharacters = 100, MaxEstimatedTokens = 10 },
                CancellationToken.None);

            Assert.Equal("original", request.Query);
            Assert.Equal(5, request.MaxItems);
            Assert.Equal("item-42", item.Id);

            var firstRead = snapshot.Items;
            firstRead[0].Scope.ScopeId = "mutated-outside";
            firstRead[0].Provenance.Evidence = "mutated-outside";

            var secondRead = snapshot.Items;
            Assert.NotEqual("mutated-outside", secondRead[0].Scope.ScopeId);
            Assert.NotEqual("mutated-outside", secondRead[0].Provenance.Evidence);
        }

        [Fact]
        public async Task AcquireAsync_PropagatesCancellationBeforeNextSource()
        {
            var first = new TestContextSource("first", "Host", new[] { CreateItem("a", 1, 1) });
            var second = new CancellingContextSource("second");
            var cancellation = new CancellationTokenSource();
            cancellation.CancelAfter(25);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                new ContextAcquirer().AcquireAsync(
                    new[] { first, second },
                    new ContextSourceRequest { MaxItems = 10 },
                    new ContextBudget { MaxItems = 10, MaxCharacters = 100, MaxEstimatedTokens = 20 },
                    cancellation.Token));
        }

        private static ContextItem CreateItem(string id, int characters, int? tokens)
        {
            return new ContextItem
            {
                Id = id,
                Source = "Test",
                Type = "Record",
                Payload = id,
                Provenance = new ContextProvenance
                {
                    SourceKind = "Test",
                    SourceId = id,
                    SourceVersion = "1",
                    Evidence = "Deterministic test"
                },
                Scope = new ContextScope { ScopeType = "Global" },
                Trust = 1,
                Importance = 1,
                Relevance = 1,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }

        private sealed class TestContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public TestContextSource(string id, string kind, IReadOnlyList<ContextItem> items)
            {
                Id = id;
                Kind = kind;
                _items = items;
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(ContextSourceRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(_items);
            }
        }

        private sealed class MutatingContextSource : IContextSource
        {
            public MutatingContextSource(ContextItem item)
            {
                Item = item;
            }

            public ContextItem Item { get; private set; }
            public string Id { get { return "mutating"; } }
            public string Kind { get { return "Host"; } }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(ContextSourceRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                request.Query = "mutated";
                return Task.FromResult<IReadOnlyList<ContextItem>>(new[] { Item });
            }
        }

        private sealed class CancellingContextSource : IContextSource
        {
            private readonly string _id;

            public CancellingContextSource(string id)
            {
                _id = id;
            }

            public string Id { get { return _id; } }
            public string Kind { get { return "Test"; } }

            public async Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(ContextSourceRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                await Task.Delay(500, cancellationToken);
                return Array.Empty<ContextItem>();
            }
        }
    }
}

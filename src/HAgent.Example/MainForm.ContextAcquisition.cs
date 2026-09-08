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
        private void AddContextAcquisitionTab()
        {
            AddApiTab(
                "Context Acquisition",
                "Run acquisition test",
                "Acquires deterministic candidates from two provider-neutral sources, enforces item/character/token budgets, and verifies the resulting snapshot is isolated from caller-owned mutable state.",
                "Three items should be selected in source order, the oversized item should be excluded, all three budget dimensions should remain within hard bounds, and snapshot reads must be isolated.",
                "customer 42",
                RunContextAcquisitionTestAsync,
                "Acquisition boundary",
                "No AI request is sent. Ranking, deduplication, compaction, caching, and provider transport remain later slices.");
        }

        private async Task RunContextAcquisitionTestAsync(string query)
        {
            var sources = new List<IContextSource>
            {
                new ExampleAcquisitionContextSource("host", "Host", new[]
                {
                    CreateAcquisitionItem("host-1", "Host customer observation", 40, 10),
                    CreateAcquisitionItem("host-too-large", "Host oversized observation", 100, 10),
                    CreateAcquisitionItem("host-2", "Host customer state", 30, 8)
                }),
                new ExampleAcquisitionContextSource("memory", "Memory", new[]
                {
                    CreateAcquisitionItem("memory-1", "Memory evidence", 30, 12),
                    CreateAcquisitionItem("memory-2", "Memory overflow", 30, 12)
                })
            };

            var budget = new ContextBudget
            {
                MaxItems = 3,
                MaxCharacters = 100,
                MaxEstimatedTokens = 30
            };

            var snapshot = await new ContextAcquirer().AcquireAsync(
                sources,
                new ContextSourceRequest
                {
                    Query = string.IsNullOrWhiteSpace(query) ? "customer 42" : query,
                    MaxItems = 10
                },
                budget,
                CancellationToken.None);

            if (snapshot.UsedItems != 3 ||
                snapshot.UsedCharacters != 100 ||
                snapshot.UsedEstimatedTokens != 30 ||
                snapshot.RemainingItems != 0 ||
                snapshot.RemainingCharacters != 0 ||
                snapshot.RemainingEstimatedTokens != 0)
                throw new InvalidOperationException("Context acquisition did not respect all configured hard budget dimensions.");

            var firstRead = snapshot.Items;
            if (firstRead.Count != 3 ||
                firstRead[0].Id != "host-1" ||
                firstRead[1].Id != "host-2" ||
                firstRead[2].Id != "memory-1")
                throw new InvalidOperationException("Context acquisition order or bounded selection was not deterministic.");

            firstRead[0].Scope.ScopeId = "mutated-outside";
            firstRead[0].Provenance.Evidence = "mutated-outside";
            var secondRead = snapshot.Items;
            if (secondRead[0].Scope.ScopeId == "mutated-outside" || secondRead[0].Provenance.Evidence == "mutated-outside")
                throw new InvalidOperationException("Context snapshot leaked mutable item state to the caller.");

            Write(
                "CONTEXT ACQUISITION",
                "Bounded context acquisition and snapshot creation succeeded." + Environment.NewLine +
                "Selected items: " + snapshot.UsedItems + Environment.NewLine +
                "Selected order: " + secondRead[0].Id + ", " + secondRead[1].Id + ", " + secondRead[2].Id + Environment.NewLine +
                "Characters used / limit: " + snapshot.UsedCharacters + " / " + snapshot.Budget.MaxCharacters + Environment.NewLine +
                "Estimated tokens used / limit: " + snapshot.UsedEstimatedTokens + " / " + snapshot.Budget.MaxEstimatedTokens + Environment.NewLine +
                "Items used / limit: " + snapshot.UsedItems + " / " + snapshot.Budget.MaxItems + Environment.NewLine +
                "Oversized candidate exclusion: verified." + Environment.NewLine +
                "Caller mutation isolation: verified." + Environment.NewLine +
                "Provider request: none.");
        }

        private static ContextItem CreateAcquisitionItem(string id, string payload, int characters, int tokens)
        {
            return new ContextItem
            {
                Id = id,
                Source = "Example",
                Type = "Observation",
                Payload = payload,
                Provenance = new ContextProvenance
                {
                    SourceKind = "Example",
                    SourceId = id,
                    SourceVersion = "1",
                    Evidence = "Deterministic context acquisition example"
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 1d,
                Importance = 0.8d,
                Relevance = 1d,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }

        private sealed class ExampleAcquisitionContextSource : IContextSource
        {
            private readonly IReadOnlyList<ContextItem> _items;

            public ExampleAcquisitionContextSource(string id, string kind, IReadOnlyList<ContextItem> items)
            {
                Id = id;
                Kind = kind;
                _items = items;
            }

            public string Id { get; private set; }
            public string Kind { get; private set; }

            public Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(ContextSourceRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_items);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddContextRankingTab()
        {
            AddApiTab(
                "Context Ranking",
                "Run ranking test",
                "Ranks deterministic provider-neutral context candidates using relevance, importance, trust, freshness, and estimated cost, then removes duplicate IDs while preserving the highest-ranked candidate.",
                "The expected order should be high, medium, low; equal-score ties must sort deterministically by ID; duplicate IDs must retain the highest-ranked copy and preserve provenance/scope metadata.",
                "customer 42",
                RunContextRankingTestAsync,
                "Ranking boundary",
                "No AI request is sent. Compaction, caching, policy filtering, and provider transport remain later slices.");
        }

        private Task RunContextRankingTestAsync(string unused)
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateRankingItem("low", 0.2d, 0.2d, 0.2d, reference.AddDays(-20)),
                CreateRankingItem("high", 1d, 0.8d, 0.9d, reference.AddDays(-1)),
                CreateRankingItem("medium", 0.7d, 0.7d, 0.7d, reference.AddDays(-5)),
                CreateRankingItem("high", 0.4d, 0.4d, 0.4d, reference.AddDays(-10))
            };

            var ranked = new ContextRanker().Rank(candidates, new ContextRankingOptions
            {
                FreshnessReference = reference,
                MaxFreshnessAge = TimeSpan.FromDays(30)
            });

            if (ranked.Count != 3 ||
                ranked[0].Id != "high" ||
                ranked[1].Id != "medium" ||
                ranked[2].Id != "low" ||
                ranked[0].Relevance != 1d ||
                ranked[0].Provenance == null ||
                ranked[0].Scope == null)
                throw new InvalidOperationException("Context ranking order or duplicate retention was not deterministic.");

            var tieCandidates = new[]
            {
                CreateRankingItem("z", 1d, 1d, 1d, reference),
                CreateRankingItem("a", 1d, 1d, 1d, reference),
                CreateRankingItem("m", 1d, 1d, 1d, reference)
            };
            var ties = new ContextRanker().Rank(tieCandidates, new ContextRankingOptions { FreshnessReference = reference });
            if (ties.Count != 3 || ties[0].Id != "a" || ties[1].Id != "m" || ties[2].Id != "z")
                throw new InvalidOperationException("Equal-score context ranking did not use stable deterministic tie-breaking.");

            Write(
                "CONTEXT RANKING",
                "Deterministic context ranking and deduplication succeeded." + Environment.NewLine +
                "Ranked order: " + string.Join(", ", GetIds(ranked)) + Environment.NewLine +
                "Duplicate high candidate retained: verified." + Environment.NewLine +
                "Deterministic equal-score tie order: " + string.Join(", ", GetIds(ties)) + Environment.NewLine +
                "Provenance/scope metadata preserved: verified." + Environment.NewLine +
                "Provider request: none.");

            return Task.CompletedTask;
        }

        private static ContextItem CreateRankingItem(string id, double relevance, double importance, double trust, DateTimeOffset capturedAt)
        {
            return new ContextItem
            {
                Id = id,
                Source = "Example",
                Type = "Observation",
                Payload = id,
                Provenance = new ContextProvenance
                {
                    SourceKind = "Example",
                    SourceId = id,
                    SourceVersion = "1",
                    Evidence = "Deterministic ranking example",
                    CapturedAt = capturedAt
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = trust,
                Importance = importance,
                Relevance = relevance,
                CapturedAt = capturedAt,
                EstimatedCharacters = id == "high" ? 10 : 20,
                EstimatedTokens = id == "high" ? 2 : 5
            };
        }

        private static IEnumerable<string> GetIds(IReadOnlyList<ContextItem> items)
        {
            for (var i = 0; i < items.Count; i++)
                yield return items[i].Id;
        }
    }
}

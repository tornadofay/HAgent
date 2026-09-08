using System;
using System.Collections.Generic;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class ContextRankingTests
    {
        [Fact]
        public void Rank_PrioritizesWeightedEvidenceAndPreservesMetadata()
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("low", 0.2d, 0.2d, 0.2d, reference.AddDays(-20), 100, 25),
                CreateItem("high", 1d, 0.8d, 0.9d, reference.AddDays(-1), 20, 5),
                CreateItem("medium", 0.7d, 0.7d, 0.7d, reference.AddDays(-5), 40, 10)
            };

            var ranked = new ContextRanker().Rank(candidates, new ContextRankingOptions
            {
                FreshnessReference = reference,
                MaxFreshnessAge = TimeSpan.FromDays(30)
            });

            Assert.Equal("high", ranked[0].Id);
            Assert.Equal("medium", ranked[1].Id);
            Assert.Equal("low", ranked[2].Id);
            Assert.Equal("Test", ranked[0].Source);
            Assert.Equal("Record", ranked[0].Type);
            Assert.NotNull(ranked[0].Provenance);
            Assert.NotNull(ranked[0].Scope);
        }

        [Fact]
        public void Rank_UsesStableTieBreakingForEqualScores()
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("z", 1d, 1d, 1d, reference, 10, 2),
                CreateItem("a", 1d, 1d, 1d, reference, 10, 2),
                CreateItem("m", 1d, 1d, 1d, reference, 10, 2)
            };

            var first = new ContextRanker().Rank(candidates, new ContextRankingOptions { FreshnessReference = reference });
            var second = new ContextRanker().Rank(new[] { candidates[2], candidates[0], candidates[1] }, new ContextRankingOptions { FreshnessReference = reference });

            Assert.Equal(new[] { "a", "m", "z" }, new[] { first[0].Id, first[1].Id, first[2].Id });
            Assert.Equal(new[] { "a", "m", "z" }, new[] { second[0].Id, second[1].Id, second[2].Id });
        }

        [Fact]
        public void Rank_RemovesDuplicateIdsAndRetainsHighestRankedCopy()
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var weakDuplicate = CreateItem("same", 0.2d, 0.2d, 0.2d, reference.AddDays(-10), 50, 12);
            var strongDuplicate = CreateItem("same", 1d, 1d, 1d, reference, 20, 5);
            var other = CreateItem("other", 0.6d, 0.6d, 0.6d, reference.AddDays(-2), 30, 8);

            var ranked = new ContextRanker().Rank(
                new[] { weakDuplicate, other, strongDuplicate },
                new ContextRankingOptions { FreshnessReference = reference });

            Assert.Equal(2, ranked.Count);
            Assert.Equal("same", ranked[0].Id);
            Assert.Equal(1d, ranked[0].Relevance);
            Assert.Equal("other", ranked[1].Id);
        }

        [Fact]
        public void Rank_CanDisableDeduplication()
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("same", 1d, 1d, 1d, reference, 10, 2),
                CreateItem("same", 0.8d, 0.8d, 0.8d, reference, 20, 5)
            };

            var ranked = new ContextRanker().Rank(candidates, new ContextRankingOptions
            {
                FreshnessReference = reference,
                Deduplicate = false
            });

            Assert.Equal(2, ranked.Count);
            Assert.Equal("same", ranked[0].Id);
            Assert.Equal(1d, ranked[0].Relevance);
        }

        private static ContextItem CreateItem(string id, double relevance, double importance, double trust, DateTimeOffset capturedAt, int characters, int tokens)
        {
            return new ContextItem
            {
                Id = id,
                Source = "Test",
                Type = "Record",
                Payload = new Dictionary<string, object> { { "Id", id } },
                Provenance = new ContextProvenance
                {
                    SourceKind = "Test",
                    SourceId = id,
                    SourceVersion = "1",
                    Evidence = "Deterministic ranking test",
                    CapturedAt = capturedAt
                },
                Scope = new ContextScope { ScopeType = "Global" },
                Trust = trust,
                Importance = importance,
                Relevance = relevance,
                CapturedAt = capturedAt,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }
    }
}

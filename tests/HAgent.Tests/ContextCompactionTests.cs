using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ContextCompactionTests
    {
        [Fact]
        public void Compact_SelectsRankedItemsWithinAllBudgetsAndPreservesMetadata()
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("first", "SourceA", 0.9d, 40, 8, reference),
                CreateItem("second", "SourceB", 0.8d, 30, 6, reference),
                CreateItem("third", "SourceC", 0.7d, 30, 6, reference)
            };

            var result = new ContextCompactor().Compact(candidates, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 2,
                    MaxCharacters = 70,
                    MaxEstimatedTokens = 14
                }
            });

            Assert.Equal(new[] { "first", "second" }, result.Snapshot.Items.Select(x => x.Id).ToArray());
            Assert.Equal(2, result.Snapshot.UsedItems);
            Assert.Equal(70, result.Snapshot.UsedCharacters);
            Assert.Equal(14, result.Snapshot.UsedEstimatedTokens);
            Assert.Equal(2, result.Snapshot.SourceCount);
            Assert.Equal(ContextCompactionDecisionReason.Included, result.Decisions[0].Reason);
            Assert.Equal(ContextCompactionDecisionReason.Included, result.Decisions[1].Reason);
            Assert.Equal("SourceA", result.Snapshot.Items[0].Source);
            Assert.Equal("SourceA-item", result.Snapshot.Items[0].Provenance.SourceId);
            Assert.Equal("user-42", result.Snapshot.Items[0].Scope.ScopeId);
        }

        [Fact]
        public void Compact_RecordsCharacterTokenAndUnknownEstimateReasons()
        {
            var now = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("fits", "A", 1d, 10, 2, now),
                CreateItem("too-large", "B", 0.9d, 95, 2, now),
                CreateItem("fits-token", "C", 0.8d, 10, 8, now),
                CreateItem("too-many-tokens", "D", 0.75d, 10, 9, now),
                CreateItem("unknown-tokens", "E", 0.7d, 10, null, now)
            };

            var result = new ContextCompactor().Compact(candidates, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 10,
                    MaxCharacters = 100,
                    MaxEstimatedTokens = 10
                }
            });

            Assert.Equal(new[] { "fits", "fits-token" }, result.Decisions
                .Where(x => x.Included)
                .Select(x => x.ItemId)
                .ToArray());
            Assert.Equal(ContextCompactionDecisionReason.CharacterBudgetExceeded, result.Decisions[1].Reason);
            Assert.Equal(ContextCompactionDecisionReason.Included, result.Decisions[2].Reason);
            Assert.Equal(ContextCompactionDecisionReason.TokenBudgetExceeded, result.Decisions[3].Reason);
            Assert.Equal(ContextCompactionDecisionReason.UnknownTokenEstimate, result.Decisions[4].Reason);
            Assert.Equal(10, result.Snapshot.UsedEstimatedTokens);
        }

        [Fact]
        public void Compact_CanStopAtFirstExcludedCandidateDeterministically()
        {
            var now = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("too-large", "A", 1d, 90, 11, now),
                CreateItem("later-fit", "B", 0.9d, 10, 1, now)
            };

            var result = new ContextCompactor().Compact(candidates, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 5,
                    MaxCharacters = 20,
                    MaxEstimatedTokens = 10
                },
                ContinueAfterExcludedCandidate = false
            });

            Assert.Empty(result.Snapshot.Items);
            Assert.True(result.WasTruncated);
            Assert.Single(result.Decisions);
            Assert.Equal(ContextCompactionDecisionReason.CharacterAndTokenBudgetExceeded, result.Decisions[0].Reason);
        }

        [Fact]
        public void Compact_WhenDiagnosticsDisabledStillProducesSnapshotAndCorrectCounts()
        {
            var now = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateItem("first", "A", 1d, 10, 2, now),
                CreateItem("second", "B", 0.9d, 90, 9, now)
            };

            var result = new ContextCompactor().Compact(candidates, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 5,
                    MaxCharacters = 20,
                    MaxEstimatedTokens = 10
                },
                CaptureDiagnostics = false
            });

            Assert.Single(result.Snapshot.Items);
            Assert.Equal("first", result.Snapshot.Items[0].Id);
            Assert.Empty(result.Decisions);
            Assert.Equal(1, result.IncludedCount);
            Assert.Equal(1, result.ExcludedCount);
        }

        [Fact]
        public void Compact_ClonesItemMetadataAndDiagnosticsDoNotExposePayload()
        {
            var now = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var source = CreateItem("secret", "A", 1d, 10, 2, now);
            source.Payload = new Dictionary<string, object> { { "secret", "value" } };

            var result = new ContextCompactor().Compact(new[] { source }, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 1,
                    MaxCharacters = 10,
                    MaxEstimatedTokens = 2
                }
            });

            source.Id = "mutated";
            Assert.Equal("secret", result.Snapshot.Items[0].Id);
            Assert.Equal("value", ((Dictionary<string, object>)result.Snapshot.Items[0].Payload)["secret"]);
            Assert.Equal("secret", result.Decisions[0].ItemId);
            Assert.DoesNotContain("value", result.Decisions[0].ItemId);
        }

        private static ContextItem CreateItem(
            string id,
            string source,
            double relevance,
            int characters,
            int? tokens,
            DateTimeOffset capturedAt)
        {
            return new ContextItem
            {
                Id = id,
                Source = source,
                Type = "Observation",
                Payload = id,
                Provenance = new ContextProvenance
                {
                    SourceKind = "Test",
                    SourceId = source + "-item",
                    SourceVersion = "1",
                    Evidence = "Deterministic compaction test",
                    CapturedAt = capturedAt
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 0.9d,
                Importance = relevance,
                Relevance = relevance,
                CapturedAt = capturedAt,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }
    }
}

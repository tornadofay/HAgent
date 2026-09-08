using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddContextCompactionTab()
        {
            AddApiTab(
                "Context Compaction",
                "Run compaction test",
                "Compacts already ranked context candidates to explicit item, character, and optional token budgets while preserving selected item metadata and emitting safe decision diagnostics.",
                "Expected results: high-priority candidates that fit are retained; oversized candidates are excluded with deterministic reasons; unknown token estimates are rejected under a hard token budget; provenance and scope survive in the bounded snapshot.",
                "Compaction boundary",
                RunContextCompactionTestAsync,
                "Deterministic truncation",
                "No payload summarization, provider tokenizer, persistence, or AI request is used in this slice.");
        }

        private Task RunContextCompactionTestAsync(string unused)
        {
            var reference = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);
            var candidates = new[]
            {
                CreateCompactionItem("high", "Host", 40, 6, reference),
                CreateCompactionItem("medium", "Memory", 30, 4, reference),
                CreateCompactionItem("too-large", "Knowledge", 50, 1, reference),
                CreateCompactionItem("unknown-tokens", "Tool", 5, null, reference)
            };

            var result = new ContextCompactor().Compact(candidates, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 3,
                    MaxCharacters = 75,
                    MaxEstimatedTokens = 10
                }
            });

            var items = result.Snapshot.Items;
            if (items.Count != 2 ||
                items[0].Id != "high" ||
                items[1].Id != "medium" ||
                result.Snapshot.UsedCharacters != 70 ||
                result.Snapshot.UsedEstimatedTokens != 10 ||
                result.ExcludedCount != 2 ||
                result.Decisions.Count != 4)
                throw new InvalidOperationException("Context compaction did not produce the expected bounded snapshot.");

            if (result.Decisions[2].Reason != ContextCompactionDecisionReason.CharacterAndTokenBudgetExceeded ||
                result.Decisions[3].Reason != ContextCompactionDecisionReason.UnknownTokenEstimate)
                throw new InvalidOperationException("Context compaction diagnostics did not explain exclusions deterministically.");

            if (items[0].Provenance == null ||
                items[0].Provenance.SourceId != "Host-high" ||
                items[0].Scope == null ||
                items[0].Scope.ScopeId != "user-42")
                throw new InvalidOperationException("Selected context provenance or scope metadata was not preserved.");

            var itemLimited = new ContextCompactor().Compact(candidates, new ContextCompactionOptions
            {
                TargetBudget = new ContextBudget
                {
                    MaxItems = 1,
                    MaxCharacters = 75,
                    MaxEstimatedTokens = 10
                }
            });

            if (itemLimited.Snapshot.Items.Count != 1 ||
                itemLimited.Snapshot.Items[0].Id != "high" ||
                itemLimited.Decisions[1].Reason != ContextCompactionDecisionReason.ItemBudgetExhausted)
                throw new InvalidOperationException("Context compaction did not enforce the item budget deterministically.");

            Write(
                "CONTEXT COMPACTION",
                "Deterministic context compaction and provenance-preserving diagnostics succeeded." + Environment.NewLine +
                "Selected order: " + string.Join(", ", items.Select(x => x.Id)) + Environment.NewLine +
                "Characters used / limit: " + result.Snapshot.UsedCharacters + " / " + result.Snapshot.Budget.MaxCharacters + Environment.NewLine +
                "Estimated tokens used / limit: " + result.Snapshot.UsedEstimatedTokens + " / " + result.Snapshot.Budget.MaxEstimatedTokens + Environment.NewLine +
                "Excluded: too-large + unknown-tokens, with explicit diagnostics." + Environment.NewLine +
                "Provenance/scope preservation: verified." + Environment.NewLine +
                "Payload summarization/provider tokenizer: none." + Environment.NewLine +
                "Provider request: none.");

            return Task.CompletedTask;
        }

        private static ContextItem CreateCompactionItem(
            string id,
            string source,
            int characters,
            int? tokens,
            DateTimeOffset capturedAt)
        {
            return new ContextItem
            {
                Id = id,
                Source = source,
                Type = "Observation",
                Payload = new Dictionary<string, object>
                {
                    { "Id", id },
                    { "Source", source }
                },
                Provenance = new ContextProvenance
                {
                    SourceKind = source,
                    SourceId = source + "-" + id,
                    SourceVersion = "1",
                    Evidence = "Deterministic compaction example",
                    CapturedAt = capturedAt
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 0.9d,
                Importance = 0.9d,
                Relevance = id == "high" ? 1d : 0.8d,
                CapturedAt = capturedAt,
                EstimatedCharacters = characters,
                EstimatedTokens = tokens
            };
        }
    }
}

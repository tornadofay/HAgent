using System;
using System.Collections.Generic;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Deterministically compacts already ranked context candidates to a target budget.
    /// It never rewrites payloads or invents provider-specific token counts.
    /// </summary>
    public sealed class ContextCompactor : IContextCompactor
    {
        public ContextCompactionResult Compact(
            IReadOnlyList<ContextItem> rankedCandidates,
            ContextCompactionOptions options = null)
        {
            if (rankedCandidates == null) throw new ArgumentNullException(nameof(rankedCandidates));

            var effectiveOptions = options == null
                ? new ContextCompactionOptions()
                : options.Clone();
            effectiveOptions.Validate();

            var budget = effectiveOptions.TargetBudget;
            var selected = new List<ContextItem>();
            var decisions = new List<ContextCompactionDecision>(rankedCandidates.Count);
            var usedCharacters = 0;
            var usedTokens = 0;
            var allTokensKnown = true;
            var stop = false;

            for (var index = 0; index < rankedCandidates.Count; index++)
            {
                var candidate = rankedCandidates[index];
                if (candidate == null)
                    throw new InvalidOperationException("Ranked context candidates cannot contain null items.");

                candidate.Validate();
                var outputIndex = -1;
                var included = false;
                var reason = ContextCompactionDecisionReason.Included;

                var usedItemsBefore = selected.Count;
                var usedCharactersBefore = usedCharacters;
                var usedTokensBefore = allTokensKnown ? (int?)usedTokens : null;

                if (stop || selected.Count >= budget.MaxItems)
                {
                    reason = ContextCompactionDecisionReason.ItemBudgetExhausted;
                    stop = true;
                }
                else if (budget.MaxEstimatedTokens.HasValue && !candidate.EstimatedTokens.HasValue)
                {
                    reason = ContextCompactionDecisionReason.UnknownTokenEstimate;
                }
                else
                {
                    var characterExceeded = candidate.EstimatedCharacters > budget.MaxCharacters - usedCharacters;
                    var tokenExceeded = budget.MaxEstimatedTokens.HasValue &&
                        candidate.EstimatedTokens.Value > budget.MaxEstimatedTokens.Value - usedTokens;

                    if (characterExceeded && tokenExceeded)
                        reason = ContextCompactionDecisionReason.CharacterAndTokenBudgetExceeded;
                    else if (characterExceeded)
                        reason = ContextCompactionDecisionReason.CharacterBudgetExceeded;
                    else if (tokenExceeded)
                        reason = ContextCompactionDecisionReason.TokenBudgetExceeded;
                    else
                    {
                        var item = candidate.Clone();
                        item.Validate();
                        selected.Add(item);
                        outputIndex = selected.Count - 1;
                        included = true;
                        usedCharacters += item.EstimatedCharacters;

                        if (item.EstimatedTokens.HasValue)
                            usedTokens += item.EstimatedTokens.Value;
                        else
                            allTokensKnown = false;
                    }
                }

                if (effectiveOptions.CaptureDiagnostics)
                {
                    decisions.Add(new ContextCompactionDecision(
                        index,
                        outputIndex,
                        candidate.Id,
                        candidate.Source,
                        included,
                        reason,
                        candidate.EstimatedCharacters,
                        candidate.EstimatedTokens,
                        usedItemsBefore,
                        usedCharactersBefore,
                        usedTokensBefore));
                }

                if (!included && !effectiveOptions.ContinueAfterExcludedCandidate)
                    stop = true;
            }

            var usedEstimatedTokens = allTokensKnown ? (int?)usedTokens : null;
            var sourceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in selected)
                sourceIds.Add(item.Source);

            var snapshot = new ContextSnapshot(
                selected,
                budget,
                selected.Count,
                usedCharacters,
                usedEstimatedTokens,
                sourceIds.Count);

            return new ContextCompactionResult(snapshot, decisions, rankedCandidates.Count);
        }
    }
}

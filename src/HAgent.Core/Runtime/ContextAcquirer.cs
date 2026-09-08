using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Deterministically acquires source candidates in supplied source order and bounds the resulting snapshot.
    /// Ranking, deduplication, compaction, policy evaluation, and provider transport are intentionally outside this slice.
    /// </summary>
    public sealed class ContextAcquirer : IContextAcquirer
    {
        public async Task<ContextSnapshot> AcquireAsync(
            IReadOnlyList<IContextSource> sources,
            ContextSourceRequest request,
            ContextBudget budget,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (budget == null) throw new ArgumentNullException(nameof(budget));

            request.Validate();
            budget.Validate();

            var sourceCopy = new List<IContextSource>(sources.Count);
            foreach (var source in sources)
            {
                if (source == null)
                    throw new ArgumentException("Context source collection cannot contain null sources.", nameof(sources));
                if (string.IsNullOrWhiteSpace(source.Id))
                    throw new ArgumentException("Context source ID is required.", nameof(sources));
                if (source.Id.Length > 256)
                    throw new ArgumentOutOfRangeException(nameof(sources), "Context source ID is too long.");
                if (string.IsNullOrWhiteSpace(source.Kind))
                    throw new ArgumentException("Context source kind is required.", nameof(sources));
                if (source.Kind.Length > 128)
                    throw new ArgumentOutOfRangeException(nameof(sources), "Context source kind is too long.");
                sourceCopy.Add(source);
            }

            var selected = new List<ContextItem>();
            var usedCharacters = 0;
            var usedTokens = 0;
            var allTokensKnown = true;

            foreach (var source in sourceCopy)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (selected.Count >= budget.MaxItems)
                    break;

                var remainingItems = budget.MaxItems - selected.Count;
                var sourceRequest = request.Clone();
                sourceRequest.MaxItems = Math.Min(sourceRequest.MaxItems, remainingItems);
                var candidates = await source.GetCandidatesAsync(sourceRequest, cancellationToken).ConfigureAwait(false);
                if (candidates == null)
                    throw new InvalidOperationException("A context source returned a null candidate collection.");

                foreach (var candidate in candidates)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (selected.Count >= budget.MaxItems)
                        break;
                    if (candidate == null)
                        throw new InvalidOperationException("A context source returned a null context item.");

                    candidate.Validate();
                    var item = candidate.Clone();
                    item.Validate();

                    if (item.EstimatedCharacters > budget.MaxCharacters - usedCharacters)
                        continue;

                    if (budget.MaxEstimatedTokens.HasValue)
                    {
                        if (!item.EstimatedTokens.HasValue)
                            continue;
                        if (item.EstimatedTokens.Value > budget.MaxEstimatedTokens.Value - usedTokens)
                            continue;
                    }

                    selected.Add(item);
                    usedCharacters += item.EstimatedCharacters;
                    if (item.EstimatedTokens.HasValue)
                        usedTokens += item.EstimatedTokens.Value;
                    else
                        allTokensKnown = false;
                }
            }

            var usedEstimatedTokens = allTokensKnown ? (int?)usedTokens : null;
            return new ContextSnapshot(
                selected,
                budget,
                selected.Count,
                usedCharacters,
                usedEstimatedTokens,
                sourceCopy.Count);
        }
    }
}

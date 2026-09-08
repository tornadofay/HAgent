using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Deterministic provider-neutral context ranker.
    /// </summary>
    public sealed class ContextRanker : IContextRanker
    {
        public IReadOnlyList<ContextItem> Rank(
            IReadOnlyList<ContextItem> candidates,
            ContextRankingOptions options = null)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            var effective = options == null ? new ContextRankingOptions() : options.Clone();
            effective.Validate();

            var ranked = candidates
                .Where(x => x != null)
                .Select((item, index) => new RankedItem(item, index, Score(item, effective)))
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.Relevance)
                .ThenByDescending(x => x.Item.Importance)
                .ThenByDescending(x => x.Item.Trust)
                .ThenByDescending(x => x.Item.CapturedAt)
                .ThenBy(x => x.Item.EstimatedCharacters)
                .ThenBy(x => x.Item.Id, StringComparer.Ordinal)
                .ThenBy(x => x.Item.Source, StringComparer.Ordinal)
                .ThenBy(x => x.Item.Type, StringComparer.Ordinal)
                .ThenBy(x => x.OriginalIndex)
                .ToList();

            var result = new List<ContextItem>(ranked.Count);
            var seen = effective.Deduplicate
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : null;

            foreach (var rankedItem in ranked)
            {
                if (seen != null && !seen.Add(rankedItem.Item.Id))
                    continue;

                var clone = rankedItem.Item.Clone();
                clone.Validate();
                result.Add(clone);
            }

            return new ReadOnlyCollection<ContextItem>(result);
        }

        private static double Score(ContextItem item, ContextRankingOptions options)
        {
            var freshness = FreshnessScore(item.CapturedAt, options.FreshnessReference, options.MaxFreshnessAge);
            var cost = CostScore(item.EstimatedCharacters, item.EstimatedTokens);
            var total = options.RelevanceWeight +
                        options.ImportanceWeight +
                        options.TrustWeight +
                        options.FreshnessWeight +
                        options.CostWeight;

            return (item.Relevance * options.RelevanceWeight +
                    item.Importance * options.ImportanceWeight +
                    item.Trust * options.TrustWeight +
                    freshness * options.FreshnessWeight +
                    cost * options.CostWeight) / total;
        }

        private static double FreshnessScore(DateTimeOffset capturedAt, DateTimeOffset reference, TimeSpan maxAge)
        {
            if (capturedAt >= reference)
                return 1d;

            var age = reference - capturedAt;
            if (age >= maxAge)
                return 0d;

            return 1d - (age.TotalMilliseconds / maxAge.TotalMilliseconds);
        }

        private static double CostScore(int characters, int? tokens)
        {
            var effective = tokens.HasValue
                ? Math.Max(1d, tokens.Value)
                : Math.Max(1d, characters / 4d);
            return 1d / (1d + Math.Log10(effective));
        }

        private sealed class RankedItem
        {
            public RankedItem(ContextItem item, int originalIndex, double score)
            {
                Item = item;
                OriginalIndex = originalIndex;
                Score = score;
            }

            public ContextItem Item { get; private set; }
            public int OriginalIndex { get; private set; }
            public double Score { get; private set; }
        }
    }
}

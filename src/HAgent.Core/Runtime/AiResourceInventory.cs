using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiResourceInventory : IAiResourceInventory
    {
        private readonly IReadOnlyList<IAiResourceInventorySource> _sources;

        public AiResourceInventory(IEnumerable<IAiResourceInventorySource> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            _sources = sources.Where(x => x != null).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(
            AiResourceInventoryQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query == null ? new AiResourceInventoryQuery() : query.Clone();
            query.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            var all = new List<AiResourceInventoryItem>();
            foreach (var source in _sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var items = await source.ListAsync(query.Clone(), cancellationToken).ConfigureAwait(false);
                if (items == null) continue;

                foreach (var item in items)
                {
                    if (item == null) continue;
                    item.Validate();
                    if (!Matches(item, query)) continue;
                    all.Add(item.Clone());
                }
            }

            var result = all
                .GroupBy(BuildIdentity, StringComparer.OrdinalIgnoreCase)
                .Select(group => group
                    .OrderByDescending(x => x.Version.HasValue ? 1 : 0)
                    .ThenByDescending(x => x.Version ?? 0)
                    .ThenByDescending(x => x.IsAuthoritative)
                    .ThenByDescending(x => x.UpdatedUtc)
                    .First())
                .OrderBy(x => x.ResourceType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.DisplayName ?? x.ResourceId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.ResourceId, StringComparer.OrdinalIgnoreCase)
                .Take(query.MaxResults)
                .Select(x => x.Clone())
                .ToList();

            return result.AsReadOnly();
        }

        private static bool Matches(AiResourceInventoryItem item, AiResourceInventoryQuery query)
        {
            if (query.ResourceTypes.Count > 0 && !query.ResourceTypes.Contains(item.ResourceType, StringComparer.OrdinalIgnoreCase))
                return false;
            if (query.Scope.HasValue && item.Scope != query.Scope.Value)
                return false;
            if (!string.IsNullOrWhiteSpace(query.OwnerId) && !string.Equals(item.OwnerId, query.OwnerId, StringComparison.OrdinalIgnoreCase))
                return false;
            if (query.AuthoritativeOnly && !item.IsAuthoritative)
                return false;
            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var search = query.SearchText.Trim();
                if ((item.DisplayName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0 &&
                    (item.ResourceId ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }
            return true;
        }

        private static string BuildIdentity(AiResourceInventoryItem item)
        {
            return (item.ResourceType ?? string.Empty) + "\n" +
                   (item.ResourceId ?? string.Empty) + "\n" +
                   item.Scope + "\n" +
                   (item.OwnerId ?? string.Empty);
        }
    }
}

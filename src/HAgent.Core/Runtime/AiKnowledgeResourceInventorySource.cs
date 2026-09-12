using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiKnowledgeResourceInventorySource : IAiResourceInventorySource
    {
        public const string KnowledgeResourceType = "knowledge";
        public const string WikiResourceType = "wiki";

        private readonly IAiKnowledgeResourceSource _source;
        private readonly int _sourceLimit;

        public AiKnowledgeResourceInventorySource(IAiKnowledgeResourceSource source, int sourceLimit = 1000)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            if (sourceLimit < 1 || sourceLimit > 1000)
                throw new ArgumentException("Knowledge inventory source limit must be between 1 and 1000.", nameof(sourceLimit));
            _sourceLimit = sourceLimit;
        }

        public async Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(
            AiResourceInventoryQuery query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            query = query == null ? new AiResourceInventoryQuery() : query.Clone();
            query.Validate();
            cancellationToken.ThrowIfCancellationRequested();

            if (query.ResourceTypes.Count > 0 &&
                !query.ResourceTypes.Any(t => string.Equals(t, KnowledgeResourceType, StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(t, WikiResourceType, StringComparison.OrdinalIgnoreCase)))
                return Empty();

            if (query.Version.HasValue && query.Version.Value <= 0)
                return Empty();

            var sourceQuery = new AiKnowledgeEnumerationQuery
            {
                Scope = query.Scope,
                OwnerId = query.OwnerId,
                Version = query.Version,
                UpdatedAfterUtc = query.UpdatedAfterUtc.HasValue ? query.UpdatedAfterUtc.Value.UtcDateTime : (DateTime?)null,
                UpdatedBeforeUtc = query.UpdatedBeforeUtc.HasValue ? query.UpdatedBeforeUtc.Value.UtcDateTime : (DateTime?)null,
                SearchText = query.SearchText,
                AuthoritativeOnly = query.AuthoritativeOnly,
                MaxResults = Math.Min(query.MaxResults, _sourceLimit)
            };

            if (!string.IsNullOrWhiteSpace(query.LifecycleStatus))
            {
                AiKnowledgeLifecycleStatus status;
                if (!Enum.TryParse(query.LifecycleStatus.Trim(), true, out status))
                    return Empty();
                sourceQuery.Status = status;
            }

            if (query.ResourceTypes.Count == 1)
            {
                var requestedType = query.ResourceTypes[0];
                if (string.Equals(requestedType, KnowledgeResourceType, StringComparison.OrdinalIgnoreCase))
                    sourceQuery.Kind = AiKnowledgeResourceKind.Knowledge;
                else if (string.Equals(requestedType, WikiResourceType, StringComparison.OrdinalIgnoreCase))
                    sourceQuery.Kind = AiKnowledgeResourceKind.Wiki;
            }

            var resources = await _source.ListAsync(sourceQuery, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var results = new List<AiResourceInventoryItem>();
            foreach (var resource in resources ?? new List<AiKnowledgeResource>())
            {
                if (resource == null) continue;
                resource.Validate();

                var resourceType = resource.Kind == AiKnowledgeResourceKind.Wiki ? WikiResourceType : KnowledgeResourceType;
                var lifecycle = resource.Status.ToString();
                if (query.ResourceTypes.Count > 0 &&
                    !query.ResourceTypes.Contains(resourceType, StringComparer.OrdinalIgnoreCase))
                    continue;
                if (query.Scope.HasValue && resource.Scope != query.Scope.Value)
                    continue;
                if (query.OwnerId != null && !string.Equals(query.OwnerId, resource.OwnerId, StringComparison.Ordinal))
                    continue;
                if (!string.IsNullOrWhiteSpace(query.LifecycleStatus) &&
                    !string.Equals(query.LifecycleStatus.Trim(), lifecycle, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (query.Version.HasValue && resource.Version != query.Version.Value)
                    continue;
                if (query.UpdatedAfterUtc.HasValue && new DateTimeOffset(resource.UpdatedUtc) < query.UpdatedAfterUtc.Value)
                    continue;
                if (query.UpdatedBeforeUtc.HasValue && new DateTimeOffset(resource.UpdatedUtc) > query.UpdatedBeforeUtc.Value)
                    continue;
                if (query.AuthoritativeOnly && !resource.IsAuthoritative)
                    continue;

                if (!string.IsNullOrWhiteSpace(query.SearchText))
                {
                    var search = query.SearchText.Trim();
                    var searchable = (resource.Id ?? string.Empty) + " " + (resource.Title ?? string.Empty);
                    if (searchable.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                results.Add(new AiResourceInventoryItem
                {
                    ResourceType = resourceType,
                    ResourceId = resource.Id,
                    Version = resource.Version,
                    Scope = resource.Scope,
                    OwnerId = resource.OwnerId,
                    DisplayName = resource.Title,
                    LifecycleStatus = lifecycle,
                    IsAuthoritative = resource.IsAuthoritative,
                    UpdatedUtc = new DateTimeOffset(resource.UpdatedUtc),
                    Source = "knowledge-resource-source"
                });
            }

            return results.AsReadOnly();
        }

        private static IReadOnlyList<AiResourceInventoryItem> Empty()
        {
            return new List<AiResourceInventoryItem>().AsReadOnly();
        }
    }
}

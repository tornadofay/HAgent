using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiMemoryResourceInventorySource : IAiResourceInventorySource
    {
        public const string ResourceType = AiMemoryResourceTypes.Memory;

        private readonly IMemoryStore _store;
        private readonly int _sourceLimit;

        public AiMemoryResourceInventorySource(IMemoryStore store, int sourceLimit = 1000)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            if (sourceLimit < 1 || sourceLimit > 1000)
                throw new ArgumentException("Memory inventory source limit must be between 1 and 1000.", nameof(sourceLimit));
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
                !query.ResourceTypes.Contains(ResourceType, StringComparer.OrdinalIgnoreCase))
                return new List<AiResourceInventoryItem>().AsReadOnly();

            MemoryScope? memoryScope = MapScope(query.Scope);
            if (query.Scope.HasValue && !memoryScope.HasValue)
                return new List<AiResourceInventoryItem>().AsReadOnly();

            var entries = await _store.SearchAsync(new MemoryQuery
            {
                Scope = memoryScope,
                OwnerId = query.OwnerId,
                MaxResults = _sourceLimit,
                IncludeExpired = true
            }, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var results = new List<AiResourceInventoryItem>();
            foreach (var entry in entries ?? new List<MemoryEntry>())
            {
                if (entry == null) continue;
                entry.Validate();

                var lifecycle = entry.IsExpired() ? "Expired" : "Published";
                var displayName = entry.TypeId;
                var searchable = (entry.Id ?? string.Empty) + " " + (displayName ?? string.Empty);

                if (!string.IsNullOrWhiteSpace(query.SearchText) &&
                    searchable.IndexOf(query.SearchText.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                if (!string.IsNullOrWhiteSpace(query.LifecycleStatus) &&
                    !string.Equals(query.LifecycleStatus.Trim(), lifecycle, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (query.Version.HasValue)
                    continue;
                if (query.UpdatedAfterUtc.HasValue && entry.CreatedAt < query.UpdatedAfterUtc.Value)
                    continue;
                if (query.UpdatedBeforeUtc.HasValue && entry.CreatedAt > query.UpdatedBeforeUtc.Value)
                    continue;

                var scope = MapScope(entry.Scope);
                if (!scope.HasValue) continue;

                results.Add(new AiResourceInventoryItem
                {
                    ResourceType = ResourceType,
                    ResourceId = entry.Id,
                    Scope = scope.Value,
                    OwnerId = entry.OwnerId,
                    DisplayName = displayName,
                    LifecycleStatus = lifecycle,
                    IsAuthoritative = true,
                    UpdatedUtc = entry.CreatedAt,
                    Source = "memory-store"
                });
            }

            return results.AsReadOnly();
        }

        private static MemoryScope? MapScope(AgentResourceScope? scope)
        {
            if (!scope.HasValue) return null;
            switch (scope.Value)
            {
                case AgentResourceScope.User: return MemoryScope.User;
                case AgentResourceScope.Workspace: return MemoryScope.Shared;
                case AgentResourceScope.Agent: return MemoryScope.Agent;
                case AgentResourceScope.Runtime: return MemoryScope.Session;
                case AgentResourceScope.Execution: return MemoryScope.Task;
                default: return null;
            }
        }

        private static AgentResourceScope? MapScope(MemoryScope scope)
        {
            switch (scope)
            {
                case MemoryScope.User: return AgentResourceScope.User;
                case MemoryScope.Shared: return AgentResourceScope.Workspace;
                case MemoryScope.Agent: return AgentResourceScope.Agent;
                case MemoryScope.Session: return AgentResourceScope.Runtime;
                case MemoryScope.Task: return AgentResourceScope.Execution;
                default: return null;
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class MemoryResourceInventorySourceTests
    {
        [Fact]
        public async Task Source_projects_authoritative_memory_entries_into_inventory_items()
        {
            var store = new InMemoryMemoryStore();
            await store.AddAsync(CreateEntry("memory-agent", MemoryScope.Agent, "agent-1", "semantic.fact"));
            await store.AddAsync(CreateEntry("memory-user", MemoryScope.User, "user-1", "semantic.preference"));

            var source = new AiMemoryResourceInventorySource(store);
            var result = await source.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true });

            Assert.Equal(2, result.Count);
            Assert.Equal("memory", result[0].ResourceType);
            Assert.All(result, item => Assert.True(item.IsAuthoritative));
            Assert.Contains(result, item => item.ResourceId == "memory-agent" && item.Scope == AgentResourceScope.Agent);
            Assert.Contains(result, item => item.ResourceId == "memory-user" && item.Scope == AgentResourceScope.User);
        }

        [Fact]
        public async Task Source_applies_inventory_filters_without_storage_specific_queries()
        {
            var store = new InMemoryMemoryStore();
            await store.AddAsync(CreateEntry("billing-memory", MemoryScope.Agent, "agent-1", "semantic.fact"));
            await store.AddAsync(CreateEntry("other-memory", MemoryScope.Agent, "agent-2", "semantic.fact"));

            var source = new AiMemoryResourceInventorySource(store);
            var result = await source.ListAsync(new AiResourceInventoryQuery
            {
                ResourceTypes = { "memory" },
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-1",
                SearchText = "billing"
            });

            Assert.Single(result);
            Assert.Equal("billing-memory", result[0].ResourceId);
        }

        [Fact]
        public async Task Source_maps_lifecycle_and_updated_filters_from_memory_expiration()
        {
            var store = new InMemoryMemoryStore();
            var expired = CreateEntry("expired-memory", MemoryScope.Agent, "agent-1", "semantic.fact");
            expired.CreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
            expired.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
            await store.AddAsync(expired);

            var source = new AiMemoryResourceInventorySource(store);
            var expiredResult = await source.ListAsync(new AiResourceInventoryQuery { LifecycleStatus = "Expired" });
            var recentResult = await source.ListAsync(new AiResourceInventoryQuery { UpdatedAfterUtc = DateTimeOffset.UtcNow.AddHours(-1) });

            Assert.Single(expiredResult);
            Assert.Equal("Expired", expiredResult[0].LifecycleStatus);
            Assert.Empty(recentResult);
        }

        [Fact]
        public async Task Source_honors_cancellation_and_rejects_version_filter()
        {
            var store = new InMemoryMemoryStore();
            await store.AddAsync(CreateEntry("memory-agent", MemoryScope.Agent, "agent-1", "semantic.fact"));
            var source = new AiMemoryResourceInventorySource(store);

            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() =>
                    source.ListAsync(new AiResourceInventoryQuery(), cancellation.Token));
            }

            var versioned = await source.ListAsync(new AiResourceInventoryQuery { Version = 1 });
            Assert.Empty(versioned);
        }

        private static MemoryEntry CreateEntry(string id, MemoryScope scope, string ownerId, string typeId)
        {
            return new MemoryEntry
            {
                Id = id,
                Scope = scope,
                Kind = MemoryKind.Fact,
                Family = AiMemoryFamily.Semantic,
                TypeId = typeId,
                OwnerId = ownerId,
                Content = "Example memory content for resource inventory verification.",
                Provenance = new AiMemoryProvenance
                {
                    Kind = AiMemoryProvenanceKind.HostProvided,
                    Source = "test"
                },
                CreatedAt = DateTimeOffset.UtcNow,
                OccurredAt = DateTimeOffset.UtcNow
            };
        }
    }
}

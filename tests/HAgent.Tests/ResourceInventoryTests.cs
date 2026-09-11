using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ResourceInventoryTests
    {
        [Fact]
        public async Task Inventory_ReturnsKnownResourcesAcrossSourcesInDeterministicOrder()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(
                    Item("skill", "skill-a", 2, "Skill A", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-a", null, "Memory A", true, DateTimeOffset.UtcNow.AddMinutes(-1)),
                    Item("knowledge", "knowledge-a", 1, "Knowledge A", true, DateTimeOffset.UtcNow.AddMinutes(-2)))
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true });

            Assert.Equal(3, result.Count);
            Assert.Equal("knowledge", result[0].ResourceType);
            Assert.Equal("memory", result[1].ResourceType);
            Assert.Equal("skill", result[2].ResourceType);
        }

        [Fact]
        public async Task Inventory_FiltersByTypeScopeOwnerSearchAndAuthority()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(
                    Item("skill", "skill-a", 1, "Billing Skill", true, DateTimeOffset.UtcNow),
                    Item("skill", "skill-b", 1, "Draft Billing Skill", false, DateTimeOffset.UtcNow),
                    Item("knowledge", "knowledge-a", 1, "Billing Knowledge", true, DateTimeOffset.UtcNow))
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery
            {
                ResourceTypes = { "skill" },
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-1",
                SearchText = "billing",
                AuthoritativeOnly = true
            });

            Assert.Single(result);
            Assert.Equal("skill-a", result[0].ResourceId);
        }

        [Fact]
        public async Task Inventory_DeduplicatesSameResourceVersionAcrossSources()
        {
            var item = Item("skill", "skill-a", 2, "Skill A", true, DateTimeOffset.UtcNow);
            var duplicate = item.Clone();
            duplicate.UpdatedUtc = duplicate.UpdatedUtc.AddMinutes(-5);
            var newer = item.Clone();
            newer.Version = 3;
            newer.UpdatedUtc = newer.UpdatedUtc.AddMinutes(-1);

            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(item),
                new TestSource(duplicate, newer)
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery());

            Assert.Equal(2, result.Count);
            Assert.Equal(3, result[1].Version);
        }

        [Fact]
        public async Task Inventory_EnforcesBoundedMaxResults()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(
                    Item("memory", "memory-1", null, "Memory 1", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-2", null, "Memory 2", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-3", null, "Memory 3", true, DateTimeOffset.UtcNow))
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 2 });

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Inventory_RejectsInvalidQueryAndItemContracts()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(Item("skill", "skill-a", 1, "Skill A", true, DateTimeOffset.UtcNow))
            });

            await Assert.ThrowsAsync<ArgumentException>(() => inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 0 }));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                new AiResourceInventory(new[]
                {
                    new TestSource(new AiResourceInventoryItem
                    {
                        ResourceType = "skill",
                        ResourceId = "skill-invalid",
                        Scope = AgentResourceScope.Agent,
                        OwnerId = "agent-1",
                        UpdatedUtc = default(DateTimeOffset)
                    })
                }).ListAsync(new AiResourceInventoryQuery()));
        }

        private static AiResourceInventoryItem Item(string type, string id, long? version, string name, bool authoritative, DateTimeOffset updated)
        {
            return new AiResourceInventoryItem
            {
                ResourceType = type,
                ResourceId = id,
                Version = version,
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-1",
                DisplayName = name,
                LifecycleStatus = authoritative ? "Published" : "Draft",
                IsAuthoritative = authoritative,
                UpdatedUtc = updated,
                Source = "test"
            };
        }

        private sealed class TestSource : IAiResourceInventorySource
        {
            private readonly IReadOnlyList<AiResourceInventoryItem> _items;

            public TestSource(params AiResourceInventoryItem[] items)
            {
                _items = items;
            }

            public Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(
                AiResourceInventoryQuery query,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_items);
            }
        }
    }
}

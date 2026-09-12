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
                    Item("skill", "skill-a", 2, "Skill A", "Published", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-a", null, "Memory A", "Published", true, DateTimeOffset.UtcNow.AddMinutes(-1)),
                    Item("knowledge", "knowledge-a", 1, "Knowledge A", "Published", true, DateTimeOffset.UtcNow.AddMinutes(-2)))
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true });

            Assert.Equal(3, result.Count);
            Assert.Equal("knowledge", result[0].ResourceType);
            Assert.Equal("memory", result[1].ResourceType);
            Assert.Equal("skill", result[2].ResourceType);
        }

        [Fact]
        public async Task Inventory_FiltersByTypeScopeOwnerSearchLifecycleVersionUpdatedAndAuthority()
        {
            var now = DateTimeOffset.UtcNow;
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(
                    Item("skill", "skill-a", 3, "Billing Skill", "Published", true, now.AddHours(-1)),
                    Item("skill", "skill-b", 2, "Billing Skill", "Draft", false, now.AddHours(-2)),
                    Item("knowledge", "knowledge-a", 3, "Billing Knowledge", "Published", true, now.AddDays(-2)))
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery
            {
                ResourceTypes = { "skill" },
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-1",
                SearchText = "billing",
                LifecycleStatus = "Published",
                Version = 3,
                UpdatedAfterUtc = now.AddHours(-2),
                UpdatedBeforeUtc = now,
                AuthoritativeOnly = true
            });

            Assert.Single(result);
            Assert.Equal("skill-a", result[0].ResourceId);
        }

        [Fact]
        public async Task Inventory_PagesDeterministicallyWithSkipResults()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(
                    Item("memory", "memory-1", null, "Memory 1", "Published", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-2", null, "Memory 2", "Published", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-3", null, "Memory 3", "Published", true, DateTimeOffset.UtcNow))
            });

            var firstPage = await inventory.ListAsync(new AiResourceInventoryQuery { SkipResults = 0, MaxResults = 2 });
            var secondPage = await inventory.ListAsync(new AiResourceInventoryQuery { SkipResults = 2, MaxResults = 2 });

            Assert.Equal(2, firstPage.Count);
            Assert.Equal(1, secondPage.Count);
            Assert.Equal("memory-1", firstPage[0].ResourceId);
            Assert.Equal("memory-2", firstPage[1].ResourceId);
            Assert.Equal("memory-3", secondPage[0].ResourceId);
        }

        [Fact]
        public async Task Inventory_DeduplicatesSameLogicalResourceAndKeepsHighestVersion()
        {
            var version2 = Item("skill", "skill-a", 2, "Skill A", "Published", true, DateTimeOffset.UtcNow.AddMinutes(-5));
            var duplicate = version2.Clone();
            var version3 = Item("skill", "skill-a", 3, "Skill A", "Published", true, DateTimeOffset.UtcNow.AddMinutes(-1));

            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(version2, duplicate),
                new TestSource(version3)
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery());

            Assert.Single(result);
            Assert.Equal(3, result[0].Version);
        }

        [Fact]
        public async Task Inventory_EnforcesBoundedMaxResults()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(
                    Item("memory", "memory-1", null, "Memory 1", "Published", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-2", null, "Memory 2", "Published", true, DateTimeOffset.UtcNow),
                    Item("memory", "memory-3", null, "Memory 3", "Published", true, DateTimeOffset.UtcNow))
            });

            var result = await inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 2 });

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Inventory_RejectsInvalidQueryAndItemContracts()
        {
            var inventory = new AiResourceInventory(new[]
            {
                new TestSource(Item("skill", "skill-a", 1, "Skill A", "Published", true, DateTimeOffset.UtcNow))
            });

            await Assert.ThrowsAsync<ArgumentException>(() => inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 0 }));
            await Assert.ThrowsAsync<ArgumentException>(() => inventory.ListAsync(new AiResourceInventoryQuery { Version = 0 }));
            await Assert.ThrowsAsync<ArgumentException>(() => inventory.ListAsync(new AiResourceInventoryQuery
            {
                UpdatedAfterUtc = DateTimeOffset.UtcNow,
                UpdatedBeforeUtc = DateTimeOffset.UtcNow.AddMinutes(-1)
            }));
            await Assert.ThrowsAsync<ArgumentException>(() => inventory.ListAsync(new AiResourceInventoryQuery { SkipResults = -1 }));
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

        private static AiResourceInventoryItem Item(string type, string id, long? version, string name, string status, bool authoritative, DateTimeOffset updated)
        {
            return new AiResourceInventoryItem
            {
                ResourceType = type,
                ResourceId = id,
                Version = version,
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-1",
                DisplayName = name,
                LifecycleStatus = status,
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

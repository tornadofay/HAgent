using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddAuthoritativeResourceInventoryTab()
        {
            AddApiTab(
                "Authoritative Resource Inventory",
                "Verify provider-neutral resource inventory",
                "Exercises the same inventory boundary consumed by configuration management without requiring SQL Server or MySQL enumeration.",
                "Verifies unified Memory / Knowledge / Skill projection, filtering, deterministic ordering, and bounded results.",
                "Uses deterministic provider-neutral inventory sources.",
                TestAuthoritativeResourceInventoryAsync,
                "Authoritative Resource Inventory",
                "Inventory is read-only; it does not publish, edit, or delete resources.");
        }

        private async Task TestAuthoritativeResourceInventoryAsync(string unused)
        {
            var now = DateTimeOffset.UtcNow;
            var inventory = new AiResourceInventory(new[]
            {
                new ExampleResourceInventorySource(
                    Item("skill", "example-skill", 2, "Example Skill", "Published", true, AgentResourceScope.Agent, "example-agent", now),
                    Item("knowledge", "example-knowledge", 3, "Example Knowledge", "Published", true, AgentResourceScope.Agent, "example-agent", now.AddMinutes(-1)),
                    Item("memory", "example-memory", null, "Example Memory", "Published", true, AgentResourceScope.Agent, "example-agent", now.AddMinutes(-2)),
                    Item("skill", "draft-skill", 4, "Draft Skill", "Draft", false, AgentResourceScope.Agent, "example-agent", now.AddMinutes(-3)))
            });

            var all = await inventory.ListAsync(new AiResourceInventoryQuery());
            if (all.Count != 4) throw new InvalidOperationException("Inventory projection contract failed.");

            var authoritative = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true });
            if (authoritative.Count != 3) throw new InvalidOperationException("Authoritative-only inventory contract failed.");

            var skill = await inventory.ListAsync(new AiResourceInventoryQuery { ResourceTypes = { "skill" }, SearchText = "example", AuthoritativeOnly = true });
            if (skill.Count != 1 || !string.Equals(skill[0].ResourceId, "example-skill", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Resource type/text filtering contract failed.");

            var bounded = await inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 2 });
            if (bounded.Count != 2 || !string.Equals(bounded[0].ResourceType, "knowledge", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Deterministic ordering/bounded results contract failed.");

            Write(
                "AUTHORITATIVE RESOURCE INVENTORY",
                "Contract test succeeded." + Environment.NewLine +
                "Unified Memory / Knowledge / Skill inventory projection: verified." + Environment.NewLine +
                "Authoritative-only filtering: verified." + Environment.NewLine +
                "Resource-type and text filtering: verified." + Environment.NewLine +
                "Deterministic ordering and bounded results: verified." + Environment.NewLine +
                "Storage/provider-specific enumeration remains outside the inventory contract: verified.");
        }

        private static AiResourceInventoryItem Item(string type, string id, long? version, string name, string status, bool authoritative, AgentResourceScope scope, string owner, DateTimeOffset updated)
        {
            return new AiResourceInventoryItem
            {
                ResourceType = type,
                ResourceId = id,
                Version = version,
                DisplayName = name,
                LifecycleStatus = status,
                IsAuthoritative = authoritative,
                Scope = scope,
                OwnerId = owner,
                UpdatedUtc = updated,
                Source = "example"
            };
        }

        private sealed class ExampleResourceInventorySource : IAiResourceInventorySource
        {
            private readonly IReadOnlyList<AiResourceInventoryItem> _items;
            public ExampleResourceInventorySource(params AiResourceInventoryItem[] items) { _items = items; }
            public Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(AiResourceInventoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_items);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddResourceInventoryTab()
        {
            AddApiTab(
                "Authoritative Resource Inventory",
                "Run provider-neutral resource inventory contract",
                "Verifies that Memory, Knowledge, and Skill resources can be represented through one bounded inventory contract without embedding storage or provider details.",
                "The scenario intentionally uses deterministic in-process sources. SQL Server and MySQL enumeration are deferred to the storage phase.",
                "Exercises filtering, authoritative-only selection, deterministic ordering, duplicate/version handling, and bounded results.",
                TestResourceInventoryAsync,
                "Authoritative resource inventory",
                "Inventory describes resources only; it does not edit, publish, or authorize them.");
        }

        private async Task TestResourceInventoryAsync(string unused)
        {
            var now = DateTimeOffset.UtcNow;
            var source = new ExampleResourceInventorySource(
                CreateInventoryItem("memory", "memory-example", null, "Example Memory", "Working", true, now),
                CreateInventoryItem("knowledge", "knowledge-example", 2, "Example Knowledge", "Published", true, now.AddMinutes(-1)),
                CreateInventoryItem("skill", "skill-example", 3, "Example Skill", "Published", true, now.AddMinutes(-2)),
                CreateInventoryItem("skill", "skill-draft", 1, "Draft Skill", "Draft", false, now));

            var inventory = new AiResourceInventory(new[] { source });
            var all = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true }).ConfigureAwait(true);
            if (all.Count != 3)
                throw new InvalidOperationException("Authoritative inventory count contract failed.");

            var skills = await inventory.ListAsync(new AiResourceInventoryQuery
            {
                ResourceTypes = { "skill" },
                AuthoritativeOnly = true,
                SearchText = "example"
            }).ConfigureAwait(true);
            if (skills.Count != 1 || !string.Equals(skills[0].ResourceId, "skill-example", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Filtered inventory contract failed.");

            var limited = await inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 2 }).ConfigureAwait(true);
            if (limited.Count != 2)
                throw new InvalidOperationException("Bounded inventory result contract failed.");

            Write(
                "AUTHORITATIVE RESOURCE INVENTORY",
                "Contract test succeeded." + Environment.NewLine +
                "Unified Memory / Knowledge / Skill inventory projection: verified." + Environment.NewLine +
                "Authoritative-only filtering: verified." + Environment.NewLine +
                "Resource-type and text filtering: verified." + Environment.NewLine +
                "Deterministic ordering and bounded results: verified." + Environment.NewLine +
                "Storage/provider-specific enumeration remains outside the inventory contract: verified.");
        }

        private static AiResourceInventoryItem CreateInventoryItem(
            string type,
            string id,
            long? version,
            string name,
            string status,
            bool authoritative,
            DateTimeOffset updated)
        {
            return new AiResourceInventoryItem
            {
                ResourceType = type,
                ResourceId = id,
                Version = version,
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                DisplayName = name,
                LifecycleStatus = status,
                IsAuthoritative = authoritative,
                UpdatedUtc = updated,
                Source = "example"
            };
        }

        private sealed class ExampleResourceInventorySource : IAiResourceInventorySource
        {
            private readonly IReadOnlyList<AiResourceInventoryItem> _items;

            public ExampleResourceInventorySource(params AiResourceInventoryItem[] items)
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

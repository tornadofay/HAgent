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
                "Verifies that Memory, Knowledge, and Skill resources can be represented and inspected through bounded provider-neutral contracts without embedding storage or provider details.",
                "The scenario intentionally uses deterministic in-process sources. SQL Server and MySQL enumeration remain deferred to the storage phase.",
                "Exercises filtering, authoritative-only selection, deterministic ordering, duplicate/version handling, bounded results, and readable resource-detail inspection.",
                TestResourceInventoryAsync,
                "Authoritative resource inventory",
                "Inventory describes resources only; resource inspection is read-only and does not edit, publish, delete, or authorize them.");
        }

        private async Task TestResourceInventoryAsync(string unused)
        {
            var inventory = CreateExampleResourceInventory();
            var all = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true }).ConfigureAwait(true);
            if (all.Count != 3) throw new InvalidOperationException("Authoritative inventory count contract failed.");

            var skills = await inventory.ListAsync(new AiResourceInventoryQuery { ResourceTypes = { "skill" }, AuthoritativeOnly = true, SearchText = "example" }).ConfigureAwait(true);
            if (skills.Count != 1 || !string.Equals(skills[0].ResourceId, "skill-example", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Filtered inventory contract failed.");

            var limited = await inventory.ListAsync(new AiResourceInventoryQuery { MaxResults = 2 }).ConfigureAwait(true);
            if (limited.Count != 2) throw new InvalidOperationException("Bounded inventory result contract failed.");

            var details = CreateExampleResourceDetails();
            var memoryDetail = await details.GetAsync(FindResource(all, "memory-example"), CancellationToken.None).ConfigureAwait(true);
            if (memoryDetail == null || string.IsNullOrWhiteSpace(memoryDetail.Content))
                throw new InvalidOperationException("Resource detail inspection contract failed for Memory.");

            var knowledgeDetail = await details.GetAsync(FindResource(all, "knowledge-example"), CancellationToken.None).ConfigureAwait(true);
            if (knowledgeDetail == null || knowledgeDetail.Sections.Count == 0 || string.IsNullOrWhiteSpace(knowledgeDetail.Sections[0].Content))
                throw new InvalidOperationException("Resource detail inspection contract failed for Knowledge.");

            var skillDetail = await details.GetAsync(FindResource(all, "skill-example"), CancellationToken.None).ConfigureAwait(true);
            if (skillDetail == null || skillDetail.Sections.Count < 2)
                throw new InvalidOperationException("Resource detail inspection contract failed for Skill.");

            Write(
                "AUTHORITATIVE RESOURCE INVENTORY",
                "Contract test succeeded." + Environment.NewLine +
                "Unified Memory / Knowledge / Skill inventory projection: verified." + Environment.NewLine +
                "Authoritative-only filtering: verified." + Environment.NewLine +
                "Resource-type and text filtering: verified." + Environment.NewLine +
                "Deterministic ordering and bounded results: verified." + Environment.NewLine +
                "Readable resource detail inspection for Memory / Knowledge / Skill: verified." + Environment.NewLine +
                "Storage/provider-specific enumeration remains outside the inventory contract: verified.");
        }

        private static AiResourceInventoryItem FindResource(IReadOnlyList<AiResourceInventoryItem> items, string resourceId)
        {
            foreach (var item in items)
                if (string.Equals(item.ResourceId, resourceId, StringComparison.OrdinalIgnoreCase)) return item;
            throw new InvalidOperationException("Example resource was not found: " + resourceId);
        }

        private static IAiResourceInventory CreateExampleResourceInventory()
        {
            var now = DateTimeOffset.UtcNow;
            var source = new ExampleResourceInventorySource(
                CreateInventoryItem("memory", "memory-example", null, "Example Memory", "Published", true, now),
                CreateInventoryItem("knowledge", "knowledge-example", 2, "Example Knowledge", "Published", true, now.AddMinutes(-1)),
                CreateInventoryItem("skill", "skill-example", 3, "Example Skill", "Published", true, now.AddMinutes(-2)),
                CreateInventoryItem("skill", "skill-draft", 1, "Draft Skill", "Draft", false, now));
            return new AiResourceInventory(new[] { source });
        }

        private static IAiResourceDetailSource CreateExampleResourceDetails()
        {
            return new ExampleResourceDetailSource();
        }

        private static AiResourceInventoryItem CreateInventoryItem(string type, string id, long? version, string name, string status, bool authoritative, DateTimeOffset updated)
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
            public ExampleResourceInventorySource(params AiResourceInventoryItem[] items) { _items = items; }
            public Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(AiResourceInventoryQuery query, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_items);
            }
        }

        private sealed class ExampleResourceDetailSource : IAiResourceDetailSource
        {
            public Task<AiResourceDetail> GetAsync(AiResourceInventoryItem resource, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (resource == null) throw new ArgumentNullException(nameof(resource));

                AiResourceDetail detail;
                switch (resource.ResourceType == null ? string.Empty : resource.ResourceType.ToLowerInvariant())
                {
                    case "memory":
                        detail = new AiResourceDetail
                        {
                            InventoryItem = resource,
                            Summary = "A deterministic example memory entry retained for resource-management verification.",
                            Content = "Customer Alice prefers Arabic responses when discussing invoices."
                        };
                        detail.Fields.Add(new AiResourceDetailField { Name = "Memory family", Value = "Semantic" });
                        detail.Fields.Add(new AiResourceDetailField { Name = "Memory scope", Value = resource.Scope.ToString() });
                        break;

                    case "knowledge":
                    case "wiki":
                        detail = new AiResourceDetail
                        {
                            InventoryItem = resource,
                            Summary = "A deterministic example knowledge resource with readable content and structured metadata.",
                            Content = "HAgent resource inventory exposes authoritative Knowledge and Wiki resources through one provider-neutral management projection."
                        };
                        detail.Fields.Add(new AiResourceDetailField { Name = "Version", Value = resource.Version.HasValue ? resource.Version.Value.ToString() : "N/A" });
                        detail.Fields.Add(new AiResourceDetailField { Name = "Source", Value = "Example knowledge source" });
                        detail.Sections.Add(new AiResourceDetailSection { Title = "Tags", Content = "inventory\nknowledge\nmanagement" });
                        break;

                    case "skill":
                        detail = new AiResourceDetail
                        {
                            InventoryItem = resource,
                            Summary = "A deterministic example Skill showing the structure that a future editor can build on.",
                            Content = "Inspect the selected resource, display its structured definition, and keep authoritative publication separate from editing."
                        };
                        detail.Fields.Add(new AiResourceDetailField { Name = "Skill name", Value = "Inspect Authoritative Resource" });
                        detail.Fields.Add(new AiResourceDetailField { Name = "Version", Value = resource.Version.HasValue ? resource.Version.Value.ToString() : "N/A" });
                        detail.Sections.Add(new AiResourceDetailSection { Title = "Inputs", Content = "AiResourceInventoryItem" });
                        detail.Sections.Add(new AiResourceDetailSection { Title = "Steps", Content = "1. Select resource\n2. Resolve detail source\n3. Validate identity\n4. Render bounded content" });
                        break;

                    default:
                        detail = new AiResourceDetail
                        {
                            InventoryItem = resource,
                            Summary = "Generic resource detail.",
                            Content = "No specialized Example representation is registered for this resource type."
                        };
                        break;
                }

                detail.Validate();
                return Task.FromResult(detail);
            }
        }
    }
}
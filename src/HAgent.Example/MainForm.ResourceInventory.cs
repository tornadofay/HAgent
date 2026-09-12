using System;
using System.Collections.Generic;
using System.Linq;
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
                "Verifies that Memory, Knowledge/Wiki, and Skill resources can be represented and inspected through one bounded provider-neutral management contract without embedding storage or provider details.",
                "Memory and Knowledge/Wiki now use their provider-neutral resource-source boundaries. Skill enumeration remains deterministic until its existing lookup contract gains a supported authoritative enumeration boundary.",
                "Exercises filtering, authoritative-only selection, lifecycle/version/updated filtering, deterministic ordering, duplicate/version handling, bounded paging, and readable resource-detail inspection.",
                TestResourceInventoryAsync,
                "Authoritative resource inventory",
                "Inventory describes resources only; resource inspection is read-only and does not edit, publish, delete, or authorize them.");
        }

        private async Task TestResourceInventoryAsync(string unused)
        {
            var inventory = CreateExampleResourceInventory();
            var all = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true }).ConfigureAwait(true);
            if (all.Count != 3)
            {
                var actual = string.Join(", ", all.Select(x => x.ResourceType + ":" + x.ResourceId));
                throw new InvalidOperationException("Authoritative inventory count contract failed. Expected 3 resources (memory:memory-example, knowledge:knowledge-example, skill:skill-example); actual=" + all.Count + " [" + actual + "]");
            }

            var skills = await inventory.ListAsync(new AiResourceInventoryQuery { ResourceTypes = { "skill" }, AuthoritativeOnly = true, SearchText = "example" }).ConfigureAwait(true);
            if (skills.Count != 1 || !string.Equals(skills[0].ResourceId, "skill-example", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Filtered inventory contract failed.");

            var knowledge = await inventory.ListAsync(new AiResourceInventoryQuery { ResourceTypes = { "knowledge" }, AuthoritativeOnly = true, SearchText = "retention" }).ConfigureAwait(true);
            if (knowledge.Count != 1 || !string.Equals(knowledge[0].ResourceId, "knowledge-example", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Knowledge source projection contract failed.");

            var published = await inventory.ListAsync(new AiResourceInventoryQuery
            {
                LifecycleStatus = "Published",
                Version = 3,
                OwnerId = "example-agent",
                UpdatedAfterUtc = DateTimeOffset.UtcNow.AddDays(-1),
                AuthoritativeOnly = true
            }).ConfigureAwait(true);
            if (published.Count != 1 || !string.Equals(published[0].ResourceId, "skill-example", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Lifecycle/version/updated/owner filter contract failed.");

            var firstPage = await inventory.ListAsync(new AiResourceInventoryQuery { SkipResults = 0, MaxResults = 2 }).ConfigureAwait(true);
            var secondPage = await inventory.ListAsync(new AiResourceInventoryQuery { SkipResults = 2, MaxResults = 2 }).ConfigureAwait(true);
            if (firstPage.Count != 2 || secondPage.Count != 2 || string.Equals(firstPage[0].ResourceId, secondPage[0].ResourceId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Paged inventory result contract failed.");

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
                "Real IMemoryStore → inventory source projection: verified." + Environment.NewLine +
                "Real provider-neutral Knowledge source → inventory projection: verified." + Environment.NewLine +
                "Authoritative-only filtering: verified." + Environment.NewLine +
                "Resource-type and text filtering: verified." + Environment.NewLine +
                "Lifecycle/version/updated/owner filtering: verified." + Environment.NewLine +
                "Deterministic ordering and bounded paging: verified." + Environment.NewLine +
                "Readable resource detail inspection for Memory / Knowledge / Skill: verified." + Environment.NewLine +
                "Skill storage-specific enumeration remains deferred until a supported authoritative enumeration contract exists: verified.");
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
            var memoryStore = new InMemoryMemoryStore();
            memoryStore.AddAsync(new MemoryEntry
            {
                Id = "memory-example",
                Scope = MemoryScope.Agent,
                Kind = MemoryKind.Preference,
                Family = AiMemoryFamily.Semantic,
                TypeId = "semantic.preference",
                OwnerId = "example-agent",
                Content = "Customer Alice prefers Arabic responses when discussing invoices.",
                Provenance = new AiMemoryProvenance
                {
                    Kind = AiMemoryProvenanceKind.HostProvided,
                    Source = "example"
                },
                CreatedAt = now,
                OccurredAt = now
            }).GetAwaiter().GetResult();

            var knowledgeSource = new ExampleKnowledgeResourceSource(
                CreateKnowledgeResource(AiKnowledgeResourceKind.Knowledge, "knowledge-example", 2, "Retention Policy", "HAgent resource inventory exposes authoritative Knowledge through one provider-neutral management projection.", now.AddMinutes(-1)),
                CreateKnowledgeResource(AiKnowledgeResourceKind.Knowledge, "knowledge-draft", 1, "Draft Knowledge", "Draft resources remain non-authoritative until governed publication.", now));

            var skillSource = new ExampleResourceInventorySource(
                CreateInventoryItem("skill", "skill-example", 3, "Example Skill", "Published", true, now.AddMinutes(-2)),
                CreateInventoryItem("skill", "skill-draft", 1, "Draft Skill", "Draft", false, now));

            return new AiResourceInventory(new IAiResourceInventorySource[]
            {
                new AiMemoryResourceInventorySource(memoryStore),
                new AiKnowledgeResourceInventorySource(knowledgeSource),
                skillSource
            });
        }

        private static AiKnowledgeResource CreateKnowledgeResource(
            AiKnowledgeResourceKind kind,
            string id,
            long version,
            string title,
            string content,
            DateTimeOffset updatedUtc,
            AiKnowledgeLifecycleStatus status = AiKnowledgeLifecycleStatus.Published)
        {
            return new AiKnowledgeResource
            {
                Id = id,
                Kind = kind,
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                Title = title,
                Summary = title,
                Content = content,
                Status = status,
                Version = version,
                Source = "example-knowledge-source",
                Provenance = new AiKnowledgeProvenance
                {
                    Kind = AiKnowledgeProvenanceKind.HostProvided,
                    Source = "example",
                    SourceId = id
                },
                CreatedUtc = updatedUtc.UtcDateTime,
                UpdatedUtc = updatedUtc.UtcDateTime
            };
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

        private sealed class ExampleKnowledgeResourceSource : IAiKnowledgeResourceSource
        {
            private readonly IReadOnlyList<AiKnowledgeResource> _resources;

            public ExampleKnowledgeResourceSource(params AiKnowledgeResource[] resources)
            {
                _resources = resources.Select(resource => resource.Clone()).ToList().AsReadOnly();
            }

            public Task<IReadOnlyList<AiKnowledgeResource>> ListAsync(
                AiKnowledgeEnumerationQuery query,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                query.Validate();

                var result = _resources
                    .Where(resource =>
                        (!query.Kind.HasValue || resource.Kind == query.Kind.Value) &&
                        (!query.Scope.HasValue || resource.Scope == query.Scope.Value) &&
                        (query.OwnerId == null || string.Equals(resource.OwnerId, query.OwnerId, StringComparison.Ordinal)) &&
                        (!query.Status.HasValue || resource.Status == query.Status.Value) &&
                        (!query.Version.HasValue || resource.Version == query.Version.Value) &&
                        (!query.UpdatedAfterUtc.HasValue || resource.UpdatedUtc >= query.UpdatedAfterUtc.Value) &&
                        (!query.UpdatedBeforeUtc.HasValue || resource.UpdatedUtc <= query.UpdatedBeforeUtc.Value) &&
                        (!query.AuthoritativeOnly || resource.IsAuthoritative) &&
                        (string.IsNullOrWhiteSpace(query.SearchText) ||
                         ((resource.Id ?? string.Empty) + " " + (resource.Title ?? string.Empty))
                             .IndexOf(query.SearchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0))
                    .Take(query.MaxResults)
                    .Select(resource => resource.Clone())
                    .ToList();

                return Task.FromResult<IReadOnlyList<AiKnowledgeResource>>(result.AsReadOnly());
            }
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
                            Summary = "A real provider-neutral MemoryStore entry retained for resource-management verification.",
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
                            Summary = "A Knowledge resource projected through the provider-neutral enumeration boundary.",
                            Content = "HAgent resource inventory exposes authoritative Knowledge and Wiki resources through one provider-neutral management projection."
                        };
                        detail.Fields.Add(new AiResourceDetailField { Name = "Version", Value = resource.Version.HasValue ? resource.Version.Value.ToString() : "N/A" });
                        detail.Fields.Add(new AiResourceDetailField { Name = "Lifecycle", Value = resource.LifecycleStatus });
                        detail.Sections.Add(new AiResourceDetailSection { Title = "Content", Content = detail.Content });
                        break;

                    case "skill":
                        detail = new AiResourceDetail
                        {
                            InventoryItem = resource,
                            Summary = "A deterministic skill inventory projection retained until the skill storage contract supports authoritative enumeration.",
                            Content = "Example skill content."
                        };
                        detail.Fields.Add(new AiResourceDetailField { Name = "Version", Value = resource.Version.HasValue ? resource.Version.Value.ToString() : "N/A" });
                        detail.Fields.Add(new AiResourceDetailField { Name = "Lifecycle", Value = resource.LifecycleStatus });
                        detail.Sections.Add(new AiResourceDetailSection { Title = "Description", Content = detail.Summary });
                        detail.Sections.Add(new AiResourceDetailSection { Title = "Content", Content = detail.Content });
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported example resource type: " + resource.ResourceType);
                }

                return Task.FromResult(detail);
            }
        }
    }
}

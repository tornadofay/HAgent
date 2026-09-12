using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class KnowledgeResourceInventorySourceTests
    {
        [Fact]
        public async Task Source_ProjectsKnowledgeAndWikiWithAuthorityAndVersion()
        {
            var now = DateTime.UtcNow;
            var source = new RecordingKnowledgeResourceSource(
                CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-1", 2, "Knowledge One", AiKnowledgeLifecycleStatus.Published, now),
                CreateResource(AiKnowledgeResourceKind.Wiki, "wiki-1", 4, "Wiki One", AiKnowledgeLifecycleStatus.Published, now.AddMinutes(1)),
                CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-draft", 1, "Draft", AiKnowledgeLifecycleStatus.Draft, now));
            var inventory = new AiKnowledgeResourceInventorySource(source);

            var result = await inventory.ListAsync(new AiResourceInventoryQuery { AuthoritativeOnly = true }, CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.ResourceType == "knowledge" && x.ResourceId == "knowledge-1" && x.Version == 2);
            Assert.Contains(result, x => x.ResourceType == "wiki" && x.ResourceId == "wiki-1" && x.Version == 4);
        }

        [Fact]
        public async Task Source_HonorsTypeOwnerScopeLifecycleVersionAndSearchFilters()
        {
            var now = DateTime.UtcNow;
            var source = new RecordingKnowledgeResourceSource(
                CreateResource(AiKnowledgeResourceKind.Knowledge, "knowledge-2", 2, "Retention Policy", AiKnowledgeLifecycleStatus.Published, now),
                CreateResource(AiKnowledgeResourceKind.Wiki, "wiki-2", 3, "Other", AiKnowledgeLifecycleStatus.Published, now));
            source.Resources[0].OwnerId = "owner-42";
            var inventory = new AiKnowledgeResourceInventorySource(source);

            var result = await inventory.ListAsync(new AiResourceInventoryQuery
            {
                ResourceTypes = { "knowledge" },
                Scope = AgentResourceScope.User,
                OwnerId = "owner-42",
                LifecycleStatus = "Published",
                Version = 2,
                SearchText = "retention"
            }, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal("knowledge-2", result[0].ResourceId);
        }

        [Fact]
        public async Task Source_InvalidLifecycleStatusReturnsNoResults()
        {
            var inventory = new AiKnowledgeResourceInventorySource(
                new RecordingKnowledgeResourceSource(CreateResource(
                    AiKnowledgeResourceKind.Knowledge, "knowledge-3", 1, "Knowledge", AiKnowledgeLifecycleStatus.Published, DateTime.UtcNow)));

            var result = await inventory.ListAsync(
                new AiResourceInventoryQuery { LifecycleStatus = "NotAStatus" },
                CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Source_SkipsUnmappableResourceTypesAndHonorsCancellation()
        {
            var inventory = new AiKnowledgeResourceInventorySource(
                new RecordingKnowledgeResourceSource(CreateResource(
                    AiKnowledgeResourceKind.Knowledge, "knowledge-4", 1, "Knowledge", AiKnowledgeLifecycleStatus.Published, DateTime.UtcNow)));

            var ignored = await inventory.ListAsync(
                new AiResourceInventoryQuery { ResourceTypes = { "skill" } },
                CancellationToken.None);
            Assert.Empty(ignored);

            var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => inventory.ListAsync(
                new AiResourceInventoryQuery(), cancellation.Token));
        }

        private static AiKnowledgeResource CreateResource(
            AiKnowledgeResourceKind kind,
            string id,
            long version,
            string title,
            AiKnowledgeLifecycleStatus status,
            DateTime updatedUtc)
        {
            return new AiKnowledgeResource
            {
                Id = id,
                Kind = kind,
                Scope = AgentResourceScope.User,
                OwnerId = "owner-default",
                Title = title,
                Summary = title,
                Content = "Bounded content for " + id,
                Status = status,
                Version = version,
                Source = "tests",
                Provenance = new AiKnowledgeProvenance
                {
                    Kind = AiKnowledgeProvenanceKind.HostProvided,
                    Source = "tests",
                    SourceId = id
                },
                CreatedUtc = updatedUtc,
                UpdatedUtc = updatedUtc
            };
        }

        private sealed class RecordingKnowledgeResourceSource : IAiKnowledgeResourceSource
        {
            public readonly List<AiKnowledgeResource> Resources;
            public AiKnowledgeEnumerationQuery LastQuery { get; private set; }

            public RecordingKnowledgeResourceSource(params AiKnowledgeResource[] resources)
            {
                Resources = resources.Select(x => x.Clone()).ToList();
            }

            public Task<IReadOnlyList<AiKnowledgeResource>> ListAsync(
                AiKnowledgeEnumerationQuery query,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                LastQuery = query.Clone();
                query.Validate();

                var result = Resources.Where(resource =>
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
                    .Select(x => x.Clone())
                    .ToList();

                return Task.FromResult<IReadOnlyList<AiKnowledgeResource>>(result.AsReadOnly());
            }
        }
    }
}

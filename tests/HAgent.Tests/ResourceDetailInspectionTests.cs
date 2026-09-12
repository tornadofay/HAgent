using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ResourceDetailInspectionTests
    {
        [Fact]
        public void Detail_requires_a_valid_inventory_item()
        {
            var detail = new AiResourceDetail();
            Assert.Throws<ArgumentNullException>(() => detail.Validate());
        }

        [Fact]
        public void Detail_fields_and_sections_require_titles()
        {
            var item = CreateItem();
            var detail = new AiResourceDetail { InventoryItem = item };
            detail.Fields.Add(new AiResourceDetailField { Name = "Owner", Value = "example-agent" });
            detail.Sections.Add(new AiResourceDetailSection { Title = "Content", Content = "Example content" });
            detail.Validate();

            detail.Fields.Add(new AiResourceDetailField { Name = "", Value = "invalid" });
            Assert.Throws<ArgumentException>(() => detail.Validate());
        }

        [Fact]
        public async Task Detail_source_returns_the_selected_resource_content()
        {
            var item = CreateItem();
            var source = new FakeDetailSource();
            var detail = await source.GetAsync(item, CancellationToken.None);

            Assert.Equal(item.ResourceId, detail.InventoryItem.ResourceId);
            Assert.Equal("Example resource content", detail.Content);
            Assert.Contains("Content", detail.Sections, section => string.Equals(section.Title, "Content", StringComparison.Ordinal));
        }

        [Fact]
        public async Task Detail_source_honors_cancellation()
        {
            var item = CreateItem();
            var source = new FakeDetailSource();
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() => source.GetAsync(item, cancellation.Token));
            }
        }

        private static AiResourceInventoryItem CreateItem()
        {
            return new AiResourceInventoryItem
            {
                ResourceType = "knowledge",
                ResourceId = "knowledge-example",
                Scope = AgentResourceScope.Agent,
                OwnerId = "example-agent",
                DisplayName = "Example Knowledge",
                LifecycleStatus = "Published",
                IsAuthoritative = true,
                UpdatedUtc = DateTimeOffset.UtcNow
            };
        }

        private sealed class FakeDetailSource : IAiResourceDetailSource
        {
            public Task<AiResourceDetail> GetAsync(AiResourceInventoryItem resource, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var detail = new AiResourceDetail
                {
                    InventoryItem = resource,
                    Summary = "Example resource summary",
                    Content = "Example resource content"
                };
                detail.Sections.Add(new AiResourceDetailSection { Title = "Content", Content = "Example resource content" });
                detail.Validate();
                return Task.FromResult(detail);
            }
        }
    }
}
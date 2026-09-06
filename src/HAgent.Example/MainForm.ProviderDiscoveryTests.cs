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
        private static int _providerDiscoveryTabsAdded;

        private void AddProviderDiscoveryTabs()
        {
            if (Interlocked.Exchange(ref _providerDiscoveryTabsAdded, 1) != 0)
                return;

            AddApiTab(
                "PROVIDER DISCOVERY",
                "Run discovery test",
                "Verifies discovery-first behavior with complete discovery, partial catalog discovery, and unsupported adapters.",
                "Known metadata should be preserved; partial discovery must leave unavailable facts as Unknown; unsupported providers must not invent model information.",
                "Uses only deterministic local adapters.",
                TestProviderDiscoveryAsync,
                "Discovery boundary",
                "This test does not contact an external provider.");
        }

        private async Task TestProviderDiscoveryAsync(string unused)
        {
            var provider = new AiProvider
            {
                Id = "discovery-provider-42",
                Name = "Discovery Provider",
                Kind = "discovery-test",
                DefaultModel = "discovery-model"
            };

            var partialProvider = new AiProvider
            {
                Id = "catalog-provider-42",
                Name = "Catalog Provider",
                Kind = "catalog-test",
                DefaultModel = string.Empty
            };

            var service = new ProviderDiscoveryService(new IAiProviderAdapter[]
            {
                new DiscoveryTestAdapter(),
                new CatalogOnlyTestAdapter()
            });

            var complete = await service.DiscoverAsync(provider, string.Empty, CancellationToken.None).ConfigureAwait(true);
            if (!complete.Succeeded || complete.IsPartial || complete.Models.Count != 1)
                throw new InvalidOperationException("Complete provider discovery did not return the expected model metadata.");
            if (complete.Models[0].Cost != AiCostStatus.Free)
                throw new InvalidOperationException("Discovered cost metadata was not preserved.");
            if (complete.Models[0].Capabilities.Get(AiCapability.Chat) != CapabilitySupport.Supported)
                throw new InvalidOperationException("Discovered Chat capability was not preserved.");

            var partial = await service.DiscoverAsync(partialProvider, string.Empty, CancellationToken.None).ConfigureAwait(true);
            if (!partial.Succeeded || !partial.IsPartial || partial.Models.Count != 2)
                throw new InvalidOperationException("Partial model-catalog discovery did not return the expected partial result.");
            if (partial.Models[0].Cost != AiCostStatus.Unknown || partial.Models[1].Cost != AiCostStatus.Unknown)
                throw new InvalidOperationException("Unavailable cost metadata was not kept Unknown.");

            var unsupported = new ProviderDiscoveryService(new IAiProviderAdapter[] { new UnsupportedTestAdapter() });
            var unsupportedResult = await unsupported.DiscoverAsync(
                new AiProvider { Id = "unsupported-provider-42", Kind = "unsupported-test" },
                string.Empty,
                CancellationToken.None).ConfigureAwait(true);
            if (unsupportedResult.Succeeded || unsupportedResult.Models.Count != 0)
                throw new InvalidOperationException("Unsupported discovery capability incorrectly invented metadata.");

            Write(
                "PROVIDER DISCOVERY",
                "Provider discovery contract test succeeded." + Environment.NewLine +
                "Complete discovery metadata: verified." + Environment.NewLine +
                "Partial catalog discovery: verified." + Environment.NewLine +
                "Unknown unavailable metadata: verified." + Environment.NewLine +
                "Unsupported provider remains explicit: verified." + Environment.NewLine +
                "Complete discovered model: " + complete.Models[0].ModelId);
        }

        private sealed class DiscoveryTestAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public string Kind { get { return "discovery-test"; } }
            public string DisplayName { get { return "Discovery Test Adapter"; } }
            public bool CanHandle(AiProvider provider) { return provider != null && provider.Kind == Kind; }
            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AIResponse { Text = "unused" });
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                var capabilities = new AiModelCapabilities { Model = "discovery-model-42" };
                capabilities.Set(AiCapability.Chat, CapabilitySupport.Supported, CapabilitySource.ProviderMetadata, 1d, "deterministic discovery");
                return Task.FromResult(new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    IsPartial = false,
                    Message = "Complete metadata discovered.",
                    Models = { new AiModelMetadata
                    {
                        ProviderId = provider.Id,
                        ModelId = "discovery-model-42",
                        LogicalModelId = "logical-discovery-42",
                        DisplayName = "Discovery Model",
                        Cost = AiCostStatus.Free,
                        Source = AiMetadataSource.DiscoveryApi,
                        Capabilities = capabilities
                    } }
                });
            }
        }

        private sealed class CatalogOnlyTestAdapter : IAiProviderAdapter, IProviderModelCatalog
        {
            public string Kind { get { return "catalog-test"; } }
            public string DisplayName { get { return "Catalog Test Adapter"; } }
            public bool CanHandle(AiProvider provider) { return provider != null && provider.Kind == Kind; }
            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AIResponse { Text = "unused" });
            }
            public Task<IReadOnlyList<string>> GetModelsAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyList<string>>(new[] { "catalog-model-1", "catalog-model-2" });
            }
        }

        private sealed class UnsupportedTestAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "unsupported-test"; } }
            public string DisplayName { get { return "Unsupported Test Adapter"; } }
            public bool CanHandle(AiProvider provider) { return provider != null && provider.Kind == Kind; }
            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AIResponse { Text = "unused" });
            }
        }
    }
}

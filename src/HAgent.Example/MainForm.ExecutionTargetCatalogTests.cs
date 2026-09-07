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
        private static int _executionTargetCatalogTabsAdded;

        private void AddExecutionTargetCatalogTabs()
        {
            if (Interlocked.Exchange(ref _executionTargetCatalogTabsAdded, 1) != 0) return;
            AddApiTab(
                "EXECUTION TARGET CATALOG",
                "Run catalog test",
                "Verifies that discovered model metadata becomes canonical concrete execution targets without fabricating unavailable facts.",
                "Complete discovery metadata must reach the planner target unchanged; partial catalog metadata must remain explicitly Unknown.",
                "Uses only deterministic local adapters.",
                TestExecutionTargetCatalogAsync,
                "Target catalog boundary",
                "This test does not contact an external provider.");
        }

        private async Task TestExecutionTargetCatalogAsync(string unused)
        {
            var provider = new AiProvider
            {
                Id = "target-catalog-provider-42",
                Name = "Target Catalog Provider",
                Kind = "target-catalog-test",
                DefaultModel = string.Empty
            };

            var adapter = new TargetCatalogTestAdapter();
            var discovery = new ProviderDiscoveryService(new IAiProviderAdapter[] { adapter }, TimeSpan.FromMinutes(5));
            var catalog = new DefaultExecutionTargetCatalog(discovery, new EmptySecretStore());

            var targets = await catalog.GetTargetsAsync(new[] { provider }, CancellationToken.None).ConfigureAwait(true);
            if (targets.Count != 2) throw new InvalidOperationException("Target catalog did not materialize the discovered models.");

            var supported = FindTarget(targets, provider.Id + "::catalog-model-supported");
            if (supported.Cost != AiCostStatus.Free || supported.LogicalModelId != "logical-catalog-supported" || supported.Capabilities.Get(AiCapability.Chat) != CapabilitySupport.Supported)
                throw new InvalidOperationException("Complete discovery metadata was not preserved in the concrete execution target.");

            var unknown = FindTarget(targets, provider.Id + "::catalog-model-unknown");
            if (unknown.Cost != AiCostStatus.Unknown || unknown.Capabilities.Get(AiCapability.Chat) != CapabilitySupport.Unknown)
                throw new InvalidOperationException("Unknown discovery facts were incorrectly promoted to supported metadata.");

            Write(
                "EXECUTION TARGET CATALOG",
                "Execution target catalog contract test succeeded." + Environment.NewLine +
                "Discovered models materialized: verified." + Environment.NewLine +
                "Complete capability/cost metadata preserved: verified." + Environment.NewLine +
                "Unknown metadata remains Unknown: verified." + Environment.NewLine +
                "Canonical target identities: " + supported.Id + ", " + unknown.Id);
        }

        private sealed class TargetCatalogTestAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public string Kind { get { return "target-catalog-test"; } }
            public string DisplayName { get { return "Target Catalog Test Adapter"; } }
            public bool CanHandle(AiProvider provider) { return provider != null && provider.Kind == Kind; }
            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AIResponse { Text = "unused" });
            }
            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                var supportedCapabilities = new AiModelCapabilities { Model = "catalog-model-supported" };
                supportedCapabilities.Set(AiCapability.Chat, CapabilitySupport.Supported, CapabilitySource.ProviderMetadata, 1d, "deterministic catalog discovery");
                var unknownCapabilities = new AiModelCapabilities { Model = "catalog-model-unknown" };
                var result = new ProviderDiscoveryResult { Succeeded = true, IsPartial = false, Message = "Deterministic target catalog discovery." };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id, ModelId = "catalog-model-supported", LogicalModelId = "logical-catalog-supported",
                    DisplayName = "Supported Catalog Model", Cost = AiCostStatus.Free, Source = AiMetadataSource.ProviderMetadata, Capabilities = supportedCapabilities
                });
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id, ModelId = "catalog-model-unknown", LogicalModelId = "logical-catalog-unknown",
                    DisplayName = "Unknown Catalog Model", Cost = AiCostStatus.Unknown, Source = AiMetadataSource.DiscoveryApi, Capabilities = unknownCapabilities
                });
                return Task.FromResult(result);
            }
        }

        private sealed class EmptySecretStore : ISecretStore
        {
            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.FromResult(string.Empty); }
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        }

        private static AiExecutionTarget FindTarget(IReadOnlyList<AiExecutionTarget> targets, string targetId)
        {
            foreach (var target in targets)
                if (string.Equals(target.Id, targetId, StringComparison.OrdinalIgnoreCase)) return target;
            throw new InvalidOperationException("Execution target was not materialized: " + targetId);
        }
    }
}

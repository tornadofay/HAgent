using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Provider-neutral discovery orchestrator. Discovery failures produce explicit unknown/partial state
    /// instead of inventing model metadata.
    /// </summary>
    public sealed class ProviderDiscoveryService
    {
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;

        public ProviderDiscoveryService(IEnumerable<IAiProviderAdapter> adapters)
        {
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
        }

        public async Task<ProviderDiscoveryResult> DiscoverAsync(
            AiProvider provider,
            string apiKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            cancellationToken.ThrowIfCancellationRequested();

            var adapter = _adapters.FirstOrDefault(x => x != null && x.CanHandle(provider));
            if (adapter == null)
            {
                return new ProviderDiscoveryResult
                {
                    Message = "No registered adapter can handle provider kind '" + provider.Kind + "'."
                };
            }

            var discovery = adapter as IProviderDiscovery;
            if (discovery != null)
            {
                try
                {
                    var result = await discovery.DiscoverAsync(provider, apiKey ?? string.Empty, cancellationToken).ConfigureAwait(false);
                    if (result == null)
                        return new ProviderDiscoveryResult { Message = "Provider discovery returned no result." };

                    Normalize(result, provider.Id);
                    return result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    return new ProviderDiscoveryResult
                    {
                        Message = "Provider discovery failed: " + ex.Message
                    };
                }
            }

            var modelCatalog = adapter as IProviderModelCatalog;
            if (modelCatalog == null)
            {
                return new ProviderDiscoveryResult
                {
                    Message = "Provider exposes neither discovery nor model-catalog metadata."
                };
            }

            try
            {
                var models = await modelCatalog.GetModelsAsync(provider, apiKey ?? string.Empty, cancellationToken).ConfigureAwait(false);
                var result = new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    IsPartial = true,
                    Message = "Model catalog discovered; detailed metadata remains unknown where unavailable."
                };

                foreach (var modelId in models ?? new List<string>())
                {
                    if (string.IsNullOrWhiteSpace(modelId)) continue;
                    result.Models.Add(new AiModelMetadata
                    {
                        ProviderId = provider.Id,
                        ModelId = modelId,
                        LogicalModelId = modelId,
                        DisplayName = modelId,
                        Source = AiMetadataSource.DiscoveryApi,
                        Cost = AiCostStatus.Unknown,
                        Capabilities = new AiModelCapabilities { Model = modelId }
                    });
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ProviderDiscoveryResult
                {
                    Message = "Model catalog discovery failed: " + ex.Message
                };
            }
        }

        private static void Normalize(ProviderDiscoveryResult result, string providerId)
        {
            var unique = new Dictionary<string, AiModelMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (var metadata in result.Models ?? new List<AiModelMetadata>())
            {
                if (metadata == null) continue;
                metadata.ProviderId = providerId;
                if (string.IsNullOrWhiteSpace(metadata.ModelId)) continue;
                if (string.IsNullOrWhiteSpace(metadata.LogicalModelId)) metadata.LogicalModelId = metadata.ModelId;
                if (metadata.Capabilities == null) metadata.Capabilities = new AiModelCapabilities { Model = metadata.ModelId };
                if (string.IsNullOrWhiteSpace(metadata.Capabilities.Model)) metadata.Capabilities.Model = metadata.ModelId;
                if (metadata.Source == AiMetadataSource.Unknown) metadata.Source = AiMetadataSource.DiscoveryApi;
                metadata.Validate();
                unique[metadata.ModelId] = metadata;
            }

            result.Models.Clear();
            foreach (var metadata in unique.Values)
                result.Models.Add(metadata);
        }
    }
}

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
    /// Converts normalized discovery metadata into concrete execution targets.
    /// Discovery is cached by ProviderDiscoveryService, so target construction does not
    /// repeatedly contact a provider while metadata remains fresh.
    /// </summary>
    public sealed class DefaultExecutionTargetCatalog : IExecutionTargetCatalog
    {
        private readonly ProviderDiscoveryService _discovery;
        private readonly ISecretStore _secrets;

        public DefaultExecutionTargetCatalog(
            ProviderDiscoveryService discovery,
            ISecretStore secrets)
        {
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
        }

        public async Task<IReadOnlyList<AiExecutionTarget>> GetTargetsAsync(
            IReadOnlyList<AiProvider> providers,
            AiAgent agent,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var targets = new List<AiExecutionTarget>();
            if (providers == null)
                return targets.AsReadOnly();

            foreach (var provider in providers)
            {
                if (provider == null || !provider.Enabled)
                    continue;

                cancellationToken.ThrowIfCancellationRequested();

                var apiKey = string.Empty;
                if (!string.IsNullOrWhiteSpace(provider.SecretId))
                    apiKey = await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(false);

                ProviderDiscoveryResult discoveryResult;
                try
                {
                    discoveryResult = await _discovery
                        .DiscoverAsync(provider, apiKey, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    discoveryResult = null;
                }

                if (discoveryResult != null && discoveryResult.Models != null && discoveryResult.Models.Count > 0)
                {
                    foreach (var metadata in discoveryResult.Models)
                    {
                        if (metadata == null) continue;
                        targets.Add(CreateTarget(metadata));
                    }
                    continue;
                }

                // Discovery may be unavailable. Fall back only to provider-owned default
                // model metadata and keep capabilities/cost explicitly Unknown.
                var fallbackModel = provider.DefaultModel == null ? string.Empty : provider.DefaultModel.Trim();
                if (string.IsNullOrWhiteSpace(fallbackModel))
                    continue;

                targets.Add(new AiExecutionTarget
                {
                    Id = provider.Id + "::" + fallbackModel,
                    ProviderId = provider.Id,
                    ModelId = fallbackModel,
                    LogicalModelId = fallbackModel,
                    DeploymentId = provider.Id + "::" + fallbackModel,
                    Capabilities = new AiModelCapabilities { Model = fallbackModel },
                    Cost = AiCostStatus.Unknown,
                    Availability = AiAvailabilityState.Unknown
                });
            }

            foreach (var target in targets)
                target.Validate();

            return targets
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList()
                .AsReadOnly();
        }

        private static AiExecutionTarget CreateTarget(AiModelMetadata metadata)
        {
            return new AiExecutionTarget
            {
                Id = metadata.ProviderId + "::" + metadata.ModelId,
                ProviderId = metadata.ProviderId,
                ModelId = metadata.ModelId,
                LogicalModelId = string.IsNullOrWhiteSpace(metadata.LogicalModelId)
                    ? metadata.ModelId
                    : metadata.LogicalModelId,
                ModelVersion = metadata.Version ?? string.Empty,
                DeploymentId = metadata.ProviderId + "::" + metadata.ModelId,
                Capabilities = metadata.Capabilities ?? new AiModelCapabilities { Model = metadata.ModelId },
                Cost = metadata.Cost,
                Availability = AiAvailabilityState.Unknown
            };
        }
    }
}

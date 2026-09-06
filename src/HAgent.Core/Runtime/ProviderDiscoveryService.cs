using System;
using System.Collections.Concurrent;
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
    /// instead of inventing model metadata. Results are cached in-memory for the configured TTL.
    /// </summary>
    public sealed class ProviderDiscoveryService
    {
        private sealed class CacheEntry
        {
            public DateTimeOffset ExpiresAt;
            public Task<ProviderDiscoveryResult> Task;
        }

        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly TimeSpan _cacheDuration;
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        public ProviderDiscoveryService(IEnumerable<IAiProviderAdapter> adapters, TimeSpan? cacheDuration = null)
        {
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(15);
            if (_cacheDuration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(cacheDuration), "Discovery cache duration cannot be negative.");
        }

        public async Task<ProviderDiscoveryResult> DiscoverAsync(
            AiProvider provider,
            string apiKey,
            CancellationToken cancellationToken = default(CancellationToken),
            bool forceRefresh = false)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            cancellationToken.ThrowIfCancellationRequested();

            var cacheKey = BuildCacheKey(provider);
            if (!forceRefresh && _cacheDuration > TimeSpan.Zero)
            {
                CacheEntry cached;
                if (_cache.TryGetValue(cacheKey, out cached) && cached != null && cached.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    var cachedResult = await AwaitWithCancellationAsync(cached.Task, cancellationToken).ConfigureAwait(false);
                    return CloneResult(cachedResult);
                }
            }

            if (_cacheDuration > TimeSpan.Zero)
            {
                var entry = new CacheEntry
                {
                    ExpiresAt = DateTimeOffset.UtcNow.Add(_cacheDuration)
                };
                entry.Task = DiscoverUncachedAsync(provider, apiKey, CancellationToken.None);
                _cache[cacheKey] = entry;
                var result = await AwaitWithCancellationAsync(entry.Task, cancellationToken).ConfigureAwait(false);
                return CloneResult(result);
            }

            return await DiscoverUncachedAsync(provider, apiKey, cancellationToken).ConfigureAwait(false);
        }

        public void Invalidate(AiProvider provider)
        {
            if (provider == null) return;
            CacheEntry ignored;
            _cache.TryRemove(BuildCacheKey(provider), out ignored);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        private async Task<ProviderDiscoveryResult> DiscoverUncachedAsync(
            AiProvider provider,
            string apiKey,
            CancellationToken cancellationToken)
        {
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

        private static Task<ProviderDiscoveryResult> AwaitWithCancellationAsync(
            Task<ProviderDiscoveryResult> task,
            CancellationToken cancellationToken)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (!cancellationToken.CanBeCanceled || task.IsCompleted)
                return task;
            return AwaitWithCancellationCoreAsync(task, cancellationToken);
        }

        private static async Task<ProviderDiscoveryResult> AwaitWithCancellationCoreAsync(
            Task<ProviderDiscoveryResult> task,
            CancellationToken cancellationToken)
        {
            var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            var completed = await Task.WhenAny(task, cancellationTask).ConfigureAwait(false);
            if (completed != task)
                throw new OperationCanceledException(cancellationToken);
            return await task.ConfigureAwait(false);
        }

        private static string BuildCacheKey(AiProvider provider)
        {
            return string.Join("|", new[]
            {
                provider.Kind ?? string.Empty,
                provider.Id ?? string.Empty,
                provider.BaseUrl ?? string.Empty,
                provider.SecretId ?? string.Empty
            });
        }

        private static ProviderDiscoveryResult CloneResult(ProviderDiscoveryResult source)
        {
            var clone = new ProviderDiscoveryResult
            {
                Succeeded = source != null && source.Succeeded,
                IsPartial = source != null && source.IsPartial,
                Message = source == null ? string.Empty : source.Message
            };

            if (source != null)
            {
                foreach (var model in source.Models ?? new List<AiModelMetadata>())
                    clone.Models.Add(model == null ? null : model.Clone());
            }

            return clone;
        }

        private static void Normalize(ProviderDiscoveryResult result, string providerId)
        {
            var unique = new Dictionary<string, AiModelMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (var sourceMetadata in result.Models ?? new List<AiModelMetadata>())
            {
                if (sourceMetadata == null) continue;
                var metadata = sourceMetadata.Clone();
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

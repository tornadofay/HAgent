using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    internal sealed class AiModelCapabilityCache
    {
        private readonly ConcurrentDictionary<string, Lazy<Task<AiModelCapabilities>>> _entries =
            new ConcurrentDictionary<string, Lazy<Task<AiModelCapabilities>>>(StringComparer.OrdinalIgnoreCase);

        public async Task<AiModelCapabilities> GetOrCreateAsync(
            string key,
            Func<Task<AiModelCapabilities>> factory,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Capability cache key is required.", nameof(key));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            cancellationToken.ThrowIfCancellationRequested();

            var lazy = _entries.GetOrAdd(
                key,
                k => new Lazy<Task<AiModelCapabilities>>(
                    factory,
                    LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                return await lazy.Value.ConfigureAwait(false) ?? new AiModelCapabilities();
            }
            catch
            {
                Lazy<Task<AiModelCapabilities>> ignored;
                _entries.TryRemove(key, out ignored);
                throw;
            }
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}

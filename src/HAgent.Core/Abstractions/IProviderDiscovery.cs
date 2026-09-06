using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>Optional provider-adapter contract for discovery-first model metadata.</summary>
    public interface IProviderDiscovery
    {
        Task<ProviderDiscoveryResult> DiscoverAsync(
            AiProvider provider,
            string apiKey,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public sealed class ProviderDiscoveryResult
    {
        public ProviderDiscoveryResult()
        {
            Models = new List<AiModelMetadata>();
            Succeeded = false;
            IsPartial = false;
            Message = string.Empty;
        }

        public bool Succeeded { get; set; }
        public bool IsPartial { get; set; }
        public string Message { get; set; }
        public IList<AiModelMetadata> Models { get; private set; }
    }
}

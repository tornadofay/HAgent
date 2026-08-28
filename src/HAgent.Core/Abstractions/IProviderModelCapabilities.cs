using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>Optional provider-adapter capability for reporting model features.</summary>
    public interface IProviderModelCapabilities
    {
        Task<AiModelCapabilities> GetCapabilitiesAsync(
            AiProvider provider,
            string model,
            string apiKey,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

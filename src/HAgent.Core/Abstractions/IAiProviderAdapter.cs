using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiProviderAdapter
    {
        string Kind { get; }
        string DisplayName { get; }
        bool CanHandle(AiProvider provider);

        Task<AIResponse> SendAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IProviderToolCallingAdapter
    {
        Task<AIResponse> SendWithToolsAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface ISettingsProvider
    {
        Task<AiProvider> GetProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken));
        Task<AiAgent> GetAgentAsync(string agentId, CancellationToken cancellationToken = default(CancellationToken));
    }
}

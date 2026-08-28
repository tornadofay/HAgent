using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiStore
    {
        Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SaveProviderAsync(AiProvider provider, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveAgentAsync(AiAgent agent, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAgentAsync(string agentId, CancellationToken cancellationToken = default(CancellationToken));
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAgentRuntimeStateStore
    {
        Task SaveAsync(AgentRuntimeStateRecord record, CancellationToken cancellationToken = default(CancellationToken));
        Task<AgentRuntimeStateRecord> GetAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<AgentRuntimeStateRecord>> SearchAsync(AgentRuntimeStateQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken));
    }
}

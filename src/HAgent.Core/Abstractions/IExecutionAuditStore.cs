using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Persistence boundary for secret-safe terminal agent execution audit records.
    /// Implementations must never persist prompts, response payloads, secrets, or raw exceptions.
    /// </summary>
    public interface IExecutionAuditStore
    {
        Task AppendAsync(AgentExecutionAuditRecord record, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<AgentExecutionAuditRecord>> SearchAsync(ExecutionAuditQuery query, CancellationToken cancellationToken = default(CancellationToken));
    }
}

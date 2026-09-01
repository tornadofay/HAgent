using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Optional host-controlled scheduler for agent executions.
    /// The scheduler decides when work enters the runtime; the runtime remains responsible for execution semantics.
    /// </summary>
    public interface IAgentExecutionScheduler
    {
        Task<AgentExecution> ScheduleAsync(
            AgentRuntimeInstance instance,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

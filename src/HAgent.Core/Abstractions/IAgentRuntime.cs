using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAgentRuntime
    {
        event EventHandler<AgentExecutionEventArgs> ExecutionChanged;

        Task<AgentExecution> ExecuteAsync(
            string agentId,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<AgentExecution> ExecuteAsync(
            AgentExecutionRequest request,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

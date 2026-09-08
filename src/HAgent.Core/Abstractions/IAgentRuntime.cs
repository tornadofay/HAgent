using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Abstractions
{
    public interface IAgentRuntime
    {
        event EventHandler<AgentExecutionEventArgs> ExecutionChanged;

        IAiInterventionWorkflow InterventionWorkflow { get; }
        AiInterventionCoordinator InterventionCoordinator { get; }

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

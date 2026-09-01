using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IWorkspaceRouter
    {
        Task<WorkspaceMessage> RouteUserMessageAsync(
            AgentWorkspace workspace,
            string senderId,
            string content,
            string recipientId = null,
            string correlationId = null,
            string causationId = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<WorkspaceMessage> RouteAgentMessageAsync(
            AgentWorkspace workspace,
            string senderId,
            string recipientId,
            string content,
            string correlationId = null,
            string causationId = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

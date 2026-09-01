using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class WorkspaceRouter : IWorkspaceRouter
    {
        private readonly IWorkspaceRolePolicy _rolePolicy;
        private long _sequence;

        public WorkspaceRouter(IWorkspaceRolePolicy rolePolicy = null)
        {
            _rolePolicy = rolePolicy;
        }

        public Task<WorkspaceMessage> RouteUserMessageAsync(
            AgentWorkspace workspace,
            string senderId,
            string content,
            string recipientId = null,
            string correlationId = null,
            string causationId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWorkspace(workspace);
            cancellationToken.ThrowIfCancellationRequested();
            ValidateParticipant(workspace, senderId, WorkspaceParticipantKind.User);

            var targetId = string.IsNullOrWhiteSpace(recipientId)
                ? workspace.DefaultRecipientId
                : recipientId.Trim();

            WorkspaceParticipant target;
            ValidateParticipant(workspace, targetId, WorkspaceParticipantKind.Agent, out target);
            if (_rolePolicy != null && !_rolePolicy.CanReceiveUserMessages(target))
                throw new InvalidOperationException("Workspace role policy does not allow user messages for participant: " + target.ParticipantId);

            return Task.FromResult(CreateMessage(
                workspace,
                WorkspaceMessageKind.User,
                senderId,
                targetId,
                content,
                correlationId,
                causationId));
        }

        public Task<WorkspaceMessage> RouteAgentMessageAsync(
            AgentWorkspace workspace,
            string senderId,
            string recipientId,
            string content,
            string correlationId = null,
            string causationId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateWorkspace(workspace);
            cancellationToken.ThrowIfCancellationRequested();

            WorkspaceParticipant sender;
            WorkspaceParticipant recipient;
            ValidateParticipant(workspace, senderId, WorkspaceParticipantKind.Agent, out sender);
            ValidateParticipant(workspace, recipientId, WorkspaceParticipantKind.Agent, out recipient);
            if (_rolePolicy != null && !_rolePolicy.CanDelegate(sender, recipient))
                throw new InvalidOperationException("Workspace role policy does not allow delegation from '" + sender.ParticipantId + "' to '" + recipient.ParticipantId + "'.");

            return Task.FromResult(CreateMessage(
                workspace,
                WorkspaceMessageKind.Delegation,
                senderId,
                recipientId.Trim(),
                content,
                correlationId,
                causationId));
        }

        private WorkspaceMessage CreateMessage(
            AgentWorkspace workspace,
            WorkspaceMessageKind kind,
            string senderId,
            string recipientId,
            string content,
            string correlationId,
            string causationId)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Workspace message content is required.", nameof(content));

            return new WorkspaceMessage
            {
                WorkspaceId = workspace.WorkspaceId,
                Kind = kind,
                SenderId = senderId.Trim(),
                RecipientId = recipientId.Trim(),
                CorrelationId = string.IsNullOrWhiteSpace(correlationId)
                    ? Guid.NewGuid().ToString("N")
                    : correlationId.Trim(),
                CausationId = causationId == null ? string.Empty : causationId.Trim(),
                Sequence = Interlocked.Increment(ref _sequence),
                Content = content
            };
        }

        private static void ValidateWorkspace(AgentWorkspace workspace)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));
        }

        private static void ValidateParticipant(
            AgentWorkspace workspace,
            string participantId,
            WorkspaceParticipantKind expectedKind,
            out WorkspaceParticipant participant)
        {
            if (string.IsNullOrWhiteSpace(participantId))
                throw new ArgumentException("Workspace participant ID is required.", nameof(participantId));

            if (!workspace.TryGetParticipant(participantId, out participant))
                throw new InvalidOperationException("Workspace participant was not found: " + participantId);
            if (participant.State != WorkspaceParticipantState.Active)
                throw new InvalidOperationException("Workspace participant is not active: " + participantId);
            if (participant.Kind != expectedKind)
                throw new InvalidOperationException("Workspace participant has an invalid kind for this operation: " + participantId);
        }
    }
}

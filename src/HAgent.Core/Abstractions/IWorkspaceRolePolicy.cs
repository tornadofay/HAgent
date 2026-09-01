using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IWorkspaceRolePolicy
    {
        WorkspaceAgentRoleAssignment GetAssignment(string participantId);

        bool CanReceiveUserMessages(WorkspaceParticipant participant);

        bool CanDelegate(WorkspaceParticipant sender, WorkspaceParticipant recipient);
    }
}

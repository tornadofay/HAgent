using System;
using System.Collections.Generic;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class WorkspaceRolePolicy : IWorkspaceRolePolicy
    {
        private readonly Dictionary<string, WorkspaceAgentRoleAssignment> _assignments =
            new Dictionary<string, WorkspaceAgentRoleAssignment>(StringComparer.OrdinalIgnoreCase);

        public WorkspaceRolePolicy(IEnumerable<WorkspaceAgentRoleAssignment> assignments = null)
        {
            if (assignments == null) return;
            foreach (var assignment in assignments)
            {
                if (assignment == null || string.IsNullOrWhiteSpace(assignment.ParticipantId))
                    throw new ArgumentException("Workspace role assignment requires a participant ID.", nameof(assignments));
                _assignments[assignment.ParticipantId.Trim()] = assignment;
            }
        }

        public WorkspaceAgentRoleAssignment GetAssignment(string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId)) return null;
            WorkspaceAgentRoleAssignment assignment;
            if (_assignments.TryGetValue(participantId.Trim(), out assignment)) return assignment;
            return new WorkspaceAgentRoleAssignment
            {
                ParticipantId = participantId.Trim(),
                Role = WorkspaceAgentRole.Participant
            };
        }

        public bool CanReceiveUserMessages(WorkspaceParticipant participant)
        {
            if (participant == null || participant.Kind != WorkspaceParticipantKind.Agent)
                return false;
            return GetAssignment(participant.ParticipantId).CanReceiveUserMessages;
        }

        public bool CanDelegate(WorkspaceParticipant sender, WorkspaceParticipant recipient)
        {
            if (sender == null || recipient == null) return false;
            if (sender.Kind != WorkspaceParticipantKind.Agent || recipient.Kind != WorkspaceParticipantKind.Agent)
                return false;

            var senderAssignment = GetAssignment(sender.ParticipantId);
            var recipientAssignment = GetAssignment(recipient.ParticipantId);
            return senderAssignment.AllowsDelegationTo(recipientAssignment.Role);
        }
    }
}

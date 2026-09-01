using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum WorkspaceAgentRole
    {
        Participant,
        Coordinator,
        Specialist
    }

    public sealed class WorkspaceAgentRoleAssignment
    {
        public WorkspaceAgentRoleAssignment()
        {
            ParticipantId = string.Empty;
            Responsibility = string.Empty;
            AllowedTargetRoles = new List<WorkspaceAgentRole>();
            CanReceiveUserMessages = true;
            CanDelegate = false;
        }

        public string ParticipantId { get; set; }
        public WorkspaceAgentRole Role { get; set; }
        public string Responsibility { get; set; }
        public bool CanReceiveUserMessages { get; set; }
        public bool CanDelegate { get; set; }
        public IList<WorkspaceAgentRole> AllowedTargetRoles { get; set; }

        public bool AllowsDelegationTo(WorkspaceAgentRole targetRole)
        {
            if (!CanDelegate) return false;
            if (AllowedTargetRoles == null || AllowedTargetRoles.Count == 0) return true;
            foreach (var role in AllowedTargetRoles)
            {
                if (role == targetRole) return true;
            }
            return false;
        }
    }
}

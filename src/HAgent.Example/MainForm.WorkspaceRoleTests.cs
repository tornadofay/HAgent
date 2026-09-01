using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestWorkspaceRolesAsync(string message)
        {
            var workspace = new AgentWorkspace("workspace-roles-42", "Workspace Roles");
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "user-42",
                Kind = WorkspaceParticipantKind.User,
                DisplayName = "User"
            });
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "coordinator-42",
                Kind = WorkspaceParticipantKind.Agent,
                DisplayName = "Coordinator",
                ProfileId = "profile-coordinator-42",
                RuntimeInstanceId = "runtime-coordinator-42"
            }, true);
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "specialist-42",
                Kind = WorkspaceParticipantKind.Agent,
                DisplayName = "Specialist",
                ProfileId = "profile-specialist-42",
                RuntimeInstanceId = "runtime-specialist-42"
            });
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "peer-42",
                Kind = WorkspaceParticipantKind.Agent,
                DisplayName = "Peer",
                ProfileId = "profile-peer-42",
                RuntimeInstanceId = "runtime-peer-42"
            });

            var policy = new WorkspaceRolePolicy(new[]
            {
                new WorkspaceAgentRoleAssignment
                {
                    ParticipantId = "coordinator-42",
                    Role = WorkspaceAgentRole.Coordinator,
                    Responsibility = "Workspace coordination",
                    CanReceiveUserMessages = true,
                    CanDelegate = true,
                    AllowedTargetRoles = new List<WorkspaceAgentRole> { WorkspaceAgentRole.Specialist }
                },
                new WorkspaceAgentRoleAssignment
                {
                    ParticipantId = "specialist-42",
                    Role = WorkspaceAgentRole.Specialist,
                    Responsibility = "Customer data subsystem",
                    CanReceiveUserMessages = false,
                    CanDelegate = false
                },
                new WorkspaceAgentRoleAssignment
                {
                    ParticipantId = "peer-42",
                    Role = WorkspaceAgentRole.Participant,
                    Responsibility = "General participant",
                    CanReceiveUserMessages = true,
                    CanDelegate = false
                }
            });

            IWorkspaceRouter router = new WorkspaceRouter(policy);
            var userMessage = await router.RouteUserMessageAsync(
                workspace,
                "user-42",
                "workspace-role-user-42",
                correlationId: "role-correlation-42").ConfigureAwait(true);
            if (userMessage.RecipientId != "coordinator-42")
                throw new InvalidOperationException("The workspace default recipient should remain the coordinator.");

            var delegation = await router.RouteAgentMessageAsync(
                workspace,
                "coordinator-42",
                "specialist-42",
                "workspace-role-delegation-42",
                correlationId: "role-correlation-43",
                causationId: userMessage.MessageId).ConfigureAwait(true);
            if (delegation.RecipientId != "specialist-42")
                throw new InvalidOperationException("Coordinator delegation did not reach the specialist.");
            if (delegation.CausationId != userMessage.MessageId)
                throw new InvalidOperationException("Delegation causation was not preserved.");

            var specialistRejected = false;
            try
            {
                await router.RouteAgentMessageAsync(
                    workspace,
                    "specialist-42",
                    "peer-42",
                    "specialist-delegation-not-allowed").ConfigureAwait(true);
            }
            catch (InvalidOperationException)
            {
                specialistRejected = true;
            }

            if (!specialistRejected)
                throw new InvalidOperationException("Specialist delegation should have been rejected by role policy.");

            var role = policy.GetAssignment("specialist-42");
            if (role == null || role.Role != WorkspaceAgentRole.Specialist ||
                !string.Equals(role.Responsibility, "Customer data subsystem", StringComparison.Ordinal))
                throw new InvalidOperationException("Specialist role metadata was not preserved.");

            Write("WORKSPACE ROLES",
                "Contract test succeeded." + Environment.NewLine +
                "Coordinator/specialist roles are policy metadata over ordinary agent participants: yes" + Environment.NewLine +
                "Default user recipient: coordinator-42" + Environment.NewLine +
                "Coordinator → specialist delegation: allowed" + Environment.NewLine +
                "Specialist → peer delegation: rejected" + Environment.NewLine +
                "Specialist responsibility metadata: Customer data subsystem" + Environment.NewLine +
                "Participant/runtime-agent classes introduced: none");
        }
    }
}

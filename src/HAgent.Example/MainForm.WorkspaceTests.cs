using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddWorkspaceRoutingTab()
        {
            AddApiTab(
                "WORKSPACE ROUTING",
                "Run workspace routing test",
                "Verifies provider-neutral workspace participants, default-recipient routing, explicit addressing, and sender/recipient correlation metadata.",
                "An unaddressed user message must reach only the workspace default agent; addressed messages and agent delegation must target the explicit active recipient.",
                "Workspace routing verification.",
                TestWorkspaceRoutingAsync,
                "Default + explicit routing",
                "Provider-independent deterministic model test; no network or storage mutation.");
        }

        private Task TestWorkspaceRoutingAsync(string message)
        {
            var workspace = new AgentWorkspace("workspace-routing-42", "Workspace Routing Test");
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "user-42",
                Kind = WorkspaceParticipantKind.User,
                DisplayName = "Example User"
            });
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "agent-default-42",
                Kind = WorkspaceParticipantKind.Agent,
                DisplayName = "Default Agent"
            }, makeDefault: true);
            workspace.AddParticipant(new WorkspaceParticipant
            {
                ParticipantId = "agent-second-42",
                Kind = WorkspaceParticipantKind.Agent,
                DisplayName = "Second Agent"
            });

            var router = new WorkspaceRouter();
            var userMessage = router.RouteUserMessageAsync(
                workspace,
                "user-42",
                "workspace-routing-42",
                correlationId: "workspace-correlation-42").GetAwaiter().GetResult();

            if (!string.Equals(userMessage.RecipientId, workspace.DefaultRecipientId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("An unaddressed user message did not route to the workspace default recipient.");
            if (!string.Equals(userMessage.SenderId, "user-42", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Workspace user sender identity was not preserved.");
            if (!string.Equals(userMessage.CorrelationId, "workspace-correlation-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Workspace correlation ID was not preserved.");

            var addressed = router.RouteUserMessageAsync(
                workspace,
                "user-42",
                "addressed-message",
                recipientId: "agent-second-42",
                correlationId: "workspace-addressed-42").GetAwaiter().GetResult();

            if (!string.Equals(addressed.RecipientId, "agent-second-42", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Explicit user-to-agent addressing did not target the requested recipient.");

            var delegation = router.RouteAgentMessageAsync(
                workspace,
                "agent-default-42",
                "agent-second-42",
                "delegated-message",
                correlationId: "workspace-delegation-42",
                causationId: userMessage.MessageId).GetAwaiter().GetResult();

            if (delegation.Kind != WorkspaceMessageKind.Delegation)
                throw new InvalidOperationException("Agent delegation was not classified as a delegation message.");
            if (!string.Equals(delegation.CausationId, userMessage.MessageId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Workspace delegation did not preserve causation metadata.");
            if (delegation.Sequence <= userMessage.Sequence)
                throw new InvalidOperationException("Workspace message sequence did not advance monotonically.");

            var rejected = false;
            try
            {
                router.RouteUserMessageAsync(
                    workspace,
                    "user-42",
                    "suspended-target",
                    recipientId: "agent-missing-42").GetAwaiter().GetResult();
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }

            if (!rejected)
                throw new InvalidOperationException("Routing accepted a message for a non-existent participant.");

            Write("WORKSPACE ROUTING",
                "Contract test succeeded." + Environment.NewLine +
                "Workspace: " + workspace.WorkspaceId + Environment.NewLine +
                "Default recipient: " + workspace.DefaultRecipientId + Environment.NewLine +
                "Unaddressed user message recipient: " + userMessage.RecipientId + Environment.NewLine +
                "Explicit user-to-agent recipient: " + addressed.RecipientId + Environment.NewLine +
                "Delegation sender: " + delegation.SenderId + Environment.NewLine +
                "Delegation recipient: " + delegation.RecipientId + Environment.NewLine +
                "Correlation preserved: yes" + Environment.NewLine +
                "Causation preserved: yes" + Environment.NewLine +
                "Unknown recipient rejected: yes");

            return Task.CompletedTask;
        }
    }
}

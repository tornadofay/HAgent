using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        static MainForm()
        {
            Application.Idle += OnExampleIdentityModuleIdle;
        }

        private static void OnExampleIdentityModuleIdle(object sender, EventArgs e)
        {
            for (var i = 0; i < Application.OpenForms.Count; i++)
            {
                var form = Application.OpenForms[i] as MainForm;
                if (form == null) continue;

                Application.Idle -= OnExampleIdentityModuleIdle;
                form.AddIdentityFeatureTabs();
                return;
            }
        }

        private void AddIdentityFeatureTabs()
        {
            AddApiTab(
                "Identity Snapshot",
                "Run snapshot test",
                "Builds an execution snapshot from a host-supplied identity and verifies the snapshot owns a separate immutable identity copy.",
                "All identity fields must match the source context, the snapshot identity must be a different object, and a missing identity must resolve to an empty safe context.",
                "No AI request is sent by this example.",
                TestIdentitySnapshotAsync,
                "Identity boundary",
                "This verifies the execution snapshot boundary independently of providers, storage, authentication, or UI hosts.");

            AddApiTab(
                "Identity Execution",
                "Run execution test",
                "Sends the canonical AgentExecutionRequest with an explicit deployment, tenant, principal, user, session, and workspace identity, then verifies execution and audit propagation.",
                "The completed execution and its audit projection must preserve every identity field exactly; no identity is inferred from the prompt.",
                "Reply with exactly IDENTITY-OK and nothing else.",
                TestIdentityExecutionAsync,
                "Runtime boundary",
                "This makes one live provider request because it verifies the public request-to-execution propagation path end to end.");

            AddApiTab(
                "Identity Tool",
                "Run tool propagation test",
                "Executes a deterministic local tool with an explicit identity context and verifies the same identity reaches the handler and the resulting ToolExecutionResult.",
                "The handler context and result must preserve deployment, tenant, principal, user, session, and workspace identity, with no provider request required.",
                "HAgent identity-tool-42",
                TestIdentityToolAsync,
                "Tool boundary",
                "This verifies the identity-aware ExecuteToolAsync overload and keeps host authentication outside HAgent.");
        }

        private Task TestIdentitySnapshotAsync(string unused)
        {
            var source = new AgentIdentityContext(
                deploymentId: "deployment-42",
                tenantId: "tenant-7",
                principalId: "principal-19",
                displayName: "Example User",
                userId: "user-19",
                sessionId: "session-42",
                workspaceId: "workspace-3");

            var agent = new AiAgent
            {
                Id = "identity-agent-42",
                Name = "Identity Example Agent",
                Enabled = true
            };

            var snapshot = new AgentExecutionSnapshot(
                agent,
                new List<AiProvider>(),
                null,
                null,
                source);

            if (ReferenceEquals(snapshot.Identity, source))
                throw new InvalidOperationException("Execution snapshot reused the caller's identity object instead of cloning it.");
            AssertIdentityEquals(source, snapshot.Identity, "Snapshot identity");

            var emptySnapshot = new AgentExecutionSnapshot(agent, new List<AiProvider>());
            AssertIdentityEmpty(emptySnapshot.Identity, "Missing identity");

            Write(
                "IDENTITY SNAPSHOT",
                "Snapshot test succeeded." + Environment.NewLine +
                "Source identity and snapshot identity values: verified." + Environment.NewLine +
                "Separate identity object: verified." + Environment.NewLine +
                "Missing identity -> empty safe context: verified." + Environment.NewLine +
                FormatIdentity(snapshot.Identity));

            return Task.CompletedTask;
        }

        private async Task TestIdentityExecutionAsync(string message)
        {
            var selection = await CreateClientAndAgentAsync();
            var requestText = RequireInput(message);
            var expected = new AgentIdentityContext(
                deploymentId: "example-deployment-42",
                tenantId: "tenant-example-7",
                principalId: "principal-example-19",
                displayName: "HAgent Example User",
                userId: "user-example-19",
                sessionId: "session-example-42",
                workspaceId: "workspace-example-3");

            var execution = await selection.Client.ExecuteAsync(
                new AgentExecutionRequest
                {
                    AgentId = selection.Agent.Id,
                    Messages = new[] { new AIMessage("user", requestText) },
                    HostCorrelationId = "identity-host-correlation-42",
                    Identity = expected
                },
                CancellationToken.None);

            AssertIdentityEquals(expected, execution.Identity, "Execution identity");

            var audit = AgentExecutionAuditRecord.FromExecution(execution);
            if (!string.Equals(audit.DeploymentId, expected.DeploymentId, StringComparison.Ordinal) ||
                !string.Equals(audit.TenantId, expected.TenantId, StringComparison.Ordinal) ||
                !string.Equals(audit.PrincipalId, expected.PrincipalId, StringComparison.Ordinal) ||
                !string.Equals(audit.UserId, expected.UserId, StringComparison.Ordinal) ||
                !string.Equals(audit.SessionId, expected.SessionId, StringComparison.Ordinal) ||
                !string.Equals(audit.WorkspaceId, expected.WorkspaceId, StringComparison.Ordinal))
                throw new InvalidOperationException("Audit projection did not preserve the complete execution identity.");

            Write(
                "IDENTITY EXECUTION",
                "Execution identity propagation succeeded." + Environment.NewLine +
                "Execution: " + execution.Id + Environment.NewLine +
                "State: " + execution.State + Environment.NewLine +
                "Response: " + (execution.Response == null ? string.Empty : execution.Response.Text) + Environment.NewLine +
                FormatIdentity(execution.Identity) + Environment.NewLine +
                "Audit identity projection: verified." + Environment.NewLine +
                "Host correlation: " + execution.HostCorrelationId);
        }

        private async Task TestIdentityToolAsync(string input)
        {
            var client = CreateToolTestClient();
            var expected = new AgentIdentityContext(
                deploymentId: "deployment-tool-42",
                tenantId: "tenant-tool-7",
                principalId: "principal-tool-19",
                displayName: "Tool User",
                userId: "user-tool-19",
                sessionId: "session-tool-42",
                workspaceId: "workspace-tool-3");

            AgentIdentityContext observed = null;
            var definition = new AiTool
            {
                Id = "example.identity-tool",
                Name = "Identity Echo",
                Description = "Returns a deterministic value while exposing the received identity to the test.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                Category = "Example",
                Enabled = true
            };

            client.RegisterTool(new DelegateAgentTool(definition, context =>
            {
                observed = context.Identity == null ? null : context.Identity.Clone();
                object value;
                context.Arguments.TryGetValue("value", out value);
                return Task.FromResult(ToolExecutionResult.Success(Convert.ToString(value) ?? string.Empty));
            }));

            var valueText = string.IsNullOrWhiteSpace(input) ? "HAgent identity-tool-42" : input.Trim();
            var result = await client.ExecuteToolAsync(
                "identity-agent-42",
                definition.Id,
                "identity-tool-call-42",
                new Dictionary<string, object> { { "value", valueText } },
                CancellationToken.None,
                "identity-tool-host-42",
                expected);

            if (!result.Succeeded)
                throw new InvalidOperationException("Identity tool execution failed: " + result.Error);
            AssertIdentityEquals(expected, observed, "Tool handler identity");
            AssertIdentityEquals(expected, result.Identity, "Tool result identity");
            if (!string.Equals(result.Output, valueText, StringComparison.Ordinal))
                throw new InvalidOperationException("Identity tool did not return the expected deterministic value.");

            Write(
                "IDENTITY TOOL",
                "Tool identity propagation succeeded." + Environment.NewLine +
                "Tool call: " + result.ToolCallId + Environment.NewLine +
                "Handler identity: verified." + Environment.NewLine +
                "Result identity: verified." + Environment.NewLine +
                "Output: " + result.Output + Environment.NewLine +
                FormatIdentity(result.Identity));
        }

        private static void AssertIdentityEquals(AgentIdentityContext expected, AgentIdentityContext actual, string name)
        {
            if (actual == null)
                throw new InvalidOperationException(name + " is null.");

            if (!string.Equals(actual.DeploymentId, expected.DeploymentId, StringComparison.Ordinal) ||
                !string.Equals(actual.TenantId, expected.TenantId, StringComparison.Ordinal) ||
                !string.Equals(actual.PrincipalId, expected.PrincipalId, StringComparison.Ordinal) ||
                !string.Equals(actual.DisplayName, expected.DisplayName, StringComparison.Ordinal) ||
                !string.Equals(actual.UserId, expected.UserId, StringComparison.Ordinal) ||
                !string.Equals(actual.SessionId, expected.SessionId, StringComparison.Ordinal) ||
                !string.Equals(actual.WorkspaceId, expected.WorkspaceId, StringComparison.Ordinal))
                throw new InvalidOperationException(name + " did not preserve all identity fields.");
        }

        private static void AssertIdentityEmpty(AgentIdentityContext identity, string name)
        {
            if (identity == null ||
                !string.IsNullOrEmpty(identity.DeploymentId) ||
                !string.IsNullOrEmpty(identity.TenantId) ||
                !string.IsNullOrEmpty(identity.PrincipalId) ||
                !string.IsNullOrEmpty(identity.DisplayName) ||
                !string.IsNullOrEmpty(identity.UserId) ||
                !string.IsNullOrEmpty(identity.SessionId) ||
                !string.IsNullOrEmpty(identity.WorkspaceId))
                throw new InvalidOperationException(name + " is not a safe empty identity context.");
        }

        private static string FormatIdentity(AgentIdentityContext identity)
        {
            return "Deployment: " + identity.DeploymentId + Environment.NewLine +
                   "Tenant: " + identity.TenantId + Environment.NewLine +
                   "Principal: " + identity.PrincipalId + Environment.NewLine +
                   "Display name: " + identity.DisplayName + Environment.NewLine +
                   "User: " + identity.UserId + Environment.NewLine +
                   "Session: " + identity.SessionId + Environment.NewLine +
                   "Workspace: " + identity.WorkspaceId;
        }
    }
}

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
        private void AddApprovalWorkflowTab()
        {
            AddApiTab(
                "Approval Workflow",
                "Run approval workflow test",
                "Verifies policy-driven approval and deferral requests at a side-effect boundary, explicit resolution identity/correlation, pending-request tracking, terminal-state protection, and separation between approval state and actual tool execution.",
                "RequireApproval and Defer must create bounded reviewable requests without running the handler. Resolution is explicit and terminal; approval never bypasses the execution boundary or silently resumes work.",
                "Uses a local deterministic policy engine, in-memory approval workflow, and a local executable tool only.",
                TestApprovalWorkflowAsync,
                "Approval boundary",
                "The unified policy engine decides that approval or deferral is required. The approval workflow tracks the human/host decision; it does not grant authorization or execute the requested operation.");
        }

        private async Task TestApprovalWorkflowAsync(string unused)
        {
            const string agentId = "approval-agent-42";
            const string toolId = "approval-tool-42";
            var handlerInvocations = 0;

            var store = new InMemoryAiStore();
            var agent = new AiAgent
            {
                Id = agentId,
                Name = "Approval Workflow Agent",
                Enabled = true
            };
            await store.SaveAgentAsync(agent, CancellationToken.None).ConfigureAwait(true);

            var policy = new AiPolicySet { Version = "approval-policy-42" };
            var approvalRule = new AiPolicyRule
            {
                Id = "approval-tool-rule-42",
                Name = "Require tool approval",
                Scope = AiPolicyScopeKind.Tool,
                ScopeId = toolId,
                Priority = 100,
                Outcome = AiPolicyOutcome.RequireApproval,
                Reason = "The tool requires explicit human approval."
            };
            approvalRule.Operations.Add("tool.invoke");
            policy.Rules.Add(approvalRule);

            var client = new HAgentClient(
                store,
                new EmptySecretStore(),
                new IAiProviderAdapter[0],
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new DefaultAiPolicyEngine(policy));

            client.RegisterTool(new DelegateAgentTool(new AiTool
            {
                Id = toolId,
                Name = "approval_tool",
                Description = "Deterministic approval workflow tool.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                Type = AiToolType.Application,
                Enabled = true
            }, context =>
            {
                handlerInvocations++;
                return Task.FromResult(ToolExecutionResult.Success("executed"));
            }));

            var requesterIdentity = new AgentIdentityContext(
                tenantId: "tenant-42",
                userId: "requester-42",
                workspaceId: "workspace-42");

            var approvalResult = await client.ExecuteToolAsync(
                agentId,
                toolId,
                "approval-call-42",
                new Dictionary<string, object> { { "value", "pending" } },
                CancellationToken.None,
                "host-correlation-42",
                requesterIdentity).ConfigureAwait(true);

            if (approvalResult.Succeeded || handlerInvocations != 0)
                throw new InvalidOperationException("RequireApproval unexpectedly executed the tool handler.");
            if (approvalResult.PolicyDecision == null || !approvalResult.PolicyDecision.RequiresApproval)
                throw new InvalidOperationException("RequireApproval result did not preserve the policy decision.");
            if (approvalResult.PolicyDecision.PolicyVersion != "approval-policy-42" ||
                approvalResult.PolicyDecision.RuleId != "approval-tool-rule-42")
                throw new InvalidOperationException("Approval result did not preserve policy provenance.");
            if (approvalResult.ApprovalRequest == null)
                throw new InvalidOperationException("RequireApproval did not create a review request.");
            if (approvalResult.ApprovalRequest.Kind != AiApprovalRequestKind.Approval ||
                approvalResult.ApprovalRequest.Status != AiApprovalRequestStatus.Pending)
                throw new InvalidOperationException("Approval request did not start in the expected pending approval state.");
            if (approvalResult.ApprovalRequest.Operation != "tool.invoke" ||
                approvalResult.ApprovalRequest.ResourceType != "tool" ||
                approvalResult.ApprovalRequest.ResourceId != toolId ||
                approvalResult.ApprovalRequest.ToolId != toolId ||
                approvalResult.ApprovalRequest.CorrelationId != approvalResult.CorrelationId ||
                approvalResult.ApprovalRequest.HostCorrelationId != "host-correlation-42" ||
                approvalResult.ApprovalRequest.RequesterIdentity.UserId != requesterIdentity.UserId)
                throw new InvalidOperationException("Approval request did not preserve operation, correlation, or requester identity metadata.");

            var pending = await client.GetPendingApprovalRequestsAsync(CancellationToken.None).ConfigureAwait(true);
            if (pending.Count != 1 || pending[0].RequestId != approvalResult.ApprovalRequest.RequestId)
                throw new InvalidOperationException("Pending approval request tracking is incorrect.");

            var approved = await client.ResolveApprovalRequestAsync(
                approvalResult.ApprovalRequest.RequestId,
                true,
                new AgentIdentityContext(tenantId: "tenant-42", userId: "approver-42", workspaceId: "workspace-42"),
                "Approved by deterministic Example verification.",
                CancellationToken.None).ConfigureAwait(true);
            if (approved.Status != AiApprovalRequestStatus.Approved ||
                approved.ResponderIdentity == null ||
                approved.ResponderIdentity.UserId != "approver-42" ||
                approved.ResolvedAt == null ||
                approved.ResolutionReason != "Approved by deterministic Example verification.")
                throw new InvalidOperationException("Approved request did not record an explicit terminal resolution.");
            if (handlerInvocations != 0)
                throw new InvalidOperationException("Resolving an approval unexpectedly executed the protected tool.");

            try
            {
                await client.ResolveApprovalRequestAsync(
                    approvalResult.ApprovalRequest.RequestId,
                    false,
                    new AgentIdentityContext(tenantId: "tenant-42", userId: "second-responder-42"),
                    "Late resolution attempt.",
                    CancellationToken.None).ConfigureAwait(true);
                throw new InvalidOperationException("A terminal approval request accepted a second resolution.");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.IndexOf("no longer pending", StringComparison.OrdinalIgnoreCase) < 0)
                    throw;
            }

            var deferPolicy = new AiPolicySet { Version = "defer-policy-42" };
            var deferRule = new AiPolicyRule
            {
                Id = "defer-tool-rule-42",
                Name = "Defer tool",
                Scope = AiPolicyScopeKind.Tool,
                ScopeId = toolId,
                Priority = 100,
                Outcome = AiPolicyOutcome.Defer,
                Reason = "Tool execution is deferred until the host is ready."
            };
            deferRule.Operations.Add("tool.invoke");
            deferPolicy.Rules.Add(deferRule);

            var deferredClient = new HAgentClient(
                store,
                new EmptySecretStore(),
                new IAiProviderAdapter[0],
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new DefaultAiPolicyEngine(deferPolicy));
            var deferred = await deferredClient.ExecuteToolAsync(
                agentId,
                toolId,
                "defer-call-42",
                new Dictionary<string, object> { { "value", "deferred" } },
                CancellationToken.None,
                "host-correlation-43",
                requesterIdentity).ConfigureAwait(true);
            if (deferred.Succeeded || deferred.ApprovalRequest == null ||
                deferred.ApprovalRequest.Kind != AiApprovalRequestKind.Deferral ||
                deferred.ApprovalRequest.Status != AiApprovalRequestStatus.Pending)
                throw new InvalidOperationException("Defer did not create a distinct pending deferral request.");

            var pendingAfterDefer = await deferredClient.GetPendingApprovalRequestsAsync(CancellationToken.None).ConfigureAwait(true);
            if (pendingAfterDefer.Count != 1 || pendingAfterDefer[0].Kind != AiApprovalRequestKind.Deferral)
                throw new InvalidOperationException("Pending deferral request was not tracked independently.");

            var rejected = await deferredClient.ResolveApprovalRequestAsync(
                deferred.ApprovalRequest.RequestId,
                false,
                new AgentIdentityContext(tenantId: "tenant-42", userId: "rejector-42"),
                "Deferred operation was rejected.",
                CancellationToken.None).ConfigureAwait(true);
            if (rejected.Status != AiApprovalRequestStatus.Rejected)
                throw new InvalidOperationException("Deferral request did not support explicit rejection.");

            var pendingFinal = await deferredClient.GetPendingApprovalRequestsAsync(CancellationToken.None).ConfigureAwait(true);
            if (pendingFinal.Count != 0)
                throw new InvalidOperationException("Resolved requests remained in the pending approval collection.");

            Write(
                "APPROVAL WORKFLOW",
                "Contract test succeeded." + Environment.NewLine +
                "RequireApproval creates pending request: verified." + Environment.NewLine +
                "Policy provenance preserved: verified." + Environment.NewLine +
                "Correlation and requester identity propagation: verified." + Environment.NewLine +
                "Pending request tracking: verified." + Environment.NewLine +
                "Explicit approval resolution: verified." + Environment.NewLine +
                "Approval does not execute protected tool: verified." + Environment.NewLine +
                "Terminal-state protection: verified." + Environment.NewLine +
                "Defer creates distinct pending request: verified." + Environment.NewLine +
                "Explicit rejection resolution: verified." + Environment.NewLine +
                "Pending request removal after resolution: verified." + Environment.NewLine +
                "Handler invocation count: " + handlerInvocations);
        }
    }
}

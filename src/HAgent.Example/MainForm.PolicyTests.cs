using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _policyTabsAdded;

        private void AddPolicyTabs()
        {
            if (Interlocked.Exchange(ref _policyTabsAdded, 1) != 0)
                return;

            AddApiTab(
                "Unified Policy",
                "Run policy contract test",
                "Verifies deterministic policy outcomes, scope/priority precedence, provenance, resource/tool/provider matching, and the built-in FreeOnly cost boundary.",
                "A policy decision must be reproducible from the same inputs and explain the exact rule or built-in guard that produced it.",
                "No provider or external service is used.",
                TestPolicyEngineAsync,
                "Policy boundary",
                "Policy is enforcement metadata, not prompt text. Host authentication and business authorization remain host-owned.");
        }

        private Task TestPolicyEngineAsync(string unused)
        {
            var policy = new AiPolicySet { Version = "policy-contract-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "system-default-allow",
                Name = "System default",
                Scope = AiPolicyScopeKind.System,
                Priority = 1,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "System default allows this operation."
            });
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "tenant-deny-tool",
                Name = "Tenant tool restriction",
                Scope = AiPolicyScopeKind.Tenant,
                ScopeId = "tenant-42",
                Priority = 5,
                Outcome = AiPolicyOutcome.Deny,
                Reason = "Tenant policy blocks the tool operation.",
                Operations = new List<string> { "tool.invoke" }
            });
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "agent-approval",
                Name = "Agent approval",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "agent-42",
                Priority = 20,
                Outcome = AiPolicyOutcome.RequireApproval,
                Reason = "The selected agent requires approval for this operation.",
                Operations = new List<string> { "tool.invoke" },
                ToolIds = new List<string> { "tool-sensitive" }
            });
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "execution-allow",
                Name = "Execution-specific allowance",
                Scope = AiPolicyScopeKind.Execution,
                ScopeId = "execution-42",
                Priority = 10,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "Execution-specific allowance."
            });

            var engine = new DefaultAiPolicyEngine(policy);
            var context = new AiPolicyEvaluationContext
            {
                Operation = "tool.invoke",
                ResourceType = "tool",
                ResourceId = "tool-sensitive",
                AgentProfileId = "agent-42",
                RuntimeInstanceId = "runtime-42",
                ExecutionId = "execution-42",
                ToolId = "tool-sensitive",
                ProviderId = "provider-42",
                ExecutionTargetId = "provider-42::model-42",
                CostStatus = AiCostStatus.Free,
                RequestedCostPolicy = AiCostPolicy.NoRestriction,
                Identity = new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42", workspaceId: "workspace-42")
            };

            var decision = engine.Evaluate(context);
            if (decision.Outcome != AiPolicyOutcome.RequireApproval || decision.RuleId != "agent-approval")
                throw new InvalidOperationException("Policy precedence did not select the expected agent approval rule.");
            if (decision.PolicyVersion != "policy-contract-42" || !decision.RequiresApproval)
                throw new InvalidOperationException("Policy decision provenance/version was not preserved.");

            context.ToolId = "tool-safe";
            context.ResourceId = "tool-safe";
            var tenantDecision = engine.Evaluate(context);
            if (tenantDecision.Outcome != AiPolicyOutcome.Deny || tenantDecision.RuleId != "tenant-deny-tool")
                throw new InvalidOperationException("Tenant-scoped denial did not apply to the matching tool operation.");

            context.Operation = "memory.read";
            var systemDecision = engine.Evaluate(context);
            if (systemDecision.Outcome != AiPolicyOutcome.Allow || systemDecision.RuleId != "system-default-allow")
                throw new InvalidOperationException("Unmatched operations did not fall back to the applicable system policy.");

            context.Operation = "model.invoke";
            context.CostStatus = AiCostStatus.Paid;
            context.RequestedCostPolicy = AiCostPolicy.FreeOnly;
            var paidDecision = engine.Evaluate(context);
            if (!paidDecision.IsDenied || !paidDecision.IsBuiltIn || paidDecision.RuleId != "builtin.cost.free-only")
                throw new InvalidOperationException("FreeOnly did not enforce the built-in cost boundary.");

            context.CostStatus = AiCostStatus.Unknown;
            var unknownDecision = engine.Evaluate(context);
            if (!unknownDecision.IsDenied || unknownDecision.RuleId != "builtin.cost.free-only")
                throw new InvalidOperationException("Unknown cost was incorrectly treated as free by FreeOnly.");

            context.RequestedCostPolicy = AiCostPolicy.FreePreferred;
            var preferredCostDecision = engine.Evaluate(context);
            if (preferredCostDecision.IsDenied)
                throw new InvalidOperationException("FreePreferred incorrectly denied a non-free target.");

            var tiePolicy = new AiPolicySet { Version = "tie-42" };
            tiePolicy.Rules.Add(new AiPolicyRule
            {
                Id = "tie-b",
                Scope = AiPolicyScopeKind.System,
                Priority = 5,
                Outcome = AiPolicyOutcome.Allow,
                Operations = new List<string> { "tie.test" }
            });
            tiePolicy.Rules.Add(new AiPolicyRule
            {
                Id = "tie-a",
                Scope = AiPolicyScopeKind.System,
                Priority = 5,
                Outcome = AiPolicyOutcome.Deny,
                Operations = new List<string> { "tie.test" }
            });
            var tieContext = new AiPolicyEvaluationContext { Operation = "tie.test" };
            var tieDecision = new DefaultAiPolicyEngine(tiePolicy).Evaluate(tieContext);
            if (tieDecision.RuleId != "tie-a" || !tieDecision.IsDenied)
                throw new InvalidOperationException("Policy tie-breaking is not deterministic.");

            Write(
                "UNIFIED POLICY",
                "Policy engine contract test succeeded." + Environment.NewLine +
                "Scope and priority precedence: verified." + Environment.NewLine +
                "RequireApproval outcome: verified." + Environment.NewLine +
                "Tenant/resource/tool matching: verified." + Environment.NewLine +
                "Decision provenance and policy version: verified." + Environment.NewLine +
                "FreeOnly paid/unknown cost denial: verified." + Environment.NewLine +
                "FreePreferred behavior: verified." + Environment.NewLine +
                "Deterministic tie-breaking: verified." + Environment.NewLine +
                "Selected rule: " + decision.RuleId);

            return Task.CompletedTask;
        }
    }
}

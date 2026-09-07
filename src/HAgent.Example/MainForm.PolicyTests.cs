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
        private static int _policyTabsAdded;

        private void AddPolicyTabs()
        {
            if (Interlocked.Exchange(ref _policyTabsAdded, 1) != 0)
                return;

            AddApiTab(
                "Unified Policy",
                "Run policy contract test",
                "Verifies deterministic policy outcomes, runtime enforcement before provider transport, provenance, scoped matching, and cost restrictions.",
                "A policy decision must be reproducible from the same inputs and must block prohibited provider execution before transport is invoked.",
                "Uses only local deterministic adapters and in-memory state.",
                TestPolicyEngineAsync,
                "Policy boundary",
                "Policy is enforcement metadata, not prompt text. Host authentication and business authorization remain host-owned.");
        }

        private async Task TestPolicyEngineAsync(string unused)
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
                Reason = "The selected agent requires approval for the operation.",
                Operations = new List<string> { "tool.invoke" },
                ToolIds = new List<string> { "tool-sensitive" }
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
                throw new InvalidOperationException("Unrestricted system policy did not apply to the unmatched operation.");

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

            await TestRuntimePolicyEnforcementAsync().ConfigureAwait(true);

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
                "Runtime provider-execution enforcement: verified." + Environment.NewLine +
                "Provider transport calls under denial: 0." + Environment.NewLine +
                "Selected rule: " + decision.RuleId);
        }

        private async Task TestRuntimePolicyEnforcementAsync()
        {
            const string providerId = "policy-runtime-provider-42";
            const string agentId = "policy-runtime-agent-42";

            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(new AiProvider
            {
                Id = providerId,
                Name = "Policy Runtime Provider",
                Kind = "PolicyRuntimeTest",
                BaseUrl = "https://invalid.local/",
                DefaultModel = "policy-model-42",
                Enabled = true
            }).ConfigureAwait(true);

            await store.SaveAgentAsync(new AiAgent
            {
                Id = agentId,
                Name = "Policy Runtime Agent",
                Enabled = true
            }).ConfigureAwait(true);

            var policy = new AiPolicySet { Version = "runtime-policy-42" };
            policy.Rules.Add(new AiPolicyRule
            {
                Id = "deny-policy-runtime-provider",
                Name = "Runtime provider denial",
                Scope = AiPolicyScopeKind.Provider,
                ScopeId = providerId,
                Priority = 100,
                Outcome = AiPolicyOutcome.Deny,
                Reason = "This deterministic runtime test blocks provider execution."
            });

            var adapter = new PolicyRuntimeTestAdapter();
            var runtime = new DefaultAgentRuntime(
                store,
                new EmptySecretStore(),
                new IAiProviderAdapter[] { adapter },
                null,
                null,
                null,
                null,
                null,
                null,
                new DefaultAiPolicyEngine(policy));

            AgentExecution terminal = null;
            runtime.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
            {
                if (args != null && args.Execution != null && args.Execution.IsCompleted)
                    terminal = args.Execution;
            };

            try
            {
                await runtime.ExecuteAsync(
                    agentId,
                    "Policy enforcement request.",
                    new AgentExecutionOptions
                    {
                        Timeout = TimeSpan.FromSeconds(5),
                        MaxProviderAttempts = 1,
                        MaxRetriesPerProvider = 0
                    },
                    CancellationToken.None).ConfigureAwait(true);

                throw new InvalidOperationException("The denied runtime execution unexpectedly completed successfully.");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.IndexOf("Execution policy denied", StringComparison.OrdinalIgnoreCase) < 0)
                    throw;
            }

            if (terminal == null)
                throw new InvalidOperationException("Runtime policy denial did not produce a terminal execution.");
            if (terminal.PolicyDecision == null || !terminal.PolicyDecision.IsDenied)
                throw new InvalidOperationException("Runtime execution did not capture the denying policy decision.");
            if (terminal.PolicyDecision.RuleId != "deny-policy-runtime-provider")
                throw new InvalidOperationException("Runtime execution captured the wrong policy provenance.");
            if (adapter.SendCount != 0)
                throw new InvalidOperationException("Provider transport was invoked despite a denying policy decision.");
        }

        private sealed class PolicyRuntimeTestAdapter : IAiProviderAdapter
        {
            public int SendCount { get; private set; }
            public string Kind { get { return "PolicyRuntimeTest"; } }
            public string DisplayName { get { return "Policy Runtime Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                SendCount++;
                return Task.FromResult(new AIResponse { Text = "UNEXPECTED-POLICY-RESPONSE" });
            }
        }
    }
}

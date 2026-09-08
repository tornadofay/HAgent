using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;

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
                "Verifies deterministic policy outcomes, runtime enforcement before provider transport, tool side-effect enforcement, host authorization composition, provenance, scoped matching, cost restrictions, effective policy snapshot isolation, and persisted policy round-tripping.",
                "A policy decision must be reproducible from the same inputs, the exact effective policy must be captured by the execution snapshot, persisted policy must round-trip without shared mutable state, and prohibited provider/tool/data operations must be blocked before their side effects occur.",
                "Uses only local deterministic adapters and temporary File storage.",
                TestPolicyEngineAsync,
                "Policy boundary",
                "Policy is enforcement metadata, not prompt text. Host authentication and business authorization remain host-owned.");
        }

        private async Task TestPolicyEngineAsync(string unused)
        {
            var policy = new AiPolicySet { Version = "policy-contract-42" };

            var systemDefaultAllow = new AiPolicyRule
            {
                Id = "system-default-allow",
                Name = "System default",
                Scope = AiPolicyScopeKind.System,
                Priority = 1,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "System default allows this operation."
            };
            policy.Rules.Add(systemDefaultAllow);

            var tenantDenyTool = new AiPolicyRule
            {
                Id = "tenant-deny-tool",
                Name = "Tenant tool restriction",
                Scope = AiPolicyScopeKind.Tenant,
                ScopeId = "tenant-42",
                Priority = 5,
                Outcome = AiPolicyOutcome.Deny,
                Reason = "Tenant policy blocks the tool operation."
            };
            tenantDenyTool.Operations.Add("tool.invoke");
            policy.Rules.Add(tenantDenyTool);

            var agentApproval = new AiPolicyRule
            {
                Id = "agent-approval",
                Name = "Agent approval",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "agent-42",
                Priority = 20,
                Outcome = AiPolicyOutcome.RequireApproval,
                Reason = "The selected agent requires approval for the operation."
            };
            agentApproval.Operations.Add("tool.invoke");
            agentApproval.ToolIds.Add("tool-sensitive");
            policy.Rules.Add(agentApproval);

            var engine = new DefaultAiPolicyEngine(policy);
            var effectivePolicy = engine.GetPolicySnapshot();
            if (effectivePolicy == null || effectivePolicy.Version != policy.Version || effectivePolicy.Rules.Count != policy.Rules.Count)
                throw new InvalidOperationException("Policy engine did not expose a complete effective policy snapshot.");
            if (ReferenceEquals(effectivePolicy, policy) || ReferenceEquals(effectivePolicy.Rules[0], policy.Rules[0]))
                throw new InvalidOperationException("Policy snapshot reused caller-owned policy state.");

            effectivePolicy.Version = "mutated-copy";
            effectivePolicy.Rules[0].Name = "Mutated copy";
            var unaffectedSnapshot = engine.GetPolicySnapshot();
            if (unaffectedSnapshot.Version != "policy-contract-42" || unaffectedSnapshot.Rules[0].Name != "System default")
                throw new InvalidOperationException("Policy engine state changed after mutation of a returned policy snapshot.");

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

            var tieB = new AiPolicyRule
            {
                Id = "tie-b",
                Scope = AiPolicyScopeKind.System,
                Priority = 5,
                Outcome = AiPolicyOutcome.Allow
            };
            tieB.Operations.Add("tie.test");
            tiePolicy.Rules.Add(tieB);

            var tieA = new AiPolicyRule
            {
                Id = "tie-a",
                Scope = AiPolicyScopeKind.System,
                Priority = 5,
                Outcome = AiPolicyOutcome.Deny
            };
            tieA.Operations.Add("tie.test");
            tiePolicy.Rules.Add(tieA);

            var tieContext = new AiPolicyEvaluationContext { Operation = "tie.test" };
            var tieDecision = new DefaultAiPolicyEngine(tiePolicy).Evaluate(tieContext);
            if (tieDecision.RuleId != "tie-a" || !tieDecision.IsDenied)
                throw new InvalidOperationException("Policy tie-breaking is not deterministic.");

            await TestPolicyPersistenceAsync().ConfigureAwait(true);
            await TestRuntimePolicyEnforcementAsync().ConfigureAwait(true);
            await TestToolPolicyEnforcementAsync().ConfigureAwait(true);

            Write(
                "UNIFIED POLICY",
                "Policy engine contract test succeeded." + Environment.NewLine +
                "Scope and priority precedence: verified." + Environment.NewLine +
                "RequireApproval outcome: verified." + Environment.NewLine +
                "Tenant/resource/tool matching: verified." + Environment.NewLine +
                "Decision provenance and policy version: verified." + Environment.NewLine +
                "Effective policy snapshot cloning/isolation: verified." + Environment.NewLine +
                "Policy persistence round-trip: verified." + Environment.NewLine +
                "FreeOnly paid/unknown cost denial: verified." + Environment.NewLine +
                "FreePreferred behavior: verified." + Environment.NewLine +
                "Deterministic tie-breaking: verified." + Environment.NewLine +
                "Runtime provider-execution enforcement: verified." + Environment.NewLine +
                "Tool side-effect enforcement: verified." + Environment.NewLine +
                "Provider transport calls under denial: 0." + Environment.NewLine +
                "Selected rule: " + decision.RuleId);
        }

        private async Task TestPolicyPersistenceAsync()
        {
            var path = Path.Combine(Path.GetTempPath(), "HAgent-Policy-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                var policy = new AiPolicySet { Version = "persisted-policy-42" };
                var rule = new AiPolicyRule
                {
                    Id = "persisted-denial",
                    Name = "Persisted denial",
                    Scope = AiPolicyScopeKind.Provider,
                    ScopeId = "provider-persisted-42",
                    Priority = 42,
                    Outcome = AiPolicyOutcome.Deny,
                    Reason = "Persisted deterministic rule."
                };
                rule.Operations.Add("model.invoke");
                rule.ResourceTypes.Add("execution-target");
                rule.ResourceIds.Add("target-persisted-42");
                rule.ToolIds.Add("tool-persisted-42");
                rule.ProviderIds.Add("provider-persisted-42");
                rule.ExecutionTargetIds.Add("target-persisted-42");
                rule.Attributes["classification"] = "sensitive";
                policy.Rules.Add(rule);

                var store = new FileAiStore(path);
                await store.SavePolicySetAsync(policy, CancellationToken.None).ConfigureAwait(true);

                var reloaded = new FileAiStore(path);
                var loaded = await reloaded.GetPolicySetAsync(CancellationToken.None).ConfigureAwait(true);
                var loadedRule = loaded == null || loaded.Rules.Count == 0 ? null : loaded.Rules[0];
                if (loaded == null || loaded.Version != policy.Version || loaded.Rules.Count != 1 || loadedRule == null ||
                    loadedRule.Id != rule.Id || loadedRule.Name != rule.Name || loadedRule.Scope != rule.Scope ||
                    loadedRule.ScopeId != rule.ScopeId || loadedRule.Priority != rule.Priority ||
                    loadedRule.Outcome != rule.Outcome || loadedRule.Reason != rule.Reason ||
                    loadedRule.Operations.Count != 1 || loadedRule.Operations[0] != "model.invoke" ||
                    loadedRule.ResourceTypes.Count != 1 || loadedRule.ResourceTypes[0] != "execution-target" ||
                    loadedRule.ResourceIds.Count != 1 || loadedRule.ResourceIds[0] != "target-persisted-42" ||
                    loadedRule.ToolIds.Count != 1 || loadedRule.ToolIds[0] != "tool-persisted-42" ||
                    loadedRule.ProviderIds.Count != 1 || loadedRule.ProviderIds[0] != "provider-persisted-42" ||
                    loadedRule.ExecutionTargetIds.Count != 1 || loadedRule.ExecutionTargetIds[0] != "target-persisted-42" ||
                    loadedRule.Attributes.Count != 1 || loadedRule.Attributes["classification"] != "sensitive")
                    throw new InvalidOperationException("Persisted policy did not round-trip its canonical rule state.");

                loaded.Rules[0].Name = "Mutated loaded copy";
                loaded.Rules[0].Operations[0] = "mutated.operation";
                loaded.Rules[0].Attributes["classification"] = "mutated";
                var reread = await reloaded.GetPolicySetAsync(CancellationToken.None).ConfigureAwait(true);
                if (reread.Rules[0].Name != "Persisted denial" ||
                    reread.Rules[0].Operations[0] != "model.invoke" ||
                    reread.Rules[0].Attributes["classification"] != "sensitive")
                    throw new InvalidOperationException("Policy storage returned shared mutable state instead of an owned clone.");
            }
            finally
            {
                try { if (File.Exists(path)) File.Delete(path); } catch { }
                try { if (File.Exists(path + ".bak")) File.Delete(path + ".bak"); } catch { }
            }
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
            var providerDenial = new AiPolicyRule
            {
                Id = "deny-policy-runtime-provider",
                Name = "Runtime provider denial",
                Scope = AiPolicyScopeKind.Provider,
                ScopeId = providerId,
                Priority = 100,
                Outcome = AiPolicyOutcome.Deny,
                Reason = "This deterministic runtime test blocks provider execution."
            };
            policy.Rules.Add(providerDenial);

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
            if (terminal.Snapshot == null || terminal.Snapshot.EffectivePolicy == null)
                throw new InvalidOperationException("Runtime execution did not capture an effective policy snapshot.");
            if (terminal.Snapshot.EffectivePolicy.Version != "runtime-policy-42")
                throw new InvalidOperationException("Runtime execution captured the wrong effective policy version.");
            if (terminal.Snapshot.EffectivePolicy.Rules.Count != 1 ||
                terminal.Snapshot.EffectivePolicy.Rules[0].Id != "deny-policy-runtime-provider")
                throw new InvalidOperationException("Runtime execution did not capture the complete effective policy state.");
            if (terminal.PolicyDecision == null || !terminal.PolicyDecision.IsDenied)
                throw new InvalidOperationException("Runtime execution did not capture the denying policy decision.");
            if (terminal.PolicyDecision.RuleId != "deny-policy-runtime-provider")
                throw new InvalidOperationException("Runtime execution captured the wrong policy provenance.");
            if (adapter.SendCount != 0)
                throw new InvalidOperationException("Provider transport was invoked despite a denying policy decision.");
        }

        private async Task TestToolPolicyEnforcementAsync()
        {
            const string toolId = "policy-tool-42";
            var invocationCount = 0;
            var definition = new AiTool
            {
                Id = toolId,
                Name = "Policy Tool",
                Description = "Deterministic tool used to verify policy enforcement.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                Type = AiToolType.Application,
                Enabled = true
            };

            var denyPolicy = new AiPolicySet { Version = "tool-policy-deny-42" };
            var denyRule = new AiPolicyRule
            {
                Id = "deny-tool-policy-42",
                Name = "Tool denial",
                Scope = AiPolicyScopeKind.Tool,
                ScopeId = toolId,
                Priority = 100,
                Outcome = AiPolicyOutcome.Deny,
                Reason = "Tool side effect is blocked by the unified policy."
            };
            denyRule.Operations.Add("tool.invoke");
            denyRule.ToolIds.Add(toolId);
            denyPolicy.Rules.Add(denyRule);

            var deniedClient = CreatePolicyTestClient(new DefaultAiPolicyEngine(denyPolicy));
            deniedClient.RegisterTool(new DelegateAgentTool(definition, context =>
            {
                invocationCount++;
                return Task.FromResult(ToolExecutionResult.Success(Convert.ToString(context.Arguments["value"])));
            }));

            var denied = await deniedClient.ExecuteToolAsync(
                "policy-agent-42",
                toolId,
                "policy-call-deny-42",
                new Dictionary<string, object> { { "value", "blocked" } },
                CancellationToken.None,
                "policy-host-correlation-42",
                new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42")).ConfigureAwait(true);

            if (denied.Succeeded || invocationCount != 0)
                throw new InvalidOperationException("A denied tool policy reached the executable tool handler.");
            if (denied.PolicyDecision == null || !denied.PolicyDecision.IsDenied || denied.PolicyDecision.RuleId != denyRule.Id)
                throw new InvalidOperationException("Tool denial did not capture the expected policy decision/provenance.");

            var approvalPolicy = new AiPolicySet { Version = "tool-policy-approval-42" };
            var approvalRule = new AiPolicyRule
            {
                Id = "approval-tool-policy-42",
                Name = "Tool approval",
                Scope = AiPolicyScopeKind.Tool,
                ScopeId = toolId,
                Priority = 100,
                Outcome = AiPolicyOutcome.RequireApproval,
                Reason = "Tool side effect requires an approval workflow."
            };
            approvalRule.Operations.Add("tool.invoke");
            approvalRule.ToolIds.Add(toolId);
            approvalPolicy.Rules.Add(approvalRule);

            var approvalClient = CreatePolicyTestClient(new DefaultAiPolicyEngine(approvalPolicy));
            approvalClient.RegisterTool(new DelegateAgentTool(definition, context =>
            {
                invocationCount++;
                return Task.FromResult(ToolExecutionResult.Success("unexpected"));
            }));

            var approval = await approvalClient.ExecuteToolAsync(
                "policy-agent-42",
                toolId,
                "policy-call-approval-42",
                new Dictionary<string, object> { { "value", "approval" } }).ConfigureAwait(true);
            if (approval.Succeeded || invocationCount != 0 || approval.PolicyDecision == null || !approval.PolicyDecision.RequiresApproval)
                throw new InvalidOperationException("RequireApproval did not stop the tool handler before side effects.");

            var allowPolicy = new AiPolicySet { Version = "tool-policy-allow-42" };
            var allowRule = new AiPolicyRule
            {
                Id = "allow-tool-policy-42",
                Name = "Tool allow",
                Scope = AiPolicyScopeKind.Tool,
                ScopeId = toolId,
                Priority = 100,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "Tool operation is permitted."
            };
            allowRule.Operations.Add("tool.invoke");
            allowRule.ToolIds.Add(toolId);
            allowPolicy.Rules.Add(allowRule);

            var allowedClient = CreatePolicyTestClient(new DefaultAiPolicyEngine(allowPolicy));
            allowedClient.RegisterTool(new DelegateAgentTool(definition, context =>
            {
                invocationCount++;
                return Task.FromResult(ToolExecutionResult.Success(Convert.ToString(context.Arguments["value"])));
            }));

            var allowed = await allowedClient.ExecuteToolAsync(
                "policy-agent-42",
                toolId,
                "policy-call-allow-42",
                new Dictionary<string, object> { { "value", "allowed" } }).ConfigureAwait(true);
            if (!allowed.Succeeded || allowed.Output != "allowed" || invocationCount != 1)
                throw new InvalidOperationException("An allowed tool policy did not permit the executable handler.");
            if (allowed.PolicyDecision == null || !allowed.PolicyDecision.IsAllowed || allowed.PolicyDecision.RuleId != allowRule.Id)
                throw new InvalidOperationException("Allowed tool execution did not capture the expected policy decision/provenance.");
        }

        private static HAgentClient CreatePolicyTestClient(IAiPolicyEngine policyEngine)
        {
            return new HAgentClient(
                new InMemoryAiStore(),
                new EmptySecretStore(),
                new IAiProviderAdapter[0],
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                policyEngine);
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

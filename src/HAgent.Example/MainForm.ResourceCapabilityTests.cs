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
        private void AddResourceCapabilityTab()
        {
            AddApiTab(
                "Resource Capabilities",
                "Run resource capability test",
                "Verifies profile-level resource defaults, exact-resource precedence, runtime Inherit/Enabled/Disabled overrides, execution snapshot isolation, resource-policy persistence, and tool gating before the executable handler.",
                "Profile and runtime resource state should resolve deterministically, Inherit should fall through to the profile layer, unspecified resources should remain enabled, persisted profile state should round-trip, and disabled tools must never reach their handlers.",
                "No AI request is sent by this example.",
                TestResourceCapabilityPolicyAsync,
                "Capability boundary",
                "Resource enablement is separate from provider capability discovery and authorization. It controls whether HAgent exposes/uses a configured resource; it never grants host authorization.");
        }

        private async Task TestResourceCapabilityPolicyAsync(string unused)
        {
            const string sharedKnowledgeId = "knowledge-shared-42";
            const string privateKnowledgeId = "knowledge-private-42";
            const string gatedToolId = "resource-gated-tool-42";

            var profile = new AiAgent
            {
                Id = "resource-capability-agent-42",
                Name = "Resource Capability Agent"
            };
            profile.ResourceCapabilities.Set("memory", AiResourceCapabilityState.Disabled);
            profile.ResourceCapabilities.Set("knowledge", sharedKnowledgeId, AiResourceCapabilityState.Enabled);
            profile.ResourceCapabilities.Set("tool", gatedToolId, AiResourceCapabilityState.Disabled);

            var runtimeOverrides = new AgentRuntimeOverrides();
            runtimeOverrides.ResourceCapabilityOverrides.Set("memory", AiResourceCapabilityState.Inherit);
            runtimeOverrides.ResourceCapabilityOverrides.Set("knowledge", sharedKnowledgeId, AiResourceCapabilityState.Disabled);
            runtimeOverrides.ResourceCapabilityOverrides.Set("knowledge", privateKnowledgeId, AiResourceCapabilityState.Enabled);
            runtimeOverrides.ResourceCapabilityOverrides.Set("tool", gatedToolId, AiResourceCapabilityState.Inherit);

            var resolved = AiResourceCapabilitySnapshot.Resolve(profile.ResourceCapabilities, runtimeOverrides.ResourceCapabilityOverrides);
            if (resolved.GetState("memory") != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Profile resource-family disable did not resolve correctly.");
            if (resolved.GetState("memory", "memory-private-42") != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Profile resource-family state did not apply to a concrete resource.");
            if (resolved.GetState("knowledge", sharedKnowledgeId) != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Exact runtime Disabled did not override the profile resource state.");
            if (resolved.GetState("knowledge", privateKnowledgeId) != AiResourceCapabilityState.Enabled)
                throw new InvalidOperationException("Exact runtime Enabled did not create an enabled resource state.");
            if (resolved.GetState("tool", gatedToolId) != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Runtime Inherit did not fall back to the profile resource state.");
            if (resolved.GetState("tool", "tool-unspecified-42") != AiResourceCapabilityState.Enabled)
                throw new InvalidOperationException("Unspecified resource state did not default to Enabled.");

            var executionSnapshot = new AgentExecutionSnapshot(
                profile,
                new List<AiProvider>(),
                runtimeOverrides,
                null,
                null,
                new AiPolicySet { Version = "resource-policy-42" });

            if (executionSnapshot.EffectiveResourceCapabilities == null ||
                executionSnapshot.EffectiveResourceCapabilities.GetState("knowledge", sharedKnowledgeId) != AiResourceCapabilityState.Disabled ||
                executionSnapshot.EffectiveResourceCapabilities.GetState("knowledge", privateKnowledgeId) != AiResourceCapabilityState.Enabled)
                throw new InvalidOperationException("Execution snapshot did not capture effective resource capability state.");

            profile.ResourceCapabilities.Set("memory", AiResourceCapabilityState.Enabled);
            runtimeOverrides.ResourceCapabilityOverrides.Set("knowledge", sharedKnowledgeId, AiResourceCapabilityState.Enabled);
            if (executionSnapshot.EffectiveResourceCapabilities.GetState("memory") != AiResourceCapabilityState.Disabled ||
                executionSnapshot.EffectiveResourceCapabilities.GetState("knowledge", sharedKnowledgeId) != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Execution resource capability snapshot changed after source configuration mutation.");

            var path = Path.Combine(Path.GetTempPath(), "HAgent-ResourceCapabilities-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                var fileStore = new FileAiStore(path);
                await fileStore.SaveAgentAsync(profile, CancellationToken.None).ConfigureAwait(true);
                var reopened = new FileAiStore(path);
                var persistedAgents = await reopened.GetAgentsAsync(CancellationToken.None).ConfigureAwait(true);
                var persisted = persistedAgents.Count == 0 ? null : persistedAgents[0];
                if (persisted == null || persisted.ResourceCapabilities == null ||
                    persisted.ResourceCapabilities.GetState("memory") != AiResourceCapabilityState.Enabled ||
                    persisted.ResourceCapabilities.Entries.Count != profile.ResourceCapabilities.Entries.Count)
                    throw new InvalidOperationException("Agent resource capability configuration did not persist its canonical state.");
            }
            finally
            {
                try { if (File.Exists(path)) File.Delete(path); } catch { }
                try { if (File.Exists(path + ".bak")) File.Delete(path + ".bak"); } catch { }
            }

            var store = new InMemoryAiStore();
            await store.SaveAgentAsync(profile, CancellationToken.None).ConfigureAwait(true);
            var client = new HAgentClient(store, new EmptySecretStore(), new IAiProviderAdapter[0]);
            var invocationCount = 0;
            var tool = new DelegateAgentTool(new AiTool
            {
                Id = gatedToolId,
                Name = "resource_gated_tool",
                Description = "Tool used to verify resource capability gating.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                Type = AiToolType.Application,
                Enabled = true
            }, context =>
            {
                invocationCount++;
                return Task.FromResult(ToolExecutionResult.Success("executed"));
            });
            client.RegisterTool(tool);

            var denied = await client.ExecuteToolAsync(
                profile.Id,
                gatedToolId,
                "resource-tool-call-42",
                new Dictionary<string, object> { { "value", "blocked" } },
                CancellationToken.None,
                "resource-host-correlation-42",
                new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42"));
            if (denied.Succeeded || invocationCount != 0 || denied.ResourceCapabilityState != AiResourceCapabilityState.Disabled)
                throw new InvalidOperationException("Disabled resource capability did not block the tool before handler execution.");

            var instanceOverrides = new AgentRuntimeOverrides();
            instanceOverrides.ResourceCapabilityOverrides.Set("tool", gatedToolId, AiResourceCapabilityState.Enabled);
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.User, instanceOverrides);
            var allowed = await client.ExecuteToolAsync(
                instance,
                gatedToolId,
                "resource-tool-call-43",
                new Dictionary<string, object> { { "value", "allowed" } },
                CancellationToken.None,
                "resource-host-correlation-43",
                new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42"));
            if (!allowed.Succeeded || invocationCount != 1 || allowed.ResourceCapabilityState != AiResourceCapabilityState.Enabled)
                throw new InvalidOperationException("Runtime Enabled override did not re-enable the tool capability.");

            instanceOverrides.ResourceCapabilityOverrides.Set("tool", gatedToolId, AiResourceCapabilityState.Disabled);
            var stillEnabled = await client.ExecuteToolAsync(
                instance,
                gatedToolId,
                "resource-tool-call-44",
                new Dictionary<string, object> { { "value", "snapshot" } },
                CancellationToken.None,
                "resource-host-correlation-44",
                new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42"));
            if (stillEnabled.Succeeded || invocationCount != 1)
                throw new InvalidOperationException("Runtime resource capability changes were not applied deterministically.");

            Write(
                "RESOURCE CAPABILITIES",
                "Contract test succeeded." + Environment.NewLine +
                "Profile capability inheritance: verified." + Environment.NewLine +
                "Runtime tri-state overrides: verified." + Environment.NewLine +
                "Exact resource precedence: verified." + Environment.NewLine +
                "Unspecified resources default to Enabled: verified." + Environment.NewLine +
                "Effective execution snapshot isolation: verified." + Environment.NewLine +
                "Resource capability persistence round-trip: verified." + Environment.NewLine +
                "Tool capability gating before handler: verified." + Environment.NewLine +
                "Runtime Enabled override: verified." + Environment.NewLine +
                "Runtime Disabled override: verified." + Environment.NewLine +
                "Handler invocation count: " + invocationCount);
        }
    }
}

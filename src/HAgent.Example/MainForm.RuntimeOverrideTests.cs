using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private void AddRuntimeOverrideTab()
        {
            AddApiTab(
                "RUNTIME OVERRIDES",
                "Run runtime override test",
                "Executes one runtime instance with provider/model/generation/context overrides and verifies that the reusable profile remains unchanged.",
                "The runtime snapshot should use the override values while the stored profile keeps its original values. The same test also verifies independent runtime memory ownership.",
                "Runtime override and memory ownership verification.",
                TestRuntimeOverridesAsync,
                "Profile isolation",
                "Uses a local adapter only. No external provider is contacted.");
        }

        private async Task TestRuntimeOverridesAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var memoryStore = await CreateConfiguredMemoryStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var providers = await store.GetProvidersAsync().ConfigureAwait(true);
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var provider = providers.FirstOrDefault(x => string.Equals(x.Id, profile.ProviderId, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
                throw new InvalidOperationException("The selected agent's primary provider could not be found.");

            var originalModel = profile.Model ?? string.Empty;
            var originalTemperature = profile.Temperature;
            var originalMaxOutputTokens = profile.MaxOutputTokens;
            var overrideModel = "runtime-model-42";
            var overrideTemperature = 0.17d;
            var overrideMaxOutputTokens = 77;
            var contextKey = "runtime-context-42";
            var contextValue = "context-value-42";

            var instance = AgentRuntimeInstance.Create(
                profile,
                AgentRuntimeScope.Task,
                new AgentRuntimeOverrides
                {
                    ProviderId = provider.Id,
                    Model = overrideModel,
                    Temperature = overrideTemperature,
                    MaxOutputTokens = overrideMaxOutputTokens,
                    SystemPrompt = "Runtime-only system prompt 42."
                });
            instance.Overrides.Context[contextKey] = contextValue;

            var client = new HAgentClient(store, secrets, new[] { new RuntimeOverrideTestAdapter() }, null, memoryStore);
            var execution = await client.ExecuteAsync(
                instance,
                string.IsNullOrWhiteSpace(message) ? "Runtime override test." : message,
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                },
                CancellationToken.None).ConfigureAwait(true);

            var snapshotAgent = execution.Snapshot.Agent;
            if (!string.Equals(snapshotAgent.Model, overrideModel, StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime model override was not applied to the execution snapshot.");
            if (snapshotAgent.Temperature != overrideTemperature)
                throw new InvalidOperationException("Runtime temperature override was not applied to the execution snapshot.");
            if (snapshotAgent.MaxOutputTokens != overrideMaxOutputTokens)
                throw new InvalidOperationException("Runtime max-output-token override was not applied to the execution snapshot.");
            if (!string.Equals(snapshotAgent.ProviderId, provider.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime provider override was not applied to the execution snapshot.");
            string capturedContext;
            if (!execution.Snapshot.RuntimeContext.TryGetValue(contextKey, out capturedContext) || !string.Equals(capturedContext, contextValue, StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime context was not captured in the execution snapshot.");

            if (!string.Equals(profile.Model ?? string.Empty, originalModel, StringComparison.Ordinal) ||
                profile.Temperature != originalTemperature ||
                profile.MaxOutputTokens != originalMaxOutputTokens ||
                !string.Equals(profile.ProviderId, provider.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime overrides mutated the reusable agent profile.");

            if (!string.Equals(instance.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instance lost its profile identity.");
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime instance was not active during execution.");

            var secondInstance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Session);
            var firstMemoryContent = "runtime-memory-instance-1-42";
            var secondMemoryContent = "runtime-memory-instance-2-42";
            var firstMemoryId = await client.RememberAsync(
                instance,
                firstMemoryContent,
                MemoryScope.Agent,
                null,
                CancellationToken.None).ConfigureAwait(true);
            var secondMemoryId = await client.RememberAsync(
                secondInstance,
                secondMemoryContent,
                MemoryScope.Agent,
                null,
                CancellationToken.None).ConfigureAwait(true);

            try
            {
                var firstMemories = await client.RecallAsync(instance, string.Empty, MemoryScope.Agent, 10, null, CancellationToken.None).ConfigureAwait(true);
                var secondMemories = await client.RecallAsync(secondInstance, string.Empty, MemoryScope.Agent, 10, null, CancellationToken.None).ConfigureAwait(true);
                if (!firstMemories.Any(x => string.Equals(x.Id, firstMemoryId, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("The first runtime instance could not recall its own memory.");
                if (firstMemories.Any(x => string.Equals(x.Id, secondMemoryId, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("The first runtime instance recalled another instance's memory.");
                if (!secondMemories.Any(x => string.Equals(x.Id, secondMemoryId, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("The second runtime instance could not recall its own memory.");
                if (secondMemories.Any(x => string.Equals(x.Id, firstMemoryId, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("The second runtime instance recalled another instance's memory.");
            }
            finally
            {
                await client.ForgetAsync(firstMemoryId, CancellationToken.None).ConfigureAwait(true);
                await client.ForgetAsync(secondMemoryId, CancellationToken.None).ConfigureAwait(true);
            }

            Write("RUNTIME OVERRIDES",
                "Contract test succeeded." + Environment.NewLine +
                "Profile: " + profile.Name + " (" + profile.Id + ")" + Environment.NewLine +
                "Runtime instance: " + instance.InstanceId + Environment.NewLine +
                "Scope: " + instance.Scope + Environment.NewLine +
                "Provider override: " + snapshotAgent.ProviderId + Environment.NewLine +
                "Model override: " + snapshotAgent.Model + Environment.NewLine +
                "Temperature override: " + snapshotAgent.Temperature + Environment.NewLine +
                "Max output tokens override: " + snapshotAgent.MaxOutputTokens + Environment.NewLine +
                "Runtime context: " + contextKey + "=" + capturedContext + Environment.NewLine +
                "Profile remained unchanged: yes." + Environment.NewLine +
                "Independent memory ownership: verified." + Environment.NewLine +
                "Memory owner 1: " + instance.MemoryOwnerId + Environment.NewLine +
                "Memory owner 2: " + secondInstance.MemoryOwnerId + Environment.NewLine +
                "Cross-instance memory access: rejected." + Environment.NewLine +
                "Execution state: " + execution.State);
        }

        private sealed class RuntimeOverrideTestAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "RuntimeOverrideTest"; } }
            public string DisplayName { get { return "Runtime Override Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null;
            }

            public Task<AIResponse> SendAsync(
                ProviderExecutionRequest request,
                CancellationToken cancellationToken)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                return Task.FromResult(new AIResponse
                {
                    Text = "RUNTIME-OVERRIDE-OK",
                    ProviderId = request.Provider == null ? string.Empty : request.Provider.Id
                });
            }
        }
    }
}

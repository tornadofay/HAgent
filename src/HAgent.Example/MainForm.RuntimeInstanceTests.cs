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
        private void AddRuntimeInstanceTab()
        {
            AddApiTab(
                "Runtime Instances",
                "Run runtime instance test",
                "Verifies that one reusable agent profile can create multiple independent runtime identities without changing the stored profile.",
                "Two instances from the same profile should have distinct identities, the same profile reference, the requested scopes, and independent retirement state.",
                "Runtime instance verification.",
                TestRuntimeInstanceAsync,
                "Profile → instances",
                "Provider-independent deterministic model test; no network or storage mutation.");

            AddApiTab(
                "RUNTIME CONCURRENCY",
                "Run runtime concurrency test",
                "Runs two independent runtime instances concurrently against a local adapter and verifies that execution identities and results remain isolated.",
                "Both executions should overlap, complete successfully, and retain distinct instance, execution, and correlation identities.",
                "Runtime concurrency verification.",
                TestRuntimeConcurrencyAsync,
                "Two independent instances",
                "Uses only a local adapter; no external provider is contacted.");

            AddApiTab(
                "RUNTIME STALE RESULTS",
                "Run stale-result test",
                "Runs two executions on one runtime instance and verifies that an older late result is no longer authoritative after a newer execution starts or the instance is retired.",
                "The first execution should be stale after the second begins; the second should be current until the instance is retired.",
                "Runtime stale-result verification.",
                TestRuntimeStaleResultsAsync,
                "Revision-based authority",
                "Uses only a local adapter; no external provider is contacted.");

            AddApiTab(
                "RUNTIME STATE PERSISTENCE",
                "Run runtime state persistence test",
                "Persists and restores runtime identity and lifecycle metadata without persisting prompts, context, secrets, or execution history.",
                "The runtime instance should round-trip its identity and host metadata, restore retirement state, and be removable from the selected HAgent backend.",
                "Runtime state persistence verification.",
                TestRuntimeStatePersistenceAsync,
                "Optional runtime persistence",
                "Uses the currently selected HAgent storage backend.");

            AddApiTab(
                "GENERIC HOST EXECUTION",
                "Run generic host execution test",
                "Verifies the canonical provider-neutral host execution request with multiple messages, host correlation, and bounded host context.",
                "The execution should preserve the host correlation and immutable host context without mutating the reusable agent profile.",
                "0.95 generic host boundary verification.",
                TestGenericHostExecutionAsync,
                "Canonical host request",
                "Uses only a local adapter; no external provider is contacted.");

            AddRuntimeSchedulingTab();
            AddRuntimeTerminalStateTab();
        }

        private Task TestRuntimeInstanceAsync(string message)
        {
            var profile = new AiAgent
            {
                Id = "runtime-profile-42",
                Name = "Runtime Instance Test Profile"
            };

            var first = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Session);
            var second = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);

            if (string.Equals(first.InstanceId, second.InstanceId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instances created from one profile must have distinct instance IDs.");
            if (!string.Equals(first.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(second.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instances did not preserve the reusable profile ID.");
            if (first.Scope != AgentRuntimeScope.Session)
                throw new InvalidOperationException("The first runtime instance did not preserve its requested scope.");
            if (second.Scope != AgentRuntimeScope.Task)
                throw new InvalidOperationException("The second runtime instance did not preserve its requested scope.");
            if (first.State != AgentRuntimeInstanceState.Active || second.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("New runtime instances must start active.");
            if (!string.Equals(profile.Id, "runtime-profile-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime instance creation mutated the reusable profile identity.");
            if (!string.Equals(first.MemoryOwnerId, first.InstanceId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(second.MemoryOwnerId, second.InstanceId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(first.MemoryOwnerId, second.MemoryOwnerId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instances must have independent memory owners.");

            first.Retire();
            if (first.State != AgentRuntimeInstanceState.Retired)
                throw new InvalidOperationException("Retiring the first runtime instance did not change its state.");
            if (second.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Retiring one runtime instance must not retire another instance from the same profile.");

            Write("RUNTIME INSTANCES",
                "Contract test succeeded." + Environment.NewLine +
                "Profile: " + profile.Id + Environment.NewLine +
                "Instance 1: " + first.InstanceId + Environment.NewLine +
                "Instance 1 scope: " + first.Scope + Environment.NewLine +
                "Instance 1 memory owner: " + first.MemoryOwnerId + Environment.NewLine +
                "Instance 1 state after retire: " + first.State + Environment.NewLine +
                "Instance 2: " + second.InstanceId + Environment.NewLine +
                "Instance 2 scope: " + second.Scope + Environment.NewLine +
                "Instance 2 memory owner: " + second.MemoryOwnerId + Environment.NewLine +
                "Instance 2 state: " + second.State + Environment.NewLine +
                "Independent memory owners: yes" + Environment.NewLine +
                "Profile remained reusable: yes.");

            return Task.CompletedTask;
        }

        private async Task TestRuntimeStaleResultsAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var adapter = new RuntimeStaleResultTestAdapter();
            var client = new HAgentClient(store, secrets, new[] { adapter });
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            var options = new AgentExecutionOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                MaxProviderAttempts = 1,
                MaxRetriesPerProvider = 0
            };

            var firstTask = client.ExecuteAsync(instance, "stale-first", options, CancellationToken.None);
            await Task.Delay(25).ConfigureAwait(true);
            var secondTask = client.ExecuteAsync(instance, "stale-second", options, CancellationToken.None);
            var first = await firstTask.ConfigureAwait(true);
            var second = await secondTask.ConfigureAwait(true);

            if (first.RuntimeInstanceRevision <= 0 || second.RuntimeInstanceRevision <= 0)
                throw new InvalidOperationException("Runtime executions did not capture an instance revision.");
            if (first.RuntimeInstanceRevision >= second.RuntimeInstanceRevision)
                throw new InvalidOperationException("Runtime execution revisions did not advance monotonically.");
            if (instance.IsExecutionCurrent(first))
                throw new InvalidOperationException("The older execution was incorrectly treated as current after a newer execution started.");
            if (!instance.IsExecutionCurrent(second))
                throw new InvalidOperationException("The newest execution was not treated as current before retirement.");
            if (first.State != AgentExecutionState.Succeeded || second.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("The stale-result verification executions did not both succeed.");

            instance.Retire();
            if (instance.IsExecutionCurrent(second))
                throw new InvalidOperationException("A completed execution remained current after the runtime instance was retired.");

            Write("RUNTIME STALE RESULTS",
                "Contract test succeeded." + Environment.NewLine +
                "Runtime instance: " + instance.InstanceId + Environment.NewLine +
                "First revision: " + first.RuntimeInstanceRevision + Environment.NewLine +
                "Second revision: " + second.RuntimeInstanceRevision + Environment.NewLine +
                "First result current: no" + Environment.NewLine +
                "Second result current before retire: yes" + Environment.NewLine +
                "Second result current after retire: no" + Environment.NewLine +
                "Both executions succeeded: yes");
        }

        private sealed class RuntimeStaleResultTestAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "RuntimeStaleResultTest"; } }
            public string DisplayName { get { return "Runtime Stale Result Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null;
            }

            public async Task<AIResponse> SendAsync(
                ProviderExecutionRequest request,
                CancellationToken cancellationToken)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                var delay = request.Messages != null && request.Messages.Count > 0 &&
                             string.Equals(request.Messages[request.Messages.Count - 1].Content, "stale-first", StringComparison.Ordinal)
                    ? 200
                    : 40;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                return new AIResponse
                {
                    Text = "RUNTIME-STALE-OK",
                    ProviderId = request.Provider == null ? string.Empty : request.Provider.Id
                };
            }
        }
    }
}

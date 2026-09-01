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
        private void AddRuntimeLifecycleTab()
        {
            AddApiTab(
                "RUNTIME SHUTDOWN",
                "Run runtime shutdown test",
                "Verifies that shutting down a runtime instance cancels outstanding instance-bound work and prevents new executions.",
                "The running execution should be cancelled by instance shutdown, the instance should enter Shutdown state, and a subsequent execution should be rejected.",
                "Runtime shutdown verification.",
                TestRuntimeShutdownAsync,
                "Shutdown lifecycle",
                "Uses only a local adapter; no external provider is contacted.");
        }

        private async Task TestRuntimeShutdownAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var adapter = new RuntimeShutdownTestAdapter();
            var client = new HAgentClient(store, secrets, new[] { adapter });
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            var options = new AgentExecutionOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                MaxProviderAttempts = 1,
                MaxRetriesPerProvider = 0
            };

            var executionTask = client.ExecuteAsync(
                instance,
                string.IsNullOrWhiteSpace(message) ? "runtime-shutdown" : message,
                options,
                CancellationToken.None);

            await adapter.Started.Task.ConfigureAwait(true);
            instance.Shutdown();

            var cancelled = false;
            try
            {
                await executionTask.ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            if (!cancelled)
                throw new InvalidOperationException("Runtime shutdown did not cancel the outstanding execution.");
            if (instance.State != AgentRuntimeInstanceState.Shutdown)
                throw new InvalidOperationException("Runtime instance did not enter Shutdown state.");

            var rejected = false;
            try
            {
                await client.ExecuteAsync(instance, "after-shutdown", options, CancellationToken.None).ConfigureAwait(true);
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }

            if (!rejected)
                throw new InvalidOperationException("Shutdown did not prevent a new execution.");

            Write("RUNTIME SHUTDOWN",
                "Contract test succeeded." + Environment.NewLine +
                "Runtime instance: " + instance.InstanceId + Environment.NewLine +
                "Shutdown cancelled outstanding execution: yes" + Environment.NewLine +
                "Instance state: " + instance.State + Environment.NewLine +
                "New execution after shutdown: rejected");
        }

        private sealed class RuntimeShutdownTestAdapter : IAiProviderAdapter
        {
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();

            public string Kind { get { return "RuntimeShutdownTest"; } }
            public string DisplayName { get { return "Runtime Shutdown Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null;
            }

            public async Task<AIResponse> SendAsync(
                AiProvider provider,
                AiAgent agent,
                string apiKey,
                string systemPrompt,
                IReadOnlyList<AIMessage> messages,
                CancellationToken cancellationToken)
            {
                Started.TrySetResult(true);
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                return new AIResponse
                {
                    Text = "RUNTIME-SHUTDOWN-UNEXPECTED",
                    ProviderId = provider == null ? string.Empty : provider.Id
                };
            }
        }
    }
}

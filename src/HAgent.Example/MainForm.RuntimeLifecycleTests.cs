using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

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
            var store = new InMemoryAiStore();
            var provider = new AiProvider
            {
                Id = "runtime-shutdown-provider-42",
                Name = "Runtime Shutdown Provider",
                Kind = "RuntimeShutdownTest",
                BaseUrl = "https://runtime-shutdown.test/v1",
                DefaultModel = "runtime-shutdown-model-42",
                Enabled = true
            };
            var profile = new AiAgent
            {
                Id = "runtime-shutdown-profile-42",
                Name = "Runtime Shutdown Test Profile",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            };

            await store.SaveProviderAsync(provider).ConfigureAwait(true);
            await store.SaveAgentAsync(profile).ConfigureAwait(true);

            var adapter = new RuntimeShutdownTestAdapter();
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
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

        //private sealed class NullSecretStore : ISecretStore
        //{
        //    public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.FromResult(string.Empty); }
        //    public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        //    public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        //}

        private sealed class RuntimeShutdownTestAdapter : IAiProviderAdapter
        {
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();

            public string Kind { get { return "RuntimeShutdownTest"; } }
            public string DisplayName { get { return "Runtime Shutdown Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public async Task<AIResponse> SendAsync(
                ProviderExecutionRequest request,
                CancellationToken cancellationToken)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                Started.TrySetResult(true);
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                return new AIResponse
                {
                    Text = "RUNTIME-SHUTDOWN-UNEXPECTED",
                    ProviderId = request.Provider == null ? string.Empty : request.Provider.Id
                };
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddRuntimeConcurrencyTab()
        {
            AddApiTab(
                "RUNTIME CONCURRENCY",
                "Run runtime concurrency test",
                "Runs two independent runtime instances concurrently against a deterministic local adapter and verifies their execution identities, results, and selected model target remain isolated.",
                "Both executions should overlap, complete successfully, and send the discovered execution target to the provider adapter.",
                "Runtime concurrency verification.",
                TestRuntimeConcurrencyAsync,
                "Two independent instances",
                "Uses only an in-memory store and local adapter. No external provider is contacted.");
        }

        private async Task TestRuntimeConcurrencyAsync(string message)
        {
            var store = new InMemoryAiStore();
            var provider = new AiProvider
            {
                Id = "runtime-concurrency-provider-42",
                Name = "Runtime Concurrency Provider",
                Kind = "RuntimeConcurrencyTest",
                BaseUrl = "https://runtime-concurrency.test/v1",
                DefaultModel = "runtime-default-model-42",
                Enabled = true
            };
            var profile = new AiAgent
            {
                Id = "runtime-concurrency-profile-42",
                Name = "Runtime Concurrency Test Profile",
                ProviderId = provider.Id,
                Model = "legacy-agent-model-that-must-not-win",
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

            var adapter = new RuntimeConcurrencyTestAdapter();
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
            var registry = new AgentRuntimeInstanceRegistry();
            var first = registry.Create(profile, AgentRuntimeScope.Session);
            var second = registry.Create(profile, AgentRuntimeScope.Task);

            var executionOptions = new AgentExecutionOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                MaxProviderAttempts = 1,
                MaxRetriesPerProvider = 0,
                HostCorrelationId = string.Empty
            };

            var firstTask = client.ExecuteAsync(first, (message ?? string.Empty) + " [instance-1]", executionOptions, CancellationToken.None);
            var secondTask = client.ExecuteAsync(second, (message ?? string.Empty) + " [instance-2]", executionOptions, CancellationToken.None);
            var executions = await Task.WhenAll(firstTask, secondTask).ConfigureAwait(true);

            if (executions.Length != 2) throw new InvalidOperationException("Expected two concurrent executions.");
            if (!adapter.OverlapObserved) throw new InvalidOperationException("The local adapter did not observe concurrent provider calls.");
            if (string.Equals(executions[0].Id, executions[1].Id, StringComparison.Ordinal)) throw new InvalidOperationException("Concurrent executions shared an execution ID.");
            if (string.Equals(executions[0].CorrelationId, executions[1].CorrelationId, StringComparison.Ordinal)) throw new InvalidOperationException("Concurrent executions shared a correlation ID.");
            if (executions.Any(x => x.State != AgentExecutionState.Succeeded)) throw new InvalidOperationException("At least one concurrent execution did not succeed.");
            if (!adapter.AllObservedTargetsCorrect) throw new InvalidOperationException("The provider adapter did not receive the runtime-selected discovered execution target.");
            if (adapter.ObservedModelId != "runtime-discovered-model-42") throw new InvalidOperationException("The provider adapter received an unexpected execution model: " + adapter.ObservedModelId);

            var active = registry.GetActiveInstances();
            if (active.Count != 2) throw new InvalidOperationException("The registry lost an active runtime instance during concurrent execution.");
            if (active.Any(x => !string.Equals(x.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("A runtime instance lost its source profile identity.");
            if (string.Equals(first.InstanceId, second.InstanceId, StringComparison.Ordinal)) throw new InvalidOperationException("Concurrent runtime instances shared an instance ID.");

            registry.Retire(first.InstanceId);
            if (first.State != AgentRuntimeInstanceState.Retired || second.State != AgentRuntimeInstanceState.Active) throw new InvalidOperationException("Retiring one concurrent instance affected another instance.");
            registry.RemoveRetired(first.InstanceId);

            Write("RUNTIME CONCURRENCY",
                "Contract test succeeded." + Environment.NewLine +
                "Profile: " + profile.Name + " (" + profile.Id + ")" + Environment.NewLine +
                "Instance 1: " + first.InstanceId + " / Session" + Environment.NewLine +
                "Instance 2: " + second.InstanceId + " / Task" + Environment.NewLine +
                "Concurrent overlap observed: yes" + Environment.NewLine +
                "Executions: 2" + Environment.NewLine +
                "Distinct execution IDs: yes" + Environment.NewLine +
                "Distinct correlation IDs: yes" + Environment.NewLine +
                "Both executions succeeded: yes" + Environment.NewLine +
                "Runtime-selected discovered model: runtime-discovered-model-42" + Environment.NewLine +
                "Instance isolation after retire: yes");
        }

        private sealed class NullSecretStore : ISecretStore
        {
            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.FromResult(string.Empty); }
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        }

        private sealed class RuntimeConcurrencyTestAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            private int _activeCalls;
            private int _overlapObserved;
            private int _allTargetsCorrect = 1;
            private string _observedModelId = string.Empty;

            public string Kind { get { return "RuntimeConcurrencyTest"; } }
            public string DisplayName { get { return "Runtime Concurrency Test Adapter"; } }
            public bool OverlapObserved { get { return Volatile.Read(ref _overlapObserved) != 0; } }
            public bool AllObservedTargetsCorrect { get { return Volatile.Read(ref _allTargetsCorrect) != 0; } }
            public string ObservedModelId { get { return _observedModelId; } }

            public bool CanHandle(AiProvider provider) { return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase); }

            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                return Task.FromResult(new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    IsPartial = false,
                    Message = "Deterministic runtime discovery.",
                    Models = { new AiModelMetadata
                    {
                        ProviderId = provider.Id,
                        ModelId = "runtime-discovered-model-42",
                        LogicalModelId = "runtime-logical-model-42",
                        Source = AiMetadataSource.DiscoveryApi,
                        Cost = AiCostStatus.Free,
                        Capabilities = new AiModelCapabilities { Model = "runtime-discovered-model-42" }
                    } }
                });
            }

            public async Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (request.ExecutionTarget == null || !string.Equals(request.ExecutionTarget.ModelId, "runtime-discovered-model-42", StringComparison.OrdinalIgnoreCase))
                    Interlocked.Exchange(ref _allTargetsCorrect, 0);
                else
                    _observedModelId = request.ExecutionTarget.ModelId;

                if (Interlocked.Increment(ref _activeCalls) > 1) Interlocked.Exchange(ref _overlapObserved, 1);
                try
                {
                    await Task.Delay(150, cancellationToken).ConfigureAwait(false);
                    return new AIResponse
                    {
                        Text = "RUNTIME-CONCURRENT-OK",
                        ProviderId = request.Provider == null ? string.Empty : request.Provider.Id,
                        Model = request.ExecutionTarget == null ? string.Empty : request.ExecutionTarget.ModelId
                    };
                }
                finally { Interlocked.Decrement(ref _activeCalls); }
            }
        }
    }
}

using System;
using System.Collections.Generic;
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
                "Runs two independent runtime instances concurrently against a local adapter and verifies their execution identities and results remain isolated.",
                "Both executions should overlap, complete successfully, and retain distinct instance/execution/correlation identities.",
                "Runtime concurrency verification.",
                TestRuntimeConcurrencyAsync,
                "Two independent instances",
                "Uses only a local adapter. No external provider is contacted.");
        }

        private async Task TestRuntimeConcurrencyAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = CreateExampleSecretStore();
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var provider = (await store.GetProvidersAsync().ConfigureAwait(true))
                .FirstOrDefault(x => string.Equals(x.Id, profile.ProviderId, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
                throw new InvalidOperationException("The selected agent's primary provider could not be found.");

            var adapter = new RuntimeConcurrencyTestAdapter();
            var client = new HAgentClient(
                store,
                secrets,
                new[] { adapter });

            var registry = new AgentRuntimeInstanceRegistry();
            var first = registry.Create(profile, AgentRuntimeScope.Session);
            var second = registry.Create(profile, AgentRuntimeScope.Task);

            var executionOptions = new AgentExecutionOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                MaxProviderAttempts = 1,
                MaxRetriesPerProvider = 0
            };

            var firstTask = client.ExecuteAsync(
                first,
                (message ?? string.Empty) + " [instance-1]",
                executionOptions,
                CancellationToken.None);
            var secondTask = client.ExecuteAsync(
                second,
                (message ?? string.Empty) + " [instance-2]",
                executionOptions,
                CancellationToken.None);

            var executions = await Task.WhenAll(firstTask, secondTask).ConfigureAwait(true);

            if (executions.Length != 2)
                throw new InvalidOperationException("Expected two concurrent executions.");
            if (!adapter.OverlapObserved)
                throw new InvalidOperationException("The local adapter did not observe concurrent provider calls.");
            if (string.Equals(executions[0].Id, executions[1].Id, StringComparison.Ordinal))
                throw new InvalidOperationException("Concurrent executions shared an execution ID.");
            if (string.Equals(executions[0].CorrelationId, executions[1].CorrelationId, StringComparison.Ordinal))
                throw new InvalidOperationException("Concurrent executions shared a correlation ID.");
            if (executions.Any(x => x.State != AgentExecutionState.Succeeded))
                throw new InvalidOperationException("At least one concurrent execution did not succeed.");

            var active = registry.GetActiveInstances();
            if (active.Count != 2)
                throw new InvalidOperationException("The registry lost an active runtime instance during concurrent execution.");
            if (active.Any(x => !string.Equals(x.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A runtime instance lost its source profile identity.");
            if (string.Equals(first.InstanceId, second.InstanceId, StringComparison.Ordinal))
                throw new InvalidOperationException("Concurrent runtime instances shared an instance ID.");

            registry.Retire(first.InstanceId);
            if (first.State != AgentRuntimeInstanceState.Retired || second.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Retiring one concurrent instance affected another instance.");
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
                "Instance isolation after retire: yes");
        }

        private sealed class RuntimeConcurrencyTestAdapter : IAiProviderAdapter
        {
            private int _activeCalls;
            private int _overlapObserved;

            public string Kind { get { return "RuntimeConcurrencyTest"; } }
            public string DisplayName { get { return "Runtime Concurrency Test Adapter"; } }
            public bool OverlapObserved { get { return Volatile.Read(ref _overlapObserved) != 0; } }

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
                if (Interlocked.Increment(ref _activeCalls) > 1)
                    Interlocked.Exchange(ref _overlapObserved, 1);

                try
                {
                    await Task.Delay(150, cancellationToken).ConfigureAwait(false);
                    return new AIResponse
                    {
                        Text = "RUNTIME-CONCURRENT-OK",
                        ProviderId = provider == null ? string.Empty : provider.Id
                    };
                }
                finally
                {
                    Interlocked.Decrement(ref _activeCalls);
                }
            }
        }
    }
}

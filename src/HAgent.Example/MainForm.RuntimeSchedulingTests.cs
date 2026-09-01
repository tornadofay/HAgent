using System;
using System.Collections.Generic;
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
        private void AddRuntimeSchedulingTab()
        {
            AddApiTab(
                "RUNTIME SCHEDULING",
                "Run runtime scheduling test",
                "Verifies host-controlled bounded admission without changing runtime execution semantics.",
                "Two executions should run one at a time with a scheduler concurrency limit of one, while queued caller cancellation remains possible.",
                "Runtime scheduling verification.",
                TestRuntimeSchedulingAsync,
                "Host-controlled admission",
                "Uses only a local adapter; no external provider is contacted.");
        }

        private async Task TestRuntimeSchedulingAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var adapter = new RuntimeSchedulingTestAdapter();
            var client = new HAgentClient(store, secrets, new[] { adapter });
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            using (var scheduler = new AgentExecutionScheduler(client, 1))
            {
                var options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                };

                var firstTask = scheduler.ScheduleAsync(instance, "schedule-first", options, CancellationToken.None);
                await adapter.FirstStarted.Task.ConfigureAwait(true);

                var secondCts = new CancellationTokenSource();
                var secondTask = scheduler.ScheduleAsync(instance, "schedule-second", options, secondCts.Token);
                secondCts.CancelAfter(25);

                var first = await firstTask.ConfigureAwait(true);
                var secondCancelled = false;
                try
                {
                    await secondTask.ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    secondCancelled = true;
                }

                if (!secondCancelled)
                    throw new InvalidOperationException("A queued scheduling request did not honor caller cancellation.");
                if (adapter.MaxActiveCalls != 1)
                    throw new InvalidOperationException("The scheduler admitted more executions than its configured concurrency limit.");
                if (first.State != AgentExecutionState.Succeeded)
                    throw new InvalidOperationException("The admitted execution did not complete successfully.");

                Write("RUNTIME SCHEDULING",
                    "Contract test succeeded." + Environment.NewLine +
                    "Runtime instance: " + instance.InstanceId + Environment.NewLine +
                    "Maximum concurrency: " + scheduler.MaximumConcurrency + Environment.NewLine +
                    "Maximum provider calls observed: " + adapter.MaxActiveCalls + Environment.NewLine +
                    "First execution succeeded: yes" + Environment.NewLine +
                    "Queued second execution cancellation: honored" + Environment.NewLine +
                    "Host controls admission: yes");
            }
        }

        private sealed class RuntimeSchedulingTestAdapter : IAiProviderAdapter
        {
            private int _activeCalls;
            private int _maxActiveCalls;

            public readonly TaskCompletionSource<bool> FirstStarted = new TaskCompletionSource<bool>();

            public string Kind { get { return "RuntimeSchedulingTest"; } }
            public string DisplayName { get { return "Runtime Scheduling Test Adapter"; } }
            public int MaxActiveCalls { get { return Volatile.Read(ref _maxActiveCalls); } }

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
                var active = Interlocked.Increment(ref _activeCalls);
                while (true)
                {
                    var observed = Volatile.Read(ref _maxActiveCalls);
                    if (active <= observed || Interlocked.CompareExchange(ref _maxActiveCalls, active, observed) == observed)
                        break;
                }

                try
                {
                    FirstStarted.TrySetResult(true);
                    await Task.Delay(120, cancellationToken).ConfigureAwait(false);
                    return new AIResponse
                    {
                        Text = "RUNTIME-SCHEDULE-OK",
                        ProviderId = request.Provider == null ? string.Empty : request.Provider.Id
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

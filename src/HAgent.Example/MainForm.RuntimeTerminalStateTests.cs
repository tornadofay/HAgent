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
        private void AddRuntimeTerminalStateTab()
        {
            AddApiTab(
                "RUNTIME TERMINAL STATE",
                "Run terminal state race test",
                "Verifies that cancellation and timeout become terminal before a non-cooperative provider completes late.",
                "Late provider responses must never overwrite a cancelled or timed-out execution, and HAgent must complete the caller-facing task without waiting for the late provider.",
                "Runtime terminal-state hardening verification.",
                TestRuntimeTerminalStateAsync,
                "First terminal outcome wins",
                "Uses only local adapters; no external provider is contacted.");
        }

        private async Task TestRuntimeTerminalStateAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            AgentExecution cancellationExecution = null;
            var cancellationAdapter = new LateResponseTestAdapter();
            var cancellationRuntime = new DefaultAgentRuntime(store, secrets, new[] { cancellationAdapter });
            cancellationRuntime.ExecutionChanged += (sender, args) =>
            {
                if (args != null && args.Execution != null)
                    cancellationExecution = args.Execution;
            };

            var cancellationCts = new CancellationTokenSource();
            var cancellationTask = cancellationRuntime.ExecuteAsync(
                profile.Id,
                "terminal-cancel",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                },
                cancellationCts.Token);

            await cancellationAdapter.Started.Task.ConfigureAwait(true);
            cancellationCts.Cancel();

            var cancellationCompleted = await Task.WhenAny(
                cancellationTask,
                Task.Delay(1000)).ConfigureAwait(true);
            if (cancellationCompleted != cancellationTask)
                throw new InvalidOperationException("Caller cancellation did not complete before the non-cooperative provider returned.");

            var cancellationObserved = false;
            try
            {
                await cancellationTask.ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                cancellationObserved = true;
            }

            if (!cancellationObserved)
                throw new InvalidOperationException("Caller cancellation did not propagate to the caller.");
            if (cancellationExecution == null)
                throw new InvalidOperationException("No execution lifecycle was captured for caller cancellation.");
            if (cancellationExecution.State != AgentExecutionState.Cancelled ||
                cancellationExecution.FailureKind != AgentExecutionFailureKind.Cancelled)
                throw new InvalidOperationException("Caller cancellation did not become the terminal execution outcome.");

            cancellationAdapter.Release();
            await cancellationAdapter.Completed.Task.ConfigureAwait(true);
            if (cancellationExecution.State != AgentExecutionState.Cancelled || cancellationExecution.Response != null)
                throw new InvalidOperationException("A late provider response overwrote a cancelled execution.");

            AgentExecution timeoutExecution = null;
            var timeoutAdapter = new LateResponseTestAdapter();
            var timeoutRuntime = new DefaultAgentRuntime(store, secrets, new[] { timeoutAdapter });
            timeoutRuntime.ExecutionChanged += (sender, args) =>
            {
                if (args != null && args.Execution != null)
                    timeoutExecution = args.Execution;
            };

            var timeoutTask = timeoutRuntime.ExecuteAsync(
                profile.Id,
                "terminal-timeout",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromMilliseconds(75),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                },
                CancellationToken.None);

            await timeoutAdapter.Started.Task.ConfigureAwait(true);
            var timeoutCompleted = await Task.WhenAny(
                timeoutTask,
                Task.Delay(1000)).ConfigureAwait(true);
            if (timeoutCompleted != timeoutTask)
                throw new InvalidOperationException("Execution timeout did not complete before the non-cooperative provider returned.");

            var timeoutObserved = false;
            try
            {
                await timeoutTask.ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                timeoutObserved = true;
            }

            if (!timeoutObserved)
                throw new InvalidOperationException("Execution timeout did not propagate as cancellation.");
            if (timeoutExecution == null)
                throw new InvalidOperationException("No execution lifecycle was captured for timeout.");
            if (timeoutExecution.State != AgentExecutionState.Cancelled ||
                timeoutExecution.FailureKind != AgentExecutionFailureKind.Timeout)
                throw new InvalidOperationException("Timeout did not become the terminal execution outcome.");

            timeoutAdapter.Release();
            await timeoutAdapter.Completed.Task.ConfigureAwait(true);
            if (timeoutExecution.State != AgentExecutionState.Cancelled || timeoutExecution.Response != null)
                throw new InvalidOperationException("A late provider response overwrote a timed-out execution.");

            Write("RUNTIME TERMINAL STATE",
                "Contract test succeeded." + Environment.NewLine +
                "Caller cancellation completed before late provider: yes" + Environment.NewLine +
                "Caller-cancelled execution state: " + cancellationExecution.State + Environment.NewLine +
                "Late response after caller cancellation overwrote state: no" + Environment.NewLine +
                "Timeout completed before late provider: yes" + Environment.NewLine +
                "Timed-out execution state: " + timeoutExecution.State + Environment.NewLine +
                "Late response after timeout overwrote state: no");
        }

        private sealed class LateResponseTestAdapter : IAiProviderAdapter
        {
            private readonly TaskCompletionSource<bool> _release = new TaskCompletionSource<bool>();

            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            public readonly TaskCompletionSource<bool> Completed = new TaskCompletionSource<bool>();

            public string Kind { get { return "LateResponseTest"; } }
            public string DisplayName { get { return "Late Response Test Adapter"; } }

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

                Started.TrySetResult(true);
                await _release.Task.ConfigureAwait(false);
                Completed.TrySetResult(true);
                return new AIResponse
                {
                    Text = "LATE-RESPONSE-IGNORED",
                    ProviderId = request.Provider == null ? string.Empty : request.Provider.Id
                };
            }

            public void Release()
            {
                _release.TrySetResult(true);
            }
        }
    }
}

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
        private void AddExecutionInterventionTab()
        {
            AddApiTab(
                "EXECUTION INTERVENTION",
                "Run execution pause/resume/cancel test",
                "Verifies that human intervention controls an active execution through the public HAgentClient API without creating a second execution engine.",
                "Pause must stop the execution at the next cooperative boundary, Resume must release it, and Cancel must terminate it even when the provider itself is not cooperative.",
                "Uses only an in-memory store, the public HAgentClient intervention API, and a deterministic local adapter.",
                TestExecutionInterventionAsync,
                "Execution control boundary",
                "Intervention is requested and explicitly approved before the runtime applies the control. Pause is cooperative; cancellation propagates through the execution token.");
        }

        private async Task TestExecutionInterventionAsync(string message)
        {
            const string providerId = "intervention-provider-42";
            const string agentId = "intervention-agent-42";

            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(new AiProvider
            {
                Id = providerId,
                Name = "Execution Intervention Provider",
                Kind = "ExecutionInterventionTest",
                BaseUrl = "https://execution-intervention.test/v1",
                DefaultModel = "execution-intervention-model-42",
                Enabled = true
            }).ConfigureAwait(true);
            await store.SaveAgentAsync(new AiAgent
            {
                Id = agentId,
                Name = "Execution Intervention Agent",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            }).ConfigureAwait(true);

            var adapter = new ExecutionInterventionTestAdapter();
            var client = new HAgentClient(store, new InterventionTestSecretStore(), new[] { adapter });
            AgentExecution activeExecution = null;
            client.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
            {
                if (activeExecution == null && args != null && args.Execution != null && !args.Execution.IsCompleted)
                    activeExecution = args.Execution;
            };

            var executionTask = client.ExecuteAsync(
                agentId,
                (message ?? string.Empty) + " [pause-resume]",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    HostCorrelationId = "host-intervention-42"
                },
                CancellationToken.None);

            await adapter.Started.Task.ConfigureAwait(true);
            if (activeExecution == null)
                throw new InvalidOperationException("Execution lifecycle event did not expose the active execution.");

            var pauseRequest = await client.RequestExecutionInterventionAsync(
                activeExecution.Id,
                AiInterventionAction.Pause,
                new AgentIdentityContext(userId: "operator-42"),
                "host-intervention-42",
                "Pause for deterministic intervention verification.",
                CancellationToken.None).ConfigureAwait(true);
            if (pauseRequest.Status != AiInterventionRequestStatus.Pending ||
                pauseRequest.TargetKind != AiInterventionTargetKind.Execution ||
                pauseRequest.RequestedAction != AiInterventionAction.Pause ||
                pauseRequest.ExecutionId != activeExecution.Id)
                throw new InvalidOperationException("Pause intervention request did not contain the expected target, action, or pending state.");

            var paused = await client.ResolveInterventionRequestAsync(
                pauseRequest.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator-42"),
                "Pause approved.",
                CancellationToken.None).ConfigureAwait(true);
            if (paused.Status != AiInterventionRequestStatus.Completed)
                throw new InvalidOperationException("Approved pause intervention did not reach Completed after the control was applied.");
            if (await client.GetExecutionControlStateAsync(activeExecution.Id, CancellationToken.None).ConfigureAwait(true) != AiExecutionControlState.Paused)
                throw new InvalidOperationException("Approved pause intervention did not place the execution in Paused state.");

            adapter.Release();
            await adapter.ResponseProduced.Task.ConfigureAwait(true);
            var pausedCompletion = await Task.WhenAny(executionTask, Task.Delay(150)).ConfigureAwait(true);
            if (pausedCompletion == executionTask)
                throw new InvalidOperationException("A paused execution completed before resume was approved.");

            var resumeRequest = await client.RequestExecutionInterventionAsync(
                activeExecution.Id,
                AiInterventionAction.Resume,
                new AgentIdentityContext(userId: "operator-42"),
                "host-intervention-42",
                "Resume after deterministic pause verification.",
                CancellationToken.None).ConfigureAwait(true);
            var resumed = await client.ResolveInterventionRequestAsync(
                resumeRequest.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator-42"),
                "Resume approved.",
                CancellationToken.None).ConfigureAwait(true);
            if (resumed.Status != AiInterventionRequestStatus.Completed)
                throw new InvalidOperationException("Approved resume intervention did not reach Completed after the control was applied.");

            var successfulExecution = await executionTask.ConfigureAwait(true);
            if (successfulExecution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Resumed execution did not complete successfully.");
            if (successfulExecution.Response == null || successfulExecution.Response.Text != "EXECUTION-INTERVENTION-OK")
                throw new InvalidOperationException("Resumed execution returned an unexpected provider response.");

            AgentExecution cancellationExecution = null;
            var cancellationAdapter = new ExecutionInterventionTestAdapter();
            var cancellationClient = new HAgentClient(store, new InterventionTestSecretStore(), new[] { cancellationAdapter });
            cancellationClient.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
            {
                if (cancellationExecution == null && args != null && args.Execution != null && !args.Execution.IsCompleted)
                    cancellationExecution = args.Execution;
            };

            var cancellationTask = cancellationClient.ExecuteAsync(
                agentId,
                (message ?? string.Empty) + " [cancel]",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    HostCorrelationId = "host-intervention-43"
                },
                CancellationToken.None);

            await cancellationAdapter.Started.Task.ConfigureAwait(true);
            if (cancellationExecution == null)
                throw new InvalidOperationException("Cancellation execution lifecycle event did not expose the active execution.");

            var cancelRequest = await cancellationClient.RequestExecutionInterventionAsync(
                cancellationExecution.Id,
                AiInterventionAction.Cancel,
                new AgentIdentityContext(userId: "operator-43"),
                "host-intervention-43",
                "Cancel for deterministic intervention verification.",
                CancellationToken.None).ConfigureAwait(true);
            var cancelled = await cancellationClient.ResolveInterventionRequestAsync(
                cancelRequest.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator-43"),
                "Cancel approved.",
                CancellationToken.None).ConfigureAwait(true);
            if (cancelled.Status != AiInterventionRequestStatus.Completed)
                throw new InvalidOperationException("Approved cancel intervention did not reach Completed after the control was applied.");

            try
            {
                await cancellationTask.ConfigureAwait(true);
                throw new InvalidOperationException("Intervention cancellation unexpectedly completed as success.");
            }
            catch (OperationCanceledException)
            {
            }

            if (cancellationExecution.State != AgentExecutionState.Cancelled ||
                cancellationExecution.FailureKind != AgentExecutionFailureKind.Cancelled)
                throw new InvalidOperationException("Approved cancel intervention did not produce the Cancelled execution outcome.");

            cancellationAdapter.Release();
            await cancellationAdapter.ResponseProduced.Task.ConfigureAwait(true);
            if (cancellationExecution.State != AgentExecutionState.Cancelled || cancellationExecution.Response != null)
                throw new InvalidOperationException("A late provider response overwrote an intervention-cancelled execution.");

            Write("EXECUTION INTERVENTION",
                "Contract test succeeded." + Environment.NewLine +
                "Public HAgentClient ExecutionChanged lifecycle: verified." + Environment.NewLine +
                "Pause intervention request target/action/correlation: verified." + Environment.NewLine +
                "Approved pause reached Completed after application: verified." + Environment.NewLine +
                "Execution entered Paused state: verified." + Environment.NewLine +
                "Paused execution remained incomplete until resume: verified." + Environment.NewLine +
                "Approved resume reached Completed after application: verified." + Environment.NewLine +
                "Resumed execution succeeded: verified." + Environment.NewLine +
                "Approved cancellation reached Completed after application: verified." + Environment.NewLine +
                "Intervention cancellation produced terminal Cancelled execution: verified." + Environment.NewLine +
                "Late provider response could not overwrite cancelled execution: verified.");
        }

        private sealed class InterventionTestSecretStore : ISecretStore
        {
            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.FromResult(string.Empty); }
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        }

        private sealed class ExecutionInterventionTestAdapter : IAiProviderAdapter
        {
            private readonly TaskCompletionSource<bool> _release = new TaskCompletionSource<bool>();

            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            public readonly TaskCompletionSource<bool> ResponseProduced = new TaskCompletionSource<bool>();

            public string Kind { get { return "ExecutionInterventionTest"; } }
            public string DisplayName { get { return "Execution Intervention Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public async Task<AIResponse> SendAsync(
                ProviderExecutionRequest request,
                CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                Started.TrySetResult(true);
                await _release.Task.ConfigureAwait(false);
                ResponseProduced.TrySetResult(true);
                return new AIResponse
                {
                    Text = "EXECUTION-INTERVENTION-OK",
                    ProviderId = request.Provider == null ? string.Empty : request.Provider.Id,
                    Model = request.ExecutionTarget == null ? string.Empty : request.ExecutionTarget.ModelId
                };
            }

            public void Release()
            {
                _release.TrySetResult(true);
            }
        }
    }
}

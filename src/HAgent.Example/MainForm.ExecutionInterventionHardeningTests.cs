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
        private void AddExecutionInterventionHardeningTab()
        {
            AddApiTab(
                "INTERVENTION HARDENING",
                "Run intervention concurrency/stale-state test",
                "Verifies stale intervention rejection, serialized concurrent control, and duplicate responder protection.",
                "A request captures the target control state and version at creation. Later control changes make that request stale instead of allowing it to act retroactively.",
                "Uses only an in-memory store, the public HAgentClient intervention API, and deterministic local adapters.",
                TestExecutionInterventionHardeningAsync,
                "Concurrency and stale state",
                "Concurrent approvals are serialized per execution. Stale or duplicate requests remain diagnosable and cannot create conflicting control transitions.");
        }

        private async Task TestExecutionInterventionHardeningAsync(string message)
        {
            const string providerId = "intervention-hardening-provider-42";
            const string agentId = "intervention-hardening-agent-42";

            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(new AiProvider
            {
                Id = providerId,
                Name = "Intervention Hardening Provider",
                Kind = "InterventionHardeningTest",
                BaseUrl = "https://intervention-hardening.test/v1",
                DefaultModel = "intervention-hardening-model-42",
                Enabled = true
            }).ConfigureAwait(true);
            await store.SaveAgentAsync(new AiAgent
            {
                Id = agentId,
                Name = "Intervention Hardening Agent",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            }).ConfigureAwait(true);

            await VerifyStaleRequestAsync(store, agentId, message, providerId).ConfigureAwait(true);
            await VerifyConcurrentRequestsAsync(store, agentId, message, providerId).ConfigureAwait(true);
            await VerifyDuplicateResponderAsync(store, agentId, message, providerId).ConfigureAwait(true);

            Write("INTERVENTION HARDENING",
                "Concurrency/stale-state contract test succeeded." + Environment.NewLine +
                "Stale intervention after terminal execution resolved as Expired: verified." + Environment.NewLine +
                "Target state/version captured at request creation: verified." + Environment.NewLine +
                "Concurrent conflicting interventions produced one applied control and one stale request: verified." + Environment.NewLine +
                "Paused execution remained blocked until explicit resume: verified." + Environment.NewLine +
                "Duplicate responder resolution could not apply a second transition: verified.");
        }

        private async Task VerifyStaleRequestAsync(InMemoryAiStore store, string agentId, string message, string providerId)
        {
            var adapter = new InterventionHardeningTestAdapter();
            var client = new HAgentClient(store, new InterventionHardeningSecretStore(), new[] { adapter });
            AgentExecution execution = null;
            client.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
            {
                if (execution == null && args != null && args.Execution != null && !args.Execution.IsCompleted)
                    execution = args.Execution;
            };

            var executionTask = client.ExecuteAsync(
                agentId,
                (message ?? string.Empty) + " [stale]",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    HostCorrelationId = "host-intervention-hardening-stale"
                },
                CancellationToken.None);

            await adapter.Started.Task.ConfigureAwait(true);
            if (execution == null)
                throw new InvalidOperationException("Stale-state execution lifecycle event did not expose the active execution.");

            var request = await client.RequestExecutionInterventionAsync(
                execution.Id,
                AiInterventionAction.Pause,
                new AgentIdentityContext(userId: "operator-hardening"),
                "host-intervention-hardening-stale",
                "Create a request before execution completes.",
                CancellationToken.None).ConfigureAwait(true);

            if (request.TargetState != AiExecutionControlState.Running.ToString() || request.TargetStateVersion != 0)
                throw new InvalidOperationException("Execution intervention did not capture the expected initial control state/version.");

            adapter.Release();
            await adapter.ResponseProduced.Task.ConfigureAwait(true);
            var completed = await executionTask.ConfigureAwait(true);
            if (completed.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("The stale-state execution did not complete successfully before resolving the intervention.");

            var expired = await client.ResolveInterventionRequestAsync(
                request.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator-hardening"),
                "Attempt to apply a stale pause request.",
                CancellationToken.None).ConfigureAwait(true);
            if (expired.Status != AiInterventionRequestStatus.Expired)
                throw new InvalidOperationException("A stale intervention request did not resolve to Expired.");
        }

        private async Task VerifyConcurrentRequestsAsync(InMemoryAiStore store, string agentId, string message, string providerId)
        {
            var adapter = new InterventionHardeningTestAdapter();
            var client = new HAgentClient(store, new InterventionHardeningSecretStore(), new[] { adapter });
            AgentExecution execution = null;
            client.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
            {
                if (execution == null && args != null && args.Execution != null && !args.Execution.IsCompleted)
                    execution = args.Execution;
            };

            var executionTask = client.ExecuteAsync(
                agentId,
                (message ?? string.Empty) + " [concurrent]",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    HostCorrelationId = "host-intervention-hardening-concurrent"
                },
                CancellationToken.None);

            await adapter.Started.Task.ConfigureAwait(true);
            if (execution == null)
                throw new InvalidOperationException("Concurrent intervention execution lifecycle event did not expose the active execution.");

            var first = await client.RequestExecutionInterventionAsync(
                execution.Id,
                AiInterventionAction.Pause,
                new AgentIdentityContext(userId: "operator-hardening-1"),
                "host-intervention-hardening-concurrent",
                "First concurrent pause.",
                CancellationToken.None).ConfigureAwait(true);
            var second = await client.RequestExecutionInterventionAsync(
                execution.Id,
                AiInterventionAction.Resume,
                new AgentIdentityContext(userId: "operator-hardening-2"),
                "host-intervention-hardening-concurrent",
                "Conflicting concurrent resume.",
                CancellationToken.None).ConfigureAwait(true);

            var firstTask = ResolveAsync(client, first.RequestId, "operator-hardening-1");
            var secondTask = ResolveAsync(client, second.RequestId, "operator-hardening-2");
            var results = await Task.WhenAll(firstTask, secondTask).ConfigureAwait(true);

            var completedCount = 0;
            var expiredCount = 0;
            foreach (var result in results)
            {
                if (result.Status == AiInterventionRequestStatus.Completed) completedCount++;
                if (result.Status == AiInterventionRequestStatus.Expired) expiredCount++;
            }

            if (completedCount != 1 || expiredCount != 1)
                throw new InvalidOperationException("Concurrent conflicting interventions did not produce exactly one applied request and one stale request.");

            if (await client.GetExecutionControlStateAsync(execution.Id, CancellationToken.None).ConfigureAwait(true) != AiExecutionControlState.Paused)
                throw new InvalidOperationException("Concurrent intervention resolution did not leave the execution in the single applied control state.");

            adapter.Release();
            await adapter.ResponseProduced.Task.ConfigureAwait(true);
            var beforeResume = await Task.WhenAny(executionTask, Task.Delay(150)).ConfigureAwait(true);
            if (beforeResume == executionTask)
                throw new InvalidOperationException("A paused execution completed before a fresh resume intervention was approved.");

            var resume = await client.RequestExecutionInterventionAsync(
                execution.Id,
                AiInterventionAction.Resume,
                new AgentIdentityContext(userId: "operator-hardening-3"),
                "host-intervention-hardening-concurrent",
                "Resume after the concurrency assertion.",
                CancellationToken.None).ConfigureAwait(true);
            var resumed = await client.ResolveInterventionRequestAsync(
                resume.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator-hardening-3"),
                "Resume approved.",
                CancellationToken.None).ConfigureAwait(true);
            if (resumed.Status != AiInterventionRequestStatus.Completed)
                throw new InvalidOperationException("Fresh resume intervention did not complete after the stale/concurrency check.");

            var completed = await executionTask.ConfigureAwait(true);
            if (completed.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Execution did not complete successfully after the fresh resume intervention.");
        }

        private async Task VerifyDuplicateResponderAsync(InMemoryAiStore store, string agentId, string message, string providerId)
        {
            var adapter = new InterventionHardeningTestAdapter();
            var client = new HAgentClient(store, new InterventionHardeningSecretStore(), new[] { adapter });
            AgentExecution execution = null;
            client.ExecutionChanged += delegate(object sender, AgentExecutionEventArgs args)
            {
                if (execution == null && args != null && args.Execution != null && !args.Execution.IsCompleted)
                    execution = args.Execution;
            };

            var executionTask = client.ExecuteAsync(
                agentId,
                (message ?? string.Empty) + " [duplicate]",
                new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    HostCorrelationId = "host-intervention-hardening-duplicate"
                },
                CancellationToken.None);

            await adapter.Started.Task.ConfigureAwait(true);
            if (execution == null)
                throw new InvalidOperationException("Duplicate responder execution lifecycle event did not expose the active execution.");

            var request = await client.RequestExecutionInterventionAsync(
                execution.Id,
                AiInterventionAction.Pause,
                new AgentIdentityContext(userId: "operator-hardening-duplicate"),
                "host-intervention-hardening-duplicate",
                "Duplicate responder test.",
                CancellationToken.None).ConfigureAwait(true);

            var firstTask = ResolveAsync(client, request.RequestId, "operator-hardening-duplicate");
            var secondTask = ResolveExpectingFailureAsync(client, request.RequestId, "operator-hardening-duplicate");
            var first = await firstTask.ConfigureAwait(true);
            var secondFailed = await secondTask.ConfigureAwait(true);

            if (first.Status != AiInterventionRequestStatus.Completed || !secondFailed)
                throw new InvalidOperationException("Duplicate responder resolution was not serialized by request lifecycle state.");

            adapter.Release();
            await adapter.ResponseProduced.Task.ConfigureAwait(true);
            var beforeResume = await Task.WhenAny(executionTask, Task.Delay(100)).ConfigureAwait(true);
            if (beforeResume == executionTask)
                throw new InvalidOperationException("Duplicate responder test unexpectedly completed while paused.");

            var resume = await client.RequestExecutionInterventionAsync(
                execution.Id,
                AiInterventionAction.Resume,
                new AgentIdentityContext(userId: "operator-hardening-duplicate"),
                "host-intervention-hardening-duplicate",
                "Resume after duplicate resolver test.",
                CancellationToken.None).ConfigureAwait(true);
            await client.ResolveInterventionRequestAsync(
                resume.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator-hardening-duplicate"),
                "Resume approved.",
                CancellationToken.None).ConfigureAwait(true);
            var completed = await executionTask.ConfigureAwait(true);
            if (completed.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Execution did not recover after the duplicate responder test.");
        }

        private static async Task<AiInterventionRequest> ResolveAsync(HAgentClient client, string requestId, string responderId)
        {
            return await client.ResolveInterventionRequestAsync(
                requestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: responderId),
                "Concurrent approval.",
                CancellationToken.None).ConfigureAwait(true);
        }

        private static async Task<bool> ResolveExpectingFailureAsync(HAgentClient client, string requestId, string responderId)
        {
            try
            {
                await client.ResolveInterventionRequestAsync(
                    requestId,
                    AiInterventionRequestStatus.Approved,
                    new AgentIdentityContext(userId: responderId),
                    "Duplicate approval.",
                    CancellationToken.None).ConfigureAwait(true);
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private sealed class InterventionHardeningSecretStore : ISecretStore
        {
            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.FromResult(string.Empty); }
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        }

        private sealed class InterventionHardeningTestAdapter : IAiProviderAdapter
        {
            private readonly TaskCompletionSource<bool> _release = new TaskCompletionSource<bool>();

            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            public readonly TaskCompletionSource<bool> ResponseProduced = new TaskCompletionSource<bool>();

            public string Kind { get { return "InterventionHardeningTest"; } }
            public string DisplayName { get { return "Intervention Hardening Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public async Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                Started.TrySetResult(true);
                await _release.Task.ConfigureAwait(false);
                ResponseProduced.TrySetResult(true);
                return new AIResponse
                {
                    Text = "INTERVENTION-HARDENING-OK",
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class InterventionTests
    {
        [Fact]
        public async Task InMemoryWorkflow_ConcurrentResolution_AllowsOnlyOneTerminalTransition()
        {
            var workflow = new InMemoryAiInterventionWorkflow();
            var request = await workflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                AiInterventionAction.Pause,
                "execution.pause",
                "execution",
                "execution-1",
                "correlation-1",
                "host-1",
                "agent-1",
                "runtime-1",
                "execution-1",
                string.Empty,
                "pause it",
                new AgentIdentityContext(userId: "requester"));

            var first = workflow.ResolveAsync(request.RequestId, AiInterventionRequestStatus.Approved, new AgentIdentityContext(userId: "a"), "approved");
            var second = workflow.ResolveAsync(request.RequestId, AiInterventionRequestStatus.Rejected, new AgentIdentityContext(userId: "b"), "rejected");

            var failures = 0;
            try { await first; } catch (InvalidOperationException) { failures++; }
            try { await second; } catch (InvalidOperationException) { failures++; }

            Assert.Equal(1, failures);
            var final = await workflow.GetAsync(request.RequestId);
            Assert.NotEqual(AiInterventionRequestStatus.Pending, final.Status);
            Assert.Equal(1, final.Version);
        }

        [Fact]
        public async Task Execution_ApprovalWaits_ThenResumesThroughSameRuntimeBoundary()
        {
            var provider = new AiProvider
            {
                Id = "provider-approval",
                Name = "Approval Provider",
                Kind = "openai-compatible",
                DefaultModel = "approval-model"
            };
            var agent = new AiAgent
            {
                Id = "agent-approval",
                Name = "Approval Agent",
                ProviderId = provider.Id,
                Model = provider.DefaultModel,
                Enabled = true
            };
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(provider);
            await store.SaveAgentAsync(agent);

            var policy = new AiPolicySet { Version = "approval-execution-policy" };
            var rule = new AiPolicyRule
            {
                Id = "approval-execution-rule",
                Name = "Require approval",
                Scope = AiPolicyScopeKind.System,
                Outcome = AiPolicyOutcome.RequireApproval,
                Priority = 100,
                Reason = "A human must approve model invocation."
            };
            rule.Operations.Add("model.invoke");
            rule.ResourceTypes.Add("execution-target");
            policy.Rules.Add(rule);

            var adapter = new ImmediateEchoAdapter();
            var client = new HAgentClient(
                store,
                new EmptySecretStore(),
                new[] { adapter },
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new DefaultAiPolicyEngine(policy));

            var waiting = new TaskCompletionSource<AgentExecution>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.InterventionWorkflow.GetType();
            client.InterventionCoordinator.GetType();
            var runtimeChanged = client.ExecuteAsync(agent.Id, "hello");
            Assert.False(runtimeChanged.IsCompleted);

            // The request becomes visible through the shared workflow while the runtime task is waiting.
            AiInterventionRequest request = null;
            for (var i = 0; i < 100 && request == null; i++)
            {
                var pending = await client.GetPendingInterventionRequestsAsync();
                if (pending.Count > 0) request = pending[0];
                else await Task.Delay(10);
            }

            Assert.NotNull(request);
            Assert.Equal(AiInterventionTargetKind.Execution, request.TargetKind);
            Assert.Equal(AiInterventionAction.Approve, request.RequestedAction);
            Assert.Equal("model.invoke", request.Operation);

            var execution = await client.GetInterventionRequestAsync(request.RequestId);
            Assert.NotNull(execution);
            Assert.Equal(AiInterventionRequestStatus.Pending, execution.Status);

            await client.ResolveInterventionRequestAsync(
                request.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "approver"),
                "Approved by test.");

            var result = await runtimeChanged;
            Assert.Equal(AgentExecutionState.Succeeded, result.State);
            Assert.Equal("echo: hello", result.Response.Text);

            var completedIntervention = await client.GetInterventionRequestAsync(request.RequestId);
            Assert.Equal(AiInterventionRequestStatus.Completed, completedIntervention.Status);
            Assert.Equal("approver", completedIntervention.ResponderIdentity.UserId);
        }

        [Fact]
        public async Task Execution_PauseResume_UsesTargetRevision_AndRejectsStaleRequests()
        {
            var setup = await CreateDelayedClientAsync();
            var task = setup.Client.ExecuteAsync(setup.Agent.Id, "pause");
            var execution = await WaitForRunningExecutionAsync(setup.Client);

            var pauseRequest = await setup.Client.InterventionWorkflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                AiInterventionAction.Pause,
                "execution.pause",
                "execution",
                execution.Id,
                execution.CorrelationId,
                string.Empty,
                setup.Agent.Id,
                string.Empty,
                execution.Id,
                string.Empty,
                "pause execution",
                new AgentIdentityContext(userId: "operator"),
                CancellationToken.None,
                execution.ControlRevision);
            await setup.Client.ResolveInterventionRequestAsync(
                pauseRequest.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "operator"),
                "pause approved");
            var applied = await setup.Client.ApplyInterventionRequestAsync(
                pauseRequest.RequestId,
                new AgentIdentityContext(userId: "operator"),
                "pause applied");
            Assert.True(applied.Applied);

            await Task.Delay(50);
            Assert.Equal(AgentExecutionState.Paused, execution.State);

            var staleResume = await setup.Client.InterventionWorkflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                AiInterventionAction.Resume,
                "execution.resume",
                "execution",
                execution.Id,
                execution.CorrelationId,
                string.Empty,
                setup.Agent.Id,
                string.Empty,
                execution.Id,
                string.Empty,
                "stale resume",
                new AgentIdentityContext(userId: "operator"),
                CancellationToken.None,
                execution.ControlRevision - 1);
            await setup.Client.ResolveInterventionRequestAsync(staleResume.RequestId, AiInterventionRequestStatus.Approved, new AgentIdentityContext(userId: "operator"), "approved");
            var staleResult = await setup.Client.ApplyInterventionRequestAsync(staleResume.RequestId, new AgentIdentityContext(userId: "operator"));
            Assert.True(staleResult.IsStale);

            var resumeRequest = await setup.Client.InterventionWorkflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                AiInterventionAction.Resume,
                "execution.resume",
                "execution",
                execution.Id,
                execution.CorrelationId,
                string.Empty,
                setup.Agent.Id,
                string.Empty,
                execution.Id,
                string.Empty,
                "resume execution",
                new AgentIdentityContext(userId: "operator"),
                CancellationToken.None,
                execution.ControlRevision);
            await setup.Client.ResolveInterventionRequestAsync(resumeRequest.RequestId, AiInterventionRequestStatus.Approved, new AgentIdentityContext(userId: "operator"), "resume approved");
            var resumed = await setup.Client.ApplyInterventionRequestAsync(resumeRequest.RequestId, new AgentIdentityContext(userId: "operator"));
            Assert.True(resumed.Applied);
            setup.Release.SetResult(true);

            var result = await task;
            Assert.Equal(AgentExecutionState.Succeeded, result.State);
        }

        [Fact]
        public async Task Execution_InterventionCancel_MakesExecutionTerminalAndCancelsProviderWait()
        {
            var setup = await CreateDelayedClientAsync();
            var task = setup.Client.ExecuteAsync(setup.Agent.Id, "cancel");
            var execution = await WaitForRunningExecutionAsync(setup.Client);

            var request = await setup.Client.InterventionWorkflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                AiInterventionAction.Cancel,
                "execution.cancel",
                "execution",
                execution.Id,
                execution.CorrelationId,
                string.Empty,
                setup.Agent.Id,
                string.Empty,
                execution.Id,
                string.Empty,
                "cancel execution",
                new AgentIdentityContext(userId: "operator"),
                CancellationToken.None,
                execution.ControlRevision);
            await setup.Client.ResolveInterventionRequestAsync(request.RequestId, AiInterventionRequestStatus.Approved, new AgentIdentityContext(userId: "operator"), "cancel approved");
            var applied = await setup.Client.ApplyInterventionRequestAsync(request.RequestId, new AgentIdentityContext(userId: "operator"));
            Assert.True(applied.Applied);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
            Assert.Equal(AgentExecutionState.Cancelled, execution.State);
            var completed = await setup.Client.GetInterventionRequestAsync(request.RequestId);
            Assert.Equal(AiInterventionRequestStatus.Completed, completed.Status);
        }

        private static async Task<(HAgentClient Client, AiAgent Agent, TaskCompletionSource<bool> Release)> CreateDelayedClientAsync()
        {
            var provider = new AiProvider { Id = Guid.NewGuid().ToString("N"), Name = "Delayed", Kind = "openai-compatible", DefaultModel = "delayed-model" };
            var agent = new AiAgent { Id = Guid.NewGuid().ToString("N"), Name = "Delayed Agent", ProviderId = provider.Id, Model = provider.DefaultModel, Enabled = true };
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(provider);
            await store.SaveAgentAsync(agent);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var client = new HAgentClient(store, new EmptySecretStore(), new[] { new BlockingEchoAdapter(release.Task) });
            return (client, agent, release);
        }

        private static async Task<AgentExecution> WaitForRunningExecutionAsync(HAgentClient client)
        {
            var events = new List<AgentExecution>();
            EventHandler<AgentExecutionEventArgs> handler = (s, e) => { if (e.Execution.State == AgentExecutionState.Running) lock (events) events.Add(e.Execution); };
            client.GetType();
            var runtimeField = typeof(HAgentClient).GetField("_runtime", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var runtime = (IAgentRuntime)runtimeField.GetValue(client);
            runtime.ExecutionChanged += handler;
            try
            {
                for (var i = 0; i < 100; i++)
                {
                    lock (events) if (events.Count > 0) return events[0];
                    await Task.Delay(10);
                }
            }
            finally { runtime.ExecutionChanged -= handler; }
            throw new InvalidOperationException("No running execution event was observed.");
        }

        private sealed class ImmediateEchoAdapter : IAiProviderAdapter
        {
            public string Kind => "openai-compatible";
            public string DisplayName => "Immediate Echo";
            public bool CanHandle(AiProvider provider) => provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken token)
            {
                return Task.FromResult(new AIResponse { AgentId = request.Agent.Id, ProviderId = request.Provider.Id, Model = request.Provider.DefaultModel, Text = "echo: " + request.Messages[request.Messages.Count - 1].Content });
            }
        }

        private sealed class BlockingEchoAdapter : IAiProviderAdapter
        {
            private readonly Task _release;
            public BlockingEchoAdapter(Task release) { _release = release; }
            public string Kind => "openai-compatible";
            public string DisplayName => "Blocking Echo";
            public bool CanHandle(AiProvider provider) => provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            public async Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken token)
            {
                await _release.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return new AIResponse { AgentId = request.Agent.Id, ProviderId = request.Provider.Id, Model = request.Provider.DefaultModel, Text = "echo: " + request.Messages[request.Messages.Count - 1].Content };
            }
        }

        private sealed class EmptySecretStore : ISecretStore
        {
            public Task SetAsync(string id, string secret, CancellationToken t = default(CancellationToken)) => Task.CompletedTask;
            public Task<string> GetAsync(string id, CancellationToken t = default(CancellationToken)) => Task.FromResult(string.Empty);
            public Task DeleteAsync(string id, CancellationToken t = default(CancellationToken)) => Task.CompletedTask;
        }
    }
}

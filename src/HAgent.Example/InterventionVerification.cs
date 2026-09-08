using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal static class InterventionVerification
    {
        public static async Task<string> RunAsync()
        {
            await VerifyWorkflowLifecycleAsync().ConfigureAwait(false);
            await VerifyExecutionApprovalAsync().ConfigureAwait(false);
            await VerifyExecutionPauseResumeCancelAsync().ConfigureAwait(false);
            await VerifyDurablePersistenceAsync().ConfigureAwait(false);
            return "Phase 0.959 intervention verification succeeded.";
        }

        private static async Task VerifyWorkflowLifecycleAsync()
        {
            var workflow = new InMemoryAiInterventionWorkflow();
            var request = await workflow.CreateAsync(
                AiInterventionRequestKind.Deferral,
                AiInterventionTargetKind.ConsequentialAction,
                AiInterventionAction.Defer,
                "action.defer",
                "action",
                "action-1",
                "correlation-1",
                "host-1",
                "agent-1",
                "runtime-1",
                "execution-1",
                string.Empty,
                "deferred",
                new AgentIdentityContext(userId: "requester"));

            if (request.Status != AiInterventionRequestStatus.Pending)
                throw new InvalidOperationException("New intervention request did not start pending.");

            await workflow.ResolveAsync(
                request.RequestId,
                AiInterventionRequestStatus.Rejected,
                new AgentIdentityContext(userId: "reviewer"),
                "rejected").ConfigureAwait(false);

            var resolved = await workflow.GetAsync(request.RequestId).ConfigureAwait(false);
            if (resolved.Status != AiInterventionRequestStatus.Rejected || resolved.ResponderIdentity.UserId != "reviewer")
                throw new InvalidOperationException("Deferral rejection lifecycle was not preserved.");

            try
            {
                await workflow.ResolveAsync(
                    request.RequestId,
                    AiInterventionRequestStatus.Approved,
                    new AgentIdentityContext(userId: "late"),
                    "late").ConfigureAwait(false);
                throw new InvalidOperationException("A terminal request accepted a second resolution.");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.IndexOf("no longer pending", StringComparison.OrdinalIgnoreCase) < 0)
                    throw;
            }
        }

        private static async Task VerifyExecutionApprovalAsync()
        {
            var environment = await CreateClientAsync(false).ConfigureAwait(false);
            var policy = new AiPolicySet { Version = "example-approval-policy" };
            var rule = new AiPolicyRule
            {
                Id = "example-approval-rule",
                Name = "Require approval",
                Scope = AiPolicyScopeKind.System,
                Priority = 100,
                Outcome = AiPolicyOutcome.RequireApproval,
                Reason = "Example verification requires an explicit approval."
            };
            rule.Operations.Add("model.invoke");
            rule.ResourceTypes.Add("execution-target");

            environment = await CreateClientAsync(false, policy).ConfigureAwait(false);
            var task = environment.Client.ExecuteAsync(environment.Agent.Id, "approval");
            AiInterventionRequest request = null;
            for (var i = 0; i < 100 && request == null; i++)
            {
                var pending = await environment.Client.GetPendingInterventionRequestsAsync().ConfigureAwait(false);
                if (pending.Count > 0) request = pending[0];
                else await Task.Delay(10).ConfigureAwait(false);
            }

            if (request == null || request.TargetKind != AiInterventionTargetKind.Execution || request.RequestedAction != AiInterventionAction.Approve)
                throw new InvalidOperationException("Execution policy approval did not create the canonical intervention request.");

            await environment.Client.ResolveInterventionRequestAsync(
                request.RequestId,
                AiInterventionRequestStatus.Approved,
                new AgentIdentityContext(userId: "example-approver"),
                "Approved by Example verification.").ConfigureAwait(false);

            var execution = await task.ConfigureAwait(false);
            if (execution.State != AgentExecutionState.Succeeded || execution.Response == null)
                throw new InvalidOperationException("Approved execution did not complete successfully.");
            var completed = await environment.Client.GetInterventionRequestAsync(request.RequestId).ConfigureAwait(false);
            if (completed.Status != AiInterventionRequestStatus.Completed)
                throw new InvalidOperationException("Approved execution intervention did not reach Completed after runtime application.");
        }

        private static async Task VerifyExecutionPauseResumeCancelAsync()
        {
            var environment = await CreateClientAsync(true).ConfigureAwait(false);
            var executionReady = new TaskCompletionSource<AgentExecution>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<AgentExecutionEventArgs> handler = (s, e) =>
            {
                if (e.Execution.State == AgentExecutionState.Running) executionReady.TrySetResult(e.Execution);
            };
            environment.Client.ExecutionChanged += handler;
            try
            {
                var task = environment.Client.ExecuteAsync(environment.Agent.Id, "pause-resume");
                var execution = await executionReady.Task.ConfigureAwait(false);

                var pause = await CreateExecutionActionAsync(environment, execution, AiInterventionAction.Pause, "pause").ConfigureAwait(false);
                var pauseResult = await environment.Client.ApplyInterventionRequestAsync(pause.RequestId, environment.Identity, "pause applied").ConfigureAwait(false);
                if (!pauseResult.Applied || execution.State != AgentExecutionState.Paused)
                    throw new InvalidOperationException("Execution pause was not applied.");

                var staleResume = await CreateExecutionActionAsync(environment, execution, AiInterventionAction.Resume, "stale resume", execution.ControlRevision - 1).ConfigureAwait(false);
                var staleResult = await environment.Client.ApplyInterventionRequestAsync(staleResume.RequestId, environment.Identity, "stale").ConfigureAwait(false);
                if (!staleResult.IsStale)
                    throw new InvalidOperationException("A stale execution intervention was accepted.");

                await environment.Client.ResolveInterventionRequestAsync(staleResume.RequestId, AiInterventionRequestStatus.Rejected, environment.Identity, "stale").ConfigureAwait(false);

                var resume = await CreateExecutionActionAsync(environment, execution, AiInterventionAction.Resume, "resume").ConfigureAwait(false);
                var resumeResult = await environment.Client.ApplyInterventionRequestAsync(resume.RequestId, environment.Identity, "resume applied").ConfigureAwait(false);
                if (!resumeResult.Applied || execution.State != AgentExecutionState.Running)
                    throw new InvalidOperationException("Execution resume was not applied.");

                environment.Release.SetResult(true);
                var completed = await task.ConfigureAwait(false);
                if (completed.State != AgentExecutionState.Succeeded)
                    throw new InvalidOperationException("Execution did not recover after pause/resume.");
            }
            finally
            {
                environment.Client.ExecutionChanged -= handler;
                if (!environment.Release.Task.IsCompleted) environment.Release.TrySetResult(true);
            }

            var cancelEnvironment = await CreateClientAsync(true).ConfigureAwait(false);
            var cancelReady = new TaskCompletionSource<AgentExecution>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<AgentExecutionEventArgs> cancelHandler = (s, e) =>
            {
                if (e.Execution.State == AgentExecutionState.Running) cancelReady.TrySetResult(e.Execution);
            };
            cancelEnvironment.Client.ExecutionChanged += cancelHandler;
            try
            {
                var task = cancelEnvironment.Client.ExecuteAsync(cancelEnvironment.Agent.Id, "cancel");
                var execution = await cancelReady.Task.ConfigureAwait(false);
                var cancel = await CreateExecutionActionAsync(cancelEnvironment, execution, AiInterventionAction.Cancel, "cancel").ConfigureAwait(false);
                var result = await cancelEnvironment.Client.ApplyInterventionRequestAsync(cancel.RequestId, cancelEnvironment.Identity, "cancel applied").ConfigureAwait(false);
                if (!result.Applied || execution.State != AgentExecutionState.Cancelled)
                    throw new InvalidOperationException("Execution cancellation was not applied.");

                try { await task.ConfigureAwait(false); throw new InvalidOperationException("Cancelled execution unexpectedly completed successfully."); }
                catch (OperationCanceledException) { }
            }
            finally
            {
                cancelEnvironment.Client.ExecutionChanged -= cancelHandler;
                if (!cancelEnvironment.Release.Task.IsCompleted) cancelEnvironment.Release.TrySetResult(true);
            }
        }

        private static async Task<AiInterventionRequest> CreateExecutionActionAsync(
            VerificationEnvironment environment,
            AgentExecution execution,
            AiInterventionAction action,
            string reason,
            long? targetRevision = null)
        {
            var request = await environment.Client.InterventionWorkflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                action,
                "execution." + action.ToString().ToLowerInvariant(),
                "execution",
                execution.Id,
                execution.CorrelationId,
                string.Empty,
                environment.Agent.Id,
                string.Empty,
                execution.Id,
                string.Empty,
                reason,
                environment.Identity,
                CancellationToken.None,
                targetRevision ?? execution.ControlRevision).ConfigureAwait(false);

            await environment.Client.ResolveInterventionRequestAsync(
                request.RequestId,
                AiInterventionRequestStatus.Approved,
                environment.Identity,
                "Approved by Example verification.").ConfigureAwait(false);
            return request;
        }

        private static async Task VerifyDurablePersistenceAsync()
        {
            var path = Path.Combine(Path.GetTempPath(), "hagent-intervention-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                using (var store = new HAgent.Storage.File.FileAiInterventionStore(path))
                {
                    var workflow = new StoreBackedAiInterventionWorkflow(store);
                    var request = await workflow.CreateAsync(
                        AiInterventionRequestKind.Intervention,
                        AiInterventionTargetKind.LearningCandidate,
                        AiInterventionAction.Inspect,
                        "learning.inspect",
                        "learning-candidate",
                        "candidate-1",
                        "correlation-1",
                        "host-1",
                        "agent-1",
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "inspect",
                        new AgentIdentityContext(userId: "requester")).ConfigureAwait(false);

                    using (var reopenedStore = new HAgent.Storage.File.FileAiInterventionStore(path))
                    {
                        var reopened = await reopenedStore.GetAsync(request.RequestId).ConfigureAwait(false);
                        if (reopened == null || reopened.RequestId != request.RequestId || reopened.TargetKind != AiInterventionTargetKind.LearningCandidate)
                            throw new InvalidOperationException("Durable intervention persistence round-trip failed.");
                    }
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private static async Task<VerificationEnvironment> CreateClientAsync(bool blocking, AiPolicySet policy = null)
        {
            var provider = new AiProvider
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Example Provider",
                Kind = "openai-compatible",
                DefaultModel = "example-model"
            };
            var agent = new AiAgent
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Example Agent",
                ProviderId = provider.Id,
                Model = provider.DefaultModel,
                Enabled = true
            };
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(provider).ConfigureAwait(false);
            await store.SaveAgentAsync(agent).ConfigureAwait(false);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IAiProviderAdapter adapter = blocking ? new BlockingAdapter(release.Task) : new ImmediateAdapter();
            var client = new HAgentClient(store, new EmptySecretStore(), new[] { adapter }, null, null, null, null, null, null, null, policy == null ? new DefaultAiPolicyEngine(new AiPolicySet()) : new DefaultAiPolicyEngine(policy));
            return new VerificationEnvironment(client, agent, release, new AgentIdentityContext(userId: "example-operator"));
        }

        private sealed class VerificationEnvironment
        {
            public VerificationEnvironment(HAgentClient client, AiAgent agent, TaskCompletionSource<bool> release, AgentIdentityContext identity)
            {
                Client = client;
                Agent = agent;
                Release = release;
                Identity = identity;
            }
            public HAgentClient Client { get; private set; }
            public AiAgent Agent { get; private set; }
            public TaskCompletionSource<bool> Release { get; private set; }
            public AgentIdentityContext Identity { get; private set; }
        }

        private sealed class ImmediateAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "openai-compatible"; } }
            public string DisplayName { get { return "Example Immediate"; } }
            public bool CanHandle(AiProvider provider) { return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase); }
            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken token)
            {
                return Task.FromResult(new AIResponse { AgentId = request.Agent.Id, ProviderId = request.Provider.Id, Model = request.Provider.DefaultModel, Text = "echo: " + request.Messages[request.Messages.Count - 1].Content });
            }
        }

        private sealed class BlockingAdapter : IAiProviderAdapter
        {
            private readonly Task _release;
            public BlockingAdapter(Task release) { _release = release; }
            public string Kind { get { return "openai-compatible"; } }
            public string DisplayName { get { return "Example Blocking"; } }
            public bool CanHandle(AiProvider provider) { return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase); }
            public async Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken token)
            {
                await _release.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return new AIResponse { AgentId = request.Agent.Id, ProviderId = request.Provider.Id, Model = request.Provider.DefaultModel, Text = "echo: " + request.Messages[request.Messages.Count - 1].Content };
            }
        }

        private sealed class EmptySecretStore : ISecretStore
        {
            public Task SetAsync(string id, string secret, CancellationToken t = default(CancellationToken)) { return Task.CompletedTask; }
            public Task<string> GetAsync(string id, CancellationToken t = default(CancellationToken)) { return Task.FromResult(string.Empty); }
            public Task DeleteAsync(string id, CancellationToken t = default(CancellationToken)) { return Task.CompletedTask; }
        }
    }
}

using System;
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
                "RUNTIME LIFECYCLE",
                "Run runtime lifecycle test",
                "Verifies the complete 0.958 lifecycle boundary: suspend, recovery, resume, retirement, shutdown, admission rejection, stale-result invalidation, and runtime-state persistence.",
                "Lifecycle transitions should advance the lifecycle revision, non-active states should reject new work, a result spanning a lifecycle transition should be stale, persisted lifecycle state should restore, and shutdown should cancel outstanding work.",
                "Runtime lifecycle verification.",
                TestRuntimeLifecycleAsync,
                "Lifecycle states and revision authority",
                "Uses only local adapters and the file runtime-state store; no external provider is contacted.");
        }

        private async Task TestRuntimeLifecycleAsync(string message)
        {
            var store = new InMemoryAiStore();
            var profile = new AiAgent
            {
                Id = "runtime-lifecycle-profile-42",
                Name = "Runtime Lifecycle Test Profile",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            };
            var provider = new AiProvider
            {
                Id = "runtime-lifecycle-provider-42",
                Name = "Runtime Lifecycle Provider",
                Kind = "RuntimeLifecycleTest",
                BaseUrl = "https://runtime-lifecycle.test/v1",
                DefaultModel = "runtime-lifecycle-model-42",
                Enabled = true
            };

            await store.SaveProviderAsync(provider).ConfigureAwait(true);
            await store.SaveAgentAsync(profile).ConfigureAwait(true);

            var adapter = new RuntimeLifecycleTestAdapter();
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Persistent);
            var options = new AgentExecutionOptions
            {
                Timeout = TimeSpan.FromSeconds(5),
                MaxProviderAttempts = 1,
                MaxRetriesPerProvider = 0
            };

            if (instance.State != AgentRuntimeInstanceState.Active || instance.CurrentLifecycleRevision != 0)
                throw new InvalidOperationException("Runtime did not start in Active state with lifecycle revision 0.");

            instance.Suspend();
            if (instance.State != AgentRuntimeInstanceState.Suspended || instance.CurrentLifecycleRevision != 1)
                throw new InvalidOperationException("Suspend transition did not advance the lifecycle revision.");
            await ExpectRejectedAsync(client, instance, "during-suspend", options).ConfigureAwait(true);

            instance.BeginRecovery();
            if (instance.State != AgentRuntimeInstanceState.Recovering || instance.CurrentLifecycleRevision != 2)
                throw new InvalidOperationException("Recovery transition did not advance the lifecycle revision.");
            await ExpectRejectedAsync(client, instance, "during-recovery", options).ConfigureAwait(true);

            instance.Resume();
            if (instance.State != AgentRuntimeInstanceState.Active || instance.CurrentLifecycleRevision != 3)
                throw new InvalidOperationException("Resume transition did not advance the lifecycle revision.");

            var executionTask = client.ExecuteAsync(
                instance,
                string.IsNullOrWhiteSpace(message) ? "runtime-lifecycle" : message,
                options,
                CancellationToken.None);
            await adapter.Started.Task.ConfigureAwait(true);
            var executionLifecycleRevision = instance.CurrentLifecycleRevision;

            instance.Suspend();
            instance.Resume();
            adapter.Release();
            var execution = await executionTask.ConfigureAwait(true);
            if (execution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Lifecycle example execution did not complete successfully.");
            if (execution.RuntimeLifecycleRevision != executionLifecycleRevision)
                throw new InvalidOperationException("Execution did not capture the lifecycle revision at admission.");
            if (instance.IsExecutionCurrent(execution))
                throw new InvalidOperationException("Execution remained authoritative after a lifecycle transition.");

            instance.Retire();
            if (instance.State != AgentRuntimeInstanceState.Retired)
                throw new InvalidOperationException("Retire transition did not enter Retired state.");
            await ExpectRejectedAsync(client, instance, "after-retire", options).ConfigureAwait(true);

            var record = AgentRuntimeStateRecord.FromInstance(
                instance,
                "example-host",
                "example-user",
                "example-workspace",
                "example-session");
            var path = Path.Combine(Path.GetTempPath(), "hagent-example-runtime-lifecycle-" + Guid.NewGuid().ToString("N") + ".jsonl");
            try
            {
                using (var runtimeStateStore = new FileAgentRuntimeStateStore(path))
                {
                    await runtimeStateStore.SaveAsync(record).ConfigureAwait(true);
                    var restoredRecord = await runtimeStateStore.GetAsync(instance.InstanceId).ConfigureAwait(true);
                    if (restoredRecord == null ||
                        restoredRecord.State != AgentRuntimeInstanceState.Retired ||
                        restoredRecord.LifecycleRevision != instance.CurrentLifecycleRevision)
                    {
                        throw new InvalidOperationException("Persisted runtime state did not preserve lifecycle state and revision.");
                    }

                    var restored = AgentRuntimeInstance.Restore(profile, restoredRecord);
                    if (restored.State != instance.State || restored.CurrentLifecycleRevision != instance.CurrentLifecycleRevision)
                        throw new InvalidOperationException("Restored runtime did not preserve lifecycle state and revision.");
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }

            var shutdownInstance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            adapter.Reset();
            var shutdownTask = client.ExecuteAsync(shutdownInstance, "shutdown", options, CancellationToken.None);
            await adapter.Started.Task.ConfigureAwait(true);
            shutdownInstance.Shutdown();

            var cancelled = false;
            try
            {
                await shutdownTask.ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            if (!cancelled)
                throw new InvalidOperationException("Shutdown did not cancel the outstanding execution.");
            if (shutdownInstance.State != AgentRuntimeInstanceState.Shutdown)
                throw new InvalidOperationException("Shutdown did not enter Shutdown state.");
            await ExpectRejectedAsync(client, shutdownInstance, "after-shutdown", options).ConfigureAwait(true);

            Write("RUNTIME LIFECYCLE",
                "Contract test succeeded." + Environment.NewLine +
                "Runtime instance: " + instance.InstanceId + Environment.NewLine +
                "Lifecycle transitions: Active -> Suspended -> Recovering -> Active -> Suspended -> Active -> Retired" + Environment.NewLine +
                "Lifecycle revision after retirement: " + instance.CurrentLifecycleRevision + Environment.NewLine +
                "Existing execution after transition: stale" + Environment.NewLine +
                "Non-active execution admission: rejected" + Environment.NewLine +
                "Persisted state restored: yes" + Environment.NewLine +
                "Shutdown cancelled outstanding execution: yes" + Environment.NewLine +
                "Shutdown state: " + shutdownInstance.State);
        }

        private static async Task ExpectRejectedAsync(HAgentClient client, AgentRuntimeInstance instance, string message, AgentExecutionOptions options)
        {
            var rejected = false;
            try
            {
                await client.ExecuteAsync(instance, message, options, CancellationToken.None).ConfigureAwait(true);
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }

            if (!rejected)
                throw new InvalidOperationException("Runtime lifecycle state did not reject new execution: " + instance.State);
        }

        private sealed class NullSecretStore : ISecretStore
        {
            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(string.Empty);
            }

            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }

            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
        }

        private sealed class RuntimeLifecycleTestAdapter : IAiProviderAdapter
        {
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            private TaskCompletionSource<bool> _release = new TaskCompletionSource<bool>();

            public string Kind { get { return "RuntimeLifecycleTest"; } }
            public string DisplayName { get { return "Runtime Lifecycle Test Adapter"; } }

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
                await _release.Task.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return new AIResponse
                {
                    Text = "RUNTIME-LIFECYCLE-OK",
                    ProviderId = request.Provider == null ? string.Empty : request.Provider.Id
                };
            }

            public void Release()
            {
                _release.TrySetResult(true);
            }

            public void Reset()
            {
                _release = new TaskCompletionSource<bool>();
                var replacement = new TaskCompletionSource<bool>();
                while (!Started.Task.IsCompleted)
                {
                    if (Started.TrySetResult(false)) break;
                }
            }
        }
    }
}

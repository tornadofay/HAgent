using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;
using Xunit;

namespace HAgent.Tests
{
    public sealed class RuntimeLifecycleTests
    {
        [Fact]
        public void RuntimeLifecycle_ValidTransitionsIncrementLifecycleRevision()
        {
            var instance = CreateInstance();
            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(0L, instance.CurrentLifecycleRevision);

            instance.Suspend();
            Assert.Equal(AgentRuntimeInstanceState.Suspended, instance.State);
            Assert.Equal(1L, instance.CurrentLifecycleRevision);

            instance.BeginRecovery();
            Assert.Equal(AgentRuntimeInstanceState.Recovering, instance.State);
            Assert.Equal(2L, instance.CurrentLifecycleRevision);

            instance.Resume();
            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(3L, instance.CurrentLifecycleRevision);

            instance.Retire();
            Assert.Equal(AgentRuntimeInstanceState.Retired, instance.State);
            Assert.Equal(4L, instance.CurrentLifecycleRevision);

            instance.Shutdown();
            Assert.Equal(AgentRuntimeInstanceState.Shutdown, instance.State);
            Assert.Equal(5L, instance.CurrentLifecycleRevision);
        }

        [Fact]
        public void RuntimeLifecycle_InvalidTransitionsDoNotAdvanceRevision()
        {
            var instance = CreateInstance();

            AssertInvalidTransition(() => instance.Resume());
            AssertInvalidTransition(() => instance.Retire());
            Assert.Equal(0L, instance.CurrentLifecycleRevision);

            instance.Suspend();
            AssertInvalidTransition(() => instance.Suspend());
            Assert.Equal(1L, instance.CurrentLifecycleRevision);

            instance.BeginRecovery();
            AssertInvalidTransition(() => instance.BeginRecovery());
            Assert.Equal(2L, instance.CurrentLifecycleRevision);

            instance.Resume();
            instance.Retire();
            AssertInvalidTransition(() => instance.Resume());
            Assert.Equal(4L, instance.CurrentLifecycleRevision);

            instance.Shutdown();
            AssertInvalidTransition(() => instance.Shutdown());
            AssertInvalidTransition(() => instance.Resume());
            AssertInvalidTransition(() => instance.Retire());
            Assert.Equal(5L, instance.CurrentLifecycleRevision);
        }

        [Fact]
        public async Task RuntimeLifecycle_NonActiveStatesRejectNewExecution()
        {
            var store = new InMemoryAiStore();
            var client = await CreateClientAsync(store, new NoOpProviderAdapter()).ConfigureAwait(false);
            var instance = CreateInstance();

            instance.Suspend();
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(instance, "suspended")).ConfigureAwait(false);

            instance.BeginRecovery();
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(instance, "recovering")).ConfigureAwait(false);

            instance.Resume();
            instance.Retire();
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(instance, "retired")).ConfigureAwait(false);

            instance.Shutdown();
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(instance, "shutdown")).ConfigureAwait(false);
        }

        [Fact]
        public async Task RuntimeLifecycle_LifecycleTransitionInvalidatesExistingExecutionAuthority()
        {
            var store = new InMemoryAiStore();
            var adapter = new BlockingProviderAdapter();
            var client = await CreateClientAsync(store, adapter).ConfigureAwait(false);
            var instance = CreateInstance();

            var executionTask = client.ExecuteAsync(instance, "lifecycle-stale");
            await adapter.Started.Task.ConfigureAwait(false);

            instance.Suspend();
            instance.Resume();
            adapter.Release();
            var execution = await executionTask.ConfigureAwait(false);

            Assert.Equal(AgentRuntimeInstanceState.Active, instance.State);
            Assert.Equal(AgentExecutionState.Succeeded, execution.State);
            Assert.Equal(0L, execution.RuntimeLifecycleRevision);
            Assert.Equal(2L, instance.CurrentLifecycleRevision);
            Assert.False(instance.IsExecutionCurrent(execution));
        }

        [Fact]
        public void RuntimeLifecycle_RestorePreservesStateAndLifecycleRevision()
        {
            var profile = CreateProfile();
            var source = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Persistent);
            source.Suspend();
            source.BeginRecovery();
            source.Resume();

            var record = AgentRuntimeStateRecord.FromInstance(
                source,
                "host-1",
                "user-1",
                "workspace-1",
                "session-1");
            var restored = AgentRuntimeInstance.Restore(profile, record);

            Assert.Equal(source.InstanceId, restored.InstanceId);
            Assert.Equal(source.ProfileId, restored.ProfileId);
            Assert.Equal(source.Scope, restored.Scope);
            Assert.Equal(source.State, restored.State);
            Assert.Equal(source.CurrentLifecycleRevision, restored.CurrentLifecycleRevision);
            Assert.Equal(source.CreatedAt, restored.CreatedAt);
        }

        [Fact]
        public async Task RuntimeLifecycle_FilePersistencePreservesLifecycleRevisionAndState()
        {
            var path = Path.Combine(Path.GetTempPath(), "hagent-runtime-lifecycle-" + Guid.NewGuid().ToString("N") + ".jsonl");
            try
            {
                var profile = CreateProfile();
                var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Session);
                instance.Suspend();
                instance.BeginRecovery();

                using (var store = new FileAgentRuntimeStateStore(path))
                {
                    await store.SaveAsync(AgentRuntimeStateRecord.FromInstance(instance)).ConfigureAwait(false);
                    var saved = await store.GetAsync(instance.InstanceId).ConfigureAwait(false);
                    Assert.NotNull(saved);
                    Assert.Equal(instance.State, saved.State);
                    Assert.Equal(instance.CurrentLifecycleRevision, saved.LifecycleRevision);

                    var restored = AgentRuntimeInstance.Restore(profile, saved);
                    Assert.Equal(instance.State, restored.State);
                    Assert.Equal(instance.CurrentLifecycleRevision, restored.CurrentLifecycleRevision);
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private static void AssertInvalidTransition(Action action)
        {
            Assert.Throws<InvalidOperationException>(action);
        }

        private static AgentRuntimeInstance CreateInstance()
        {
            return AgentRuntimeInstance.Create(CreateProfile(), AgentRuntimeScope.Task);
        }

        private static AiAgent CreateProfile()
        {
            return new AiAgent
            {
                Id = "runtime-lifecycle-profile",
                Name = "Runtime Lifecycle Profile",
                Enabled = true,
                CapabilityRequirements = new AiCapabilityRequirements(),
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.TryNextCandidate,
                    CostPolicy = AiCostPolicy.NoRestriction
                }
            };
        }

        private static async Task<HAgentClient> CreateClientAsync(InMemoryAiStore store, IAiProviderAdapter adapter)
        {
            var provider = new AiProvider
            {
                Id = "runtime-lifecycle-provider",
                Name = "Runtime Lifecycle Provider",
                Kind = adapter.Kind,
                DefaultModel = "runtime-lifecycle-model",
                BaseUrl = "https://runtime-lifecycle.test/v1",
                Enabled = true
            };
            await store.SaveProviderAsync(provider).ConfigureAwait(false);
            await store.SaveAgentAsync(CreateProfile()).ConfigureAwait(false);
            return new HAgentClient(store, new NullSecretStore(), new[] { adapter });
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

        private sealed class NoOpProviderAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "RuntimeLifecycleNoOp"; } }
            public string DisplayName { get { return "Runtime Lifecycle No-op"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AIResponse
                {
                    Text = "ok",
                    ProviderId = request.Provider.Id
                });
            }
        }

        private sealed class BlockingProviderAdapter : IAiProviderAdapter
        {
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            private readonly TaskCompletionSource<bool> _release = new TaskCompletionSource<bool>();

            public string Kind { get { return "RuntimeLifecycleBlocking"; } }
            public string DisplayName { get { return "Runtime Lifecycle Blocking"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public async Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                Started.TrySetResult(true);
                await _release.Task.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                return new AIResponse
                {
                    Text = "ok",
                    ProviderId = request.Provider.Id
                };
            }

            public void Release()
            {
                _release.TrySetResult(true);
            }
        }
    }
}

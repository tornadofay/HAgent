using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddRuntimeSingleOwnerSpikeTab()
        {
            AddApiTab(
                "RUNTIME SINGLE OWNER",
                "Run single-owner runtime spike",
                "Validates the one-owner-per-agent state model: asynchronous work may run outside the state owner, state mutations are serialized per agent, and many independent runtime agents can operate concurrently without a shared cognitive bottleneck.",
                "The test preserves event order, rejects a late result from an older revision, cancels in-flight work, prevents mutation after shutdown, and proves 12 independent agents can overlap while each keeps its own serialized state.",
                "Single-owner runtime architecture spike.",
                TestRuntimeSingleOwnerSpikeAsync,
                "One state owner per agent",
                "Example-only spike. It does not modify the production runtime implementation and does not contact an external provider.");
        }

        private async Task TestRuntimeSingleOwnerSpikeAsync(string message)
        {
            var profile = new AiAgent
            {
                Id = "runtime-single-owner-profile-42",
                Name = "Runtime Single Owner Spike Profile"
            };

            var firstInstance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            var secondInstance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);

            using (var first = new SingleOwnerRuntimeProbe(firstInstance))
            using (var second = new SingleOwnerRuntimeProbe(secondInstance))
            {
                await first.EnqueueAsync("event-1", delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                {
                    state.ApplyMutation("event-1");
                    return Task.CompletedTask;
                }).ConfigureAwait(true);

                var firstSnapshot = await first.CaptureSnapshotAsync().ConfigureAwait(true);
                var lateResultRevision = firstSnapshot.Revision;

                var longOperation = first.EnqueueAsync("long-operation", async delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                {
                    state.ApplyMutation("long-start");
                    await Task.Delay(80, cancellationToken).ConfigureAwait(false);
                    state.ApplyMutation("long-end");
                });

                var queuedEvent = first.EnqueueAsync("event-2", delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                {
                    state.ApplyMutation("event-2");
                    return Task.CompletedTask;
                });

                await longOperation.ConfigureAwait(true);
                await queuedEvent.ConfigureAwait(true);

                var lateResult = await Task.Run(async delegate
                {
                    await Task.Delay(120).ConfigureAwait(false);
                    return await first.EnqueueLateResultAsync(lateResultRevision, "late-llm-result").ConfigureAwait(false);
                }).ConfigureAwait(true);

                if (lateResult.Applied)
                    throw new InvalidOperationException("A late result from an older state revision was incorrectly applied.");
                if (!lateResult.Stale)
                    throw new InvalidOperationException("The late result did not report an explicit stale outcome.");

                var firstState = await first.CaptureSnapshotAsync().ConfigureAwait(true);
                if (!firstState.Events.Contains("long-start") ||
                    !firstState.Events.Contains("long-end") ||
                    !firstState.Events.Contains("event-2"))
                    throw new InvalidOperationException("The single-owner loop lost an event during asynchronous work.");

                if (firstState.Events.IndexOf("long-start") >= firstState.Events.IndexOf("long-end") ||
                    firstState.Events.IndexOf("long-end") >= firstState.Events.IndexOf("event-2"))
                    throw new InvalidOperationException("The single-owner loop did not preserve mutation order.");

                var cancellationSource = new CancellationTokenSource();
                var cancelledOperation = first.EnqueueAsync(
                    "cancelled-operation",
                    async delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        state.ApplyMutation("cancelled-operation-completed");
                    },
                    cancellationSource.Token);

                cancellationSource.CancelAfter(25);
                var cancellationObserved = false;
                try
                {
                    await cancelledOperation.ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    cancellationObserved = true;
                }
                finally
                {
                    cancellationSource.Dispose();
                }

                if (!cancellationObserved)
                    throw new InvalidOperationException("The single-owner loop did not honor operation cancellation.");

                var cancelledState = await first.CaptureSnapshotAsync().ConfigureAwait(true);
                if (cancelledState.Events.Contains("cancelled-operation-completed"))
                    throw new InvalidOperationException("Cancelled work mutated state after cancellation.");

                await second.EnqueueAsync("agent-b-event", delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                {
                    state.ApplyMutation("agent-b-event");
                    return Task.CompletedTask;
                }).ConfigureAwait(true);

                var secondState = await second.CaptureSnapshotAsync().ConfigureAwait(true);
                if (!secondState.Events.Contains("agent-b-event"))
                    throw new InvalidOperationException("The independent second agent did not process its own state.");
                if (secondState.Events.Contains("event-2") || secondState.Events.Contains("late-llm-result"))
                    throw new InvalidOperationException("One agent received state belonging to another agent.");

                await TestIndependentAgentConcurrencyAsync(profile).ConfigureAwait(true);

                var beforeShutdown = await first.CaptureSnapshotAsync().ConfigureAwait(true);
                firstInstance.Shutdown();
                first.Shutdown();

                var shutdownRejected = false;
                try
                {
                    await first.EnqueueAsync("after-shutdown", delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                    {
                        state.ApplyMutation("after-shutdown");
                        return Task.CompletedTask;
                    }).ConfigureAwait(true);
                }
                catch (ObjectDisposedException)
                {
                    shutdownRejected = true;
                }
                catch (InvalidOperationException)
                {
                    shutdownRejected = true;
                }

                if (!shutdownRejected)
                    throw new InvalidOperationException("A shutdown agent accepted new state work.");

                var afterShutdown = first.Snapshot;
                if (afterShutdown.Revision != beforeShutdown.Revision)
                    throw new InvalidOperationException("Shutdown unexpectedly changed the Example-owned cognitive revision.");
                if (afterShutdown.Events.Contains("after-shutdown"))
                    throw new InvalidOperationException("State changed after shutdown.");

                Write("RUNTIME SINGLE OWNER",
                    "Spike succeeded." + Environment.NewLine +
                    "Agent 1: " + firstInstance.InstanceId + Environment.NewLine +
                    "Agent 2: " + secondInstance.InstanceId + Environment.NewLine +
                    "One authoritative state owner per agent: yes" + Environment.NewLine +
                    "Concurrent event queued during async work: yes" + Environment.NewLine +
                    "State mutation order preserved: yes" + Environment.NewLine +
                    "Late older-revision result rejected: yes" + Environment.NewLine +
                    "Cancellation honored: yes" + Environment.NewLine +
                    "Post-shutdown mutation rejected: yes" + Environment.NewLine +
                    "Independent agent isolation: yes" + Environment.NewLine +
                    "Independent concurrent agents verified: 12" + Environment.NewLine +
                    "All 12 owner loops overlapped: yes" + Environment.NewLine +
                    "Per-agent follow-up mutation remained serialized: yes" + Environment.NewLine +
                    "Production runtime changed by spike: no");
            }
        }

        private async Task TestIndependentAgentConcurrencyAsync(AiAgent profile)
        {
            const int agentCount = 12;
            var instances = new List<AgentRuntimeInstance>(agentCount);
            var probes = new List<SingleOwnerRuntimeProbe>(agentCount);
            var startedGate = new TaskCompletionSource<object>();
            var workTasks = new List<Task>(agentCount);
            var followUpTasks = new List<Task>(agentCount);
            var startedCount = 0;
            var activeCount = 0;
            var maximumConcurrent = 0;

            try
            {
                for (var i = 0; i < agentCount; i++)
                {
                    var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
                    instances.Add(instance);
                    probes.Add(new SingleOwnerRuntimeProbe(instance));
                }

                foreach (var probe in probes)
                {
                    workTasks.Add(probe.EnqueueAsync(
                        "concurrent-work",
                        async delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                        {
                            var active = Interlocked.Increment(ref activeCount);
                            UpdateMaximum(ref maximumConcurrent, active);

                            if (Interlocked.Increment(ref startedCount) == agentCount)
                                startedGate.TrySetResult(null);

                            await startedGate.Task.ConfigureAwait(false);
                            state.ApplyMutation("concurrent-work");
                            await Task.Delay(75, cancellationToken).ConfigureAwait(false);
                            Interlocked.Decrement(ref activeCount);
                        }));
                }

                await Task.WhenAll(workTasks).ConfigureAwait(true);

                if (startedCount != agentCount)
                    throw new InvalidOperationException("Not every independent runtime agent reached the concurrent-work barrier.");
                if (maximumConcurrent != agentCount)
                    throw new InvalidOperationException("Independent runtime agents did not all overlap; observed maximum concurrency was " + maximumConcurrent + " of " + agentCount + ".");

                foreach (var probe in probes)
                    followUpTasks.Add(probe.EnqueueAsync("follow-up", delegate(SingleOwnerRuntimeState state, CancellationToken cancellationToken)
                    {
                        state.ApplyMutation("follow-up");
                        return Task.CompletedTask;
                    }));

                await Task.WhenAll(followUpTasks).ConfigureAwait(true);

                foreach (var probe in probes)
                {
                    var snapshot = await probe.CaptureSnapshotAsync().ConfigureAwait(true);
                    if (snapshot.Revision != 2)
                        throw new InvalidOperationException("An independent runtime agent did not preserve its own serialized mutation count.");
                    if (snapshot.Events.Count != 2 ||
                        !string.Equals(snapshot.Events[0], "concurrent-work", StringComparison.Ordinal) ||
                        !string.Equals(snapshot.Events[1], "follow-up", StringComparison.Ordinal))
                        throw new InvalidOperationException("An independent runtime agent did not preserve per-agent mutation order.");
                }
            }
            finally
            {
                foreach (var probe in probes)
                    probe.Dispose();

                foreach (var instance in instances)
                {
                    if (instance.State != AgentRuntimeInstanceState.Shutdown)
                        instance.Shutdown();
                }
            }
        }

        private static void UpdateMaximum(ref int target, int candidate)
        {
            while (true)
            {
                var current = Volatile.Read(ref target);
                if (candidate <= current)
                    return;
                if (Interlocked.CompareExchange(ref target, candidate, current) == current)
                    return;
            }
        }

        private sealed class SingleOwnerRuntimeProbe : IDisposable
        {
            private readonly AgentRuntimeInstance _instance;
            private readonly object _queueSync = new object();
            private readonly Queue<SingleOwnerOperation> _queue = new Queue<SingleOwnerOperation>();
            private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
            private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
            private readonly Task _ownerTask;
            private readonly SingleOwnerRuntimeState _state = new SingleOwnerRuntimeState();
            private bool _stopped;

            public SingleOwnerRuntimeProbe(AgentRuntimeInstance instance)
            {
                if (instance == null) throw new ArgumentNullException(nameof(instance));
                _instance = instance;
                _ownerTask = Task.Run(RunOwnerLoopAsync);
            }

            public SingleOwnerRuntimeSnapshot Snapshot
            {
                get
                {
                    return _state.CreateSnapshot();
                }
            }

            public Task EnqueueAsync(
                string name,
                Func<SingleOwnerRuntimeState, CancellationToken, Task> operation,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return EnqueueCoreAsync(name, operation, cancellationToken);
            }

            public Task<SingleOwnerRuntimeSnapshot> CaptureSnapshotAsync()
            {
                return EnqueueSnapshotAsync();
            }

            public Task<LateResultOutcome> EnqueueLateResultAsync(long capturedRevision, string name)
            {
                return EnqueueLateResultCoreAsync(capturedRevision, name);
            }

            private Task EnqueueCoreAsync(
                string name,
                Func<SingleOwnerRuntimeState, CancellationToken, Task> operation,
                CancellationToken cancellationToken)
            {
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                var completion = new TaskCompletionSource<object>();
                var linked = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token, cancellationToken);
                lock (_queueSync)
                {
                    if (_stopped || _instance.State == AgentRuntimeInstanceState.Shutdown)
                    {
                        linked.Dispose();
                        completion.SetException(new InvalidOperationException("Single-owner runtime probe is stopped."));
                        return completion.Task;
                    }

                    _queue.Enqueue(new SingleOwnerOperation(name, operation, linked, completion));
                    _signal.Release();
                }

                return completion.Task;
            }

            private Task<SingleOwnerRuntimeSnapshot> EnqueueSnapshotAsync()
            {
                var completion = new TaskCompletionSource<SingleOwnerRuntimeSnapshot>();
                var linked = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
                lock (_queueSync)
                {
                    if (_stopped || _instance.State == AgentRuntimeInstanceState.Shutdown)
                    {
                        linked.Dispose();
                        completion.SetException(new InvalidOperationException("Single-owner runtime probe is stopped."));
                        return completion.Task;
                    }

                    _queue.Enqueue(SingleOwnerOperation.CreateSnapshot(linked, completion));
                    _signal.Release();
                }

                return completion.Task;
            }

            private Task<LateResultOutcome> EnqueueLateResultCoreAsync(long capturedRevision, string name)
            {
                var completion = new TaskCompletionSource<LateResultOutcome>();
                var linked = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
                lock (_queueSync)
                {
                    if (_stopped || _instance.State == AgentRuntimeInstanceState.Shutdown)
                    {
                        linked.Dispose();
                        completion.SetException(new InvalidOperationException("Single-owner runtime probe is stopped."));
                        return completion.Task;
                    }

                    _queue.Enqueue(SingleOwnerOperation.CreateLateResult(capturedRevision, name, linked, completion));
                    _signal.Release();
                }

                return completion.Task;
            }

            private async Task RunOwnerLoopAsync()
            {
                try
                {
                    while (!_shutdown.IsCancellationRequested)
                    {
                        await _signal.WaitAsync(_shutdown.Token).ConfigureAwait(false);
                        SingleOwnerOperation operation = null;
                        lock (_queueSync)
                        {
                            if (_queue.Count > 0)
                                operation = _queue.Dequeue();
                        }

                        if (operation == null)
                            continue;

                        await operation.ExecuteAsync(_state).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Shutdown is expected to stop the owner loop.
                }
            }

            public void Shutdown()
            {
                lock (_queueSync)
                {
                    if (_stopped)
                        return;
                    _stopped = true;
                    _shutdown.Cancel();
                }
            }

            public void Dispose()
            {
                Shutdown();
                try { _ownerTask.Wait(250); } catch { }
                _shutdown.Dispose();
                _signal.Dispose();
            }
        }

        private sealed class SingleOwnerOperation
        {
            private readonly string _name;
            private readonly Func<SingleOwnerRuntimeState, CancellationToken, Task> _operation;
            private readonly CancellationTokenSource _cancellation;
            private readonly TaskCompletionSource<object> _completion;
            private readonly bool _captureSnapshot;
            private readonly bool _lateResult;
            private readonly long _capturedRevision;
            private readonly TaskCompletionSource<SingleOwnerRuntimeSnapshot> _snapshotCompletion;
            private readonly TaskCompletionSource<LateResultOutcome> _lateResultCompletion;

            public SingleOwnerOperation(
                string name,
                Func<SingleOwnerRuntimeState, CancellationToken, Task> operation,
                CancellationTokenSource cancellation,
                TaskCompletionSource<object> completion)
            {
                _name = name;
                _operation = operation;
                _cancellation = cancellation;
                _completion = completion;
            }

            private SingleOwnerOperation(
                CancellationTokenSource cancellation,
                TaskCompletionSource<SingleOwnerRuntimeSnapshot> completion)
            {
                _cancellation = cancellation;
                _captureSnapshot = true;
                _snapshotCompletion = completion;
            }

            private SingleOwnerOperation(
                long capturedRevision,
                string name,
                CancellationTokenSource cancellation,
                TaskCompletionSource<LateResultOutcome> completion)
            {
                _capturedRevision = capturedRevision;
                _name = name;
                _cancellation = cancellation;
                _lateResult = true;
                _lateResultCompletion = completion;
            }

            public static SingleOwnerOperation CreateSnapshot(
                CancellationTokenSource cancellation,
                TaskCompletionSource<SingleOwnerRuntimeSnapshot> completion)
            {
                return new SingleOwnerOperation(cancellation, completion);
            }

            public static SingleOwnerOperation CreateLateResult(
                long capturedRevision,
                string name,
                CancellationTokenSource cancellation,
                TaskCompletionSource<LateResultOutcome> completion)
            {
                return new SingleOwnerOperation(capturedRevision, name, cancellation, completion);
            }

            public async Task ExecuteAsync(SingleOwnerRuntimeState state)
            {
                try
                {
                    if (_captureSnapshot)
                    {
                        _snapshotCompletion.SetResult(state.CreateSnapshot());
                        return;
                    }

                    if (_lateResult)
                    {
                        if (state.Revision != _capturedRevision)
                        {
                            state.MarkStaleResult(_name);
                            _lateResultCompletion.SetResult(new LateResultOutcome(false, true));
                        }
                        else
                        {
                            state.ApplyMutation(_name);
                            _lateResultCompletion.SetResult(new LateResultOutcome(true, false));
                        }
                        return;
                    }

                    await _operation(state, _cancellation.Token).ConfigureAwait(false);
                    _completion.SetResult(null);
                }
                catch (OperationCanceledException ex)
                {
                    _completion.SetException(ex);
                    if (_snapshotCompletion != null) _snapshotCompletion.SetException(ex);
                    if (_lateResultCompletion != null) _lateResultCompletion.SetException(ex);
                }
                catch (Exception ex)
                {
                    _completion.SetException(ex);
                    if (_snapshotCompletion != null) _snapshotCompletion.SetException(ex);
                    if (_lateResultCompletion != null) _lateResultCompletion.SetException(ex);
                }
                finally
                {
                    _cancellation.Dispose();
                }
            }
        }

        private sealed class SingleOwnerRuntimeState
        {
            private readonly List<string> _events = new List<string>();

            public long Revision { get; private set; }

            public void ApplyMutation(string name)
            {
                Revision++;
                _events.Add(name);
            }

            public void MarkStaleResult(string name)
            {
                _events.Add("stale-rejected:" + name);
            }

            public SingleOwnerRuntimeSnapshot CreateSnapshot()
            {
                return new SingleOwnerRuntimeSnapshot(Revision, new List<string>(_events));
            }
        }

        private sealed class SingleOwnerRuntimeSnapshot
        {
            public SingleOwnerRuntimeSnapshot(long revision, IList<string> events)
            {
                Revision = revision;
                Events = events;
            }

            public long Revision { get; private set; }
            public IList<string> Events { get; private set; }
        }

        private sealed class LateResultOutcome
        {
            public LateResultOutcome(bool applied, bool stale)
            {
                Applied = applied;
                Stale = stale;
            }

            public bool Applied { get; private set; }
            public bool Stale { get; private set; }
        }
    }
}

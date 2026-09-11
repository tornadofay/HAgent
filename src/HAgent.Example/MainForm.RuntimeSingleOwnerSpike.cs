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
                "Validates the proposed one-owner-per-agent state model: asynchronous work may run outside the state owner, but state mutations are serialized through one owner queue.",
                "The test should preserve event order, reject a late result from an older revision, cancel in-flight work, prevent mutation after shutdown, and keep a second agent independent.",
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
                catch (InvalidOperationException)
                {
                    shutdownRejected = true;
                }
                catch (ObjectDisposedException)
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
                    "Production runtime changed by spike: no");
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

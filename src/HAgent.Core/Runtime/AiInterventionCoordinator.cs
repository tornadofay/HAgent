using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    internal interface IInterventionControllableRuntime
    {
        AiInterventionCoordinator InterventionCoordinator { get; }
    }

    /// <summary>
    /// Coordinates human intervention with the execution boundary. It owns only
    /// intervention/control state; execution itself remains owned by the runtime.
    /// </summary>
    public sealed class AiInterventionCoordinator
    {
        private readonly IAiInterventionWorkflow _workflow;
        private readonly ConcurrentDictionary<string, ExecutionControl> _executionControls =
            new ConcurrentDictionary<string, ExecutionControl>(StringComparer.OrdinalIgnoreCase);

        public AiInterventionCoordinator(IAiInterventionWorkflow workflow)
        {
            _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        }

        public IAiInterventionWorkflow Workflow { get { return _workflow; } }

        internal void RegisterExecution(AgentExecution execution)
        {
            if (execution == null) throw new ArgumentNullException(nameof(execution));
            var control = new ExecutionControl(execution);
            if (!_executionControls.TryAdd(execution.Id, control))
            {
                control.Dispose();
                throw new InvalidOperationException("An execution control already exists for execution: " + execution.Id);
            }
        }

        internal CancellationToken GetExecutionCancellationToken(string executionId)
        {
            ExecutionControl control;
            if (!_executionControls.TryGetValue(executionId ?? string.Empty, out control))
                return CancellationToken.None;
            return control.CancellationToken;
        }

        internal Task WaitIfPausedAsync(string executionId, CancellationToken cancellationToken)
        {
            ExecutionControl control;
            if (!_executionControls.TryGetValue(executionId ?? string.Empty, out control))
                throw new InvalidOperationException("Execution is not registered for intervention control: " + executionId);
            return control.WaitIfPausedAsync(cancellationToken);
        }

        internal bool IsInterventionCancellationRequested(string executionId)
        {
            ExecutionControl control;
            return _executionControls.TryGetValue(executionId ?? string.Empty, out control) && control.IsCancellationRequested;
        }

        internal void UnregisterExecution(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId)) return;
            ExecutionControl control;
            if (_executionControls.TryRemove(executionId, out control))
                control.Dispose();
        }

        public Task<AiExecutionControlState> GetExecutionControlStateAsync(
            string executionId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(executionId)) throw new ArgumentException("Execution ID is required.", nameof(executionId));
            return Task.FromResult(GetControl(executionId).State);
        }

        internal void PauseExecution(string executionId)
        {
            GetControl(executionId).Pause();
        }

        internal void ResumeExecution(string executionId)
        {
            GetControl(executionId).Resume();
        }

        internal void CancelExecution(string executionId)
        {
            GetControl(executionId).Cancel();
        }

        public async Task<AiInterventionRequest> RequestExecutionInterventionAsync(
            string executionId,
            AiInterventionAction requestedAction,
            AgentIdentityContext requesterIdentity,
            string hostCorrelationId = null,
            string reason = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(executionId)) throw new ArgumentException("Execution ID is required.", nameof(executionId));
            if (requestedAction != AiInterventionAction.Pause &&
                requestedAction != AiInterventionAction.Resume &&
                requestedAction != AiInterventionAction.Cancel)
                throw new ArgumentException("Execution intervention currently supports Pause, Resume, or Cancel.", nameof(requestedAction));

            var control = GetControl(executionId);
            var observed = control.GetSnapshot();
            var operation = requestedAction == AiInterventionAction.Pause
                ? "execution.pause"
                : requestedAction == AiInterventionAction.Resume
                    ? "execution.resume"
                    : "execution.cancel";

            return await _workflow.CreateAsync(
                AiInterventionRequestKind.Intervention,
                AiInterventionTargetKind.Execution,
                requestedAction,
                operation,
                "execution",
                executionId,
                executionId,
                hostCorrelationId,
                string.Empty,
                string.Empty,
                executionId,
                string.Empty,
                reason,
                requesterIdentity,
                cancellationToken,
                observed.State.ToString(),
                observed.Version).ConfigureAwait(false);
        }

        public async Task<AiInterventionRequest> ResolveInterventionAsync(
            string requestId,
            AiInterventionRequestStatus resolution,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = await _workflow.GetAsync(requestId, cancellationToken).ConfigureAwait(false);
            if (request == null)
                throw new InvalidOperationException("Intervention request was not found: " + requestId);

            if (request.TargetKind == AiInterventionTargetKind.Execution &&
                (request.RequestedAction == AiInterventionAction.Pause ||
                 request.RequestedAction == AiInterventionAction.Resume ||
                 request.RequestedAction == AiInterventionAction.Cancel))
            {
                if (resolution == AiInterventionRequestStatus.Approved)
                    return await ResolveExecutionInterventionAsync(request, responderIdentity, reason, cancellationToken).ConfigureAwait(false);

                if (resolution != AiInterventionRequestStatus.Rejected &&
                    resolution != AiInterventionRequestStatus.Cancelled &&
                    resolution != AiInterventionRequestStatus.Expired)
                    throw new ArgumentOutOfRangeException(nameof(resolution));
            }

            return await _workflow.ResolveAsync(
                requestId,
                resolution,
                responderIdentity,
                reason,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<AiInterventionRequest> ResolveExecutionInterventionAsync(
            AiInterventionRequest request,
            AgentIdentityContext responderIdentity,
            string reason,
            CancellationToken cancellationToken)
        {
            ExecutionControl control;
            if (!_executionControls.TryGetValue(request.ExecutionId ?? string.Empty, out control))
            {
                return await _workflow.ResolveAsync(
                    request.RequestId,
                    AiInterventionRequestStatus.Expired,
                    responderIdentity,
                    BuildStaleReason(request, "The target execution is no longer active."),
                    cancellationToken).ConfigureAwait(false);
            }

            await control.ResolutionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var current = await _workflow.GetAsync(request.RequestId, cancellationToken).ConfigureAwait(false);
                if (current == null)
                    throw new InvalidOperationException("Intervention request was not found: " + request.RequestId);
                if (current.Status != AiInterventionRequestStatus.Pending)
                    throw new InvalidOperationException("Intervention request is no longer pending: " + request.RequestId);

                var application = control.TryApply(
                    current.RequestedAction,
                    current.TargetState,
                    current.TargetStateVersion);
                if (!application.Applied)
                {
                    return await _workflow.ResolveAsync(
                        current.RequestId,
                        AiInterventionRequestStatus.Expired,
                        responderIdentity,
                        BuildStaleReason(current, application.Reason),
                        cancellationToken).ConfigureAwait(false);
                }

                var approved = await _workflow.ResolveAsync(
                    current.RequestId,
                    AiInterventionRequestStatus.Approved,
                    responderIdentity,
                    reason,
                    cancellationToken).ConfigureAwait(false);

                if (current.RequestedAction != AiInterventionAction.Cancel && control.IsExecutionTerminal)
                {
                    return await _workflow.ResolveAsync(
                        approved.RequestId,
                        AiInterventionRequestStatus.Expired,
                        responderIdentity,
                        BuildStaleReason(current, "The target execution became terminal while the intervention was being applied."),
                        cancellationToken).ConfigureAwait(false);
                }

                return await _workflow.CompleteAsync(
                    approved.RequestId,
                    responderIdentity,
                    string.IsNullOrWhiteSpace(reason)
                        ? "Execution intervention was applied."
                        : reason,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                control.ResolutionGate.Release();
            }
        }

        private static string BuildStaleReason(AiInterventionRequest request, string detail)
        {
            return string.IsNullOrWhiteSpace(detail)
                ? "The intervention target state changed before the request could be applied."
                : detail;
        }

        private ExecutionControl GetControl(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId)) throw new ArgumentException("Execution ID is required.", nameof(executionId));
            ExecutionControl control;
            if (!_executionControls.TryGetValue(executionId, out control))
                throw new InvalidOperationException("Execution is no longer active: " + executionId);
            return control;
        }

        private sealed class ExecutionControl : IDisposable
        {
            private readonly object _sync = new object();
            private readonly AgentExecution _execution;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private TaskCompletionSource<bool> _resumeSignal;
            private AiExecutionControlState _state;
            private long _version;
            private bool _disposed;

            public ExecutionControl(AgentExecution execution)
            {
                _execution = execution ?? throw new ArgumentNullException(nameof(execution));
                _resumeSignal = CreateCompletedSignal();
                _state = AiExecutionControlState.Running;
                _version = 0;
                ResolutionGate = new SemaphoreSlim(1, 1);
            }

            public SemaphoreSlim ResolutionGate { get; private set; }
            public CancellationToken CancellationToken { get { return _cancellation.Token; } }
            public bool IsCancellationRequested { get { return _cancellation.IsCancellationRequested; } }
            public bool IsExecutionTerminal { get { return _execution.IsCompleted; } }

            public AiExecutionControlState State
            {
                get
                {
                    lock (_sync)
                    {
                        return _state;
                    }
                }
            }

            public ControlSnapshot GetSnapshot()
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return new ControlSnapshot(_state, _version);
                }
            }

            public void Pause()
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    if (_state == AiExecutionControlState.Cancelling || _state == AiExecutionControlState.Cancelled)
                        throw new InvalidOperationException("Execution is already cancelling or cancelled.");
                    if (_state == AiExecutionControlState.Paused)
                        return;
                    if (_execution.IsCompleted)
                        throw new InvalidOperationException("Execution is already terminal.");
                    _state = AiExecutionControlState.Paused;
                    _version++;
                    _resumeSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }

            public void Resume()
            {
                TaskCompletionSource<bool> signal;
                lock (_sync)
                {
                    ThrowIfDisposed();
                    if (_state == AiExecutionControlState.Cancelling || _state == AiExecutionControlState.Cancelled)
                        throw new InvalidOperationException("Execution is already cancelling or cancelled.");
                    if (_state != AiExecutionControlState.Paused)
                        return;
                    if (_execution.IsCompleted)
                        throw new InvalidOperationException("Execution is already terminal.");
                    _state = AiExecutionControlState.Running;
                    _version++;
                    signal = _resumeSignal;
                    _resumeSignal = CreateCompletedSignal();
                }
                signal.TrySetResult(true);
            }

            public void Cancel()
            {
                TaskCompletionSource<bool> signal;
                lock (_sync)
                {
                    ThrowIfDisposed();
                    if (_state == AiExecutionControlState.Cancelled || _state == AiExecutionControlState.Cancelling)
                        return;
                    if (_execution.IsCompleted)
                        throw new InvalidOperationException("Execution is already terminal.");
                    _state = AiExecutionControlState.Cancelling;
                    _version++;
                    signal = _resumeSignal;
                    _resumeSignal = CreateCompletedSignal();
                }
                _cancellation.Cancel();
                signal.TrySetResult(true);
            }

            public ApplyResult TryApply(AiInterventionAction action, string expectedState, long expectedVersion)
            {
                TaskCompletionSource<bool> signal = null;
                var applied = false;
                var reason = string.Empty;

                lock (_sync)
                {
                    ThrowIfDisposed();
                    if (_execution.IsCompleted)
                        return ApplyResult.Stale("The target execution is already terminal.");
                    if (!string.Equals(_state.ToString(), expectedState ?? string.Empty, StringComparison.Ordinal))
                        return ApplyResult.Stale("The target control state changed from " + (expectedState ?? string.Empty) + " to " + _state + ".");
                    if (_version != expectedVersion)
                        return ApplyResult.Stale("The target control version changed from " + expectedVersion + " to " + _version + ".");

                    switch (action)
                    {
                        case AiInterventionAction.Pause:
                            if (_state == AiExecutionControlState.Cancelling || _state == AiExecutionControlState.Cancelled)
                                reason = "The execution is already cancelling or cancelled.";
                            else
                            {
                                _state = AiExecutionControlState.Paused;
                                _version++;
                                _resumeSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                                applied = true;
                            }
                            break;

                        case AiInterventionAction.Resume:
                            if (_state != AiExecutionControlState.Paused)
                                reason = "The execution is no longer paused.";
                            else
                            {
                                _state = AiExecutionControlState.Running;
                                _version++;
                                signal = _resumeSignal;
                                _resumeSignal = CreateCompletedSignal();
                                applied = true;
                            }
                            break;

                        case AiInterventionAction.Cancel:
                            if (_state == AiExecutionControlState.Cancelling || _state == AiExecutionControlState.Cancelled)
                            {
                                reason = "The execution is already cancelling or cancelled.";
                            }
                            else
                            {
                                _state = AiExecutionControlState.Cancelling;
                                _version++;
                                signal = _resumeSignal;
                                _resumeSignal = CreateCompletedSignal();
                                applied = true;
                            }
                            break;

                        default:
                            reason = "The execution intervention action is not supported: " + action;
                            break;
                    }
                }

                if (applied && action == AiInterventionAction.Cancel)
                    _cancellation.Cancel();
                if (signal != null)
                    signal.TrySetResult(true);

                return applied ? ApplyResult.AppliedResult : ApplyResult.Stale(reason);
            }

            public async Task WaitIfPausedAsync(CancellationToken cancellationToken)
            {
                while (true)
                {
                    Task resumeTask;
                    lock (_sync)
                    {
                        ThrowIfDisposed();
                        if (_state != AiExecutionControlState.Paused)
                            return;
                        resumeTask = _resumeSignal.Task;
                    }
                    await AwaitSignalAsync(resumeTask, cancellationToken).ConfigureAwait(false);
                }
            }

            public void Dispose()
            {
                lock (_sync)
                {
                    if (_disposed) return;
                    _disposed = true;
                    _state = _cancellation.IsCancellationRequested
                        ? AiExecutionControlState.Cancelled
                        : _state;
                    _resumeSignal.TrySetResult(true);
                }
                _cancellation.Dispose();
                ResolutionGate.Dispose();
            }

            private static TaskCompletionSource<bool> CreateCompletedSignal()
            {
                var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                source.TrySetResult(true);
                return source;
            }

            private static async Task AwaitSignalAsync(Task signal, CancellationToken cancellationToken)
            {
                if (signal.IsCompleted)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await signal.ConfigureAwait(false);
                    return;
                }

                var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                var completed = await Task.WhenAny(signal, cancellationTask).ConfigureAwait(false);
                if (completed != signal)
                    cancellationToken.ThrowIfCancellationRequested();
                await signal.ConfigureAwait(false);
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                    throw new ObjectDisposedException("ExecutionControl");
            }
        }

        private struct ControlSnapshot
        {
            public ControlSnapshot(AiExecutionControlState state, long version)
            {
                State = state;
                Version = version;
            }

            public AiExecutionControlState State { get; private set; }
            public long Version { get; private set; }
        }

        private struct ApplyResult
        {
            private ApplyResult(bool applied, string reason)
            {
                Applied = applied;
                Reason = reason ?? string.Empty;
            }

            public bool Applied { get; private set; }
            public string Reason { get; private set; }

            public static ApplyResult AppliedResult { get { return new ApplyResult(true, string.Empty); } }
            public static ApplyResult Stale(string reason) { return new ApplyResult(false, reason); }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
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
            var control = new ExecutionControl();
            if (!_executionControls.TryAdd(execution.Id, control))
                throw new InvalidOperationException("An execution control already exists for execution: " + execution.Id);
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

        public async Task<AiExecutionControlState> GetExecutionControlStateAsync(
            string executionId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(executionId)) throw new ArgumentException("Execution ID is required.", nameof(executionId));

            ExecutionControl control;
            if (!_executionControls.TryGetValue(executionId, out control))
                return AiExecutionControlState.Cancelled;
            return control.State;
        }

        public Task PauseExecutionAsync(string executionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetControl(executionId).Pause();
            return Task.CompletedTask;
        }

        public Task ResumeExecutionAsync(string executionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetControl(executionId).Resume();
            return Task.CompletedTask;
        }

        public Task CancelExecutionAsync(string executionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetControl(executionId).Cancel();
            return Task.CompletedTask;
        }

        public Task<AiInterventionRequest> RequestExecutionInterventionAsync(
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
            var operation = requestedAction == AiInterventionAction.Pause
                ? "execution.pause"
                : requestedAction == AiInterventionAction.Resume
                    ? "execution.resume"
                    : "execution.cancel";

            return _workflow.CreateAsync(
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
                cancellationToken);
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
                if (resolution != AiInterventionRequestStatus.Approved &&
                    resolution != AiInterventionRequestStatus.Rejected &&
                    resolution != AiInterventionRequestStatus.Cancelled &&
                    resolution != AiInterventionRequestStatus.Expired)
                    throw new ArgumentOutOfRangeException(nameof(resolution));

                if (resolution == AiInterventionRequestStatus.Approved)
                {
                    var control = GetControl(request.ExecutionId);
                    switch (request.RequestedAction)
                    {
                        case AiInterventionAction.Pause:
                            control.Pause();
                            break;
                        case AiInterventionAction.Resume:
                            control.Resume();
                            break;
                        case AiInterventionAction.Cancel:
                            control.Cancel();
                            break;
                    }

                    var approved = await _workflow.ResolveAsync(
                        requestId,
                        AiInterventionRequestStatus.Approved,
                        responderIdentity,
                        reason,
                        cancellationToken).ConfigureAwait(false);
                    return await _workflow.CompleteAsync(
                        requestId,
                        responderIdentity,
                        string.IsNullOrWhiteSpace(reason)
                            ? "Execution intervention was applied."
                            : reason,
                        cancellationToken).ConfigureAwait(false);
                }
            }

            return await _workflow.ResolveAsync(
                requestId,
                resolution,
                responderIdentity,
                reason,
                cancellationToken).ConfigureAwait(false);
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
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private TaskCompletionSource<bool> _resumeSignal;
            private AiExecutionControlState _state;
            private bool _disposed;

            public ExecutionControl()
            {
                _resumeSignal = CreateCompletedSignal();
                _state = AiExecutionControlState.Running;
            }

            public CancellationToken CancellationToken { get { return _cancellation.Token; } }
            public bool IsCancellationRequested { get { return _cancellation.IsCancellationRequested; } }

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

            public void Pause()
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    if (_state == AiExecutionControlState.Cancelling || _state == AiExecutionControlState.Cancelled)
                        throw new InvalidOperationException("Execution is already cancelling or cancelled.");
                    if (_state == AiExecutionControlState.Paused)
                        return;
                    _state = AiExecutionControlState.Paused;
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
                    _state = AiExecutionControlState.Running;
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
                    if (_state == AiExecutionControlState.Cancelled)
                        return;
                    _state = AiExecutionControlState.Cancelling;
                    signal = _resumeSignal;
                    _resumeSignal = CreateCompletedSignal();
                }
                _cancellation.Cancel();
                signal.TrySetResult(true);
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
    }
}

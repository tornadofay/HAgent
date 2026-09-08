using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public sealed class AgentExecution
    {
        private readonly object _terminalSync = new object();
        private readonly CancellationTokenSource _interventionCts = new CancellationTokenSource();
        private TaskCompletionSource<bool> _resumeSignal = CreateCompletedSignal();

        internal AgentExecution(AgentExecutionSnapshot snapshot, IReadOnlyList<AIMessage> messages)
        {
            Id = Guid.NewGuid().ToString("N");
            CorrelationId = Guid.NewGuid().ToString("N");
            Snapshot = snapshot;
            Messages = messages;
            State = Runtime.AgentExecutionState.Created;
            FailureKind = AgentExecutionFailureKind.None;
            ProviderErrorKind = ProviderErrorKind.Unknown;
            PolicyDecision = new AiPolicyDecision();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public string Id { get; private set; }
        public string CorrelationId { get; private set; }
        public string HostCorrelationId { get; internal set; }
        public string RuntimeInstanceId { get; internal set; }
        public long RuntimeInstanceRevision { get; internal set; }
        public AgentExecutionSnapshot Snapshot { get; private set; }
        public IReadOnlyList<AIMessage> Messages { get; internal set; }
        public AIResponse Response { get; internal set; }
        public Exception Error { get; internal set; }
        public Runtime.AgentExecutionState State { get; internal set; }
        public AgentExecutionFailureKind FailureKind { get; internal set; }
        public ProviderErrorKind ProviderErrorKind { get; internal set; }
        public AiPolicyDecision PolicyDecision { get; internal set; }
        public AiInterventionRequest InterventionRequest { get; internal set; }
        public string LastProviderId { get; internal set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? StartedAt { get; internal set; }
        public DateTimeOffset? CompletedAt { get; internal set; }
        public long ControlRevision { get; private set; }
        internal CancellationToken InterventionCancellationToken { get { return _interventionCts.Token; } }

        public TimeSpan? Duration
        {
            get { return StartedAt.HasValue && CompletedAt.HasValue ? CompletedAt.Value - StartedAt.Value : (TimeSpan?)null; }
        }

        public AgentIdentityContext Identity
        {
            get { return Snapshot == null ? new AgentIdentityContext() : Snapshot.Identity; }
        }

        public bool IsCompleted
        {
            get
            {
                lock (_terminalSync)
                {
                    return IsTerminalState(State);
                }
            }
        }

        internal bool TrySetWaitingForIntervention()
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State) || State == Runtime.AgentExecutionState.WaitingForIntervention) return false;
                if (State != Runtime.AgentExecutionState.Running && State != Runtime.AgentExecutionState.Created) return false;
                State = Runtime.AgentExecutionState.WaitingForIntervention;
                ++ControlRevision;
                return true;
            }
        }

        internal bool TryResumeFromIntervention()
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State) || State != Runtime.AgentExecutionState.WaitingForIntervention) return false;
                State = Runtime.AgentExecutionState.Running;
                ++ControlRevision;
                return true;
            }
        }

        internal bool TryPauseByIntervention()
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State) || State != Runtime.AgentExecutionState.Running) return false;
                State = Runtime.AgentExecutionState.Paused;
                ++ControlRevision;
                _resumeSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                return true;
            }
        }

        internal bool TryResumeByIntervention()
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State) || State != Runtime.AgentExecutionState.Paused) return false;
                State = Runtime.AgentExecutionState.Running;
                ++ControlRevision;
                _resumeSignal.TrySetResult(true);
                return true;
            }
        }

        internal bool TryCancelByIntervention(string reason)
        {
            _interventionCts.Cancel();
            return TryCompleteCancelled(
                new OperationCanceledException(string.IsNullOrWhiteSpace(reason) ? "Execution was cancelled by intervention." : reason),
                AgentExecutionFailureKind.Cancelled,
                DateTimeOffset.UtcNow);
        }

        internal async Task WaitForRunnableAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Task resumeTask;
                lock (_terminalSync)
                {
                    if (IsTerminalState(State)) return;
                    if (State != Runtime.AgentExecutionState.Paused) return;
                    resumeTask = _resumeSignal.Task;
                }

                var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                var completed = await Task.WhenAny(resumeTask, cancellationTask).ConfigureAwait(false);
                if (completed == cancellationTask)
                    cancellationToken.ThrowIfCancellationRequested();
            }
        }

        internal bool TryCompleteSucceeded(AIResponse response, DateTimeOffset completedAt)
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State)) return false;
                if (State == Runtime.AgentExecutionState.Paused || State == Runtime.AgentExecutionState.WaitingForIntervention) return false;

                Response = response;
                Error = null;
                FailureKind = AgentExecutionFailureKind.None;
                ProviderErrorKind = ProviderErrorKind.Unknown;
                State = Runtime.AgentExecutionState.Succeeded;
                CompletedAt = completedAt;
                _resumeSignal.TrySetResult(true);
                _interventionCts.Cancel();
                return true;
            }
        }

        internal bool TryCompleteFailed(
            Exception error,
            AgentExecutionFailureKind failureKind,
            ProviderErrorKind providerErrorKind,
            DateTimeOffset completedAt)
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State)) return false;

                Response = null;
                Error = error;
                FailureKind = failureKind == AgentExecutionFailureKind.None
                    ? AgentExecutionFailureKind.Unknown
                    : failureKind;
                ProviderErrorKind = providerErrorKind;
                State = Runtime.AgentExecutionState.Failed;
                CompletedAt = completedAt;
                _resumeSignal.TrySetResult(true);
                _interventionCts.Cancel();
                return true;
            }
        }

        internal bool TryCompleteCancelled(
            Exception error,
            AgentExecutionFailureKind failureKind,
            DateTimeOffset completedAt)
        {
            if (failureKind != AgentExecutionFailureKind.Cancelled &&
                failureKind != AgentExecutionFailureKind.Timeout)
                throw new ArgumentException("Cancellation completion must use Cancelled or Timeout failure kind.", nameof(failureKind));

            lock (_terminalSync)
            {
                if (IsTerminalState(State)) return false;

                Response = null;
                Error = error;
                FailureKind = failureKind;
                State = Runtime.AgentExecutionState.Cancelled;
                CompletedAt = completedAt;
                _resumeSignal.TrySetResult(true);
                _interventionCts.Cancel();
                return true;
            }
        }

        private static TaskCompletionSource<bool> CreateCompletedSignal()
        {
            var signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            signal.TrySetResult(true);
            return signal;
        }

        private static bool IsTerminalState(Runtime.AgentExecutionState state)
        {
            return state == Runtime.AgentExecutionState.Succeeded ||
                   state == Runtime.AgentExecutionState.Failed ||
                   state == Runtime.AgentExecutionState.Cancelled;
        }
    }
}

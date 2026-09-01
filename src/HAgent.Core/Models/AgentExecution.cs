using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AgentExecution
    {
        private readonly object _terminalSync = new object();

        internal AgentExecution(AgentExecutionSnapshot snapshot, IReadOnlyList<AIMessage> messages)
        {
            Id = Guid.NewGuid().ToString("N");
            CorrelationId = Guid.NewGuid().ToString("N");
            Snapshot = snapshot;
            Messages = messages;
            State = Runtime.AgentExecutionState.Created;
            FailureKind = AgentExecutionFailureKind.None;
            ProviderErrorKind = ProviderErrorKind.Unknown;
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
        public string LastProviderId { get; internal set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? StartedAt { get; internal set; }
        public DateTimeOffset? CompletedAt { get; internal set; }
        public TimeSpan? Duration
        {
            get { return StartedAt.HasValue && CompletedAt.HasValue ? CompletedAt.Value - StartedAt.Value : (TimeSpan?)null; }
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

        internal bool TryCompleteSucceeded(AIResponse response, DateTimeOffset completedAt)
        {
            lock (_terminalSync)
            {
                if (IsTerminalState(State)) return false;

                Response = response;
                Error = null;
                FailureKind = AgentExecutionFailureKind.None;
                ProviderErrorKind = ProviderErrorKind.Unknown;
                State = Runtime.AgentExecutionState.Succeeded;
                CompletedAt = completedAt;
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
                return true;
            }
        }

        private static bool IsTerminalState(Runtime.AgentExecutionState state)
        {
            return state == Runtime.AgentExecutionState.Succeeded ||
                   state == Runtime.AgentExecutionState.Failed ||
                   state == Runtime.AgentExecutionState.Cancelled;
        }
    }
}

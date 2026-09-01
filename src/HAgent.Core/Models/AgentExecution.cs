using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AgentExecution
    {
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
                return State == Runtime.AgentExecutionState.Succeeded ||
                       State == Runtime.AgentExecutionState.Failed ||
                       State == Runtime.AgentExecutionState.Cancelled;
            }
        }
    }
}

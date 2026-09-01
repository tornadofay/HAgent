using System;

namespace HAgent.Models
{
    public enum WorkspaceMessageKind
    {
        User,
        Agent,
        Delegation,
        System
    }

    public sealed class WorkspaceMessage
    {
        public WorkspaceMessage()
        {
            MessageId = Guid.NewGuid().ToString("N");
            CorrelationId = string.Empty;
            CausationId = string.Empty;
            SenderId = string.Empty;
            RecipientId = string.Empty;
            Content = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            Sequence = 0;
        }

        public string MessageId { get; set; }
        public string WorkspaceId { get; set; }
        public WorkspaceMessageKind Kind { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public string CorrelationId { get; set; }
        public string CausationId { get; set; }
        public long Sequence { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

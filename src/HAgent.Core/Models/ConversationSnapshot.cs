using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    /// <summary>
    /// Persistable representation of an agent conversation.
    /// </summary>
    public sealed class ConversationSnapshot
    {
        public ConversationSnapshot()
        {
            Messages = new List<AIMessage>();
        }

        public string SessionId { get; set; }
        public string AgentId { get; set; }
        public List<AIMessage> Messages { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AIReadResult
    {
        public IReadOnlyList<AIMessage> Messages { get; }

        public AIReadResult(IReadOnlyList<AIMessage> messages)
        {
            Messages = messages;
        }
    }
}

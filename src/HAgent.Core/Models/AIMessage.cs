using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AIMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public string ToolCallId { get; set; }
        public IReadOnlyList<AIToolCall> ToolCalls { get; set; }

        public AIMessage()
        {
            Role = string.Empty;
            Content = string.Empty;
            ToolCallId = string.Empty;
            ToolCalls = new List<AIToolCall>().AsReadOnly();
        }

        public AIMessage(string role, string content)
            : this()
        {
            Role = role ?? string.Empty;
            Content = content ?? string.Empty;
        }
    }
}

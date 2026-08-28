using System;

namespace HAgent.Models
{
    public sealed class AIResponseDelta
    {
        public string Text { get; set; }
        public string Reasoning { get; set; }
        public string ToolCallId { get; set; }
        public string ToolCallName { get; set; }
        public string ToolCallArgumentsDelta { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public AIResponseDelta()
        {
            Text = string.Empty;
            Reasoning = string.Empty;
            ToolCallId = string.Empty;
            ToolCallName = string.Empty;
            ToolCallArgumentsDelta = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}

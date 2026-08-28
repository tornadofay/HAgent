using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class AIResponse
    {
        public string AgentId { get; set; }
        public string ProviderId { get; set; }
        public string Model { get; set; }
        public string Text { get; set; }
        public string RequestId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IReadOnlyDictionary<string, object> Usage { get; set; }

        public AIResponse()
        {
            AgentId = string.Empty;
            ProviderId = string.Empty;
            Model = string.Empty;
            Text = string.Empty;
            RequestId = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            Usage = new Dictionary<string, object>();
        }
    }
}

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
        public string Reasoning { get; set; }
        public string RawText { get; set; }
        public string StructuredOutputJson { get; set; }
        public IReadOnlyList<AIToolCall> ToolCalls { get; set; }
        public string RequestId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IReadOnlyDictionary<string, object> Usage { get; set; }
        public IReadOnlyDictionary<string, object> ProviderMetadata { get; set; }

        public AIResponse()
        {
            AgentId = string.Empty;
            ProviderId = string.Empty;
            Model = string.Empty;
            Text = string.Empty;
            Reasoning = string.Empty;
            RawText = string.Empty;
            StructuredOutputJson = string.Empty;
            ToolCalls = new List<AIToolCall>().AsReadOnly();
            RequestId = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            Usage = new Dictionary<string, object>();
            ProviderMetadata = new Dictionary<string, object>();
        }
    }
}

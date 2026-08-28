using System;

namespace HAgent.Models
{
    /// <summary>Provider-neutral representation of a model-requested tool call.</summary>
    public sealed class AIToolCall
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ArgumentsJson { get; set; }

        public AIToolCall()
        {
            Id = string.Empty;
            Name = string.Empty;
            ArgumentsJson = string.Empty;
        }

        public AIToolCall(string id, string name, string argumentsJson)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            ArgumentsJson = argumentsJson ?? string.Empty;
        }
    }
}

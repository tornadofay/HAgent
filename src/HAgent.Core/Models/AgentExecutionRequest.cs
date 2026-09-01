using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Canonical provider-neutral execution request supplied by a host application.
    /// </summary>
    public sealed class AgentExecutionRequest
    {
        public AgentExecutionRequest()
        {
            AgentId = string.Empty;
            Messages = new ReadOnlyCollection<AIMessage>(new List<AIMessage>());
            HostCorrelationId = string.Empty;
            HostContext = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            Options = new AgentExecutionOptions();
        }

        public string AgentId { get; set; }
        public IReadOnlyList<AIMessage> Messages { get; set; }
        public string HostCorrelationId { get; set; }
        public IReadOnlyDictionary<string, string> HostContext { get; set; }
        public AgentExecutionOptions Options { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AgentId))
                throw new ArgumentException("Agent id is required.", nameof(AgentId));
            if (Messages == null || Messages.Count == 0)
                throw new ArgumentException("At least one message is required.", nameof(Messages));
            if (Messages.Count > 128)
                throw new ArgumentOutOfRangeException(nameof(Messages), "A maximum of 128 messages is supported per execution request.");

            if (HostContext != null)
            {
                if (HostContext.Count > 64)
                    throw new ArgumentOutOfRangeException(nameof(HostContext), "A maximum of 64 host context entries is supported per execution request.");

                foreach (var item in HostContext)
                {
                    if (string.IsNullOrWhiteSpace(item.Key) || item.Key.Length > 256)
                        throw new ArgumentException("Host context keys must be non-empty and at most 256 characters.", nameof(HostContext));
                    if (item.Value != null && item.Value.Length > 4096)
                        throw new ArgumentException("Host context values must be at most 4096 characters.", nameof(HostContext));
                }
            }

            if (Options == null)
                Options = new AgentExecutionOptions();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Canonical provider-facing execution request.
    /// This is intentionally separate from AgentExecutionRequest so provider transport
    /// details can evolve without leaking into the host-facing contract.
    /// </summary>
    public sealed class ProviderExecutionRequest
    {
        public ProviderExecutionRequest()
        {
            Provider = null;
            Agent = null;
            ApiKey = string.Empty;
            SystemPrompt = string.Empty;
            Messages = new ReadOnlyCollection<AIMessage>(new List<AIMessage>());
            StructuredOutput = null;
            Tools = new ReadOnlyCollection<AiTool>(new List<AiTool>());
            Progress = null;
        }

        public AiProvider Provider { get; set; }
        public AiAgent Agent { get; set; }
        public string ApiKey { get; set; }
        public string SystemPrompt { get; set; }
        public IReadOnlyList<AIMessage> Messages { get; set; }
        public StructuredOutputOptions StructuredOutput { get; set; }
        public IReadOnlyList<AiTool> Tools { get; set; }
        public IProgress<AIResponseDelta> Progress { get; set; }

        public void Validate()
        {
            if (Provider == null)
                throw new ArgumentNullException(nameof(Provider));
            if (Agent == null)
                throw new ArgumentNullException(nameof(Agent));
            if (Messages == null || Messages.Count == 0)
                throw new ArgumentException("At least one provider message is required.", nameof(Messages));
            if (Messages.Count > 128)
                throw new ArgumentOutOfRangeException(nameof(Messages), "A maximum of 128 messages is supported per provider request.");

            if (StructuredOutput != null)
                StructuredOutput.Validate();

            if (Tools != null && Tools.Count > 128)
                throw new ArgumentOutOfRangeException(nameof(Tools), "A maximum of 128 tools is supported per provider request.");
        }
    }
}

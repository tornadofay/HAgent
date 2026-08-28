using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum CapabilitySupport
    {
        Unknown,
        Supported,
        Unsupported
    }

    public sealed class AiModelCapabilities
    {
        public AiModelCapabilities()
        {
            Model = string.Empty;
            Capabilities = new Dictionary<AiCapability, CapabilitySupport>();
        }

        public string Model { get; set; }
        public IDictionary<AiCapability, CapabilitySupport> Capabilities { get; private set; }

        public CapabilitySupport Get(AiCapability capability)
        {
            CapabilitySupport value;
            return Capabilities.TryGetValue(capability, out value) ? value : CapabilitySupport.Unknown;
        }

        public void Set(AiCapability capability, CapabilitySupport support)
        {
            Capabilities[capability] = support;
        }
    }

    [Flags]
    public enum AiCapability
    {
        None = 0,
        Chat = 1,
        Streaming = 2,
        StructuredOutput = 4,
        ToolCalling = 8,
        Vision = 16,
        AudioInput = 32,
        AudioOutput = 64,
        Embeddings = 128,
        Reasoning = 256
    }
}

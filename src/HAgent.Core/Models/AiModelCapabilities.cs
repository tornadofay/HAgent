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

    public enum CapabilitySource
    {
        Unknown,
        ProviderMetadata,
        AdapterKnowledge,
        UserConfigured,
        RuntimeObservation
    }

    public sealed class CapabilityEvidence
    {
        public CapabilityEvidence()
        {
            Support = CapabilitySupport.Unknown;
            Source = CapabilitySource.Unknown;
            Confidence = 0d;
            ObservedAt = DateTimeOffset.UtcNow;
            Note = string.Empty;
        }

        public CapabilitySupport Support { get; set; }
        public CapabilitySource Source { get; set; }
        public double Confidence { get; set; }
        public DateTimeOffset ObservedAt { get; set; }
        public string Note { get; set; }
    }

    public sealed class AiModelCapabilities
    {
        public AiModelCapabilities()
        {
            Model = string.Empty;
            Capabilities = new Dictionary<AiCapability, CapabilitySupport>();
            Evidence = new Dictionary<AiCapability, CapabilityEvidence>();
        }

        public string Model { get; set; }
        public IDictionary<AiCapability, CapabilitySupport> Capabilities { get; private set; }
        public IDictionary<AiCapability, CapabilityEvidence> Evidence { get; private set; }

        public CapabilitySupport Get(AiCapability capability)
        {
            CapabilitySupport value;
            return Capabilities.TryGetValue(capability, out value) ? value : CapabilitySupport.Unknown;
        }

        public CapabilityEvidence GetEvidence(AiCapability capability)
        {
            CapabilityEvidence value;
            return Evidence.TryGetValue(capability, out value) ? value : new CapabilityEvidence();
        }

        public void Set(AiCapability capability, CapabilitySupport support)
        {
            Set(capability, support, CapabilitySource.Unknown, 0d, null);
        }

        public void Set(
            AiCapability capability,
            CapabilitySupport support,
            CapabilitySource source,
            double confidence,
            string note = null)
        {
            if (confidence < 0d) confidence = 0d;
            if (confidence > 1d) confidence = 1d;

            Capabilities[capability] = support;
            Evidence[capability] = new CapabilityEvidence
            {
                Support = support,
                Source = source,
                Confidence = confidence,
                ObservedAt = DateTimeOffset.UtcNow,
                Note = note ?? string.Empty
            };
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

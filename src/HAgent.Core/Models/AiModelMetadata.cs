using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiMetadataSource
    {
        Unknown,
        ProviderMetadata,
        DiscoveryApi,
        AdapterKnowledge,
        ProviderDocumentation,
        ControlledProbe,
        SuccessfulExecution,
        ResponseMetadata,
        UserOverride
    }

    public sealed class AiModelMetadata
    {
        public AiModelMetadata()
        {
            ProviderId = string.Empty;
            ModelId = string.Empty;
            LogicalModelId = string.Empty;
            DisplayName = string.Empty;
            Version = string.Empty;
            Capabilities = new AiModelCapabilities();
            Cost = AiCostStatus.Unknown;
            Source = AiMetadataSource.Unknown;
            ObservedAt = DateTimeOffset.UtcNow;
            ExpiresAt = null;
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string ProviderId { get; set; }
        public string ModelId { get; set; }
        public string LogicalModelId { get; set; }
        public string DisplayName { get; set; }
        public string Version { get; set; }
        public AiModelCapabilities Capabilities { get; set; }
        public AiCostStatus Cost { get; set; }
        public AiMetadataSource Source { get; set; }
        public DateTimeOffset ObservedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public IDictionary<string, string> Attributes { get; private set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ProviderId)) throw new ArgumentException("Provider id is required.", nameof(ProviderId));
            if (string.IsNullOrWhiteSpace(ModelId)) throw new ArgumentException("Model id is required.", nameof(ModelId));
            if (ProviderId.Length > 256) throw new ArgumentOutOfRangeException(nameof(ProviderId));
            if (ModelId.Length > 512) throw new ArgumentOutOfRangeException(nameof(ModelId));
            if (LogicalModelId != null && LogicalModelId.Length > 512) throw new ArgumentOutOfRangeException(nameof(LogicalModelId));
            if (DisplayName != null && DisplayName.Length > 512) throw new ArgumentOutOfRangeException(nameof(DisplayName));
            if (Version != null && Version.Length > 256) throw new ArgumentOutOfRangeException(nameof(Version));
            if (Capabilities == null) throw new ArgumentNullException(nameof(Capabilities));
        }

        public AiModelMetadata Clone()
        {
            var clone = new AiModelMetadata
            {
                ProviderId = ProviderId,
                ModelId = ModelId,
                LogicalModelId = LogicalModelId,
                DisplayName = DisplayName,
                Version = Version,
                Cost = Cost,
                Source = Source,
                ObservedAt = ObservedAt,
                ExpiresAt = ExpiresAt
            };

            clone.Capabilities = new AiModelCapabilities
            {
                Model = Capabilities == null ? string.Empty : Capabilities.Model
            };
            if (Capabilities != null)
            {
                foreach (var pair in Capabilities.Capabilities)
                    clone.Capabilities.Capabilities[pair.Key] = pair.Value;
                foreach (var pair in Capabilities.Evidence)
                {
                    if (pair.Value == null) continue;
                    clone.Capabilities.Evidence[pair.Key] = new CapabilityEvidence
                    {
                        Support = pair.Value.Support,
                        Source = pair.Value.Source,
                        Confidence = pair.Value.Confidence,
                        ObservedAt = pair.Value.ObservedAt,
                        Note = pair.Value.Note
                    };
                }
            }

            foreach (var pair in Attributes)
                clone.Attributes[pair.Key] = pair.Value;

            return clone;
        }
    }
}

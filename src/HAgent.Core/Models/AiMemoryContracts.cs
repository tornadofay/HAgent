using System;

namespace HAgent.Models
{
    /// <summary>
    /// Provider-neutral memory family. The family is intentionally small and extensible through TypeId.
    /// </summary>
    public enum AiMemoryFamily
    {
        Working = 0,
        Episodic = 1,
        Semantic = 2,
        Procedural = 3,
        Custom = 4
    }

    public enum AiMemoryProvenanceKind
    {
        HostProvided = 0,
        UserProvided = 1,
        Imported = 2,
        ModelGenerated = 3,
        SystemGenerated = 4
    }

    /// <summary>
    /// Bounded provenance for a memory record. It identifies where the memory came from
    /// without making the model, provider, or host application an authority over the record.
    /// </summary>
    public sealed class AiMemoryProvenance
    {
        public AiMemoryProvenanceKind Kind { get; set; }
        public string Source { get; set; }
        public string SourceId { get; set; }
        public string SourceUri { get; set; }
        public string CreatedBy { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string Evidence { get; set; }
        public decimal? Confidence { get; set; }

        public AiMemoryProvenance Clone()
        {
            return new AiMemoryProvenance
            {
                Kind = Kind,
                Source = Source,
                SourceId = SourceId,
                SourceUri = SourceUri,
                CreatedBy = CreatedBy,
                SourceExecutionId = SourceExecutionId,
                SourceRuntimeInstanceId = SourceRuntimeInstanceId,
                Evidence = Evidence,
                Confidence = Confidence
            };
        }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiMemoryProvenanceKind), Kind))
                throw new ArgumentException("Invalid memory provenance kind.");

            ValidateText(Source, 256, false, nameof(Source));
            ValidateText(SourceId, 512, false, nameof(SourceId));
            ValidateText(SourceUri, 2048, false, nameof(SourceUri));
            ValidateText(CreatedBy, 512, false, nameof(CreatedBy));
            ValidateText(SourceExecutionId, 256, false, nameof(SourceExecutionId));
            ValidateText(SourceRuntimeInstanceId, 256, false, nameof(SourceRuntimeInstanceId));
            ValidateText(Evidence, 4096, false, nameof(Evidence));

            if (Confidence.HasValue && (Confidence.Value < 0m || Confidence.Value > 1m))
                throw new ArgumentException("Memory provenance confidence must be between 0 and 1.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }
}

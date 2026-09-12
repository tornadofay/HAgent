using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiReliabilityOutcomeKind
    {
        Success = 0,
        Failure = 1,
        Contradiction = 2,
        InvalidPrecondition = 3
    }

    public enum AiReliabilityDisposition
    {
        Reinforced = 0,
        Weakened = 1,
        QuarantineRecommended = 2
    }

    public sealed class AiResourceReliabilityIdentity
    {
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
        public long? Version { get; set; }
        public AgentResourceScope Scope { get; set; }

        public AiResourceReliabilityIdentity Clone()
        {
            return new AiResourceReliabilityIdentity
            {
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Version = Version,
                Scope = Scope
            };
        }

        public void Validate()
        {
            Require(ResourceType, 128, nameof(ResourceType));
            Require(ResourceId, 512, nameof(ResourceId));
            if (Version.HasValue && Version.Value <= 0)
                throw new ArgumentException("Resource reliability Version must be positive.", nameof(Version));
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope))
                throw new ArgumentException("Invalid resource reliability scope.", nameof(Scope));
        }

        public string ToStableKey()
        {
            Validate();
            return ResourceType + "|" + ResourceId + "|" + (Version.HasValue ? Version.Value.ToString() : "-") + "|" + Scope;
        }

        private static void Require(string value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiReliabilityEvidence
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Summary { get; set; }
        public bool IsPromotionEvidence { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }
        public string EvaluationReferenceId { get; set; }
        public DateTimeOffset ObservedAtUtc { get; set; }

        public AiReliabilityEvidence Clone()
        {
            return new AiReliabilityEvidence
            {
                Id = Id,
                Kind = Kind,
                Summary = Summary,
                IsPromotionEvidence = IsPromotionEvidence,
                SourceExecutionId = SourceExecutionId,
                SourceRuntimeInstanceId = SourceRuntimeInstanceId,
                SourceAgentProfileId = SourceAgentProfileId,
                EvaluationReferenceId = EvaluationReferenceId,
                ObservedAtUtc = ObservedAtUtc
            };
        }

        public void Validate()
        {
            Require(Id, 256, nameof(Id));
            Require(Kind, 128, nameof(Kind));
            Require(Summary, 2048, nameof(Summary));
            Optional(SourceExecutionId, 512, nameof(SourceExecutionId));
            Optional(SourceRuntimeInstanceId, 512, nameof(SourceRuntimeInstanceId));
            Optional(SourceAgentProfileId, 512, nameof(SourceAgentProfileId));
            Optional(EvaluationReferenceId, 512, nameof(EvaluationReferenceId));
            if (ObservedAtUtc == default(DateTimeOffset))
                throw new ArgumentException("Evidence observation time is required.", nameof(ObservedAtUtc));
        }

        private static void Require(string value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }

        private static void Optional(string value, int maxLength, string name)
        {
            if (value != null && value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiReliabilityInitialization
    {
        public AiReliabilityInitialization()
        {
            PromotionEvidence = new AiReliabilityEvidence();
            InitialReliability = 0.50m;
        }

        public AiReliabilityEvidence PromotionEvidence { get; set; }
        public decimal InitialReliability { get; set; }

        public void Validate()
        {
            if (PromotionEvidence == null) throw new ArgumentNullException(nameof(PromotionEvidence));
            PromotionEvidence.IsPromotionEvidence = true;
            PromotionEvidence.Validate();
            if (InitialReliability < 0m || InitialReliability > 1m)
                throw new ArgumentOutOfRangeException(nameof(InitialReliability));
        }
    }

    public sealed class AiValidatedResourceOutcome
    {
        public AiValidatedResourceOutcome()
        {
            Kind = AiReliabilityOutcomeKind.Success;
            ValidationMethod = string.Empty;
            SourceExecutionId = string.Empty;
            SourceRuntimeInstanceId = string.Empty;
            SourceAgentProfileId = string.Empty;
            EvaluationReferenceId = string.Empty;
        }

        public AiReliabilityOutcomeKind Kind { get; set; }
        public bool IsValidated { get; set; }
        public string ValidationMethod { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }
        public string EvaluationReferenceId { get; set; }
        public string EvidenceSummary { get; set; }
        public DateTimeOffset ObservedAtUtc { get; set; }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiReliabilityOutcomeKind), Kind))
                throw new ArgumentOutOfRangeException(nameof(Kind));
            if (!IsValidated)
                throw new InvalidOperationException("Only validated resource outcomes may change reliability evidence.");
            Require(ValidationMethod, 128, nameof(ValidationMethod));
            Require(EvidenceSummary, 2048, nameof(EvidenceSummary));
            Optional(SourceExecutionId, 512, nameof(SourceExecutionId));
            Optional(SourceRuntimeInstanceId, 512, nameof(SourceRuntimeInstanceId));
            Optional(SourceAgentProfileId, 512, nameof(SourceAgentProfileId));
            Optional(EvaluationReferenceId, 512, nameof(EvaluationReferenceId));
            if (ObservedAtUtc == default(DateTimeOffset))
                throw new ArgumentException("Outcome observation time is required.", nameof(ObservedAtUtc));
        }

        private static void Require(string value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }

        private static void Optional(string value, int maxLength, string name)
        {
            if (value != null && value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiResourceReliabilityRecord
    {
        public AiResourceReliabilityRecord()
        {
            Identity = new AiResourceReliabilityIdentity();
            Revision = 0;
            PromotionEvidence = new List<AiReliabilityEvidence>();
            OutcomeEvidence = new List<AiReliabilityEvidence>();
            ReliabilityScore = 0.50m;
            LastOutcomeKind = AiReliabilityOutcomeKind.Success;
        }

        public AiResourceReliabilityIdentity Identity { get; set; }
        public long Revision { get; set; }
        public decimal ReliabilityScore { get; set; }
        public long ValidatedOutcomeCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
        public long ContradictionCount { get; set; }
        public long InvalidPreconditionCount { get; set; }
        public bool RequiresReview { get; set; }
        public bool QuarantineRecommended { get; set; }
        public DateTimeOffset? LastOutcomeAtUtc { get; set; }
        public AiReliabilityOutcomeKind LastOutcomeKind { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public IList<AiReliabilityEvidence> PromotionEvidence { get; private set; }
        public IList<AiReliabilityEvidence> OutcomeEvidence { get; private set; }

        public AiResourceReliabilityRecord Clone()
        {
            var clone = new AiResourceReliabilityRecord
            {
                Identity = Identity == null ? null : Identity.Clone(),
                Revision = Revision,
                ReliabilityScore = ReliabilityScore,
                ValidatedOutcomeCount = ValidatedOutcomeCount,
                SuccessCount = SuccessCount,
                FailureCount = FailureCount,
                ContradictionCount = ContradictionCount,
                InvalidPreconditionCount = InvalidPreconditionCount,
                RequiresReview = RequiresReview,
                QuarantineRecommended = QuarantineRecommended,
                LastOutcomeAtUtc = LastOutcomeAtUtc,
                LastOutcomeKind = LastOutcomeKind,
                UpdatedAtUtc = UpdatedAtUtc
            };
            foreach (var evidence in PromotionEvidence)
                clone.PromotionEvidence.Add(evidence == null ? null : evidence.Clone());
            foreach (var evidence in OutcomeEvidence)
                clone.OutcomeEvidence.Add(evidence == null ? null : evidence.Clone());
            return clone;
        }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (Revision < 0) throw new ArgumentOutOfRangeException(nameof(Revision));
            if (ReliabilityScore < 0m || ReliabilityScore > 1m) throw new ArgumentOutOfRangeException(nameof(ReliabilityScore));
            ValidateCount(ValidatedOutcomeCount, nameof(ValidatedOutcomeCount));
            ValidateCount(SuccessCount, nameof(SuccessCount));
            ValidateCount(FailureCount, nameof(FailureCount));
            ValidateCount(ContradictionCount, nameof(ContradictionCount));
            ValidateCount(InvalidPreconditionCount, nameof(InvalidPreconditionCount));
            if (PromotionEvidence == null || PromotionEvidence.Count == 0 || PromotionEvidence.Count > 8)
                throw new ArgumentException("Promotion evidence must contain 1-8 entries.", nameof(PromotionEvidence));
            if (OutcomeEvidence == null || OutcomeEvidence.Count > 64)
                throw new ArgumentException("Outcome evidence exceeds its bound.", nameof(OutcomeEvidence));
            foreach (var evidence in PromotionEvidence)
            {
                if (evidence == null || !evidence.IsPromotionEvidence) throw new ArgumentException("Promotion evidence is invalid.", nameof(PromotionEvidence));
                evidence.Validate();
            }
            foreach (var evidence in OutcomeEvidence)
            {
                if (evidence == null || evidence.IsPromotionEvidence) throw new ArgumentException("Outcome evidence is invalid.", nameof(OutcomeEvidence));
                evidence.Validate();
            }
            if (UpdatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("UpdatedAtUtc is required.", nameof(UpdatedAtUtc));
        }

        private static void ValidateCount(long value, string name)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiResourceReliabilityOutcomeResult
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public long PreviousRevision { get; set; }
        public long Revision { get; set; }
        public decimal PreviousReliabilityScore { get; set; }
        public decimal ReliabilityScore { get; set; }
        public AiReliabilityOutcomeKind OutcomeKind { get; set; }
        public AiReliabilityDisposition Disposition { get; set; }
        public bool RequiresReview { get; set; }
        public bool QuarantineRecommended { get; set; }
        public string PolicyRuleId { get; set; }
        public string PolicyVersion { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (Revision < 0 || PreviousRevision < 0) throw new ArgumentOutOfRangeException(nameof(Revision));
            if (ReliabilityScore < 0m || ReliabilityScore > 1m || PreviousReliabilityScore < 0m || PreviousReliabilityScore > 1m)
                throw new ArgumentOutOfRangeException(nameof(ReliabilityScore));
            if (!Enum.IsDefined(typeof(AiReliabilityOutcomeKind), OutcomeKind)) throw new ArgumentOutOfRangeException(nameof(OutcomeKind));
            if (!Enum.IsDefined(typeof(AiReliabilityDisposition), Disposition)) throw new ArgumentOutOfRangeException(nameof(Disposition));
            if (UpdatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("UpdatedAtUtc is required.", nameof(UpdatedAtUtc));
        }
    }
}

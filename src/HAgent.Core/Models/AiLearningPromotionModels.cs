using System;

namespace HAgent.Models
{
    public sealed class AiLearningPromotionContext
    {
        public string CandidateId { get; set; }
        public AgentIdentityContext Identity { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }
        public long PolicyVersion { get; set; }
        public string PolicyRuleId { get; set; }
        public DateTimeOffset PromotedAt { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CandidateId) || CandidateId.Length > 512)
                throw new ArgumentException("CandidateId is required and bounded.", nameof(CandidateId));
            if (Identity == null)
                throw new ArgumentException("Promotion identity is required.", nameof(Identity));
            Identity.Validate();
            if (SourceExecutionId != null && SourceExecutionId.Length > 512)
                throw new ArgumentException("SourceExecutionId exceeds its maximum length.", nameof(SourceExecutionId));
            if (SourceRuntimeInstanceId != null && SourceRuntimeInstanceId.Length > 512)
                throw new ArgumentException("SourceRuntimeInstanceId exceeds its maximum length.", nameof(SourceRuntimeInstanceId));
            if (SourceAgentProfileId != null && SourceAgentProfileId.Length > 512)
                throw new ArgumentException("SourceAgentProfileId exceeds its maximum length.", nameof(SourceAgentProfileId));
            if (PolicyVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(PolicyVersion));
            if (PolicyRuleId != null && PolicyRuleId.Length > 128)
                throw new ArgumentException("PolicyRuleId exceeds its maximum length.", nameof(PolicyRuleId));
            if (PromotedAt == default(DateTimeOffset))
                throw new ArgumentException("PromotedAt is required.", nameof(PromotedAt));
        }
    }

    public sealed class AiLearningPromotionResult
    {
        public string CandidateId { get; set; }
        public AiLearningCandidateType CandidateType { get; set; }
        public AiLearningCandidateStatus CandidateStatus { get; set; }
        public long CandidateRevision { get; set; }
        public string AuthoritativeResourceType { get; set; }
        public string AuthoritativeResourceId { get; set; }
        public long? AuthoritativeResourceVersion { get; set; }
        public DateTimeOffset PromotedAt { get; set; }
        public string PolicyId { get; set; }
        public long PolicyVersion { get; set; }
        public string PolicyRuleId { get; set; }
        public string AuthorizationOutcome { get; set; }
        public string AuthorizationReason { get; set; }
        public string Provenance { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CandidateId)) throw new ArgumentException("CandidateId is required.");
            if (!Enum.IsDefined(typeof(AiLearningCandidateType), CandidateType)) throw new ArgumentOutOfRangeException(nameof(CandidateType));
            if (!Enum.IsDefined(typeof(AiLearningCandidateStatus), CandidateStatus)) throw new ArgumentOutOfRangeException(nameof(CandidateStatus));
            if (CandidateRevision < 0) throw new ArgumentOutOfRangeException(nameof(CandidateRevision));
            if (string.IsNullOrWhiteSpace(AuthoritativeResourceType)) throw new ArgumentException("AuthoritativeResourceType is required.");
            if (string.IsNullOrWhiteSpace(AuthoritativeResourceId)) throw new ArgumentException("AuthoritativeResourceId is required.");
            if (AuthoritativeResourceVersion.HasValue && AuthoritativeResourceVersion.Value <= 0) throw new ArgumentOutOfRangeException(nameof(AuthoritativeResourceVersion));
            if (PromotedAt == default(DateTimeOffset)) throw new ArgumentException("PromotedAt is required.");
            if (PolicyVersion < 0) throw new ArgumentOutOfRangeException(nameof(PolicyVersion));
        }
    }

    public sealed class AiLearningPromotionConflictException : InvalidOperationException
    {
        public AiLearningPromotionConflictException(string message) : base(message) { }
    }
}

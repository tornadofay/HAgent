using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiLearnedResourceRuntimeDecision
    {
        Use = 0,
        FallBack = 1
    }

    public enum AiLearnedResourceFallbackKind
    {
        DeterministicSafeAction = 0,
        AlternateResource = 1,
        BoundedReasoning = 2,
        HostEscalation = 3
    }

    public sealed class AiLearnedResourceExecutionSnapshot
    {
        public AiLearnedResourceExecutionSnapshot()
        {
            Identity = new AiResourceReliabilityIdentity();
            LifecycleStatus = AiLearnedResourceLifecycleStatus.Active;
            LifecycleCondition = AiLearnedResourceCondition.Current;
            RetentionDecision = AiLearnedResourceRetentionDecision.Keep;
            ApplicabilityOutcome = AiApplicabilityOutcome.Uncertain;
        }

        public AiResourceReliabilityIdentity Identity { get; set; }
        public long ReliabilityRevision { get; set; }
        public decimal ReliabilityScore { get; set; }
        public bool ReliabilityRequiresReview { get; set; }
        public bool ReliabilityQuarantineRecommended { get; set; }
        public long LifecycleRevision { get; set; }
        public AiLearnedResourceLifecycleStatus LifecycleStatus { get; set; }
        public AiLearnedResourceCondition LifecycleCondition { get; set; }
        public AiLearnedResourceRetentionDecision RetentionDecision { get; set; }
        public AiApplicabilityOutcome ApplicabilityOutcome { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }

        public bool IsSafeToUse
        {
            get
            {
                return LifecycleStatus == AiLearnedResourceLifecycleStatus.Active &&
                       LifecycleCondition == AiLearnedResourceCondition.Current &&
                       ApplicabilityOutcome == AiApplicabilityOutcome.Applicable &&
                       !ReliabilityRequiresReview &&
                       !ReliabilityQuarantineRecommended;
            }
        }

        public AiLearnedResourceExecutionSnapshot Clone()
        {
            return new AiLearnedResourceExecutionSnapshot
            {
                Identity = Identity == null ? null : Identity.Clone(),
                ReliabilityRevision = ReliabilityRevision,
                ReliabilityScore = ReliabilityScore,
                ReliabilityRequiresReview = ReliabilityRequiresReview,
                ReliabilityQuarantineRecommended = ReliabilityQuarantineRecommended,
                LifecycleRevision = LifecycleRevision,
                LifecycleStatus = LifecycleStatus,
                LifecycleCondition = LifecycleCondition,
                RetentionDecision = RetentionDecision,
                ApplicabilityOutcome = ApplicabilityOutcome,
                CapturedAtUtc = CapturedAtUtc
            };
        }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (ReliabilityRevision < 0) throw new ArgumentOutOfRangeException(nameof(ReliabilityRevision));
            if (LifecycleRevision < 0) throw new ArgumentOutOfRangeException(nameof(LifecycleRevision));
            if (ReliabilityScore < 0m || ReliabilityScore > 1m) throw new ArgumentOutOfRangeException(nameof(ReliabilityScore));
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), LifecycleStatus)) throw new ArgumentOutOfRangeException(nameof(LifecycleStatus));
            if (!Enum.IsDefined(typeof(AiLearnedResourceCondition), LifecycleCondition)) throw new ArgumentOutOfRangeException(nameof(LifecycleCondition));
            if (!Enum.IsDefined(typeof(AiLearnedResourceRetentionDecision), RetentionDecision)) throw new ArgumentOutOfRangeException(nameof(RetentionDecision));
            if (!Enum.IsDefined(typeof(AiApplicabilityOutcome), ApplicabilityOutcome)) throw new ArgumentOutOfRangeException(nameof(ApplicabilityOutcome));
            if (CapturedAtUtc == default(DateTimeOffset)) throw new ArgumentException("CapturedAtUtc is required.", nameof(CapturedAtUtc));
        }
    }

    public sealed class AiLearnedResourceRuntimeAssessmentRequest
    {
        public AiLearnedResourceRuntimeAssessmentRequest()
        {
            Identity = new AiResourceReliabilityIdentity();
            PolicyIdentity = new AgentIdentityContext();
            PreferredFallbacks = new List<AiLearnedResourceFallbackKind>();
            PreferredFallbacks.Add(AiLearnedResourceFallbackKind.DeterministicSafeAction);
            PreferredFallbacks.Add(AiLearnedResourceFallbackKind.AlternateResource);
            PreferredFallbacks.Add(AiLearnedResourceFallbackKind.BoundedReasoning);
            PreferredFallbacks.Add(AiLearnedResourceFallbackKind.HostEscalation);
        }

        public AiResourceReliabilityIdentity Identity { get; set; }
        public long ExpectedReliabilityRevision { get; set; }
        public long ExpectedLifecycleRevision { get; set; }
        public AiApplicabilityDecision Applicability { get; set; }
        public AgentIdentityContext PolicyIdentity { get; set; }
        public IList<AiLearnedResourceFallbackKind> PreferredFallbacks { get; private set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (ExpectedReliabilityRevision < 0) throw new ArgumentOutOfRangeException(nameof(ExpectedReliabilityRevision));
            if (ExpectedLifecycleRevision < 0) throw new ArgumentOutOfRangeException(nameof(ExpectedLifecycleRevision));
            if (Applicability == null) throw new ArgumentNullException(nameof(Applicability));
            Applicability.Validate();
            if (PolicyIdentity == null) throw new ArgumentNullException(nameof(PolicyIdentity));
            PolicyIdentity.Validate();
            if (PreferredFallbacks == null || PreferredFallbacks.Count == 0 || PreferredFallbacks.Count > 4)
                throw new ArgumentException("PreferredFallbacks must contain 1-4 entries.", nameof(PreferredFallbacks));
        }
    }

    public sealed class AiLearnedResourceRuntimeAssessmentResult
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public AiLearnedResourceRuntimeDecision Decision { get; set; }
        public AiLearnedResourceFallbackKind Fallback { get; set; }
        public bool ReliabilityRevisionCurrent { get; set; }
        public bool LifecycleRevisionCurrent { get; set; }
        public AiLearnedResourceExecutionSnapshot Snapshot { get; set; }
        public string PolicyRuleId { get; set; }
        public string PolicyVersion { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset EvaluatedAtUtc { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (!Enum.IsDefined(typeof(AiLearnedResourceRuntimeDecision), Decision)) throw new ArgumentOutOfRangeException(nameof(Decision));
            if (!Enum.IsDefined(typeof(AiLearnedResourceFallbackKind), Fallback)) throw new ArgumentOutOfRangeException(nameof(Fallback));
            if (Snapshot == null) throw new ArgumentNullException(nameof(Snapshot));
            Snapshot.Validate();
            if (string.IsNullOrWhiteSpace(Reason) || Reason.Length > 2048) throw new ArgumentException("Reason is required and bounded.", nameof(Reason));
            if (EvaluatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("EvaluatedAtUtc is required.", nameof(EvaluatedAtUtc));
        }
    }
}

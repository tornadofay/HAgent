using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiLearnedResourceLifecycleStatus
    {
        Active = 0,
        UnderReview = 1,
        Quarantined = 2,
        Archived = 3,
        Retired = 4
    }

    public enum AiLearnedResourceCondition
    {
        Current = 0,
        Stale = 1,
        Degraded = 2,
        Drifted = 3,
        Contradicted = 4
    }

    public enum AiLearnedResourceRetentionDecision
    {
        Keep = 0,
        Archive = 1,
        Retire = 2,
        Restore = 3
    }

    public sealed class AiLearnedResourceLifecycleEvent
    {
        public DateTimeOffset OccurredAtUtc { get; set; }
        public AiLearnedResourceLifecycleStatus FromStatus { get; set; }
        public AiLearnedResourceLifecycleStatus ToStatus { get; set; }
        public AiLearnedResourceCondition Condition { get; set; }
        public string Reason { get; set; }
        public string PolicyRuleId { get; set; }
        public string PolicyVersion { get; set; }

        public AiLearnedResourceLifecycleEvent Clone()
        {
            return new AiLearnedResourceLifecycleEvent
            {
                OccurredAtUtc = OccurredAtUtc,
                FromStatus = FromStatus,
                ToStatus = ToStatus,
                Condition = Condition,
                Reason = Reason,
                PolicyRuleId = PolicyRuleId,
                PolicyVersion = PolicyVersion
            };
        }

        public void Validate()
        {
            if (OccurredAtUtc == default(DateTimeOffset)) throw new ArgumentException("Lifecycle event time is required.", nameof(OccurredAtUtc));
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), FromStatus)) throw new ArgumentOutOfRangeException(nameof(FromStatus));
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), ToStatus)) throw new ArgumentOutOfRangeException(nameof(ToStatus));
            if (!Enum.IsDefined(typeof(AiLearnedResourceCondition), Condition)) throw new ArgumentOutOfRangeException(nameof(Condition));
            Require(Reason, 2048, nameof(Reason));
            Optional(PolicyRuleId, 128, nameof(PolicyRuleId));
            Optional(PolicyVersion, 128, nameof(PolicyVersion));
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

    public sealed class AiLearnedResourceLifecycleRecord
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public long Revision { get; set; }
        public AiLearnedResourceLifecycleStatus Status { get; set; }
        public AiLearnedResourceCondition LastCondition { get; set; }
        public DateTimeOffset PromotedAtUtc { get; set; }
        public DateTimeOffset? LastAssessedAtUtc { get; set; }
        public DateTimeOffset? LastTransitionAtUtc { get; set; }
        public long? LastReliabilityRevision { get; set; }
        public decimal? LastReliabilityScore { get; set; }
        public AiApplicabilityOutcome? LastApplicabilityOutcome { get; set; }
        public AiEvaluationOutcome? LastEvaluationOutcome { get; set; }
        public decimal? LastRetentionUtilityScore { get; set; }
        public AiLearnedResourceRetentionDecision LastRetentionDecision { get; set; }
        public DateTimeOffset? LastRetentionAssessedAtUtc { get; set; }
        public int RetentionAssessmentCount { get; set; }
        public string LastReason { get; set; }
        public IList<AiLearnedResourceLifecycleEvent> History { get; private set; }

        public AiLearnedResourceLifecycleRecord()
        {
            Identity = new AiResourceReliabilityIdentity();
            Status = AiLearnedResourceLifecycleStatus.Active;
            LastCondition = AiLearnedResourceCondition.Current;
            LastRetentionDecision = AiLearnedResourceRetentionDecision.Keep;
            LastReason = string.Empty;
            History = new List<AiLearnedResourceLifecycleEvent>();
        }

        public bool IsAutomaticallyUsable
        {
            get { return Status == AiLearnedResourceLifecycleStatus.Active && LastCondition == AiLearnedResourceCondition.Current; }
        }

        public bool IsRestorable
        {
            get { return Status == AiLearnedResourceLifecycleStatus.Archived; }
        }

        public AiLearnedResourceLifecycleRecord Clone()
        {
            var clone = new AiLearnedResourceLifecycleRecord
            {
                Identity = Identity == null ? null : Identity.Clone(),
                Revision = Revision,
                Status = Status,
                LastCondition = LastCondition,
                PromotedAtUtc = PromotedAtUtc,
                LastAssessedAtUtc = LastAssessedAtUtc,
                LastTransitionAtUtc = LastTransitionAtUtc,
                LastReliabilityRevision = LastReliabilityRevision,
                LastReliabilityScore = LastReliabilityScore,
                LastApplicabilityOutcome = LastApplicabilityOutcome,
                LastEvaluationOutcome = LastEvaluationOutcome,
                LastRetentionUtilityScore = LastRetentionUtilityScore,
                LastRetentionDecision = LastRetentionDecision,
                LastRetentionAssessedAtUtc = LastRetentionAssessedAtUtc,
                RetentionAssessmentCount = RetentionAssessmentCount,
                LastReason = LastReason
            };
            foreach (var item in History ?? new List<AiLearnedResourceLifecycleEvent>())
                clone.History.Add(item == null ? null : item.Clone());
            return clone;
        }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (Revision < 0) throw new ArgumentOutOfRangeException(nameof(Revision));
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), Status)) throw new ArgumentOutOfRangeException(nameof(Status));
            if (!Enum.IsDefined(typeof(AiLearnedResourceCondition), LastCondition)) throw new ArgumentOutOfRangeException(nameof(LastCondition));
            if (!Enum.IsDefined(typeof(AiLearnedResourceRetentionDecision), LastRetentionDecision)) throw new ArgumentOutOfRangeException(nameof(LastRetentionDecision));
            if (PromotedAtUtc == default(DateTimeOffset)) throw new ArgumentException("PromotedAtUtc is required.", nameof(PromotedAtUtc));
            if (LastAssessedAtUtc.HasValue && LastAssessedAtUtc.Value < PromotedAtUtc) throw new ArgumentException("LastAssessedAtUtc cannot precede promotion.");
            if (LastTransitionAtUtc.HasValue && LastTransitionAtUtc.Value < PromotedAtUtc) throw new ArgumentException("LastTransitionAtUtc cannot precede promotion.");
            if (LastRetentionAssessedAtUtc.HasValue && LastRetentionAssessedAtUtc.Value < PromotedAtUtc) throw new ArgumentException("LastRetentionAssessedAtUtc cannot precede promotion.");
            if (LastReliabilityRevision.HasValue && LastReliabilityRevision.Value < 0) throw new ArgumentOutOfRangeException(nameof(LastReliabilityRevision));
            if (LastReliabilityScore.HasValue && (LastReliabilityScore.Value < 0m || LastReliabilityScore.Value > 1m)) throw new ArgumentOutOfRangeException(nameof(LastReliabilityScore));
            if (LastRetentionUtilityScore.HasValue && (LastRetentionUtilityScore.Value < 0m || LastRetentionUtilityScore.Value > 1m)) throw new ArgumentOutOfRangeException(nameof(LastRetentionUtilityScore));
            if (RetentionAssessmentCount < 0) throw new ArgumentOutOfRangeException(nameof(RetentionAssessmentCount));
            if (LastApplicabilityOutcome.HasValue && !Enum.IsDefined(typeof(AiApplicabilityOutcome), LastApplicabilityOutcome.Value)) throw new ArgumentOutOfRangeException(nameof(LastApplicabilityOutcome));
            if (LastEvaluationOutcome.HasValue && !Enum.IsDefined(typeof(AiEvaluationOutcome), LastEvaluationOutcome.Value)) throw new ArgumentOutOfRangeException(nameof(LastEvaluationOutcome));
            Optional(LastReason, 2048, nameof(LastReason));
            if (History == null || History.Count > 64) throw new ArgumentException("Lifecycle history exceeds its bound.", nameof(History));
            foreach (var item in History)
            {
                if (item == null) throw new ArgumentException("Lifecycle history cannot contain null entries.", nameof(History));
                item.Validate();
            }
        }

        private static void Optional(string value, int maxLength, string name)
        {
            if (value != null && value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiLearnedResourceRevalidationRequest
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public AiResourceReliabilityRecord Reliability { get; set; }
        public AiApplicabilityDecision PreviousApplicability { get; set; }
        public AiApplicabilityDecision CurrentApplicability { get; set; }
        public AiEvaluation Evaluation { get; set; }
        public DateTimeOffset EvaluatedAtUtc { get; set; }
        public TimeSpan MaxAge { get; set; }
        public AgentIdentityContext PolicyIdentity { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (Reliability == null) throw new ArgumentNullException(nameof(Reliability));
            Reliability.Validate();
            if (PreviousApplicability == null) throw new ArgumentNullException(nameof(PreviousApplicability));
            PreviousApplicability.Validate();
            if (CurrentApplicability == null) throw new ArgumentNullException(nameof(CurrentApplicability));
            CurrentApplicability.Validate();
            if (Evaluation != null) Evaluation.Validate();
            if (EvaluatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("EvaluatedAtUtc is required.", nameof(EvaluatedAtUtc));
            if (MaxAge <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(MaxAge));
            if (PolicyIdentity == null) throw new ArgumentNullException(nameof(PolicyIdentity));
            PolicyIdentity.Validate();
            if (!string.Equals(Identity.ResourceType, Reliability.Identity.ResourceType, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(Identity.ResourceId, Reliability.Identity.ResourceId, StringComparison.Ordinal) ||
                Identity.Version != Reliability.Identity.Version || Identity.Scope != Reliability.Identity.Scope)
                throw new ArgumentException("Revalidation identity must match reliability identity.", nameof(Reliability));
        }
    }

    public sealed class AiLearnedResourceRevalidationResult
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public AiLearnedResourceLifecycleStatus PreviousStatus { get; set; }
        public AiLearnedResourceLifecycleStatus Status { get; set; }
        public AiLearnedResourceCondition Condition { get; set; }
        public long Revision { get; set; }
        public bool ReplacementRecommended { get; set; }
        public string PolicyRuleId { get; set; }
        public string PolicyVersion { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset EvaluatedAtUtc { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), PreviousStatus)) throw new ArgumentOutOfRangeException(nameof(PreviousStatus));
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), Status)) throw new ArgumentOutOfRangeException(nameof(Status));
            if (!Enum.IsDefined(typeof(AiLearnedResourceCondition), Condition)) throw new ArgumentOutOfRangeException(nameof(Condition));
            if (Revision < 0) throw new ArgumentOutOfRangeException(nameof(Revision));
            Optional(PolicyRuleId, 128, nameof(PolicyRuleId));
            Optional(PolicyVersion, 128, nameof(PolicyVersion));
            Require(Reason, 2048, nameof(Reason));
            if (EvaluatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("EvaluatedAtUtc is required.", nameof(EvaluatedAtUtc));
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

    public sealed class AiLearnedResourceReplacementCandidateRequest
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public AgentResourceScope Scope { get; set; }
        public string ProposedScope { get; set; }
        public string Evidence { get; set; }
        public string Provenance { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }
        public decimal Confidence { get; set; }
        public AiLearningCandidateType CandidateType { get; set; }
        public AgentIdentityContext PolicyIdentity { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (!Enum.IsDefined(typeof(AgentResourceScope), Scope)) throw new ArgumentOutOfRangeException(nameof(Scope));
            Require(ProposedScope, 256, nameof(ProposedScope));
            Require(Evidence, 4096, nameof(Evidence));
            Require(Provenance, 4096, nameof(Provenance));
            Optional(SourceExecutionId, 512, nameof(SourceExecutionId));
            Optional(SourceRuntimeInstanceId, 512, nameof(SourceRuntimeInstanceId));
            Optional(SourceAgentProfileId, 512, nameof(SourceAgentProfileId));
            if (Confidence < 0m || Confidence > 1m) throw new ArgumentOutOfRangeException(nameof(Confidence));
            if (!Enum.IsDefined(typeof(AiLearningCandidateType), CandidateType)) throw new ArgumentOutOfRangeException(nameof(CandidateType));
            if (PolicyIdentity == null) throw new ArgumentNullException(nameof(PolicyIdentity));
            PolicyIdentity.Validate();
        }

        private static void Require(string value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength) throw new ArgumentException(name + " is required and bounded.", name);
        }

        private static void Optional(string value, int maxLength, string name)
        {
            if (value != null && value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiLearnedResourceRetentionRequest
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public DateTimeOffset EvaluatedAtUtc { get; set; }
        public DateTimeOffset? LastUsedAtUtc { get; set; }
        public int ValidatedUseCount { get; set; }
        public decimal UtilityScore { get; set; }
        public int ConsecutiveLowUtilityAssessments { get; set; }
        public TimeSpan StaleAfter { get; set; }
        public bool Superseded { get; set; }
        public string SupersedingResourceId { get; set; }
        public bool Contradicted { get; set; }
        public bool ExplicitRetirementRequested { get; set; }
        public bool ExplicitRetentionRequested { get; set; }
        public int AuthorityRank { get; set; }
        public int HighestKnownCompetingAuthorityRank { get; set; }
        public string EvidenceSummary { get; set; }
        public AgentIdentityContext PolicyIdentity { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (EvaluatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("EvaluatedAtUtc is required.", nameof(EvaluatedAtUtc));
            if (LastUsedAtUtc.HasValue && LastUsedAtUtc.Value > EvaluatedAtUtc) throw new ArgumentException("LastUsedAtUtc cannot be after EvaluatedAtUtc.");
            if (ValidatedUseCount < 0) throw new ArgumentOutOfRangeException(nameof(ValidatedUseCount));
            if (UtilityScore < 0m || UtilityScore > 1m) throw new ArgumentOutOfRangeException(nameof(UtilityScore));
            if (ConsecutiveLowUtilityAssessments < 0) throw new ArgumentOutOfRangeException(nameof(ConsecutiveLowUtilityAssessments));
            if (StaleAfter <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(StaleAfter));
            Optional(SupersedingResourceId, 512, nameof(SupersedingResourceId));
            if (AuthorityRank < 0) throw new ArgumentOutOfRangeException(nameof(AuthorityRank));
            if (HighestKnownCompetingAuthorityRank < 0) throw new ArgumentOutOfRangeException(nameof(HighestKnownCompetingAuthorityRank));
            if (ExplicitRetirementRequested && ExplicitRetentionRequested)
                throw new ArgumentException("Explicit retirement and explicit retention cannot both be requested.");
            if (ExplicitRetirementRequested && AuthorityRank < HighestKnownCompetingAuthorityRank)
                throw new InvalidOperationException("A lower-authority resource cannot be explicitly retired while a higher-authority competing resource is known.");
            Require(EvidenceSummary, 4096, nameof(EvidenceSummary));
            if (PolicyIdentity == null) throw new ArgumentNullException(nameof(PolicyIdentity));
            PolicyIdentity.Validate();
        }

        private static void Require(string value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength) throw new ArgumentException(name + " is required and bounded.", name);
        }

        private static void Optional(string value, int maxLength, string name)
        {
            if (value != null && value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiLearnedResourceRetentionResult
    {
        public AiResourceReliabilityIdentity Identity { get; set; }
        public AiLearnedResourceLifecycleStatus PreviousStatus { get; set; }
        public AiLearnedResourceLifecycleStatus Status { get; set; }
        public AiLearnedResourceRetentionDecision Decision { get; set; }
        public decimal UtilityScore { get; set; }
        public bool EligibleForAction { get; set; }
        public bool Restorable { get; set; }
        public long Revision { get; set; }
        public string PolicyRuleId { get; set; }
        public string PolicyVersion { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset EvaluatedAtUtc { get; set; }

        public void Validate()
        {
            if (Identity == null) throw new ArgumentNullException(nameof(Identity));
            Identity.Validate();
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), PreviousStatus)) throw new ArgumentOutOfRangeException(nameof(PreviousStatus));
            if (!Enum.IsDefined(typeof(AiLearnedResourceLifecycleStatus), Status)) throw new ArgumentOutOfRangeException(nameof(Status));
            if (!Enum.IsDefined(typeof(AiLearnedResourceRetentionDecision), Decision)) throw new ArgumentOutOfRangeException(nameof(Decision));
            if (UtilityScore < 0m || UtilityScore > 1m) throw new ArgumentOutOfRangeException(nameof(UtilityScore));
            if (Revision < 0) throw new ArgumentOutOfRangeException(nameof(Revision));
            Optional(PolicyRuleId, 128, nameof(PolicyRuleId));
            Optional(PolicyVersion, 128, nameof(PolicyVersion));
            Require(Reason, 4096, nameof(Reason));
            if (EvaluatedAtUtc == default(DateTimeOffset)) throw new ArgumentException("EvaluatedAtUtc is required.", nameof(EvaluatedAtUtc));
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
}

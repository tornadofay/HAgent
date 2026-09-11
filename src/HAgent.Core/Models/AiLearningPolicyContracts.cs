using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiLearningEvidenceRequirement
    {
        Optional,
        Required
    }

    public enum AiLearningProvenanceRequirement
    {
        Optional,
        Required
    }

    public enum AiLearningContradictionRequirement
    {
        Ignore,
        RequireClear
    }

    public enum AiLearningEvaluationRequirement
    {
        NotRequired,
        RequiredPassed
    }

    public enum AiLearningPromotionAuthorization
    {
        NotPermitted,
        UnifiedPolicyRequired,
        UnifiedPolicyAndReview
    }

    public sealed class AiLearningPolicyRule
    {
        public AiLearningPolicyRule()
        {
            Id = Guid.NewGuid().ToString("N");
            CandidateType = AiLearningCandidateType.Memory;
            AllowedScopes = new List<string>();
            MinimumConfidence = null;
            EvidenceRequirement = AiLearningEvidenceRequirement.Optional;
            ProvenanceRequirement = AiLearningProvenanceRequirement.Optional;
            ContradictionRequirement = AiLearningContradictionRequirement.RequireClear;
            EvaluationRequirement = AiLearningEvaluationRequirement.RequiredPassed;
            RetentionClass = string.Empty;
            PromotionAuthorization = AiLearningPromotionAuthorization.NotPermitted;
            Priority = 0;
        }

        public string Id { get; set; }
        public AiLearningCandidateType CandidateType { get; set; }
        public IList<string> AllowedScopes { get; private set; }
        public decimal? MinimumConfidence { get; set; }
        public AiLearningEvidenceRequirement EvidenceRequirement { get; set; }
        public AiLearningProvenanceRequirement ProvenanceRequirement { get; set; }
        public AiLearningContradictionRequirement ContradictionRequirement { get; set; }
        public AiLearningEvaluationRequirement EvaluationRequirement { get; set; }
        public string RetentionClass { get; set; }
        public AiLearningPromotionAuthorization PromotionAuthorization { get; set; }
        public int Priority { get; set; }

        public AiLearningPolicyRule Clone()
        {
            var clone = new AiLearningPolicyRule
            {
                Id = Id,
                CandidateType = CandidateType,
                MinimumConfidence = MinimumConfidence,
                EvidenceRequirement = EvidenceRequirement,
                ProvenanceRequirement = ProvenanceRequirement,
                ContradictionRequirement = ContradictionRequirement,
                EvaluationRequirement = EvaluationRequirement,
                RetentionClass = RetentionClass,
                PromotionAuthorization = PromotionAuthorization,
                Priority = Priority
            };
            foreach (var scope in AllowedScopes)
                clone.AllowedScopes.Add(scope);
            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 128);
            if (!Enum.IsDefined(typeof(AiLearningCandidateType), CandidateType))
                throw new ArgumentOutOfRangeException(nameof(CandidateType));
            if (AllowedScopes == null || AllowedScopes.Count > 32)
                throw new ArgumentException("AllowedScopes exceeds its bounds.", nameof(AllowedScopes));
            foreach (var scope in AllowedScopes)
                Require(scope, nameof(AllowedScopes), 256);
            if (MinimumConfidence.HasValue && (MinimumConfidence.Value < 0m || MinimumConfidence.Value > 1m))
                throw new ArgumentOutOfRangeException(nameof(MinimumConfidence));
            if (!Enum.IsDefined(typeof(AiLearningEvidenceRequirement), EvidenceRequirement))
                throw new ArgumentOutOfRangeException(nameof(EvidenceRequirement));
            if (!Enum.IsDefined(typeof(AiLearningProvenanceRequirement), ProvenanceRequirement))
                throw new ArgumentOutOfRangeException(nameof(ProvenanceRequirement));
            if (!Enum.IsDefined(typeof(AiLearningContradictionRequirement), ContradictionRequirement))
                throw new ArgumentOutOfRangeException(nameof(ContradictionRequirement));
            if (!Enum.IsDefined(typeof(AiLearningEvaluationRequirement), EvaluationRequirement))
                throw new ArgumentOutOfRangeException(nameof(EvaluationRequirement));
            if (!Enum.IsDefined(typeof(AiLearningPromotionAuthorization), PromotionAuthorization))
                throw new ArgumentOutOfRangeException(nameof(PromotionAuthorization));
            if (RetentionClass != null && RetentionClass.Length > 64)
                throw new ArgumentOutOfRangeException(nameof(RetentionClass));
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
                throw new ArgumentException(name + " is required and bounded.", name);
        }
    }

    public sealed class AiLearningPolicy
    {
        public AiLearningPolicy()
        {
            Id = Guid.NewGuid().ToString("N");
            Version = 1;
            Rules = new List<AiLearningPolicyRule>();
        }

        public string Id { get; set; }
        public long Version { get; set; }
        public IList<AiLearningPolicyRule> Rules { get; private set; }

        public AiLearningPolicy Clone()
        {
            var clone = new AiLearningPolicy { Id = Id, Version = Version };
            foreach (var rule in Rules)
                clone.Rules.Add(rule == null ? null : rule.Clone());
            return clone;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id) || Id.Length > 128)
                throw new ArgumentException("Learning policy Id is required and bounded.");
            if (Version <= 0)
                throw new ArgumentException("Learning policy Version must be positive.");
            if (Rules == null || Rules.Count > 128)
                throw new ArgumentException("Learning policy Rules exceed their bounds.");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in Rules)
            {
                if (rule == null)
                    throw new ArgumentException("Learning policy rules cannot contain null entries.");
                rule.Validate();
                if (!ids.Add(rule.Id))
                    throw new ArgumentException("Learning policy rule Ids must be unique.");
            }
        }

        public AiLearningPolicyDecision Evaluate(AiLearningTypedCandidate candidate)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            Validate();
            candidate.Validate();

            AiLearningPolicyRule selected = null;
            foreach (var rule in Rules)
            {
                if (rule.CandidateType != candidate.Type)
                    continue;
                if (rule.AllowedScopes.Count > 0 && !ContainsIgnoreCase(rule.AllowedScopes, candidate.ProposedScope))
                    continue;
                if (selected == null || rule.Priority > selected.Priority)
                    selected = rule;
            }

            if (selected == null)
                return AiLearningPolicyDecision.Deny(Id, Version, "No learning policy rule authorizes this candidate type and scope.");

            if (selected.MinimumConfidence.HasValue &&
                (!candidate.Confidence.HasValue || candidate.Confidence.Value < selected.MinimumConfidence.Value))
                return AiLearningPolicyDecision.Deny(Id, Version, "Candidate confidence is below the learning policy threshold.");

            if (selected.EvidenceRequirement == AiLearningEvidenceRequirement.Required &&
                string.IsNullOrWhiteSpace(candidate.Evidence))
                return AiLearningPolicyDecision.Deny(Id, Version, "Learning policy requires evidence.");

            if (selected.ProvenanceRequirement == AiLearningProvenanceRequirement.Required &&
                string.IsNullOrWhiteSpace(candidate.Provenance))
                return AiLearningPolicyDecision.Deny(Id, Version, "Learning policy requires provenance.");

            if (selected.ContradictionRequirement == AiLearningContradictionRequirement.RequireClear &&
                !IsClear(candidate.ContradictionState))
                return AiLearningPolicyDecision.Deny(Id, Version, "Learning policy requires contradictions to be clear.");

            if (selected.EvaluationRequirement == AiLearningEvaluationRequirement.RequiredPassed &&
                !string.Equals(candidate.EvaluationState, "Passed", StringComparison.OrdinalIgnoreCase))
                return AiLearningPolicyDecision.Deny(Id, Version, "Learning policy requires a passed evaluation.");

            if (!string.IsNullOrWhiteSpace(selected.RetentionClass) &&
                !string.Equals(selected.RetentionClass, candidate.RetentionClass, StringComparison.OrdinalIgnoreCase))
                return AiLearningPolicyDecision.Deny(Id, Version, "Candidate retention class does not satisfy the learning policy rule.");

            return AiLearningPolicyDecision.Allow(
                Id,
                Version,
                selected.Id,
                selected.PromotionAuthorization,
                "Candidate satisfies the learning policy rule.");
        }

        private static bool ContainsIgnoreCase(IList<string> values, string value)
        {
            foreach (var item in values)
                if (string.Equals(item, value, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static bool IsClear(string contradictionState)
        {
            return string.IsNullOrWhiteSpace(contradictionState) ||
                   string.Equals(contradictionState, "None", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(contradictionState, "Clear", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class AiLearningPolicyDecision
    {
        private AiLearningPolicyDecision() { }

        public string PolicyId { get; private set; }
        public long PolicyVersion { get; private set; }
        public string RuleId { get; private set; }
        public bool IsAllowed { get; private set; }
        public string Reason { get; private set; }
        public AiLearningPromotionAuthorization PromotionAuthorization { get; private set; }

        public static AiLearningPolicyDecision Allow(
            string policyId,
            long policyVersion,
            string ruleId,
            AiLearningPromotionAuthorization promotionAuthorization,
            string reason)
        {
            return new AiLearningPolicyDecision
            {
                PolicyId = policyId,
                PolicyVersion = policyVersion,
                RuleId = ruleId,
                IsAllowed = true,
                Reason = reason,
                PromotionAuthorization = promotionAuthorization
            };
        }

        public static AiLearningPolicyDecision Deny(string policyId, long policyVersion, string reason)
        {
            return new AiLearningPolicyDecision
            {
                PolicyId = policyId,
                PolicyVersion = policyVersion,
                RuleId = string.Empty,
                IsAllowed = false,
                Reason = reason,
                PromotionAuthorization = AiLearningPromotionAuthorization.NotPermitted
            };
        }
    }

    public abstract class AiLearningTypedCandidate
    {
        protected AiLearningTypedCandidate(AiLearningCandidate lifecycle, AiLearningCandidateType expectedType)
        {
            Lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            if (lifecycle.Type != expectedType)
                throw new ArgumentException("Learning candidate type does not match the typed candidate contract.", nameof(lifecycle));
            Confidence = null;
            EvidenceState = string.Empty;
            ProvenanceState = string.Empty;
            ContradictionState = "None";
            RetentionClass = string.Empty;
            EvaluationState = "Unspecified";
        }

        public AiLearningCandidate Lifecycle { get; private set; }
        public AiLearningCandidateType Type { get { return Lifecycle.Type; } }
        public string Id { get { return Lifecycle.Id; } }
        public AiLearningCandidateStatus Status { get { return Lifecycle.Status; } }
        public string ProposedScope { get { return Lifecycle.ProposedScope; } }
        public string Provenance { get { return Lifecycle.Provenance; } }
        public string Evidence { get { return Lifecycle.Evidence; } }
        public decimal? Confidence { get; set; }
        public string EvidenceState { get; set; }
        public string ProvenanceState { get; set; }
        public string ContradictionState { get; set; }
        public string RetentionClass { get; set; }
        public string EvaluationState { get; set; }

        public void Validate()
        {
            if (Lifecycle == null)
                throw new InvalidOperationException("Typed learning candidate lifecycle is required.");
            if (string.IsNullOrWhiteSpace(Lifecycle.Id) || Lifecycle.Id.Length > 512)
                throw new ArgumentException("Learning candidate Id is required and bounded.");
            if (string.IsNullOrWhiteSpace(Lifecycle.ProposedScope) || Lifecycle.ProposedScope.Length > 256)
                throw new ArgumentException("Learning candidate ProposedScope is required and bounded.");
            if (Confidence.HasValue && (Confidence.Value < 0m || Confidence.Value > 1m))
                throw new ArgumentOutOfRangeException(nameof(Confidence));
            RequireOptional(EvidenceState, nameof(EvidenceState), 64);
            RequireOptional(ProvenanceState, nameof(ProvenanceState), 64);
            RequireOptional(ContradictionState, nameof(ContradictionState), 64);
            RequireOptional(RetentionClass, nameof(RetentionClass), 64);
            RequireOptional(EvaluationState, nameof(EvaluationState), 64);
            ValidatePayload();
        }

        public void Approve()
        {
            Lifecycle.Approve();
        }

        public void Reject()
        {
            Lifecycle.Reject();
        }

        public void Promote()
        {
            Lifecycle.Promote();
        }

        protected abstract void ValidatePayload();

        private static void RequireOptional(string value, string name, int maxLength)
        {
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class MemoryCandidate : AiLearningTypedCandidate
    {
        public MemoryCandidate(MemoryEntry memory, AiLearningCandidate lifecycle)
            : base(lifecycle, AiLearningCandidateType.Memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            Memory = memory.Clone();
        }

        public MemoryEntry Memory { get; private set; }

        protected override void ValidatePayload()
        {
            Memory.Validate();
            if (!string.Equals(ProposedScope, Memory.Scope.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Memory candidate scope does not match the proposed candidate scope.");
        }
    }

    public sealed class KnowledgeCandidate : AiLearningTypedCandidate
    {
        public KnowledgeCandidate(AiKnowledgeResource knowledge, AiLearningCandidate lifecycle)
            : base(lifecycle, AiLearningCandidateType.Knowledge)
        {
            if (knowledge == null) throw new ArgumentNullException(nameof(knowledge));
            Knowledge = knowledge.Clone();
        }

        public AiKnowledgeResource Knowledge { get; private set; }

        protected override void ValidatePayload()
        {
            Knowledge.Validate();
            if (Knowledge.IsAuthoritative)
                throw new ArgumentException("Knowledge candidates must not contain an authoritative published resource.");
            if (!string.Equals(ProposedScope, Knowledge.Scope.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Knowledge candidate scope does not match the proposed candidate scope.");
        }
    }

    public sealed class SkillCandidate : AiLearningTypedCandidate
    {
        public SkillCandidate(AiSkillDefinition skill, AiLearningCandidate lifecycle)
            : base(lifecycle, AiLearningCandidateType.Skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            Skill = skill.Clone();
        }

        public AiSkillDefinition Skill { get; private set; }

        protected override void ValidatePayload()
        {
            Skill.Validate();
            if (Skill.IsAuthoritative)
                throw new ArgumentException("Skill candidates must not contain an authoritative published definition.");
            if (!string.Equals(ProposedScope, Skill.Scope.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Skill candidate scope does not match the proposed candidate scope.");
        }
    }
}

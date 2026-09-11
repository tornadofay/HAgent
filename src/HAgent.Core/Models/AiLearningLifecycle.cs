using System;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public sealed class AiLearningLifecycleDecision
    {
        public AiLearningLifecycleDecision()
        {
            CandidateId = string.Empty;
            PolicyId = string.Empty;
            PolicyVersion = 0;
            LearningMode = AiLearningMode.Disabled;
            TargetStatus = AiLearningCandidateStatus.Rejected;
            PromotionAuthorization = AiLearningPromotionAuthorization.NotPermitted;
            Reason = string.Empty;
            AuthorizationDecision = new AiPolicyDecision();
        }

        public string CandidateId { get; set; }
        public string PolicyId { get; set; }
        public long PolicyVersion { get; set; }
        public string RuleId { get; set; }
        public AiLearningMode LearningMode { get; set; }
        public AiLearningCandidateStatus TargetStatus { get; set; }
        public AiLearningPromotionAuthorization PromotionAuthorization { get; set; }
        public AiPolicyDecision AuthorizationDecision { get; set; }
        public bool RequiresReview { get; set; }
        public bool CanProceedToPromotion { get; set; }
        public string Reason { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CandidateId) || CandidateId.Length > 512)
                throw new ArgumentException("Learning lifecycle CandidateId is required and bounded.", nameof(CandidateId));
            if (string.IsNullOrWhiteSpace(PolicyId) || PolicyId.Length > 128)
                throw new ArgumentException("Learning lifecycle PolicyId is required and bounded.", nameof(PolicyId));
            if (PolicyVersion <= 0)
                throw new ArgumentException("Learning lifecycle PolicyVersion must be positive.", nameof(PolicyVersion));
            if (!Enum.IsDefined(typeof(AiLearningMode), LearningMode))
                throw new ArgumentOutOfRangeException(nameof(LearningMode));
            if (!Enum.IsDefined(typeof(AiLearningCandidateStatus), TargetStatus))
                throw new ArgumentOutOfRangeException(nameof(TargetStatus));
            if (!Enum.IsDefined(typeof(AiLearningPromotionAuthorization), PromotionAuthorization))
                throw new ArgumentOutOfRangeException(nameof(PromotionAuthorization));
            if (AuthorizationDecision == null)
                throw new ArgumentNullException(nameof(AuthorizationDecision));
            if (Reason != null && Reason.Length > 2048)
                throw new ArgumentOutOfRangeException(nameof(Reason));
        }
    }

    public static class AiLearningLifecycleCoordinator
    {
        public static AiLearningLifecycleDecision Evaluate(
            AiLearningTypedCandidate candidate,
            AiLearningPolicy learningPolicy,
            AiLearningMode learningMode,
            IAiPolicyEngine authorizationPolicy,
            AgentIdentityContext identity)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (learningPolicy == null) throw new ArgumentNullException(nameof(learningPolicy));
            if (authorizationPolicy == null) throw new ArgumentNullException(nameof(authorizationPolicy));

            AiLearningModePolicy.Validate(learningMode);
            candidate.Validate();
            learningPolicy.Validate();

            if (candidate.Status != AiLearningCandidateStatus.Proposed)
                throw new InvalidOperationException("Learning lifecycle admission requires a Proposed candidate.");

            var learningDecision = learningPolicy.Evaluate(candidate);
            var result = new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = learningDecision.PolicyId,
                PolicyVersion = learningDecision.PolicyVersion,
                RuleId = learningDecision.RuleId,
                LearningMode = learningMode,
                PromotionAuthorization = learningDecision.PromotionAuthorization,
                TargetStatus = AiLearningCandidateStatus.Rejected,
                RequiresReview = false,
                CanProceedToPromotion = false,
                AuthorizationDecision = new AiPolicyDecision
                {
                    Outcome = learningDecision.IsAllowed ? AiPolicyOutcome.Allow : AiPolicyOutcome.Deny,
                    PolicyVersion = learningDecision.PolicyVersion.ToString(),
                    RuleId = learningDecision.RuleId ?? string.Empty,
                    Reason = learningDecision.Reason ?? string.Empty
                },
                Reason = learningDecision.Reason ?? string.Empty
            };

            if (!learningDecision.IsAllowed)
            {
                result.TargetStatus = AiLearningCandidateStatus.Rejected;
                result.AuthorizationDecision.Outcome = AiPolicyOutcome.Deny;
                result.Validate();
                return result;
            }

            if (learningMode == AiLearningMode.Disabled)
            {
                result.TargetStatus = AiLearningCandidateStatus.Rejected;
                result.PromotionAuthorization = AiLearningPromotionAuthorization.NotPermitted;
                result.AuthorizationDecision.Outcome = AiPolicyOutcome.Deny;
                result.Reason = "Learning Mode is Disabled; candidates cannot enter the active learning lifecycle.";
                result.Validate();
                return result;
            }

            if (learningDecision.PromotionAuthorization == AiLearningPromotionAuthorization.NotPermitted)
            {
                result.TargetStatus = AiLearningCandidateStatus.Rejected;
                result.AuthorizationDecision.Outcome = AiPolicyOutcome.Deny;
                result.Reason = "Learning policy does not permit promotion for this candidate.";
                result.Validate();
                return result;
            }

            if (learningMode == AiLearningMode.SuggestOnly)
            {
                result.TargetStatus = AiLearningCandidateStatus.PendingReview;
                result.RequiresReview = true;
                result.AuthorizationDecision.Outcome = AiPolicyOutcome.RequireApproval;
                result.Reason = "SuggestOnly learning requires explicit review before promotion.";
                result.Validate();
                return result;
            }

            var request = new AiLearningPromotionRequest
            {
                CandidateId = candidate.Id,
                CandidateType = candidate.Type,
                ProposedScope = candidate.ProposedScope,
                ConfidenceBand = ToConfidenceBand(candidate.Confidence),
                EvidenceState = candidate.EvidenceState ?? string.Empty,
                ProvenanceState = candidate.ProvenanceState ?? string.Empty,
                ContradictionState = candidate.ContradictionState ?? string.Empty,
                RetentionClass = candidate.RetentionClass ?? string.Empty,
                SourceExecutionId = candidate.Lifecycle.SourceExecutionId,
                SourceRuntimeInstanceId = candidate.Lifecycle.SourceRuntimeInstanceId,
                SourceAgentProfileId = candidate.Lifecycle.SourceAgentProfileId,
                LearningMode = learningMode.ToString(),
                Identity = identity == null ? new AgentIdentityContext() : identity.Clone()
            };

            var authorizationDecision = AiLearningPromotionPolicy.Evaluate(authorizationPolicy, request);
            result.AuthorizationDecision = authorizationDecision;

            if (authorizationDecision.IsDenied || authorizationDecision.Outcome == AiPolicyOutcome.NotApplicable)
            {
                result.TargetStatus = AiLearningCandidateStatus.Rejected;
                result.Reason = authorizationDecision.Outcome == AiPolicyOutcome.NotApplicable
                    ? "No applicable unified policy authorizes learning promotion."
                    : "Unified policy denied learning promotion. " + authorizationDecision.Reason;
                result.Validate();
                return result;
            }

            if (authorizationDecision.RequiresApproval || authorizationDecision.IsDeferred ||
                learningDecision.PromotionAuthorization == AiLearningPromotionAuthorization.UnifiedPolicyAndReview)
            {
                result.TargetStatus = AiLearningCandidateStatus.PendingReview;
                result.RequiresReview = true;
                result.CanProceedToPromotion = false;
                result.Reason = authorizationDecision.IsDeferred
                    ? "Unified policy deferred learning promotion."
                    : "Learning promotion requires explicit review.";
                result.Validate();
                return result;
            }

            result.TargetStatus = AiLearningCandidateStatus.Approved;
            result.CanProceedToPromotion = true;
            result.RequiresReview = false;
            result.Reason = string.IsNullOrWhiteSpace(authorizationDecision.Reason)
                ? "Learning candidate satisfied learning policy and unified authorization."
                : authorizationDecision.Reason;
            result.Validate();
            return result;
        }

        public static AiLearningLifecycleDecision EvaluateAndApply(
            AiLearningTypedCandidate candidate,
            AiLearningPolicy learningPolicy,
            AiLearningMode learningMode,
            IAiPolicyEngine authorizationPolicy,
            AgentIdentityContext identity)
        {
            var decision = Evaluate(candidate, learningPolicy, learningMode, authorizationPolicy, identity);
            Apply(candidate, decision);
            return decision;
        }

        public static void Apply(AiLearningTypedCandidate candidate, AiLearningLifecycleDecision decision)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (decision == null) throw new ArgumentNullException(nameof(decision));
            decision.Validate();
            if (!string.Equals(candidate.Id, decision.CandidateId, StringComparison.Ordinal))
                throw new InvalidOperationException("Learning lifecycle decision does not match the candidate identity.");
            if (candidate.Status != AiLearningCandidateStatus.Proposed)
                throw new InvalidOperationException("Learning lifecycle application requires a Proposed candidate.");

            switch (decision.TargetStatus)
            {
                case AiLearningCandidateStatus.Rejected:
                    candidate.Reject();
                    break;
                case AiLearningCandidateStatus.PendingReview:
                    candidate.Lifecycle.ApplyPolicyDecision(new AiPolicyDecision
                    {
                        Outcome = AiPolicyOutcome.RequireApproval,
                        PolicyVersion = decision.PolicyVersion.ToString(),
                        RuleId = decision.RuleId ?? string.Empty,
                        Reason = decision.Reason ?? string.Empty
                    });
                    break;
                case AiLearningCandidateStatus.Approved:
                    candidate.Lifecycle.ApplyPolicyDecision(new AiPolicyDecision
                    {
                        Outcome = AiPolicyOutcome.Allow,
                        PolicyVersion = decision.PolicyVersion.ToString(),
                        RuleId = decision.RuleId ?? string.Empty,
                        Reason = decision.Reason ?? string.Empty
                    });
                    break;
                default:
                    throw new InvalidOperationException("A learning lifecycle gate may only enter Rejected, PendingReview, or Approved state.");
            }
        }

        private static string ToConfidenceBand(decimal? confidence)
        {
            if (!confidence.HasValue) return string.Empty;
            if (confidence.Value >= 0.85m) return "High";
            if (confidence.Value >= 0.60m) return "Medium";
            return "Low";
        }
    }
}

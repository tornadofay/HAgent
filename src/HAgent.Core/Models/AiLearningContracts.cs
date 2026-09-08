using System;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public enum AiLearningCandidateType
    {
        Memory,
        Knowledge,
        Skill
    }

    public enum AiLearningCandidateStatus
    {
        Proposed,
        PendingReview,
        Approved,
        Rejected,
        Promoted
    }

    public sealed class AiLearningPromotionRequest
    {
        public AiLearningPromotionRequest()
        {
            CandidateId = string.Empty;
            CandidateType = AiLearningCandidateType.Memory;
            ProposedScope = string.Empty;
            ConfidenceBand = string.Empty;
            EvidenceState = string.Empty;
            ProvenanceState = string.Empty;
            ContradictionState = string.Empty;
            RetentionClass = string.Empty;
            SourceExecutionId = string.Empty;
            SourceRuntimeInstanceId = string.Empty;
            SourceAgentProfileId = string.Empty;
            LearningMode = string.Empty;
            Identity = new AgentIdentityContext();
        }

        public string CandidateId { get; set; }
        public AiLearningCandidateType CandidateType { get; set; }
        public string ProposedScope { get; set; }
        public string ConfidenceBand { get; set; }
        public string EvidenceState { get; set; }
        public string ProvenanceState { get; set; }
        public string ContradictionState { get; set; }
        public string RetentionClass { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }
        public string LearningMode { get; set; }
        public AgentIdentityContext Identity { get; set; }

        public void Validate()
        {
            Require(CandidateId, nameof(CandidateId), 512);
            if (!Enum.IsDefined(typeof(AiLearningCandidateType), CandidateType))
                throw new ArgumentOutOfRangeException(nameof(CandidateType));
            Require(ProposedScope, nameof(ProposedScope), 256);
            Require(ConfidenceBand, nameof(ConfidenceBand), 64);
            Require(EvidenceState, nameof(EvidenceState), 64);
            Require(ProvenanceState, nameof(ProvenanceState), 64);
            Require(ContradictionState, nameof(ContradictionState), 64);
            Require(RetentionClass, nameof(RetentionClass), 64);
            Require(SourceExecutionId, nameof(SourceExecutionId), 512, false);
            Require(SourceRuntimeInstanceId, nameof(SourceRuntimeInstanceId), 512, false);
            Require(SourceAgentProfileId, nameof(SourceAgentProfileId), 512, false);
            Require(LearningMode, nameof(LearningMode), 64, false);
            if (Identity != null)
                Identity.Validate();
        }

        public AiPolicyEvaluationContext ToPolicyContext()
        {
            Validate();
            var context = new AiPolicyEvaluationContext
            {
                Operation = "learning.promote",
                ResourceType = "learning-candidate",
                ResourceId = CandidateId,
                AgentProfileId = SourceAgentProfileId,
                RuntimeInstanceId = SourceRuntimeInstanceId,
                ExecutionId = SourceExecutionId,
                Identity = Identity == null ? new AgentIdentityContext() : Identity.Clone()
            };
            context.Attributes["candidateType"] = CandidateType.ToString();
            context.Attributes["proposedScope"] = ProposedScope;
            context.Attributes["confidenceBand"] = ConfidenceBand;
            context.Attributes["evidenceState"] = EvidenceState;
            context.Attributes["provenanceState"] = ProvenanceState;
            context.Attributes["contradictionState"] = ContradictionState;
            context.Attributes["retentionClass"] = RetentionClass;
            if (!string.IsNullOrWhiteSpace(LearningMode))
                context.Attributes["learningMode"] = LearningMode;
            return context;
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public static class AiLearningPromotionPolicy
    {
        public static AiPolicyDecision Evaluate(
            IAiPolicyEngine policyEngine,
            AiLearningPromotionRequest request)
        {
            if (policyEngine == null) throw new ArgumentNullException(nameof(policyEngine));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var decision = policyEngine.Evaluate(request.ToPolicyContext());
            if (decision == null)
                throw new InvalidOperationException("The policy engine returned no learning promotion decision.");
            return decision;
        }
    }

    public sealed class AiLearningCandidate
    {
        private readonly object _sync = new object();
        private long _revision;

        public AiLearningCandidate()
        {
            Id = Guid.NewGuid().ToString("N");
            Type = AiLearningCandidateType.Memory;
            Status = AiLearningCandidateStatus.Proposed;
            ProposedScope = string.Empty;
            Provenance = string.Empty;
            Evidence = string.Empty;
            SourceExecutionId = string.Empty;
            SourceRuntimeInstanceId = string.Empty;
            SourceAgentProfileId = string.Empty;
            _revision = 0;
        }

        public string Id { get; set; }
        public AiLearningCandidateType Type { get; set; }
        public AiLearningCandidateStatus Status { get { lock (_sync) { return _status; } } }
        private AiLearningCandidateStatus _status;
        public long Revision { get { lock (_sync) { return _revision; } } }
        public string ProposedScope { get; set; }
        public string Provenance { get; set; }
        public string Evidence { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }

        public void ApplyPolicyDecision(AiPolicyDecision decision)
        {
            if (decision == null) throw new ArgumentNullException(nameof(decision));

            switch (decision.Outcome)
            {
                case AiPolicyOutcome.Allow:
                    Transition(AiLearningCandidateStatus.Approved);
                    return;
                case AiPolicyOutcome.RequireApproval:
                case AiPolicyOutcome.Defer:
                    Transition(AiLearningCandidateStatus.PendingReview);
                    return;
                case AiPolicyOutcome.Deny:
                    Transition(AiLearningCandidateStatus.Rejected);
                    return;
                default:
                    throw new InvalidOperationException("A NotApplicable learning policy decision cannot authorize promotion.");
            }
        }

        public void Approve()
        {
            EnsureStatus(AiLearningCandidateStatus.PendingReview);
            Transition(AiLearningCandidateStatus.Approved);
        }

        public void Reject()
        {
            lock (_sync)
            {
                if (_status != AiLearningCandidateStatus.Proposed &&
                    _status != AiLearningCandidateStatus.PendingReview &&
                    _status != AiLearningCandidateStatus.Approved)
                    throw new InvalidOperationException("Only proposed, pending-review, or approved learning candidates can be rejected.");
                TransitionLocked(AiLearningCandidateStatus.Rejected);
            }
        }

        public void Promote()
        {
            EnsureStatus(AiLearningCandidateStatus.Approved);
            Transition(AiLearningCandidateStatus.Promoted);
        }

        internal CandidateSnapshot GetSnapshot()
        {
            lock (_sync)
            {
                return new CandidateSnapshot(_status, _revision);
            }
        }

        internal bool TryApplyIntervention(AiInterventionAction action, string expectedState, long expectedRevision, out string reason)
        {
            lock (_sync)
            {
                if (!string.Equals(_status.ToString(), expectedState ?? string.Empty, StringComparison.Ordinal) || _revision != expectedRevision)
                {
                    reason = "The learning candidate changed from " + (expectedState ?? string.Empty) + "/" + expectedRevision + " to " + _status + "/" + _revision + ".";
                    return false;
                }

                try
                {
                    if (action == AiInterventionAction.Approve)
                        TransitionLocked(AiLearningCandidateStatus.Approved);
                    else if (action == AiInterventionAction.Reject)
                        TransitionLocked(AiLearningCandidateStatus.Rejected);
                    else
                    {
                        reason = "Learning candidate intervention supports Approve or Reject, not " + action + ".";
                        return false;
                    }
                    reason = string.Empty;
                    return true;
                }
                catch (InvalidOperationException ex)
                {
                    reason = ex.Message;
                    return false;
                }
            }
        }

        private void Transition(AiLearningCandidateStatus target)
        {
            lock (_sync)
            {
                TransitionLocked(target);
            }
        }

        private void TransitionLocked(AiLearningCandidateStatus target)
        {
            switch (_status)
            {
                case AiLearningCandidateStatus.Proposed:
                    if (target != AiLearningCandidateStatus.PendingReview && target != AiLearningCandidateStatus.Approved && target != AiLearningCandidateStatus.Rejected)
                        throw InvalidTransition(target);
                    break;
                case AiLearningCandidateStatus.PendingReview:
                    if (target != AiLearningCandidateStatus.Approved && target != AiLearningCandidateStatus.Rejected)
                        throw InvalidTransition(target);
                    break;
                case AiLearningCandidateStatus.Approved:
                    if (target != AiLearningCandidateStatus.Promoted && target != AiLearningCandidateStatus.Rejected)
                        throw InvalidTransition(target);
                    break;
                case AiLearningCandidateStatus.Rejected:
                case AiLearningCandidateStatus.Promoted:
                    throw InvalidTransition(target);
                default:
                    throw new InvalidOperationException("Unknown learning candidate status: " + _status.ToString());
            }

            _status = target;
            _revision++;
        }

        private void EnsureStatus(AiLearningCandidateStatus expected)
        {
            lock (_sync)
            {
                if (_status != expected)
                    throw new InvalidOperationException("Learning candidate must be in " + expected + " state but is " + _status + ".");
            }
        }

        private InvalidOperationException InvalidTransition(AiLearningCandidateStatus target)
        {
            return new InvalidOperationException(
                "Invalid learning candidate transition: " + _status + " -> " + target + ".");
        }
    }

    internal sealed class CandidateSnapshot
    {
        public CandidateSnapshot(AiLearningCandidateStatus state, long revision)
        {
            State = state;
            Revision = revision;
        }
        public AiLearningCandidateStatus State { get; private set; }
        public long Revision { get; private set; }
    }
}

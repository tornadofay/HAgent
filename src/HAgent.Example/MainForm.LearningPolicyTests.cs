using System;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningPolicyTab()
        {
            AddApiTab(
                "Learning Policy",
                "Run learning promotion test",
                "Verifies typed learning-promotion requests through the unified policy engine and explicit candidate review/approval/promote/reject transitions.",
                "Learning promotion must remain policy-controlled, preserve candidate provenance and review context, and never allow a model-generated proposal to bypass approval or policy enforcement.",
                "Uses only deterministic in-memory policy evaluation; no learning repository or provider call is used.",
                TestLearningPromotionPolicyAsync,
                "Learning governance",
                "Learning candidates are proposals. Policy decides whether promotion may proceed; typed candidate transitions control review and promotion state.");
        }

        private Task TestLearningPromotionPolicyAsync(string unused)
        {
            var policy = new AiPolicySet { Version = "learning-policy-42" };

            var safePromotion = new AiPolicyRule
            {
                Id = "learning-approved-skill",
                Name = "Approved skill promotion",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "learning-agent-42",
                Priority = 100,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "High-confidence, evidence-backed, provenance-complete skill candidates may proceed to approval."
            };
            safePromotion.Operations.Add("learning.promote");
            safePromotion.ResourceTypes.Add("learning-candidate");
            safePromotion.Attributes["candidateType"] = "Skill";
            safePromotion.Attributes["proposedScope"] = "Agent";
            safePromotion.Attributes["confidenceBand"] = "High";
            safePromotion.Attributes["evidenceState"] = "Strong";
            safePromotion.Attributes["provenanceState"] = "Complete";
            safePromotion.Attributes["contradictionState"] = "None";
            safePromotion.Attributes["retentionClass"] = "Standard";
            policy.Rules.Add(safePromotion);

            var reviewSensitive = new AiPolicyRule
            {
                Id = "learning-review-knowledge",
                Name = "Knowledge review",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "learning-agent-42",
                Priority = 90,
                Outcome = AiPolicyOutcome.RequireApproval,
                Reason = "Knowledge promotion requires explicit review."
            };
            reviewSensitive.Operations.Add("learning.promote");
            reviewSensitive.ResourceTypes.Add("learning-candidate");
            reviewSensitive.Attributes["candidateType"] = "Knowledge";
            reviewSensitive.Attributes["proposedScope"] = "Tenant";
            reviewSensitive.Attributes["confidenceBand"] = "High";
            reviewSensitive.Attributes["evidenceState"] = "Strong";
            reviewSensitive.Attributes["provenanceState"] = "Complete";
            reviewSensitive.Attributes["contradictionState"] = "None";
            reviewSensitive.Attributes["retentionClass"] = "Standard";
            policy.Rules.Add(reviewSensitive);

            var contradictionDeny = new AiPolicyRule
            {
                Id = "learning-contradiction-deny",
                Name = "Contradiction guard",
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = "learning-agent-42",
                Priority = 200,
                Outcome = AiPolicyOutcome.Deny,
                Reason = "Candidates with unresolved contradictions cannot be promoted."
            };
            contradictionDeny.Operations.Add("learning.promote");
            contradictionDeny.ResourceTypes.Add("learning-candidate");
            contradictionDeny.Attributes["contradictionState"] = "Detected";
            policy.Rules.Add(contradictionDeny);

            var engine = new DefaultAiPolicyEngine(policy);

            var skillRequest = new AiLearningPromotionRequest
            {
                CandidateId = "learning-skill-candidate-42",
                CandidateType = AiLearningCandidateType.Skill,
                ProposedScope = "Agent",
                ConfidenceBand = "High",
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                SourceExecutionId = "execution-42",
                SourceRuntimeInstanceId = "runtime-42",
                SourceAgentProfileId = "learning-agent-42",
                LearningMode = "AutomaticWithPolicy",
                Identity = new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42", workspaceId: "workspace-42")
            };

            var allowed = AiLearningPromotionPolicy.Evaluate(engine, skillRequest);
            if (!allowed.IsAllowed || allowed.RuleId != "learning-approved-skill" || allowed.PolicyVersion != "learning-policy-42")
                throw new InvalidOperationException("The safe learning promotion candidate did not receive the expected policy allow decision.");
            if (!allowed.IsAllowed || allowed.RuleId != "learning-approved-skill")
                throw new InvalidOperationException("Learning promotion decision provenance was not preserved.");

            var candidate = new AiLearningCandidate
            {
                Id = skillRequest.CandidateId,
                Type = skillRequest.CandidateType,
                ProposedScope = skillRequest.ProposedScope,
                Provenance = "execution/runtime provenance",
                Evidence = "deterministic evidence",
                SourceExecutionId = skillRequest.SourceExecutionId,
                SourceRuntimeInstanceId = skillRequest.SourceRuntimeInstanceId,
                SourceAgentProfileId = skillRequest.SourceAgentProfileId
            };
            candidate.ApplyPolicyDecision(allowed);
            if (candidate.Status != AiLearningCandidateStatus.Approved)
                throw new InvalidOperationException("An allowed learning promotion did not transition the candidate to Approved.");
            candidate.Promote();
            if (candidate.Status != AiLearningCandidateStatus.Promoted)
                throw new InvalidOperationException("Approved learning candidate did not transition to Promoted.");

            var reviewRequest = new AiLearningPromotionRequest
            {
                CandidateId = "learning-knowledge-candidate-42",
                CandidateType = AiLearningCandidateType.Knowledge,
                ProposedScope = "Tenant",
                ConfidenceBand = "High",
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                SourceExecutionId = "execution-43",
                SourceRuntimeInstanceId = "runtime-42",
                SourceAgentProfileId = "learning-agent-42",
                LearningMode = "SuggestOnly",
                Identity = new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42", workspaceId: "workspace-42")
            };
            var requiresReview = AiLearningPromotionPolicy.Evaluate(engine, reviewRequest);
            if (!requiresReview.RequiresApproval || requiresReview.RuleId != "learning-review-knowledge")
                throw new InvalidOperationException("Review-required knowledge promotion did not receive RequireApproval.");

            var reviewCandidate = new AiLearningCandidate
            {
                Id = reviewRequest.CandidateId,
                Type = reviewRequest.CandidateType,
                ProposedScope = reviewRequest.ProposedScope,
                Provenance = "complete",
                Evidence = "strong",
                SourceExecutionId = reviewRequest.SourceExecutionId,
                SourceRuntimeInstanceId = reviewRequest.SourceRuntimeInstanceId,
                SourceAgentProfileId = reviewRequest.SourceAgentProfileId
            };
            reviewCandidate.ApplyPolicyDecision(requiresReview);
            if (reviewCandidate.Status != AiLearningCandidateStatus.PendingReview)
                throw new InvalidOperationException("RequireApproval did not transition the candidate to PendingReview.");
            reviewCandidate.Approve();
            reviewCandidate.Promote();
            if (reviewCandidate.Status != AiLearningCandidateStatus.Promoted)
                throw new InvalidOperationException("Reviewed and approved candidate did not transition to Promoted.");

            var deniedRequest = new AiLearningPromotionRequest
            {
                CandidateId = "learning-conflict-candidate-42",
                CandidateType = AiLearningCandidateType.Knowledge,
                ProposedScope = "Tenant",
                ConfidenceBand = "High",
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "Detected",
                RetentionClass = "Standard",
                SourceExecutionId = "execution-44",
                SourceRuntimeInstanceId = "runtime-42",
                SourceAgentProfileId = "learning-agent-42",
                Identity = new AgentIdentityContext(tenantId: "tenant-42", userId: "user-42")
            };
            var denied = AiLearningPromotionPolicy.Evaluate(engine, deniedRequest);
            if (!denied.IsDenied || denied.RuleId != "learning-contradiction-deny")
                throw new InvalidOperationException("Contradictory learning candidate was not denied by policy.");

            var deniedCandidate = new AiLearningCandidate { Id = deniedRequest.CandidateId, Type = deniedRequest.CandidateType };
            deniedCandidate.ApplyPolicyDecision(denied);
            if (deniedCandidate.Status != AiLearningCandidateStatus.Rejected)
                throw new InvalidOperationException("Policy denial did not transition the candidate to Rejected.");

            var notApplicableRequest = new AiLearningPromotionRequest
            {
                CandidateId = "learning-unknown-candidate-42",
                CandidateType = AiLearningCandidateType.Memory,
                ProposedScope = "Runtime",
                ConfidenceBand = "Low",
                EvidenceState = "Weak",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                SourceExecutionId = "execution-45",
                SourceRuntimeInstanceId = "runtime-42",
                SourceAgentProfileId = "learning-agent-42"
            };
            var notApplicable = AiLearningPromotionPolicy.Evaluate(engine, notApplicableRequest);
            if (notApplicable.Outcome != AiPolicyOutcome.NotApplicable)
                throw new InvalidOperationException("An unmatched learning promotion request was incorrectly authorized.");

            Write(
                "LEARNING POLICY",
                "Contract test succeeded." + Environment.NewLine +
                "Typed learning promotion request: verified." + Environment.NewLine +
                "Candidate type/scope matching: verified." + Environment.NewLine +
                "Confidence/evidence/provenance policy matching: verified." + Environment.NewLine +
                "Contradiction guard: verified." + Environment.NewLine +
                "Policy version/provenance: verified." + Environment.NewLine +
                "Allow -> Approved -> Promoted: verified." + Environment.NewLine +
                "RequireApproval -> PendingReview -> Approved -> Promoted: verified." + Environment.NewLine +
                "Deny -> Rejected: verified." + Environment.NewLine +
                "Unmatched request -> NotApplicable (no promotion authority): verified.");

            return Task.CompletedTask;
        }
    }
}

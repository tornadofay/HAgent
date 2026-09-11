using System;
using HAgent.Models;
using Xunit;

namespace HAgent.Tests
{
    public sealed class LearningPolicyTests
    {
        [Fact]
        public void TypedCandidatesValidateAgainstTheirCanonicalPayloads()
        {
            var memory = new MemoryEntry
            {
                Scope = MemoryScope.Agent,
                OwnerId = "agent-owner",
                Content = "Customer prefers annual billing.",
                Provenance = new AiMemoryProvenance
                {
                    Source = "LearningPolicyTests",
                    SourceExecutionId = "execution-42",
                    SourceRuntimeInstanceId = "runtime-42",
                    Evidence = "deterministic observation",
                    Confidence = 0.95m
                }
            };

            var lifecycle = NewLifecycle(AiLearningCandidateType.Memory, "Agent");
            var candidate = new MemoryCandidate(memory, lifecycle)
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };

            candidate.Validate();
            Assert.Equal(AiLearningCandidateStatus.Proposed, candidate.Status);
            Assert.Equal("execution-42", candidate.Memory.Provenance.SourceExecutionId);
        }

        [Fact]
        public void LearningPolicyRequiresConfidenceEvidenceProvenanceAndEvaluation()
        {
            var policy = CreatePolicy();
            var candidate = CreateMemoryCandidate(0.90m, "Strong", "Complete", "None", "Standard", "Passed");

            var decision = policy.Evaluate(candidate);

            Assert.True(decision.IsAllowed);
            Assert.Equal("memory-agent", decision.RuleId);
            Assert.Equal(AiLearningPromotionAuthorization.UnifiedPolicyRequired, decision.PromotionAuthorization);
        }

        [Fact]
        public void LearningPolicyRejectsCandidateBelowThreshold()
        {
            var policy = CreatePolicy();
            var candidate = CreateMemoryCandidate(0.40m, "Strong", "Complete", "None", "Standard", "Passed");

            var decision = policy.Evaluate(candidate);

            Assert.False(decision.IsAllowed);
            Assert.Contains("confidence", decision.Reason, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(AiLearningPromotionAuthorization.NotPermitted, decision.PromotionAuthorization);
        }

        [Fact]
        public void LearningPolicyRejectsMissingEvaluationAndContradictions()
        {
            var policy = CreatePolicy();

            var unevaluated = CreateMemoryCandidate(0.90m, "Strong", "Complete", "None", "Standard", "Pending");
            var evaluationDecision = policy.Evaluate(unevaluated);
            Assert.False(evaluationDecision.IsAllowed);
            Assert.Contains("evaluation", evaluationDecision.Reason, StringComparison.OrdinalIgnoreCase);

            var contradictory = CreateMemoryCandidate(0.90m, "Strong", "Complete", "Detected", "Standard", "Passed");
            var contradictionDecision = policy.Evaluate(contradictory);
            Assert.False(contradictionDecision.IsAllowed);
            Assert.Contains("contradictions", contradictionDecision.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void KnowledgeAndSkillCandidatesCannotCarryPublishedAuthoritativePayloads()
        {
            var knowledge = new AiKnowledgeResource
            {
                Id = "knowledge-42",
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-owner",
                Title = "Published knowledge",
                Content = "Authoritative content.",
                Status = AiKnowledgeLifecycleStatus.Published,
                Provenance = new AiKnowledgeProvenance()
            };
            Assert.Throws<ArgumentException>(() => new KnowledgeCandidate(knowledge, NewLifecycle(AiLearningCandidateType.Knowledge, "Agent")).Validate());

            var skill = new AiSkillDefinition
            {
                Id = "skill-42",
                Scope = AgentResourceScope.Agent,
                OwnerId = "agent-owner",
                Name = "Published skill",
                Description = "Authoritative skill.",
                Status = AiSkillLifecycleStatus.Published
            };
            Assert.Throws<ArgumentException>(() => new SkillCandidate(skill, NewLifecycle(AiLearningCandidateType.Skill, "Agent")).Validate());
        }

        [Fact]
        public void TypedCandidateUsesTheExistingLifecycleWithoutCreatingAnotherStateMachine()
        {
            var candidate = CreateMemoryCandidate(0.90m, "Strong", "Complete", "None", "Standard", "Passed");

            Assert.Equal(AiLearningCandidateStatus.Proposed, candidate.Status);
            var reviewDecision = new AiPolicyDecision
            {
                Outcome = AiPolicyOutcome.RequireApproval,
                PolicyVersion = "test",
                RuleId = "test-rule",
                RuleName = "Test rule",
                Reason = "Test approval."
            };
            candidate.Lifecycle.ApplyPolicyDecision(reviewDecision);
            Assert.Equal(AiLearningCandidateStatus.PendingReview, candidate.Status);
            candidate.Approve();
            Assert.Equal(AiLearningCandidateStatus.Approved, candidate.Status);
            candidate.Promote();
            Assert.Equal(AiLearningCandidateStatus.Promoted, candidate.Status);
        }

        private static AiLearningPolicy CreatePolicy()
        {
            var policy = new AiLearningPolicy { Id = "learning-policy-42", Version = 1 };
            policy.Rules.Add(new AiLearningPolicyRule
            {
                Id = "memory-agent",
                CandidateType = AiLearningCandidateType.Memory,
                MinimumConfidence = 0.80m,
                EvidenceRequirement = AiLearningEvidenceRequirement.Required,
                ProvenanceRequirement = AiLearningProvenanceRequirement.Required,
                ContradictionRequirement = AiLearningContradictionRequirement.RequireClear,
                EvaluationRequirement = AiLearningEvaluationRequirement.RequiredPassed,
                RetentionClass = "Standard",
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                Priority = 100
            });
            policy.Rules[0].AllowedScopes.Add("Agent");
            return policy;
        }

        private static MemoryCandidate CreateMemoryCandidate(
            decimal confidence,
            string evidenceState,
            string provenanceState,
            string contradictionState,
            string retentionClass,
            string evaluationState)
        {
            var memory = new MemoryEntry
            {
                Scope = MemoryScope.Agent,
                OwnerId = "agent-owner",
                Content = "Learning candidate content.",
                Provenance = new AiMemoryProvenance
                {
                    Source = "LearningPolicyTests",
                    SourceExecutionId = "execution-42",
                    SourceRuntimeInstanceId = "runtime-42",
                    Evidence = "deterministic evidence",
                    Confidence = confidence
                }
            };
            return new MemoryCandidate(memory, NewLifecycle(AiLearningCandidateType.Memory, "Agent"))
            {
                Confidence = confidence,
                EvidenceState = evidenceState,
                ProvenanceState = provenanceState,
                ContradictionState = contradictionState,
                RetentionClass = retentionClass,
                EvaluationState = evaluationState
            };
        }

        private static AiLearningCandidate NewLifecycle(AiLearningCandidateType type, string scope)
        {
            return new AiLearningCandidate
            {
                Id = "candidate-42",
                Type = type,
                ProposedScope = scope,
                Provenance = "execution/runtime/profile provenance",
                Evidence = "deterministic evidence",
                SourceExecutionId = "execution-42",
                SourceRuntimeInstanceId = "runtime-42",
                SourceAgentProfileId = "agent-profile-42"
            };
        }
    }
}

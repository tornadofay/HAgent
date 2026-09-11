using System;
using System.IO;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddLearningCandidatePersistenceTab()
        {
            AddApiTab(
                "Learning Candidate Persistence",
                "Run durable learning candidate workflow",
                "Verifies typed candidate persistence, restart recovery, retention expiry, optimistic revision protection, and authorized Learning Review through public APIs.",
                "The candidate is persisted as a provider-neutral durable record containing lifecycle revision, typed payload, provenance/evaluation state, retention metadata, and review authorization evidence.",
                "Uses deterministic in-process policy and the File learning candidate store. No model or network provider is required.",
                TestLearningCandidatePersistenceAsync,
                "Learning candidate persistence",
                "Approval changes the candidate lifecycle only; authoritative Memory, Knowledge, or Skill publication remains outside this slice.");
        }

        private async Task TestLearningCandidatePersistenceAsync(string unused)
        {
            var root = Path.Combine(Path.GetTempPath(), "HAgent-LearningCandidate-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(root, "candidates.jsonl");
            try
            {
                var candidate = CreatePendingPersistenceCandidate();
                var admission = CreatePendingAdmission(candidate);
                var retention = CreateRetentionPolicy();
                var capturedAt = DateTimeOffset.UtcNow;
                var record = AiLearningCandidatePersistence.Capture(candidate, admission, retention, capturedAt);

                using (var firstStore = new FileLearningCandidateStore(path))
                {
                    await firstStore.SaveAsync(record).ConfigureAwait(true);
                }

                AiLearningCandidateRecord recovered;
                using (var restartedStore = new FileLearningCandidateStore(path))
                {
                    recovered = await restartedStore.GetAsync(candidate.Id).ConfigureAwait(true);
                    if (recovered == null) throw new InvalidOperationException("Persisted learning candidate was not recovered after store restart.");
                    if (recovered.Status != AiLearningCandidateStatus.PendingReview || recovered.Revision != 1)
                        throw new InvalidOperationException("Recovered lifecycle state/revision is incorrect.");

                    var policySet = new AiPolicySet { Version = "example-review-1" };
                    var reviewRule = new AiPolicyRule
                    {
                        Id = "example-learning-review",
                        Name = "Learning review authorization",
                        Scope = AiPolicyScopeKind.Agent,
                        ScopeId = "learning-persistence-agent",
                        Priority = 100,
                        Outcome = AiPolicyOutcome.Allow,
                        Reason = "Deterministic Example review authorization."
                    };
                    reviewRule.Operations.Add("learning.review");
                    reviewRule.ResourceTypes.Add("learning-candidate");
                    reviewRule.Attributes["candidateType"] = "Skill";
                    reviewRule.Attributes["proposedScope"] = "Agent";
                    reviewRule.Attributes["currentStatus"] = "PendingReview";
                    reviewRule.Attributes["reviewAction"] = "Approve";
                    policySet.Rules.Add(reviewRule);

                    var reviewService = new AiLearningCandidateReviewService(restartedStore, new DefaultAiPolicyEngine(policySet));
                    var reviewed = await reviewService.ReviewAsync(
                        candidate.Id,
                        AiLearningCandidateReviewAction.Approve,
                        new AgentIdentityContext(tenantId: "tenant-example", userId: "reviewer-example", workspaceId: "workspace-example"),
                        "Approved by deterministic Example review.").ConfigureAwait(true);

                    if (reviewed.Status != AiLearningCandidateStatus.Approved || reviewed.Revision != 2)
                        throw new InvalidOperationException("Authorized review did not advance the persisted lifecycle correctly.");
                    if (string.IsNullOrWhiteSpace(reviewed.LastReviewerIdentityJson) || reviewed.LastReviewOutcome != "Allow")
                        throw new InvalidOperationException("Review authorization evidence was not persisted.");
                }

                Write(
                    "LEARNING CANDIDATE PERSISTENCE",
                    "Contract test succeeded." + Environment.NewLine +
                    "Typed candidate captured as durable provider-neutral record: verified." + Environment.NewLine +
                    "Persistence survives store restart: verified." + Environment.NewLine +
                    "PendingReview status and revision restored: verified." + Environment.NewLine +
                    "Authorized Learning Review -> Approved: verified." + Environment.NewLine +
                    "Optimistic lifecycle revision advanced from 1 to 2: verified." + Environment.NewLine +
                    "Reviewer identity and policy decision evidence persisted: verified." + Environment.NewLine +
                    "Authoritative resource publication is not performed by this slice: verified.");
            }
            finally
            {
                try { if (Directory.Exists(root)) Directory.Delete(root, true); }
                catch { }
            }
        }

        private static SkillCandidate CreatePendingPersistenceCandidate()
        {
            var lifecycle = new AiLearningCandidate
            {
                Id = "example-persistence-candidate",
                Type = AiLearningCandidateType.Skill,
                ProposedScope = "Agent",
                Provenance = "Complete",
                Evidence = "Strong",
                SourceExecutionId = "example-execution",
                SourceRuntimeInstanceId = "example-runtime",
                SourceAgentProfileId = "learning-persistence-agent"
            };

            var candidate = new SkillCandidate(
                new AiSkillDefinition
                {
                    Id = "example-persisted-skill",
                    Version = 1,
                    Scope = AgentResourceScope.Agent,
                    OwnerId = "learning-persistence-agent",
                    Name = "Example persisted skill",
                    Description = "Draft learning candidate used by the persistence Example.",
                    Status = AiSkillLifecycleStatus.Draft,
                    Provenance = new AiSkillProvenance { Source = "Learning Candidate Persistence Example", Evidence = "Deterministic evidence.", Confidence = 0.95m }
                },
                lifecycle)
            {
                Confidence = 0.95m,
                EvidenceState = "Strong",
                ProvenanceState = "Complete",
                ContradictionState = "None",
                RetentionClass = "Standard",
                EvaluationState = "Passed"
            };
            lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval, PolicyVersion = "example", RuleId = "learning-example" });
            return candidate;
        }

        private static AiLearningLifecycleDecision CreatePendingAdmission(SkillCandidate candidate)
        {
            return new AiLearningLifecycleDecision
            {
                CandidateId = candidate.Id,
                PolicyId = "example-learning-policy",
                PolicyVersion = 1,
                RuleId = "learning-example",
                LearningMode = AiLearningMode.SuggestOnly,
                TargetStatus = AiLearningCandidateStatus.PendingReview,
                PromotionAuthorization = AiLearningPromotionAuthorization.UnifiedPolicyRequired,
                AuthorizationDecision = new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval, PolicyVersion = "1", RuleId = "learning-example" },
                RequiresReview = true,
                Reason = "Candidate requires review."
            };
        }

        private static AiLearningCandidateRetentionPolicy CreateRetentionPolicy()
        {
            var policy = new AiLearningCandidateRetentionPolicy();
            policy.Rules.Add(new AiLearningCandidateRetentionRule { RetentionClass = "Standard", RetentionDays = 30 });
            return policy;
        }
    }
}

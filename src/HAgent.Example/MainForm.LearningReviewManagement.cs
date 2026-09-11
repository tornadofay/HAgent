using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private const string LearningReviewExampleAgentId = "learning-review-ui-example-agent";
        private const string LearningReviewApproveRuleId = "example-learning-review-ui-approve";
        private const string LearningReviewRejectRuleId = "example-learning-review-ui-reject";
        private const string LearningReviewInputPlaceholder = "Paste the Candidate ID returned by 'Create review candidate'.";

        private string _learningReviewSeedStorePath;
        private string _learningReviewSeedCandidateId;

        private void AddLearningReviewManagementTab()
        {
            AddApiTab(
                "Learning Review Seed",
                "Create review candidate",
                "Creates one real durable PendingReview candidate in the same learning-candidate store used by Configuration → Learning Review and ensures the Example policy authorizes reviewing that candidate.",
                "The output must show Candidate ID, PendingReview status, Revision 1, and the shared candidate-store path. Then open Configuration → Learning Review and review this candidate manually.",
                "No AI request is sent. The candidate is provider-neutral test data.",
                SeedLearningReviewCandidateAsync,
                "Manual review boundary",
                "This is the first half of the UI integration test. Approve or Reject the displayed candidate in Configuration → Learning Review before running the verification example.");

            AddApiTab(
                "Learning Review Verify",
                "Verify review result",
                "Reopens the durable candidate store from a fresh store instance and verifies that the candidate is no longer PendingReview, its lifecycle revision advanced, and review authorization evidence was persisted.",
                "Enter the Candidate ID printed by the seed example. The verifier must report Approved or Rejected, Revision 2, a persisted reviewer identity, and Allow review authorization evidence.",
                LearningReviewInputPlaceholder,
                VerifyLearningReviewResultAsync,
                "Restart / persistence boundary",
                "This is the second half of the UI integration test. It proves that the button click changed durable state rather than merely changing the screen. Leaving the example placeholder unchanged now selects the newest candidate seeded by this Example agent profile.");
        }

        private async Task SeedLearningReviewCandidateAsync(string unused)
        {
            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            var root = options.GetEffectiveRootPath();
            var path = Path.Combine(root, "learning", "candidates.jsonl");
            _learningReviewSeedStorePath = path;

            var candidateId = "example-learning-review-" + Guid.NewGuid().ToString("N");
            _learningReviewSeedCandidateId = candidateId;
            var candidate = CreateLearningReviewExampleCandidate(candidateId);
            var admission = CreatePendingAdmission(candidate);
            var retention = CreateRetentionPolicy();
            var record = AiLearningCandidatePersistence.Capture(candidate, admission, retention, DateTimeOffset.UtcNow);

            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            await EnsureLearningReviewExamplePolicyAsync(store).ConfigureAwait(true);

            using (var candidates = new FileLearningCandidateStore(path))
            {
                await candidates.SaveAsync(record).ConfigureAwait(true);
            }

            Write(
                "LEARNING REVIEW SEED",
                "Created durable PendingReview candidate: verified." + Environment.NewLine +
                "Candidate ID: " + candidateId + Environment.NewLine +
                "Status: " + record.Status + Environment.NewLine +
                "Revision: " + record.Revision + Environment.NewLine +
                "Type: " + record.CandidateType + Environment.NewLine +
                "Source agent profile: " + record.SourceAgentProfileId + Environment.NewLine +
                "Candidate store: " + path + Environment.NewLine +
                "Review policy: " + LearningReviewApproveRuleId + " / " + LearningReviewRejectRuleId + Environment.NewLine +
                "Next: open Configuration → Learning Review and explicitly Approve or Reject this candidate.");
        }

        private async Task VerifyLearningReviewResultAsync(string input)
        {
            var rawInput = input == null ? string.Empty : input.Trim();
            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            var configuredPath = Path.Combine(options.GetEffectiveRootPath(), "learning", "candidates.jsonl");
            var candidateId = rawInput;

            if (string.IsNullOrWhiteSpace(candidateId) ||
                string.Equals(candidateId, LearningReviewInputPlaceholder, StringComparison.OrdinalIgnoreCase))
            {
                candidateId = await ResolveLatestLearningReviewExampleCandidateIdAsync(configuredPath).ConfigureAwait(true);
                if (string.IsNullOrWhiteSpace(candidateId))
                    throw new InvalidOperationException(
                        "No Learning Review Example candidate was found in the configured durable candidate store. Run 'Learning Review Seed' first, then review the candidate in Configuration → Learning Review.");
            }

            var paths = string.Equals(configuredPath, _learningReviewSeedStorePath, StringComparison.OrdinalIgnoreCase)
                ? new[] { configuredPath }
                : new[] { configuredPath, _learningReviewSeedStorePath };

            AiLearningCandidateRecord record = null;
            string resolvedPath = null;
            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                using (var candidates = new FileLearningCandidateStore(path))
                {
                    record = await candidates.GetAsync(candidateId).ConfigureAwait(true);
                }

                if (record != null)
                {
                    resolvedPath = path;
                    break;
                }
            }

            if (record == null)
                throw new InvalidOperationException(
                    "The candidate was not found after reopening the durable learning candidate store." +
                    Environment.NewLine +
                    "Candidate ID: " + candidateId +
                    Environment.NewLine +
                    "Configured candidate store: " + configuredPath +
                    Environment.NewLine +
                    "Seed candidate store: " + (_learningReviewSeedStorePath ?? "<not recorded in this application session>"));

            if (record.Status != AiLearningCandidateStatus.Approved && record.Status != AiLearningCandidateStatus.Rejected)
                throw new InvalidOperationException("The candidate is still PendingReview. Use Configuration → Learning Review to Approve or Reject it first.");
            if (record.Revision != 2)
                throw new InvalidOperationException("The reviewed candidate revision is " + record.Revision + "; expected 2 after the first review.");
            if (string.IsNullOrWhiteSpace(record.LastReviewerIdentityJson))
                throw new InvalidOperationException("Reviewer identity evidence was not persisted.");
            if (!string.Equals(record.LastReviewOutcome, "Allow", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Review authorization evidence was not persisted as Allow.");

            Write(
                "LEARNING REVIEW VERIFY",
                "Fresh store instance reopened: verified." + Environment.NewLine +
                "Candidate ID: " + record.CandidateId + Environment.NewLine +
                "Final status: " + record.Status + Environment.NewLine +
                "Lifecycle revision: " + record.Revision + Environment.NewLine +
                "Reviewer identity persisted: yes" + Environment.NewLine +
                "Policy authorization outcome persisted: " + record.LastReviewOutcome + Environment.NewLine +
                "Candidate store reopened: " + resolvedPath + Environment.NewLine +
                "Durable Learning Review integration: verified.");
        }

        private static async Task<string> ResolveLatestLearningReviewExampleCandidateIdAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            using (var candidates = new FileLearningCandidateStore(path))
            {
                var records = await candidates.QueryAsync(new AiLearningCandidateQuery
                {
                    CandidateType = AiLearningCandidateType.Skill,
                    SourceAgentProfileId = LearningReviewExampleAgentId,
                    IncludeExpired = false,
                    MaxResults = 1
                }).ConfigureAwait(true);

                return records == null || records.Count == 0 ? null : records[0].CandidateId;
            }
        }

        private async Task EnsureLearningReviewExamplePolicyAsync(IAiStore store)
        {
            var policy = await store.GetPolicySetAsync().ConfigureAwait(true) ?? new AiPolicySet();
            if (string.IsNullOrWhiteSpace(policy.Version)) policy.Version = "example-learning-review-1";

            bool changed = false;
            if (!policy.Rules.Any(x => x != null && string.Equals(x.Id, LearningReviewApproveRuleId, StringComparison.OrdinalIgnoreCase)))
            {
                policy.Rules.Add(CreateLearningReviewExampleRule(LearningReviewApproveRuleId, AiLearningCandidateReviewAction.Approve));
                changed = true;
            }

            if (!policy.Rules.Any(x => x != null && string.Equals(x.Id, LearningReviewRejectRuleId, StringComparison.OrdinalIgnoreCase)))
            {
                policy.Rules.Add(CreateLearningReviewExampleRule(LearningReviewRejectRuleId, AiLearningCandidateReviewAction.Reject));
                changed = true;
            }

            if (changed)
                await store.SavePolicySetAsync(policy).ConfigureAwait(true);
        }

        private static AiPolicyRule CreateLearningReviewExampleRule(string id, AiLearningCandidateReviewAction action)
        {
            var rule = new AiPolicyRule
            {
                Id = id,
                Name = "Example Learning Review " + action,
                Scope = AiPolicyScopeKind.Agent,
                ScopeId = LearningReviewExampleAgentId,
                Priority = 100,
                Outcome = AiPolicyOutcome.Allow,
                Reason = "Deterministic Example authorization for the interactive Learning Review test."
            };
            rule.Operations.Add("learning.review");
            rule.ResourceTypes.Add("learning-candidate");
            rule.Attributes["candidateType"] = "Skill";
            rule.Attributes["proposedScope"] = "Agent";
            rule.Attributes["currentStatus"] = "PendingReview";
            rule.Attributes["reviewAction"] = action.ToString();
            return rule;
        }

        private static SkillCandidate CreateLearningReviewExampleCandidate(string candidateId)
        {
            var lifecycle = new AiLearningCandidate
            {
                Id = candidateId,
                Type = AiLearningCandidateType.Skill,
                ProposedScope = "Agent",
                Provenance = "Complete",
                Evidence = "Strong",
                SourceExecutionId = "example-learning-review-execution",
                SourceRuntimeInstanceId = "example-learning-review-runtime",
                SourceAgentProfileId = LearningReviewExampleAgentId
            };

            var candidate = new SkillCandidate(
                new AiSkillDefinition
                {
                    Id = candidateId + "-skill",
                    Version = 1,
                    Scope = AgentResourceScope.Agent,
                    OwnerId = LearningReviewExampleAgentId,
                    Name = "Example Learning Review Skill",
                    Description = "Durable learning candidate created specifically for the WinForms Learning Review integration example.",
                    Status = AiSkillLifecycleStatus.Draft,
                    Provenance = new AiSkillProvenance
                    {
                        Source = "Learning Review Management Example",
                        Evidence = "Deterministic manual review test.",
                        Confidence = 0.95m
                    }
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

            lifecycle.ApplyPolicyDecision(new AiPolicyDecision
            {
                Outcome = AiPolicyOutcome.RequireApproval,
                PolicyVersion = "example",
                RuleId = "example-learning-review-seed"
            });
            return candidate;
        }
    }
}

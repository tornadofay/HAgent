using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public enum AiLearningCandidateReviewAction
    {
        Approve,
        Reject
    }

    public sealed class AiLearningCandidateRetentionRule
    {
        public string RetentionClass { get; set; }
        public int? RetentionDays { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RetentionClass) || RetentionClass.Length > 64)
                throw new ArgumentException("RetentionClass is required and bounded.", nameof(RetentionClass));
            if (RetentionDays.HasValue && (RetentionDays.Value < 0 || RetentionDays.Value > 36500))
                throw new ArgumentOutOfRangeException(nameof(RetentionDays));
        }
    }

    public sealed class AiLearningCandidateRetentionPolicy
    {
        public IList<AiLearningCandidateRetentionRule> Rules { get; private set; }

        public AiLearningCandidateRetentionPolicy()
        {
            Rules = new List<AiLearningCandidateRetentionRule>();
        }

        public DateTimeOffset? CalculateExpiry(string retentionClass, DateTimeOffset capturedAt)
        {
            if (string.IsNullOrWhiteSpace(retentionClass)) return null;
            AiLearningCandidateRetentionRule selected = null;
            foreach (var rule in Rules)
            {
                if (rule == null) continue;
                rule.Validate();
                if (string.Equals(rule.RetentionClass, retentionClass, StringComparison.OrdinalIgnoreCase))
                {
                    selected = rule;
                    break;
                }
            }
            if (selected == null || !selected.RetentionDays.HasValue) return null;
            return capturedAt.AddDays(selected.RetentionDays.Value);
        }
    }

    public sealed class AiLearningCandidateRecord
    {
        public string CandidateId { get; set; }
        public AiLearningCandidateType CandidateType { get; set; }
        public AiLearningCandidateStatus Status { get; set; }
        public long Revision { get; set; }
        public string ProposedScope { get; set; }
        public string Provenance { get; set; }
        public string Evidence { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string SourceAgentProfileId { get; set; }
        public decimal? Confidence { get; set; }
        public string EvidenceState { get; set; }
        public string ProvenanceState { get; set; }
        public string ContradictionState { get; set; }
        public string RetentionClass { get; set; }
        public string EvaluationState { get; set; }
        public string PolicyId { get; set; }
        public long PolicyVersion { get; set; }
        public string PolicyRuleId { get; set; }
        public AiLearningPromotionAuthorization PromotionAuthorization { get; set; }
        public string AuthorizationOutcome { get; set; }
        public string AuthorizationReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public string PayloadJson { get; set; }
        public string LastReviewAction { get; set; }
        public string LastReviewerIdentityJson { get; set; }
        public string LastReviewPolicyVersion { get; set; }
        public string LastReviewRuleId { get; set; }
        public string LastReviewOutcome { get; set; }
        public string LastReviewReason { get; set; }
        public DateTimeOffset? LastReviewedAt { get; set; }

        public bool IsExpired(DateTimeOffset? at = null)
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= (at ?? DateTimeOffset.UtcNow);
        }

        public void Validate()
        {
            Require(CandidateId, nameof(CandidateId), 512);
            if (!Enum.IsDefined(typeof(AiLearningCandidateType), CandidateType)) throw new ArgumentOutOfRangeException(nameof(CandidateType));
            if (!Enum.IsDefined(typeof(AiLearningCandidateStatus), Status)) throw new ArgumentOutOfRangeException(nameof(Status));
            if (Revision < 0) throw new ArgumentOutOfRangeException(nameof(Revision));
            Require(ProposedScope, nameof(ProposedScope), 256);
            Optional(Provenance, 4096, nameof(Provenance));
            Optional(Evidence, 4096, nameof(Evidence));
            Optional(SourceExecutionId, 512, nameof(SourceExecutionId));
            Optional(SourceRuntimeInstanceId, 512, nameof(SourceRuntimeInstanceId));
            Optional(SourceAgentProfileId, 512, nameof(SourceAgentProfileId));
            if (Confidence.HasValue && (Confidence.Value < 0m || Confidence.Value > 1m)) throw new ArgumentOutOfRangeException(nameof(Confidence));
            Optional(EvidenceState, 64, nameof(EvidenceState));
            Optional(ProvenanceState, 64, nameof(ProvenanceState));
            Optional(ContradictionState, 64, nameof(ContradictionState));
            Optional(RetentionClass, 64, nameof(RetentionClass));
            Optional(EvaluationState, 64, nameof(EvaluationState));
            Optional(PolicyId, 128, nameof(PolicyId));
            if (PolicyVersion < 0) throw new ArgumentOutOfRangeException(nameof(PolicyVersion));
            Optional(PolicyRuleId, 128, nameof(PolicyRuleId));
            if (!Enum.IsDefined(typeof(AiLearningPromotionAuthorization), PromotionAuthorization)) throw new ArgumentOutOfRangeException(nameof(PromotionAuthorization));
            Optional(AuthorizationOutcome, 64, nameof(AuthorizationOutcome));
            Optional(AuthorizationReason, 2048, nameof(AuthorizationReason));
            Optional(PayloadJson, 2000000, nameof(PayloadJson));
            Optional(LastReviewAction, 64, nameof(LastReviewAction));
            Optional(LastReviewerIdentityJson, 4096, nameof(LastReviewerIdentityJson));
            Optional(LastReviewPolicyVersion, 128, nameof(LastReviewPolicyVersion));
            Optional(LastReviewRuleId, 128, nameof(LastReviewRuleId));
            Optional(LastReviewOutcome, 64, nameof(LastReviewOutcome));
            Optional(LastReviewReason, 2048, nameof(LastReviewReason));
            if (CreatedAt == default(DateTimeOffset) || UpdatedAt == default(DateTimeOffset) || UpdatedAt < CreatedAt)
                throw new ArgumentException("Candidate timestamps are invalid.");
            if (ExpiresAt.HasValue && ExpiresAt.Value < CreatedAt)
                throw new ArgumentException("Candidate ExpiresAt cannot be earlier than CreatedAt.");
            if (string.IsNullOrWhiteSpace(PayloadJson)) throw new ArgumentException("Candidate payload is required.", nameof(PayloadJson));
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength) throw new ArgumentException(name + " is required and bounded.", name);
        }

        private static void Optional(string value, int maxLength, string name)
        {
            if (value != null && value.Length > maxLength) throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiLearningCandidateQuery
    {
        public AiLearningCandidateStatus? Status { get; set; }
        public AiLearningCandidateType? CandidateType { get; set; }
        public string ProposedScope { get; set; }
        public string SourceAgentProfileId { get; set; }
        public bool IncludeExpired { get; set; }
        public int MaxResults { get; set; }

        public AiLearningCandidateQuery()
        {
            MaxResults = 100;
        }
    }

    public interface IAiLearningCandidateStore
    {
        Task SaveAsync(AiLearningCandidateRecord record, CancellationToken cancellationToken = default(CancellationToken));
        Task<AiLearningCandidateRecord> GetAsync(string candidateId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<AiLearningCandidateRecord>> QueryAsync(AiLearningCandidateQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task<AiLearningCandidateRecord> TryUpdateAsync(AiLearningCandidateRecord record, long expectedRevision, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> PurgeExpiredAsync(DateTimeOffset? at = null, CancellationToken cancellationToken = default(CancellationToken));
    }

    public static class AiLearningCandidatePersistence
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };

        public static AiLearningCandidateRecord Capture(
            AiLearningTypedCandidate candidate,
            AiLearningLifecycleDecision admission,
            AiLearningCandidateRetentionPolicy retentionPolicy,
            DateTimeOffset capturedAt,
            AiLearningCandidateRecord prior = null)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (admission == null) throw new ArgumentNullException(nameof(admission));
            if (retentionPolicy == null) throw new ArgumentNullException(nameof(retentionPolicy));
            candidate.Validate();
            admission.Validate();

            var record = new AiLearningCandidateRecord
            {
                CandidateId = candidate.Id,
                CandidateType = candidate.Type,
                Status = candidate.Status,
                Revision = candidate.Lifecycle.Revision,
                ProposedScope = candidate.ProposedScope,
                Provenance = candidate.Provenance,
                Evidence = candidate.Evidence,
                SourceExecutionId = candidate.Lifecycle.SourceExecutionId,
                SourceRuntimeInstanceId = candidate.Lifecycle.SourceRuntimeInstanceId,
                SourceAgentProfileId = candidate.Lifecycle.SourceAgentProfileId,
                Confidence = candidate.Confidence,
                EvidenceState = candidate.EvidenceState,
                ProvenanceState = candidate.ProvenanceState,
                ContradictionState = candidate.ContradictionState,
                RetentionClass = candidate.RetentionClass,
                EvaluationState = candidate.EvaluationState,
                PolicyId = admission.PolicyId,
                PolicyVersion = admission.PolicyVersion,
                PolicyRuleId = admission.RuleId,
                PromotionAuthorization = admission.PromotionAuthorization,
                AuthorizationOutcome = admission.AuthorizationDecision == null ? string.Empty : admission.AuthorizationDecision.Outcome.ToString(),
                AuthorizationReason = admission.AuthorizationDecision == null ? string.Empty : admission.AuthorizationDecision.Reason,
                CreatedAt = prior == null ? capturedAt : prior.CreatedAt,
                UpdatedAt = capturedAt,
                ExpiresAt = prior != null ? prior.ExpiresAt : retentionPolicy.CalculateExpiry(candidate.RetentionClass, capturedAt),
                PayloadJson = SerializePayload(candidate)
            };

            if (prior != null)
            {
                record.LastReviewAction = prior.LastReviewAction;
                record.LastReviewerIdentityJson = prior.LastReviewerIdentityJson;
                record.LastReviewPolicyVersion = prior.LastReviewPolicyVersion;
                record.LastReviewRuleId = prior.LastReviewRuleId;
                record.LastReviewOutcome = prior.LastReviewOutcome;
                record.LastReviewReason = prior.LastReviewReason;
                record.LastReviewedAt = prior.LastReviewedAt;
            }

            record.Validate();
            return record;
        }

        public static AiLearningTypedCandidate Restore(AiLearningCandidateRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();

            var lifecycle = new AiLearningCandidate
            {
                Id = record.CandidateId,
                Type = record.CandidateType,
                ProposedScope = record.ProposedScope,
                Provenance = record.Provenance ?? string.Empty,
                Evidence = record.Evidence ?? string.Empty,
                SourceExecutionId = record.SourceExecutionId ?? string.Empty,
                SourceRuntimeInstanceId = record.SourceRuntimeInstanceId ?? string.Empty,
                SourceAgentProfileId = record.SourceAgentProfileId ?? string.Empty
            };

            ApplyState(lifecycle, record.Status, record.Revision);

            switch (record.CandidateType)
            {
                case AiLearningCandidateType.Memory:
                    return BuildMemoryCandidate(lifecycle, record);
                case AiLearningCandidateType.Knowledge:
                    return BuildKnowledgeCandidate(lifecycle, record);
                case AiLearningCandidateType.Skill:
                    return BuildSkillCandidate(lifecycle, record);
                default:
                    throw new InvalidOperationException("Unsupported learning candidate type.");
            }
        }

        public static AiLearningCandidateRecord ApplyReview(
            AiLearningCandidateRecord record,
            AiLearningCandidateReviewAction action,
            AgentIdentityContext reviewerIdentity,
            AiPolicyDecision authorization,
            string reason,
            DateTimeOffset reviewedAt)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (reviewerIdentity == null) throw new ArgumentNullException(nameof(reviewerIdentity));
            if (authorization == null) throw new ArgumentNullException(nameof(authorization));
            if (!authorization.IsAllowed) throw new InvalidOperationException("Review authorization must allow the requested review action.");

            var candidate = Restore(record);
            if (candidate.Status != AiLearningCandidateStatus.PendingReview)
                throw new InvalidOperationException("Only PendingReview learning candidates can be reviewed.");

            if (action == AiLearningCandidateReviewAction.Approve) candidate.Approve();
            else if (action == AiLearningCandidateReviewAction.Reject) candidate.Reject();
            else throw new ArgumentOutOfRangeException(nameof(action));

            var updated = new AiLearningCandidateRecord
            {
                CandidateId = record.CandidateId,
                CandidateType = candidate.Type,
                Status = candidate.Status,
                Revision = candidate.Lifecycle.Revision,
                ProposedScope = candidate.ProposedScope,
                Provenance = candidate.Provenance,
                Evidence = candidate.Evidence,
                SourceExecutionId = candidate.Lifecycle.SourceExecutionId,
                SourceRuntimeInstanceId = candidate.Lifecycle.SourceRuntimeInstanceId,
                SourceAgentProfileId = candidate.Lifecycle.SourceAgentProfileId,
                Confidence = candidate.Confidence,
                EvidenceState = candidate.EvidenceState,
                ProvenanceState = candidate.ProvenanceState,
                ContradictionState = candidate.ContradictionState,
                RetentionClass = candidate.RetentionClass,
                EvaluationState = candidate.EvaluationState,
                PolicyId = record.PolicyId,
                PolicyVersion = record.PolicyVersion,
                PolicyRuleId = record.PolicyRuleId,
                PromotionAuthorization = record.PromotionAuthorization,
                AuthorizationOutcome = record.AuthorizationOutcome,
                AuthorizationReason = record.AuthorizationReason,
                CreatedAt = record.CreatedAt,
                UpdatedAt = reviewedAt,
                ExpiresAt = record.ExpiresAt,
                PayloadJson = record.PayloadJson,
                LastReviewAction = action.ToString(),
                LastReviewerIdentityJson = JsonSerializer.Serialize(reviewerIdentity, JsonOptions),
                LastReviewPolicyVersion = authorization.PolicyVersion,
                LastReviewRuleId = authorization.RuleId,
                LastReviewOutcome = authorization.Outcome.ToString(),
                LastReviewReason = reason ?? authorization.Reason,
                LastReviewedAt = reviewedAt
            };
            updated.Validate();
            return updated;
        }

        private static string SerializePayload(AiLearningTypedCandidate candidate)
        {
            switch (candidate.Type)
            {
                case AiLearningCandidateType.Memory:
                    return JsonSerializer.Serialize(((MemoryCandidate)candidate).Memory, JsonOptions);
                case AiLearningCandidateType.Knowledge:
                    return JsonSerializer.Serialize(((KnowledgeCandidate)candidate).Knowledge, JsonOptions);
                case AiLearningCandidateType.Skill:
                    return JsonSerializer.Serialize(((SkillCandidate)candidate).Skill, JsonOptions);
                default:
                    throw new InvalidOperationException("Unsupported learning candidate type.");
            }
        }

        private static MemoryCandidate BuildMemoryCandidate(AiLearningCandidate lifecycle, AiLearningCandidateRecord record)
        {
            var payload = JsonSerializer.Deserialize<MemoryEntry>(record.PayloadJson, JsonOptions);
            var candidate = new MemoryCandidate(payload, lifecycle)
            {
                Confidence = record.Confidence,
                EvidenceState = record.EvidenceState,
                ProvenanceState = record.ProvenanceState,
                ContradictionState = record.ContradictionState,
                RetentionClass = record.RetentionClass,
                EvaluationState = record.EvaluationState
            };
            candidate.Validate();
            return candidate;
        }

        private static KnowledgeCandidate BuildKnowledgeCandidate(AiLearningCandidate lifecycle, AiLearningCandidateRecord record)
        {
            var payload = JsonSerializer.Deserialize<AiKnowledgeResource>(record.PayloadJson, JsonOptions);
            var candidate = new KnowledgeCandidate(payload, lifecycle)
            {
                Confidence = record.Confidence,
                EvidenceState = record.EvidenceState,
                ProvenanceState = record.ProvenanceState,
                ContradictionState = record.ContradictionState,
                RetentionClass = record.RetentionClass,
                EvaluationState = record.EvaluationState
            };
            candidate.Validate();
            return candidate;
        }

        private static SkillCandidate BuildSkillCandidate(AiLearningCandidate lifecycle, AiLearningCandidateRecord record)
        {
            var payload = JsonSerializer.Deserialize<AiSkillDefinition>(record.PayloadJson, JsonOptions);
            var candidate = new SkillCandidate(payload, lifecycle)
            {
                Confidence = record.Confidence,
                EvidenceState = record.EvidenceState,
                ProvenanceState = record.ProvenanceState,
                ContradictionState = record.ContradictionState,
                RetentionClass = record.RetentionClass,
                EvaluationState = record.EvaluationState
            };
            candidate.Validate();
            return candidate;
        }

        private static void ApplyState(AiLearningCandidate lifecycle, AiLearningCandidateStatus status, long revision)
        {
            if (revision == 0 && status == AiLearningCandidateStatus.Proposed) return;
            if (revision == 1)
            {
                if (status == AiLearningCandidateStatus.PendingReview)
                {
                    lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval });
                    return;
                }
                if (status == AiLearningCandidateStatus.Approved)
                {
                    lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.Allow });
                    return;
                }
                if (status == AiLearningCandidateStatus.Rejected)
                {
                    lifecycle.Reject();
                    return;
                }
            }
            if (revision == 2)
            {
                lifecycle.ApplyPolicyDecision(new AiPolicyDecision { Outcome = AiPolicyOutcome.RequireApproval });
                if (status == AiLearningCandidateStatus.Approved) { lifecycle.Approve(); return; }
                if (status == AiLearningCandidateStatus.Rejected) { lifecycle.Reject(); return; }
                if (status == AiLearningCandidateStatus.Promoted) { lifecycle.Approve(); lifecycle.Promote(); return; }
            }
            throw new InvalidOperationException("Persisted learning candidate lifecycle state/revision is inconsistent.");
        }
    }

    public sealed class AiLearningCandidateReviewService
    {
        private readonly IAiLearningCandidateStore _store;
        private readonly IAiPolicyEngine _policy;

        public AiLearningCandidateReviewService(IAiLearningCandidateStore store, IAiPolicyEngine policy)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public async Task<AiLearningCandidateRecord> ReviewAsync(
            string candidateId,
            AiLearningCandidateReviewAction action,
            AgentIdentityContext reviewerIdentity,
            string reason,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(candidateId)) throw new ArgumentException("CandidateId is required.", nameof(candidateId));
            if (reviewerIdentity == null) throw new ArgumentNullException(nameof(reviewerIdentity));
            cancellationToken.ThrowIfCancellationRequested();

            var record = await _store.GetAsync(candidateId, cancellationToken).ConfigureAwait(false);
            if (record == null) throw new KeyNotFoundException("Learning candidate was not found: " + candidateId + ".");
            if (record.Status != AiLearningCandidateStatus.PendingReview)
                throw new InvalidOperationException("Only PendingReview learning candidates can be reviewed.");

            var context = new AiPolicyEvaluationContext
            {
                Operation = "learning.review",
                ResourceType = "learning-candidate",
                ResourceId = record.CandidateId,
                AgentProfileId = record.SourceAgentProfileId ?? string.Empty,
                RuntimeInstanceId = record.SourceRuntimeInstanceId ?? string.Empty,
                ExecutionId = record.SourceExecutionId ?? string.Empty,
                Identity = reviewerIdentity.Clone()
            };
            context.Attributes["candidateType"] = record.CandidateType.ToString();
            context.Attributes["proposedScope"] = record.ProposedScope;
            context.Attributes["currentStatus"] = record.Status.ToString();
            context.Attributes["reviewAction"] = action.ToString();
            context.Validate();

            var authorization = _policy.Evaluate(context);
            if (authorization == null || !authorization.IsAllowed)
                throw new UnauthorizedAccessException("Learning review was not authorized. " + (authorization == null ? string.Empty : authorization.Reason));

            var updated = AiLearningCandidatePersistence.ApplyReview(record, action, reviewerIdentity, authorization, reason, DateTimeOffset.UtcNow);
            return await _store.TryUpdateAsync(updated, record.Revision, cancellationToken).ConfigureAwait(false);
        }
    }
}

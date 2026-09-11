using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AiLearningPromotionService
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> CandidateGates =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        private readonly IAiLearningCandidateStore _candidateStore;
        private readonly IMemoryStore _memoryStore;
        private readonly IAiKnowledgePromotionTarget _knowledgeTarget;
        private readonly IAiSkillPromotionTarget _skillTarget;
        private readonly IAiPolicyEngine _policyEngine;

        public AiLearningPromotionService(
            IAiLearningCandidateStore candidateStore,
            IMemoryStore memoryStore,
            IAiKnowledgePromotionTarget knowledgeTarget,
            IAiSkillPromotionTarget skillTarget,
            IAiPolicyEngine policyEngine)
        {
            _candidateStore = candidateStore ?? throw new ArgumentNullException(nameof(candidateStore));
            _memoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
            _knowledgeTarget = knowledgeTarget ?? throw new ArgumentNullException(nameof(knowledgeTarget));
            _skillTarget = skillTarget ?? throw new ArgumentNullException(nameof(skillTarget));
            _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        }

        public async Task<AiLearningPromotionResult> PromoteAsync(
            string candidateId,
            AgentIdentityContext identity,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(candidateId))
                throw new ArgumentException("CandidateId is required.", nameof(candidateId));
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));
            identity.Validate();

            var gate = CandidateGates.GetOrAdd(candidateId, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await PromoteCoreAsync(candidateId, identity, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                gate.Release();
                SemaphoreSlim ignored;
                CandidateGates.TryRemove(candidateId, out ignored);
            }
        }

        private async Task<AiLearningPromotionResult> PromoteCoreAsync(
            string candidateId,
            AgentIdentityContext identity,
            CancellationToken cancellationToken)
        {
            var record = await _candidateStore.GetAsync(candidateId, cancellationToken).ConfigureAwait(false);
            if (record == null)
                throw new InvalidOperationException("Learning candidate was not found or is expired: " + candidateId + ".");

            record.Validate();
            if (record.IsExpired())
                throw new InvalidOperationException("Expired learning candidates cannot be promoted: " + candidateId + ".");
            if (record.Status != AiLearningCandidateStatus.Approved)
                throw new InvalidOperationException("Only Approved learning candidates can be promoted. Current status: " + record.Status + ".");
            if (record.PromotionAuthorization == AiLearningPromotionAuthorization.NotPermitted)
                throw new UnauthorizedAccessException("The learning candidate is not permitted to reach authoritative promotion.");
            if (record.PromotionAuthorization == AiLearningPromotionAuthorization.UnifiedPolicyAndReview &&
                !string.Equals(record.LastReviewAction, "Approve", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("This learning candidate requires an explicit approved review before promotion.");

            var policyRequest = new AiLearningPromotionRequest
            {
                CandidateId = record.CandidateId,
                CandidateType = record.CandidateType,
                ProposedScope = record.ProposedScope,
                ConfidenceBand = GetConfidenceBand(record.Confidence),
                EvidenceState = record.EvidenceState ?? string.Empty,
                ProvenanceState = record.ProvenanceState ?? string.Empty,
                ContradictionState = record.ContradictionState ?? string.Empty,
                RetentionClass = record.RetentionClass ?? string.Empty,
                SourceExecutionId = record.SourceExecutionId ?? string.Empty,
                SourceRuntimeInstanceId = record.SourceRuntimeInstanceId ?? string.Empty,
                SourceAgentProfileId = record.SourceAgentProfileId ?? string.Empty,
                Identity = identity.Clone()
            };
            var authorization = AiLearningPromotionPolicy.Evaluate(_policyEngine, policyRequest);
            if (!authorization.IsAllowed)
                throw new UnauthorizedAccessException("Learning promotion authorization denied: " + authorization.Reason);

            var candidate = AiLearningCandidatePersistence.Restore(record);
            candidate.Validate();
            var promotedAt = DateTimeOffset.UtcNow;
            var context = new AiLearningPromotionContext
            {
                CandidateId = record.CandidateId,
                Identity = identity.Clone(),
                SourceExecutionId = record.SourceExecutionId,
                SourceRuntimeInstanceId = record.SourceRuntimeInstanceId,
                SourceAgentProfileId = record.SourceAgentProfileId,
                PolicyVersion = ParsePolicyVersion(authorization.PolicyVersion),
                PolicyRuleId = authorization.RuleId,
                PromotedAt = promotedAt
            };
            context.Validate();

            string resourceType;
            string resourceId;
            long? resourceVersion;

            switch (candidate.Type)
            {
                case AiLearningCandidateType.Memory:
                    var memoryCandidate = candidate as MemoryCandidate;
                    if (memoryCandidate == null) throw new InvalidOperationException("Learning candidate payload type mismatch for Memory.");
                    var memory = memoryCandidate.Memory.Clone();
                    memory.Id = Guid.NewGuid().ToString("N");
                    memory.Metadata["learning.candidateId"] = record.CandidateId;
                    memory.Metadata["learning.promotionPolicyRuleId"] = authorization.RuleId ?? string.Empty;
                    memory.Validate();
                    await _memoryStore.AddAsync(memory, cancellationToken).ConfigureAwait(false);
                    resourceType = "memory";
                    resourceId = memory.Id;
                    resourceVersion = null;
                    break;

                case AiLearningCandidateType.Knowledge:
                    var knowledgeCandidate = candidate as KnowledgeCandidate;
                    if (knowledgeCandidate == null) throw new InvalidOperationException("Learning candidate payload type mismatch for Knowledge.");
                    var knowledge = knowledgeCandidate.Knowledge.Clone();
                    knowledge.Metadata["learning.candidateId"] = record.CandidateId;
                    knowledge.Status = AiKnowledgeLifecycleStatus.Draft;
                    knowledge.UpdatedUtc = DateTime.UtcNow;
                    knowledge.Validate();
                    var publishedKnowledge = await _knowledgeTarget.PublishAsync(knowledge, context, cancellationToken).ConfigureAwait(false);
                    if (publishedKnowledge == null) throw new InvalidOperationException("Knowledge promotion target returned no resource.");
                    publishedKnowledge.Validate();
                    if (!publishedKnowledge.IsAuthoritative || !string.Equals(publishedKnowledge.Id, knowledge.Id, StringComparison.OrdinalIgnoreCase) || publishedKnowledge.Version != knowledge.Version)
                        throw new InvalidOperationException("Knowledge promotion target returned an invalid authoritative version.");
                    resourceType = "knowledge";
                    resourceId = publishedKnowledge.Id;
                    resourceVersion = publishedKnowledge.Version;
                    break;

                case AiLearningCandidateType.Skill:
                    var skillCandidate = candidate as SkillCandidate;
                    if (skillCandidate == null) throw new InvalidOperationException("Learning candidate payload type mismatch for Skill.");
                    var skill = skillCandidate.Skill.Clone();
                    skill.Metadata["learning.candidateId"] = record.CandidateId;
                    skill.Status = AiSkillLifecycleStatus.Draft;
                    skill.UpdatedUtc = DateTime.UtcNow;
                    skill.Validate();
                    var publishedSkill = await _skillTarget.PublishAsync(skill, context, cancellationToken).ConfigureAwait(false);
                    if (publishedSkill == null) throw new InvalidOperationException("Skill promotion target returned no definition.");
                    publishedSkill.Validate();
                    if (!publishedSkill.IsAuthoritative || !string.Equals(publishedSkill.Id, skill.Id, StringComparison.OrdinalIgnoreCase) || publishedSkill.Version != skill.Version)
                        throw new InvalidOperationException("Skill promotion target returned an invalid authoritative version.");
                    resourceType = "skill";
                    resourceId = publishedSkill.Id;
                    resourceVersion = publishedSkill.Version;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported learning candidate type.");
            }

            candidate.Promote();
            var promotedRecord = CreatePromotedRecord(record, candidate, promotedAt);
            var persisted = await _candidateStore.TryUpdateAsync(promotedRecord, record.Revision, cancellationToken).ConfigureAwait(false);

            var result = new AiLearningPromotionResult
            {
                CandidateId = persisted.CandidateId,
                CandidateType = persisted.CandidateType,
                CandidateStatus = persisted.Status,
                CandidateRevision = persisted.Revision,
                AuthoritativeResourceType = resourceType,
                AuthoritativeResourceId = resourceId,
                AuthoritativeResourceVersion = resourceVersion,
                PromotedAt = promotedAt,
                PolicyId = persisted.PolicyId,
                PolicyVersion = ParsePolicyVersion(authorization.PolicyVersion),
                PolicyRuleId = authorization.RuleId,
                AuthorizationOutcome = authorization.Outcome.ToString(),
                AuthorizationReason = authorization.Reason,
                Provenance = persisted.Provenance,
                SourceExecutionId = persisted.SourceExecutionId,
                SourceRuntimeInstanceId = persisted.SourceRuntimeInstanceId,
                SourceAgentProfileId = persisted.SourceAgentProfileId
            };
            result.Validate();
            return result;
        }

        private static AiLearningCandidateRecord CreatePromotedRecord(
            AiLearningCandidateRecord source,
            AiLearningTypedCandidate candidate,
            DateTimeOffset promotedAt)
        {
            return new AiLearningCandidateRecord
            {
                CandidateId = source.CandidateId,
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
                PolicyId = source.PolicyId,
                PolicyVersion = source.PolicyVersion,
                PolicyRuleId = source.PolicyRuleId,
                PromotionAuthorization = source.PromotionAuthorization,
                AuthorizationOutcome = source.AuthorizationOutcome,
                AuthorizationReason = source.AuthorizationReason,
                CreatedAt = source.CreatedAt,
                UpdatedAt = promotedAt,
                ExpiresAt = source.ExpiresAt,
                PayloadJson = source.PayloadJson,
                LastReviewAction = source.LastReviewAction,
                LastReviewerIdentityJson = source.LastReviewerIdentityJson,
                LastReviewPolicyVersion = source.LastReviewPolicyVersion,
                LastReviewRuleId = source.LastReviewRuleId,
                LastReviewOutcome = source.LastReviewOutcome,
                LastReviewReason = source.LastReviewReason,
                LastReviewedAt = source.LastReviewedAt
            };
        }

        private static string GetConfidenceBand(decimal? confidence)
        {
            if (!confidence.HasValue) return string.Empty;
            if (confidence.Value >= 0.90m) return "High";
            if (confidence.Value >= 0.70m) return "Medium";
            return "Low";
        }

        private static long ParsePolicyVersion(string value)
        {
            long version;
            return long.TryParse(value, out version) && version >= 0 ? version : 0;
        }
    }
}

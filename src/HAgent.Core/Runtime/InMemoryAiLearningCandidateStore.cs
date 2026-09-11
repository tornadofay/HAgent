using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryAiLearningCandidateStore : IAiLearningCandidateStore
    {
        private readonly ConcurrentDictionary<string, AiLearningCandidateRecord> _records = new ConcurrentDictionary<string, AiLearningCandidateRecord>(StringComparer.OrdinalIgnoreCase);

        public Task SaveAsync(AiLearningCandidateRecord record, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();
            var clone = Clone(record);
            _records[clone.CandidateId] = clone;
            return Task.CompletedTask;
        }

        public Task<AiLearningCandidateRecord> GetAsync(string candidateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(candidateId)) return Task.FromResult<AiLearningCandidateRecord>(null);
            AiLearningCandidateRecord record;
            if (!_records.TryGetValue(candidateId, out record) || record.IsExpired()) return Task.FromResult<AiLearningCandidateRecord>(null);
            return Task.FromResult(Clone(record));
        }

        public Task<IReadOnlyList<AiLearningCandidateRecord>> QueryAsync(AiLearningCandidateQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            query = query ?? new AiLearningCandidateQuery();
            var max = Math.Max(1, Math.Min(query.MaxResults, 1000));
            var records = _records.Values
                .Where(x => query.Status == null || x.Status == query.Status.Value)
                .Where(x => query.CandidateType == null || x.CandidateType == query.CandidateType.Value)
                .Where(x => string.IsNullOrWhiteSpace(query.ProposedScope) || string.Equals(x.ProposedScope, query.ProposedScope, StringComparison.OrdinalIgnoreCase))
                .Where(x => string.IsNullOrWhiteSpace(query.SourceAgentProfileId) || string.Equals(x.SourceAgentProfileId, query.SourceAgentProfileId, StringComparison.OrdinalIgnoreCase))
                .Where(x => query.IncludeExpired || !x.IsExpired())
                .OrderByDescending(x => x.UpdatedAt)
                .Take(max)
                .Select(Clone)
                .ToList();
            return Task.FromResult((IReadOnlyList<AiLearningCandidateRecord>)records.AsReadOnly());
        }

        public Task<AiLearningCandidateRecord> TryUpdateAsync(AiLearningCandidateRecord record, long expectedRevision, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (record == null) throw new ArgumentNullException(nameof(record));
            record.Validate();

            AiLearningCandidateRecord current;
            if (!_records.TryGetValue(record.CandidateId, out current))
                throw new InvalidOperationException("Learning candidate does not exist: " + record.CandidateId + ".");
            if (current.Revision != expectedRevision)
                throw new InvalidOperationException("Learning candidate revision is stale. Expected " + expectedRevision + " but found " + current.Revision + ".");

            if (!_records.TryUpdate(record.CandidateId, Clone(record), current))
                throw new InvalidOperationException("Learning candidate changed concurrently; review the candidate again.");
            return Task.FromResult(Clone(record));
        }

        public Task<int> PurgeExpiredAsync(DateTimeOffset? at = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = at ?? DateTimeOffset.UtcNow;
            var removed = 0;
            foreach (var pair in _records)
            {
                if (!pair.Value.IsExpired(now)) continue;
                if (_records.TryRemove(pair.Key, out _)) removed++;
            }
            return Task.FromResult(removed);
        }

        private static AiLearningCandidateRecord Clone(AiLearningCandidateRecord source)
        {
            return new AiLearningCandidateRecord
            {
                CandidateId = source.CandidateId,
                CandidateType = source.CandidateType,
                Status = source.Status,
                Revision = source.Revision,
                ProposedScope = source.ProposedScope,
                Provenance = source.Provenance,
                Evidence = source.Evidence,
                SourceExecutionId = source.SourceExecutionId,
                SourceRuntimeInstanceId = source.SourceRuntimeInstanceId,
                SourceAgentProfileId = source.SourceAgentProfileId,
                Confidence = source.Confidence,
                EvidenceState = source.EvidenceState,
                ProvenanceState = source.ProvenanceState,
                ContradictionState = source.ContradictionState,
                RetentionClass = source.RetentionClass,
                EvaluationState = source.EvaluationState,
                PolicyId = source.PolicyId,
                PolicyVersion = source.PolicyVersion,
                PolicyRuleId = source.PolicyRuleId,
                PromotionAuthorization = source.PromotionAuthorization,
                AuthorizationOutcome = source.AuthorizationOutcome,
                AuthorizationReason = source.AuthorizationReason,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
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
    }
}

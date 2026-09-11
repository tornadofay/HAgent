using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiLearningCandidateStore
    {
        Task SaveAsync(AiLearningCandidateRecord record, CancellationToken cancellationToken = default(CancellationToken));
        Task<AiLearningCandidateRecord> GetAsync(string candidateId, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<AiLearningCandidateRecord>> QueryAsync(AiLearningCandidateQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task<AiLearningCandidateRecord> TryUpdateAsync(AiLearningCandidateRecord record, long expectedRevision, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> PurgeExpiredAsync(DateTimeOffset? at = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}

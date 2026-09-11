using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Provider-neutral storage boundary for bounded execution observations that may later feed learning analysis.
    /// The store does not create candidates and never mutates authoritative resources.
    /// </summary>
    public interface IAiLearningObservationStore
    {
        Task AppendAsync(AiLearningExecutionObservation observation, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<AiLearningExecutionObservation>> QueryAsync(
            AiLearningObservationQuery query,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

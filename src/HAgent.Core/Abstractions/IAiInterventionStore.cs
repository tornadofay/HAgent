using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Durable persistence boundary for provider-neutral intervention requests.
    /// Implementations persist metadata/state only; executable handlers and authorization callbacks remain runtime-owned.
    /// </summary>
    public interface IAiInterventionStore
    {
        Task<AiInterventionRequest> GetAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<AiInterventionRequest>> SearchAsync(
            AiInterventionQuery query,
            CancellationToken cancellationToken = default(CancellationToken));

        Task CreateAsync(AiInterventionRequest request, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Updates a request only when its current version equals expectedVersion.
        /// Returns false when another writer has already changed the request.
        /// </summary>
        Task<bool> TryUpdateAsync(
            AiInterventionRequest request,
            long expectedVersion,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

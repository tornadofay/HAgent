using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Acquires bounded provider-neutral context candidates and seals them into an execution-owned snapshot.
    /// The acquirer does not authorize sources, rank candidates, compact content, or contact providers.
    /// </summary>
    public interface IContextAcquirer
    {
        Task<ContextSnapshot> AcquireAsync(
            IReadOnlyList<IContextSource> sources,
            ContextSourceRequest request,
            ContextBudget budget,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

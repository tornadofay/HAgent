using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Supplies bounded provider-neutral candidate context.
    /// Implementations own source access and authorization; this contract does not grant either.
    /// </summary>
    public interface IContextSource
    {
        string Id { get; }
        string Kind { get; }

        Task<IReadOnlyList<ContextItem>> GetCandidatesAsync(
            ContextSourceRequest request,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

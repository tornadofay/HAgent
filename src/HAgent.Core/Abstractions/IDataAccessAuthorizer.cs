using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Host callback that authorizes one concrete data operation for a runtime identity and context.
    /// Implementations are runtime-owned and are never persisted as configuration.
    /// </summary>
    public interface IDataAccessAuthorizer
    {
        Task<bool> AuthorizeAsync(DataAuthorizationRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}

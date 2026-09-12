using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Provider-neutral read boundary for one resource selected from the authoritative inventory.
    /// Implementations resolve the actual resource representation without granting edit or publish authority.
    /// </summary>
    public interface IAiResourceDetailSource
    {
        Task<AiResourceDetail> GetAsync(AiResourceInventoryItem resource, CancellationToken cancellationToken = default(CancellationToken));
    }
}
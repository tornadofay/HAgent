using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiResourceInventorySource
    {
        Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(
            AiResourceInventoryQuery query,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IAiResourceInventory
    {
        Task<IReadOnlyList<AiResourceInventoryItem>> ListAsync(
            AiResourceInventoryQuery query,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

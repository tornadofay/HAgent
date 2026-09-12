using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiResourceReliabilityStore
    {
        Task<AiResourceReliabilityRecord> GetAsync(
            AiResourceReliabilityIdentity identity,
            CancellationToken cancellationToken);

        Task<bool> TryCreateAsync(
            AiResourceReliabilityRecord record,
            CancellationToken cancellationToken);

        Task<bool> TryUpdateAsync(
            AiResourceReliabilityRecord record,
            long expectedRevision,
            CancellationToken cancellationToken);
    }
}

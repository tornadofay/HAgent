using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiLearnedResourceLifecycleStore
    {
        Task<AiLearnedResourceLifecycleRecord> GetAsync(
            AiResourceReliabilityIdentity identity,
            CancellationToken cancellationToken);

        Task<bool> TryCreateAsync(
            AiLearnedResourceLifecycleRecord record,
            CancellationToken cancellationToken);

        Task<bool> TryUpdateAsync(
            AiLearnedResourceLifecycleRecord record,
            long expectedRevision,
            CancellationToken cancellationToken);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IMemoryStore
    {
        Task AddAsync(MemoryEntry entry, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<MemoryEntry>> SearchAsync(MemoryQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task RemoveAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken));
        Task ClearAsync(string scope, string ownerId, CancellationToken cancellationToken = default(CancellationToken));
    }
}

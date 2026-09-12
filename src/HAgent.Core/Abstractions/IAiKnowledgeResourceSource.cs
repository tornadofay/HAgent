using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IAiKnowledgeResourceSource
    {
        Task<IReadOnlyList<AiKnowledgeResource>> ListAsync(
            AiKnowledgeEnumerationQuery query,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

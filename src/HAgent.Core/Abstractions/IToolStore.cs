using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IToolStore
    {
        Task<IReadOnlyList<AiTool>> GetToolsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SaveToolAsync(AiTool tool, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteToolAsync(string toolId, CancellationToken cancellationToken = default(CancellationToken));
    }
}

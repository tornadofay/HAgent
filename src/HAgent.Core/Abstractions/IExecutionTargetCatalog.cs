using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Builds provider-neutral concrete execution targets from current provider metadata.
    /// </summary>
    public interface IExecutionTargetCatalog
    {
        Task<IReadOnlyList<AiExecutionTarget>> GetTargetsAsync(
            IReadOnlyList<AiProvider> providers,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Composes policy admission, deterministic ranking, and bounded compaction into one provider-neutral context result.
    /// </summary>
    public interface IContextAssembler
    {
        Task<ContextAssemblyResult> AssembleAsync(
            IReadOnlyList<ContextRetrievalSource> sources,
            ContextBudget budget,
            ContextAdmissionContext admissionContext,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}

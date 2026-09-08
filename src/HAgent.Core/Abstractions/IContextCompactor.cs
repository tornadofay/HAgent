using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IContextCompactor
    {
        ContextCompactionResult Compact(
            IReadOnlyList<ContextItem> rankedCandidates,
            ContextCompactionOptions options = null);
    }
}

using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    /// <summary>
    /// Ranks and deterministically prioritizes provider-neutral context candidates.
    /// </summary>
    public interface IContextRanker
    {
        IReadOnlyList<ContextItem> Rank(
            IReadOnlyList<ContextItem> candidates,
            ContextRankingOptions options = null);
    }
}

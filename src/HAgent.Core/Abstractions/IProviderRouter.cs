using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IProviderRouter
    {
        IReadOnlyList<AiProvider> OrderProviders(AiAgent agent, IReadOnlyList<AiProvider> providers);
    }
}

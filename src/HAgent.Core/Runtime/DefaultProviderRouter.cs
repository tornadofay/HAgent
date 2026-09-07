using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class DefaultProviderRouter : IProviderRouter
    {
        public IReadOnlyList<AiProvider> OrderProviders(AiAgent agent, IReadOnlyList<AiProvider> providers)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));
            if (providers == null) throw new ArgumentNullException(nameof(providers));

            // Provider ordering is no longer derived from agent-owned provider IDs.
            // Agent selection is resolved by the execution planner from concrete targets.
            return providers
                .Where(x => x != null && x.Enabled)
                .ToList()
                .AsReadOnly();
        }
    }
}

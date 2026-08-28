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

            var ids = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.ProviderId)) ids.Add(agent.ProviderId);
            if (agent.ProviderIds != null)
            {
                foreach (var id in agent.ProviderIds)
                    if (!string.IsNullOrWhiteSpace(id) && !ids.Any(x => string.Equals(x, id, StringComparison.OrdinalIgnoreCase)))
                        ids.Add(id);
            }

            var ordered = new List<AiProvider>();
            foreach (var id in ids)
            {
                var provider = providers.FirstOrDefault(x => x != null && string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
                if (provider != null && provider.Enabled) ordered.Add(provider);
            }

            return ordered.AsReadOnly();
        }
    }
}

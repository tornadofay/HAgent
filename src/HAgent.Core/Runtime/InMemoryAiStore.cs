using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryAiStore : IAiStore
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, AiProvider> _providers = new Dictionary<string, AiProvider>();
        private readonly Dictionary<string, AiAgent> _agents = new Dictionary<string, AiAgent>();

        public Task<IReadOnlyList<AiProvider>> GetProvidersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) return Task.FromResult<IReadOnlyList<AiProvider>>(_providers.Values.Select(Clone).ToList().AsReadOnly());
        }

        public Task<IReadOnlyList<AiAgent>> GetAgentsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) return Task.FromResult<IReadOnlyList<AiAgent>>(_agents.Values.Select(Clone).ToList().AsReadOnly());
        }

        public Task SaveProviderAsync(AiProvider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) _providers[provider.Id] = Clone(provider);
            return Task.CompletedTask;
        }

        public Task SaveAgentAsync(AiAgent agent, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) _agents[agent.Id] = Clone(agent);
            return Task.CompletedTask;
        }

        public Task DeleteProviderAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync)
            {
                if (_agents.Values.Any(x => string.Equals(x.ProviderId, providerId, StringComparison.OrdinalIgnoreCase) ||
                                             (x.ProviderIds != null && x.ProviderIds.Any(id => string.Equals(id, providerId, StringComparison.OrdinalIgnoreCase)))))
                    throw new InvalidOperationException("Provider cannot be deleted while an agent references it.");
                _providers.Remove(providerId);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAgentAsync(string agentId, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_sync) _agents.Remove(agentId);
            return Task.CompletedTask;
        }

        private static AiProvider Clone(AiProvider x) => new AiProvider
        {
            Id = x.Id, Name = x.Name, Kind = x.Kind, BaseUrl = x.BaseUrl, DefaultModel = x.DefaultModel,
            DefaultSystemPrompt = x.DefaultSystemPrompt, SecretId = x.SecretId, Enabled = x.Enabled
        };

        private static AiAgent Clone(AiAgent x) => new AiAgent
        {
            Id = x.Id, Name = x.Name, ProviderId = x.ProviderId,
            ProviderIds = x.ProviderIds == null ? new List<string>() : new List<string>(x.ProviderIds),
            Model = x.Model, SystemPrompt = x.SystemPrompt,
            UseProviderSystemPrompt = x.UseProviderSystemPrompt, Temperature = x.Temperature,
            MaxOutputTokens = x.MaxOutputTokens, ToolIds = x.ToolIds == null ? new List<string>() : new List<string>(x.ToolIds), Enabled = x.Enabled
        };
    }
}

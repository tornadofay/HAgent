using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class HAgentClient
    {
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
        }

        public async Task<AIResponse> SendAsync(string agentId, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

            var agent = (await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false)).FirstOrDefault(x => x.Id == agentId);
            if (agent == null) throw new InvalidOperationException("Agent was not found: " + agentId);
            if (!agent.Enabled) throw new InvalidOperationException("Agent is disabled: " + agent.Name);

            var providers = await _store.GetProvidersAsync(cancellationToken).ConfigureAwait(false);
            var providerIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.ProviderId)) providerIds.Add(agent.ProviderId);
            if (agent.ProviderIds != null)
                providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));

            foreach (var providerId in providerIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var provider = providers.FirstOrDefault(x => x.Id == providerId);
                if (provider == null || !provider.Enabled) continue;

                var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
                if (adapter == null) continue;

                try
                {
                    var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                        ? string.Empty
                        : await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(false);

                    var systemPrompt = BuildSystemPrompt(provider, agent);
                    var messages = new List<AIMessage> { new AIMessage("user", message) };
                    if (!string.IsNullOrWhiteSpace(systemPrompt))
                        messages.Insert(0, new AIMessage("system", systemPrompt));

                    return await adapter.SendAsync(provider, agent, apiKey, systemPrompt, messages, cancellationToken).ConfigureAwait(false);
                }
                catch when (!cancellationToken.IsCancellationRequested)
                {
                    // Try the next configured provider. The last failure will be surfaced below.
                }
            }

            throw new InvalidOperationException("No enabled and compatible provider could handle agent: " + agent.Name);
        }

        public AgentSession CreateSession(string agentId)
        {
            return new AgentSession(agentId, (message, token) => SendAsync(agentId, message, token));
        }

        private static string BuildSystemPrompt(AiProvider provider, AiAgent agent)
        {
            var providerPrompt = agent.UseProviderSystemPrompt ? provider.DefaultSystemPrompt : string.Empty;
            var agentPrompt = agent.SystemPrompt;

            if (string.IsNullOrWhiteSpace(providerPrompt)) return agentPrompt ?? string.Empty;
            if (string.IsNullOrWhiteSpace(agentPrompt)) return providerPrompt;
            return providerPrompt.Trim() + Environment.NewLine + Environment.NewLine + agentPrompt.Trim();
        }
    }
}

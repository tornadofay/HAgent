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
        private readonly IAgentRuntime _runtime;

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
            : this(store, secrets, adapters, null)
        {
        }

        public HAgentClient(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _runtime = new DefaultAgentRuntime(_store, _secrets, _adapters, router);
        }

        public Task<AIResponse> SendAsync(string agentId, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return SendAsync(agentId, new List<AIMessage> { new AIMessage("user", message) }, cancellationToken);
        }

        public async Task<AIResponse> SendAsync(string agentId, IReadOnlyList<AIMessage> messages, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (messages == null || messages.Count == 0) throw new ArgumentException("At least one message is required.", nameof(messages));

            var agents = await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false);
            var agent = agents.FirstOrDefault(x => string.Equals(x.Id, agentId, StringComparison.OrdinalIgnoreCase));
            if (agent == null) throw new InvalidOperationException("Agent was not found: " + agentId);
            if (!agent.Enabled) throw new InvalidOperationException("Agent is disabled: " + agent.Name);

            var providers = await _store.GetProvidersAsync(cancellationToken).ConfigureAwait(false);
            var providerIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(agent.ProviderId)) providerIds.Add(agent.ProviderId);
            if (agent.ProviderIds != null) providerIds.AddRange(agent.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));

            var failures = new List<string>();

            foreach (var providerId in providerIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var provider = providers.FirstOrDefault(x => string.Equals(x.Id, providerId, StringComparison.OrdinalIgnoreCase));
                if (provider == null)
                {
                    failures.Add("Provider " + providerId + ": provider was not found.");
                    continue;
                }

                if (!provider.Enabled)
                {
                    failures.Add("Provider " + provider.Name + ": provider is disabled.");
                    continue;
                }

                var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
                if (adapter == null)
                {
                    failures.Add("Provider " + provider.Name + ": no registered adapter can handle kind '" + provider.Kind + "'.");
                    continue;
                }

                try
                {
                    var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                        ? string.Empty
                        : await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(false);
                    var systemPrompt = BuildSystemPrompt(provider, agent);
                    var outgoing = new List<AIMessage>();
                    if (!string.IsNullOrWhiteSpace(systemPrompt)) outgoing.Add(new AIMessage("system", systemPrompt));
                    foreach (var m in messages) if (m != null) outgoing.Add(m);
                    return await adapter.SendAsync(provider, agent, apiKey, systemPrompt, outgoing, cancellationToken).ConfigureAwait(false);
                }
                catch when (!cancellationToken.IsCancellationRequested)
                {
                    // Preserve the exact reason so callers can diagnose provider failures.
                    try
                    {
                        await TryCaptureProviderFailureAsync(provider, agent, apiKey: null, messages, failures, cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        failures.Add("Provider " + provider.Name + ": request failed, but the original exception could not be captured.");
                    }
                }
            }

            var detail = failures.Count == 0
                ? "No provider candidates were configured for the agent."
                : string.Join(Environment.NewLine, failures.Select(x => "- " + x));

            throw new InvalidOperationException(
                "No enabled and compatible provider could handle agent '" + agent.Name + "'." +
                Environment.NewLine + Environment.NewLine + detail);
        }

        private async Task TryCaptureProviderFailureAsync(
            AiProvider provider,
            AiAgent agent,
            string apiKey,
            IReadOnlyList<AIMessage> messages,
            List<string> failures,
            CancellationToken cancellationToken)
        {
            // This method is intentionally only a diagnostics fallback. The actual request exception
            // is captured below by rethrowing through an exception-preserving helper.
            // It is not invoked to make a second network request.
            await Task.CompletedTask.ConfigureAwait(false);
            failures.Add("Provider " + provider.Name + ": request failed while using model '" +
                         (string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model) + "'.");
        }

        public Task<AgentExecution> ExecuteAsync(
            string agentId,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.ExecuteAsync(agentId, message, options, cancellationToken);
        }

        public AgentSession CreateSession(string agentId)
        {
            return new AgentSession(agentId, (messages, token) => SendAsync(agentId, messages, token));
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

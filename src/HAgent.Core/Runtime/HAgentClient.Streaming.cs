using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        public Task<AIResponse> StreamAsync(
            string agentId,
            string message,
            IProgress<AIResponseDelta> progress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return StreamAsync(
                agentId,
                new List<AIMessage> { new AIMessage("user", message) },
                progress,
                cancellationToken);
        }

        public async Task<AIResponse> StreamAsync(
            string agentId,
            IReadOnlyList<AIMessage> messages,
            IProgress<AIResponseDelta> progress,
            CancellationToken cancellationToken = default(CancellationToken))
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
            var contextMessages = _contextBuilder.Build(messages);

            foreach (var providerId in providerIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var provider = providers.FirstOrDefault(x => string.Equals(x.Id, providerId, StringComparison.OrdinalIgnoreCase));
                if (provider == null) { failures.Add("Provider " + providerId + ": provider was not found."); continue; }
                if (!provider.Enabled) { failures.Add("Provider " + provider.Name + ": provider is disabled."); continue; }

                var adapter = _adapters.FirstOrDefault(x => x.CanHandle(provider));
                if (adapter == null) { failures.Add("Provider " + provider.Name + ": no registered adapter can handle kind '" + provider.Kind + "'."); continue; }

                var streamingAdapter = adapter as IProviderStreamingAdapter;
                if (streamingAdapter == null)
                {
                    failures.Add("Provider " + provider.Name + ": adapter does not support streaming.");
                    continue;
                }

                try
                {
                    var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                        ? string.Empty
                        : await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(false);
                    var selectedModel = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model;
                    var capabilities = await GetEffectiveCapabilitiesAsync(provider, selectedModel, adapter, apiKey, cancellationToken).ConfigureAwait(false);
                    if (capabilities.Get(AiCapability.Streaming) == CapabilitySupport.Unsupported)
                    {
                        failures.Add("Provider " + provider.Name + ": model '" + selectedModel + "' is not marked as supporting Streaming.");
                        continue;
                    }

                    return await streamingAdapter.SendStreamingAsync(
                        new ProviderExecutionRequest
                        {
                            Provider = provider,
                            Agent = agent,
                            ApiKey = apiKey,
                            SystemPrompt = BuildSystemPrompt(provider, agent, null),
                            Messages = contextMessages,
                            Progress = progress
                        },
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    var kind = ProviderErrorAdvisor.InferKind(ex);
                    var selectedModel = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model;
                    var detail = ProviderErrorAdvisor.GetActionableMessage(kind, provider.Name, selectedModel, ex.Message);
                    failures.Add("Provider " + provider.Name + " (" + provider.Kind + ") [" + kind + "]: " + detail);
                }
            }

            var detailText = failures.Count == 0
                ? "No provider candidates were configured for the agent."
                : string.Join(Environment.NewLine, failures.Select(x => "- " + x));
            throw new InvalidOperationException(
                "No enabled and streaming-compatible provider could handle agent '" + agent.Name + "'." +
                Environment.NewLine + Environment.NewLine + detailText);
        }
    }
}

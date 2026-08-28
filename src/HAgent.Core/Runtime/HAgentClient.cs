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
        private readonly IMemoryStore _memory;
        private readonly IConversationStore _conversations;

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
            : this(store, secrets, adapters, null, null, null)
        {
        }

        public HAgentClient(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router)
            : this(store, secrets, adapters, router, null, null)
        {
        }

        public HAgentClient(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router,
            IMemoryStore memory)
            : this(store, secrets, adapters, router, memory, null)
        {
        }

        public HAgentClient(
            IAiStore store,
            ISecretStore secrets,
            IEnumerable<IAiProviderAdapter> adapters,
            IProviderRouter router,
            IMemoryStore memory,
            IConversationStore conversations)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _memory = memory;
            _conversations = conversations;
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

                    return await adapter.SendAsync(
                        provider,
                        agent,
                        apiKey,
                        systemPrompt,
                        messages,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    var messageText = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
                    failures.Add("Provider " + provider.Name + " (" + provider.Kind + "): " + ex.GetType().Name + ": " + messageText);
                }
            }

            var detail = failures.Count == 0
                ? "No provider candidates were configured for the agent."
                : string.Join(Environment.NewLine, failures.Select(x => "- " + x));

            throw new InvalidOperationException(
                "No enabled and compatible provider could handle agent '" + agent.Name + "'." +
                Environment.NewLine + Environment.NewLine + detail);
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
            return CreateSession(agentId, Guid.NewGuid().ToString("N"), null, null);
        }

        public AgentSession CreateSession(string agentId, string sessionId)
        {
            return CreateSession(agentId, sessionId, _conversations, null);
        }

        public AgentSession CreateSession(string agentId, string sessionId, IConversationStore conversationStore)
        {
            return CreateSession(agentId, sessionId, conversationStore, null);
        }

        private AgentSession CreateSession(string agentId, string sessionId, IConversationStore conversationStore, IReadOnlyList<AIMessage> initialMessages)
        {
            return new AgentSession(agentId, sessionId, (messages, token) => SendAsync(agentId, messages, token), conversationStore, initialMessages);
        }

        public async Task<AgentSession> OpenSessionAsync(string agentId, string sessionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_conversations == null)
                throw new InvalidOperationException("No conversation store is configured for this HAgentClient.");
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session id is required.", nameof(sessionId));

            var snapshot = await _conversations.LoadAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (snapshot == null)
                return CreateSession(agentId, sessionId, _conversations, null);

            if (!string.Equals(snapshot.AgentId, agentId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Conversation belongs to a different agent: " + snapshot.AgentId);

            return CreateSession(agentId, sessionId, _conversations, snapshot.Messages);
        }

        public async Task<string> RememberAsync(
            string ownerId,
            string content,
            MemoryScope scope = MemoryScope.Agent,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureMemoryStore();
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Memory owner ID is required.", nameof(ownerId));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Memory content is required.", nameof(content));

            var entry = new MemoryEntry
            {
                Scope = scope,
                OwnerId = ownerId,
                Content = content.Trim(),
                Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _memory.AddAsync(entry, cancellationToken).ConfigureAwait(false);
            return entry.Id;
        }

        public Task<IReadOnlyList<MemoryEntry>> RecallAsync(
            string ownerId,
            string text,
            MemoryScope? scope = null,
            int maxResults = 10,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureMemoryStore();
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Memory owner ID is required.", nameof(ownerId));

            return _memory.SearchAsync(new MemoryQuery
            {
                OwnerId = ownerId,
                Scope = scope,
                Text = text ?? string.Empty,
                MaxResults = maxResults,
                Metadata = metadata
            }, cancellationToken);
        }

        public Task ForgetAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureMemoryStore();
            return _memory.RemoveAsync(memoryId, cancellationToken);
        }

        private void EnsureMemoryStore()
        {
            if (_memory == null)
                throw new InvalidOperationException("No memory store is configured for this HAgentClient.");
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

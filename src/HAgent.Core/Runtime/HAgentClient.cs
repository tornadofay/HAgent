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
        private readonly IAiStore _store;
        private readonly ISecretStore _secrets;
        private readonly IReadOnlyList<IAiProviderAdapter> _adapters;
        private readonly IAgentRuntime _runtime;
        private readonly IMemoryStore _memory;
        private readonly IConversationStore _conversations;
        private readonly ConversationContextBuilder _contextBuilder;
        private readonly IConversationMemoryPolicy _memoryPolicy;
        private readonly AiModelCapabilityCache _capabilityCache = new AiModelCapabilityCache();

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters)
            : this(store, secrets, adapters, null, null, null, null, null) { }

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IProviderRouter router)
            : this(store, secrets, adapters, router, null, null, null, null) { }

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IProviderRouter router, IMemoryStore memory)
            : this(store, secrets, adapters, router, memory, null, null, null) { }

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IProviderRouter router, IMemoryStore memory, IConversationStore conversations)
            : this(store, secrets, adapters, router, memory, conversations, null, null) { }

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IProviderRouter router, IMemoryStore memory, IConversationStore conversations, ConversationContextOptions contextOptions)
            : this(store, secrets, adapters, router, memory, conversations, contextOptions, null) { }

        public HAgentClient(IAiStore store, ISecretStore secrets, IEnumerable<IAiProviderAdapter> adapters, IProviderRouter router, IMemoryStore memory, IConversationStore conversations, ConversationContextOptions contextOptions, IConversationMemoryPolicy memoryPolicy)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));
            _adapters = (adapters ?? throw new ArgumentNullException(nameof(adapters))).ToList().AsReadOnly();
            _memory = memory;
            _conversations = conversations;
            _contextBuilder = new ConversationContextBuilder(contextOptions);
            _memoryPolicy = memoryPolicy ?? (_memory == null ? null : new ExplicitConversationMemoryPolicy());
            _runtime = new DefaultAgentRuntime(_store, _secrets, _adapters, router);
        }

        public ConversationContextOptions ContextOptions { get { return _contextBuilder.Options; } }
        public bool AutomaticMemoryEnabled { get { return _memory != null && _memoryPolicy != null; } }

        public Task<AIResponse> SendAsync(string agentId, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return SendAsync(agentId, new List<AIMessage> { new AIMessage("user", message) }, null, cancellationToken);
        }

        public Task<AIResponse> SendAsync(string agentId, string message, AgentExecutionOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return SendAsync(agentId, new List<AIMessage> { new AIMessage("user", message) }, options, cancellationToken);
        }

        public Task<AIResponse> SendAsync(string agentId, IReadOnlyList<AIMessage> messages, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(agentId, messages, null, cancellationToken);
        }

        public async Task<AIResponse> SendAsync(string agentId, IReadOnlyList<AIMessage> messages, AgentExecutionOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (messages == null || messages.Count == 0) throw new ArgumentException("At least one message is required.", nameof(messages));

            var agents = await _store.GetAgentsAsync(cancellationToken).ConfigureAwait(false);
            var agent = agents.FirstOrDefault(x => string.Equals(x.Id, agentId, StringComparison.OrdinalIgnoreCase));
            if (agent == null) throw new InvalidOperationException("Agent was not found: " + agentId);
            if (!agent.Enabled) throw new InvalidOperationException("Agent is disabled: " + agent.Name);

            options = options ?? new AgentExecutionOptions();

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

                try
                {
                    var apiKey = string.IsNullOrWhiteSpace(provider.SecretId) ? string.Empty : await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(false);
                    var selectedModel = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model;
                    var capabilities = await GetEffectiveCapabilitiesAsync(provider, selectedModel, adapter, apiKey, cancellationToken).ConfigureAwait(false);
                    if (capabilities.Get(AiCapability.Chat) == CapabilitySupport.Unsupported)
                    {
                        failures.Add("Provider " + provider.Name + ": model '" + selectedModel + "' is not marked as supporting Chat.");
                        continue;
                    }

                    return await adapter.SendAsync(provider, agent, apiKey, BuildSystemPrompt(provider, agent, options.SystemPromptLayers), contextMessages, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    var kind = ProviderErrorAdvisor.InferKind(ex);
                    var detail = ProviderErrorAdvisor.GetActionableMessage(kind, provider.Name, string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model, ex.Message);
                    failures.Add("Provider " + provider.Name + " (" + provider.Kind + ") [" + kind + "]: " + detail);
                }
            }

            var detailText = failures.Count == 0 ? "No provider candidates were configured for the agent." : string.Join(Environment.NewLine, failures.Select(x => "- " + x));
            throw new InvalidOperationException("No enabled and compatible provider could handle agent '" + agent.Name + "'." + Environment.NewLine + Environment.NewLine + detailText);
        }

        public Task<AgentExecution> ExecuteAsync(string agentId, string message, AgentExecutionOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _runtime.ExecuteAsync(agentId, message, options, cancellationToken);
        }

        public async Task<AiModelCapabilities> GetModelCapabilitiesAsync(string providerId, string model = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(providerId)) throw new ArgumentException("Provider id is required.", nameof(providerId));
            var providers = await _store.GetProvidersAsync(cancellationToken).ConfigureAwait(false);
            var provider = providers.FirstOrDefault(x => string.Equals(x.Id, providerId, StringComparison.OrdinalIgnoreCase));
            if (provider == null) throw new InvalidOperationException("Provider was not found: " + providerId);
            if (!provider.Enabled) throw new InvalidOperationException("Provider is disabled: " + provider.Name);

            var adapter = _adapters.FirstOrDefault(x => x is IProviderModelCapabilities && x.CanHandle(provider));
            var selectedModel = string.IsNullOrWhiteSpace(model) ? provider.DefaultModel : model;
            if (adapter == null) return new AiModelCapabilities { Model = selectedModel ?? string.Empty };

            var apiKey = string.IsNullOrWhiteSpace(provider.SecretId)
                ? string.Empty
                : await _secrets.GetAsync(provider.SecretId, cancellationToken).ConfigureAwait(false);

            return await GetEffectiveCapabilitiesAsync(provider, selectedModel, adapter, apiKey, cancellationToken).ConfigureAwait(false);
        }

        public void ClearModelCapabilityCache() { _capabilityCache.Clear(); }

        private Task<AiModelCapabilities> GetEffectiveCapabilitiesAsync(AiProvider provider, string model, IAiProviderAdapter adapter, string apiKey, CancellationToken cancellationToken)
        {
            var capabilitiesAdapter = adapter as IProviderModelCapabilities;
            if (capabilitiesAdapter == null)
                return Task.FromResult(new AiModelCapabilities { Model = model ?? string.Empty });

            var selectedModel = model ?? string.Empty;
            var key = provider.Kind + "|" + provider.Id + "|" + provider.BaseUrl + "|" + selectedModel;
            return _capabilityCache.GetOrCreateAsync(key, () => capabilitiesAdapter.GetCapabilitiesAsync(provider, selectedModel, apiKey, CancellationToken.None), cancellationToken);
        }

        private AgentSession CreateSession(string agentId, string sessionId, IConversationStore conversationStore, IReadOnlyList<AIMessage> initialMessages)
        {
            return new AgentSession(agentId, sessionId, (messages, token) => SendAsync(agentId, messages, null, token), conversationStore, initialMessages, null, _memory, _memoryPolicy);
        }

        public AgentSession CreateSession(string agentId) { return CreateSession(agentId, Guid.NewGuid().ToString("N"), null, null); }
        public AgentSession CreateSession(string agentId, string sessionId) { return CreateSession(agentId, sessionId, _conversations, null); }
        public AgentSession CreateSession(string agentId, string sessionId, IConversationStore conversationStore) { return CreateSession(agentId, sessionId, conversationStore, null); }

        public async Task<AgentSession> OpenSessionAsync(string agentId, string sessionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_conversations == null) throw new InvalidOperationException("No conversation store is configured for this HAgentClient.");
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Session id is required.", nameof(sessionId));
            var snapshot = await _conversations.LoadAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (snapshot == null) return CreateSession(agentId, sessionId, _conversations, null);
            if (!string.Equals(snapshot.AgentId, agentId, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Conversation belongs to a different agent: " + snapshot.AgentId);
            return CreateSession(agentId, sessionId, _conversations, snapshot.Messages);
        }

        public Task<string> RememberAsync(string ownerId, string content, MemoryScope scope = MemoryScope.Agent, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return RememberTypedAsync(ownerId, string.Empty, content, MemoryKind.Fact, scope, metadata, DateTimeOffset.UtcNow, cancellationToken);
        }

        public Task<string> RememberTaskEventAsync(string ownerId, string taskId, string content, MemoryKind kind = MemoryKind.Event, IDictionary<string, string> metadata = null, DateTimeOffset? occurredAt = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (kind != MemoryKind.Task && kind != MemoryKind.Event) throw new ArgumentException("Task event memory must use MemoryKind.Task or MemoryKind.Event.", nameof(kind));
            return RememberTypedAsync(ownerId, taskId, content, kind, MemoryScope.Task, metadata, occurredAt ?? DateTimeOffset.UtcNow, cancellationToken);
        }

        public Task<IReadOnlyList<MemoryEntry>> RecallTaskEventsAsync(string ownerId, string taskId, string text = null, int maxResults = 10, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureMemoryStore();
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Memory owner ID is required.", nameof(ownerId));
            if (string.IsNullOrWhiteSpace(taskId)) throw new ArgumentException("Task ID is required.", nameof(taskId));
            return _memory.SearchAsync(new MemoryQuery { OwnerId = ownerId, Scope = MemoryScope.Task, TaskId = taskId, Text = text ?? string.Empty, MaxResults = maxResults }, cancellationToken);
        }

        private async Task<string> RememberTypedAsync(string ownerId, string taskId, string content, MemoryKind kind, MemoryScope scope, IDictionary<string, string> metadata, DateTimeOffset occurredAt, CancellationToken cancellationToken)
        {
            EnsureMemoryStore();
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Memory owner ID is required.", nameof(ownerId));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Memory content is required.", nameof(content));
            var entry = new MemoryEntry
            {
                Scope = scope,
                Kind = kind,
                OwnerId = ownerId,
                TaskId = taskId ?? string.Empty,
                Content = content.Trim(),
                Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                CreatedAt = DateTimeOffset.UtcNow,
                OccurredAt = occurredAt
            };
            await _memory.AddAsync(entry, cancellationToken).ConfigureAwait(false);
            return entry.Id;
        }

        public Task<IReadOnlyList<MemoryEntry>> RecallAsync(string ownerId, string text, MemoryScope? scope = null, int maxResults = 10, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureMemoryStore();
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Memory owner ID is required.", nameof(ownerId));
            return _memory.SearchAsync(new MemoryQuery { OwnerId = ownerId, Scope = scope, Text = text ?? string.Empty, MaxResults = maxResults, Metadata = metadata }, cancellationToken);
        }

        public Task ForgetAsync(string memoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureMemoryStore();
            return _memory.RemoveAsync(memoryId, cancellationToken);
        }

        private void EnsureMemoryStore() { if (_memory == null) throw new InvalidOperationException("No memory store is configured for this HAgentClient."); }

        private static string BuildSystemPrompt(AiProvider provider, AiAgent agent, IEnumerable<SystemPromptLayer> executionLayers)
        {
            var layers = new List<SystemPromptLayer>();
            if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
                layers.Add(new SystemPromptLayer("provider", "Provider", provider.DefaultSystemPrompt, 100));

            if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
                layers.Add(new SystemPromptLayer("agent", "Agent", agent.SystemPrompt, 200));

            if (executionLayers != null)
                layers.AddRange(executionLayers);

            return SystemPromptComposer.Compose(layers);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;

namespace HAgent.Models
{
    public sealed class AgentSession
    {
        private readonly Func<IReadOnlyList<AIMessage>, CancellationToken, Task<AIResponse>> _send;
        private readonly IConversationStore _conversationStore;
        private readonly IMemoryStore _memoryStore;
        private readonly IConversationMemoryPolicy _memoryPolicy;
        private readonly List<AIMessage> _messages;
        private readonly DateTimeOffset _createdAt;
        private DateTimeOffset _updatedAt;

        internal AgentSession(
            string agentId,
            string sessionId,
            Func<IReadOnlyList<AIMessage>, CancellationToken, Task<AIResponse>> send,
            IConversationStore conversationStore = null,
            IReadOnlyList<AIMessage> initialMessages = null,
            DateTimeOffset? createdAt = null,
            IMemoryStore memoryStore = null,
            IConversationMemoryPolicy memoryPolicy = null,
            string memoryOwnerId = null)
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Session id is required.", nameof(sessionId));

            AgentId = agentId;
            SessionId = sessionId;
            MemoryOwnerId = string.IsNullOrWhiteSpace(memoryOwnerId) ? agentId : memoryOwnerId;
            _send = send ?? throw new ArgumentNullException(nameof(send));
            _conversationStore = conversationStore;
            _memoryStore = memoryStore;
            _memoryPolicy = memoryPolicy;
            _messages = initialMessages == null ? new List<AIMessage>() : new List<AIMessage>(initialMessages);
            _createdAt = createdAt ?? DateTimeOffset.UtcNow;
            _updatedAt = _createdAt;
        }

        public string AgentId { get; private set; }
        public string SessionId { get; private set; }
        public string MemoryOwnerId { get; private set; }
        public IReadOnlyList<AIMessage> Messages { get { return _messages.AsReadOnly(); } }
        public bool IsPersistent { get { return _conversationStore != null; } }
        public bool HasAutomaticMemory { get { return _memoryStore != null && _memoryPolicy != null; } }

        public async Task<AIResponse> SendAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

            var originalCount = _messages.Count;
            _messages.Add(new AIMessage("user", message));
            try
            {
                var response = await _send(_messages.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                if (response == null) throw new InvalidOperationException("The provider returned no response.");

                _messages.Add(new AIMessage("assistant", response.Text));
                _updatedAt = DateTimeOffset.UtcNow;

                await SaveMemoriesAsync(_messages[_messages.Count - 2], _messages[_messages.Count - 1], cancellationToken).ConfigureAwait(false);
                await SaveAsync(cancellationToken).ConfigureAwait(false);
                return response;
            }
            catch
            {
                while (_messages.Count > originalCount)
                    _messages.RemoveAt(_messages.Count - 1);
                throw;
            }
        }

        public Task<AIReadResult> ReadAsync()
        {
            return Task.FromResult(new AIReadResult(_messages.AsReadOnly()));
        }

        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_conversationStore == null) return Task.CompletedTask;
            return _conversationStore.DeleteAsync(SessionId, cancellationToken);
        }

        private async Task SaveMemoriesAsync(AIMessage userMessage, AIMessage assistantMessage, CancellationToken cancellationToken)
        {
            if (_memoryStore == null || _memoryPolicy == null) return;

            var candidates = _memoryPolicy.ExtractMemories(userMessage, assistantMessage);
            if (candidates == null || candidates.Count == 0) return;

            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;

                var entry = new MemoryEntry
                {
                    Scope = MemoryScope.Agent,
                    OwnerId = MemoryOwnerId,
                    Content = candidate.Trim(),
                    Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "source", "conversation" },
                        { "policy", _memoryPolicy.GetType().Name },
                        { "sessionId", SessionId }
                    },
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _memoryStore.AddAsync(entry, cancellationToken).ConfigureAwait(false);
            }
        }

        internal async Task SaveAsync(CancellationToken cancellationToken)
        {
            if (_conversationStore == null) return;

            var snapshot = new ConversationSnapshot
            {
                SessionId = SessionId,
                AgentId = AgentId,
                Messages = new List<AIMessage>(_messages),
                CreatedAt = _createdAt,
                UpdatedAt = _updatedAt
            };

            await _conversationStore.SaveAsync(snapshot, cancellationToken).ConfigureAwait(false);
        }
    }
}

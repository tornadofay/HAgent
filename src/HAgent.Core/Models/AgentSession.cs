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
        private readonly List<AIMessage> _messages;
        private readonly DateTimeOffset _createdAt;
        private DateTimeOffset _updatedAt;

        internal AgentSession(
            string agentId,
            string sessionId,
            Func<IReadOnlyList<AIMessage>, CancellationToken, Task<AIResponse>> send,
            IConversationStore conversationStore = null,
            IReadOnlyList<AIMessage> initialMessages = null,
            DateTimeOffset? createdAt = null)
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("Session id is required.", nameof(sessionId));

            AgentId = agentId;
            SessionId = sessionId;
            _send = send ?? throw new ArgumentNullException(nameof(send));
            _conversationStore = conversationStore;
            _messages = initialMessages == null ? new List<AIMessage>() : new List<AIMessage>(initialMessages);
            _createdAt = createdAt ?? DateTimeOffset.UtcNow;
            _updatedAt = _createdAt;
        }

        public string AgentId { get; private set; }
        public string SessionId { get; private set; }
        public IReadOnlyList<AIMessage> Messages { get { return _messages.AsReadOnly(); } }
        public bool IsPersistent { get { return _conversationStore != null; } }

        public async Task<AIResponse> SendAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

            _messages.Add(new AIMessage("user", message));
            try
            {
                var response = await _send(_messages.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                if (response == null) throw new InvalidOperationException("The provider returned no response.");

                _messages.Add(new AIMessage("assistant", response.Text));
                _updatedAt = DateTimeOffset.UtcNow;
                await SaveAsync(cancellationToken).ConfigureAwait(false);
                return response;
            }
            catch
            {
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public sealed class AgentSession
    {
        private readonly Func<string, CancellationToken, Task<AIResponse>> _send;
        private readonly List<AIMessage> _messages = new List<AIMessage>();

        internal AgentSession(string agentId, Func<string, CancellationToken, Task<AIResponse>> send)
        {
            AgentId = agentId;
            _send = send;
        }

        public string AgentId { get; }
        public IReadOnlyList<AIMessage> Messages => _messages.AsReadOnly();

        public async Task<AIResponse> SendAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required.", nameof(message));

            _messages.Add(new AIMessage("user", message));
            var response = await _send(message, cancellationToken).ConfigureAwait(false);
            _messages.Add(new AIMessage("assistant", response.Text));
            return response;
        }

        public Task<AIReadResult> ReadAsync()
        {
            return Task.FromResult(new AIReadResult(_messages.AsReadOnly()));
        }
    }
}

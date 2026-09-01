using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        /// <summary>
        /// Creates a session owned by a runtime instance for automatic memory isolation.
        /// </summary>
        public AgentSession CreateSession(AgentRuntimeInstance instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime agent instance is retired: " + instance.InstanceId);

            return CreateSession(
                instance.ProfileId,
                Guid.NewGuid().ToString("N"),
                _conversations,
                null,
                instance.MemoryOwnerId);
        }

        /// <summary>
        /// Opens or creates a persistent session owned by a runtime instance.
        /// </summary>
        public async Task<AgentSession> OpenSessionAsync(
            AgentRuntimeInstance instance,
            string sessionId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime agent instance is retired: " + instance.InstanceId);
            if (_conversations == null)
                throw new InvalidOperationException("No conversation store is configured for this HAgentClient.");
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session id is required.", nameof(sessionId));

            var snapshot = await _conversations.LoadAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (snapshot == null)
                return CreateSession(instance.ProfileId, sessionId, _conversations, null, instance.MemoryOwnerId);

            if (!string.Equals(snapshot.AgentId, instance.ProfileId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Conversation belongs to a different agent: " + snapshot.AgentId);

            return CreateSession(instance.ProfileId, sessionId, _conversations, snapshot.Messages, instance.MemoryOwnerId);
        }

        /// <summary>
        /// Stores an explicit memory entry under the runtime instance's independent memory owner.
        /// </summary>
        public Task<string> RememberAsync(
            AgentRuntimeInstance instance,
            string content,
            MemoryScope scope = MemoryScope.Agent,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime agent instance is retired: " + instance.InstanceId);

            return RememberAsync(instance.MemoryOwnerId, content, scope, metadata, cancellationToken);
        }

        /// <summary>
        /// Recalls memory belonging only to the runtime instance's independent memory owner.
        /// </summary>
        public Task<IReadOnlyList<MemoryEntry>> RecallAsync(
            AgentRuntimeInstance instance,
            string text,
            MemoryScope? scope = null,
            int maxResults = 10,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Runtime agent instance is retired: " + instance.InstanceId);

            return RecallAsync(instance.MemoryOwnerId, text, scope, maxResults, metadata, cancellationToken);
        }
    }
}

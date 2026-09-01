using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class AgentRuntimeStatePersistence
    {
        private readonly IAgentRuntimeStateStore _store;

        public AgentRuntimeStatePersistence(IAgentRuntimeStateStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Task SaveAsync(
            AgentRuntimeInstance instance,
            string hostInstanceId = null,
            string userId = null,
            string workspaceId = null,
            string sessionId = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var record = AgentRuntimeStateRecord.FromInstance(instance, hostInstanceId, userId, workspaceId, sessionId);
            return _store.SaveAsync(record, cancellationToken);
        }

        public async Task<AgentRuntimeInstance> RestoreAsync(
            AiAgent profile,
            string instanceId,
            AgentRuntimeOverrides overrides = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(instanceId));

            var record = await _store.GetAsync(instanceId, cancellationToken).ConfigureAwait(false);
            if (record == null) return null;
            return AgentRuntimeInstance.Restore(profile, record, overrides);
        }

        public Task DeleteAsync(string instanceId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _store.DeleteAsync(instanceId, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Thread-safe owner for live runtime-agent instances.
    /// Persistent agent profiles remain owned by the configured store.
    /// </summary>
    public sealed class AgentRuntimeInstanceRegistry
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, AgentRuntimeInstance> _instances =
            new Dictionary<string, AgentRuntimeInstance>(StringComparer.OrdinalIgnoreCase);

        public AgentRuntimeInstance Create(AiAgent profile, AgentRuntimeScope scope = AgentRuntimeScope.Ephemeral)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var instance = AgentRuntimeInstance.Create(profile, scope);
            lock (_sync)
            {
                _instances.Add(instance.InstanceId, instance);
            }

            return instance;
        }

        public bool TryGet(string instanceId, out AgentRuntimeInstance instance)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                instance = null;
                return false;
            }

            lock (_sync)
            {
                return _instances.TryGetValue(instanceId, out instance);
            }
        }

        public IReadOnlyList<AgentRuntimeInstance> GetActiveInstances()
        {
            var result = new List<AgentRuntimeInstance>();
            lock (_sync)
            {
                foreach (var instance in _instances.Values)
                {
                    if (instance.State == AgentRuntimeInstanceState.Active)
                        result.Add(instance);
                }
            }

            return result.AsReadOnly();
        }

        public bool Retire(string instanceId)
        {
            AgentRuntimeInstance instance;
            if (!TryGet(instanceId, out instance))
                return false;

            instance.Retire();
            return true;
        }

        public bool RemoveRetired(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                return false;

            lock (_sync)
            {
                AgentRuntimeInstance instance;
                if (!_instances.TryGetValue(instanceId, out instance))
                    return false;
                if (instance.State != AgentRuntimeInstanceState.Retired)
                    return false;

                return _instances.Remove(instanceId);
            }
        }
    }
}

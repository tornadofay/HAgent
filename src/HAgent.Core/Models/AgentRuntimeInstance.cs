using System;
using System.Threading;

namespace HAgent.Models
{
    public sealed class AgentRuntimeInstance
    {
        private readonly object _sync = new object();
        private long _executionRevision;
        private AgentRuntimeInstanceState _state;
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();

        private AgentRuntimeInstance(AiAgent profile, AgentRuntimeScope scope, string instanceId, AgentRuntimeOverrides overrides)
        {
            ProfileId = profile == null ? string.Empty : profile.Id;
            InstanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
            Scope = scope;
            CreatedAt = DateTimeOffset.UtcNow;
            _state = AgentRuntimeInstanceState.Active;
            Overrides = overrides ?? new AgentRuntimeOverrides();
        }

        public string InstanceId { get; private set; }
        public string ProfileId { get; private set; }
        public AgentRuntimeScope Scope { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public AgentRuntimeInstanceState State
        {
            get
            {
                lock (_sync)
                {
                    return _state;
                }
            }
        }

        public AgentRuntimeOverrides Overrides { get; private set; }
        public string MemoryOwnerId { get { return InstanceId; } }
        public long CurrentExecutionRevision
        {
            get
            {
                lock (_sync)
                {
                    return _executionRevision;
                }
            }
        }

        internal CancellationToken ShutdownToken { get { return _shutdownCts.Token; } }

        public static AgentRuntimeInstance Create(AiAgent profile, AgentRuntimeScope scope = AgentRuntimeScope.Ephemeral, AgentRuntimeOverrides overrides = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(profile.Id)) throw new ArgumentException("Agent profile ID is required.", nameof(profile));
            return new AgentRuntimeInstance(profile, scope, null, overrides);
        }

        internal long BeginExecution()
        {
            lock (_sync)
            {
                if (_state != AgentRuntimeInstanceState.Active)
                    throw new InvalidOperationException("Runtime agent instance is not active: " + InstanceId);

                return ++_executionRevision;
            }
        }

        public bool IsExecutionCurrent(AgentExecution execution)
        {
            if (execution == null) return false;
            lock (_sync)
            {
                if (_state != AgentRuntimeInstanceState.Active) return false;
                if (!string.Equals(execution.RuntimeInstanceId, InstanceId, StringComparison.OrdinalIgnoreCase)) return false;
                return execution.RuntimeInstanceRevision == _executionRevision;
            }
        }

        public void Retire()
        {
            lock (_sync)
            {
                if (_state == AgentRuntimeInstanceState.Retired || _state == AgentRuntimeInstanceState.Shutdown)
                    return;
                _state = AgentRuntimeInstanceState.Retired;
            }
        }

        /// <summary>
        /// Permanently shuts down the runtime instance and cancels its outstanding instance-bound work.
        /// A shutdown instance cannot accept new executions.
        /// </summary>
        public void Shutdown()
        {
            lock (_sync)
            {
                if (_state == AgentRuntimeInstanceState.Shutdown)
                    return;
                _state = AgentRuntimeInstanceState.Shutdown;
                ++_executionRevision;
                _shutdownCts.Cancel();
            }
        }
    }
}

namespace HAgent.Models
{
    public enum AgentRuntimeInstanceState
    {
        Active,
        Retired,
        Shutdown
    }
}

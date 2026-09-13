using System;
using System.Threading;

namespace HAgent.Models
{
    public sealed class AgentRuntimeInstance
    {
        private readonly object _sync = new object();
        private long _executionRevision;
        private long _lifecycleRevision;
        private AgentRuntimeInstanceState _state;
        private AiRuntimeHealth _health;
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();

        private AgentRuntimeInstance(
            AiAgent profile,
            AgentRuntimeScope scope,
            string instanceId,
            AgentRuntimeOverrides overrides,
            AgentRuntimeInstanceState state,
            long lifecycleRevision,
            AiRuntimeHealth health)
        {
            ProfileId = profile == null ? string.Empty : profile.Id;
            InstanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
            Scope = scope;
            CreatedAt = DateTimeOffset.UtcNow;
            _state = state;
            _lifecycleRevision = lifecycleRevision;
            _health = health == null ? AiRuntimeHealth.CreateUnknown() : health.Clone();
            _health.Validate();
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

        public AiRuntimeHealth Health
        {
            get
            {
                lock (_sync)
                {
                    return _health.Clone();
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

        public long CurrentLifecycleRevision
        {
            get
            {
                lock (_sync)
                {
                    return _lifecycleRevision;
                }
            }
        }

        internal CancellationToken ShutdownToken { get { return _shutdownCts.Token; } }

        public static AgentRuntimeInstance Create(
            AiAgent profile,
            AgentRuntimeScope scope = AgentRuntimeScope.Ephemeral,
            AgentRuntimeOverrides overrides = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(profile.Id)) throw new ArgumentException("Agent profile ID is required.", nameof(profile));
            return new AgentRuntimeInstance(
                profile,
                scope,
                null,
                overrides,
                AgentRuntimeInstanceState.Active,
                0L,
                AiRuntimeHealth.CreateUnknown());
        }

        /// <summary>
        /// Restores a runtime instance from an explicitly persisted runtime-state record.
        /// Runtime context, prompts, secrets, and execution history are not restored by this operation.
        /// </summary>
        public static AgentRuntimeInstance Restore(
            AiAgent profile,
            AgentRuntimeStateRecord record,
            AgentRuntimeOverrides overrides = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(record.InstanceId)) throw new ArgumentException("Runtime instance ID is required.", nameof(record));
            if (!string.Equals(profile.Id, record.ProfileId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Runtime state profile does not match the supplied agent profile.", nameof(record));
            if (!Enum.IsDefined(typeof(AgentRuntimeInstanceState), record.State))
                throw new ArgumentException("Runtime state contains an unsupported lifecycle state.", nameof(record));
            if (record.LifecycleRevision < 0)
                throw new ArgumentException("Runtime lifecycle revision cannot be negative.", nameof(record));
            var health = record.Health == null ? AiRuntimeHealth.CreateUnknown() : record.Health.Clone();
            health.Validate();

            var instance = new AgentRuntimeInstance(
                profile,
                record.Scope,
                record.InstanceId,
                overrides,
                record.State,
                record.LifecycleRevision,
                health);
            if (record.CreatedAt != default(DateTimeOffset))
                instance.CreatedAt = record.CreatedAt;

            if (record.State == AgentRuntimeInstanceState.Shutdown)
                instance._shutdownCts.Cancel();

            return instance;
        }

        public void SetHealth(AiRuntimeHealth health)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            health.Validate();
            var snapshot = health.Clone();
            lock (_sync)
            {
                _health = snapshot;
            }
        }

        internal long BeginExecution(out long lifecycleRevision)
        {
            lock (_sync)
            {
                if (_state != AgentRuntimeInstanceState.Active)
                    throw new InvalidOperationException("Runtime agent instance is not active: " + InstanceId);

                ++_executionRevision;
                lifecycleRevision = _lifecycleRevision;
                return _executionRevision;
            }
        }

        public bool IsExecutionCurrent(AgentExecution execution)
        {
            if (execution == null) return false;
            lock (_sync)
            {
                if (_state != AgentRuntimeInstanceState.Active) return false;
                if (!string.Equals(execution.RuntimeInstanceId, InstanceId, StringComparison.OrdinalIgnoreCase)) return false;
                return execution.RuntimeInstanceRevision == _executionRevision &&
                       execution.RuntimeLifecycleRevision == _lifecycleRevision;
            }
        }

        public void Suspend()
        {
            TransitionTo(AgentRuntimeInstanceState.Suspended);
        }

        public void BeginRecovery()
        {
            TransitionTo(AgentRuntimeInstanceState.Recovering);
        }

        public void Resume()
        {
            TransitionTo(AgentRuntimeInstanceState.Active);
        }

        public void Retire()
        {
            TransitionTo(AgentRuntimeInstanceState.Retired);
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
                    throw new InvalidOperationException("Runtime agent instance is already shut down: " + InstanceId);

                _state = AgentRuntimeInstanceState.Shutdown;
                ++_lifecycleRevision;
                ++_executionRevision;
                _shutdownCts.Cancel();
            }
        }

        private void TransitionTo(AgentRuntimeInstanceState target)
        {
            lock (_sync)
            {
                if (!IsValidTransition(_state, target))
                {
                    throw new InvalidOperationException(
                        "Invalid runtime lifecycle transition from " + _state + " to " + target + ".");
                }

                _state = target;
                ++_lifecycleRevision;
            }
        }

        private static bool IsValidTransition(AgentRuntimeInstanceState current, AgentRuntimeInstanceState target)
        {
            switch (current)
            {
                case AgentRuntimeInstanceState.Active:
                    return target == AgentRuntimeInstanceState.Suspended ||
                           target == AgentRuntimeInstanceState.Recovering ||
                           target == AgentRuntimeInstanceState.Retired;

                case AgentRuntimeInstanceState.Suspended:
                    return target == AgentRuntimeInstanceState.Active ||
                           target == AgentRuntimeInstanceState.Recovering ||
                           target == AgentRuntimeInstanceState.Retired;

                case AgentRuntimeInstanceState.Recovering:
                    return target == AgentRuntimeInstanceState.Active ||
                           target == AgentRuntimeInstanceState.Suspended ||
                           target == AgentRuntimeInstanceState.Retired;

                case AgentRuntimeInstanceState.Retired:
                case AgentRuntimeInstanceState.Shutdown:
                    return false;

                default:
                    return false;
            }
        }
    }
}

namespace HAgent.Models
{
    public enum AgentRuntimeInstanceState
    {
        Active = 0,
        Retired = 1,
        Shutdown = 2,
        Suspended = 3,
        Recovering = 4
    }
}

using System;

namespace HAgent.Models
{
    public sealed class AgentRuntimeInstance
    {
        private AgentRuntimeInstance(AiAgent profile, AgentRuntimeScope scope, string instanceId)
        {
            ProfileId = profile == null ? string.Empty : profile.Id;
            InstanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
            Scope = scope;
            CreatedAt = DateTimeOffset.UtcNow;
            State = AgentRuntimeInstanceState.Active;
        }

        public string InstanceId { get; private set; }
        public string ProfileId { get; private set; }
        public AgentRuntimeScope Scope { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public AgentRuntimeInstanceState State { get; private set; }

        public static AgentRuntimeInstance Create(AiAgent profile, AgentRuntimeScope scope = AgentRuntimeScope.Ephemeral)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(profile.Id)) throw new ArgumentException("Agent profile ID is required.", nameof(profile));
            return new AgentRuntimeInstance(profile, scope, null);
        }

        public void Retire()
        {
            if (State == AgentRuntimeInstanceState.Retired)
                return;

            State = AgentRuntimeInstanceState.Retired;
        }
    }
}

namespace HAgent.Models
{
    public enum AgentRuntimeInstanceState
    {
        Active,
        Retired
    }
}

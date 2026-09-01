using System;

namespace HAgent.Models
{
    public sealed class AgentRuntimeStateRecord
    {
        public AgentRuntimeStateRecord()
        {
            InstanceId = string.Empty;
            ProfileId = string.Empty;
            HostInstanceId = string.Empty;
            UserId = string.Empty;
            WorkspaceId = string.Empty;
            SessionId = string.Empty;
            Scope = AgentRuntimeScope.Ephemeral;
            State = AgentRuntimeInstanceState.Active;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public string InstanceId { get; set; }
        public string ProfileId { get; set; }
        public string HostInstanceId { get; set; }
        public string UserId { get; set; }
        public string WorkspaceId { get; set; }
        public string SessionId { get; set; }
        public AgentRuntimeScope Scope { get; set; }
        public AgentRuntimeInstanceState State { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public static AgentRuntimeStateRecord FromInstance(
            AgentRuntimeInstance instance,
            string hostInstanceId = null,
            string userId = null,
            string workspaceId = null,
            string sessionId = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var now = DateTimeOffset.UtcNow;
            return new AgentRuntimeStateRecord
            {
                InstanceId = instance.InstanceId,
                ProfileId = instance.ProfileId,
                HostInstanceId = hostInstanceId ?? string.Empty,
                UserId = userId ?? string.Empty,
                WorkspaceId = workspaceId ?? string.Empty,
                SessionId = sessionId ?? string.Empty,
                Scope = instance.Scope,
                State = instance.State,
                CreatedAt = instance.CreatedAt,
                UpdatedAt = now
            };
        }
    }

    public sealed class AgentRuntimeStateQuery
    {
        public string HostInstanceId { get; set; }
        public string UserId { get; set; }
        public string WorkspaceId { get; set; }
        public string SessionId { get; set; }
        public string ProfileId { get; set; }
        public AgentRuntimeScope? Scope { get; set; }
        public int MaxResults { get; set; } = 50;

        public int GetEffectiveMaxResults()
        {
            if (MaxResults < 1 || MaxResults > 100)
                throw new ArgumentOutOfRangeException(nameof(MaxResults), "MaxResults must be between 1 and 100.");
            return MaxResults;
        }
    }
}

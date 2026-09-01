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

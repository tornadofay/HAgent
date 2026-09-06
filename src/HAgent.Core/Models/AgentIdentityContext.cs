using System;

namespace HAgent.Models
{
    /// <summary>
    /// Provider-neutral identity context supplied by the host and captured for an execution.
    /// Authentication and identity verification remain host responsibilities.
    /// </summary>
    public sealed class AgentIdentityContext
    {
        public AgentIdentityContext()
        {
            DeploymentId = string.Empty;
            TenantId = string.Empty;
            PrincipalId = string.Empty;
            DisplayName = string.Empty;
            UserId = string.Empty;
            SessionId = string.Empty;
            WorkspaceId = string.Empty;
        }

        public string DeploymentId { get; set; }
        public string TenantId { get; set; }
        public string PrincipalId { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string WorkspaceId { get; set; }

        public AgentIdentityContext Clone()
        {
            return new AgentIdentityContext
            {
                DeploymentId = DeploymentId ?? string.Empty,
                TenantId = TenantId ?? string.Empty,
                PrincipalId = PrincipalId ?? string.Empty,
                DisplayName = DisplayName ?? string.Empty,
                UserId = UserId ?? string.Empty,
                SessionId = SessionId ?? string.Empty,
                WorkspaceId = WorkspaceId ?? string.Empty
            };
        }

        internal void Validate()
        {
            ValidateValue(DeploymentId, nameof(DeploymentId));
            ValidateValue(TenantId, nameof(TenantId));
            ValidateValue(PrincipalId, nameof(PrincipalId));
            ValidateValue(DisplayName, nameof(DisplayName));
            ValidateValue(UserId, nameof(UserId));
            ValidateValue(SessionId, nameof(SessionId));
            ValidateValue(WorkspaceId, nameof(WorkspaceId));
        }

        private static void ValidateValue(string value, string name)
        {
            if (value != null && value.Length > 256)
                throw new ArgumentException(name + " must be at most 256 characters.", name);
        }
    }
}

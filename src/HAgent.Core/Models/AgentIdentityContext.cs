using System;

namespace HAgent.Models
{
    /// <summary>
    /// Provider-neutral identity context supplied by the host and captured for an execution.
    /// Authentication and identity verification remain host responsibilities.
    /// </summary>
    public sealed class AgentIdentityContext
    {
        public AgentIdentityContext(
            string deploymentId = null,
            string tenantId = null,
            string principalId = null,
            string displayName = null,
            string userId = null,
            string sessionId = null,
            string workspaceId = null)
        {
            DeploymentId = Normalize(deploymentId);
            TenantId = Normalize(tenantId);
            PrincipalId = Normalize(principalId);
            DisplayName = Normalize(displayName);
            UserId = Normalize(userId);
            SessionId = Normalize(sessionId);
            WorkspaceId = Normalize(workspaceId);
        }

        public string DeploymentId { get; private set; }
        public string TenantId { get; private set; }
        public string PrincipalId { get; private set; }
        public string DisplayName { get; private set; }
        public string UserId { get; private set; }
        public string SessionId { get; private set; }
        public string WorkspaceId { get; private set; }

        public AgentIdentityContext Clone()
        {
            return new AgentIdentityContext(
                DeploymentId,
                TenantId,
                PrincipalId,
                DisplayName,
                UserId,
                SessionId,
                WorkspaceId);
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

        private static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private static void ValidateValue(string value, string name)
        {
            if (value != null && value.Length > 256)
                throw new ArgumentException(name + " must be at most 256 characters.", name);
        }
    }
}

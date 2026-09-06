using System;

namespace HAgent.Models
{
    /// <summary>
    /// Stable ownership scopes for HAgent-owned resources. Resource stores may continue
    /// to persist the resulting OwnerId string while using this contract to derive it.
    /// </summary>
    public enum AgentResourceScope
    {
        Global,
        Tenant,
        User,
        Workspace,
        Agent,
        Runtime,
        Execution
    }

    public static class AgentResourceOwnership
    {
        public static string GetOwnerId(
            AgentResourceScope scope,
            AgentIdentityContext identity,
            string resourceId = null)
        {
            identity = identity ?? new AgentIdentityContext();
            identity.Validate();

            switch (scope)
            {
                case AgentResourceScope.Global:
                    return Key("global", identity.DeploymentId);

                case AgentResourceScope.Tenant:
                    Require(identity.TenantId, "TenantId", scope);
                    return Key("tenant", identity.DeploymentId, identity.TenantId);

                case AgentResourceScope.User:
                    Require(identity.UserId, "UserId", scope);
                    return Key("user", identity.DeploymentId, identity.TenantId, identity.UserId);

                case AgentResourceScope.Workspace:
                    Require(identity.WorkspaceId, "WorkspaceId", scope);
                    return Key("workspace", identity.DeploymentId, identity.TenantId, identity.WorkspaceId);

                case AgentResourceScope.Agent:
                    return Key("agent", identity.DeploymentId, identity.TenantId, RequireResourceId(resourceId, scope));

                case AgentResourceScope.Runtime:
                    return Key("runtime", identity.DeploymentId, identity.TenantId, RequireResourceId(resourceId, scope));

                case AgentResourceScope.Execution:
                    return Key("execution", identity.DeploymentId, identity.TenantId, RequireResourceId(resourceId, scope));

                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported resource scope.");
            }
        }

        private static string Key(params string[] parts)
        {
            var result = string.Empty;
            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i] ?? string.Empty;
                result += value.Length.ToString() + ":" + value;
                if (i < parts.Length - 1) result += "|";
            }
            return result;
        }

        private static void Require(string value, string name, AgentResourceScope scope)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(scope + " resources require " + name + ".");
        }

        private static string RequireResourceId(string resourceId, AgentResourceScope scope)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException(scope + " resources require a resource ID.", nameof(resourceId));
            return resourceId.Trim();
        }
    }
}

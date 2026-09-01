using System;
using System.IO;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;
using HAgent.Storage.MySql;
using HAgent.Storage.SqlServer;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task<IAgentRuntimeStateStore> CreateConfiguredRuntimeStateStoreAsync()
        {
            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            switch (options.StorageType)
            {
                case HAgentStorageType.File:
                    return new FileAgentRuntimeStateStore(Path.Combine(options.GetEffectiveRootPath(), "runtime-state", "instances.jsonl"));

                case HAgentStorageType.SqlServer:
                {
                    var profile = options.GetDatabaseProfile(HAgentStorageType.SqlServer);
                    var password = await LoadDatabasePasswordAsync(options, HAgentStorageType.SqlServer, default(System.Threading.CancellationToken)).ConfigureAwait(true);
                    var bootstrapper = new SqlServerHAgentStorageBootstrapper();
                    await bootstrapper.EnsureCreatedAsync(options, password).ConfigureAwait(true);
                    var connectionString = SqlServerHAgentStorageBootstrapper.BuildConnectionString(
                        profile.ServerName,
                        profile.GetEffectivePort(HAgentStorageType.SqlServer),
                        profile.UserName,
                        password,
                        options.GetEffectiveDatabaseName());
                    await SqlServerAgentRuntimeStateStore.EnsureSchemaAsync(connectionString).ConfigureAwait(true);
                    return new SqlServerAgentRuntimeStateStore(connectionString);
                }

                case HAgentStorageType.MySql:
                {
                    var profile = options.GetDatabaseProfile(HAgentStorageType.MySql);
                    var password = await LoadDatabasePasswordAsync(options, HAgentStorageType.MySql, default(System.Threading.CancellationToken)).ConfigureAwait(true);
                    var bootstrapper = new MySqlHAgentStorageBootstrapper();
                    await bootstrapper.EnsureCreatedAsync(options, password).ConfigureAwait(true);
                    var connectionString = MySqlHAgentStorageBootstrapper.BuildConnectionString(
                        profile.ServerName,
                        profile.GetEffectivePort(HAgentStorageType.MySql),
                        profile.UserName,
                        password,
                        options.GetEffectiveDatabaseName());
                    await MySqlAgentRuntimeStateStore.EnsureSchemaAsync(connectionString).ConfigureAwait(true);
                    return new MySqlAgentRuntimeStateStore(connectionString);
                }

                default:
                    throw new InvalidOperationException("Unsupported HAgent storage backend.");
            }
        }

        private async Task TestRuntimeStatePersistenceAsync(string message)
        {
            var store = await CreateConfiguredRuntimeStateStoreAsync().ConfigureAwait(true);
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var instance = AgentRuntimeInstance.Create(profile, AgentRuntimeScope.Task);
            var persistence = new AgentRuntimeStatePersistence(store);
            var hostId = "example-host-42";
            var userId = "example-user-42";
            var workspaceId = "example-workspace-42";
            var sessionId = "example-session-42";

            await persistence.SaveAsync(instance, hostId, userId, workspaceId, sessionId).ConfigureAwait(true);
            var restored = await persistence.RestoreAsync(profile, instance.InstanceId).ConfigureAwait(true);
            if (restored == null)
                throw new InvalidOperationException("Persisted runtime instance could not be restored.");
            if (!string.Equals(restored.InstanceId, instance.InstanceId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime instance ID was not preserved during persistence.");
            if (!string.Equals(restored.ProfileId, profile.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Runtime profile ID was not preserved during persistence.");
            if (restored.Scope != AgentRuntimeScope.Task)
                throw new InvalidOperationException("Runtime scope was not preserved during persistence.");
            if (restored.State != AgentRuntimeInstanceState.Active)
                throw new InvalidOperationException("Active runtime state was not restored correctly.");

            var saved = await store.GetAsync(instance.InstanceId).ConfigureAwait(true);
            if (saved == null || !string.Equals(saved.HostInstanceId, hostId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(saved.UserId, userId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(saved.WorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(saved.SessionId, sessionId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Host runtime identity metadata was not persisted correctly.");

            instance.Retire();
            await persistence.SaveAsync(instance, hostId, userId, workspaceId, sessionId).ConfigureAwait(true);
            var retired = await persistence.RestoreAsync(profile, instance.InstanceId).ConfigureAwait(true);
            if (retired == null || retired.State != AgentRuntimeInstanceState.Retired)
                throw new InvalidOperationException("Retired runtime state was not persisted/restored correctly.");

            await persistence.DeleteAsync(instance.InstanceId).ConfigureAwait(true);
            if (await store.GetAsync(instance.InstanceId).ConfigureAwait(true) != null)
                throw new InvalidOperationException("Persisted runtime instance was not deleted.");

            Write("RUNTIME STATE PERSISTENCE",
                "Contract test succeeded." + Environment.NewLine +
                "Storage backend: " + (await LoadStorageOptionsAsync().ConfigureAwait(true)).StorageType + Environment.NewLine +
                "Runtime instance: " + instance.InstanceId + Environment.NewLine +
                "Active state round-trip: yes" + Environment.NewLine +
                "Host/user/workspace/session metadata: preserved" + Environment.NewLine +
                "Retired state round-trip: yes" + Environment.NewLine +
                "Deletion: verified" + Environment.NewLine +
                "Runtime context/prompts/secrets/execution history: not persisted");
        }
    }
}

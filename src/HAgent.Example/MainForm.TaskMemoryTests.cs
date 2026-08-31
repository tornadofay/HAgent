using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestTaskMemoryAsync(string message)
        {
            var input = RequireInput(message);
            var taskId = "task-" + Guid.NewGuid().ToString("N");
            var ownerId = "HAgent.Example";
            var memory = await CreateConfiguredMemoryStoreAsync().ConfigureAwait(true);
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));

            try
            {
                var client = new HAgentClient(
                    store,
                    secrets,
                    new[] { new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter() },
                    null,
                    memory);

                await client.RememberTaskEventAsync(
                    ownerId,
                    taskId,
                    "Started task: " + input,
                    MemoryKind.Task,
                    new Dictionary<string, string> { { "status", "started" } },
                    DateTimeOffset.UtcNow,
                    CancellationToken.None);

                await client.RememberTaskEventAsync(
                    ownerId,
                    taskId,
                    "Task reached first checkpoint.",
                    MemoryKind.Event,
                    new Dictionary<string, string> { { "status", "checkpoint" } },
                    DateTimeOffset.UtcNow,
                    CancellationToken.None);

                var results = await client.RecallTaskEventsAsync(
                    ownerId,
                    taskId,
                    null,
                    10,
                    CancellationToken.None);

                if (results.Count != 2)
                    throw new InvalidOperationException("Expected two task/event records but found " + results.Count + ".");
                if (results.Any(x => x.Scope != MemoryScope.Task || x.TaskId != taskId))
                    throw new InvalidOperationException("Task memory filtering returned an entry from the wrong task.");
                if (!results.Any(x => x.Kind == MemoryKind.Task) || !results.Any(x => x.Kind == MemoryKind.Event))
                    throw new InvalidOperationException("Task/event memory kinds were not preserved.");

                Write("TASK / EVENT MEMORY",
                    "Test succeeded." + Environment.NewLine +
                    "Storage backend: " + GetConfiguredStorageTypeLabel() + Environment.NewLine +
                    "Task ID: " + taskId + Environment.NewLine +
                    "Records: " + results.Count + Environment.NewLine +
                    string.Join(Environment.NewLine, results.Select(x =>
                        "  " + x.Kind + " | " + x.OccurredAt.ToLocalTime().ToString("HH:mm:ss") + " | " + x.Content)));
            }
            finally
            {
                var disposable = memory as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }

        private async Task<string> GetConfiguredStorageTypeLabelAsync()
        {
            var options = await LoadStorageOptionsAsync().ConfigureAwait(true);
            return options.StorageType.ToString();
        }

        private string GetConfiguredStorageTypeLabel()
        {
            var configurationPath = StorageConfigurationPath;
            try
            {
                var json = System.IO.File.ReadAllText(configurationPath);
                if (json.IndexOf("SqlServer", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "SqlServer";
                if (json.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "MySql";
            }
            catch
            {
            }

            return "File";
        }
    }
}

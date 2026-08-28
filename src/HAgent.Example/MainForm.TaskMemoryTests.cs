using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestTaskMemoryAsync(string message)
        {
            var input = RequireInput(message);
            var taskId = "task-" + Guid.NewGuid().ToString("N");
            var path = Path.Combine(_basePath, "memory", "example-task-memory-" + Guid.NewGuid().ToString("N") + ".jsonl");
            var store = new FileMemoryStore(path);
            const string ownerId = "HAgent.Example";

            try
            {
                await new HAgent.Runtime.HAgentClient(
                    new FileAiStore(Path.Combine(_basePath, "settings.json")),
                    new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets")),
                    new[] { new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter() },
                    null,
                    store).RememberTaskEventAsync(ownerId, taskId,
                        "Started task: " + input,
                        MemoryKind.Task,
                        new Dictionary<string, string> { { "status", "started" } },
                        DateTimeOffset.UtcNow,
                        CancellationToken.None);

                await new HAgent.Runtime.HAgentClient(
                    new FileAiStore(Path.Combine(_basePath, "settings.json")),
                    new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets")),
                    new[] { new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter() },
                    null,
                    store).RememberTaskEventAsync(ownerId, taskId,
                        "Task reached first checkpoint.",
                        MemoryKind.Event,
                        new Dictionary<string, string> { { "status", "checkpoint" } },
                        DateTimeOffset.UtcNow,
                        CancellationToken.None);

                var results = await new HAgent.Runtime.HAgentClient(
                    new FileAiStore(Path.Combine(_basePath, "settings.json")),
                    new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets")),
                    new[] { new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter() },
                    null,
                    store).RecallTaskEventsAsync(ownerId, taskId, null, 10, CancellationToken.None);

                if (results.Count != 2)
                    throw new InvalidOperationException("Expected two task/event records but found " + results.Count + ".");
                if (results.Any(x => x.Scope != MemoryScope.Task || x.TaskId != taskId))
                    throw new InvalidOperationException("Task memory filtering returned an entry from the wrong task.");
                if (!results.Any(x => x.Kind == MemoryKind.Task) || !results.Any(x => x.Kind == MemoryKind.Event))
                    throw new InvalidOperationException("Task/event memory kinds were not preserved.");

                Write("TASK / EVENT MEMORY",
                    "Test succeeded." + Environment.NewLine +
                    "Task ID: " + taskId + Environment.NewLine +
                    "Records: " + results.Count + Environment.NewLine +
                    string.Join(Environment.NewLine, results.Select(x =>
                        "  " + x.Kind + " | " + x.OccurredAt.ToLocalTime().ToString("HH:mm:ss") + " | " + x.Content)));
            }
            finally
            {
                store.Dispose();
                try { if (File.Exists(path)) File.Delete(path); }
                catch { }
            }
        }
    }
}

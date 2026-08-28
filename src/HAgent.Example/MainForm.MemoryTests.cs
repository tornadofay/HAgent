using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestAutomaticMemoryAsync(string message)
        {
            var input = RequireInput(message);
            var store = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var secrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var memoryPath = Path.Combine(_basePath, "memory", "example-automatic-memory-" + Guid.NewGuid().ToString("N") + ".jsonl");
            var memory = new FileMemoryStore(memoryPath);
            var agent = GetSelectedAgent();
            if (agent == null) throw new InvalidOperationException("Select an agent first.");

            try
            {
                var client = new HAgentClient(
                    store,
                    secrets,
                    new[] { new OpenAICompatibleProviderAdapter() },
                    null,
                    memory);

                await client.SendAsync(agent.Id, input, CancellationToken.None);

                var expected = input;
                var marker = "remember this:";
                var index = input.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                    expected = input.Substring(index + marker.Length).Trim().TrimEnd('.', '!', '?').Trim('"', '\'');

                var recalled = await client.RecallAsync(
                    agent.Id,
                    expected,
                    MemoryScope.Agent,
                    10,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "source", "conversation" },
                        { "policy", nameof(ExplicitConversationMemoryPolicy) }
                    },
                    CancellationToken.None);

                var found = recalled.FirstOrDefault(x => string.Equals(x.Content, expected, StringComparison.OrdinalIgnoreCase));

                if (!input.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    if (found != null)
                        throw new InvalidOperationException("The automatic policy stored a message that did not contain an explicit memory trigger.");

                    Write("AUTOMATIC MEMORY", "Policy test succeeded." + Environment.NewLine +
                                               "Input was not an explicit memory request." + Environment.NewLine +
                                               "No durable memory entry was created.");
                    return;
                }

                if (found == null)
                    throw new InvalidOperationException("The explicit memory trigger did not create a durable memory entry.");

                await client.ForgetAsync(found.Id, CancellationToken.None);
                Write("AUTOMATIC MEMORY", "Policy test succeeded." + Environment.NewLine +
                                           "Stored: " + found.Content + Environment.NewLine +
                                           "Scope: " + found.Scope + Environment.NewLine +
                                           "Owner: " + found.OwnerId + Environment.NewLine +
                                           "Policy: " + found.Metadata["policy"] + Environment.NewLine +
                                           "The memory was removed after verification.");
            }
            finally
            {
                memory.Dispose();
                try
                {
                    if (File.Exists(memoryPath)) File.Delete(memoryPath);
                }
                catch
                {
                    // Example cleanup must not hide the actual policy test result.
                }
            }
        }
    }
}

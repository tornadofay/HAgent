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
        private async Task TestAgentToolAssignmentAsync(string input)
        {
            var path = Path.Combine(_basePath, "tool-definitions", "assignment-test-tools.json");
            var toolStore = new FileToolStore(path);
            var tool = new AiTool
            {
                Id = "example.assignment.tool",
                Name = "Assignment Example",
                Description = "Temporary tool used to verify persisted agent-tool assignment.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{}}",
                Category = "Example",
                Type = AiToolType.Application,
                Enabled = true
            };
            var agent = new AiAgent
            {
                Id = "example-assignment-agent",
                Name = "Assignment Example Agent",
                ProviderId = "example-provider",
                Model = "example-model",
                ToolIds = new List<string> { tool.Id },
                Enabled = true
            };

            try
            {
                await toolStore.SaveToolAsync(tool, CancellationToken.None).ConfigureAwait(false);

                var settingsPath = Path.Combine(_basePath, "assignment-test-settings.json");
                var aiStore = new FileAiStore(settingsPath);
                await aiStore.SaveAgentAsync(agent, CancellationToken.None).ConfigureAwait(false);

                var reopenedToolStore = new FileToolStore(path);
                var reopenedAiStore = new FileAiStore(settingsPath);
                try
                {
                    var tools = await reopenedToolStore.GetToolsAsync(CancellationToken.None).ConfigureAwait(false);
                    var agents = await reopenedAiStore.GetAgentsAsync(CancellationToken.None).ConfigureAwait(false);
                    var reopenedAgent = agents.FirstOrDefault(x => string.Equals(x.Id, agent.Id, StringComparison.OrdinalIgnoreCase));
                    var assigned = reopenedAgent != null && reopenedAgent.ToolIds != null && reopenedAgent.ToolIds.Any(x => string.Equals(x, tool.Id, StringComparison.OrdinalIgnoreCase));
                    var definitionPresent = tools.Any(x => string.Equals(x.Id, tool.Id, StringComparison.OrdinalIgnoreCase));

                    if (!definitionPresent) throw new InvalidOperationException("The persisted tool definition could not be reopened.");
                    if (!assigned) throw new InvalidOperationException("The persisted agent did not retain its assigned tool ID.");

                    Write("AGENT TOOL ASSIGNMENT",
                        "Persistence test succeeded." + Environment.NewLine +
                        "Agent: " + agent.Name + Environment.NewLine +
                        "Tool: " + tool.Name + Environment.NewLine +
                        "Tool ID: " + tool.Id + Environment.NewLine +
                        "Assignment retained: yes" + Environment.NewLine +
                        "Tool definition retained: yes" + Environment.NewLine +
                        "Execution handler persisted: no (application-owned)." );
                }
                finally
                {
                    reopenedToolStore.Dispose();
                    await reopenedAiStore.DeleteAgentAsync(agent.Id, CancellationToken.None).ConfigureAwait(false);
                    try { System.IO.File.Delete(path); } catch { }
                    try { System.IO.File.Delete(settingsPath); } catch { }
                }
            }
            finally
            {
                toolStore.Dispose();
            }
        }
    }
}

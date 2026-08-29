using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestToolPersistenceAsync(string input)
        {
            var path = Path.Combine(_basePath, "tool-definitions", "example-tools.json");
            var tool = new AiTool
            {
                Id = "example.persisted.tool",
                Name = "Persisted Example Tool",
                Description = "Verifies durable storage of a tool definition without persisting executable code.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                Category = "Example",
                Type = AiToolType.Application,
                IsBuiltIn = false,
                Enabled = true
            };

            var first = new FileToolStore(path);
            await first.SaveToolAsync(tool, CancellationToken.None).ConfigureAwait(true);
            var beforeDispose = await first.GetToolsAsync(CancellationToken.None).ConfigureAwait(true);
            first = null;

            var second = new FileToolStore(path);
            try
            {
                var tools = await second.GetToolsAsync(CancellationToken.None).ConfigureAwait(true);
                var restored = tools.Count == 1 ? tools[0] : null;
                if (restored == null)
                    throw new InvalidOperationException("The second tool-store instance could not read the persisted definition.");
                if (!string.Equals(restored.Id, tool.Id, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(restored.Name, tool.Name, StringComparison.Ordinal) ||
                    !string.Equals(restored.InputSchemaJson, tool.InputSchemaJson, StringComparison.Ordinal) ||
                    restored.Type != tool.Type || restored.Enabled != tool.Enabled)
                    throw new InvalidOperationException("The persisted tool definition was not restored exactly.");

                await second.DeleteToolAsync(tool.Id, CancellationToken.None).ConfigureAwait(true);
                var afterDelete = await second.GetToolsAsync(CancellationToken.None).ConfigureAwait(true);
                if (afterDelete.Count != 0)
                    throw new InvalidOperationException("The persisted tool definition remained after deletion.");

                Write("TOOL PERSISTENCE",
                    "Persistence test succeeded." + Environment.NewLine +
                    "File: " + path + Environment.NewLine +
                    "Tool: " + restored.Name + Environment.NewLine +
                    "Type: " + restored.Type + Environment.NewLine +
                    "Definitions before reopen: " + beforeDispose.Count + Environment.NewLine +
                    "Definitions after reopen: " + tools.Count + Environment.NewLine +
                    "Cleanup: persisted definition deleted." + Environment.NewLine +
                    "Note: only the definition/schema is persisted; executable handlers remain application-owned.");
            }
            finally
            {
                second.DeleteToolAsync(tool.Id, CancellationToken.None).GetAwaiter().GetResult();
                try
                {
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) && Directory.GetFiles(directory).Length == 0)
                        Directory.Delete(directory);
                }
                catch { }
            }
        }
    }
}

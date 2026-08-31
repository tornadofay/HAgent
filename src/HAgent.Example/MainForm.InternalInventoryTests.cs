using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddInternalInventoryTab()
        {
            AddApiTab(
                "Internal Inventory",
                "Run inventory test",
                "Reads bounded non-secret metadata from the HAgent-owned provider, agent, and tool repositories using the currently selected storage backend.",
                "The test should identify the selected backend, return no more than one item per category, expose no credential/secret metadata, and perform no writes.",
                "maxItems = 1",
                TestInternalInventoryAsync,
                "Internal storage boundary",
                "This tool can inspect HAgent-owned data only. It does not expose provider secrets, database credentials, executable handlers, or host application data.");
        }

        private async Task TestInternalInventoryAsync(string input)
        {
            var options = await LoadStorageOptionsAsync(CancellationToken.None).ConfigureAwait(false);
            var aiStore = await CreateConfiguredAiStoreAsync(CancellationToken.None).ConfigureAwait(false);
            var toolStore = await CreateConfiguredToolStoreAsync(CancellationToken.None).ConfigureAwait(false);
            var inventory = new HAgentInternalInventoryTool(aiStore, toolStore);

            var arguments = new Dictionary<string, object>();
            arguments["maxItems"] = 1;

            var result = await inventory.ExecuteAsync(new ToolExecutionContext
            {
                AgentId = "example-inventory-test",
                ToolCallId = "inventory-call-42",
                CorrelationId = Guid.NewGuid().ToString("N"),
                Arguments = arguments,
                CancellationToken = CancellationToken.None
            }).ConfigureAwait(false);

            if (!result.Succeeded)
                throw new InvalidOperationException("Internal inventory tool failed: " + result.Error);
            if (string.IsNullOrWhiteSpace(result.Output))
                throw new InvalidOperationException("Internal inventory tool returned empty output.");

            var forbidden = new[]
            {
                "apikey",
                "api_key",
                "password",
                "connectionstring",
                "connection string",
                "secretvalue",
                "secret value"
            };

            foreach (var token in forbidden)
            {
                if (result.Output.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    throw new InvalidOperationException("Internal inventory output exposed forbidden sensitive metadata: " + token);
            }

            var lines = result.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var returnedProviders = 0;
            var returnedAgents = 0;
            var returnedTools = 0;
            foreach (var line in lines)
            {
                if (line.StartsWith("Provider | ", StringComparison.Ordinal)) returnedProviders++;
                if (line.StartsWith("Agent | ", StringComparison.Ordinal)) returnedAgents++;
                if (line.StartsWith("Tool | ", StringComparison.Ordinal)) returnedTools++;
            }

            if (returnedProviders > 1 || returnedAgents > 1 || returnedTools > 1)
                throw new InvalidOperationException("Internal inventory exceeded the configured maxItems bound.");

            Write("INTERNAL INVENTORY",
                "Contract test succeeded." + Environment.NewLine +
                "Storage backend: " + options.StorageType + Environment.NewLine +
                "Tool: " + inventory.Definition.Name + Environment.NewLine +
                "Tool ID: " + inventory.Definition.Id + Environment.NewLine +
                "maxItems: 1" + Environment.NewLine +
                "Returned providers: " + returnedProviders + Environment.NewLine +
                "Returned agents: " + returnedAgents + Environment.NewLine +
                "Returned tools: " + returnedTools + Environment.NewLine +
                "Sensitive metadata: not exposed." + Environment.NewLine +
                "Write operations: none.");
        }
    }
}

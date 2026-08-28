using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestToolRegistryAsync(string input)
        {
            var toolId = "example.echo";
            var definition = new AiTool
            {
                Id = toolId,
                Name = "Example Echo",
                Description = "Returns the supplied value so the tool contract can be verified without a provider.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
                Category = "Example",
                IsBuiltIn = false,
                Enabled = true
            };

            var tool = new DelegateAgentTool(definition, context =>
            {
                object value;
                context.Arguments.TryGetValue("value", out value);
                return Task.FromResult(ToolExecutionResult.Success(Convert.ToString(value) ?? string.Empty));
            });

            var client = CreateToolTestClient();
            client.RegisterTool(tool);

            IAgentTool registered;
            if (!client.TryGetTool(toolId, out registered))
                throw new InvalidOperationException("Registered tool could not be resolved.");

            var valueText = string.IsNullOrWhiteSpace(input) ? "HAgent-tool-42" : input.Trim();
            var result = await client.ExecuteToolAsync(
                "example-agent",
                toolId,
                "call-example-42",
                new Dictionary<string, object> { { "value", valueText } });

            if (!result.Succeeded)
                throw new InvalidOperationException("Registered tool returned a failure: " + result.Error);
            if (!string.Equals(result.Output, valueText, StringComparison.Ordinal))
                throw new InvalidOperationException("Tool did not receive or return the expected argument.");

            var definitions = client.GetToolDefinitions();
            if (definitions.Count != 1 || definitions[0].Id != toolId)
                throw new InvalidOperationException("Tool definition registry did not retain the expected definition.");

            client.UnregisterTool(toolId);

            IAgentTool removed;
            if (client.TryGetTool(toolId, out removed))
                throw new InvalidOperationException("Tool remained registered after unregistering it.");

            Write("TOOL REGISTRY",
                "Contract test succeeded." + Environment.NewLine +
                "Tool: " + definition.Name + Environment.NewLine +
                "Tool ID: " + toolId + Environment.NewLine +
                "Tool call ID: call-example-42" + Environment.NewLine +
                "Argument value: " + valueText + Environment.NewLine +
                "Result: " + result.Output + Environment.NewLine +
                "Definition count: " + definitions.Count + Environment.NewLine +
                "Cleanup: tool unregistered successfully.");
        }
    }
}

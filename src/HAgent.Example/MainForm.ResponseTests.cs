using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestResponseNormalizationAsync(string unused)
        {
            var toolCalls = new List<AIToolCall>
            {
                new AIToolCall("call-42", "ui.read_control", "{\"controlId\":\"txtCustomerName\"}")
            }.AsReadOnly();

            var response = new AIResponse
            {
                AgentId = "example-agent",
                ProviderId = "example-provider",
                Model = "example-model",
                Text = "Customer data is ready.",
                Reasoning = "Provider-side reasoning metadata.",
                RawText = "Customer data is ready.",
                StructuredOutputJson = "{\"customerId\":42,\"status\":\"active\"}",
                ToolCalls = toolCalls,
                RequestId = "request-42"
            };

            if (response.ToolCalls == null || response.ToolCalls.Count != 1)
                throw new InvalidOperationException("Normalized response did not retain the expected tool call.");
            if (response.ToolCalls[0].Name != "ui.read_control")
                throw new InvalidOperationException("Normalized tool call name was not preserved.");
            if (string.IsNullOrWhiteSpace(response.StructuredOutputJson))
                throw new InvalidOperationException("Normalized structured output was not preserved.");
            if (response.Text != "Customer data is ready.")
                throw new InvalidOperationException("Backward-compatible response text was not preserved.");
            if (response.Reasoning != "Provider-side reasoning metadata.")
                throw new InvalidOperationException("Separate reasoning content was not preserved.");

            Write("RESPONSE NORMALIZATION",
                "Contract test succeeded." + Environment.NewLine +
                "Text: " + response.Text + Environment.NewLine +
                "Reasoning: " + response.Reasoning + Environment.NewLine +
                "Structured output: " + response.StructuredOutputJson + Environment.NewLine +
                "Tool calls: " + response.ToolCalls.Count + Environment.NewLine +
                "Tool: " + response.ToolCalls[0].Name + Environment.NewLine +
                "Arguments JSON: " + response.ToolCalls[0].ArgumentsJson + Environment.NewLine +
                "Request ID: " + response.RequestId);

            await Task.CompletedTask;
        }
    }
}

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
                RequestId = "request-42",
                NormalizedUsage = new AIUsage
                {
                    PromptTokens = 120,
                    CompletionTokens = 80,
                    TotalTokens = 200,
                    CachedPromptTokens = 40,
                    ReasoningTokens = 25
                }
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
            if (!response.NormalizedUsage.HasTokenUsage ||
                response.NormalizedUsage.PromptTokens != 120 ||
                response.NormalizedUsage.CompletionTokens != 80 ||
                response.NormalizedUsage.TotalTokens != 200 ||
                response.NormalizedUsage.CachedPromptTokens != 40 ||
                response.NormalizedUsage.ReasoningTokens != 25)
            {
                throw new InvalidOperationException("Normalized usage values were not preserved.");
            }

            Write("RESPONSE NORMALIZATION",
                "Contract test succeeded." + Environment.NewLine +
                "Text: " + response.Text + Environment.NewLine +
                "Reasoning: " + response.Reasoning + Environment.NewLine +
                "Structured output: " + response.StructuredOutputJson + Environment.NewLine +
                "Tool calls: " + response.ToolCalls.Count + Environment.NewLine +
                "Tool: " + response.ToolCalls[0].Name + Environment.NewLine +
                "Arguments JSON: " + response.ToolCalls[0].ArgumentsJson + Environment.NewLine +
                "Prompt tokens: " + response.NormalizedUsage.PromptTokens + Environment.NewLine +
                "Completion tokens: " + response.NormalizedUsage.CompletionTokens + Environment.NewLine +
                "Cached prompt tokens: " + response.NormalizedUsage.CachedPromptTokens + Environment.NewLine +
                "Reasoning tokens: " + response.NormalizedUsage.ReasoningTokens + Environment.NewLine +
                "Total tokens: " + response.NormalizedUsage.TotalTokens + Environment.NewLine +
                "Request ID: " + response.RequestId);

            await Task.CompletedTask;
        }
    }
}

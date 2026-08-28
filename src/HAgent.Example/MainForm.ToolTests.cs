using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
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
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false}",
                Category = "Example",
                IsBuiltIn = false,
                Enabled = true
            };

            var invocationCount = 0;
            var tool = new DelegateAgentTool(definition, context =>
            {
                invocationCount++;
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
            if (invocationCount != 1)
                throw new InvalidOperationException("Tool handler invocation count was unexpected after valid execution.");

            var invalidType = await client.ExecuteToolAsync(
                "example-agent",
                toolId,
                "call-invalid-type",
                new Dictionary<string, object> { { "value", 42 } });
            if (invalidType.Succeeded || invocationCount != 1 || invalidType.Error.IndexOf("expected a string", StringComparison.OrdinalIgnoreCase) < 0)
                throw new InvalidOperationException("Invalid argument type was not rejected before the handler executed.");

            var missingRequired = await client.ExecuteToolAsync(
                "example-agent",
                toolId,
                "call-missing",
                new Dictionary<string, object>());
            if (missingRequired.Succeeded || invocationCount != 1 || missingRequired.Error.IndexOf("required argument is missing", StringComparison.OrdinalIgnoreCase) < 0)
                throw new InvalidOperationException("Missing required argument was not rejected before the handler executed.");

            var extraArgument = await client.ExecuteToolAsync(
                "example-agent",
                toolId,
                "call-extra",
                new Dictionary<string, object> { { "value", valueText }, { "unexpected", true } });
            if (extraArgument.Succeeded || invocationCount != 1 || extraArgument.Error.IndexOf("not allowed", StringComparison.OrdinalIgnoreCase) < 0)
                throw new InvalidOperationException("Unexpected argument was not rejected by the schema.");

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
                "Schema validation: valid, wrong type, missing required, and extra argument cases verified." + Environment.NewLine +
                "Handler invocations: " + invocationCount + Environment.NewLine +
                "Definition count: " + definitions.Count + Environment.NewLine +
                "Cleanup: tool unregistered successfully.");
        }

        private async Task TestProviderToolTransportAsync(string input)
        {
            var handler = new RecordingToolRequestHandler();
            using (var httpClient = new HttpClient(handler))
            {
                var adapter = new OpenAICompatibleProviderAdapter(httpClient);
                var provider = new AiProvider
                {
                    Id = "provider-example",
                    Name = "Example Provider",
                    Kind = OpenAICompatibleProviderAdapter.ProviderKind,
                    BaseUrl = "https://example.invalid/v1",
                    DefaultModel = "example-model",
                    Enabled = true
                };
                var agent = new AiAgent
                {
                    Id = "example-agent",
                    Name = "Example Agent",
                    ProviderId = provider.Id,
                    Model = provider.DefaultModel,
                    Enabled = true
                };
                var tool = new AiTool
                {
                    Id = "example.echo",
                    Name = "example_echo",
                    Description = "Returns a supplied value.",
                    InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
                    Type = AiToolType.Application,
                    Enabled = true
                };

                var response = await adapter.SendWithToolsAsync(
                    provider,
                    agent,
                    string.Empty,
                    string.Empty,
                    new List<AIMessage> { new AIMessage("user", string.IsNullOrWhiteSpace(input) ? "Call the tool." : input) },
                    new List<AiTool> { tool },
                    CancellationToken.None);

                var request = handler.RequestBody ?? string.Empty;
                if (request.IndexOf("\"tools\"", StringComparison.OrdinalIgnoreCase) < 0 ||
                    request.IndexOf("example_echo", StringComparison.OrdinalIgnoreCase) < 0 ||
                    request.IndexOf("\"parameters\"", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Provider request did not contain the expected tool definition and JSON schema.");
                if (response.ToolCalls == null || response.ToolCalls.Count != 1 || response.ToolCalls[0].Name != "example_echo")
                    throw new InvalidOperationException("Provider response tool call was not normalized correctly.");

                Write("PROVIDER TOOL TRANSPORT",
                    "Contract test succeeded." + Environment.NewLine +
                    "Tool definition sent: " + tool.Name + Environment.NewLine +
                    "Schema sent: yes" + Environment.NewLine +
                    "Response tool call: " + response.ToolCalls[0].Name + Environment.NewLine +
                    "Tool call ID: " + response.ToolCalls[0].Id + Environment.NewLine +
                    "No external provider was contacted; HTTP was captured by the local test handler.");
            }
        }

        private static HAgentClient CreateToolTestClient()
        {
            return new HAgentClient(new InMemoryAiStore(), new EmptySecretStore(), new IAiProviderAdapter[0]);
        }

        private sealed class RecordingToolRequestHandler : HttpMessageHandler
        {
            public string RequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                var body = "{\"id\":\"tool-request-42\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"call-tool-42\",\"type\":\"function\",\"function\":{\"name\":\"example_echo\",\"arguments\":\"{\\\"value\\\":\\\"HAgent-tool-42\\\"}\"}}]}}]}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        private sealed class EmptySecretStore : ISecretStore
        {
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }

            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(string.Empty);
            }

            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
        }
    }
}

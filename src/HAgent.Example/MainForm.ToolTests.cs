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
using Newtonsoft.Json.Linq;

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
            var observedCorrelationId = string.Empty;
            var tool = new DelegateAgentTool(definition, context =>
            {
                invocationCount++;
                observedCorrelationId = context.CorrelationId;
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
            const string hostCorrelationId = "host-tool-correlation-42";
            var result = await client.ExecuteToolAsync(
                "example-agent", toolId, "call-example-42",
                new Dictionary<string, object> { { "value", valueText } },
                CancellationToken.None, hostCorrelationId);

            if (!result.Succeeded || result.Output != valueText)
                throw new InvalidOperationException("Registered tool did not return the expected output.");
            if (invocationCount != 1 || string.IsNullOrWhiteSpace(result.CorrelationId))
                throw new InvalidOperationException("Valid tool execution metadata was not produced.");
            if (!string.Equals(observedCorrelationId, result.CorrelationId, StringComparison.Ordinal))
                throw new InvalidOperationException("Tool context correlation did not match the result.");

            var invalidType = await client.ExecuteToolAsync(
                "example-agent", toolId, "call-invalid-type",
                new Dictionary<string, object> { { "value", 42 } });
            if (invalidType.Succeeded || invocationCount != 1)
                throw new InvalidOperationException("Invalid tool arguments were not rejected before handler execution.");

            var missingRequired = await client.ExecuteToolAsync(
                "example-agent", toolId, "call-missing", new Dictionary<string, object>());
            if (missingRequired.Succeeded || invocationCount != 1)
                throw new InvalidOperationException("Missing required tool arguments were not rejected.");

            client.UnregisterTool(toolId);
            IAgentTool removed;
            if (client.TryGetTool(toolId, out removed))
                throw new InvalidOperationException("Tool remained registered after unregistering it.");

            Write("TOOL REGISTRY",
                "Contract test succeeded." + Environment.NewLine +
                "Tool: " + definition.Name + Environment.NewLine +
                "Valid execution: verified" + Environment.NewLine +
                "Schema rejection: invalid type + missing required argument verified" + Environment.NewLine +
                "Correlation metadata: verified" + Environment.NewLine +
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
                    provider.DefaultModel,
                    CancellationToken.None);

                var request = handler.RequestBody ?? string.Empty;
                if (request.IndexOf("\"tools\"", StringComparison.OrdinalIgnoreCase) < 0 ||
                    request.IndexOf("example_echo", StringComparison.OrdinalIgnoreCase) < 0 ||
                    request.IndexOf("\"parameters\"", StringComparison.OrdinalIgnoreCase) < 0)
                    throw new InvalidOperationException("Provider request did not contain the expected tool definition.");
                if (response.ToolCalls == null || response.ToolCalls.Count != 1 || response.ToolCalls[0].Name != "example_echo")
                    throw new InvalidOperationException("Provider response tool call was not normalized correctly.");

                Write("PROVIDER TOOL TRANSPORT",
                    "Contract test succeeded." + Environment.NewLine +
                    "Selected execution target model: " + provider.DefaultModel + Environment.NewLine +
                    "Tool definition sent: " + tool.Name + Environment.NewLine +
                    "Schema sent: yes" + Environment.NewLine +
                    "Response tool call: " + response.ToolCalls[0].Name + Environment.NewLine +
                    "HTTP: local test handler only.");
            }
        }

        private async Task TestToolLoopAsync(string input)
        {
            var handler = new ToolLoopRequestHandler();
            using (var httpClient = new HttpClient(handler))
            {
                var adapter = new OpenAICompatibleProviderAdapter(httpClient);
                var provider = new AiProvider
                {
                    Id = "provider-loop-example",
                    Name = "Example Loop Provider",
                    Kind = OpenAICompatibleProviderAdapter.ProviderKind,
                    BaseUrl = "https://example.invalid/v1",
                    DefaultModel = "example-model",
                    Enabled = true
                };
                var tool = new AiTool
                {
                    Id = "example.add",
                    Name = "example_add",
                    Description = "Adds two integer values.",
                    InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"integer\"},\"b\":{\"type\":\"integer\"}},\"required\":[\"a\",\"b\"],\"additionalProperties\":false}",
                    Type = AiToolType.Application,
                    Enabled = true
                };
                var agent = new AiAgent
                {
                    Id = "example-loop-agent",
                    Name = "Example Loop Agent",
                    ToolIds = new List<string> { tool.Id },
                    Enabled = true
                };

                var store = new InMemoryAiStore();
                await store.SaveProviderAsync(provider, CancellationToken.None);
                await store.SaveAgentAsync(agent, CancellationToken.None);

                var toolClient = new HAgentClient(store, new EmptySecretStore(), new IAiProviderAdapter[] { adapter });
                toolClient.RegisterTool(new DelegateAgentTool(tool, context =>
                {
                    object a;
                    object b;
                    context.Arguments.TryGetValue("a", out a);
                    context.Arguments.TryGetValue("b", out b);
                    return Task.FromResult(ToolExecutionResult.Success((Convert.ToInt32(a) + Convert.ToInt32(b)).ToString()));
                }));

                var loop = await toolClient.RunToolLoopAsync(
                    agent.Id,
                    string.IsNullOrWhiteSpace(input) ? "Use the add tool." : input,
                    4, 4, CancellationToken.None);

                if (loop.Turns != 2 || loop.ToolCallsExecuted != 1)
                    throw new InvalidOperationException("The canonical tool loop did not complete with the expected turns/tool calls.");
                if (loop.Response == null || loop.Response.Text != "The tool returned 7.")
                    throw new InvalidOperationException("The final model response did not consume the tool result as expected.");
                if (handler.RequestCount != 2)
                    throw new InvalidOperationException("Expected exactly two provider requests.");

                Write("TOOL LOOP",
                    "Contract test succeeded." + Environment.NewLine +
                    "Turns: " + loop.Turns + Environment.NewLine +
                    "Tool calls executed: " + loop.ToolCallsExecuted + Environment.NewLine +
                    "Final response: " + loop.Response.Text + Environment.NewLine +
                    "Provider requests: " + handler.RequestCount + Environment.NewLine +
                    "Canonical execution target selection: provider default model used through runtime catalog.");
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
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
            }
        }

        private sealed class ToolLoopRequestHandler : HttpMessageHandler
        {
            public int RequestCount { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;
                var requestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (RequestCount == 1)
                {
                    var body = "{\"id\":\"loop-1\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":null,\"tool_calls\":[{\"id\":\"call-add-42\",\"type\":\"function\",\"function\":{\"name\":\"example_add\",\"arguments\":\"{\\\"a\\\":3,\\\"b\\\":4}\"}}]}}]}";
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
                }

                var parsed = JObject.Parse(requestBody);
                var messages = parsed["messages"] as JArray;
                JToken toolMessage = null;
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        if (message != null && string.Equals(Convert.ToString(message["role"]), "tool", StringComparison.OrdinalIgnoreCase))
                        {
                            toolMessage = message;
                            break;
                        }
                    }
                }

                if (toolMessage == null ||
                    Convert.ToString(toolMessage["tool_call_id"]) != "call-add-42" ||
                    Convert.ToString(toolMessage["content"]) != "7")
                    throw new InvalidOperationException("The second provider request did not contain the expected validated tool result.");

                var finalBody = "{\"id\":\"loop-2\",\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"The tool returned 7.\"}}]}";
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(finalBody, System.Text.Encoding.UTF8, "application/json") };
            }
        }

        private sealed class EmptySecretStore : ISecretStore
        {
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.FromResult(string.Empty); }
            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken)) { return Task.CompletedTask; }
        }
    }
}

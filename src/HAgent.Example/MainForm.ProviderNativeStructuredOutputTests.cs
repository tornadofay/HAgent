using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddProviderNativeStructuredOutputTab()
        {
            AddApiTab(
                "PROVIDER NATIVE STRUCTURED OUTPUT",
                "Run native structured-output test",
                "Verifies that the OpenAI-compatible adapter sends the host schema as a native response_format request and normalizes the response.",
                "The local HTTP handler should observe response_format/type=json_schema and the exact host schema. No external provider is contacted.",
                "Provider-native structured-output verification.",
                TestProviderNativeStructuredOutputAsync,
                "Native schema transport",
                "Uses a local HTTP handler only; no external provider is contacted.");
        }

        private async Task TestProviderNativeStructuredOutputAsync(string message)
        {
            var handler = new NativeStructuredOutputHttpHandler();
            using (var httpClient = new HttpClient(handler))
            {
                var adapter = new OpenAICompatibleProviderAdapter(httpClient);
                var provider = new AiProvider
                {
                    Id = "native-provider-42",
                    Name = "Native Structured Output Test",
                    Kind = OpenAICompatibleProviderAdapter.ProviderKind,
                    BaseUrl = "https://hagent-native-structured-output.test/v1",
                    DefaultModel = "native-model-42"
                };
                var agent = new AiAgent
                {
                    Id = "native-agent-42",
                    Name = "Native Structured Output Agent",
                    ProviderId = provider.Id,
                    Model = provider.DefaultModel,
                    Temperature = 0.2d,
                    MaxOutputTokens = 64
                };
                var schema = "{\"type\":\"object\",\"properties\":{\"status\":{\"type\":\"string\"}},\"required\":[\"status\"],\"additionalProperties\":false}";
                var request = new ProviderExecutionRequest
                {
                    Provider = provider,
                    Agent = agent,
                    Messages = new[] { new AIMessage("user", "return structured output") },
                    StructuredOutput = new StructuredOutputOptions
                    {
                        SchemaJson = schema,
                        RequireValidJson = true
                    }
                };

                var response = await adapter.SendAsync(request, CancellationToken.None).ConfigureAwait(true);
                if (!handler.RequestReceived)
                    throw new InvalidOperationException("The local provider handler did not receive the request.");
                if (!handler.NativeResponseFormatObserved)
                    throw new InvalidOperationException("The provider request did not contain native response_format/json_schema fields.");
                if (!string.Equals(handler.SchemaJson, schema, StringComparison.Ordinal))
                    throw new InvalidOperationException("The native provider request did not preserve the exact host schema.");
                if (response == null || !string.Equals(response.StructuredOutputJson, "{\"status\":\"active\"}", StringComparison.Ordinal))
                    throw new InvalidOperationException("The native structured-output response was not normalized as expected.");
                if (response.ProviderMetadata == null || !response.ProviderMetadata.ContainsKey("structured_output_native") ||
                    !(response.ProviderMetadata["structured_output_native"] is bool) ||
                    !(bool)response.ProviderMetadata["structured_output_native"])
                    throw new InvalidOperationException("Native structured-output metadata was not reported.");

                Write("PROVIDER NATIVE STRUCTURED OUTPUT",
                    "Contract test succeeded." + Environment.NewLine +
                    "Native response_format observed: yes" + Environment.NewLine +
                    "Response format type: json_schema" + Environment.NewLine +
                    "Schema preserved: yes" + Environment.NewLine +
                    "Normalized structured output: " + response.StructuredOutputJson + Environment.NewLine +
                    "Native enforcement metadata: yes" + Environment.NewLine +
                    "Provider network calls: local test handler only.");
            }
        }

        private sealed class NativeStructuredOutputHttpHandler : HttpMessageHandler
        {
            public bool RequestReceived { get; private set; }
            public bool NativeResponseFormatObserved { get; private set; }
            public string SchemaJson { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestReceived = true;
                var body = request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);

                NativeResponseFormatObserved =
                    body.IndexOf("\"response_format\"", StringComparison.Ordinal) >= 0 &&
                    body.IndexOf("\"type\":\"json_schema\"", StringComparison.Ordinal) >= 0 &&
                    body.IndexOf("\"json_schema\"", StringComparison.Ordinal) >= 0;

                const string marker = "\"schema\":";
                var markerIndex = body.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex >= 0)
                {
                    var schemaStart = markerIndex + marker.Length;
                    var depth = 0;
                    var inString = false;
                    var escaped = false;
                    for (var i = schemaStart; i < body.Length; i++)
                    {
                        var ch = body[i];
                        if (inString)
                        {
                            if (escaped) escaped = false;
                            else if (ch == '\\') escaped = true;
                            else if (ch == '"') inString = false;
                            continue;
                        }

                        if (ch == '"')
                        {
                            inString = true;
                            continue;
                        }
                        if (ch == '{' || ch == '[') depth++;
                        else if (ch == '}' || ch == ']')
                        {
                            depth--;
                            if (depth == 0)
                            {
                                SchemaJson = body.Substring(schemaStart, i - schemaStart + 1);
                                break;
                            }
                        }
                    }
                }

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\":\"native-response-42\",\"choices\":[{\"message\":{\"content\":\"{\\\"status\\\":\\\"active\\\"}\"}}]}",
                        System.Text.Encoding.UTF8,
                        "application/json")
                };
                return response;
            }
        }
    }
}

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
                "Verifies native OpenAI-compatible JSON Schema transport and safe fallback when the endpoint does not support response_format.",
                "The local HTTP handler exercises both native success and unsupported-feature fallback. No external provider is contacted.",
                "Provider-native structured-output verification.",
                TestProviderNativeStructuredOutputAsync,
                "Native schema transport + fallback",
                "Uses a local HTTP handler only; no external provider is contacted.");
        }

        private async Task TestProviderNativeStructuredOutputAsync(string message)
        {
            var schema = "{\"type\":\"object\",\"properties\":{\"status\":{\"type\":\"string\"}},\"required\":[\"status\"],\"additionalProperties\":false}";
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

            var nativeHandler = new NativeStructuredOutputHttpHandler(false);
            using (var nativeHttpClient = new HttpClient(nativeHandler))
            {
                var adapter = new OpenAICompatibleProviderAdapter(nativeHttpClient);
                var response = await adapter.SendAsync(request, CancellationToken.None).ConfigureAwait(true);

                if (!nativeHandler.RequestReceived || !nativeHandler.NativeResponseFormatObserved)
                    throw new InvalidOperationException("The native provider request did not contain the expected response_format/json_schema payload.");
                if (!string.Equals(nativeHandler.SchemaJson, schema, StringComparison.Ordinal))
                    throw new InvalidOperationException("The native provider request did not preserve the exact host schema.");
                if (response == null || !string.Equals(response.StructuredOutputJson, "{\"status\":\"active\"}", StringComparison.Ordinal))
                    throw new InvalidOperationException("The native structured-output response was not normalized as expected.");
                if (!HasBooleanMetadata(response, "structured_output_native", true))
                    throw new InvalidOperationException("Native structured-output metadata was not reported.");
            }

            var fallbackHandler = new NativeStructuredOutputHttpHandler(true);
            using (var fallbackHttpClient = new HttpClient(fallbackHandler))
            {
                var adapter = new OpenAICompatibleProviderAdapter(fallbackHttpClient);
                var response = await adapter.SendAsync(request, CancellationToken.None).ConfigureAwait(true);

                if (fallbackHandler.RequestCount != 2)
                    throw new InvalidOperationException("Unsupported native structured output should fall back with exactly one retry using the ordinary request shape.");
                if (!fallbackHandler.NativeResponseFormatObserved)
                    throw new InvalidOperationException("The initial fallback request did not attempt native response_format transport.");
                if (!fallbackHandler.FallbackRequestObservedWithoutResponseFormat)
                    throw new InvalidOperationException("The fallback request still contained response_format.");
                if (response == null || !string.Equals(response.StructuredOutputJson, "{\"status\":\"active\"}", StringComparison.Ordinal))
                    throw new InvalidOperationException("The fallback response was not normalized as expected.");
                if (!HasBooleanMetadata(response, "structured_output_native", false) ||
                    !HasBooleanMetadata(response, "structured_output_native_fallback", true))
                    throw new InvalidOperationException("Structured-output fallback metadata was not reported correctly.");
            }

            Write("PROVIDER NATIVE STRUCTURED OUTPUT",
                "Contract test succeeded." + Environment.NewLine +
                "Native response_format observed: yes" + Environment.NewLine +
                "Response format type: json_schema" + Environment.NewLine +
                "Schema preserved: yes" + Environment.NewLine +
                "Native structured output normalized: yes" + Environment.NewLine +
                "Unsupported native feature fallback: verified" + Environment.NewLine +
                "Fallback request removed response_format: yes" + Environment.NewLine +
                "Provider network calls: local test handler only.");
        }

        private static bool HasBooleanMetadata(AIResponse response, string key, bool expected)
        {
            if (response == null || response.ProviderMetadata == null)
                return false;
            object value;
            if (!response.ProviderMetadata.TryGetValue(key, out value) || !(value is bool))
                return false;
            return (bool)value == expected;
        }

        private sealed class NativeStructuredOutputHttpHandler : HttpMessageHandler
        {
            private readonly bool _rejectNative;

            public NativeStructuredOutputHttpHandler(bool rejectNative)
            {
                _rejectNative = rejectNative;
            }

            public bool RequestReceived { get; private set; }
            public int RequestCount { get; private set; }
            public bool NativeResponseFormatObserved { get; private set; }
            public bool FallbackRequestObservedWithoutResponseFormat { get; private set; }
            public string SchemaJson { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestReceived = true;
                RequestCount++;
                var body = request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync().ConfigureAwait(false);

                var hasResponseFormat = body.IndexOf("\"response_format\"", StringComparison.Ordinal) >= 0;
                var isNative = hasResponseFormat &&
                               body.IndexOf("\"type\":\"json_schema\"", StringComparison.Ordinal) >= 0 &&
                               body.IndexOf("\"json_schema\"", StringComparison.Ordinal) >= 0;
                NativeResponseFormatObserved = NativeResponseFormatObserved || isNative;

                if (isNative)
                {
                    SchemaJson = ExtractJsonValue(body, "\"schema\":");
                    if (_rejectNative)
                    {
                        return CreateResponse(HttpStatusCode.BadRequest,
                            "{\"error\":{\"message\":\"response_format json_schema is not supported\"}}");
                    }
                }
                else if (_rejectNative && RequestCount == 2)
                {
                    FallbackRequestObservedWithoutResponseFormat = !hasResponseFormat;
                }

                return CreateResponse(HttpStatusCode.OK,
                    "{\"id\":\"native-response-42\",\"choices\":[{\"message\":{\"content\":\"{\\\"status\\\":\\\"active\\\"}\"}}]}");
            }

            private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content)
            {
                return new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
                };
            }

            private static string ExtractJsonValue(string text, string marker)
            {
                var markerIndex = text.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex < 0) return string.Empty;
                var start = markerIndex + marker.Length;
                var depth = 0;
                var inString = false;
                var escaped = false;
                for (var i = start; i < text.Length; i++)
                {
                    var ch = text[i];
                    if (inString)
                    {
                        if (escaped) escaped = false;
                        else if (ch == '\\') escaped = true;
                        else if (ch == '"') inString = false;
                        continue;
                    }
                    if (ch == '"') { inString = true; continue; }
                    if (ch == '{' || ch == '[') depth++;
                    else if (ch == '}' || ch == ']')
                    {
                        depth--;
                        if (depth == 0)
                            return text.Substring(start, i - start + 1);
                    }
                }
                return string.Empty;
            }
        }
    }
}

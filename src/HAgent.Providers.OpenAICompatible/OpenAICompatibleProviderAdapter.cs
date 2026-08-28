using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using Newtonsoft.Json;

namespace HAgent.Providers.OpenAICompatible
{
    /// <summary>
    /// Adapter for providers exposing an OpenAI-compatible /chat/completions endpoint.
    /// Uses Newtonsoft.Json so the same source works on .NET Framework 4.8.1 and modern .NET.
    /// </summary>
    public sealed class OpenAICompatibleProviderAdapter : IAiProviderAdapter, IProviderConnectionTester, IProviderModelCatalog, IProviderModelCapabilities, IProviderStreamingAdapter
    {
        public const string ProviderKind = "openai-compatible";

        private readonly HttpClient _httpClient;

        public OpenAICompatibleProviderAdapter(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public string Kind => ProviderKind;
        public string DisplayName => "OpenAI-Compatible";

        public bool CanHandle(AiProvider provider)
        {
            return provider != null &&
                   string.Equals(provider.Kind, ProviderKind, StringComparison.OrdinalIgnoreCase);
        }

        public Task<AiModelCapabilities> GetCapabilitiesAsync(
            AiProvider provider,
            string model,
            string apiKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            var result = new AiModelCapabilities
            {
                Model = model ?? string.Empty
            };

            result.Set(AiCapability.Chat, CapabilitySupport.Supported, CapabilitySource.AdapterKnowledge, 0.95d,
                "The OpenAI-compatible adapter establishes support for the chat transport shape.");
            result.Set(AiCapability.Streaming, CapabilitySupport.Supported, CapabilitySource.AdapterKnowledge, 0.90d,
                "The adapter supports OpenAI-compatible Server-Sent Events streaming.");
            result.Set(AiCapability.StructuredOutput, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
            result.Set(AiCapability.ToolCalling, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
            result.Set(AiCapability.Vision, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
            result.Set(AiCapability.AudioInput, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
            result.Set(AiCapability.AudioOutput, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
            result.Set(AiCapability.Embeddings, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
            result.Set(AiCapability.Reasoning, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");

            return Task.FromResult(result);
        }

        public async Task TestConnectionAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var models = await GetModelsAsync(provider, apiKey, cancellationToken).ConfigureAwait(false);
            if (models == null)
                throw new InvalidOperationException("The provider returned no model catalog.");
        }

        public async Task<IReadOnlyList<string>> GetModelsAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var url = NormalizeModelsEndpoint(provider.BaseUrl);

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (!string.IsNullOrWhiteSpace(apiKey))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw new HttpRequestException("AI provider model request returned " + (int)response.StatusCode + ": " + body);

                    var dto = JsonConvert.DeserializeObject<ModelListResponse>(body);
                    var result = new List<string>();
                    if (dto != null && dto.Data != null)
                    {
                        foreach (var item in dto.Data)
                        {
                            if (item == null || string.IsNullOrWhiteSpace(item.Id))
                                continue;

                            if (result.FindIndex(x => string.Equals(x, item.Id, StringComparison.OrdinalIgnoreCase)) < 0)
                                result.Add(item.Id);
                        }
                    }
                    result.Sort(StringComparer.OrdinalIgnoreCase);
                    return result.AsReadOnly();
                }
            }
        }

        public async Task<AIResponse> SendAsync(
            AiProvider provider,
            AiAgent agent,
            string apiKey,
            string systemPrompt,
            IReadOnlyList<AIMessage> messages,
            CancellationToken cancellationToken)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (agent == null) throw new ArgumentNullException(nameof(agent));

            var url = NormalizeEndpoint(provider.BaseUrl);
            var request = new ChatCompletionRequest
            {
                Model = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model,
                Messages = ToRequestDtos(messages, systemPrompt),
                Temperature = agent.Temperature,
                MaxTokens = agent.MaxOutputTokens
            };

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var json = JsonConvert.SerializeObject(request);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    httpRequest.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);
                }

                using (var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            "AI provider returned " + (int)response.StatusCode + ": " + body);
                    }

                    var dto = JsonConvert.DeserializeObject<ChatCompletionResponse>(body);
                    if (dto == null || dto.Choices == null || dto.Choices.Count == 0)
                    {
                        throw new InvalidOperationException("The AI provider returned no choices.");
                    }

                    var choice = dto.Choices[0];
                    var content = choice.Message == null ? string.Empty : choice.Message.Content ?? string.Empty;
                    var reasoning = choice.Message == null ? string.Empty : choice.Message.ReasoningContent ?? string.Empty;
                    var toolCalls = NormalizeToolCalls(choice.Message == null ? null : choice.Message.ToolCalls);
                    var structuredOutput = IsJsonDocument(content) ? content.Trim() : string.Empty;
                    var normalizedUsage = NormalizeUsage(dto.Usage);

                    var usage = new Dictionary<string, object>();
                    if (dto.Usage != null)
                    {
                        usage["prompt_tokens"] = dto.Usage.PromptTokens;
                        usage["completion_tokens"] = dto.Usage.CompletionTokens;
                        usage["total_tokens"] = dto.Usage.TotalTokens;
                        if (dto.Usage.PromptTokensDetails != null)
                            usage["prompt_tokens_details"] = dto.Usage.PromptTokensDetails;
                        if (dto.Usage.CompletionTokensDetails != null)
                            usage["completion_tokens_details"] = dto.Usage.CompletionTokensDetails;
                    }

                    var metadata = new Dictionary<string, object>();
                    if (!string.IsNullOrWhiteSpace(dto.Id)) metadata["provider_request_id"] = dto.Id;
                    if (!string.IsNullOrWhiteSpace(reasoning)) metadata["reasoning_source"] = "provider-field";
                    if (toolCalls.Count > 0) metadata["tool_call_count"] = toolCalls.Count;
                    if (!string.IsNullOrWhiteSpace(structuredOutput)) metadata["structured_output_detected"] = true;
                    if (!string.IsNullOrWhiteSpace(content) && content.IndexOf("<think>", StringComparison.OrdinalIgnoreCase) >= 0)
                        metadata["reasoning_markup_detected"] = true;

                    return new AIResponse
                    {
                        AgentId = agent.Id,
                        ProviderId = provider.Id,
                        Model = request.Model,
                        Text = content,
                        Reasoning = reasoning,
                        RawText = content,
                        StructuredOutputJson = structuredOutput,
                        ToolCalls = toolCalls.AsReadOnly(),
                        RequestId = dto.Id ?? string.Empty,
                        CreatedAt = DateTimeOffset.UtcNow,
                        NormalizedUsage = normalizedUsage,
                        Usage = usage,
                        ProviderMetadata = metadata
                    };
                }
            }
        }

        public async Task<AIResponse> SendStreamingAsync(
            AiProvider provider,
            AiAgent agent,
            string apiKey,
            string systemPrompt,
            IReadOnlyList<AIMessage> messages,
            IProgress<AIResponseDelta> progress,
            CancellationToken cancellationToken)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (agent == null) throw new ArgumentNullException(nameof(agent));

            var url = NormalizeEndpoint(provider.BaseUrl);
            var request = new ChatCompletionRequest
            {
                Model = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model,
                Messages = ToRequestDtos(messages, systemPrompt),
                Temperature = agent.Temperature,
                MaxTokens = agent.MaxOutputTokens,
                Stream = true
            };

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var json = JsonConvert.SerializeObject(request);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                if (!string.IsNullOrWhiteSpace(apiKey))
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using (var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new HttpRequestException("AI provider returned " + (int)response.StatusCode + ": " + errorBody);
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var reader = new StreamReader(stream))
                    {
                        var text = new StringBuilder();
                        var reasoning = new StringBuilder();
                        var toolBuilders = new Dictionary<int, StreamingToolBuilder>();
                        var requestId = string.Empty;
                        AIUsage normalizedUsage = new AIUsage();

                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            string line;
                            try
                            {
                                line = await reader.ReadLineAsync().ConfigureAwait(false);
                            }
                            catch (Exception) when (cancellationToken.IsCancellationRequested)
                            {
                                throw new OperationCanceledException(cancellationToken);
                            }

                            if (line == null) break;
                            if (line.Length == 0 || !line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;

                            var payload = line.Substring(5).TrimStart();
                            if (string.Equals(payload, "[DONE]", StringComparison.OrdinalIgnoreCase)) break;
                            if (string.IsNullOrWhiteSpace(payload)) continue;

                            StreamResponse chunk;
                            try
                            {
                                chunk = JsonConvert.DeserializeObject<StreamResponse>(payload);
                            }
                            catch (JsonException)
                            {
                                continue;
                            }

                            if (chunk == null) continue;
                            if (string.IsNullOrWhiteSpace(requestId)) requestId = chunk.Id ?? string.Empty;
                            if (chunk.Usage != null) normalizedUsage = NormalizeUsage(chunk.Usage);
                            if (chunk.Choices == null) continue;

                            foreach (var choice in chunk.Choices)
                            {
                                if (choice == null || choice.Delta == null) continue;

                                var textDelta = choice.Delta.Content ?? string.Empty;
                                var reasoningDelta = choice.Delta.ReasoningContent ?? string.Empty;
                                if (!string.IsNullOrEmpty(textDelta)) text.Append(textDelta);
                                if (!string.IsNullOrEmpty(reasoningDelta)) reasoning.Append(reasoningDelta);

                                if (choice.Delta.ToolCalls != null)
                                {
                                    foreach (var call in choice.Delta.ToolCalls)
                                    {
                                        if (call == null) continue;
                                        StreamingToolBuilder builder;
                                        if (!toolBuilders.TryGetValue(call.Index, out builder))
                                        {
                                            builder = new StreamingToolBuilder();
                                            toolBuilders.Add(call.Index, builder);
                                        }

                                        if (!string.IsNullOrWhiteSpace(call.Id)) builder.Id = call.Id;
                                        if (call.Function != null)
                                        {
                                            if (!string.IsNullOrWhiteSpace(call.Function.Name)) builder.Name = call.Function.Name;
                                            if (!string.IsNullOrEmpty(call.Function.Arguments)) builder.Arguments.Append(call.Function.Arguments);
                                        }
                                    }
                                }

                                if (progress != null && (!string.IsNullOrEmpty(textDelta) || !string.IsNullOrEmpty(reasoningDelta)))
                                {
                                    progress.Report(new AIResponseDelta
                                    {
                                        Text = textDelta,
                                        Reasoning = reasoningDelta
                                    });
                                }

                                if (progress != null && choice.Delta.ToolCalls != null)
                                {
                                    foreach (var call in choice.Delta.ToolCalls)
                                    {
                                        if (call == null) continue;
                                        progress.Report(new AIResponseDelta
                                        {
                                            ToolCallId = call.Id ?? string.Empty,
                                            ToolCallName = call.Function == null ? string.Empty : call.Function.Name ?? string.Empty,
                                            ToolCallArgumentsDelta = call.Function == null ? string.Empty : call.Function.Arguments ?? string.Empty
                                        });
                                    }
                                }
                            }
                        }

                        var toolCalls = new List<AIToolCall>();
                        foreach (var pair in toolBuilders)
                        {
                            var builder = pair.Value;
                            if (string.IsNullOrWhiteSpace(builder.Name)) continue;
                            toolCalls.Add(new AIToolCall(builder.Id ?? string.Empty, builder.Name, builder.Arguments.ToString()));
                        }

                        var finalText = text.ToString();
                        var structuredOutput = IsJsonDocument(finalText) ? finalText.Trim() : string.Empty;
                        var metadata = new Dictionary<string, object>();
                        if (!string.IsNullOrWhiteSpace(requestId)) metadata["provider_request_id"] = requestId;
                        metadata["streaming"] = true;
                        if (toolCalls.Count > 0) metadata["tool_call_count"] = toolCalls.Count;
                        if (!string.IsNullOrWhiteSpace(reasoning.ToString())) metadata["reasoning_source"] = "provider-field";

                        return new AIResponse
                        {
                            AgentId = agent.Id,
                            ProviderId = provider.Id,
                            Model = request.Model,
                            Text = finalText,
                            Reasoning = reasoning.ToString(),
                            RawText = finalText,
                            StructuredOutputJson = structuredOutput,
                            ToolCalls = toolCalls.AsReadOnly(),
                            RequestId = requestId,
                            CreatedAt = DateTimeOffset.UtcNow,
                            NormalizedUsage = normalizedUsage,
                            Usage = normalizedUsage.ProviderUsage,
                            ProviderMetadata = metadata
                        };
                    }
                }
            }
        }

        private static AIUsage NormalizeUsage(UsageDto usage)
        {
            var result = new AIUsage();
            if (usage == null) return result;

            result.PromptTokens = usage.PromptTokens;
            result.CompletionTokens = usage.CompletionTokens;
            result.TotalTokens = usage.TotalTokens;
            if (usage.PromptTokensDetails != null)
                result.CachedPromptTokens = usage.PromptTokensDetails.CachedTokens;
            if (usage.CompletionTokensDetails != null)
                result.ReasoningTokens = usage.CompletionTokensDetails.ReasoningTokens;

            result.ProviderUsage["prompt_tokens"] = usage.PromptTokens;
            result.ProviderUsage["completion_tokens"] = usage.CompletionTokens;
            result.ProviderUsage["total_tokens"] = usage.TotalTokens;
            if (usage.PromptTokensDetails != null)
                result.ProviderUsage["prompt_tokens_details"] = usage.PromptTokensDetails;
            if (usage.CompletionTokensDetails != null)
                result.ProviderUsage["completion_tokens_details"] = usage.CompletionTokensDetails;

            return result;
        }

        private static List<AIToolCall> NormalizeToolCalls(IReadOnlyList<ResponseToolCallDto> calls)
        {
            var result = new List<AIToolCall>();
            if (calls == null) return result;

            foreach (var call in calls)
            {
                if (call == null || call.Function == null || string.IsNullOrWhiteSpace(call.Function.Name))
                    continue;

                result.Add(new AIToolCall(
                    call.Id ?? string.Empty,
                    call.Function.Name,
                    call.Function.Arguments ?? string.Empty));
            }

            return result;
        }

        private static bool IsJsonDocument(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            var value = text.Trim();
            if (!(value.StartsWith("{", StringComparison.Ordinal) || value.StartsWith("[", StringComparison.Ordinal)))
                return false;

            try
            {
                JsonConvert.DeserializeObject(value);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string NormalizeEndpoint(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Provider Base URL is required.");

            return baseUrl.TrimEnd('/') + "/chat/completions";
        }

        private static string NormalizeModelsEndpoint(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Provider Base URL is required.");

            return baseUrl.TrimEnd('/') + "/models";
        }

        private static List<RequestChatMessageDto> ToRequestDtos(IReadOnlyList<AIMessage> messages, string systemPrompt)
        {
            var result = new List<RequestChatMessageDto>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                result.Add(new RequestChatMessageDto { Role = "system", Content = systemPrompt });
            }

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    if (message == null) continue;
                    result.Add(new RequestChatMessageDto { Role = message.Role, Content = message.Content });
                }
            }

            return result;
        }

        private sealed class ChatCompletionRequest
        {
            [JsonProperty("model")]
            public string Model { get; set; }

            [JsonProperty("messages")]
            public List<RequestChatMessageDto> Messages { get; set; }

            [JsonProperty("temperature")]
            public double? Temperature { get; set; }

            [JsonProperty("max_tokens")]
            public int? MaxTokens { get; set; }

            [JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Stream { get; set; }
        }

        private sealed class RequestChatMessageDto
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }

        private sealed class ResponseChatMessageDto
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("reasoning_content")]
            public string ReasoningContent { get; set; }

            [JsonProperty("tool_calls")]
            public List<ResponseToolCallDto> ToolCalls { get; set; }
        }

        private sealed class ResponseToolCallDto
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("function")]
            public ResponseToolFunctionDto Function { get; set; }
        }

        private sealed class ResponseToolFunctionDto
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("arguments")]
            public string Arguments { get; set; }
        }

        private sealed class ChatCompletionResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("choices")]
            public List<ChoiceDto> Choices { get; set; }

            [JsonProperty("usage")]
            public UsageDto Usage { get; set; }
        }

        private sealed class ChoiceDto
        {
            [JsonProperty("message")]
            public ResponseChatMessageDto Message { get; set; }
        }

        private sealed class StreamResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("choices")]
            public List<StreamChoiceDto> Choices { get; set; }

            [JsonProperty("usage")]
            public UsageDto Usage { get; set; }
        }

        private sealed class StreamChoiceDto
        {
            [JsonProperty("index")]
            public int Index { get; set; }

            [JsonProperty("delta")]
            public StreamDeltaDto Delta { get; set; }
        }

        private sealed class StreamDeltaDto
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("reasoning_content")]
            public string ReasoningContent { get; set; }

            [JsonProperty("tool_calls")]
            public List<StreamToolCallDto> ToolCalls { get; set; }
        }

        private sealed class StreamToolCallDto
        {
            [JsonProperty("index")]
            public int Index { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("function")]
            public ResponseToolFunctionDto Function { get; set; }
        }

        private sealed class StreamingToolBuilder
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public StringBuilder Arguments { get; private set; }

            public StreamingToolBuilder()
            {
                Id = string.Empty;
                Name = string.Empty;
                Arguments = new StringBuilder();
            }
        }

        private sealed class ModelListResponse
        {
            [JsonProperty("data")]
            public List<ModelDto> Data { get; set; }
        }

        private sealed class ModelDto
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        private sealed class UsageDto
        {
            [JsonProperty("prompt_tokens")]
            public long? PromptTokens { get; set; }

            [JsonProperty("completion_tokens")]
            public long? CompletionTokens { get; set; }

            [JsonProperty("total_tokens")]
            public long? TotalTokens { get; set; }

            [JsonProperty("prompt_tokens_details")]
            public PromptTokensDetailsDto PromptTokensDetails { get; set; }

            [JsonProperty("completion_tokens_details")]
            public CompletionTokensDetailsDto CompletionTokensDetails { get; set; }
        }

        private sealed class PromptTokensDetailsDto
        {
            [JsonProperty("cached_tokens")]
            public long? CachedTokens { get; set; }
        }

        private sealed class CompletionTokensDetailsDto
        {
            [JsonProperty("reasoning_tokens")]
            public long? ReasoningTokens { get; set; }
        }
    }
}

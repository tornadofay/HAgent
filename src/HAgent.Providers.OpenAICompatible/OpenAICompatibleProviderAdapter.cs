using System;
using System.Collections.Generic;
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
    public sealed class OpenAICompatibleProviderAdapter : IAiProviderAdapter, IProviderConnectionTester, IProviderModelCatalog, IProviderModelCapabilities
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

            // The OpenAI-compatible transport guarantees the chat request shape,
            // but the endpoint does not guarantee which optional model features exist.
            result.Set(AiCapability.Chat, CapabilitySupport.Supported, CapabilitySource.AdapterKnowledge, 0.95d,
                "The OpenAI-compatible adapter establishes support for the chat transport shape.");
            result.Set(AiCapability.Streaming, CapabilitySupport.Unknown, CapabilitySource.Unknown, 0d, "Not established by the adapter.");
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

                    var usage = new Dictionary<string, object>();
                    if (dto.Usage != null)
                    {
                        usage["prompt_tokens"] = dto.Usage.PromptTokens;
                        usage["completion_tokens"] = dto.Usage.CompletionTokens;
                        usage["total_tokens"] = dto.Usage.TotalTokens;
                    }

                    var metadata = new Dictionary<string, object>();
                    if (!string.IsNullOrWhiteSpace(dto.Id)) metadata["provider_request_id"] = dto.Id;
                    if (!string.IsNullOrWhiteSpace(reasoning)) metadata["reasoning_source"] = "provider-field";
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
                        RequestId = dto.Id ?? string.Empty,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Usage = usage,
                        ProviderMetadata = metadata
                    };
                }
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
            public int PromptTokens { get; set; }

            [JsonProperty("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonProperty("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}

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
    public sealed class OpenAICompatibleProviderAdapter : IAiProviderAdapter, IProviderConnectionTester, IProviderModelCatalog
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
                Messages = ToDtos(messages, systemPrompt),
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

                    var text = dto.Choices[0].Message == null
                        ? string.Empty
                        : dto.Choices[0].Message.Content ?? string.Empty;

                    var usage = new Dictionary<string, object>();
                    if (dto.Usage != null)
                    {
                        usage["prompt_tokens"] = dto.Usage.PromptTokens;
                        usage["completion_tokens"] = dto.Usage.CompletionTokens;
                        usage["total_tokens"] = dto.Usage.TotalTokens;
                    }

                    return new AIResponse
                    {
                        AgentId = agent.Id,
                        ProviderId = provider.Id,
                        Model = request.Model,
                        Text = text,
                        RequestId = dto.Id ?? string.Empty,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Usage = usage
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

        private static List<ChatMessageDto> ToDtos(
            IReadOnlyList<AIMessage> messages,
            string systemPrompt)
        {
            var result = new List<ChatMessageDto>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                result.Add(new ChatMessageDto
                {
                    Role = "system",
                    Content = systemPrompt
                });
            }

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    if (message == null) continue;

                    result.Add(new ChatMessageDto
                    {
                        Role = message.Role,
                        Content = message.Content
                    });
                }
            }

            return result;
        }

        private sealed class ChatCompletionRequest
        {
            [JsonProperty("model")]
            public string Model { get; set; }

            [JsonProperty("messages")]
            public List<ChatMessageDto> Messages { get; set; }

            [JsonProperty("temperature")]
            public double? Temperature { get; set; }

            [JsonProperty("max_tokens")]
            public int? MaxTokens { get; set; }
        }

        private sealed class ChatMessageDto
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
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
            public ChatMessageDto Message { get; set; }
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

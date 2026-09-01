using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HAgent.Providers.OpenAICompatible
{
    public sealed partial class OpenAICompatibleProviderAdapter
    {
        private async Task<AIResponse> SendWithStructuredOutputAsync(
            ProviderExecutionRequest providerRequest,
            CancellationToken cancellationToken)
        {
            providerRequest.Validate();

            if (providerRequest.StructuredOutput == null)
                return await SendAsync(
                    providerRequest.Provider,
                    providerRequest.Agent,
                    providerRequest.ApiKey,
                    providerRequest.SystemPrompt,
                    providerRequest.Messages,
                    cancellationToken).ConfigureAwait(false);

            var url = NormalizeEndpoint(providerRequest.Provider.BaseUrl);
            var requestModel = string.IsNullOrWhiteSpace(providerRequest.Agent.Model)
                ? providerRequest.Provider.DefaultModel
                : providerRequest.Agent.Model;
            var transportRequest = new ChatCompletionRequest
            {
                Model = requestModel,
                Messages = ToRequestDtos(providerRequest.Messages, providerRequest.SystemPrompt),
                Temperature = providerRequest.Agent.Temperature,
                MaxTokens = providerRequest.Agent.MaxOutputTokens
            };

            var body = JObject.Parse(JsonConvert.SerializeObject(transportRequest));
            body["response_format"] = new JObject
            {
                ["type"] = "json_schema",
                ["json_schema"] = new JObject
                {
                    ["name"] = "hagent_response",
                    ["strict"] = true,
                    ["schema"] = JToken.Parse(providerRequest.StructuredOutput.SchemaJson)
                }
            };

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                httpRequest.Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json");
                if (!string.IsNullOrWhiteSpace(providerRequest.ApiKey))
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", providerRequest.ApiKey);

                using (var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (IsNativeStructuredOutputUnsupported(responseBody))
                        {
                            return await SendAsync(
                                providerRequest.Provider,
                                providerRequest.Agent,
                                providerRequest.ApiKey,
                                providerRequest.SystemPrompt,
                                providerRequest.Messages,
                                cancellationToken).ConfigureAwait(false);
                        }

                        throw new HttpRequestException(
                            "AI provider returned " + (int)response.StatusCode + ": " + responseBody);
                    }

                    var dto = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseBody);
                    if (dto == null || dto.Choices == null || dto.Choices.Count == 0)
                        throw new InvalidOperationException("The AI provider returned no choices.");

                    var choice = dto.Choices[0];
                    var content = choice.Message == null ? string.Empty : choice.Message.Content ?? string.Empty;
                    var reasoning = choice.Message == null ? string.Empty : choice.Message.ReasoningContent ?? string.Empty;
                    var toolCalls = NormalizeToolCalls(choice.Message == null ? null : choice.Message.ToolCalls);
                    var normalizedUsage = NormalizeUsage(dto.Usage);

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
                    metadata["structured_output_native"] = true;
                    if (toolCalls.Count > 0) metadata["tool_call_count"] = toolCalls.Count;

                    return new AIResponse
                    {
                        AgentId = providerRequest.Agent.Id,
                        ProviderId = providerRequest.Provider.Id,
                        Model = requestModel,
                        Text = content,
                        Reasoning = reasoning,
                        RawText = content,
                        StructuredOutputJson = content,
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

        private static bool IsNativeStructuredOutputUnsupported(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody)) return false;
            var value = responseBody.ToLowerInvariant();
            return value.Contains("response_format") &&
                   (value.Contains("not supported") ||
                    value.Contains("unsupported") ||
                    value.Contains("json_schema") ||
                    value.Contains("structured output"));
        }
    }
}

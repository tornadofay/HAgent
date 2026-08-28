using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<AIResponse> SendWithToolsAsync(
            AiProvider provider,
            AiAgent agent,
            string apiKey,
            string systemPrompt,
            IReadOnlyList<AIMessage> messages,
            IReadOnlyList<AiTool> tools,
            CancellationToken cancellationToken)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (agent == null) throw new ArgumentNullException(nameof(agent));

            var url = NormalizeEndpoint(provider.BaseUrl);
            var request = new
            {
                model = string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model,
                messages = ToToolRequestMessages(messages, systemPrompt),
                tools = BuildToolDefinitions(tools),
                temperature = agent.Temperature,
                max_tokens = agent.MaxOutputTokens
            };

            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, url))
            {
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                if (!string.IsNullOrWhiteSpace(apiKey))
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using (var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw new HttpRequestException("AI provider returned " + (int)response.StatusCode + ": " + body);

                    var dto = JsonConvert.DeserializeObject<ChatCompletionResponse>(body);
                    if (dto == null || dto.Choices == null || dto.Choices.Count == 0)
                        throw new InvalidOperationException("The AI provider returned no choices.");

                    var choice = dto.Choices[0];
                    var content = choice.Message == null ? string.Empty : choice.Message.Content ?? string.Empty;
                    var reasoning = choice.Message == null ? string.Empty : choice.Message.ReasoningContent ?? string.Empty;
                    var toolCalls = NormalizeToolCalls(choice.Message == null ? null : choice.Message.ToolCalls);
                    var structuredOutput = IsJsonDocument(content) ? content.Trim() : string.Empty;
                    var usage = NormalizeUsage(dto.Usage);
                    var metadata = new Dictionary<string, object>();
                    if (!string.IsNullOrWhiteSpace(dto.Id)) metadata["provider_request_id"] = dto.Id;
                    metadata["tools_sent"] = tools == null ? 0 : tools.Count;
                    if (toolCalls.Count > 0) metadata["tool_call_count"] = toolCalls.Count;
                    if (!string.IsNullOrWhiteSpace(reasoning)) metadata["reasoning_source"] = "provider-field";

                    return new AIResponse
                    {
                        AgentId = agent.Id,
                        ProviderId = provider.Id,
                        Model = request.model,
                        Text = content,
                        Reasoning = reasoning,
                        RawText = content,
                        StructuredOutputJson = structuredOutput,
                        ToolCalls = toolCalls.AsReadOnly(),
                        RequestId = dto.Id ?? string.Empty,
                        CreatedAt = DateTimeOffset.UtcNow,
                        NormalizedUsage = usage,
                        Usage = usage.ProviderUsage,
                        ProviderMetadata = metadata
                    };
                }
            }
        }

        private static List<object> BuildToolDefinitions(IReadOnlyList<AiTool> tools)
        {
            var result = new List<object>();
            if (tools == null) return result;

            foreach (var tool in tools.Where(x => x != null && x.Enabled && !string.IsNullOrWhiteSpace(x.Name)))
            {
                JToken parameters;
                try
                {
                    parameters = JToken.Parse(string.IsNullOrWhiteSpace(tool.InputSchemaJson) ? "{\"type\":\"object\",\"properties\":{}}" : tool.InputSchemaJson);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException("Tool '" + tool.Name + "' has invalid input schema JSON.", ex);
                }

                result.Add(new
                {
                    type = "function",
                    function = new
                    {
                        name = tool.Name,
                        description = tool.Description ?? string.Empty,
                        parameters = parameters
                    }
                });
            }

            return result;
        }

        private static List<object> ToToolRequestMessages(IReadOnlyList<AIMessage> messages, string systemPrompt)
        {
            var result = new List<object>();
            if (!string.IsNullOrWhiteSpace(systemPrompt))
                result.Add(new { role = "system", content = systemPrompt });

            if (messages == null) return result;

            foreach (var message in messages)
            {
                if (message == null) continue;

                if (string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase) && message.ToolCalls != null && message.ToolCalls.Count > 0)
                {
                    result.Add(new
                    {
                        role = "assistant",
                        content = string.IsNullOrEmpty(message.Content) ? null : message.Content,
                        tool_calls = message.ToolCalls.Select(x => new
                        {
                            id = x.Id,
                            type = "function",
                            function = new
                            {
                                name = x.Name,
                                arguments = x.ArgumentsJson ?? string.Empty
                            }
                        }).ToList()
                    });
                    continue;
                }

                if (string.Equals(message.Role, "tool", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new
                    {
                        role = "tool",
                        tool_call_id = message.ToolCallId ?? string.Empty,
                        content = message.Content ?? string.Empty
                    });
                    continue;
                }

                result.Add(new { role = message.Role, content = message.Content });
            }

            return result;
        }
    }
}

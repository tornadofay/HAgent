using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.Storage.File;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestLiveToolLoopAsync(string input)
        {
            var selected = GetSelectedAgent();
            if (selected == null)
                throw new InvalidOperationException("Select an agent first.");

            var sourceStore = new FileAiStore(Path.Combine(_basePath, "settings.json"));
            var sourceSecrets = new ProtectedDataSecretStore(Path.Combine(_basePath, "secrets"));
            var providers = await sourceStore.GetProvidersAsync();
            var providerIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(selected.ProviderId)) providerIds.Add(selected.ProviderId);
            if (selected.ProviderIds != null) providerIds.AddRange(selected.ProviderIds.Where(x => !string.IsNullOrWhiteSpace(x)));
            var provider = providerIds.Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(id => providers.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(p => p != null && p.Enabled);
            if (provider == null)
                throw new InvalidOperationException("The selected agent has no enabled provider.");

            var agent = CloneAgent(selected);
            var tool = new AiTool
            {
                Id = "example.live.add",
                Name = "example_add",
                Description = "Adds two integer values and returns the sum.",
                InputSchemaJson = "{\"type\":\"object\",\"properties\":{\"a\":{\"type\":\"integer\"},\"b\":{\"type\":\"integer\"}},\"required\":[\"a\",\"b\"],\"additionalProperties\":false}",
                Type = AiToolType.Application,
                Enabled = true
            };
            agent.ToolIds = new List<string> { tool.Id };

            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(CloneProvider(provider));
            await store.SaveAgentAsync(agent);

            var client = new HAgentClient(store, sourceSecrets, new IAiProviderAdapter[] { new OpenAICompatibleProviderAdapter() });
            var calls = 0;
            var argumentsSeen = string.Empty;
            var toolHandler = new DelegateAgentTool(tool, context =>
            {
                calls++;
                object a;
                object b;
                context.Arguments.TryGetValue("a", out a);
                context.Arguments.TryGetValue("b", out b);
                argumentsSeen = "a=" + Convert.ToString(a) + ", b=" + Convert.ToString(b);
                var sum = Convert.ToDecimal(a) + Convert.ToDecimal(b);
                return Task.FromResult(ToolExecutionResult.Success(sum.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            });
            client.RegisterTool(toolHandler);

            var request = RequireInput(string.IsNullOrWhiteSpace(input)
                ? "You must use the example_add tool with a=3 and b=4. After the tool returns, reply with the result in one short sentence."
                : input);

            try
            {
                var loop = await client.RunToolLoopAsync(selected.Id, request, 4, 4, CancellationToken.None);
                if (calls < 1)
                    throw new InvalidOperationException("The live provider completed without calling example_add. The selected model may not have chosen the tool.");

                Write("LIVE TOOL LOOP",
                    "Live provider test completed." + Environment.NewLine +
                    "Agent: " + selected.Name + Environment.NewLine +
                    "Provider: " + provider.Name + Environment.NewLine +
                    "Model: " + (string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model) + Environment.NewLine +
                    "Request: " + request + Environment.NewLine +
                    "Tool calls executed: " + loop.ToolCallsExecuted + Environment.NewLine +
                    "Tool arguments: " + argumentsSeen + Environment.NewLine +
                    "Turns: " + loop.Turns + Environment.NewLine +
                    "Final response: " + (loop.Response == null ? string.Empty : loop.Response.Text));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.IndexOf("tool-compatible provider", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ex.Message.IndexOf("tool calling", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ex.Message.IndexOf("Tool Calling", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Write("LIVE TOOL LOOP",
                        "Live provider test was not executed because the selected model/provider does not support tool calling." + Environment.NewLine +
                        "Agent: " + selected.Name + Environment.NewLine +
                        "Provider: " + provider.Name + Environment.NewLine +
                        "Model: " + (string.IsNullOrWhiteSpace(agent.Model) ? provider.DefaultModel : agent.Model) + Environment.NewLine +
                        "Request: " + request + Environment.NewLine +
                        "Action: Select a model with Tool Calling support and run the test again." + Environment.NewLine +
                        "Provider detail: " + ex.Message);
                    return;
                }

                throw;
            }
        }

        private static AiProvider CloneProvider(AiProvider source)
        {
            return new AiProvider
            {
                Id = source.Id,
                Name = source.Name,
                Kind = source.Kind,
                BaseUrl = source.BaseUrl,
                DefaultModel = source.DefaultModel,
                DefaultSystemPrompt = source.DefaultSystemPrompt,
                SecretId = source.SecretId,
                Enabled = source.Enabled
            };
        }

        private static AiAgent CloneAgent(AiAgent source)
        {
            return new AiAgent
            {
                Id = source.Id,
                Name = source.Name,
                ProviderId = source.ProviderId,
                ProviderIds = source.ProviderIds == null ? new List<string>() : new List<string>(source.ProviderIds),
                Model = source.Model,
                SystemPrompt = source.SystemPrompt,
                UseProviderSystemPrompt = source.UseProviderSystemPrompt,
                Temperature = source.Temperature,
                MaxOutputTokens = source.MaxOutputTokens,
                ToolIds = source.ToolIds == null ? new List<string>() : new List<string>(source.ToolIds),
                Enabled = source.Enabled
            };
        }
    }
}

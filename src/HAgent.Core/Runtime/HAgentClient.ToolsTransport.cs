using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        public async Task<AIResponse> SendWithToolsAsync(
            string agentId,
            string message,
            IReadOnlyList<AiTool> tools,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return await SendWithToolsAsync(
                agentId,
                new List<AIMessage> { new AIMessage("user", message) },
                tools,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<AIResponse> SendWithToolsAsync(
            string agentId,
            IReadOnlyList<AIMessage> messages,
            IReadOnlyList<AiTool> tools,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (messages == null || messages.Count == 0) throw new ArgumentException("At least one message is required.", nameof(messages));

            var enabledTools = (tools ?? new List<AiTool>())
                .Where(x => x != null && x.Enabled)
                .ToList()
                .AsReadOnly();
            if (enabledTools.Count == 0) throw new ArgumentException("At least one enabled tool is required.", nameof(tools));

            var requirements = new AiCapabilityRequirements();
            requirements.Prefer(AiCapability.ToolCalling);

            var planned = await PlanExecutionTargetAsync(
                agentId,
                requirements,
                cancellationToken).ConfigureAwait(false);

            var toolAdapter = planned.Adapter as IProviderToolCallingAdapter;
            if (toolAdapter == null)
                throw new InvalidOperationException("Provider adapter does not support tool-definition transport: " + planned.Provider.Name);

            var apiKey = string.IsNullOrWhiteSpace(planned.Provider.SecretId)
                ? string.Empty
                : await _secrets.GetAsync(planned.Provider.SecretId, cancellationToken).ConfigureAwait(false);

            return await toolAdapter.SendWithToolsAsync(
                new ProviderExecutionRequest
                {
                    Provider = planned.Provider,
                    Agent = planned.Agent,
                    ExecutionTarget = planned.Target,
                    ApiKey = apiKey,
                    SystemPrompt = BuildSystemPromptForClient(planned.Provider, planned.Agent),
                    Messages = new List<AIMessage>(messages).AsReadOnly(),
                    Tools = enabledTools
                },
                cancellationToken).ConfigureAwait(false);
        }

        private static string BuildSystemPromptForClient(AiProvider provider, AiAgent agent)
        {
            var layers = new List<SystemPromptLayer>();
            if (agent.UseProviderSystemPrompt && !string.IsNullOrWhiteSpace(provider.DefaultSystemPrompt))
                layers.Add(new SystemPromptLayer("provider", "Provider", provider.DefaultSystemPrompt, 100));
            if (!string.IsNullOrWhiteSpace(agent.SystemPrompt))
                layers.Add(new SystemPromptLayer("agent", "Agent", agent.SystemPrompt, 200));
            return SystemPromptComposer.Compose(layers);
        }
    }
}

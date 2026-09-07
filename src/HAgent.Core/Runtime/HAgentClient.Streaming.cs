using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed partial class HAgentClient
    {
        public Task<AIResponse> StreamAsync(
            string agentId,
            string message,
            IProgress<AIResponseDelta> progress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
            return StreamAsync(
                agentId,
                new List<AIMessage> { new AIMessage("user", message) },
                progress,
                cancellationToken);
        }

        public async Task<AIResponse> StreamAsync(
            string agentId,
            IReadOnlyList<AIMessage> messages,
            IProgress<AIResponseDelta> progress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("Agent id is required.", nameof(agentId));
            if (messages == null || messages.Count == 0) throw new ArgumentException("At least one message is required.", nameof(messages));

            var requirements = new AiCapabilityRequirements();
            requirements.Require(AiCapability.Streaming);

            var planned = await PlanExecutionTargetAsync(
                agentId,
                requirements,
                cancellationToken).ConfigureAwait(false);

            var streamingAdapter = planned.Adapter as IProviderStreamingAdapter;
            if (streamingAdapter == null)
                throw new InvalidOperationException("Provider adapter does not support streaming: " + planned.Provider.Name);

            var apiKey = string.IsNullOrWhiteSpace(planned.Provider.SecretId)
                ? string.Empty
                : await _secrets.GetAsync(planned.Provider.SecretId, cancellationToken).ConfigureAwait(false);

            return await streamingAdapter.SendStreamingAsync(
                new ProviderExecutionRequest
                {
                    Provider = planned.Provider,
                    Agent = planned.Agent,
                    ExecutionTarget = planned.Target,
                    ApiKey = apiKey,
                    SystemPrompt = BuildSystemPromptForClient(planned.Provider, planned.Agent),
                    Messages = new List<AIMessage>(messages).AsReadOnly(),
                    Progress = progress
                },
                cancellationToken).ConfigureAwait(false);
        }
    }
}

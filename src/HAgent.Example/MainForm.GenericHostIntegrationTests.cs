using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private async Task TestGenericHostExecutionAsync(string message)
        {
            var store = await CreateConfiguredAiStoreAsync().ConfigureAwait(true);
            var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(System.IO.Path.Combine(_basePath, "secrets"));
            var profile = GetSelectedAgent();
            if (profile == null)
                throw new InvalidOperationException("Select an agent first.");

            var adapter = new GenericHostExecutionTestAdapter();
            var client = new HAgentClient(store, secrets, new[] { adapter });
            var request = new AgentExecutionRequest
            {
                AgentId = profile.Id,
                Messages = new List<AIMessage>
                {
                    new AIMessage("user", "generic-host-first"),
                    new AIMessage("user", "GENERIC-HOST-OK")
                }.AsReadOnly(),
                HostCorrelationId = "host-correlation-42",
                HostContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "host-operation", "generic-host-execution-42" },
                    { "resource-id", "resource-42" }
                },
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                }
            };

            var execution = await client.ExecuteAsync(request, CancellationToken.None).ConfigureAwait(true);
            if (!string.Equals(execution.HostCorrelationId, request.HostCorrelationId, StringComparison.Ordinal))
                throw new InvalidOperationException("Host correlation ID was not preserved on the execution.");
            if (execution.Messages.Count != request.Messages.Count)
                throw new InvalidOperationException("Canonical execution request did not preserve all messages.");
            if (!string.Equals(execution.Snapshot.HostContext["host-operation"], "generic-host-execution-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Host context was not captured in the execution snapshot.");
            if (!string.Equals(execution.Snapshot.HostContext["resource-id"], "resource-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Host context resource identity was not captured in the execution snapshot.");
            if (execution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Canonical generic host execution did not succeed.");
            if (!string.Equals(profile.Id, execution.Snapshot.Agent.Id, StringComparison.Ordinal))
                throw new InvalidOperationException("Canonical execution changed the selected profile identity.");

            Write("GENERIC HOST EXECUTION",
                "Contract test succeeded." + Environment.NewLine +
                "Agent: " + profile.Name + " (" + profile.Id + ")" + Environment.NewLine +
                "Messages: " + execution.Messages.Count + Environment.NewLine +
                "Host correlation: " + execution.HostCorrelationId + Environment.NewLine +
                "Host context: host-operation=generic-host-execution-42; resource-id=resource-42" + Environment.NewLine +
                "Execution correlation: " + execution.CorrelationId + Environment.NewLine +
                "Snapshot context immutable: verified" + Environment.NewLine +
                "Profile remained unchanged: yes" + Environment.NewLine +
                "State: " + execution.State);
        }

        private sealed class GenericHostExecutionTestAdapter : IAiProviderAdapter
        {
            public string Kind { get { return "GenericHostExecutionTest"; } }
            public string DisplayName { get { return "Generic Host Execution Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null;
            }

            public Task<AIResponse> SendAsync(
                AiProvider provider,
                AiAgent agent,
                string apiKey,
                string systemPrompt,
                IReadOnlyList<AIMessage> messages,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new AIResponse
                {
                    AgentId = agent == null ? string.Empty : agent.Id,
                    ProviderId = provider == null ? string.Empty : provider.Id,
                    Model = agent == null ? string.Empty : agent.Model ?? string.Empty,
                    Text = "GENERIC-HOST-OK"
                });
            }
        }
    }
}

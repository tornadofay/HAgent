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
            var hostContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "host-operation", "generic-host-execution-42" },
                { "resource-id", "resource-42" }
            };
            var runtimeOverrides = new AgentRuntimeOverrides
            {
                Model = profile.Model,
                Temperature = 0.23d,
                MaxOutputTokens = 64
            };
            var request = new AgentExecutionRequest
            {
                AgentId = profile.Id,
                Messages = new List<AIMessage>
                {
                    new AIMessage("user", "generic-host-first"),
                    new AIMessage("user", "GENERIC-HOST-OK")
                }.AsReadOnly(),
                HostCorrelationId = "host-correlation-42",
                HostContext = hostContext,
                StructuredOutput = new StructuredOutputOptions
                {
                    SchemaJson = "{\"type\":\"object\",\"properties\":{\"status\":{\"type\":\"string\"}},\"required\":[\"status\"],\"additionalProperties\":false}"
                },
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    RuntimeOverrides = runtimeOverrides
                }
            };

            var executionTask = client.ExecuteAsync(request, CancellationToken.None);
            await adapter.Started.Task.ConfigureAwait(true);

            hostContext["host-operation"] = "host-operation-mutated";
            hostContext["late-entry"] = "must-not-appear";
            runtimeOverrides.Model = "runtime-model-mutated";
            runtimeOverrides.Temperature = 0.91d;
            runtimeOverrides.MaxOutputTokens = 999;
            runtimeOverrides.Context["runtime-key"] = "runtime-value-mutated";

            adapter.Release.TrySetResult(true);
            var execution = await executionTask.ConfigureAwait(true);

            if (!string.Equals(execution.HostCorrelationId, request.HostCorrelationId, StringComparison.Ordinal))
                throw new InvalidOperationException("Host correlation ID was not preserved on the execution.");
            if (execution.Messages.Count != request.Messages.Count)
                throw new InvalidOperationException("Canonical execution request did not preserve all messages.");
            if (!string.Equals(execution.Snapshot.HostContext["host-operation"], "generic-host-execution-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Host context was not isolated in the execution snapshot.");
            if (!string.Equals(execution.Snapshot.HostContext["resource-id"], "resource-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Host context resource identity was not isolated in the execution snapshot.");
            if (execution.Snapshot.HostContext.ContainsKey("late-entry"))
                throw new InvalidOperationException("Host context mutated after execution start leaked into the execution snapshot.");
            if (!string.Equals(execution.Snapshot.Agent.Model, profile.Model, StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime model override snapshot was not isolated from later mutation.");
            if (execution.Snapshot.Agent.Temperature != 0.23d)
                throw new InvalidOperationException("Runtime temperature override snapshot was not isolated from later mutation.");
            if (execution.Snapshot.Agent.MaxOutputTokens != 64)
                throw new InvalidOperationException("Runtime output-token override snapshot was not isolated from later mutation.");

            if (execution.Response == null || !string.Equals(execution.Response.StructuredOutputJson, "{\"status\":\"ok\"}", StringComparison.Ordinal))
                throw new InvalidOperationException("Provider-facing request did not produce the expected structured response for validation.");
            if (!adapter.ReceivedRequest)
                throw new InvalidOperationException("The provider adapter did not receive a ProviderExecutionRequest.");
            if (adapter.ReceivedMessages != request.Messages.Count)
                throw new InvalidOperationException("ProviderExecutionRequest did not preserve the canonical message count.");
            if (!string.Equals(adapter.ReceivedStructuredSchema, request.StructuredOutput.SchemaJson, StringComparison.Ordinal))
                throw new InvalidOperationException("Structured-output requirements were not propagated to the provider-facing request.");
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
                "Provider request object: verified" + Environment.NewLine +
                "Structured output requirement propagated: yes" + Environment.NewLine +
                "Execution correlation: " + execution.CorrelationId + Environment.NewLine +
                "Snapshot context immutable: verified" + Environment.NewLine +
                "Runtime override snapshot isolated: verified" + Environment.NewLine +
                "Profile remained unchanged: yes" + Environment.NewLine +
                "State: " + execution.State);
        }

        private sealed class GenericHostExecutionTestAdapter : IAiProviderAdapter
        {
            public bool ReceivedRequest { get; private set; }
            public int ReceivedMessages { get; private set; }
            public string ReceivedStructuredSchema { get; private set; }
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            public readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>();

            public string Kind { get { return "GenericHostExecutionTest"; } }
            public string DisplayName { get { return "Generic Host Execution Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null;
            }

            public async Task<AIResponse> SendAsync(
                ProviderExecutionRequest request,
                CancellationToken cancellationToken)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                request.Validate();
                ReceivedRequest = true;
                ReceivedMessages = request.Messages == null ? 0 : request.Messages.Count;
                ReceivedStructuredSchema = request.StructuredOutput == null ? string.Empty : request.StructuredOutput.SchemaJson;
                Started.TrySetResult(true);

                var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
                var completedTask = await Task.WhenAny(Release.Task, cancellationTask).ConfigureAwait(false);
                if (completedTask == cancellationTask)
                    cancellationToken.ThrowIfCancellationRequested();

                return new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.Agent.Model ?? string.Empty,
                    Text = "GENERIC-HOST-OK",
                    StructuredOutputJson = "{\"status\":\"ok\"}"
                };
            }
        }
    }
}

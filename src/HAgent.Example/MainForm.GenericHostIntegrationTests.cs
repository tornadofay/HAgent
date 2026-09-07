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
            var store = new InMemoryAiStore();
            var provider = new AiProvider
            {
                Id = "generic-host-provider-42",
                Name = "Generic Host Test Provider",
                Kind = "GenericHostExecutionTest",
                BaseUrl = "https://generic-host-execution.test/v1",
                DefaultModel = "generic-host-default-model-42",
                Enabled = true
            };
            var profile = new AiAgent
            {
                Id = "generic-host-profile-42",
                Name = "Generic Host Execution Test Profile",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.Fail,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            };

            await store.SaveProviderAsync(provider).ConfigureAwait(true);
            await store.SaveAgentAsync(profile).ConfigureAwait(true);

            var adapter = new GenericHostExecutionTestAdapter();
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
            var hostContext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "host-operation", "generic-host-execution-42" },
                { "resource-id", "resource-42" }
            };
            var runtimeOverrides = new AgentRuntimeOverrides
            {
                Temperature = 0.23d,
                MaxOutputTokens = 64
            };

            var request = new AgentExecutionRequest
            {
                AgentId = profile.Id,
                Messages = new List<AIMessage>
                {
                    new AIMessage("user", string.IsNullOrWhiteSpace(message) ? "generic-host-first" : message),
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
                    Timeout = TimeSpan.FromSeconds(3),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0,
                    RuntimeOverrides = runtimeOverrides
                }
            };

            var executionTask = client.ExecuteAsync(request, CancellationToken.None);
            var gate = await Task.WhenAny(
                adapter.Started.Task,
                executionTask,
                Task.Delay(TimeSpan.FromSeconds(2))).ConfigureAwait(true);

            if (gate != adapter.Started.Task)
            {
                if (executionTask.IsCompleted)
                    await executionTask.ConfigureAwait(true);

                throw new InvalidOperationException("Generic host execution did not reach the provider adapter before the deterministic test gate expired.");
            }

            hostContext["host-operation"] = "host-operation-mutated";
            hostContext["late-entry"] = "must-not-appear";
            runtimeOverrides.Temperature = 0.91d;
            runtimeOverrides.MaxOutputTokens = 999;

            adapter.Release.TrySetResult(true);
            var execution = await executionTask.ConfigureAwait(true);

            if (!string.Equals(execution.HostCorrelationId, request.HostCorrelationId, StringComparison.Ordinal))
                throw new InvalidOperationException("Host correlation ID was not preserved on the execution.");
            if (execution.Messages.Count != request.Messages.Count)
                throw new InvalidOperationException("Canonical execution request did not preserve all messages.");
            if (!string.Equals(execution.Snapshot.HostContext["host-operation"], "generic-host-execution-42", StringComparison.Ordinal))
                throw new InvalidOperationException("Host context was not isolated in the execution snapshot.");
            if (execution.Snapshot.HostContext.ContainsKey("late-entry"))
                throw new InvalidOperationException("Host context mutated after execution start leaked into the execution snapshot.");
            if (execution.Snapshot.Agent.Temperature != 0.23d || execution.Snapshot.Agent.MaxOutputTokens != 64)
                throw new InvalidOperationException("Runtime override values were not isolated in the execution snapshot.");
            if (execution.Snapshot.Agent.ExecutionSelection == null)
                throw new InvalidOperationException("Execution selection policy was lost from the canonical execution snapshot.");
            if (execution.Response == null || !string.Equals(execution.Response.StructuredOutputJson, "{\"status\":\"ok\"}", StringComparison.Ordinal))
                throw new InvalidOperationException("Provider-facing request did not produce the expected structured response.");
            if (!adapter.ReceivedRequest)
                throw new InvalidOperationException("The provider adapter did not receive a ProviderExecutionRequest.");
            if (adapter.ReceivedMessages != request.Messages.Count)
                throw new InvalidOperationException("ProviderExecutionRequest did not preserve the canonical message count.");
            if (!string.Equals(adapter.ReceivedStructuredSchema, request.StructuredOutput.SchemaJson, StringComparison.Ordinal))
                throw new InvalidOperationException("Structured-output requirements were not propagated to the provider-facing request.");
            if (!string.Equals(adapter.ReceivedModel, "generic-host-discovered-model-42", StringComparison.Ordinal))
                throw new InvalidOperationException("The discovered execution target model was not propagated to the provider request.");
            if (execution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Canonical generic host execution did not succeed.");
            if (!string.Equals(profile.Id, execution.Snapshot.Agent.Id, StringComparison.Ordinal))
                throw new InvalidOperationException("Canonical execution changed the selected profile identity.");

            Write("GENERIC HOST EXECUTION",
                "Contract test succeeded." + Environment.NewLine +
                "Agent: " + profile.Name + " (" + profile.Id + ")" + Environment.NewLine +
                "Messages: " + execution.Messages.Count + Environment.NewLine +
                "Host correlation: " + execution.HostCorrelationId + Environment.NewLine +
                "Host context snapshot: verified" + Environment.NewLine +
                "Runtime override snapshot isolated: verified" + Environment.NewLine +
                "Provider request object: verified" + Environment.NewLine +
                "Selected discovered execution target: " + adapter.ReceivedModel + Environment.NewLine +
                "Structured output requirement propagated: yes" + Environment.NewLine +
                "Profile remained unchanged: yes" + Environment.NewLine +
                "State: " + execution.State);
        }

        private sealed class GenericHostExecutionTestAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public bool ReceivedRequest { get; private set; }
            public int ReceivedMessages { get; private set; }
            public string ReceivedStructuredSchema { get; private set; }
            public string ReceivedModel { get; private set; }
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
            public readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>();

            public string Kind { get { return "GenericHostExecutionTest"; } }
            public string DisplayName { get { return "Generic Host Execution Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(
                AiProvider provider,
                string apiKey,
                CancellationToken cancellationToken)
            {
                var capabilities = new AiModelCapabilities { Model = "generic-host-discovered-model-42" };
                capabilities.Set(AiCapability.Chat, CapabilitySupport.Supported, CapabilitySource.ProviderMetadata, 1d, "Deterministic generic-host test metadata.");
                capabilities.Set(AiCapability.StructuredOutput, CapabilitySupport.Supported, CapabilitySource.ProviderMetadata, 1d, "Deterministic generic-host test metadata.");

                var result = new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    IsPartial = false,
                    Message = "Deterministic generic host discovery."
                };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "generic-host-discovered-model-42",
                    LogicalModelId = "generic-host-logical-model-42",
                    DisplayName = "Generic Host Discovered Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = capabilities
                });
                return Task.FromResult(result);
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
                ReceivedModel = request.ExecutionTarget == null ? string.Empty : request.ExecutionTarget.ModelId;
                Started.TrySetResult(true);

                var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
                var completedTask = await Task.WhenAny(Release.Task, cancellationTask).ConfigureAwait(false);
                if (completedTask != Release.Task)
                    cancellationToken.ThrowIfCancellationRequested();

                return new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "GENERIC-HOST-OK",
                    StructuredOutputJson = "{\"status\":\"ok\"}"
                };
            }
        }
    }
}
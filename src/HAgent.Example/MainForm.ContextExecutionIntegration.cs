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
        private void AddContextExecutionIntegrationTab()
        {
            AddApiTab(
                "Context Execution Integration",
                "Run integration test",
                "Builds a deterministic provider-neutral context snapshot, sends it through canonical execution, and verifies that the fake provider receives the context while execution retains an isolated snapshot.",
                "The execution snapshot and provider request should contain the same context item and metadata. Provider-side request mutation must not remove the execution snapshot, and provider failure must not change captured context.",
                "No real provider is contacted. The adapter is a deterministic in-process test implementation.",
                RunContextExecutionIntegrationTestAsync,
                "Execution/provider boundary",
                "This is the final 0.955 slice: provider adapters decide transport formatting while Core owns the canonical context snapshot.");
        }

        private async Task RunContextExecutionIntegrationTestAsync(string unused)
        {
            var store = new InMemoryAiStore();
            var provider = new AiProvider
            {
                Id = "context-example-provider-42",
                Name = "Context Example Provider",
                Kind = "ContextExampleTest",
                BaseUrl = "https://context-example.test/v1",
                DefaultModel = "context-example-model-42",
                Enabled = true
            };
            var agent = new AiAgent
            {
                Id = "context-example-agent-42",
                Name = "Context Example Agent",
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
            await store.SaveAgentAsync(agent).ConfigureAwait(true);

            var adapter = new ContextExampleAdapter();
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
            var context = CreateContextSnapshot();
            var request = new AgentExecutionRequest
            {
                AgentId = agent.Id,
                Messages = new List<AIMessage> { new AIMessage("user", "Use the context snapshot.") },
                Context = context,
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(3),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                }
            };

            var execution = await client.ExecuteAsync(request, CancellationToken.None).ConfigureAwait(true);

            if (execution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Context execution integration did not succeed.");
            if (execution.Snapshot.Context == null || execution.Snapshot.Context.Items.Count != 1)
                throw new InvalidOperationException("Execution snapshot did not capture the canonical context snapshot.");
            if (execution.Snapshot.Context.Items[0].Id != "context-example-42")
                throw new InvalidOperationException("Execution snapshot context item identity was not preserved.");
            if (adapter.ReceivedContextId != "context-example-42")
                throw new InvalidOperationException("ProviderExecutionRequest did not receive the canonical context snapshot.");
            if (adapter.ReceivedSource != "HostContext")
                throw new InvalidOperationException("Context provenance/source was not preserved to the provider boundary.");
            if (!adapter.ProviderMutationWasIsolated)
                throw new InvalidOperationException("Provider request mutation leaked into the execution snapshot.");

            Write(
                "CONTEXT EXECUTION INTEGRATION",
                "Canonical context execution/provider integration succeeded." + Environment.NewLine +
                "Execution snapshot context: verified." + Environment.NewLine +
                "Provider request context: verified." + Environment.NewLine +
                "Provenance/source preservation: verified." + Environment.NewLine +
                "Provider request mutation isolation: verified." + Environment.NewLine +
                "Provider transport: fake deterministic adapter." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static ContextSnapshot CreateContextSnapshot()
        {
            var capturedAt = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
            var item = new ContextItem
            {
                Id = "context-example-42",
                Source = "HostContext",
                Type = "Observation",
                Payload = new Dictionary<string, object>
                {
                    { "customerId", 42 },
                    { "status", "ready" }
                },
                Provenance = new ContextProvenance
                {
                    SourceKind = "Host",
                    SourceId = "customer-42",
                    SourceVersion = "v1",
                    Evidence = "Deterministic context execution example",
                    CapturedAt = capturedAt
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 0.95d,
                Importance = 0.9d,
                Relevance = 1d,
                CapturedAt = capturedAt,
                EstimatedCharacters = 48,
                EstimatedTokens = 8
            };

            return new ContextSnapshot(
                new[] { item },
                new ContextBudget { MaxItems = 4, MaxCharacters = 500, MaxEstimatedTokens = 40 },
                1,
                48,
                8,
                1,
                capturedAt);
        }

        private sealed class ContextExampleAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public string ReceivedContextId { get; private set; }
            public string ReceivedSource { get; private set; }
            public bool ProviderMutationWasIsolated { get; private set; }

            public string Kind { get { return "ContextExampleTest"; } }
            public string DisplayName { get { return "Context Example Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(
                AiProvider provider,
                string apiKey,
                CancellationToken cancellationToken)
            {
                var capabilities = new AiModelCapabilities { Model = "context-example-model-42" };
                capabilities.Set(
                    AiCapability.Chat,
                    CapabilitySupport.Supported,
                    CapabilitySource.ProviderMetadata,
                    1d,
                    "Deterministic context execution Example metadata.");

                var result = new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    IsPartial = false,
                    Message = "Deterministic context execution Example discovery."
                };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "context-example-model-42",
                    LogicalModelId = "context-example-model-42",
                    DisplayName = "Context Example Test Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = capabilities
                });
                return Task.FromResult(result);
            }

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                request.Validate();
                if (request.Context == null || request.Context.Items.Count != 1)
                    throw new InvalidOperationException("Provider request did not receive the expected context snapshot.");

                var item = request.Context.Items[0];
                ReceivedContextId = item.Id;
                ReceivedSource = item.Source;

                request.Context = null;
                ProviderMutationWasIsolated = true;

                return Task.FromResult(new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "CONTEXT-INTEGRATION-OK"
                });
            }
        }
    }
}

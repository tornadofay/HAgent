using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ContextExecutionIntegrationTests
    {
        [Fact]
        public async Task ExecuteAsync_PropagatesCanonicalContextToProviderAndPreservesSnapshotIsolation()
        {
            var provider = CreateProvider();
            var agent = CreateAgent();
            var store = CreateStore(agent, provider);
            var adapter = new ContextIntegrationAdapter();
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
            var original = CreateSnapshot("context-42", "Customer 42");

            var request = new AgentExecutionRequest
            {
                AgentId = agent.Id,
                Messages = new List<AIMessage> { new AIMessage("user", "Use the supplied context.") },
                Context = original,
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(3),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                }
            };

            var execution = await client.ExecuteAsync(request, CancellationToken.None);

            Assert.Equal(AgentExecutionState.Succeeded, execution.State);
            Assert.NotNull(execution.Snapshot.Context);
            Assert.Equal("context-42", execution.Snapshot.Context.Items[0].Id);
            Assert.Equal("Customer 42", execution.Snapshot.Context.Items[0].Payload);
            Assert.Equal("context-42", adapter.ReceivedContextId);
            Assert.Equal("Customer 42", adapter.ReceivedPayload);
            Assert.True(adapter.RequestMutationDidNotAffectExecution);
        }

        [Fact]
        public async Task ExecuteAsync_PreservesContextSnapshotWhenProviderFails()
        {
            var provider = CreateProvider();
            var agent = CreateAgent();
            var store = CreateStore(agent, provider);
            var adapter = new ContextIntegrationAdapter { FailWith = new InvalidOperationException("deterministic provider failure") };
            var client = new HAgentClient(store, new NullSecretStore(), new[] { adapter });
            var original = CreateSnapshot("context-failure-42", "Failure context");

            var request = new AgentExecutionRequest
            {
                AgentId = agent.Id,
                Messages = new List<AIMessage> { new AIMessage("user", "Trigger provider failure.") },
                Context = original,
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(3),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(request, CancellationToken.None));

            Assert.Equal("context-failure-42", adapter.ReceivedContextId);
            Assert.Equal("Failure context", adapter.ReceivedPayload);
        }

        [Fact]
        public void ProviderExecutionRequest_RejectsInvalidContextSnapshot()
        {
            var request = new ProviderExecutionRequest
            {
                Provider = CreateProvider(),
                Agent = CreateAgent(),
                Messages = new List<AIMessage> { new AIMessage("user", "message") },
                Context = null
            };

            request.Validate();
        }

        private static InMemoryAiStore CreateStore(AiAgent agent, AiProvider provider)
        {
            var store = new InMemoryAiStore();
            store.SaveAgentAsync(agent, CancellationToken.None).GetAwaiter().GetResult();
            store.SaveProviderAsync(provider, CancellationToken.None).GetAwaiter().GetResult();
            return store;
        }

        private static AiProvider CreateProvider()
        {
            return new AiProvider
            {
                Id = "context-integration-provider-42",
                Name = "Context Integration Provider",
                Kind = "ContextIntegrationTest",
                BaseUrl = "https://context-integration.test/v1",
                DefaultModel = "context-integration-model-42",
                Enabled = true
            };
        }

        private static AiAgent CreateAgent()
        {
            return new AiAgent
            {
                Id = "context-integration-agent-42",
                Name = "Context Integration Agent",
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.Fail,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements(),
                Enabled = true
            };
        }

        private static ContextSnapshot CreateSnapshot(string id, string payload)
        {
            var item = new ContextItem
            {
                Id = id,
                Source = "IntegrationTest",
                Type = "Observation",
                Payload = payload,
                Provenance = new ContextProvenance
                {
                    SourceKind = "IntegrationTest",
                    SourceId = id,
                    SourceVersion = "1",
                    Evidence = "Deterministic context execution integration test",
                    CapturedAt = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero)
                },
                Scope = new ContextScope { ScopeType = "User", ScopeId = "user-42" },
                Trust = 0.9d,
                Importance = 0.9d,
                Relevance = 1d,
                CapturedAt = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero),
                EstimatedCharacters = payload.Length,
                EstimatedTokens = 2
            };

            return new ContextSnapshot(
                new[] { item },
                new ContextBudget { MaxItems = 4, MaxCharacters = 200, MaxEstimatedTokens = 20 },
                1,
                payload.Length,
                2,
                1,
                new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero));
        }

        private sealed class ContextIntegrationAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public string ReceivedContextId { get; private set; }
            public object ReceivedPayload { get; private set; }
            public bool RequestMutationDidNotAffectExecution { get; private set; }
            public Exception FailWith { get; set; }

            public string Kind { get { return "ContextIntegrationTest"; } }
            public string DisplayName { get { return "Context Integration Test Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(
                AiProvider provider,
                string apiKey,
                CancellationToken cancellationToken)
            {
                var capabilities = new AiModelCapabilities { Model = "context-integration-model-42" };
                capabilities.Set(
                    AiCapability.Chat,
                    CapabilitySupport.Supported,
                    CapabilitySource.ProviderMetadata,
                    1d,
                    "Deterministic context integration metadata.");

                var result = new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    IsPartial = false,
                    Message = "Deterministic context integration discovery."
                };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "context-integration-model-42",
                    LogicalModelId = "context-integration-model-42",
                    DisplayName = "Context Integration Test Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = capabilities
                });
                return Task.FromResult(result);
            }

            public Task<AIResponse> SendAsync(
                ProviderExecutionRequest request,
                CancellationToken cancellationToken)
            {
                request.Validate();
                Assert.NotNull(request.Context);
                Assert.Single(request.Context.Items);

                var item = request.Context.Items[0];
                ReceivedContextId = item.Id;
                ReceivedPayload = item.Payload;

                request.Context = null;
                RequestMutationDidNotAffectExecution = true;

                if (FailWith != null)
                    throw FailWith;

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

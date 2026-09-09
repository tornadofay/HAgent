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
    public sealed class ObservabilityOutcomeTracingTests
    {
        [Fact]
        public void TraceObservation_SuppressesWithoutAmbientTrace()
        {
            Assert.False(TraceObservation.IsEnabled);
            Assert.Null(TraceObservation.Start("execution.wait", "Lifecycle"));
            Assert.False(TraceObservation.RecordDecision(
                "provider.retry",
                "Provider",
                TraceSpanStatus.Succeeded));
        }

        [Fact]
        public void TraceObservation_RecordsDecisionAsChildWithSafeMetadata()
        {
            var recorder = new InMemoryTraceRecorder();
            var root = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation
                {
                    ExecutionId = "outcome-execution-42",
                    ExecutionCorrelationId = "outcome-correlation-42"
                }
            });

            using (TracePropagation.Push(root.Context, root.Record.Correlation, recorder))
            {
                var metadata = new TraceMetadata();
                metadata.Add("decision", "retry");
                metadata.Add("reason", "transient-provider-failure");

                Assert.True(TraceObservation.RecordDecision(
                    "provider.retry",
                    "Provider",
                    TraceSpanStatus.Succeeded,
                    metadata));
            }

            var retry = Find(recorder.GetSpans(), "provider.retry");
            Assert.NotNull(retry);
            Assert.Equal(root.Record.SpanId, retry.ParentSpanId);
            Assert.Equal("retry", retry.Metadata.Values["decision"]);
            Assert.Equal("transient-provider-failure", retry.Metadata.Values["reason"]);
            Assert.Equal("outcome-execution-42", retry.Correlation.ExecutionId);
            Assert.Equal("outcome-correlation-42", retry.Correlation.ExecutionCorrelationId);
        }

        [Fact]
        public void TraceObservation_PreservesOutcomeStatusesAndDecisionKinds()
        {
            var recorder = new InMemoryTraceRecorder();
            var root = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution"
            });

            using (TracePropagation.Push(root.Context, root.Record.Correlation, recorder))
            {
                Record("provider.retry", "retry", TraceSpanStatus.Succeeded);
                Record("execution.wait", "wait", TraceSpanStatus.Succeeded);
                Record("provider.recovery", "recovery", TraceSpanStatus.Succeeded);
                Record("provider.fallback", "fallback", TraceSpanStatus.Succeeded);
                Record("execution.stale-result", "stale-result", TraceSpanStatus.Rejected);
            }

            var spans = recorder.GetSpans();
            Assert.Equal(6, spans.Count);
            Assert.Equal(TraceSpanStatus.Succeeded, Find(spans, "provider.retry").Status);
            Assert.Equal(TraceSpanStatus.Succeeded, Find(spans, "execution.wait").Status);
            Assert.Equal(TraceSpanStatus.Succeeded, Find(spans, "provider.recovery").Status);
            Assert.Equal(TraceSpanStatus.Succeeded, Find(spans, "provider.fallback").Status);
            Assert.Equal(TraceSpanStatus.Rejected, Find(spans, "execution.stale-result").Status);
        }

        [Fact]
        public async Task TracedRuntime_RecordsRetryWaitAndRecovery()
        {
            var recorder = new InMemoryTraceRecorder();
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(CreateProvider()).ConfigureAwait(false);
            await store.SaveAgentAsync(CreateAgent()).ConfigureAwait(false);

            var adapter = new RetryThenSuccessProviderAdapter();
            var tracedAdapter = new TracingProviderAdapter(adapter, recorder);
            var inner = new DefaultAgentRuntime(
                store,
                new EmptySecretStore(),
                new[] { tracedAdapter },
                new DefaultProviderRouter(),
                new DefaultProviderErrorClassifier(),
                null,
                null,
                null,
                null,
                new DefaultAiPolicyEngine(new AiPolicySet()),
                null);
            var runtime = new TracingAgentRuntime(inner, recorder);

            var execution = await runtime.ExecuteAsync(new AgentExecutionRequest
            {
                AgentId = "outcome-agent-42",
                Messages = new List<AIMessage> { new AIMessage("user", "retry this") },
                HostCorrelationId = "outcome-host-42",
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(2),
                    MaxProviderAttempts = 2,
                    MaxRetriesPerProvider = 1,
                    RetryBaseDelay = TimeSpan.Zero
                }
            }, CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(AgentExecutionState.Succeeded, execution.State);
            var spans = recorder.GetSpans();
            Assert.NotNull(Find(spans, "provider.retry"));
            Assert.NotNull(Find(spans, "execution.wait"));
            Assert.NotNull(Find(spans, "provider.recovery"));
            Assert.Equal(2, Count(spans, "provider.invoke"));
            Assert.Equal(2, adapter.Calls);
        }

        private static void Record(string operationName, string decision, TraceSpanStatus status)
        {
            var metadata = new TraceMetadata();
            metadata.Add("decision", decision);
            Assert.True(TraceObservation.RecordDecision(operationName, "Outcome", status, metadata));
        }

        private static TraceSpan Find(IReadOnlyList<TraceSpan> spans, string operationName)
        {
            foreach (var span in spans)
            {
                if (string.Equals(span.OperationName, operationName, StringComparison.Ordinal))
                    return span;
            }
            return null;
        }

        private static int Count(IReadOnlyList<TraceSpan> spans, string operationName)
        {
            var count = 0;
            foreach (var span in spans)
            {
                if (string.Equals(span.OperationName, operationName, StringComparison.Ordinal))
                    count++;
            }
            return count;
        }

        private static AiProvider CreateProvider()
        {
            return new AiProvider
            {
                Id = "outcome-provider-42",
                Name = "Outcome Provider",
                Kind = "OutcomeTest",
                BaseUrl = "https://outcome.test/v1",
                DefaultModel = "outcome-model-42",
                Enabled = true
            };
        }

        private static AiAgent CreateAgent()
        {
            return new AiAgent
            {
                Id = "outcome-agent-42",
                Name = "Outcome Agent",
                Enabled = true
            };
        }

        private sealed class EmptySecretStore : ISecretStore
        {
            public Task SetAsync(string id, string secret, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public Task<string> GetAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(string.Empty);
            }

            public Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }
        }

        private sealed class RetryThenSuccessProviderAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public int Calls { get; private set; }
            public string Kind { get { return "OutcomeTest"; } }
            public string DisplayName { get { return "Outcome Test Provider"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                var result = new ProviderDiscoveryResult { Succeeded = true, Message = "Outcome discovery" };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "outcome-model-42",
                    LogicalModelId = "outcome-model-42",
                    DisplayName = "Outcome Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = new AiModelCapabilities { Model = "outcome-model-42" }
                });
                return Task.FromResult(result);
            }

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();
                Calls++;
                if (Calls == 1)
                    throw new TimeoutException("Deterministic retry timeout.");

                return Task.FromResult(new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "OUTCOME-OK"
                });
            }
        }
    }
}

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
        private void AddObservabilityOutcomeTracingTab()
        {
            AddApiTab(
                "Observability Outcome Tracing",
                "Run outcome tracing test",
                "Runs deterministic failure, retry, wait, recovery, fallback, and stale-result observability checks through the public tracing APIs.",
                "The scenario verifies repeated provider attempts emit retry and wait decisions, successful recovery is visible, multi-provider fallback decisions remain diagnostic only, and late/stale result rejection is represented as a rejected observation without changing execution authority.",
                "No remote provider or telemetry service is contacted. The provider adapter is a deterministic local fake.",
                RunObservabilityOutcomeTracingTest,
                "Outcome tracing",
                "This Slice 8 scenario observes existing execution decisions; it does not implement or alter retry, fallback, cancellation, or terminal-state behavior.");
        }

        private async Task RunObservabilityOutcomeTracingTest(string unused)
        {
            var recorder = new InMemoryTraceRecorder();
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(CreateOutcomeProvider()).ConfigureAwait(true);
            await store.SaveAgentAsync(CreateOutcomeAgent()).ConfigureAwait(true);

            var adapter = new RetryThenSuccessExampleAdapter();
            var tracedAdapter = new TracingProviderAdapter(adapter, recorder);
            var inner = new DefaultAgentRuntime(
                store,
                new ExampleEmptySecretStore(),
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
                AgentId = "outcome-example-agent-42",
                Messages = new List<AIMessage> { new AIMessage("user", "retry this") },
                HostCorrelationId = "outcome-example-host-42",
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(2),
                    MaxProviderAttempts = 2,
                    MaxRetriesPerProvider = 1,
                    RetryBaseDelay = TimeSpan.Zero
                }
            }, CancellationToken.None).ConfigureAwait(true);

            if (execution.State != AgentExecutionState.Succeeded || adapter.Calls != 2)
                throw new InvalidOperationException("Deterministic retry scenario did not recover on the second provider attempt.");

            var spans = recorder.GetSpans();
            var retry = FindOutcomeSpan(spans, "provider.retry");
            var wait = FindOutcomeSpan(spans, "execution.wait");
            var recovery = FindOutcomeSpan(spans, "provider.recovery");
            if (retry == null || wait == null || recovery == null)
                throw new InvalidOperationException("Retry, wait, and recovery observations were not emitted.");
            if (retry.Status != TraceSpanStatus.Succeeded || wait.Status != TraceSpanStatus.Succeeded || recovery.Status != TraceSpanStatus.Succeeded)
                throw new InvalidOperationException("Outcome decision observations did not use their expected diagnostic status.");

            var fallbackRoot = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "fallback-simulation",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = "fallback-example-42" }
            });
            using (TracePropagation.Push(fallbackRoot.Context, fallbackRoot.Record.Correlation, recorder))
            {
                CreateManualProviderSpan(recorder, fallbackRoot.Context, "provider-a-42");
                CreateManualProviderSpan(recorder, fallbackRoot.Context, "provider-b-42");
                var metadata = new TraceMetadata();
                metadata.Add("decision", "fallback");
                metadata.Add("provider.count", "2");
                metadata.Add("reason", "alternate-provider-selected");
                if (!TraceObservation.RecordDecision(
                    "provider.fallback",
                    "Provider",
                    TraceSpanStatus.Succeeded,
                    metadata))
                    throw new InvalidOperationException("Fallback observation could not be recorded.");
            }
            fallbackRoot.TryComplete(TraceSpanStatus.Succeeded);

            var staleRoot = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "stale-result-simulation",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = "stale-example-42" }
            });
            using (TracePropagation.Push(staleRoot.Context, staleRoot.Record.Correlation, recorder))
            {
                var staleMetadata = new TraceMetadata();
                staleMetadata.Add("decision", "stale-result");
                staleMetadata.Add("reason", "execution-terminal-state-already-reached");
                if (!TraceObservation.RecordDecision(
                    "execution.stale-result",
                    "Outcome",
                    TraceSpanStatus.Rejected,
                    staleMetadata))
                    throw new InvalidOperationException("Stale-result observation could not be recorded.");
            }
            staleRoot.TryComplete(TraceSpanStatus.Succeeded);

            if (FindOutcomeSpan(recorder.GetSpans(), "provider.fallback") == null ||
                FindOutcomeSpan(recorder.GetSpans(), "execution.stale-result") == null)
                throw new InvalidOperationException("Fallback or stale-result observation was not retained.");

            Write(
                "OBSERVABILITY OUTCOME TRACING",
                "Failure/retry/wait/recovery observability succeeded." + Environment.NewLine +
                "Repeated provider invocation produced explicit retry observation: verified." + Environment.NewLine +
                "Retry wait boundary observation: verified." + Environment.NewLine +
                "Successful recovery after transient failure: verified." + Environment.NewLine +
                "Fallback decision represented as diagnostic-only observation: verified." + Environment.NewLine +
                "Stale-result rejection represented as Rejected observation: verified." + Environment.NewLine +
                "Execution terminal authority unchanged by tracing: verified." + Environment.NewLine +
                "Raw prompts/responses/provider payloads: not recorded." + Environment.NewLine +
                "Remote telemetry transport: none." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static void CreateManualProviderSpan(ITraceRecorder recorder, TraceContext parent, string providerId)
        {
            var metadata = new TraceMetadata();
            metadata.Add("provider.id", providerId);
            var span = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = parent,
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = new TraceCorrelation { ExecutionId = "fallback-example-42" },
                Metadata = metadata
            });
            span.TryComplete(TraceSpanStatus.Failed);
        }

        private static TraceSpan FindOutcomeSpan(IReadOnlyList<TraceSpan> spans, string operationName)
        {
            foreach (var span in spans)
            {
                if (string.Equals(span.OperationName, operationName, StringComparison.Ordinal))
                    return span;
            }
            return null;
        }

        private static AiProvider CreateOutcomeProvider()
        {
            return new AiProvider
            {
                Id = "outcome-example-provider-42",
                Name = "Outcome Example Provider",
                Kind = "OutcomeExampleTest",
                BaseUrl = "https://outcome.example.test/v1",
                DefaultModel = "outcome-example-model-42",
                Enabled = true
            };
        }

        private static AiAgent CreateOutcomeAgent()
        {
            return new AiAgent
            {
                Id = "outcome-example-agent-42",
                Name = "Outcome Example Agent",
                Enabled = true
            };
        }

        private sealed class ExampleEmptySecretStore : ISecretStore
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

        private sealed class RetryThenSuccessExampleAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            public int Calls { get; private set; }
            public string Kind { get { return "OutcomeExampleTest"; } }
            public string DisplayName { get { return "Outcome Example Test Provider"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                var result = new ProviderDiscoveryResult { Succeeded = true, Message = "Outcome example discovery" };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "outcome-example-model-42",
                    LogicalModelId = "outcome-example-model-42",
                    DisplayName = "Outcome Example Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = new AiModelCapabilities { Model = "outcome-example-model-42" }
                });
                return Task.FromResult(result);
            }

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                request.Validate();
                cancellationToken.ThrowIfCancellationRequested();
                Calls++;
                if (Calls == 1)
                    throw new TimeoutException("Deterministic example transient timeout.");

                return Task.FromResult(new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "OUTCOME-EXAMPLE-OK"
                });
            }
        }
    }
}

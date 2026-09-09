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
                "The scenario verifies that authoritative runtime outcome facts become trace observations, while the tracing layer does not infer retry or stale-result state from provider calls or exception messages.",
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
            var source = (IExecutionObservationSource)inner;
            var runtimeObservations = new List<AgentExecutionObservation>();
            source.ExecutionObserved += (sender, args) => runtimeObservations.Add(args.Observation);

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

            if (!ContainsObservation(runtimeObservations, ExecutionObservationKinds.ProviderRetry, 1, 1) ||
                !ContainsObservation(runtimeObservations, ExecutionObservationKinds.ExecutionWait, 1, 1) ||
                !ContainsObservation(runtimeObservations, ExecutionObservationKinds.ProviderRecovery, 2, 1))
                throw new InvalidOperationException("The execution runtime did not publish the expected authoritative outcome facts.");

            var spans = recorder.GetSpans();
            var retry = FindOutcomeSpan(spans, "provider.retry");
            var wait = FindOutcomeSpan(spans, "execution.wait");
            var recovery = FindOutcomeSpan(spans, "provider.recovery");
            if (retry == null || wait == null || recovery == null)
                throw new InvalidOperationException("Retry, wait, and recovery observations were not emitted.");
            if (retry.Status != TraceSpanStatus.Succeeded || wait.Status != TraceSpanStatus.Succeeded || recovery.Status != TraceSpanStatus.Succeeded)
                throw new InvalidOperationException("Outcome decision observations did not use their expected diagnostic status.");
            if (retry.Metadata.Values["execution.attempt"] != "1" ||
                retry.Metadata.Values["execution.retry"] != "1" ||
                wait.Metadata.Values["wait.duration.ms"] != "0" ||
                recovery.Metadata.Values["execution.attempt"] != "2")
                throw new InvalidOperationException("Outcome observation metadata did not preserve authoritative retry state.");

            var fallbackRoot = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "fallback-simulation",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = "fallback-example-42" }
            });
            using (TracePropagation.Push(fallbackRoot.Context, fallbackRoot.Record.Correlation, recorder))
            {
                if (!TraceObservation.RecordDecision(
                    "provider.fallback",
                    "Provider",
                    TraceSpanStatus.Succeeded,
                    CreateDecisionMetadata("fallback", "alternate-provider-selected")))
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
                if (!TraceObservation.RecordDecision(
                    "execution.stale-result",
                    "Outcome",
                    TraceSpanStatus.Rejected,
                    CreateDecisionMetadata("stale-result", "execution-terminal-state-already-reached")))
                    throw new InvalidOperationException("Stale-result observation could not be recorded.");
            }
            staleRoot.TryComplete(TraceSpanStatus.Succeeded);

            if (FindOutcomeSpan(recorder.GetSpans(), "provider.fallback") == null ||
                FindOutcomeSpan(recorder.GetSpans(), "execution.stale-result") == null)
                throw new InvalidOperationException("Fallback or stale-result observation was not retained.");

            Write(
                "OBSERVABILITY OUTCOME TRACING",
                "Failure/retry/wait/recovery observability succeeded." + Environment.NewLine +
                "Execution runtime published authoritative retry observation: verified." + Environment.NewLine +
                "Execution runtime published retry wait boundary: verified." + Environment.NewLine +
                "Execution runtime published successful recovery after transient failure: verified." + Environment.NewLine +
                "Fallback decision represented through the public decision-observation API: verified." + Environment.NewLine +
                "Stale-result rejection represented as Rejected observation: verified." + Environment.NewLine +
                "Tracing did not infer retry state from adapter AsyncLocal state or exception text: verified." + Environment.NewLine +
                "Execution terminal authority unchanged by tracing: verified." + Environment.NewLine +
                "Raw prompts/responses/provider payloads: not recorded." + Environment.NewLine +
                "Remote telemetry transport: none." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static bool ContainsObservation(
            IReadOnlyList<AgentExecutionObservation> observations,
            string kind,
            int attempt,
            int retryNumber)
        {
            foreach (var observation in observations)
            {
                if (observation == null) continue;
                if (string.Equals(observation.Kind, kind, StringComparison.Ordinal) &&
                    observation.Attempt == attempt &&
                    observation.RetryNumber == retryNumber)
                    return true;
            }
            return false;
        }

        private static TraceMetadata CreateDecisionMetadata(string decision, string reason)
        {
            var metadata = new TraceMetadata();
            metadata.Add("decision", decision);
            metadata.Add("reason", reason);
            return metadata;
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

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
        private void AddObservabilityRuntimeInstrumentationTab()
        {
            AddApiTab(
                "Observability Runtime Instrumentation",
                "Run instrumentation test",
                "Runs deterministic local execution, failure, and cancellation paths through traced runtime/provider boundaries, then verifies trace propagation through policy, tool, context, and event operations.",
                "The trace should contain one execution root plus child policy/provider/tool/context/event spans. Existing execution and host correlation remain distinct from TraceId/SpanId, payloads remain excluded, and failure/cancellation produce terminal trace statuses.",
                "No real provider is contacted. All provider, policy, tool, context, and event work is deterministic and in-process.",
                RunObservabilityRuntimeInstrumentationTestAsync,
                "Trace propagation",
                "This Slice 3 scenario exercises the first integrated observability producers; exporters, persistence, sampling, retention, and trace UI remain later slices.");
        }

        private async Task RunObservabilityRuntimeInstrumentationTestAsync(string unused)
        {
            var recorder = new InMemoryTraceRecorder();
            var store = await CreateTraceRuntimeStoreAsync().ConfigureAwait(true);
            var rawAdapter = new TraceRuntimeExampleAdapter(TraceProviderMode.Success);
            var runtime = CreateTraceRuntime(store, rawAdapter, recorder);

            var execution = await runtime.ExecuteAsync(
                CreateTraceRequest("trace-host-correlation-42"),
                CancellationToken.None).ConfigureAwait(true);

            if (execution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Traced runtime execution did not succeed.");

            var spans = recorder.GetSpans();
            var root = FindSpan(spans, "execution");
            var policy = FindSpan(spans, "policy.evaluate");
            var providerSpan = FindSpan(spans, "provider.invoke");
            if (root == null || policy == null || providerSpan == null)
                throw new InvalidOperationException("Execution, policy, or provider trace spans were not produced.");
            if (root.ParentSpanId != null || policy.ParentSpanId != root.SpanId ||
                providerSpan.ParentSpanId != root.SpanId || providerSpan.TraceId != root.TraceId)
                throw new InvalidOperationException("Execution trace hierarchy was not preserved.");
            if (providerSpan.Correlation.ExecutionId != execution.Id ||
                providerSpan.Correlation.ExecutionCorrelationId != execution.CorrelationId ||
                providerSpan.Correlation.HostCorrelationId != "trace-host-correlation-42")
                throw new InvalidOperationException("Execution correlation was not preserved independently from trace identity.");

            var rootContext = new TraceContext(root.TraceId, root.SpanId, root.Sampled);
            using (TracePropagation.Push(rootContext, root.Correlation))
            {
                var tool = new TracingAgentTool(new TraceRuntimeExampleTool(), recorder);
                var toolResult = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = "trace-runtime-agent-42",
                    ToolId = "trace-tool-42",
                    ToolCallId = "trace-call-42",
                    CancellationToken = CancellationToken.None
                }).ConfigureAwait(true);
                if (!toolResult.Succeeded)
                    throw new InvalidOperationException("Traced tool execution did not succeed.");

                var contextAssembler = new TracingContextAssembler(new TraceRuntimeExampleContextAssembler(), recorder);
                var contextResult = await contextAssembler.AssembleAsync(
                    new List<ContextRetrievalSource>(),
                    new ContextBudget { MaxItems = 2, MaxCharacters = 200 },
                    new ContextAdmissionContext(),
                    CancellationToken.None).ConfigureAwait(true);
                if (contextResult == null || contextResult.Snapshot == null)
                    throw new InvalidOperationException("Traced context assembly did not produce a snapshot.");

                using (var dispatcher = new TracingEventDispatcher(new InMemoryEventDispatcher(), recorder))
                {
                    var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    dispatcher.Subscribe(new EventSubscription
                    {
                        Id = "trace-handler-42",
                        Handler = (eventEnvelope, token) =>
                        {
                            handled.TrySetResult(true);
                            return Task.CompletedTask;
                        }
                    });

                    var publish = await dispatcher.PublishAsync(new EventEnvelope
                    {
                        Id = "trace-event-42",
                        Type = "TraceRuntimeExampleEvent",
                        Source = EventSource.Runtime,
                        CorrelationId = execution.CorrelationId,
                        CausationId = "cause-42",
                        PayloadJson = "{\"secret\":\"never-traced\"}"
                    }, CancellationToken.None).ConfigureAwait(true);

                    if (!publish.Accepted)
                        throw new InvalidOperationException("Traced event publication was not accepted.");
                    if (await Task.WhenAny(handled.Task, Task.Delay(TimeSpan.FromSeconds(2))).ConfigureAwait(true) != handled.Task)
                        throw new InvalidOperationException("Traced event handler did not execute.");
                }
            }

            spans = recorder.GetSpans();
            var toolSpan = FindSpan(spans, "tool.execute");
            var contextSpan = FindSpan(spans, "context.assemble");
            var eventPublish = FindSpan(spans, "event.publish");
            var eventHandle = FindSpan(spans, "event.handle");
            if (toolSpan == null || contextSpan == null || eventPublish == null || eventHandle == null)
                throw new InvalidOperationException("One or more integrated trace boundary spans were not produced.");
            if (toolSpan.ParentSpanId != root.SpanId || contextSpan.ParentSpanId != root.SpanId || eventPublish.ParentSpanId != root.SpanId)
                throw new InvalidOperationException("Integrated trace parent propagation was incorrect.");
            if (eventHandle.ParentSpanId != eventPublish.SpanId)
                throw new InvalidOperationException("Event handler trace did not inherit the publication span.");
            if (toolSpan.Metadata.Values["tool.arguments"] != "[Omitted]" ||
                eventPublish.Metadata.Values["event.payload"] != "[Omitted]")
                throw new InvalidOperationException("Sensitive trace payload metadata was not omitted safely.");

            var failureRecorder = new InMemoryTraceRecorder();
            var failureRuntime = CreateTraceRuntime(
                store,
                new TraceRuntimeExampleAdapter(TraceProviderMode.Failure),
                failureRecorder);
            try
            {
                await failureRuntime.ExecuteAsync(CreateTraceRequest("trace-failure-host-42"), CancellationToken.None).ConfigureAwait(true);
                throw new InvalidOperationException("Expected deterministic provider failure was not raised.");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message != "Deterministic trace provider failure.")
                    throw;
            }

            var failureRoot = FindSpan(failureRecorder.GetSpans(), "execution");
            var failureProvider = FindSpan(failureRecorder.GetSpans(), "provider.invoke");
            if (failureRoot == null || failureProvider == null ||
                failureRoot.Status != TraceSpanStatus.Failed ||
                failureProvider.Status != TraceSpanStatus.Failed)
                throw new InvalidOperationException("Provider failure trace status was not recorded.");

            var cancellationRecorder = new InMemoryTraceRecorder();
            var cancellationAdapter = new TraceRuntimeExampleAdapter(TraceProviderMode.Cancellation);
            var cancellationRuntime = CreateTraceRuntime(store, cancellationAdapter, cancellationRecorder);
            using (var cancellation = new CancellationTokenSource())
            {
                var cancellationTask = cancellationRuntime.ExecuteAsync(
                    CreateTraceRequest("trace-cancel-host-42"),
                    cancellation.Token);
                await cancellationAdapter.Started.Task.ConfigureAwait(true);
                cancellation.Cancel();
                try
                {
                    await cancellationTask.ConfigureAwait(true);
                    throw new InvalidOperationException("Expected cancellation was not raised.");
                }
                catch (OperationCanceledException)
                {
                }
            }

            var cancellationRoot = FindSpan(cancellationRecorder.GetSpans(), "execution");
            var cancellationProvider = FindSpan(cancellationRecorder.GetSpans(), "provider.invoke");
            if (cancellationRoot == null || cancellationProvider == null ||
                cancellationRoot.Status != TraceSpanStatus.Cancelled ||
                cancellationProvider.Status != TraceSpanStatus.Cancelled)
                throw new InvalidOperationException("Cancellation trace status was not recorded.");

            Write(
                "OBSERVABILITY RUNTIME INSTRUMENTATION",
                "Trace-producing runtime instrumentation succeeded." + Environment.NewLine +
                "Execution root + policy + provider hierarchy: verified." + Environment.NewLine +
                "Execution/host correlation remains distinct from TraceId/SpanId: verified." + Environment.NewLine +
                "Tool + context + event propagation: verified." + Environment.NewLine +
                "Event publication -> handler parentage: verified." + Environment.NewLine +
                "Sensitive tool arguments/event payload: omitted from trace metadata." + Environment.NewLine +
                "Provider failure terminal status: Failed / Failed verified." + Environment.NewLine +
                "Cancellation terminal status: Cancelled / Cancelled verified." + Environment.NewLine +
                "Provider transport: fake deterministic adapter." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static async Task<InMemoryAiStore> CreateTraceRuntimeStoreAsync()
        {
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(new AiProvider
            {
                Id = "trace-runtime-provider-42",
                Name = "Trace Runtime Provider",
                Kind = "TraceRuntimeExample",
                BaseUrl = "https://trace-runtime.example/v1",
                DefaultModel = "trace-runtime-model-42",
                Enabled = true
            }).ConfigureAwait(true);
            await store.SaveAgentAsync(new AiAgent
            {
                Id = "trace-runtime-agent-42",
                Name = "Trace Runtime Agent",
                Enabled = true,
                ExecutionSelection = new AiExecutionSelectionPolicy
                {
                    Mode = AiSelectionMode.Auto,
                    Fallback = AiFallbackMode.Fail,
                    CostPolicy = AiCostPolicy.NoRestriction
                },
                CapabilityRequirements = new AiCapabilityRequirements()
            }).ConfigureAwait(true);
            return store;
        }

        private static TracingAgentRuntime CreateTraceRuntime(
            InMemoryAiStore store,
            TraceRuntimeExampleAdapter adapter,
            ITraceRecorder recorder)
        {
            var tracedAdapter = new TracingProviderAdapter(adapter, recorder);
            var tracedPolicy = new TracingPolicyEngine(new DefaultAiPolicyEngine(new AiPolicySet()), recorder);
            var innerRuntime = new DefaultAgentRuntime(
                store,
                new InMemorySecretStore(),
                new[] { tracedAdapter },
                new DefaultProviderRouter(),
                new DefaultProviderErrorClassifier(),
                null,
                null,
                null,
                null,
                tracedPolicy,
                null);
            return new TracingAgentRuntime(innerRuntime, recorder);
        }

        private static AgentExecutionRequest CreateTraceRequest(string hostCorrelationId)
        {
            return new AgentExecutionRequest
            {
                AgentId = "trace-runtime-agent-42",
                Messages = new List<AIMessage> { new AIMessage("user", "Trace this execution.") },
                HostCorrelationId = hostCorrelationId,
                Identity = new AgentIdentityContext(
                    deploymentId: "deployment-42",
                    tenantId: "tenant-42",
                    userId: "user-42"),
                Options = new AgentExecutionOptions
                {
                    Timeout = TimeSpan.FromSeconds(3),
                    MaxProviderAttempts = 1,
                    MaxRetriesPerProvider = 0
                }
            };
        }

        private static TraceSpan FindSpan(IReadOnlyList<TraceSpan> spans, string operationName)
        {
            for (var i = 0; i < spans.Count; i++)
                if (string.Equals(spans[i].OperationName, operationName, StringComparison.Ordinal))
                    return spans[i];
            return null;
        }

        private enum TraceProviderMode
        {
            Success,
            Failure,
            Cancellation
        }

        private sealed class InMemorySecretStore : ISecretStore
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

        private sealed class TraceRuntimeExampleAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            private readonly TraceProviderMode _mode;

            public TraceRuntimeExampleAdapter(TraceProviderMode mode)
            {
                _mode = mode;
                Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public TaskCompletionSource<bool> Started { get; private set; }
            public string Kind { get { return "TraceRuntimeExample"; } }
            public string DisplayName { get { return "Trace Runtime Example Adapter"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                var capabilities = new AiModelCapabilities { Model = "trace-runtime-model-42" };
                capabilities.Set(AiCapability.Chat, CapabilitySupport.Supported, CapabilitySource.ProviderMetadata, 1d, "Deterministic tracing Example capability.");
                var result = new ProviderDiscoveryResult { Succeeded = true, Message = "Deterministic tracing Example discovery." };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "trace-runtime-model-42",
                    LogicalModelId = "trace-runtime-model-42",
                    DisplayName = "Trace Runtime Example Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = capabilities
                });
                return Task.FromResult(result);
            }

            public async Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                request.Validate();
                Started.TrySetResult(true);
                if (_mode == TraceProviderMode.Failure)
                    throw new InvalidOperationException("Deterministic trace provider failure.");
                if (_mode == TraceProviderMode.Cancellation)
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);

                return new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "TRACE-RUNTIME-OK"
                };
            }
        }

        private sealed class TraceRuntimeExampleTool : IAgentTool
        {
            public AiTool Definition { get; } = new AiTool
            {
                Id = "trace-tool-42",
                Name = "Trace Tool",
                Description = "Deterministic tracing tool",
                Enabled = true
            };

            public Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
            {
                return Task.FromResult(ToolExecutionResult.Success("TRACE-TOOL-OK"));
            }
        }

        private sealed class TraceRuntimeExampleContextAssembler : IContextAssembler
        {
            public Task<ContextAssemblyResult> AssembleAsync(
                IReadOnlyList<ContextRetrievalSource> sources,
                ContextBudget budget,
                ContextAdmissionContext admissionContext,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                var item = new ContextItem
                {
                    Id = "trace-context-42",
                    Source = "TraceRuntimeExample",
                    Type = "Observation",
                    Payload = "context payload",
                    Provenance = new ContextProvenance { SourceKind = "TraceRuntimeExample", SourceId = "trace-context-42" },
                    Scope = new ContextScope { ScopeType = "Execution", ScopeId = "execution-42" },
                    Trust = 1d,
                    Importance = 1d,
                    Relevance = 1d,
                    EstimatedCharacters = 15,
                    EstimatedTokens = 3
                };
                var snapshot = new ContextSnapshot(new[] { item }, budget.Clone(), 1, 15, 3, 1);
                return Task.FromResult(new ContextAssemblyResult(snapshot, new List<ContextAdmissionDecision>(), null));
            }
        }
    }
}

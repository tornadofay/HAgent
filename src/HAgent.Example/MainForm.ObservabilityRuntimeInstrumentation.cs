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
                "Runs a deterministic local execution through the traced runtime and provider boundary, then verifies trace propagation through policy, tool, context, and event operations.",
                "The trace should contain one execution root plus child policy/provider/tool/context/event spans. Existing execution and host correlation remain distinct from TraceId/SpanId, and payloads stay out of metadata.",
                "No real provider is contacted. All provider, policy, tool, context, and event work is deterministic and in-process.",
                RunObservabilityRuntimeInstrumentationTestAsync,
                "Trace propagation",
                "This Slice 3 scenario exercises the first integrated observability producers; exporters, persistence, sampling, retention, and trace UI remain later slices.");
        }

        private async Task RunObservabilityRuntimeInstrumentationTestAsync(string unused)
        {
            var recorder = new InMemoryTraceRecorder();
            var store = new InMemoryAiStore();
            var provider = new AiProvider
            {
                Id = "trace-runtime-provider-42",
                Name = "Trace Runtime Provider",
                Kind = "TraceRuntimeExample",
                BaseUrl = "https://trace-runtime.example/v1",
                DefaultModel = "trace-runtime-model-42",
                Enabled = true
            };
            var agent = new AiAgent
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
            };

            await store.SaveProviderAsync(provider).ConfigureAwait(true);
            await store.SaveAgentAsync(agent).ConfigureAwait(true);

            var rawAdapter = new TraceRuntimeExampleAdapter();
            var tracedAdapter = new TracingProviderAdapter(rawAdapter, recorder);
            var tracedPolicy = new TracingPolicyEngine(new DefaultAiPolicyEngine(new AiPolicySet()), recorder);
            var innerRuntime = new DefaultAgentRuntime(
                store,
                new NullSecretStore(),
                new[] { tracedAdapter },
                policyEngine: tracedPolicy);
            var runtime = new TracingAgentRuntime(innerRuntime, recorder);

            var execution = await runtime.ExecuteAsync(
                new AgentExecutionRequest
                {
                    AgentId = agent.Id,
                    Messages = new List<AIMessage> { new AIMessage("user", "Trace this execution.") },
                    HostCorrelationId = "trace-host-correlation-42",
                    Identity = new AgentIdentityContext
                    {
                        DeploymentId = "deployment-42",
                        TenantId = "tenant-42",
                        UserId = "user-42"
                    },
                    Options = new AgentExecutionOptions
                    {
                        RuntimeInstanceId = "runtime-42",
                        Timeout = TimeSpan.FromSeconds(3),
                        MaxProviderAttempts = 1,
                        MaxRetriesPerProvider = 0
                    }
                },
                CancellationToken.None).ConfigureAwait(true);

            if (execution.State != AgentExecutionState.Succeeded)
                throw new InvalidOperationException("Traced runtime execution did not succeed.");

            var spans = recorder.GetSpans();
            var root = FindSpan(spans, "execution");
            var policy = FindSpan(spans, "policy.evaluate");
            var providerSpan = FindSpan(spans, "provider.invoke");
            if (root == null || policy == null || providerSpan == null)
                throw new InvalidOperationException("Execution, policy, or provider trace spans were not produced.");
            if (providerSpan.ParentSpanId != root.SpanId || providerSpan.TraceId != root.TraceId)
                throw new InvalidOperationException("Provider trace propagation did not preserve the execution parent.");
            if (providerSpan.Correlation.ExecutionId != execution.Id ||
                providerSpan.Correlation.ExecutionCorrelationId != execution.CorrelationId ||
                providerSpan.Correlation.HostCorrelationId != "trace-host-correlation-42")
                throw new InvalidOperationException("Execution correlation was not preserved independently from trace identity.");

            using (TracePropagation.Push(root.Context, root.Record.Correlation))
            {
                var tool = new TracingAgentTool(new TraceRuntimeExampleTool(), recorder);
                var toolResult = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = agent.Id,
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
            if (toolSpan.Metadata.Values.ContainsKey("tool.arguments") && toolSpan.Metadata.Values["tool.arguments"] != "[Omitted]")
                throw new InvalidOperationException("Tool argument metadata was not omitted safely.");
            if (eventPublish.Metadata.Values["event.payload"] != "[Omitted]")
                throw new InvalidOperationException("Event payload was not omitted safely.");

            Write(
                "OBSERVABILITY RUNTIME INSTRUMENTATION",
                "Trace-producing runtime instrumentation succeeded." + Environment.NewLine +
                "Execution root + policy + provider hierarchy: verified." + Environment.NewLine +
                "Execution/host correlation remains distinct from TraceId/SpanId: verified." + Environment.NewLine +
                "Tool + context + event propagation: verified." + Environment.NewLine +
                "Event publication -> handler parentage: verified." + Environment.NewLine +
                "Sensitive tool arguments/event payload: omitted from trace metadata." + Environment.NewLine +
                "Provider transport: fake deterministic adapter." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static TraceSpan FindSpan(IReadOnlyList<TraceSpan> spans, string operationName)
        {
            for (var i = 0; i < spans.Count; i++)
                if (string.Equals(spans[i].OperationName, operationName, StringComparison.Ordinal))
                    return spans[i];
            return null;
        }

        private sealed class TraceRuntimeExampleAdapter : IAiProviderAdapter, IProviderDiscovery
        {
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

            public Task<AIResponse> SendAsync(ProviderExecutionRequest request, CancellationToken cancellationToken)
            {
                request.Validate();
                return Task.FromResult(new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "TRACE-RUNTIME-OK"
                });
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

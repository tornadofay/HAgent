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
    public sealed class ObservabilityRuntimeInstrumentationTests
    {
        [Fact]
        public async Task TracedRuntime_PreservesExecutionHierarchyAndProviderCorrelation()
        {
            var recorder = new InMemoryTraceRecorder();
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(CreateProvider()).ConfigureAwait(false);
            await store.SaveAgentAsync(CreateAgent()).ConfigureAwait(false);

            var rawAdapter = new TraceTestProviderAdapter(TraceProviderMode.Success);
            var runtime = CreateRuntime(store, rawAdapter, recorder);

            var execution = await runtime.ExecuteAsync(
                CreateRequest("host-correlation-42", CancellationToken.None)).ConfigureAwait(false);

            Assert.Equal(AgentExecutionState.Succeeded, execution.State);
            var spans = recorder.GetSpans();
            Assert.True(spans.Count >= 3);

            TraceSpan root = Find(spans, "execution");
            TraceSpan policy = Find(spans, "policy.evaluate");
            TraceSpan provider = Find(spans, "provider.invoke");

            Assert.NotNull(root);
            Assert.NotNull(policy);
            Assert.NotNull(provider);
            Assert.Null(root.ParentSpanId);
            Assert.Equal(root.TraceId, policy.TraceId);
            Assert.Equal(root.TraceId, provider.TraceId);
            Assert.Equal(root.SpanId, policy.ParentSpanId);
            Assert.Equal(root.SpanId, provider.ParentSpanId);
            Assert.Equal(execution.Id, root.Correlation.ExecutionId);
            Assert.Equal(execution.CorrelationId, provider.Correlation.ExecutionCorrelationId);
            Assert.Equal("host-correlation-42", provider.Correlation.HostCorrelationId);
            Assert.Equal("runtime-42", provider.Correlation.RuntimeInstanceId);
            Assert.Equal("deployment-42", provider.Correlation.DeploymentId);
            Assert.Equal("tenant-42", provider.Correlation.TenantId);
            Assert.True(root.IsCompleted);
            Assert.Equal(TraceSpanStatus.Succeeded, root.Status);
        }

        [Fact]
        public async Task TracedRuntime_RecordsFailureAndCancellationTerminalStatuses()
        {
            var failureRecorder = new InMemoryTraceRecorder();
            var failureStore = new InMemoryAiStore();
            await failureStore.SaveProviderAsync(CreateProvider()).ConfigureAwait(false);
            await failureStore.SaveAgentAsync(CreateAgent()).ConfigureAwait(false);
            var failureRuntime = CreateRuntime(
                failureStore,
                new TraceTestProviderAdapter(TraceProviderMode.Failure),
                failureRecorder);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                failureRuntime.ExecuteAsync(CreateRequest("host-failure-42", CancellationToken.None)));

            var failureRoot = Find(failureRecorder.GetSpans(), "execution");
            Assert.NotNull(failureRoot);
            Assert.True(failureRoot.IsCompleted);
            Assert.Equal(TraceSpanStatus.Failed, failureRoot.Status);

            var cancellationRecorder = new InMemoryTraceRecorder();
            var cancellationStore = new InMemoryAiStore();
            await cancellationStore.SaveProviderAsync(CreateProvider()).ConfigureAwait(false);
            await cancellationStore.SaveAgentAsync(CreateAgent()).ConfigureAwait(false);
            var cancellationAdapter = new TraceTestProviderAdapter(TraceProviderMode.Cancellation);
            var cancellationRuntime = CreateRuntime(cancellationStore, cancellationAdapter, cancellationRecorder);
            using (var cancellation = new CancellationTokenSource())
            {
                var executionTask = cancellationRuntime.ExecuteAsync(CreateRequest("host-cancel-42", cancellation.Token));
                await cancellationAdapter.Started.Task.ConfigureAwait(false);
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => executionTask);
            }

            var cancellationRoot = Find(cancellationRecorder.GetSpans(), "execution");
            Assert.NotNull(cancellationRoot);
            Assert.True(cancellationRoot.IsCompleted);
            Assert.Equal(TraceSpanStatus.Cancelled, cancellationRoot.Status);
        }

        [Fact]
        public async Task TracedBoundaries_PropagateParentAndKeepPayloadsOutOfTrace()
        {
            var recorder = new InMemoryTraceRecorder();
            var rootCorrelation = new TraceCorrelation
            {
                ExecutionId = "execution-42",
                ExecutionCorrelationId = "execution-correlation-42",
                HostCorrelationId = "host-correlation-42",
                RuntimeInstanceId = "runtime-42"
            };
            var root = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = rootCorrelation
            });

            using (TracePropagation.Push(root.Context, root.Record.Correlation))
            {
                var tool = new TracingAgentTool(new TestAgentTool(), recorder);
                var toolResult = await tool.ExecuteAsync(new ToolExecutionContext
                {
                    AgentId = "agent-42",
                    ToolId = "tool-42",
                    ToolCallId = "call-42",
                    CorrelationId = "execution-correlation-42",
                    Arguments = new Dictionary<string, object> { { "secret", "do-not-record" } },
                    CancellationToken = CancellationToken.None
                });
                Assert.True(toolResult.Succeeded);

                var context = new TracingContextAssembler(new FixedContextAssembler(), recorder);
                var assembly = await context.AssembleAsync(
                    new List<ContextRetrievalSource>(),
                    new ContextBudget { MaxItems = 2, MaxCharacters = 200 },
                    new ContextAdmissionContext(),
                    CancellationToken.None);
                Assert.NotNull(assembly.Snapshot);

                var dispatcher = new TracingEventDispatcher(new InMemoryEventDispatcher(), recorder);
                var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                dispatcher.Subscribe(new EventSubscription
                {
                    Id = "trace-handler-42",
                    Handler = (envelope, token) =>
                    {
                        handled.TrySetResult(true);
                        return Task.CompletedTask;
                    }
                });

                var publish = await dispatcher.PublishAsync(new EventEnvelope
                {
                    Id = "event-42",
                    Type = "TraceEvent",
                    Source = EventSource.Runtime,
                    CorrelationId = "event-correlation-42",
                    CausationId = "cause-42",
                    PayloadJson = "{\"secret\":\"do-not-record\"}"
                }, CancellationToken.None);
                Assert.True(publish.Accepted);

                var completed = await Task.WhenAny(handled.Task, Task.Delay(TimeSpan.FromSeconds(2)));
                Assert.Same(handled.Task, completed);
                dispatcher.Dispose();
            }

            root.TryComplete(TraceSpanStatus.Succeeded);
            var spans = recorder.GetSpans();
            TraceSpan toolSpan = Find(spans, "tool.execute");
            TraceSpan contextSpan = Find(spans, "context.assemble");
            TraceSpan eventPublish = Find(spans, "event.publish");
            TraceSpan eventHandle = Find(spans, "event.handle");

            Assert.NotNull(toolSpan);
            Assert.NotNull(contextSpan);
            Assert.NotNull(eventPublish);
            Assert.NotNull(eventHandle);
            Assert.Equal(root.SpanId, toolSpan.ParentSpanId);
            Assert.Equal(root.SpanId, contextSpan.ParentSpanId);
            Assert.Equal(root.SpanId, eventPublish.ParentSpanId);
            Assert.Equal(eventPublish.SpanId, eventHandle.ParentSpanId);
            Assert.Equal(root.Correlation.ExecutionId, toolSpan.Correlation.ExecutionId);
            Assert.Equal(root.Correlation.ExecutionId, contextSpan.Correlation.ExecutionId);
            Assert.DoesNotContain(toolSpan.Metadata.Values.Values, x => x.Contains("do-not-record"));
            Assert.DoesNotContain(eventPublish.Metadata.Values.Values, x => x.Contains("do-not-record"));
        }

        [Fact]
        public async Task TracedRuntime_RestoresOuterTraceContextAfterCompletion()
        {
            var recorder = new InMemoryTraceRecorder();
            var outerCorrelation = new TraceCorrelation { ExecutionId = "outer-execution-42" };
            var outer = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "outer",
                Kind = "Execution",
                Correlation = outerCorrelation
            });
            var store = new InMemoryAiStore();
            await store.SaveProviderAsync(CreateProvider()).ConfigureAwait(false);
            await store.SaveAgentAsync(CreateAgent()).ConfigureAwait(false);
            var runtime = CreateRuntime(store, new TraceTestProviderAdapter(TraceProviderMode.Success), recorder);

            using (TracePropagation.Push(outer.Context, outer.Record.Correlation))
            {
                await runtime.ExecuteAsync(CreateRequest("nested-host-42", CancellationToken.None)).ConfigureAwait(false);
                Assert.NotNull(TracePropagation.Current);
                Assert.Equal(outer.TraceId, TracePropagation.Current.TraceId);
            }

            Assert.Null(TracePropagation.Current);
            outer.TryComplete(TraceSpanStatus.Succeeded);
        }

        private static DefaultExecutionRequest CreateRequest(string hostCorrelationId, CancellationToken cancellationToken)
        {
            return new DefaultExecutionRequest(hostCorrelationId, cancellationToken).Value;
        }

        private static TracingAgentRuntime CreateRuntime(
            InMemoryAiStore store,
            TraceTestProviderAdapter adapter,
            ITraceRecorder recorder)
        {
            var tracedPolicy = new TracingPolicyEngine(new DefaultAiPolicyEngine(new AiPolicySet()), recorder);
            var tracedAdapter = new TracingProviderAdapter(adapter, recorder);
            var inner = new DefaultAgentRuntime(
                store,
                new NullSecretStore(),
                new[] { tracedAdapter },
                policyEngine: tracedPolicy);
            return new TracingAgentRuntime(inner, recorder);
        }

        private static AiProvider CreateProvider()
        {
            return new AiProvider
            {
                Id = "trace-provider-42",
                Name = "Trace Provider",
                Kind = "TraceTest",
                BaseUrl = "https://trace.test/v1",
                DefaultModel = "trace-model-42",
                Enabled = true
            };
        }

        private static AiAgent CreateAgent()
        {
            return new AiAgent
            {
                Id = "trace-agent-42",
                Name = "Trace Agent",
                Enabled = true
            };
        }

        private static TraceSpan Find(IReadOnlyList<TraceSpan> spans, string operationName)
        {
            foreach (var span in spans)
                if (string.Equals(span.OperationName, operationName, StringComparison.Ordinal))
                    return span;
            return null;
        }

        private sealed class DefaultExecutionRequest
        {
            public DefaultExecutionRequest(string hostCorrelationId, CancellationToken token)
            {
                Value = new AgentExecutionRequest
                {
                    AgentId = "trace-agent-42",
                    Messages = new List<AIMessage> { new AIMessage("user", "trace this") },
                    HostCorrelationId = hostCorrelationId,
                    Identity = new AgentIdentityContext
                    {
                        DeploymentId = "deployment-42",
                        TenantId = "tenant-42",
                        UserId = "user-42"
                    },
                    Options = new AgentExecutionOptions
                    {
                        RuntimeInstanceId = "runtime-42",
                        Timeout = TimeSpan.FromSeconds(2),
                        MaxProviderAttempts = 1,
                        MaxRetriesPerProvider = 0
                    }
                };
            }

            public AgentExecutionRequest Value { get; private set; }
        }

        private enum TraceProviderMode
        {
            Success,
            Failure,
            Cancellation
        }

        private sealed class TraceTestProviderAdapter : IAiProviderAdapter, IProviderDiscovery
        {
            private readonly TraceProviderMode _mode;

            public TraceTestProviderAdapter(TraceProviderMode mode)
            {
                _mode = mode;
                Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public TaskCompletionSource<bool> Started { get; private set; }
            public string Kind { get { return "TraceTest"; } }
            public string DisplayName { get { return "Trace Test Provider"; } }

            public bool CanHandle(AiProvider provider)
            {
                return provider != null && string.Equals(provider.Kind, Kind, StringComparison.OrdinalIgnoreCase);
            }

            public Task<ProviderDiscoveryResult> DiscoverAsync(AiProvider provider, string apiKey, CancellationToken cancellationToken)
            {
                var result = new ProviderDiscoveryResult
                {
                    Succeeded = true,
                    Message = "Trace test discovery"
                };
                result.Models.Add(new AiModelMetadata
                {
                    ProviderId = provider.Id,
                    ModelId = "trace-model-42",
                    LogicalModelId = "trace-model-42",
                    DisplayName = "Trace Model",
                    Cost = AiCostStatus.Free,
                    Source = AiMetadataSource.DiscoveryApi,
                    Capabilities = new AiModelCapabilities { Model = "trace-model-42" }
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
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
                }

                return new AIResponse
                {
                    AgentId = request.Agent.Id,
                    ProviderId = request.Provider.Id,
                    Model = request.ExecutionTarget.ModelId,
                    Text = "TRACE-OK"
                };
            }
        }

        private sealed class TestAgentTool : IAgentTool
        {
            public AiTool Definition { get; } = new AiTool { Id = "tool-42", Name = "Trace Tool", Description = "Trace test tool", Enabled = true };

            public Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
            {
                return Task.FromResult(ToolExecutionResult.Success("TOOL-OK"));
            }
        }

        private sealed class FixedContextAssembler : IContextAssembler
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
                    Source = "TraceTest",
                    Type = "Observation",
                    Payload = "sensitive payload",
                    Provenance = new ContextProvenance { SourceKind = "TraceTest", SourceId = "trace-context-42" },
                    Scope = new ContextScope { ScopeType = "Execution", ScopeId = "execution-42" },
                    Trust = 1d,
                    Importance = 1d,
                    Relevance = 1d,
                    CapturedAt = DateTimeOffset.UtcNow,
                    EstimatedCharacters = 18,
                    EstimatedTokens = 3
                };
                var snapshot = new ContextSnapshot(new[] { item }, budget.Clone(), 1, 18, 3, 1);
                return Task.FromResult(new ContextAssemblyResult(snapshot, new List<ContextAdmissionDecision>(), null));
            }
        }
    }
}

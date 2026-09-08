using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Provider-neutral runtime decorator that creates a root execution trace and observes
    /// canonical lifecycle, policy, and context integration boundaries.
    /// </summary>
    public sealed class TracingAgentRuntime : IAgentRuntime
    {
        private sealed class ExecutionTraceState
        {
            public ITraceSpan Root;
            public ITraceSpan Policy;
            public ITraceSpan Context;
        }

        private readonly IAgentRuntime _inner;
        private readonly ITraceRecorder _recorder;
        private readonly ConcurrentDictionary<string, ExecutionTraceState> _executions =
            new ConcurrentDictionary<string, ExecutionTraceState>(StringComparer.OrdinalIgnoreCase);

        public TracingAgentRuntime(IAgentRuntime inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _inner.ExecutionChanged += OnExecutionChanged;
        }

        public event EventHandler<AgentExecutionEventArgs> ExecutionChanged
        {
            add { _inner.ExecutionChanged += value; }
            remove { _inner.ExecutionChanged -= value; }
        }

        public Task<AgentExecution> ExecuteAsync(
            string agentId,
            string message,
            AgentExecutionOptions options = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required.", nameof(message));

            return ExecuteAsync(
                new AgentExecutionRequest
                {
                    AgentId = agentId,
                    Messages = new List<AIMessage> { new AIMessage("user", message) }.AsReadOnly(),
                    HostCorrelationId = options == null ? string.Empty : options.HostCorrelationId,
                    HostContext = options == null ? null : options.HostContext,
                    Options = options ?? new AgentExecutionOptions()
                },
                cancellationToken);
        }

        public async Task<AgentExecution> ExecuteAsync(
            AgentExecutionRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var previous = TraceAmbient.Current;
            var execution = await ExecuteInnerAsync(request, cancellationToken).ConfigureAwait(false);
            if (execution != null)
                CompleteRoot(execution);

            if (previous == null)
                TraceAmbient.Set(null, null);
            else
                TraceAmbient.Set(previous, TraceAmbient.CurrentCorrelation);

            return execution;
        }

        private async Task<AgentExecution> ExecuteInnerAsync(
            AgentExecutionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inner.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch (OperationCanceledException)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    var current = TraceAmbient.Current;
                    if (current != null)
                    {
                        var matching = FindRootByTrace(current.TraceId);
                        if (matching != null)
                            matching.Root.TryComplete(TraceSpanStatus.Timeout);
                    }
                }
                else
                {
                    var current = TraceAmbient.Current;
                    if (current != null)
                    {
                        var matching = FindRootByTrace(current.TraceId);
                        if (matching != null)
                            matching.Root.TryComplete(TraceSpanStatus.Cancelled);
                    }
                }
                throw;
            }
            catch
            {
                var current = TraceAmbient.Current;
                if (current != null)
                {
                    var matching = FindRootByTrace(current.TraceId);
                    if (matching != null)
                        matching.Root.TryComplete(TraceSpanStatus.Failed);
                }
                throw;
            }
        }

        private void OnExecutionChanged(object sender, AgentExecutionEventArgs args)
        {
            var execution = args == null ? null : args.Execution;
            if (execution == null)
                return;

            if (execution.State == AgentExecutionState.Created)
            {
                var correlation = new TraceCorrelation
                {
                    DeploymentId = execution.Identity == null ? string.Empty : execution.Identity.DeploymentId ?? string.Empty,
                    TenantId = execution.Identity == null ? string.Empty : execution.Identity.TenantId ?? string.Empty,
                    PrincipalId = execution.Identity == null ? string.Empty : execution.Identity.PrincipalId ?? string.Empty,
                    UserId = execution.Identity == null ? string.Empty : execution.Identity.UserId ?? string.Empty,
                    SessionId = execution.Identity == null ? string.Empty : execution.Identity.SessionId ?? string.Empty,
                    WorkspaceId = execution.Identity == null ? string.Empty : execution.Identity.WorkspaceId ?? string.Empty,
                    AgentProfileId = execution.Snapshot == null || execution.Snapshot.Agent == null ? string.Empty : execution.Snapshot.Agent.Id,
                    RuntimeInstanceId = execution.RuntimeInstanceId ?? string.Empty,
                    ExecutionId = execution.Id ?? string.Empty,
                    ExecutionCorrelationId = execution.CorrelationId ?? string.Empty,
                    HostCorrelationId = execution.HostCorrelationId ?? string.Empty
                };

                var metadata = new TraceMetadata();
                metadata.Add("agent.id", correlation.AgentProfileId);
                metadata.Add("execution.id", correlation.ExecutionId);

                var root = _recorder.StartSpan(new TraceSpanStartOptions
                {
                    ParentContext = TraceAmbient.Current,
                    OperationName = "execution",
                    Kind = "Execution",
                    Correlation = correlation,
                    Metadata = metadata
                });

                var state = new ExecutionTraceState { Root = root };
                _executions[execution.Id] = state;
                TraceAmbient.Set(root.Context, correlation);
                return;
            }

            ExecutionTraceState traceState;
            if (!_executions.TryGetValue(execution.Id, out traceState))
                return;

            TraceAmbient.Set(traceState.Root.Context, traceState.Root.Record.Correlation);

            if (execution.State == AgentExecutionState.Running)
            {
                if (traceState.Policy == null && execution.PolicyDecision != null)
                {
                    var metadata = new TraceMetadata();
                    metadata.Add("policy.outcome", execution.PolicyDecision.Outcome.ToString());
                    if (!string.IsNullOrWhiteSpace(execution.PolicyDecision.RuleId))
                        metadata.Add("policy.rule.id", execution.PolicyDecision.RuleId);
                    if (!string.IsNullOrWhiteSpace(execution.PolicyDecision.Reason))
                        metadata.Add("policy.reason", execution.PolicyDecision.Reason.Length > 512
                            ? execution.PolicyDecision.Reason.Substring(0, 512)
                            : execution.PolicyDecision.Reason);

                    traceState.Policy = _recorder.StartSpan(new TraceSpanStartOptions
                    {
                        ParentContext = traceState.Root.Context,
                        OperationName = "policy.evaluate",
                        Kind = "Policy",
                        Correlation = traceState.Root.Record.Correlation,
                        Metadata = metadata
                    });
                    traceState.Policy.TryComplete(execution.PolicyDecision.IsDenied || execution.PolicyDecision.RequiresApproval
                        ? TraceSpanStatus.Rejected
                        : execution.PolicyDecision.IsDeferred
                            ? TraceSpanStatus.Skipped
                            : TraceSpanStatus.Succeeded);
                }

                if (traceState.Context == null && execution.Snapshot != null && execution.Snapshot.Context != null)
                {
                    var metadata = new TraceMetadata();
                    metadata.Add("context.selected.items", execution.Snapshot.Context.UsedItems.ToString());
                    metadata.Add("context.sources", execution.Snapshot.Context.SourceCount.ToString());
                    traceState.Context = _recorder.StartSpan(new TraceSpanStartOptions
                    {
                        ParentContext = traceState.Root.Context,
                        OperationName = "context.assemble",
                        Kind = "Context",
                        Correlation = traceState.Root.Record.Correlation,
                        Metadata = metadata
                    });
                    traceState.Context.TryComplete(TraceSpanStatus.Succeeded);
                }
            }

            if (execution.State == AgentExecutionState.Succeeded ||
                execution.State == AgentExecutionState.Failed ||
                execution.State == AgentExecutionState.Cancelled)
            {
                CompleteRoot(execution);
            }
        }

        private void CompleteRoot(AgentExecution execution)
        {
            ExecutionTraceState state;
            if (!_executions.TryRemove(execution.Id, out state) || state.Root.Record.IsCompleted)
                return;

            CompleteRoot(state.Root, execution.State, execution.FailureKind);
            TraceAmbient.Set(null, null);
        }

        private ExecutionTraceState FindRootByTrace(string traceId)
        {
            foreach (var pair in _executions)
            {
                if (pair.Value.Root.Record.TraceId == traceId)
                    return pair.Value;
            }
            return null;
        }

        private static void CompleteRoot(ITraceSpan root, AgentExecutionState state, AgentExecutionFailureKind failureKind)
        {
            if (root == null || root.Record.IsCompleted)
                return;

            switch (state)
            {
                case AgentExecutionState.Succeeded:
                    root.TryComplete(TraceSpanStatus.Succeeded);
                    break;
                case AgentExecutionState.Cancelled:
                    root.TryComplete(failureKind == AgentExecutionFailureKind.Timeout
                        ? TraceSpanStatus.Timeout
                        : TraceSpanStatus.Cancelled);
                    break;
                case AgentExecutionState.Failed:
                    root.TryComplete(TraceSpanStatus.Failed);
                    break;
                default:
                    root.TryComplete(TraceSpanStatus.Unset);
                    break;
            }
        }
    }
}

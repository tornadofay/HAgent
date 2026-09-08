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
    /// the canonical execution lifecycle. Other HAgent boundaries have dedicated tracing producers.
    /// </summary>
    public sealed class TracingAgentRuntime : IAgentRuntime
    {
        private readonly IAgentRuntime _inner;
        private readonly ITraceRecorder _recorder;
        private readonly ConcurrentDictionary<string, ITraceSpan> _executions =
            new ConcurrentDictionary<string, ITraceSpan>(StringComparer.OrdinalIgnoreCase);

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
            var previousCorrelation = TraceAmbient.CurrentCorrelation;
            try
            {
                var execution = await _inner.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
                if (execution != null)
                    CompleteRoot(execution);
                return execution;
            }
            catch (OperationCanceledException)
            {
                var current = TraceAmbient.Current;
                if (current != null)
                {
                    var root = FindRootByTrace(current.TraceId);
                    if (root != null)
                    {
                        root.TryComplete(cancellationToken.IsCancellationRequested
                            ? TraceSpanStatus.Cancelled
                            : TraceSpanStatus.Timeout);
                        RemoveCompletedRoot(root);
                    }
                }
                throw;
            }
            catch
            {
                var current = TraceAmbient.Current;
                if (current != null)
                {
                    var root = FindRootByTrace(current.TraceId);
                    if (root != null)
                    {
                        root.TryComplete(TraceSpanStatus.Failed);
                        RemoveCompletedRoot(root);
                    }
                }
                throw;
            }
            finally
            {
                TraceAmbient.Set(previous, previousCorrelation);
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

                _executions[execution.Id] = root;
                TraceAmbient.Set(root.Context, correlation);
                return;
            }

            ITraceSpan traceState;
            if (!_executions.TryGetValue(execution.Id, out traceState))
                return;

            TraceAmbient.Set(traceState.Context, traceState.Record.Correlation);

            if (execution.State == AgentExecutionState.Succeeded ||
                execution.State == AgentExecutionState.Failed ||
                execution.State == AgentExecutionState.Cancelled)
            {
                CompleteRoot(execution);
            }
        }

        private void CompleteRoot(AgentExecution execution)
        {
            ITraceSpan root;
            if (!_executions.TryRemove(execution.Id, out root))
                return;
            if (root.Record.IsCompleted)
                return;

            switch (execution.State)
            {
                case AgentExecutionState.Succeeded:
                    root.TryComplete(TraceSpanStatus.Succeeded);
                    break;
                case AgentExecutionState.Cancelled:
                    root.TryComplete(execution.FailureKind == AgentExecutionFailureKind.Timeout
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

        private void RemoveCompletedRoot(ITraceSpan completed)
        {
            if (completed == null)
                return;
            foreach (var pair in _executions)
            {
                if (ReferenceEquals(pair.Value, completed))
                {
                    ITraceSpan ignored;
                    _executions.TryRemove(pair.Key, out ignored);
                    return;
                }
            }
        }

        private ITraceSpan FindRootByTrace(string traceId)
        {
            foreach (var pair in _executions)
            {
                if (pair.Value.Record.TraceId == traceId)
                    return pair.Value;
            }
            return null;
        }
    }
}

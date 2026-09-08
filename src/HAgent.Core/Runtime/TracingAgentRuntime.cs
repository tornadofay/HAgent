using System;
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
        private readonly IAgentRuntime _inner;
        private readonly ITraceRecorder _recorder;

        public TracingAgentRuntime(IAgentRuntime inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
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

            var metadata = new TraceMetadata();
            metadata.Add("agent.id", request.AgentId ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(request.HostCorrelationId))
                metadata.Add("host.correlation", request.HostCorrelationId);

            var correlation = new TraceCorrelation
            {
                HostCorrelationId = request.HostCorrelationId ?? string.Empty,
                DeploymentId = request.Identity == null ? string.Empty : request.Identity.DeploymentId ?? string.Empty,
                TenantId = request.Identity == null ? string.Empty : request.Identity.TenantId ?? string.Empty,
                PrincipalId = request.Identity == null ? string.Empty : request.Identity.PrincipalId ?? string.Empty,
                UserId = request.Identity == null ? string.Empty : request.Identity.UserId ?? string.Empty,
                SessionId = request.Identity == null ? string.Empty : request.Identity.SessionId ?? string.Empty,
                WorkspaceId = request.Identity == null ? string.Empty : request.Identity.WorkspaceId ?? string.Empty,
                AgentProfileId = request.AgentId ?? string.Empty
            };

            var root = _recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = TraceAmbient.Current,
                OperationName = "execution",
                Kind = "Execution",
                Correlation = correlation,
                Metadata = metadata
            });

            using (TraceAmbient.Push(root.Context))
            {
                try
                {
                    var execution = await _inner.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
                    CompleteRoot(root, execution == null ? AgentExecutionState.Failed : execution.State,
                        execution == null ? AgentExecutionFailureKind.Unknown : execution.FailureKind);
                    return execution;
                }
                catch (OperationCanceledException)
                {
                    root.TryComplete(cancellationToken.IsCancellationRequested
                        ? TraceSpanStatus.Cancelled
                        : TraceSpanStatus.Timeout);
                    throw;
                }
                catch (Exception)
                {
                    root.TryComplete(TraceSpanStatus.Failed);
                    throw;
                }
            }
        }

        private void ObserveExecutionChanged(object sender, AgentExecutionEventArgs args)
        {
            var execution = args == null ? null : args.Execution;
            if (execution == null)
                return;
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

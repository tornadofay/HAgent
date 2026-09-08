using System;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Agent-tool decorator that creates a child trace span around executable tool work.
    /// Raw arguments/results are never copied into trace metadata.
    /// </summary>
    public sealed class TracingAgentTool : IAgentTool
    {
        private readonly IAgentTool _inner;
        private readonly ITraceRecorder _recorder;

        public TracingAgentTool(IAgentTool inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        public AiTool Definition { get { return _inner.Definition; } }

        public async Task<ToolExecutionResult> ExecuteAsync(ToolExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var metadata = new TraceMetadata();
            metadata.Add("tool.id", context.ToolId ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(context.ToolCallId))
                metadata.Add("tool.call.id", context.ToolCallId);
            if (!string.IsNullOrWhiteSpace(context.AgentId))
                metadata.Add("agent.id", context.AgentId);
            if (!string.IsNullOrWhiteSpace(context.CorrelationId))
                metadata.Add("execution.correlation", context.CorrelationId);
            metadata.AddOmitted("tool.arguments");

            var span = _recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = TraceAmbient.Current,
                OperationName = "tool.execute",
                Kind = "Tool",
                Metadata = metadata
            });

            using (TraceAmbient.Push(span.Context))
            {
                try
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    var result = await _inner.ExecuteAsync(context).ConfigureAwait(false);
                    if (result == null)
                    {
                        span.TryComplete(TraceSpanStatus.Failed);
                        return null;
                    }

                    span.TryComplete(result.Succeeded
                        ? TraceSpanStatus.Succeeded
                        : TraceSpanStatus.Failed);
                    return result;
                }
                catch (OperationCanceledException)
                {
                    span.TryComplete(context.CancellationToken.IsCancellationRequested
                        ? TraceSpanStatus.Cancelled
                        : TraceSpanStatus.Timeout);
                    throw;
                }
                catch
                {
                    span.TryComplete(TraceSpanStatus.Failed);
                    throw;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Context assembler decorator that records the bounded context integration boundary.
    /// It records metadata only; context payloads are never copied into the trace.
    /// </summary>
    public sealed class TracingContextAssembler : IContextAssembler
    {
        private readonly IContextAssembler _inner;
        private readonly ITraceRecorder _recorder;

        public TracingContextAssembler(IContextAssembler inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        public async Task<ContextAssemblyResult> AssembleAsync(
            IReadOnlyList<ContextRetrievalSource> sources,
            ContextBudget budget,
            ContextAdmissionContext admissionContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var metadata = new TraceMetadata();
            metadata.Add("context.sources", (sources == null ? 0 : sources.Count).ToString());
            metadata.Add("context.max.items", budget == null ? string.Empty : budget.MaxItems.ToString());

            var span = _recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = TraceAmbient.Current,
                OperationName = "context.assemble",
                Kind = "Context",
                Metadata = metadata
            });

            using (TraceAmbient.Push(span.Context))
            {
                try
                {
                    var result = await _inner.AssembleAsync(
                        sources,
                        budget == null ? null : budget.Clone(),
                        admissionContext,
                        cancellationToken).ConfigureAwait(false);

                    if (result == null || result.Snapshot == null)
                    {
                        span.TryComplete(TraceSpanStatus.Failed);
                        return result;
                    }

                    span.Record.Metadata.Add("context.selected.items", result.Snapshot.UsedItems.ToString());
                    span.Record.Metadata.Add("context.admission.decisions", (result.AdmissionDecisions == null ? 0 : result.AdmissionDecisions.Count).ToString());
                    span.TryComplete(TraceSpanStatus.Succeeded);
                    return result;
                }
                catch (OperationCanceledException)
                {
                    span.TryComplete(cancellationToken.IsCancellationRequested
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

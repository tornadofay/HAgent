using System;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Records bounded runtime decisions beneath the current traced operation.
    /// When no traced recorder is active, observations are intentionally suppressed.
    /// </summary>
    public static class TraceObservation
    {
        public static bool IsEnabled
        {
            get { return TraceAmbient.Current != null && TraceAmbient.CurrentRecorder != null; }
        }

        public static ITraceSpan Start(
            string operationName,
            string kind,
            TraceMetadata metadata = null,
            TraceCorrelation correlation = null)
        {
            var recorder = TraceAmbient.CurrentRecorder;
            var parent = TraceAmbient.Current;
            if (recorder == null || parent == null)
                return null;

            return recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = parent,
                OperationName = operationName,
                Kind = kind,
                Correlation = correlation ?? TraceAmbient.CurrentCorrelation ?? new TraceCorrelation(),
                Metadata = metadata ?? new TraceMetadata()
            });
        }

        public static bool RecordDecision(
            string operationName,
            string kind,
            TraceSpanStatus status,
            TraceMetadata metadata = null,
            TraceCorrelation correlation = null)
        {
            var span = Start(operationName, kind, metadata, correlation);
            return span != null && span.TryComplete(status);
        }
    }
}

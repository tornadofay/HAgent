using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    /// <summary>
    /// Bounded options for the human-readable diagnostic trace projection.
    /// </summary>
    public sealed class TraceDiagnosticProjectionOptions
    {
        public TraceDiagnosticProjectionOptions()
        {
            MaxSpans = 256;
            MaxMetadataEntriesPerSpan = 16;
            MaxOperationNameLength = 128;
            MaxKindLength = 64;
            MaxIdentifierLength = 128;
            MaxMetadataKeyLength = 64;
            MaxMetadataValueLength = 256;
        }

        public int MaxSpans { get; set; }
        public int MaxMetadataEntriesPerSpan { get; set; }
        public int MaxOperationNameLength { get; set; }
        public int MaxKindLength { get; set; }
        public int MaxIdentifierLength { get; set; }
        public int MaxMetadataKeyLength { get; set; }
        public int MaxMetadataValueLength { get; set; }

        public void Validate()
        {
            if (MaxSpans <= 0) throw new ArgumentOutOfRangeException(nameof(MaxSpans));
            if (MaxMetadataEntriesPerSpan <= 0) throw new ArgumentOutOfRangeException(nameof(MaxMetadataEntriesPerSpan));
            if (MaxOperationNameLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxOperationNameLength));
            if (MaxKindLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxKindLength));
            if (MaxIdentifierLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxIdentifierLength));
            if (MaxMetadataKeyLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxMetadataKeyLength));
            if (MaxMetadataValueLength <= 0) throw new ArgumentOutOfRangeException(nameof(MaxMetadataValueLength));
        }

        public TraceDiagnosticProjectionOptions Clone()
        {
            return new TraceDiagnosticProjectionOptions
            {
                MaxSpans = MaxSpans,
                MaxMetadataEntriesPerSpan = MaxMetadataEntriesPerSpan,
                MaxOperationNameLength = MaxOperationNameLength,
                MaxKindLength = MaxKindLength,
                MaxIdentifierLength = MaxIdentifierLength,
                MaxMetadataKeyLength = MaxMetadataKeyLength,
                MaxMetadataValueLength = MaxMetadataValueLength
            };
        }
    }

    /// <summary>
    /// Safe metadata item exposed to management/diagnostic consumers.
    /// </summary>
    public sealed class TraceDiagnosticMetadataItem
    {
        internal TraceDiagnosticMetadataItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }

    /// <summary>
    /// Human-readable, bounded projection of one trace span.
    /// </summary>
    public sealed class TraceDiagnosticSpan
    {
        internal TraceDiagnosticSpan(
            string traceId,
            string spanId,
            string parentSpanId,
            string operationName,
            string kind,
            DateTimeOffset startedAt,
            TimeSpan? duration,
            TraceSpanStatus status,
            string executionId,
            string executionCorrelationId,
            string hostCorrelationId,
            string runtimeInstanceId,
            IReadOnlyList<TraceDiagnosticMetadataItem> metadata,
            int omittedMetadataCount)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            OperationName = operationName;
            Kind = kind;
            StartedAt = startedAt;
            Duration = duration;
            Status = status;
            ExecutionId = executionId;
            ExecutionCorrelationId = executionCorrelationId;
            HostCorrelationId = hostCorrelationId;
            RuntimeInstanceId = runtimeInstanceId;
            Metadata = metadata;
            OmittedMetadataCount = omittedMetadataCount;
        }

        public string TraceId { get; private set; }
        public string SpanId { get; private set; }
        public string ParentSpanId { get; private set; }
        public string OperationName { get; private set; }
        public string Kind { get; private set; }
        public DateTimeOffset StartedAt { get; private set; }
        public TimeSpan? Duration { get; private set; }
        public TraceSpanStatus Status { get; private set; }
        public string ExecutionId { get; private set; }
        public string ExecutionCorrelationId { get; private set; }
        public string HostCorrelationId { get; private set; }
        public string RuntimeInstanceId { get; private set; }
        public IReadOnlyList<TraceDiagnosticMetadataItem> Metadata { get; private set; }
        public int OmittedMetadataCount { get; private set; }
    }

    /// <summary>
    /// Safe management/diagnostic projection containing only bounded, allowlisted trace information.
    /// </summary>
    public sealed class TraceDiagnosticProjection
    {
        internal TraceDiagnosticProjection(IReadOnlyList<TraceDiagnosticSpan> spans, int omittedSpanCount)
        {
            Spans = spans;
            OmittedSpanCount = omittedSpanCount;
        }

        public IReadOnlyList<TraceDiagnosticSpan> Spans { get; private set; }
        public int OmittedSpanCount { get; private set; }
    }
}

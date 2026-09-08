using System;
using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryTraceRecorder : ITraceRecorder
    {
        private readonly object _sync = new object();
        private readonly List<TraceSpan> _spans = new List<TraceSpan>();
        private long _sequence;

        public ITraceSpan StartSpan(TraceSpanStartOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            var parent = options.ParentContext;
            var traceId = parent == null ? Guid.NewGuid().ToString("N") : parent.TraceId;
            var parentSpanId = parent == null ? null : parent.ParentSpanId;
            var sampled = parent == null || parent.Sampled;
            var spanId = Guid.NewGuid().ToString("N");
            long sequence;

            lock (_sync)
            {
                sequence = ++_sequence;
                var span = new TraceSpan(
                    traceId,
                    spanId,
                    parentSpanId,
                    options.OperationName,
                    options.Kind,
                    DateTimeOffset.UtcNow,
                    sampled,
                    options.Correlation,
                    options.Metadata,
                    sequence);
                _spans.Add(span);
                return new InMemoryTraceSpanHandle(this, span);
            }
        }

        public IReadOnlyList<TraceSpan> GetSpans()
        {
            lock (_sync)
                return new List<TraceSpan>(_spans).AsReadOnly();
        }

        private sealed class InMemoryTraceSpanHandle : ITraceSpan
        {
            private readonly InMemoryTraceRecorder _owner;
            private readonly TraceSpan _span;

            public InMemoryTraceSpanHandle(InMemoryTraceRecorder owner, TraceSpan span)
            {
                _owner = owner;
                _span = span;
            }

            public TraceSpan Record { get { return _span; } }

            public TraceContext Context
            {
                get { return new TraceContext(_span.TraceId, _span.SpanId, _span.Sampled); }
            }

            public bool TryComplete(TraceSpanStatus status)
            {
                return _span.TryComplete(status, DateTimeOffset.UtcNow);
            }
        }
    }
}

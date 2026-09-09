using System;
using System.Collections.Generic;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryTraceRecorder : ITraceRecorder
    {
        private readonly object _sync = new object();
        private readonly List<TraceSpan> _spans = new List<TraceSpan>();
        private readonly ITraceSampler _sampler;
        private readonly TraceRetentionOptions _retention;
        private long _sequence;

        public InMemoryTraceRecorder()
            : this(null, null)
        {
        }

        public InMemoryTraceRecorder(ITraceSampler sampler, TraceRetentionOptions retentionOptions = null)
        {
            _sampler = sampler;
            _retention = retentionOptions == null ? new TraceRetentionOptions() : retentionOptions.Clone();
            _retention.Validate();
        }

        public ITraceSpan StartSpan(TraceSpanStartOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Validate();

            var parent = options.ParentContext;
            var traceId = parent == null ? Guid.NewGuid().ToString("N") : parent.TraceId;
            var parentSpanId = parent == null ? null : parent.ParentSpanId;
            var sampled = options.Sampled.HasValue
                ? options.Sampled.Value
                : parent != null
                    ? parent.Sampled
                    : _sampler == null || _sampler.ShouldSample(options);
            var spanId = Guid.NewGuid().ToString("N");
            long sequence;
            var startedAt = DateTimeOffset.UtcNow;

            lock (_sync)
            {
                sequence = ++_sequence;
                var span = new TraceSpan(
                    traceId,
                    spanId,
                    parentSpanId,
                    options.OperationName,
                    options.Kind,
                    startedAt,
                    sampled,
                    options.Correlation,
                    options.Metadata,
                    sequence);

                if (sampled)
                    RetainUnsafe(span);

                return new InMemoryTraceSpanHandle(span);
            }
        }

        public IReadOnlyList<TraceSpan> GetSpans()
        {
            lock (_sync)
            {
                PruneExpiredUnsafe(DateTimeOffset.UtcNow);
                return new List<TraceSpan>(_spans).AsReadOnly();
            }
        }

        private void RetainUnsafe(TraceSpan span)
        {
            PruneExpiredUnsafe(span.StartedAt);

            var traceExists = ContainsTraceUnsafe(span.TraceId);
            if (!traceExists && CountTraceIdsUnsafe() >= _retention.MaxTraceCount)
                EvictOldestTraceUnsafe();

            if (CountSpansForTraceUnsafe(span.TraceId) >= _retention.MaxSpansPerTrace)
                return;

            if (_spans.Count >= _retention.MaxSpanCount)
            {
                if (traceExists)
                    return;
                EvictOldestTraceUnsafe();
            }

            var spanCost = EstimateMetadataCharacters(span);
            if (spanCost > _retention.MaxAggregateMetadataCharacters)
                return;

            while (_spans.Count > 0 && AggregateMetadataCharactersUnsafe() + spanCost > _retention.MaxAggregateMetadataCharacters)
            {
                if (!EvictOldestTraceUnsafe(span.TraceId))
                    return;
            }

            traceExists = ContainsTraceUnsafe(span.TraceId);
            if (!traceExists && CountTraceIdsUnsafe() >= _retention.MaxTraceCount)
                return;
            if (_spans.Count >= _retention.MaxSpanCount)
                return;
            if (CountSpansForTraceUnsafe(span.TraceId) >= _retention.MaxSpansPerTrace)
                return;

            _spans.Add(span);
        }

        private void PruneExpiredUnsafe(DateTimeOffset now)
        {
            if (_retention.MaxAge == Timeout.InfiniteTimeSpan || _spans.Count == 0)
                return;

            var cutoff = now - _retention.MaxAge;
            var expiredTraceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var span in _spans)
            {
                if (span.StartedAt < cutoff)
                    expiredTraceIds.Add(span.TraceId);
            }

            if (expiredTraceIds.Count == 0)
                return;

            _spans.RemoveAll(span => expiredTraceIds.Contains(span.TraceId));
        }

        private bool EvictOldestTraceUnsafe(string excludedTraceId = null)
        {
            if (_spans.Count == 0)
                return false;

            string oldestTraceId = null;
            long oldestSequence = long.MaxValue;
            foreach (var span in _spans)
            {
                if (excludedTraceId != null && string.Equals(span.TraceId, excludedTraceId, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (span.Sequence < oldestSequence)
                {
                    oldestSequence = span.Sequence;
                    oldestTraceId = span.TraceId;
                }
            }

            if (oldestTraceId == null)
                return false;

            _spans.RemoveAll(span => string.Equals(span.TraceId, oldestTraceId, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        private bool ContainsTraceUnsafe(string traceId)
        {
            foreach (var span in _spans)
                if (string.Equals(span.TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private int CountTraceIdsUnsafe()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var span in _spans)
                ids.Add(span.TraceId);
            return ids.Count;
        }

        private int CountSpansForTraceUnsafe(string traceId)
        {
            var count = 0;
            foreach (var span in _spans)
                if (string.Equals(span.TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                    count++;
            return count;
        }

        private int AggregateMetadataCharactersUnsafe()
        {
            var total = 0;
            foreach (var span in _spans)
                total += EstimateMetadataCharacters(span);
            return total;
        }

        private static int EstimateMetadataCharacters(TraceSpan span)
        {
            var total = 0;
            foreach (var pair in span.Metadata.Values)
            {
                total += (pair.Key == null ? 0 : pair.Key.Length) + (pair.Value == null ? 0 : pair.Value.Length);
            }
            return total;
        }

        private sealed class InMemoryTraceSpanHandle : ITraceSpan
        {
            private readonly TraceSpan _span;

            public InMemoryTraceSpanHandle(TraceSpan span)
            {
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

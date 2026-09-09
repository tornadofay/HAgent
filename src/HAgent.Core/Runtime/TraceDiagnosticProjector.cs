using System;
using System.Collections.Generic;
using System.Linq;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Creates a deterministic, bounded, human-readable projection of retained trace spans.
    /// </summary>
    public sealed class TraceDiagnosticProjector
    {
        private static readonly string[] SafeMetadataPrefixes =
        {
            "agent.",
            "context.",
            "event.",
            "execution.",
            "failure.",
            "lifecycle.",
            "outcome.",
            "policy.",
            "provider.",
            "resource.",
            "runtime.",
            "sampling.",
            "tool."
        };

        private static readonly HashSet<string> SafeMetadataKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "decision"
            };

        private readonly TraceDiagnosticProjectionOptions _options;

        public TraceDiagnosticProjector()
            : this(null)
        {
        }

        public TraceDiagnosticProjector(TraceDiagnosticProjectionOptions options)
        {
            _options = options == null
                ? new TraceDiagnosticProjectionOptions()
                : options.Clone();
            _options.Validate();
        }

        public TraceDiagnosticProjection Project(IReadOnlyList<TraceSpan> spans)
        {
            if (spans == null) throw new ArgumentNullException(nameof(spans));

            var ordered = spans
                .Where(span => span != null && span.Sampled)
                .OrderBy(span => span.Sequence)
                .ThenBy(span => span.TraceId, StringComparer.Ordinal)
                .ThenBy(span => span.SpanId, StringComparer.Ordinal)
                .ToList();

            var omittedSpans = Math.Max(0, ordered.Count - _options.MaxSpans);
            var projected = new List<TraceDiagnosticSpan>(Math.Min(ordered.Count, _options.MaxSpans));

            for (var index = 0; index < ordered.Count && index < _options.MaxSpans; index++)
                projected.Add(ProjectSpan(ordered[index]));

            return new TraceDiagnosticProjection(projected.AsReadOnly(), omittedSpans);
        }

        private TraceDiagnosticSpan ProjectSpan(TraceSpan span)
        {
            var metadata = new List<TraceDiagnosticMetadataItem>();
            var omittedMetadata = 0;

            foreach (var pair in span.Metadata.Values.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (!IsSafeMetadataKey(pair.Key))
                {
                    omittedMetadata++;
                    continue;
                }

                if (metadata.Count >= _options.MaxMetadataEntriesPerSpan)
                {
                    omittedMetadata++;
                    continue;
                }

                var key = Truncate(pair.Key, _options.MaxMetadataKeyLength);
                var value = pair.Value ?? string.Empty;
                metadata.Add(new TraceDiagnosticMetadataItem(
                    key,
                    Truncate(value, _options.MaxMetadataValueLength)));
            }

            return new TraceDiagnosticSpan(
                Truncate(span.TraceId, _options.MaxIdentifierLength),
                Truncate(span.SpanId, _options.MaxIdentifierLength),
                TruncateOptional(span.ParentSpanId, _options.MaxIdentifierLength),
                Truncate(span.OperationName, _options.MaxOperationNameLength),
                Truncate(span.Kind, _options.MaxKindLength),
                span.StartedAt,
                span.Duration,
                span.Status,
                Truncate(span.Correlation == null ? null : span.Correlation.ExecutionId, _options.MaxIdentifierLength),
                Truncate(span.Correlation == null ? null : span.Correlation.ExecutionCorrelationId, _options.MaxIdentifierLength),
                Truncate(span.Correlation == null ? null : span.Correlation.HostCorrelationId, _options.MaxIdentifierLength),
                Truncate(span.Correlation == null ? null : span.Correlation.RuntimeInstanceId, _options.MaxIdentifierLength),
                metadata.AsReadOnly(),
                omittedMetadata);
        }

        private static bool IsSafeMetadataKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            if (SafeMetadataKeys.Contains(key))
                return true;

            foreach (var prefix in SafeMetadataPrefixes)
            {
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string TruncateOptional(string value, int maxLength)
        {
            return string.IsNullOrEmpty(value) ? value : Truncate(value, maxLength);
        }
    }
}

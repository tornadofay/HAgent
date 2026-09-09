using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddObservabilityDiagnosticProjectionTab()
        {
            AddApiTab(
                "Observability Diagnostic Projection",
                "Run diagnostic projection test",
                "Runs bounded human-readable trace projection checks through the public tracing APIs.",
                "The scenario verifies deterministic ordering, bounded span and text output, safe correlation visibility, allowlisted diagnostic metadata, explicit omission accounting, redaction markers, status/duration/parent relationships, and suppression of unsampled spans.",
                "No network provider or remote telemetry service is contacted. The projection consumes deterministic in-memory trace data only.",
                RunObservabilityDiagnosticProjectionTest,
                "Diagnostic projection",
                "This Slice 6 scenario is a management/UI-safe projection boundary; raw trace storage, payloads, and vendor telemetry remain outside the projection.");
        }

        private async Task RunObservabilityDiagnosticProjectionTest(string unused)
        {
            var recorder = new InMemoryTraceRecorder();
            var root = StartDiagnosticRoot(recorder, "diagnostic-root-42");
            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = new string('o', 40),
                Kind = "Tool",
                Correlation = root.Record.Correlation
            });
            var metadata = new TraceMetadata();
            metadata.Add("provider.id", "provider-42");
            metadata.AddRedacted("provider.secret");
            metadata.Add("decision", "Allow");
            metadata.Add("prompt", "secret prompt content");
            metadata.Add("custom.payload", "sensitive payload");

            var unsafeChild = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "tool.execute",
                Kind = "Tool",
                Correlation = root.Record.Correlation,
                Metadata = metadata
            });

            if (!unsafeChild.TryComplete(TraceSpanStatus.Cancelled) ||
                !child.TryComplete(TraceSpanStatus.Failed) ||
                !root.TryComplete(TraceSpanStatus.Succeeded))
                throw new InvalidOperationException("Diagnostic projection source spans did not complete.");

            var projection = new TraceDiagnosticProjector(new TraceDiagnosticProjectionOptions
            {
                MaxSpans = 8,
                MaxMetadataEntriesPerSpan = 4,
                MaxOperationNameLength = 16,
                MaxKindLength = 16,
                MaxIdentifierLength = 64,
                MaxMetadataKeyLength = 32,
                MaxMetadataValueLength = 64
            }).Project(recorder.GetSpans());

            if (projection.Spans.Count != 3)
                throw new InvalidOperationException("Diagnostic projection did not include all sampled retained spans.");
            if (projection.Spans[0].SpanId != root.Record.SpanId ||
                projection.Spans[1].SpanId != child.Record.SpanId ||
                projection.Spans[2].SpanId != unsafeChild.Record.SpanId)
                throw new InvalidOperationException("Diagnostic projection ordering was not deterministic.");
            if (projection.Spans[1].OperationName.Length > 16 || projection.Spans[1].Kind.Length > 16)
                throw new InvalidOperationException("Diagnostic projection exceeded configured text bounds.");
            if (projection.Spans[2].ExecutionId != "diagnostic-root-42")
                throw new InvalidOperationException("Diagnostic projection lost execution correlation.");
            if (projection.Spans[1].ParentSpanId != root.Record.SpanId)
                throw new InvalidOperationException("Diagnostic projection lost parent relationship.");
            if (projection.Spans[0].Status != TraceSpanStatus.Succeeded ||
                projection.Spans[1].Status != TraceSpanStatus.Failed ||
                projection.Spans[2].Status != TraceSpanStatus.Cancelled)
                throw new InvalidOperationException("Diagnostic projection lost span status.");
            if (!projection.Spans[0].Duration.HasValue || !projection.Spans[1].Duration.HasValue || !projection.Spans[2].Duration.HasValue)
                throw new InvalidOperationException("Diagnostic projection lost completed duration information.");

            var projectedMetadata = projection.Spans[2].Metadata;
            if (projectedMetadata.Count != 3 ||
                projectedMetadata[0].Key != "decision" ||
                projectedMetadata[1].Key != "provider.id" ||
                projectedMetadata[2].Key != "provider.secret")
                throw new InvalidOperationException("Diagnostic metadata allowlist/order was not deterministic.");
            if (projectedMetadata[2].Value != "[Redacted]")
                throw new InvalidOperationException("Diagnostic projection lost the explicit redaction marker.");
            if (unsafeChild.Record.Metadata.Values.ContainsKey("prompt") &&
                ContainsMetadataKey(projectedMetadata, "prompt"))
                throw new InvalidOperationException("Sensitive prompt metadata reached the diagnostic projection.");
            if (unsafeChild.Record.Metadata.Values.ContainsKey("custom.payload") &&
                ContainsMetadataKey(projectedMetadata, "custom.payload"))
                throw new InvalidOperationException("Arbitrary payload metadata reached the diagnostic projection.");
            if (projection.Spans[2].OmittedMetadataCount != 2)
                throw new InvalidOperationException("Diagnostic projection omission accounting was incorrect.");

            var limited = new TraceDiagnosticProjector(new TraceDiagnosticProjectionOptions { MaxSpans = 2 })
                .Project(recorder.GetSpans());
            if (limited.Spans.Count != 2 || limited.OmittedSpanCount != 1)
                throw new InvalidOperationException("Diagnostic projection span bound was not enforced.");

            var unsampledRecorder = new InMemoryTraceRecorder(
                new FixedTraceSampler(false),
                new TraceRetentionOptions());
            var unsampled = StartDiagnosticRoot(unsampledRecorder, "diagnostic-unsampled-42");
            if (!unsampled.TryComplete(TraceSpanStatus.Succeeded))
                throw new InvalidOperationException("Unsampled diagnostic source span did not complete.");
            var unsampledProjection = new TraceDiagnosticProjector().Project(unsampledRecorder.GetSpans());
            if (unsampledProjection.Spans.Count != 0 || unsampledProjection.OmittedSpanCount != 0)
                throw new InvalidOperationException("Unsampled trace data appeared in the diagnostic projection.");

            Write(
                "OBSERVABILITY DIAGNOSTIC PROJECTION",
                "Safe human-readable diagnostic projection succeeded." + Environment.NewLine +
                "Deterministic span ordering: verified." + Environment.NewLine +
                "Bounded span count and text lengths: verified." + Environment.NewLine +
                "Execution correlation and parent relationship: verified." + Environment.NewLine +
                "Status and duration projection: verified." + Environment.NewLine +
                "Allowlisted metadata only: verified." + Environment.NewLine +
                "Explicit [Redacted] marker preserved for safe diagnostic metadata: verified." + Environment.NewLine +
                "Sensitive/arbitrary metadata omission accounting: verified." + Environment.NewLine +
                "Unsampled trace suppression: verified." + Environment.NewLine +
                "Raw prompts/responses, tool payloads, host context, secrets, and arbitrary objects: not exposed." + Environment.NewLine +
                "Provider transport: none." + Environment.NewLine +
                "Remote telemetry transport: none." + Environment.NewLine +
                "Real provider request: none.");

            await Task.CompletedTask.ConfigureAwait(true);
        }

        private static ITraceSpan StartDiagnosticRoot(ITraceRecorder recorder, string executionId)
        {
            return recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = executionId }
            });
        }

        private static bool ContainsMetadataKey(IReadOnlyList<TraceDiagnosticMetadataItem> metadata, string key)
        {
            foreach (var item in metadata)
            {
                if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private sealed class FixedTraceSampler : ITraceSampler
        {
            private readonly bool _sample;

            public FixedTraceSampler(bool sample)
            {
                _sample = sample;
            }

            public bool ShouldSample(TraceSpanStartOptions options)
            {
                return _sample;
            }
        }
    }
}

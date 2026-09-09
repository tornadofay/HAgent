using System;
using System.Collections.Generic;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ObservabilityDiagnosticProjectionTests
    {
        [Fact]
        public void Projector_IsDeterministicAndOrdersBySpanSequence()
        {
            var recorder = new InMemoryTraceRecorder();
            var second = StartRoot(recorder, "diagnostic-second");
            var first = StartRoot(recorder, "diagnostic-first");
            Assert.True(first.TryComplete(TraceSpanStatus.Succeeded));
            Assert.True(second.TryComplete(TraceSpanStatus.Failed));

            var spans = recorder.GetSpans();
            var reversed = new List<TraceSpan> { spans[1], spans[0] };
            var projector = new TraceDiagnosticProjector();

            var projection = projector.Project(reversed);

            Assert.Equal(2, projection.Spans.Count);
            Assert.Equal(second.RecordSpanId(), projection.Spans[0].SpanId);
            Assert.Equal(first.RecordSpanId(), projection.Spans[1].SpanId);
        }

        [Fact]
        public void Projector_BoundsResultAndTextLengths()
        {
            var recorder = new InMemoryTraceRecorder();
            for (var i = 0; i < 5; i++)
            {
                var span = recorder.StartSpan(new TraceSpanStartOptions
                {
                    OperationName = new string('o', 40),
                    Kind = new string('k', 20),
                    Correlation = new TraceCorrelation { ExecutionId = new string('e', 40) }
                });
                Assert.True(span.TryComplete(TraceSpanStatus.Succeeded));
            }

            var projection = new TraceDiagnosticProjector(new TraceDiagnosticProjectionOptions
            {
                MaxSpans = 2,
                MaxOperationNameLength = 10,
                MaxKindLength = 8,
                MaxIdentifierLength = 12
            }).Project(recorder.GetSpans());

            Assert.Equal(2, projection.Spans.Count);
            Assert.Equal(3, projection.OmittedSpanCount);
            Assert.All(projection.Spans, item =>
            {
                Assert.True(item.OperationName.Length <= 10);
                Assert.True(item.Kind.Length <= 8);
                Assert.True(item.ExecutionId.Length <= 12);
            });
        }

        [Fact]
        public void Projector_AllowListsSafeMetadataAndCountsOmittedValues()
        {
            var metadata = new TraceMetadata();
            metadata.Add("provider.id", "provider-42");
            metadata.Add("decision", "Allow");
            metadata.Add("prompt", "secret prompt content");
            metadata.Add("custom.payload", "sensitive custom payload");
            metadata.AddRedacted("request.body");

            var recorder = new InMemoryTraceRecorder();
            var span = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = new TraceCorrelation
                {
                    ExecutionId = "execution-42",
                    ExecutionCorrelationId = "correlation-42",
                    HostCorrelationId = "host-42",
                    RuntimeInstanceId = "runtime-42"
                },
                Metadata = metadata
            });
            Assert.True(span.TryComplete(TraceSpanStatus.Succeeded));

            var diagnostic = new TraceDiagnosticProjector(new TraceDiagnosticProjectionOptions
            {
                MaxMetadataEntriesPerSpan = 16,
                MaxMetadataKeyLength = 64,
                MaxMetadataValueLength = 256
            }).Project(recorder.GetSpans()).Spans[0];

            Assert.Equal("execution-42", diagnostic.ExecutionId);
            Assert.Equal("correlation-42", diagnostic.ExecutionCorrelationId);
            Assert.Equal("host-42", diagnostic.HostCorrelationId);
            Assert.Equal("runtime-42", diagnostic.RuntimeInstanceId);
            Assert.Contains(diagnostic.Metadata, item => item.Key == "provider.id" && item.Value == "provider-42");
            Assert.Contains(diagnostic.Metadata, item => item.Key == "decision" && item.Value == "Allow");
            Assert.DoesNotContain(diagnostic.Metadata, item => item.Key == "prompt");
            Assert.DoesNotContain(diagnostic.Metadata, item => item.Key == "custom.payload");
            Assert.DoesNotContain(diagnostic.Metadata, item => item.Key == "request.body");
            Assert.Equal(3, diagnostic.OmittedMetadataCount);
        }

        [Fact]
        public void Projector_PreservesStatusDurationAndParentRelationship()
        {
            var recorder = new InMemoryTraceRecorder();
            var root = StartRoot(recorder, "diagnostic-root");
            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "tool.execute",
                Kind = "Tool",
                Correlation = root.Record.Correlation
            });
            Assert.True(child.TryComplete(TraceSpanStatus.Cancelled));
            Assert.True(root.TryComplete(TraceSpanStatus.Succeeded));

            var projection = new TraceDiagnosticProjector().Project(recorder.GetSpans());
            var projectedRoot = projection.Spans[0];
            var projectedChild = projection.Spans[1];

            Assert.Equal(TraceSpanStatus.Succeeded, projectedRoot.Status);
            Assert.Equal(TraceSpanStatus.Cancelled, projectedChild.Status);
            Assert.Equal(root.Record.SpanId, projectedChild.ParentSpanId);
            Assert.NotNull(projectedRoot.Duration);
            Assert.NotNull(projectedChild.Duration);
        }

        [Fact]
        public void Projector_ExcludesUnsampledSpansFromDiagnosticRecords()
        {
            var recorder = new InMemoryTraceRecorder(new FixedTraceSampler(false), new TraceRetentionOptions());
            var span = StartRoot(recorder, "diagnostic-unsampled");
            Assert.False(span.Context.Sampled);
            Assert.True(span.TryComplete(TraceSpanStatus.Succeeded));

            var projection = new TraceDiagnosticProjector().Project(
                new[] { span.Record });

            Assert.Empty(projection.Spans);
            Assert.Equal(0, projection.OmittedSpanCount);
        }

        private static ITraceSpan StartRoot(ITraceRecorder recorder, string executionId)
        {
            return recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = executionId }
            });
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

    internal static class TraceSpanTestExtensions
    {
        public static string RecordSpanId(this ITraceSpan span)
        {
            return span.Record.SpanId;
        }
    }
}

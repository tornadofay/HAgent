using System;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public class ObservabilityTraceContractsTests
    {
        [Fact]
        public void TraceRecorder_CreatesHierarchyAndPreservesCorrelation()
        {
            var recorder = new InMemoryTraceRecorder();
            var correlation = new TraceCorrelation
            {
                ExecutionId = "execution-42",
                ExecutionCorrelationId = "execution-correlation-42",
                HostCorrelationId = "host-correlation-42",
                RuntimeInstanceId = "runtime-42",
                EventId = "event-42",
                CausationId = "event-parent-42"
            };

            var root = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = correlation
            });

            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "provider",
                Kind = "Provider",
                Correlation = correlation
            });

            Assert.NotEqual(string.Empty, root.Record.TraceId);
            Assert.NotEqual(string.Empty, root.Record.SpanId);
            Assert.Equal(root.Record.TraceId, child.Record.TraceId);
            Assert.Null(root.Record.ParentSpanId);
            Assert.Equal(root.Record.SpanId, child.Record.ParentSpanId);
            Assert.Equal("execution-42", child.Record.Correlation.ExecutionId);
            Assert.Equal("host-correlation-42", child.Record.Correlation.HostCorrelationId);
            Assert.Equal("event-parent-42", child.Record.Correlation.CausationId);
            Assert.NotEqual(root.Record.SpanId, child.Record.SpanId);
        }

        [Fact]
        public void TraceMetadata_IsBoundedAndSupportsRedactedValuesOnly()
        {
            var metadata = new TraceMetadata();
            metadata.Add("decision", "Allow");
            metadata.AddRedacted("prompt");
            metadata.AddOmitted("tool.arguments");

            Assert.Equal("Allow", metadata.Values["decision"]);
            Assert.Equal("[Redacted]", metadata.Values["prompt"]);
            Assert.Equal("[Omitted]", metadata.Values["tool.arguments"]);
            Assert.Throws<ArgumentOutOfRangeException>(() => metadata.Add("too-large", new string('x', 513)));
            Assert.Throws<ArgumentException>(() => metadata.Add("", "value"));
        }

        [Fact]
        public void TraceSpan_CompletionIsTerminalAndOrderingIsDeterministic()
        {
            var recorder = new InMemoryTraceRecorder();
            var first = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "first",
                Kind = "Execution"
            });
            var second = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = first.Context,
                OperationName = "second",
                Kind = "Outcome"
            });

            Assert.True(first.TryComplete(TraceSpanStatus.Succeeded));
            Assert.False(first.TryComplete(TraceSpanStatus.Failed));
            Assert.True(second.TryComplete(TraceSpanStatus.Cancelled));

            var spans = recorder.GetSpans();
            Assert.Equal(2, spans.Count);
            Assert.Equal(1L, spans[0].Sequence);
            Assert.Equal(2L, spans[1].Sequence);
            Assert.Equal(TraceSpanStatus.Succeeded, spans[0].Status);
            Assert.Equal(TraceSpanStatus.Cancelled, spans[1].Status);
            Assert.True(spans[0].IsCompleted);
            Assert.NotNull(spans[0].CompletedAt);
            Assert.NotNull(spans[0].Duration);
        }
    }
}

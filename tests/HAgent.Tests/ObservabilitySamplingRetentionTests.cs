using System;
using System.Collections.Generic;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ObservabilitySamplingRetentionTests
    {
        [Fact]
        public void DeterministicSampler_IsStableAndHonorsBoundaryRates()
        {
            var options = new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = "execution-sample-42" }
            };

            var none = new DeterministicTraceSampler(new TraceSamplingOptions { SampleRate = 0d, Salt = "test" });
            var all = new DeterministicTraceSampler(new TraceSamplingOptions { SampleRate = 1d, Salt = "test" });
            var middle = new DeterministicTraceSampler(new TraceSamplingOptions { SampleRate = 0.5d, Salt = "test" });

            Assert.False(none.ShouldSample(options));
            Assert.True(all.ShouldSample(options));
            Assert.Equal(middle.ShouldSample(options), middle.ShouldSample(options));
        }

        [Fact]
        public void InMemoryRecorder_UnsampledRootAndChildrenRemainUnsampledAndUnretained()
        {
            var recorder = new InMemoryTraceRecorder(
                new DeterministicTraceSampler(new TraceSamplingOptions { SampleRate = 0d }),
                new TraceRetentionOptions());

            var root = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = "unsampled-42" }
            });

            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = root.Record.Correlation
            });

            Assert.False(root.Context.Sampled);
            Assert.False(child.Context.Sampled);
            Assert.True(root.TryComplete(TraceSpanStatus.Succeeded));
            Assert.True(child.TryComplete(TraceSpanStatus.Succeeded));
            Assert.Empty(recorder.GetSpans());
        }

        [Fact]
        public void InMemoryRecorder_RetainsOnlyConfiguredNumberOfTracesAndSpansPerTrace()
        {
            var retention = new TraceRetentionOptions
            {
                MaxTraceCount = 2,
                MaxSpanCount = 100,
                MaxSpansPerTrace = 2,
                MaxAggregateMetadataCharacters = 10000,
                MaxAge = Timeout.InfiniteTimeSpan
            };
            var recorder = new InMemoryTraceRecorder(new FixedTraceSampler(true), retention);

            var first = StartRoot(recorder, "trace-one");
            Complete(first);
            var second = StartRoot(recorder, "trace-two");
            Complete(second);
            var third = StartRoot(recorder, "trace-three");
            Complete(third);

            var spansAfterTraceLimit = recorder.GetSpans();
            Assert.Equal(2, CountDistinctTraceIds(spansAfterTraceLimit));
            Assert.DoesNotContain(spansAfterTraceLimit, x => x.Correlation.ExecutionId == "trace-one");

            var root = StartRoot(recorder, "trace-per-span");
            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "child.one",
                Kind = "Provider",
                Correlation = root.Record.Correlation
            });
            var rejected = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "child.two",
                Kind = "Tool",
                Correlation = root.Record.Correlation
            });
            Complete(root);
            Complete(child);
            Complete(rejected);

            var perTrace = recorder.GetSpans();
            Assert.Equal(2, CountForExecution(perTrace, "trace-per-span"));
            Assert.DoesNotContain(perTrace, x => x.OperationName == "child.two");
            Assert.True(rejected.IsCompleted);
        }

        [Fact]
        public void InMemoryRecorder_EnforcesSpanAndAggregateMetadataBoundsWithoutAffectingLifecycle()
        {
            var retention = new TraceRetentionOptions
            {
                MaxTraceCount = 10,
                MaxSpanCount = 3,
                MaxSpansPerTrace = 10,
                MaxAggregateMetadataCharacters = 20,
                MaxAge = Timeout.InfiniteTimeSpan
            };
            var recorder = new InMemoryTraceRecorder(new FixedTraceSampler(true), retention);

            var oversized = recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "oversized",
                Kind = "Diagnostic",
                Correlation = new TraceCorrelation { ExecutionId = "oversized-42" },
                Metadata = Metadata("01234567890123456789X")
            });

            var root = StartRoot(recorder, "bounded-42");
            var child1 = StartChild(recorder, root, "child-one");
            var child2 = StartChild(recorder, root, "child-two");
            var child3 = StartChild(recorder, root, "child-three");

            Complete(oversized);
            Complete(root);
            Complete(child1);
            Complete(child2);
            Complete(child3);

            Assert.True(oversized.IsCompleted);
            Assert.True(root.IsCompleted);
            Assert.True(child1.IsCompleted);
            Assert.True(child2.IsCompleted);
            Assert.True(child3.IsCompleted);

            var retained = recorder.GetSpans();
            Assert.True(retained.Count <= 3);
            Assert.DoesNotContain(retained, x => x.Correlation.ExecutionId == "oversized-42");
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

        private static ITraceSpan StartChild(ITraceRecorder recorder, ITraceSpan parent, string operationName)
        {
            return recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = parent.Context,
                OperationName = operationName,
                Kind = "Diagnostic",
                Correlation = parent.Record.Correlation
            });
        }

        private static TraceMetadata Metadata(string value)
        {
            var metadata = new TraceMetadata();
            metadata.Add("value", value);
            return metadata;
        }

        private static void Complete(ITraceSpan span)
        {
            span.TryComplete(TraceSpanStatus.Succeeded);
        }

        private static int CountDistinctTraceIds(IReadOnlyList<TraceSpan> spans)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var span in spans)
                ids.Add(span.TraceId);
            return ids.Count;
        }

        private static int CountForExecution(IReadOnlyList<TraceSpan> spans, string executionId)
        {
            var count = 0;
            foreach (var span in spans)
            {
                if (string.Equals(span.Correlation.ExecutionId, executionId, StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
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

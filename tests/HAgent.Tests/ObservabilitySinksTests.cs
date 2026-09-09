using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;
using Xunit;

namespace HAgent.Tests
{
    public sealed class ObservabilitySinksTests
    {
        [Fact]
        public async Task Recorder_DispatchesRetainedSampledCompletedSpansInQueueOrder()
        {
            var sink = new RecordingSink();
            var dispatcher = new TraceSinkDispatcher(new[] { sink });
            var recorder = new InMemoryTraceRecorder(null, null, dispatcher);

            var root = StartRoot(recorder, "sink-order-42");
            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = root.Record.Correlation
            });

            Assert.True(root.TryComplete(TraceSpanStatus.Succeeded));
            Assert.True(child.TryComplete(TraceSpanStatus.Succeeded));

            await dispatcher.FlushAsync().ConfigureAwait(false);

            Assert.Equal(2, sink.Spans.Count);
            Assert.Equal(root.Record.SpanId, sink.Spans[0].SpanId);
            Assert.Equal(child.Record.SpanId, sink.Spans[1].SpanId);
            Assert.Equal(2L, dispatcher.AcceptedCount);
            Assert.Equal(2L, dispatcher.PublishedCount);
            Assert.Equal(0L, dispatcher.FailedCount);
            Assert.Equal(0L, dispatcher.DroppedCount);
        }

        [Fact]
        public async Task SinkFailure_DoesNotAffectSpanCompletionOrOtherSinks()
        {
            var failing = new FailingSink();
            var recording = new RecordingSink();
            var dispatcher = new TraceSinkDispatcher(new ITraceSink[] { failing, recording });
            var recorder = new InMemoryTraceRecorder(null, null, dispatcher);
            var span = StartRoot(recorder, "sink-failure-42");

            Assert.True(span.TryComplete(TraceSpanStatus.Succeeded));
            Assert.True(span.Record.IsCompleted);

            await dispatcher.FlushAsync().ConfigureAwait(false);

            Assert.Single(recording.Spans);
            Assert.Equal(1L, dispatcher.FailedCount);
            Assert.Equal(1L, dispatcher.PublishedCount);
        }

        [Fact]
        public async Task SlowSink_DoesNotBlockSpanCompletionAndFlushWaitsForAcceptedWork()
        {
            var sink = new BlockingSink();
            var dispatcher = new TraceSinkDispatcher(new[] { sink });
            var recorder = new InMemoryTraceRecorder(null, null, dispatcher);
            var span = StartRoot(recorder, "sink-slow-42");

            Assert.True(span.TryComplete(TraceSpanStatus.Succeeded));
            Assert.True(span.Record.IsCompleted);

            await sink.Started.ConfigureAwait(false);
            var flush = dispatcher.FlushAsync();
            Assert.False(flush.IsCompleted);

            sink.Release();
            await flush.ConfigureAwait(false);

            Assert.Single(sink.Spans);
            Assert.Equal(1L, dispatcher.PublishedCount);
            Assert.Equal(0L, dispatcher.FailedCount);
        }

        [Fact]
        public async Task UnsampledOrNotRetainedSpans_AreNotExported()
        {
            var unsampledSink = new RecordingSink();
            var unsampledDispatcher = new TraceSinkDispatcher(new[] { unsampledSink });
            var unsampledRecorder = new InMemoryTraceRecorder(
                new FixedTraceSampler(false),
                new TraceRetentionOptions(),
                unsampledDispatcher);

            var unsampled = StartRoot(unsampledRecorder, "sink-unsampled-42");
            Assert.True(unsampled.TryComplete(TraceSpanStatus.Succeeded));
            await unsampledDispatcher.FlushAsync().ConfigureAwait(false);

            Assert.Empty(unsampledSink.Spans);
            Assert.Equal(0L, unsampledDispatcher.AcceptedCount);

            var retainedSink = new RecordingSink();
            var retainedDispatcher = new TraceSinkDispatcher(new[] { retainedSink });
            var retention = new TraceRetentionOptions
            {
                MaxTraceCount = 8,
                MaxSpanCount = 8,
                MaxSpansPerTrace = 1,
                MaxAggregateMetadataCharacters = 10000,
                MaxAge = Timeout.InfiniteTimeSpan
            };
            var retainedRecorder = new InMemoryTraceRecorder(new FixedTraceSampler(true), retention, retainedDispatcher);
            var root = StartRoot(retainedRecorder, "sink-not-retained-42");
            Assert.True(root.TryComplete(TraceSpanStatus.Succeeded));
            await retainedDispatcher.FlushAsync().ConfigureAwait(false);

            var rejectedChild = retainedRecorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "tool.execute",
                Kind = "Tool",
                Correlation = root.Record.Correlation
            });
            Assert.True(rejectedChild.TryComplete(TraceSpanStatus.Succeeded));
            await retainedDispatcher.FlushAsync().ConfigureAwait(false);

            Assert.Single(retainedSink.Spans);
            Assert.Equal(root.Record.SpanId, retainedSink.Spans[0].SpanId);
            Assert.Equal(1L, retainedDispatcher.AcceptedCount);

            unsampledDispatcher.Dispose();
            retainedDispatcher.Dispose();
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

        private sealed class RecordingSink : ITraceSink
        {
            private readonly object _sync = new object();
            private readonly List<TraceSpan> _spans = new List<TraceSpan>();

            public IReadOnlyList<TraceSpan> Spans
            {
                get
                {
                    lock (_sync)
                        return new List<TraceSpan>(_spans).AsReadOnly();
                }
            }

            public Task PublishAsync(TraceSpan span, CancellationToken cancellationToken)
            {
                lock (_sync)
                    _spans.Add(span);
                return Task.CompletedTask;
            }
        }

        private sealed class FailingSink : ITraceSink
        {
            public Task PublishAsync(TraceSpan span, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Synthetic sink rejection.");
            }
        }

        private sealed class BlockingSink : ITraceSink
        {
            private readonly TaskCompletionSource<bool> _started =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource<bool> _release =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly RecordingSink _recording = new RecordingSink();

            public Task Started { get { return _started.Task; } }
            public IReadOnlyList<TraceSpan> Spans { get { return _recording.Spans; } }

            public Task PublishAsync(TraceSpan span, CancellationToken cancellationToken)
            {
                _started.TrySetResult(true);
                return PublishCoreAsync(span, cancellationToken);
            }

            public void Release()
            {
                _release.TrySetResult(true);
            }

            private async Task PublishCoreAsync(TraceSpan span, CancellationToken cancellationToken)
            {
                await _release.Task.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                await _recording.PublishAsync(span, cancellationToken).ConfigureAwait(false);
            }
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

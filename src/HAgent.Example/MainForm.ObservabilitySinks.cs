using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private void AddObservabilitySinksTab()
        {
            AddApiTab(
                "Observability Sinks",
                "Run trace sink test",
                "Runs deterministic trace sink and safe export-boundary checks through the public tracing APIs.",
                "The scenario verifies retained sampled spans are delivered in completion order, sink failures are isolated, slow sink work does not block span completion, and unsampled spans are not exported.",
                "No network provider or remote telemetry service is contacted. Sink delivery is bounded in-process work only.",
                RunObservabilitySinksTest,
                "Trace sinks",
                "This Slice 5 scenario defines the provider-neutral export boundary; remote transport, durable storage, and diagnostic UI remain later work.");
        }

        private async Task RunObservabilitySinksTest(string unused)
        {
            var recordingSink = new ExampleRecordingTraceSink();
            var dispatcher = new TraceSinkDispatcher(new ITraceSink[] { recordingSink });
            var recorder = new InMemoryTraceRecorder(null, null, dispatcher);

            var root = StartExampleRoot(recorder, "example-sink-order-42");
            var child = recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = root.Context,
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = root.Record.Correlation
            });

            if (!root.TryComplete(TraceSpanStatus.Succeeded) || !child.TryComplete(TraceSpanStatus.Succeeded))
                throw new InvalidOperationException("Sampled trace spans did not complete successfully.");

            await dispatcher.FlushAsync().ConfigureAwait(true);

            if (recordingSink.Spans.Count != 2 ||
                recordingSink.Spans[0].SpanId != root.Record.SpanId ||
                recordingSink.Spans[1].SpanId != child.Record.SpanId)
                throw new InvalidOperationException("Trace sink ordering was not preserved.");

            var failingSink = new ExampleFailingTraceSink();
            var secondarySink = new ExampleRecordingTraceSink();
            var failureDispatcher = new TraceSinkDispatcher(new ITraceSink[] { failingSink, secondarySink });
            var failureRecorder = new InMemoryTraceRecorder(null, null, failureDispatcher);
            var failureSpan = StartExampleRoot(failureRecorder, "example-sink-failure-42");

            if (!failureSpan.TryComplete(TraceSpanStatus.Succeeded) || !failureSpan.Record.IsCompleted)
                throw new InvalidOperationException("Sink failure changed span lifecycle completion.");

            await failureDispatcher.FlushAsync().ConfigureAwait(true);
            if (failureDispatcher.FailedCount != 1 || secondarySink.Spans.Count != 1)
                throw new InvalidOperationException("Sink failure was not isolated from the remaining sink.");

            var blockingSink = new ExampleBlockingTraceSink();
            var slowDispatcher = new TraceSinkDispatcher(new[] { (ITraceSink)blockingSink });
            var slowRecorder = new InMemoryTraceRecorder(null, null, slowDispatcher);
            var slowSpan = StartExampleRoot(slowRecorder, "example-sink-slow-42");
            if (!slowSpan.TryComplete(TraceSpanStatus.Succeeded) || !slowSpan.Record.IsCompleted)
                throw new InvalidOperationException("Slow sink affected span completion.");

            await blockingSink.Started.ConfigureAwait(true);
            var pendingFlush = slowDispatcher.FlushAsync();
            if (pendingFlush.IsCompleted)
                throw new InvalidOperationException("Slow sink work was not retained as pending dispatcher work.");

            blockingSink.Release();
            await pendingFlush.ConfigureAwait(true);

            var unsampledSink = new ExampleRecordingTraceSink();
            var unsampledDispatcher = new TraceSinkDispatcher(new[] { (ITraceSink)unsampledSink });
            var unsampledRecorder = new InMemoryTraceRecorder(
                new FixedTraceSampler(false),
                new TraceRetentionOptions(),
                unsampledDispatcher);
            var unsampled = StartExampleRoot(unsampledRecorder, "example-sink-unsampled-42");
            if (!unsampled.TryComplete(TraceSpanStatus.Succeeded))
                throw new InvalidOperationException("Unsampled span could not complete normally.");
            await unsampledDispatcher.FlushAsync().ConfigureAwait(true);
            if (unsampledSink.Spans.Count != 0 || unsampledDispatcher.AcceptedCount != 0)
                throw new InvalidOperationException("Unsampled span crossed the sink boundary.");

            dispatcher.Dispose();
            failureDispatcher.Dispose();
            slowDispatcher.Dispose();
            unsampledDispatcher.Dispose();

            Write(
                "OBSERVABILITY SINKS",
                "Integrated trace sink and safe export boundary succeeded." + Environment.NewLine +
                "Retained sampled spans delivered in completion order: verified." + Environment.NewLine +
                "Sink failure isolated from span completion and other sinks: verified." + Environment.NewLine +
                "Slow sink does not block span completion: verified." + Environment.NewLine +
                "Dispatcher waits for accepted asynchronous sink work: verified." + Environment.NewLine +
                "Unsampled spans suppressed at export boundary: verified." + Environment.NewLine +
                "Bounded in-process queue: verified by dispatcher contract." + Environment.NewLine +
                "Remote telemetry transport: none." + Environment.NewLine +
                "Durable trace storage: none." + Environment.NewLine +
                "Real provider request: none.");
        }

        private static ITraceSpan StartExampleRoot(ITraceRecorder recorder, string executionId)
        {
            return recorder.StartSpan(new TraceSpanStartOptions
            {
                OperationName = "execution",
                Kind = "Execution",
                Correlation = new TraceCorrelation { ExecutionId = executionId }
            });
        }

        private sealed class ExampleRecordingTraceSink : ITraceSink
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

        private sealed class ExampleFailingTraceSink : ITraceSink
        {
            public Task PublishAsync(TraceSpan span, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Synthetic sink rejection.");
            }
        }

        private sealed class ExampleBlockingTraceSink : ITraceSink
        {
            private readonly TaskCompletionSource<bool> _started =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource<bool> _release =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly ExampleRecordingTraceSink _recording = new ExampleRecordingTraceSink();

            public Task Started { get { return _started.Task; } }

            public void Release()
            {
                _release.TrySetResult(true);
            }

            public Task PublishAsync(TraceSpan span, CancellationToken cancellationToken)
            {
                _started.TrySetResult(true);
                return PublishCoreAsync(span, cancellationToken);
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

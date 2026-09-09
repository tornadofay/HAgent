using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Asynchronous, bounded delivery boundary for completed trace spans.
    /// Queue saturation and sink failures are telemetry concerns and never propagate to the caller.
    /// </summary>
    public sealed class TraceSinkDispatcher
    {
        private readonly BlockingCollection<TraceSpan> _queue;
        private readonly IReadOnlyList<ITraceSink> _sinks;
        private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
        private readonly object _flushSync = new object();
        private TaskCompletionSource<bool> _idle = CompletedSource();
        private long _pending;
        private long _accepted;
        private long _dropped;
        private long _published;
        private long _failed;
        private bool _disposed;

        public TraceSinkDispatcher(IEnumerable<ITraceSink> sinks, TraceSinkOptions options = null)
        {
            if (sinks == null) throw new ArgumentNullException(nameof(sinks));

            var sinkList = new List<ITraceSink>();
            foreach (var sink in sinks)
            {
                if (sink == null) throw new ArgumentException("Trace sink collection cannot contain null entries.", nameof(sinks));
                sinkList.Add(sink);
            }

            if (sinkList.Count == 0)
                throw new ArgumentException("At least one trace sink is required.", nameof(sinks));

            var effectiveOptions = options == null ? new TraceSinkOptions() : options.Clone();
            effectiveOptions.Validate();

            _sinks = sinkList.AsReadOnly();
            _queue = new BlockingCollection<TraceSpan>(effectiveOptions.MaxPendingSpans);
            _ = Task.Factory.StartNew(
                DrainAsync,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default).Unwrap();
        }

        public long AcceptedCount { get { return Interlocked.Read(ref _accepted); } }
        public long DroppedCount { get { return Interlocked.Read(ref _dropped); } }
        public long PublishedCount { get { return Interlocked.Read(ref _published); } }
        public long FailedCount { get { return Interlocked.Read(ref _failed); } }

        /// <summary>
        /// Enqueues a completed sampled span without waiting for sink work.
        /// Returns false when the dispatcher is stopped or its bounded queue is full.
        /// </summary>
        public bool TryEnqueue(TraceSpan span)
        {
            if (span == null) throw new ArgumentNullException(nameof(span));

            lock (_flushSync)
            {
                if (_disposed)
                    return false;

                if (_pending == 0)
                    _idle = NewSource();

                _pending++;
                if (_queue.TryAdd(span))
                {
                    Interlocked.Increment(ref _accepted);
                    return true;
                }

                _pending--;
                Interlocked.Increment(ref _dropped);
                CompleteIdleIfNeededUnsafe();
                return false;
            }
        }

        /// <summary>
        /// Waits until all spans currently accepted by the dispatcher finish sink processing.
        /// </summary>
        public Task FlushAsync()
        {
            lock (_flushSync)
            {
                return _idle.Task;
            }
        }

        /// <summary>
        /// Stops new work and cancels the dispatcher worker. Span lifecycle state is unaffected.
        /// </summary>
        public void Dispose()
        {
            lock (_flushSync)
            {
                if (_disposed)
                    return;
                _disposed = true;
                _queue.CompleteAdding();
            }

            _shutdown.Cancel();
        }

        private async Task DrainAsync()
        {
            try
            {
                while (!_shutdown.IsCancellationRequested)
                {
                    TraceSpan span;
                    try
                    {
                        span = _queue.Take(_shutdown.Token);
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    foreach (var sink in _sinks)
                    {
                        try
                        {
                            await sink.PublishAsync(span, _shutdown.Token).ConfigureAwait(false);
                            Interlocked.Increment(ref _published);
                        }
                        catch (OperationCanceledException)
                        {
                            if (!_shutdown.IsCancellationRequested)
                                Interlocked.Increment(ref _failed);
                        }
                        catch (Exception)
                        {
                            Interlocked.Increment(ref _failed);
                        }
                    }

                    lock (_flushSync)
                    {
                        _pending--;
                        CompleteIdleIfNeededUnsafe();
                    }
                }
            }
            finally
            {
                lock (_flushSync)
                {
                    _pending = 0;
                    CompleteIdleIfNeededUnsafe();
                }
            }
        }

        private void CompleteIdleIfNeededUnsafe()
        {
            if (_pending == 0)
                _idle.TrySetResult(true);
        }

        private static TaskCompletionSource<bool> NewSource()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private static TaskCompletionSource<bool> CompletedSource()
        {
            var source = NewSource();
            source.TrySetResult(true);
            return source;
        }
    }
}

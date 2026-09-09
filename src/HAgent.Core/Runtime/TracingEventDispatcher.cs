using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Event-dispatcher decorator that traces publication and handler execution without
    /// copying event payload/context into trace metadata.
    /// </summary>
    public sealed class TracingEventDispatcher : IEventDispatcher
    {
        private readonly IEventDispatcher _inner;
        private readonly ITraceRecorder _recorder;

        public TracingEventDispatcher(IEventDispatcher inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        public async Task<EventPublishResult> PublishAsync(
            EventEnvelope envelope,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            var metadata = new TraceMetadata();
            metadata.Add("event.type", envelope.Type ?? string.Empty);
            metadata.Add("event.source", envelope.Source.ToString());
            if (!string.IsNullOrWhiteSpace(envelope.SourceId))
                metadata.Add("event.source.id", envelope.SourceId);
            if (!string.IsNullOrWhiteSpace(envelope.Id))
                metadata.Add("event.id", envelope.Id);
            if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
                metadata.Add("event.correlation", envelope.CorrelationId);
            if (!string.IsNullOrWhiteSpace(envelope.CausationId))
                metadata.Add("event.causation", envelope.CausationId);
            metadata.AddOmitted("event.payload");
            metadata.AddOmitted("event.context");

            var parent = envelope.TraceContext ?? TraceAmbient.Current;
            var correlation = TraceAmbient.CurrentCorrelation ?? new TraceCorrelation();
            if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
                correlation.EventId = envelope.CorrelationId;
            if (!string.IsNullOrWhiteSpace(envelope.CausationId))
                correlation.CausationId = envelope.CausationId;

            var span = _recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = parent,
                OperationName = "event.publish",
                Kind = "Event",
                Correlation = correlation,
                Metadata = metadata
            });

            var tracedEnvelope = envelope.Clone();
            tracedEnvelope.TraceContext = span.Context;

            using (TraceAmbient.Push(span.Context, span.Record.Correlation))
            {
                try
                {
                    var result = await _inner.PublishAsync(tracedEnvelope, cancellationToken).ConfigureAwait(false);
                    span.TryComplete(result == null || result.Accepted
                        ? TraceSpanStatus.Succeeded
                        : TraceSpanStatus.Rejected);
                    return result;
                }
                catch (OperationCanceledException)
                {
                    span.TryComplete(cancellationToken.IsCancellationRequested
                        ? TraceSpanStatus.Cancelled
                        : TraceSpanStatus.Timeout);
                    throw;
                }
                catch
                {
                    span.TryComplete(TraceSpanStatus.Failed);
                    throw;
                }
            }
        }

        public IEventSubscription Subscribe(EventSubscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (subscription.Handler == null) throw new ArgumentException("Event subscription handler is required.", nameof(subscription));

            var originalHandler = subscription.Handler;
            var wrapped = new EventSubscription
            {
                Id = subscription.Id,
                Filter = subscription.Filter,
                Handler = async (envelope, cancellationToken) =>
                {
                    var metadata = new TraceMetadata();
                    metadata.Add("event.type", envelope == null ? string.Empty : envelope.Type ?? string.Empty);
                    metadata.Add("event.source", envelope == null ? string.Empty : envelope.Source.ToString());
                    if (envelope != null && !string.IsNullOrWhiteSpace(envelope.Id))
                        metadata.Add("event.id", envelope.Id);
                    metadata.AddOmitted("event.payload");

                    var parent = envelope == null ? TraceAmbient.Current : envelope.TraceContext ?? TraceAmbient.Current;
                    var correlation = TraceAmbient.CurrentCorrelation ?? new TraceCorrelation();
                    if (envelope != null)
                    {
                        if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
                            correlation.EventId = envelope.CorrelationId;
                        if (!string.IsNullOrWhiteSpace(envelope.CausationId))
                            correlation.CausationId = envelope.CausationId;
                    }

                    var span = _recorder.StartSpan(new TraceSpanStartOptions
                    {
                        ParentContext = parent,
                        OperationName = "event.handle",
                        Kind = "Event",
                        Correlation = correlation,
                        Metadata = metadata
                    });

                    using (TraceAmbient.Push(span.Context, span.Record.Correlation))
                    {
                        try
                        {
                            await originalHandler(envelope, cancellationToken).ConfigureAwait(false);
                            span.TryComplete(TraceSpanStatus.Succeeded);
                        }
                        catch (OperationCanceledException)
                        {
                            span.TryComplete(cancellationToken.IsCancellationRequested
                                ? TraceSpanStatus.Cancelled
                                : TraceSpanStatus.Timeout);
                            throw;
                        }
                        catch
                        {
                            span.TryComplete(TraceSpanStatus.Failed);
                            throw;
                        }
                    }
                }
            };

            return _inner.Subscribe(wrapped);
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}

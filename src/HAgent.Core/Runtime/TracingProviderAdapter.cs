using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Provider adapter decorator that records provider operation boundaries without changing
    /// the provider-neutral adapter contract or transporting trace state to a vendor payload.
    /// </summary>
    public sealed class TracingProviderAdapter : IAiProviderAdapter
    {
        private sealed class AttemptState
        {
            public string TraceId;
            public int Attempt;
        }

        private readonly IAiProviderAdapter _inner;
        private readonly ITraceRecorder _recorder;
        private static readonly AsyncLocal<AttemptState> CurrentAttempt = new AsyncLocal<AttemptState>();

        public TracingProviderAdapter(IAiProviderAdapter inner, ITraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        public string Kind { get { return _inner.Kind; } }
        public string DisplayName { get { return _inner.DisplayName; } }

        public bool CanHandle(AiProvider provider)
        {
            return _inner.CanHandle(provider);
        }

        public async Task<AIResponse> SendAsync(
            ProviderExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var parent = TraceAmbient.Current;
            var correlation = TraceAmbient.CurrentCorrelation ?? new TraceCorrelation();
            var attempt = GetAttempt(parent == null ? string.Empty : parent.TraceId);
            if (attempt > 1)
            {
                var retryMetadata = new TraceMetadata();
                retryMetadata.Add("decision", "retry");
                retryMetadata.Add("execution.attempt", attempt.ToString());
                if (request.Provider != null)
                    retryMetadata.Add("provider.id", request.Provider.Id ?? string.Empty);
                TraceObservation.RecordDecision(
                    "provider.retry",
                    "Provider",
                    TraceSpanStatus.Succeeded,
                    retryMetadata,
                    correlation);
            }

            var metadata = new TraceMetadata();
            metadata.Add("provider.kind", _inner.Kind ?? string.Empty);
            metadata.Add("provider.display", _inner.DisplayName ?? string.Empty);
            metadata.Add("execution.attempt", attempt.ToString());
            if (request.Provider != null)
                metadata.Add("provider.id", request.Provider.Id ?? string.Empty);
            if (request.ExecutionTarget != null)
                metadata.Add("execution.target", request.ExecutionTarget.Id ?? string.Empty);

            var span = _recorder.StartSpan(new TraceSpanStartOptions
            {
                ParentContext = parent,
                OperationName = "provider.invoke",
                Kind = "Provider",
                Correlation = correlation,
                Metadata = metadata
            });

            using (TraceAmbient.Push(span.Context, correlation))
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var response = await _inner.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    span.TryComplete(TraceSpanStatus.Succeeded);
                    if (attempt > 1)
                    {
                        var recoveryMetadata = new TraceMetadata();
                        recoveryMetadata.Add("decision", "recovery");
                        recoveryMetadata.Add("execution.attempt", attempt.ToString());
                        if (request.Provider != null)
                            recoveryMetadata.Add("provider.id", request.Provider.Id ?? string.Empty);
                        TraceObservation.RecordDecision(
                            "provider.recovery",
                            "Outcome",
                            TraceSpanStatus.Succeeded,
                            recoveryMetadata,
                            correlation);
                    }
                    CurrentAttempt.Value = null;
                    return response;
                }
                catch (OperationCanceledException)
                {
                    span.TryComplete(cancellationToken.IsCancellationRequested
                        ? TraceSpanStatus.Cancelled
                        : TraceSpanStatus.Timeout);
                    throw;
                }
                catch (Exception)
                {
                    span.TryComplete(TraceSpanStatus.Failed);
                    throw;
                }
            }
        }

        private static int GetAttempt(string traceId)
        {
            var current = CurrentAttempt.Value;
            if (current == null || !string.Equals(current.TraceId, traceId, StringComparison.Ordinal))
            {
                CurrentAttempt.Value = new AttemptState { TraceId = traceId, Attempt = 1 };
                return 1;
            }

            current.Attempt++;
            return current.Attempt;
        }
    }
}

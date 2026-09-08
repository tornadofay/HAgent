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
        private readonly IAiProviderAdapter _inner;
        private readonly ITraceRecorder _recorder;

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
            var metadata = new TraceMetadata();
            metadata.Add("provider.kind", _inner.Kind ?? string.Empty);
            metadata.Add("provider.display", _inner.DisplayName ?? string.Empty);
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
    }
}

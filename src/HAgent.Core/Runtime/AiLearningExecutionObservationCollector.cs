using System;
using System.Threading;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Adapts authoritative runtime observations into the learning observation store.
    /// It does not create candidates, promote resources, or alter runtime decisions.
    /// </summary>
    public sealed class AiLearningExecutionObservationCollector : IDisposable
    {
        private readonly IExecutionObservationSource _source;
        private readonly IAiLearningObservationStore _store;
        private bool _disposed;

        public AiLearningExecutionObservationCollector(
            IExecutionObservationSource source,
            IAiLearningObservationStore store)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _source.ExecutionObserved += OnExecutionObserved;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _source.ExecutionObserved -= OnExecutionObserved;
        }

        private void OnExecutionObserved(object sender, AgentExecutionObservationEventArgs args)
        {
            if (_disposed || args == null || args.Observation == null)
                return;

            var source = args.Observation;
            var learningObservation = new AiLearningExecutionObservation
            {
                ExecutionId = source.ExecutionId,
                Kind = source.Kind,
                ProviderId = source.ProviderId,
                PreviousProviderId = source.PreviousProviderId,
                Attempt = source.Attempt,
                RetryNumber = source.RetryNumber,
                Reason = source.Reason,
                WaitDuration = source.WaitDuration,
                CapturedAt = source.OccurredAt
            };

            learningObservation.Validate();
            _store.AppendAsync(learningObservation, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}

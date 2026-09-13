using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public static class AiRuntimeObservationPublisher
    {
        public static Task<EventPublishResult> PublishAsync(IEventDispatcher dispatcher, AiRuntimeObservation observation, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (observation == null) throw new ArgumentNullException(nameof(observation));

            var envelope = new EventEnvelope
            {
                Type = "runtime." + ToEventType(observation.Kind),
                Source = EventSource.Runtime,
                SourceId = observation.RuntimeInstanceId,
                OccurredAt = observation.ObservedAt,
                Importance = EventImportance.Normal,
                Scope = EventScope.Runtime,
                ScopeId = observation.RuntimeInstanceId,
                PayloadJson = "{\"kind\":\"" + observation.Kind + "\",\"state\":\"" + observation.LifecycleState + "\",\"health\":\"" + (observation.Health == null ? AiRuntimeHealthStatus.Unknown.ToString() : observation.Health.Status.ToString()) + "\"}",
                Context = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
            envelope.Context["runtimeInstanceId"] = observation.RuntimeInstanceId;
            envelope.Context["profileId"] = observation.ProfileId;
            envelope.Context["lifecycleRevision"] = observation.LifecycleRevision.ToString(System.Globalization.CultureInfo.InvariantCulture);
            envelope.Validate();
            return dispatcher.PublishAsync(envelope, cancellationToken);
        }

        private static string ToEventType(AiRuntimeObservationKind kind)
        {
            switch (kind)
            {
                case AiRuntimeObservationKind.LifecycleChanged: return "lifecycle.changed";
                case AiRuntimeObservationKind.HealthChanged: return "health.changed";
                case AiRuntimeObservationKind.ProgressChanged: return "progress.changed";
                case AiRuntimeObservationKind.RecoveryCompleted: return "recovery.completed";
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}

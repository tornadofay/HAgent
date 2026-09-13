using System;

namespace HAgent.Models
{
    public enum AiRuntimeHealthStatus
    {
        Unknown = 0,
        Healthy = 1,
        Degraded = 2,
        Failed = 3
    }

    public enum AiRuntimeHealthSource
    {
        RuntimeObservation = 0,
        RecoveryResult = 1,
        HostSignal = 2,
        ExternalEvidence = 3
    }

    public enum AiRuntimeHealthFailureKind
    {
        None = 0,
        Transient = 1,
        Terminal = 2
    }

    public sealed class AiRuntimeHealth
    {
        public const int MaxReasonLength = 512;
        public const int MaxEvidenceLength = 2048;

        public AiRuntimeHealth(
            AiRuntimeHealthStatus status,
            AiRuntimeHealthSource source,
            AiRuntimeHealthFailureKind failureKind,
            string reason,
            string evidence,
            DateTimeOffset? observedAt)
        {
            Status = status;
            Source = source;
            FailureKind = failureKind;
            Reason = Normalize(reason);
            Evidence = Normalize(evidence);
            ObservedAt = observedAt;
        }

        public AiRuntimeHealthStatus Status { get; private set; }
        public AiRuntimeHealthSource Source { get; private set; }
        public AiRuntimeHealthFailureKind FailureKind { get; private set; }
        public string Reason { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset? ObservedAt { get; private set; }

        public static AiRuntimeHealth CreateUnknown()
        {
            return new AiRuntimeHealth(
                AiRuntimeHealthStatus.Unknown,
                AiRuntimeHealthSource.RuntimeObservation,
                AiRuntimeHealthFailureKind.None,
                "No health observation has been recorded.",
                string.Empty,
                null);
        }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiRuntimeHealthStatus), Status))
                throw new ArgumentException("Runtime health status is not supported.", nameof(Status));
            if (!Enum.IsDefined(typeof(AiRuntimeHealthSource), Source))
                throw new ArgumentException("Runtime health source is not supported.", nameof(Source));
            if (!Enum.IsDefined(typeof(AiRuntimeHealthFailureKind), FailureKind))
                throw new ArgumentException("Runtime health failure kind is not supported.", nameof(FailureKind));
            if (Reason.Length > MaxReasonLength)
                throw new ArgumentException("Runtime health reason exceeds the configured bound.", nameof(Reason));
            if (Evidence.Length > MaxEvidenceLength)
                throw new ArgumentException("Runtime health evidence exceeds the configured bound.", nameof(Evidence));

            switch (Status)
            {
                case AiRuntimeHealthStatus.Unknown:
                    if (FailureKind != AiRuntimeHealthFailureKind.None)
                        throw new ArgumentException("Unknown runtime health cannot declare a failure kind.", nameof(FailureKind));
                    break;

                case AiRuntimeHealthStatus.Healthy:
                    if (FailureKind != AiRuntimeHealthFailureKind.None)
                        throw new ArgumentException("Healthy runtime health cannot declare a failure kind.", nameof(FailureKind));
                    break;

                case AiRuntimeHealthStatus.Degraded:
                    if (FailureKind != AiRuntimeHealthFailureKind.Transient)
                        throw new ArgumentException("Degraded runtime health must be transient.", nameof(FailureKind));
                    if (string.IsNullOrWhiteSpace(Reason))
                        throw new ArgumentException("Degraded runtime health requires a reason.", nameof(Reason));
                    break;

                case AiRuntimeHealthStatus.Failed:
                    if (FailureKind != AiRuntimeHealthFailureKind.Terminal)
                        throw new ArgumentException("Failed runtime health must be terminal.", nameof(FailureKind));
                    if (string.IsNullOrWhiteSpace(Reason))
                        throw new ArgumentException("Failed runtime health requires a reason.", nameof(Reason));
                    break;

                default:
                    throw new ArgumentException("Runtime health status is not supported.", nameof(Status));
            }

            if (ObservedAt.HasValue && ObservedAt.Value == default(DateTimeOffset))
                throw new ArgumentException("Runtime health observation timestamp is invalid.", nameof(ObservedAt));
        }

        public AiRuntimeHealth Clone()
        {
            return new AiRuntimeHealth(Status, Source, FailureKind, Reason, Evidence, ObservedAt);
        }

        private static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}

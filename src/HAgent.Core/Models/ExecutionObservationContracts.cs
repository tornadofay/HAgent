using System;

namespace HAgent.Models
{
    /// <summary>
    /// Provider-neutral execution observation categories emitted by an execution runtime.
    /// These describe facts already decided by the runtime; they do not change execution behavior.
    /// </summary>
    public static class ExecutionObservationKinds
    {
        public const string ProviderRetry = "provider.retry";
        public const string ExecutionWait = "execution.wait";
        public const string ProviderRecovery = "provider.recovery";
        public const string ProviderFallback = "provider.fallback";
        public const string ExecutionStaleResult = "execution.stale-result";
    }

    /// <summary>
    /// Immutable, bounded diagnostic fact emitted by an execution runtime.
    /// It deliberately carries execution facts rather than trace state.
    /// </summary>
    public sealed class AgentExecutionObservation
    {
        public AgentExecutionObservation(
            string executionId,
            string kind,
            string providerId = null,
            string previousProviderId = null,
            int attempt = 0,
            int retryNumber = 0,
            string reason = null,
            TimeSpan? waitDuration = null,
            DateTimeOffset? occurredAt = null)
        {
            ExecutionId = ValidateText(executionId, "executionId", 128, true);
            Kind = ValidateText(kind, "kind", 128, true);
            ProviderId = ValidateText(providerId, "providerId", 128, false);
            PreviousProviderId = ValidateText(previousProviderId, "previousProviderId", 128, false);
            if (attempt < 0) throw new ArgumentOutOfRangeException(nameof(attempt));
            if (retryNumber < 0) throw new ArgumentOutOfRangeException(nameof(retryNumber));
            if (waitDuration.HasValue && waitDuration.Value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(waitDuration));
            Attempt = attempt;
            RetryNumber = retryNumber;
            Reason = ValidateText(reason, "reason", 512, false);
            WaitDuration = waitDuration;
            OccurredAt = occurredAt ?? DateTimeOffset.UtcNow;
        }

        public string ExecutionId { get; private set; }
        public string Kind { get; private set; }
        public string ProviderId { get; private set; }
        public string PreviousProviderId { get; private set; }
        public int Attempt { get; private set; }
        public int RetryNumber { get; private set; }
        public string Reason { get; private set; }
        public TimeSpan? WaitDuration { get; private set; }
        public DateTimeOffset OccurredAt { get; private set; }

        private static string ValidateText(string value, string parameterName, int maxLength, bool required)
        {
            value = value == null ? string.Empty : value.Trim();
            if (required && value.Length == 0)
                throw new ArgumentException("A value is required.", parameterName);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(parameterName, "Value exceeds the maximum length of " + maxLength + ".");
            return value;
        }
    }

    public sealed class AgentExecutionObservationEventArgs : EventArgs
    {
        public AgentExecutionObservationEventArgs(AgentExecutionObservation observation)
        {
            Observation = observation ?? throw new ArgumentNullException(nameof(observation));
        }

        public AgentExecutionObservation Observation { get; private set; }
    }
}

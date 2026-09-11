using System;

namespace HAgent.Models
{
    /// <summary>
    /// Bounded, provider-neutral execution observation retained as input to later learning analysis.
    /// It mirrors authoritative runtime facts without copying prompts, responses, or raw payloads.
    /// </summary>
    public sealed class AiLearningExecutionObservation
    {
        public AiLearningExecutionObservation()
        {
            ExecutionId = string.Empty;
            Kind = string.Empty;
            ProviderId = string.Empty;
            PreviousProviderId = string.Empty;
            Reason = string.Empty;
            CapturedAt = DateTimeOffset.UtcNow;
        }

        public string ExecutionId { get; set; }
        public string Kind { get; set; }
        public string ProviderId { get; set; }
        public string PreviousProviderId { get; set; }
        public int Attempt { get; set; }
        public int RetryNumber { get; set; }
        public string Reason { get; set; }
        public TimeSpan? WaitDuration { get; set; }
        public DateTimeOffset CapturedAt { get; set; }

        public AiLearningExecutionObservation Clone()
        {
            return new AiLearningExecutionObservation
            {
                ExecutionId = ExecutionId,
                Kind = Kind,
                ProviderId = ProviderId,
                PreviousProviderId = PreviousProviderId,
                Attempt = Attempt,
                RetryNumber = RetryNumber,
                Reason = Reason,
                WaitDuration = WaitDuration,
                CapturedAt = CapturedAt
            };
        }

        public void Validate()
        {
            ValidateText(ExecutionId, nameof(ExecutionId), 128, true);
            ValidateText(Kind, nameof(Kind), 128, true);
            ValidateText(ProviderId, nameof(ProviderId), 128, false);
            ValidateText(PreviousProviderId, nameof(PreviousProviderId), 128, false);
            ValidateText(Reason, nameof(Reason), 512, false);
            if (Attempt < 0) throw new ArgumentOutOfRangeException(nameof(Attempt));
            if (RetryNumber < 0) throw new ArgumentOutOfRangeException(nameof(RetryNumber));
            if (WaitDuration.HasValue && WaitDuration.Value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(WaitDuration));
            if (CapturedAt == default(DateTimeOffset))
                throw new ArgumentException("Observation capture time is required.", nameof(CapturedAt));
        }

        private static void ValidateText(string value, string name, int maximum, bool required)
        {
            value = value ?? string.Empty;
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A value is required.", name);
            if (value.Length > maximum)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiLearningObservationQuery
    {
        public AiLearningObservationQuery()
        {
            ExecutionId = string.Empty;
            Kind = string.Empty;
            MaxResults = 100;
        }

        public string ExecutionId { get; set; }
        public string Kind { get; set; }
        public int MaxResults { get; set; }

        public void Validate()
        {
            if (ExecutionId != null && ExecutionId.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(ExecutionId));
            if (Kind != null && Kind.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(Kind));
            if (MaxResults <= 0 || MaxResults > 1000)
                throw new ArgumentOutOfRangeException(nameof(MaxResults));
        }
    }
}

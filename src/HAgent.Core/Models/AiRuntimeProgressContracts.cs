using System;

namespace HAgent.Models
{
    public enum AiRuntimeProgressKind
    {
        Progress = 0,
        Heartbeat = 1
    }

    public enum AiRuntimeRecoveryStatus
    {
        Succeeded = 0,
        Failed = 1,
        Cancelled = 2
    }

    public sealed class AiRuntimeProgressSnapshot
    {
        public const int MaxDetailLength = 512;
        public const int MaxEvidenceLength = 1024;

        public AiRuntimeProgressSnapshot(long sequence, AiRuntimeProgressKind kind, int? percentComplete, string detail, string evidence, DateTimeOffset reportedAt)
        {
            Sequence = sequence;
            Kind = kind;
            PercentComplete = percentComplete;
            Detail = Normalize(detail);
            Evidence = Normalize(evidence);
            ReportedAt = reportedAt;
        }

        public long Sequence { get; private set; }
        public AiRuntimeProgressKind Kind { get; private set; }
        public int? PercentComplete { get; private set; }
        public string Detail { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset ReportedAt { get; private set; }

        public void Validate()
        {
            if (Sequence < 1L) throw new ArgumentOutOfRangeException(nameof(Sequence));
            if (!Enum.IsDefined(typeof(AiRuntimeProgressKind), Kind)) throw new ArgumentException("Runtime progress kind is not supported.", nameof(Kind));
            if (PercentComplete.HasValue && (PercentComplete.Value < 0 || PercentComplete.Value > 100)) throw new ArgumentOutOfRangeException(nameof(PercentComplete));
            if (Detail.Length > MaxDetailLength) throw new ArgumentException("Runtime progress detail exceeds the configured bound.", nameof(Detail));
            if (Evidence.Length > MaxEvidenceLength) throw new ArgumentException("Runtime progress evidence exceeds the configured bound.", nameof(Evidence));
            if (ReportedAt == default(DateTimeOffset)) throw new ArgumentException("Runtime progress timestamp is required.", nameof(ReportedAt));
        }

        public AiRuntimeProgressSnapshot Clone()
        {
            return new AiRuntimeProgressSnapshot(Sequence, Kind, PercentComplete, Detail, Evidence, ReportedAt);
        }

        private static string Normalize(string value) { return value == null ? string.Empty : value.Trim(); }
    }

    public sealed class AiRuntimeStallPolicy
    {
        public AiRuntimeStallPolicy(TimeSpan maxSilence) { MaxSilence = maxSilence; }
        public TimeSpan MaxSilence { get; private set; }

        public void Validate()
        {
            if (MaxSilence <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(MaxSilence));
            if (MaxSilence > TimeSpan.FromDays(7)) throw new ArgumentOutOfRangeException(nameof(MaxSilence), "Maximum stall silence cannot exceed 7 days.");
        }
    }

    public sealed class AiRuntimeStallAssessment
    {
        internal AiRuntimeStallAssessment(bool isStalled, string reason, DateTimeOffset evaluatedAt, AiRuntimeProgressSnapshot lastProgress)
        {
            IsStalled = isStalled;
            Reason = reason ?? string.Empty;
            EvaluatedAt = evaluatedAt;
            LastProgress = lastProgress == null ? null : lastProgress.Clone();
        }

        public bool IsStalled { get; private set; }
        public string Reason { get; private set; }
        public DateTimeOffset EvaluatedAt { get; private set; }
        public AiRuntimeProgressSnapshot LastProgress { get; private set; }
    }

    public static class AiRuntimeProgressMonitor
    {
        public static AiRuntimeStallAssessment Evaluate(AiRuntimeProgressSnapshot progress, AiRuntimeStallPolicy policy, DateTimeOffset evaluatedAt)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            policy.Validate();
            if (evaluatedAt == default(DateTimeOffset)) throw new ArgumentException("Stall evaluation timestamp is required.", nameof(evaluatedAt));
            if (progress == null) return new AiRuntimeStallAssessment(false, "No progress or heartbeat evidence has been recorded.", evaluatedAt, null);

            progress.Validate();
            var silence = evaluatedAt - progress.ReportedAt;
            if (silence < TimeSpan.Zero) throw new ArgumentException("Stall evaluation timestamp cannot precede the latest progress timestamp.", nameof(evaluatedAt));
            if (silence > policy.MaxSilence) return new AiRuntimeStallAssessment(true, "Configured progress/heartbeat silence threshold was exceeded.", evaluatedAt, progress);
            return new AiRuntimeStallAssessment(false, "Configured progress/heartbeat evidence is still current.", evaluatedAt, progress);
        }
    }

    public sealed class AiRuntimeRecoveryResult
    {
        public const int MaxReasonLength = 512;
        public const int MaxEvidenceLength = 1024;

        public AiRuntimeRecoveryResult(AiRuntimeRecoveryStatus status, string reason, string evidence, DateTimeOffset completedAt)
        {
            Status = status;
            Reason = Normalize(reason);
            Evidence = Normalize(evidence);
            CompletedAt = completedAt;
        }

        public AiRuntimeRecoveryStatus Status { get; private set; }
        public string Reason { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset CompletedAt { get; private set; }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AiRuntimeRecoveryStatus), Status)) throw new ArgumentException("Runtime recovery status is not supported.", nameof(Status));
            if (Reason.Length > MaxReasonLength) throw new ArgumentException("Runtime recovery reason exceeds the configured bound.", nameof(Reason));
            if (Evidence.Length > MaxEvidenceLength) throw new ArgumentException("Runtime recovery evidence exceeds the configured bound.", nameof(Evidence));
            if (CompletedAt == default(DateTimeOffset)) throw new ArgumentException("Runtime recovery completion timestamp is required.", nameof(CompletedAt));
            if (Status != AiRuntimeRecoveryStatus.Succeeded && string.IsNullOrWhiteSpace(Reason)) throw new ArgumentException("Failed or cancelled runtime recovery requires a reason.", nameof(Reason));
        }

        public AiRuntimeRecoveryResult Clone() { return new AiRuntimeRecoveryResult(Status, Reason, Evidence, CompletedAt); }
        private static string Normalize(string value) { return value == null ? string.Empty : value.Trim(); }
    }
}

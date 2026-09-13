using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiGoalStatus
    {
        Draft = 0,
        Active = 1,
        Suspended = 2,
        Achieved = 3,
        Failed = 4,
        Abandoned = 5,
        Superseded = 6
    }

    public enum AiIntentionStatus
    {
        Proposed = 0,
        Adopted = 1,
        Suspended = 2,
        Revised = 3,
        Completed = 4,
        Failed = 5,
        Abandoned = 6,
        Superseded = 7
    }

    public enum AiGoalPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public enum AiGoalAuthority
    {
        HostSupplied = 0,
        AgentInferred = 1
    }

    public sealed class AiGoalProvenance
    {
        public AiGoalAuthority Authority { get; set; }
        public string Source { get; set; }
        public string SourceExecutionId { get; set; }
        public string SourceRuntimeInstanceId { get; set; }
        public string Evidence { get; set; }

        public AiGoalProvenance()
        {
            Source = string.Empty;
            SourceExecutionId = string.Empty;
            SourceRuntimeInstanceId = string.Empty;
            Evidence = string.Empty;
        }

        public AiGoalProvenance Clone()
        {
            return new AiGoalProvenance
            {
                Authority = Authority,
                Source = Source,
                SourceExecutionId = SourceExecutionId,
                SourceRuntimeInstanceId = SourceRuntimeInstanceId,
                Evidence = Evidence
            };
        }
    }

    public sealed class AiGoal
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AiGoalStatus Status { get; set; }
        public AiGoalPriority Priority { get; set; }
        public IList<string> Constraints { get; private set; }
        public AiGoalProvenance Provenance { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public long Revision { get; set; }

        public AiGoal()
        {
            Id = Guid.NewGuid().ToString("N");
            Title = string.Empty;
            Description = string.Empty;
            Status = AiGoalStatus.Draft;
            Priority = AiGoalPriority.Normal;
            Constraints = new List<string>();
            Provenance = new AiGoalProvenance();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Revision = 0L;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("Goal ID is required.", nameof(Id));
            if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Goal title is required.", nameof(Title));
            if (!Enum.IsDefined(typeof(AiGoalStatus), Status)) throw new ArgumentException("Goal status is not supported.", nameof(Status));
            if (!Enum.IsDefined(typeof(AiGoalPriority), Priority)) throw new ArgumentException("Goal priority is not supported.", nameof(Priority));
            if (Provenance == null) throw new ArgumentException("Goal provenance is required.", nameof(Provenance));
            if (Revision < 0L) throw new ArgumentException("Goal revision cannot be negative.", nameof(Revision));
            if (CreatedAt == default(DateTimeOffset)) throw new ArgumentException("Goal creation timestamp is required.", nameof(CreatedAt));
            if (UpdatedAt == default(DateTimeOffset)) throw new ArgumentException("Goal update timestamp is required.", nameof(UpdatedAt));
        }

        public AiGoal Clone()
        {
            var clone = new AiGoal
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Status = Status,
                Priority = Priority,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Revision = Revision
            };
            foreach (var constraint in Constraints ?? new List<string>()) clone.Constraints.Add(constraint);
            return clone;
        }
    }

    public sealed class AiIntention
    {
        public string Id { get; set; }
        public string GoalId { get; set; }
        public string Description { get; set; }
        public AiIntentionStatus Status { get; set; }
        public AiGoalPriority Priority { get; set; }
        public IList<string> Constraints { get; private set; }
        public AiGoalProvenance Provenance { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? AdoptedAt { get; set; }
        public long Revision { get; set; }

        public AiIntention()
        {
            Id = Guid.NewGuid().ToString("N");
            GoalId = string.Empty;
            Description = string.Empty;
            Status = AiIntentionStatus.Proposed;
            Priority = AiGoalPriority.Normal;
            Constraints = new List<string>();
            Provenance = new AiGoalProvenance();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Revision = 0L;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("Intention ID is required.", nameof(Id));
            if (string.IsNullOrWhiteSpace(GoalId)) throw new ArgumentException("Intention goal ID is required.", nameof(GoalId));
            if (string.IsNullOrWhiteSpace(Description)) throw new ArgumentException("Intention description is required.", nameof(Description));
            if (!Enum.IsDefined(typeof(AiIntentionStatus), Status)) throw new ArgumentException("Intention status is not supported.", nameof(Status));
            if (!Enum.IsDefined(typeof(AiGoalPriority), Priority)) throw new ArgumentException("Intention priority is not supported.", nameof(Priority));
            if (Provenance == null) throw new ArgumentException("Intention provenance is required.", nameof(Provenance));
            if (Revision < 0L) throw new ArgumentException("Intention revision cannot be negative.", nameof(Revision));
            if (CreatedAt == default(DateTimeOffset)) throw new ArgumentException("Intention creation timestamp is required.", nameof(CreatedAt));
            if (UpdatedAt == default(DateTimeOffset)) throw new ArgumentException("Intention update timestamp is required.", nameof(UpdatedAt));
            if (AdoptedAt.HasValue && AdoptedAt.Value < CreatedAt) throw new ArgumentException("Intention adoption timestamp cannot precede creation.", nameof(AdoptedAt));
        }

        public AiIntention Clone()
        {
            var clone = new AiIntention
            {
                Id = Id,
                GoalId = GoalId,
                Description = Description,
                Status = Status,
                Priority = Priority,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                AdoptedAt = AdoptedAt,
                Revision = Revision
            };
            foreach (var constraint in Constraints ?? new List<string>()) clone.Constraints.Add(constraint);
            return clone;
        }
    }

    public sealed class AiIntentionStatusChange
    {
        public string Id { get; private set; }
        public string IntentionId { get; private set; }
        public long Revision { get; private set; }
        public AiIntentionStatus PreviousStatus { get; private set; }
        public AiIntentionStatus NewStatus { get; private set; }
        public string Reason { get; private set; }
        public string Evidence { get; private set; }
        public DateTimeOffset ChangedAt { get; private set; }
        public AiGoalAuthority Authority { get; private set; }

        public AiIntentionStatusChange(
            string intentionId,
            long revision,
            AiIntentionStatus previousStatus,
            AiIntentionStatus newStatus,
            string reason,
            string evidence,
            DateTimeOffset changedAt,
            AiGoalAuthority authority)
        {
            if (string.IsNullOrWhiteSpace(intentionId)) throw new ArgumentException("Intention ID is required.", nameof(intentionId));
            if (revision < 1L) throw new ArgumentOutOfRangeException(nameof(revision));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Status change reason is required.", nameof(reason));
            if (!Enum.IsDefined(typeof(AiIntentionStatus), previousStatus)) throw new ArgumentException("Previous intention status is not supported.", nameof(previousStatus));
            if (!Enum.IsDefined(typeof(AiIntentionStatus), newStatus)) throw new ArgumentException("New intention status is not supported.", nameof(newStatus));
            if (!Enum.IsDefined(typeof(AiGoalAuthority), authority)) throw new ArgumentException("Status change authority is not supported.", nameof(authority));
            if (changedAt == default(DateTimeOffset)) throw new ArgumentException("Status change timestamp is required.", nameof(changedAt));

            Id = Guid.NewGuid().ToString("N");
            IntentionId = intentionId;
            Revision = revision;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            Reason = reason.Trim();
            Evidence = evidence == null ? string.Empty : evidence.Trim();
            ChangedAt = changedAt;
            Authority = authority;
        }
    }
}

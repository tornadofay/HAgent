using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public enum AiPlanStatus
    {
        Draft = 0,
        Active = 1,
        Suspended = 2,
        Completed = 3,
        Failed = 4,
        Abandoned = 5,
        Superseded = 6
    }

    public enum AiPlanStepStatus
    {
        Pending = 0,
        Ready = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Blocked = 5,
        Skipped = 6,
        Cancelled = 7,
        Superseded = 8
    }

    public sealed class AiPlan
    {
        public string Id { get; set; }
        public string GoalId { get; set; }
        public string IntentionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AiPlanStatus Status { get; set; }
        public IList<string> Preconditions { get; private set; }
        public IList<string> Assumptions { get; private set; }
        public IList<string> ExpectedEffects { get; private set; }
        public IList<string> FailureConditions { get; private set; }
        public IList<string> CompletionCriteria { get; private set; }
        public IList<AiPlanStep> Steps { get; private set; }
        public AiGoalProvenance Provenance { get; set; }
        public string RevisionReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public long Revision { get; set; }

        public AiPlan()
        {
            Id = Guid.NewGuid().ToString("N");
            GoalId = string.Empty;
            IntentionId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Status = AiPlanStatus.Draft;
            Preconditions = new List<string>();
            Assumptions = new List<string>();
            ExpectedEffects = new List<string>();
            FailureConditions = new List<string>();
            CompletionCriteria = new List<string>();
            Steps = new List<AiPlanStep>();
            Provenance = new AiGoalProvenance();
            RevisionReason = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
            Revision = 0L;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("Plan ID is required.", nameof(Id));
            if (string.IsNullOrWhiteSpace(GoalId)) throw new ArgumentException("Plan goal ID is required.", nameof(GoalId));
            if (string.IsNullOrWhiteSpace(IntentionId)) throw new ArgumentException("Plan intention ID is required.", nameof(IntentionId));
            if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Plan title is required.", nameof(Title));
            if (!Enum.IsDefined(typeof(AiPlanStatus), Status)) throw new ArgumentException("Plan status is not supported.", nameof(Status));
            if (Provenance == null) throw new ArgumentException("Plan provenance is required.", nameof(Provenance));
            if (Revision < 0L) throw new ArgumentException("Plan revision cannot be negative.", nameof(Revision));
            if (string.IsNullOrWhiteSpace(RevisionReason) && Revision > 0L) throw new ArgumentException("Plan revision reason is required for revised plans.", nameof(RevisionReason));
            if (CreatedAt == default(DateTimeOffset)) throw new ArgumentException("Plan creation timestamp is required.", nameof(CreatedAt));
            if (UpdatedAt == default(DateTimeOffset)) throw new ArgumentException("Plan update timestamp is required.", nameof(UpdatedAt));

            var stepIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var step in Steps ?? new List<AiPlanStep>())
            {
                if (step == null) throw new ArgumentException("Plan steps cannot contain null entries.", nameof(Steps));
                step.Validate();
                if (!stepIds.Add(step.Id)) throw new ArgumentException("Plan step IDs must be unique.", nameof(Steps));
                if (!string.Equals(step.PlanId, Id, StringComparison.Ordinal)) throw new ArgumentException("Plan step must reference its owning plan.", nameof(Steps));
            }
        }

        public AiPlan Clone()
        {
            var clone = new AiPlan
            {
                Id = Id,
                GoalId = GoalId,
                IntentionId = IntentionId,
                Title = Title,
                Description = Description,
                Status = Status,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                RevisionReason = RevisionReason,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Revision = Revision
            };
            CopyStrings(Preconditions, clone.Preconditions);
            CopyStrings(Assumptions, clone.Assumptions);
            CopyStrings(ExpectedEffects, clone.ExpectedEffects);
            CopyStrings(FailureConditions, clone.FailureConditions);
            CopyStrings(CompletionCriteria, clone.CompletionCriteria);
            foreach (var step in Steps ?? new List<AiPlanStep>()) clone.Steps.Add(step == null ? null : step.Clone());
            return clone;
        }

        private static void CopyStrings(IList<string> source, IList<string> destination)
        {
            foreach (var value in source ?? new List<string>()) destination.Add(value);
        }
    }

    public sealed class AiPlanStep
    {
        public string Id { get; set; }
        public string PlanId { get; set; }
        public int Sequence { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AiPlanStepStatus Status { get; set; }
        public IList<string> DependsOnStepIds { get; private set; }
        public IList<string> Preconditions { get; private set; }
        public IList<string> Assumptions { get; private set; }
        public IList<string> ExpectedEffects { get; private set; }
        public IList<string> FailureConditions { get; private set; }
        public IList<string> CompletionCriteria { get; private set; }
        public AiGoalProvenance Provenance { get; set; }
        public long Revision { get; set; }

        public AiPlanStep()
        {
            Id = Guid.NewGuid().ToString("N");
            PlanId = string.Empty;
            Sequence = 0;
            Title = string.Empty;
            Description = string.Empty;
            Status = AiPlanStepStatus.Pending;
            DependsOnStepIds = new List<string>();
            Preconditions = new List<string>();
            Assumptions = new List<string>();
            ExpectedEffects = new List<string>();
            FailureConditions = new List<string>();
            CompletionCriteria = new List<string>();
            Provenance = new AiGoalProvenance();
            Revision = 0L;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("Plan step ID is required.", nameof(Id));
            if (string.IsNullOrWhiteSpace(PlanId)) throw new ArgumentException("Plan step plan ID is required.", nameof(PlanId));
            if (Sequence < 0) throw new ArgumentException("Plan step sequence cannot be negative.", nameof(Sequence));
            if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Plan step title is required.", nameof(Title));
            if (!Enum.IsDefined(typeof(AiPlanStepStatus), Status)) throw new ArgumentException("Plan step status is not supported.", nameof(Status));
            if (Provenance == null) throw new ArgumentException("Plan step provenance is required.", nameof(Provenance));
            if (Revision < 0L) throw new ArgumentException("Plan step revision cannot be negative.", nameof(Revision));

            foreach (var dependencyId in DependsOnStepIds ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(dependencyId)) throw new ArgumentException("Plan step dependency IDs cannot be empty.", nameof(DependsOnStepIds));
                if (string.Equals(dependencyId, Id, StringComparison.Ordinal)) throw new ArgumentException("Plan step cannot depend on itself.", nameof(DependsOnStepIds));
            }
        }

        public AiPlanStep Clone()
        {
            var clone = new AiPlanStep
            {
                Id = Id,
                PlanId = PlanId,
                Sequence = Sequence,
                Title = Title,
                Description = Description,
                Status = Status,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                Revision = Revision
            };
            CopyStrings(DependsOnStepIds, clone.DependsOnStepIds);
            CopyStrings(Preconditions, clone.Preconditions);
            CopyStrings(Assumptions, clone.Assumptions);
            CopyStrings(ExpectedEffects, clone.ExpectedEffects);
            CopyStrings(FailureConditions, clone.FailureConditions);
            CopyStrings(CompletionCriteria, clone.CompletionCriteria);
            return clone;
        }

        private static void CopyStrings(IList<string> source, IList<string> destination)
        {
            foreach (var value in source ?? new List<string>()) destination.Add(value);
        }
    }
}

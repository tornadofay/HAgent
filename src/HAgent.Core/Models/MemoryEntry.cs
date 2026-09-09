using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class MemoryEntry
    {
        public MemoryEntry()
        {
            Id = Guid.NewGuid().ToString("N");
            Scope = MemoryScope.Agent;
            Kind = MemoryKind.Fact;
            Family = AiMemoryFamily.Semantic;
            TypeId = "semantic.fact";
            OwnerId = string.Empty;
            TaskId = string.Empty;
            Content = string.Empty;
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Provenance = new AiMemoryProvenance();
            CreatedAt = DateTimeOffset.UtcNow;
            OccurredAt = CreatedAt;
            ExpiresAt = null;
        }

        public string Id { get; set; }
        public MemoryScope Scope { get; set; }
        public MemoryKind Kind { get; set; }
        public AiMemoryFamily Family { get; set; }
        public string TypeId { get; set; }
        public string OwnerId { get; set; }
        public string TaskId { get; set; }
        public string Content { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public AiMemoryProvenance Provenance { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }

        public bool IsExpired(DateTimeOffset? at = null)
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= (at ?? DateTimeOffset.UtcNow);
        }

        public MemoryEntry Clone()
        {
            var clone = new MemoryEntry
            {
                Id = Id,
                Scope = Scope,
                Kind = Kind,
                Family = Family,
                TypeId = TypeId,
                OwnerId = OwnerId,
                TaskId = TaskId,
                Content = Content,
                Provenance = Provenance == null ? null : Provenance.Clone(),
                CreatedAt = CreatedAt,
                OccurredAt = OccurredAt,
                ExpiresAt = ExpiresAt
            };

            if (Metadata != null)
            {
                foreach (var pair in Metadata)
                    clone.Metadata[pair.Key] = pair.Value;
            }

            return clone;
        }

        public void Validate()
        {
            ValidateText(Id, 128, true, nameof(Id));
            if (!Enum.IsDefined(typeof(MemoryScope), Scope))
                throw new ArgumentException("Invalid memory scope.");
            if (!Enum.IsDefined(typeof(MemoryKind), Kind))
                throw new ArgumentException("Invalid memory kind.");
            if (!Enum.IsDefined(typeof(AiMemoryFamily), Family))
                throw new ArgumentException("Invalid memory family.");
            ValidateText(TypeId, 256, true, nameof(TypeId));
            ValidateText(OwnerId, 2048, true, nameof(OwnerId));
            ValidateText(TaskId, 512, false, nameof(TaskId));
            ValidateText(Content, 200000, true, nameof(Content));

            if (Metadata == null || Metadata.Count > 32)
                throw new ArgumentException("Memory metadata exceeds its bounds.");
            foreach (var pair in Metadata)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 128 ||
                    pair.Value == null || pair.Value.Length > 4096)
                    throw new ArgumentException("Memory metadata contains an invalid entry.");
            }

            if (Provenance == null)
                throw new ArgumentException("Memory provenance is required.");
            Provenance.Validate();

            if (CreatedAt == default(DateTimeOffset) || OccurredAt == default(DateTimeOffset))
                throw new ArgumentException("Memory timestamps are required.");
            if (ExpiresAt.HasValue && ExpiresAt.Value <= CreatedAt)
                throw new ArgumentException("Memory ExpiresAt must be later than CreatedAt.");

            if (Family == AiMemoryFamily.Custom && TypeId.StartsWith("working.", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Custom memory types cannot use reserved built-in TypeId prefixes.");

            ValidateFamilyTypeConsistency();
        }

        private void ValidateFamilyTypeConsistency()
        {
            if (Family == AiMemoryFamily.Working && !TypeId.StartsWith("working.", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Working memory types must use the working.* TypeId namespace.");
            if (Family == AiMemoryFamily.Episodic && !TypeId.StartsWith("episodic.", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Episodic memory types must use the episodic.* TypeId namespace.");
            if (Family == AiMemoryFamily.Semantic && !TypeId.StartsWith("semantic.", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Semantic memory types must use the semantic.* TypeId namespace.");
            if (Family == AiMemoryFamily.Procedural && !TypeId.StartsWith("procedural.", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Procedural memory types must use the procedural.* TypeId namespace.");
            if (Family == AiMemoryFamily.Custom &&
                (TypeId.StartsWith("working.", StringComparison.OrdinalIgnoreCase) ||
                 TypeId.StartsWith("episodic.", StringComparison.OrdinalIgnoreCase) ||
                 TypeId.StartsWith("semantic.", StringComparison.OrdinalIgnoreCase) ||
                 TypeId.StartsWith("procedural.", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Custom memory TypeId must use an application-specific namespace.");
        }

        private static void ValidateText(string value, int maxLength, bool required, string name)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.");
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " exceeds its maximum length.");
        }
    }

    public enum MemoryScope
    {
        Session,
        Task,
        Agent,
        User,
        Application,
        Shared
    }

    public enum MemoryKind
    {
        Fact,
        Preference,
        Task,
        Event
    }
}

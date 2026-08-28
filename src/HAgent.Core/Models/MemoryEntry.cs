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
            OwnerId = string.Empty;
            TaskId = string.Empty;
            Content = string.Empty;
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CreatedAt = DateTimeOffset.UtcNow;
            OccurredAt = CreatedAt;
        }

        public string Id { get; set; }
        public MemoryScope Scope { get; set; }
        public MemoryKind Kind { get; set; }
        public string OwnerId { get; set; }
        public string TaskId { get; set; }
        public string Content { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
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

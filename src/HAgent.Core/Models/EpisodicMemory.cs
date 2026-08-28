using System;
using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class EpisodicMemory
    {
        public EpisodicMemory()
        {
            Id = Guid.NewGuid().ToString("N");
            OwnerId = string.Empty;
            TaskId = string.Empty;
            SessionId = string.Empty;
            Title = string.Empty;
            Summary = string.Empty;
            Outcome = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            OccurredAt = CreatedAt;
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Id { get; set; }
        public string OwnerId { get; set; }
        public string TaskId { get; set; }
        public string SessionId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Outcome { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}

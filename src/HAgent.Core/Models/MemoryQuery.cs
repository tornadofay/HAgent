using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class MemoryQuery
    {
        public MemoryQuery()
        {
            Scope = null;
            Kind = null;
            OwnerId = string.Empty;
            TaskId = string.Empty;
            Text = string.Empty;
            MaxResults = 10;
        }

        public MemoryScope? Scope { get; set; }
        public MemoryKind? Kind { get; set; }
        public string OwnerId { get; set; }
        public string TaskId { get; set; }
        public string Text { get; set; }
        public int MaxResults { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}

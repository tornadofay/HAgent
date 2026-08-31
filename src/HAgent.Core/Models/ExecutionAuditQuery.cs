using System;

namespace HAgent.Models
{
    /// <summary>
    /// Bounded query for secret-safe execution audit records.
    /// </summary>
    public sealed class ExecutionAuditQuery
    {
        public ExecutionAuditQuery()
        {
            MaxResults = 50;
        }

        public string ExecutionId { get; set; }
        public string CorrelationId { get; set; }
        public string AgentId { get; set; }
        public int MaxResults { get; set; }

        public int GetEffectiveMaxResults()
        {
            if (MaxResults <= 0) return 50;
            return Math.Min(MaxResults, 200);
        }
    }
}

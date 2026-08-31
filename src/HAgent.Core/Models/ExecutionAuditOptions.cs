using System;

namespace HAgent.Models
{
    /// <summary>
    /// Controls optional automatic persistence of terminal agent execution audit metadata.
    /// </summary>
    public sealed class ExecutionAuditOptions
    {
        public const int DefaultMaxRecords = 5000;
        public const int MaximumMaxRecords = 1000000;

        public ExecutionAuditOptions()
        {
            Enabled = true;
            MaxRecords = DefaultMaxRecords;
        }

        public bool Enabled { get; set; }
        public int MaxRecords { get; set; }

        public int GetEffectiveMaxRecords()
        {
            if (MaxRecords <= 0) return DefaultMaxRecords;
            return Math.Min(MaxRecords, MaximumMaxRecords);
        }

        public void Validate()
        {
            if (MaxRecords < 1 || MaxRecords > MaximumMaxRecords)
                throw new ArgumentOutOfRangeException(nameof(MaxRecords), "Audit retention must be between 1 and " + MaximumMaxRecords + " records.");
        }
    }
}

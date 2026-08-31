using System;
using System.Collections.Generic;
using System.Linq;

namespace HAgent.Models
{
    /// <summary>
    /// Explicit field projection over a bounded data source. This contract intentionally does not contain SQL, expressions, filters, or arbitrary code.
    /// </summary>
    public sealed class DataProjectionRequest
    {
        public DataProjectionRequest()
        {
            Fields = new List<string>();
            Take = 100;
        }

        public IReadOnlyList<string> Fields { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public void Validate(int maximumTake = 1000, int maximumSkip = 100000)
        {
            if (Fields == null || Fields.Count == 0)
                throw new ArgumentException("At least one projected field is required.", nameof(Fields));
            if (Fields.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Projected field names cannot be empty.", nameof(Fields));
            if (Skip < 0 || Skip > maximumSkip)
                throw new ArgumentOutOfRangeException(nameof(Skip));
            if (Take < 1 || Take > maximumTake)
                throw new ArgumentOutOfRangeException(nameof(Take));
        }
    }
}

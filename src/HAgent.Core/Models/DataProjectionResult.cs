using System.Collections.Generic;

namespace HAgent.Models
{
    public sealed class DataProjectionResult
    {
        public DataProjectionResult()
        {
            Rows = new List<IReadOnlyDictionary<string, object>>();
        }

        public IReadOnlyList<IReadOnlyDictionary<string, object>> Rows { get; set; }
        public int Skipped { get; set; }
        public int Returned { get; set; }
        public bool HasMore { get; set; }
    }
}

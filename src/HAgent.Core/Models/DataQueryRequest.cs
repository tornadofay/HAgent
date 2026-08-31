using System;
using System.Collections.Generic;
using System.Linq;

namespace HAgent.Models
{
    /// <summary>
    /// Provider-neutral structured data query intent.
    /// This contract contains only allow-listed fields, scalar comparisons, sorting, and bounded paging.
    /// It contains no SQL, scripting, arbitrary expressions, or executable callbacks.
    /// </summary>
    public sealed class DataQueryRequest
    {
        public DataQueryRequest()
        {
            Fields = new List<string>();
            Filters = new List<DataFilterCondition>();
            Sorts = new List<DataSort>();
            Take = 100;
        }

        public IReadOnlyList<string> Fields { get; set; }
        public IReadOnlyList<DataFilterCondition> Filters { get; set; }
        public IReadOnlyList<DataSort> Sorts { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public void Validate(int maximumTake = 1000, int maximumSkip = 100000, int maximumFilters = 16, int maximumSorts = 8)
        {
            if (Fields == null || Fields.Count == 0)
                throw new ArgumentException("At least one query field is required.", nameof(Fields));
            if (Fields.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Query field names cannot be empty.", nameof(Fields));
            if (Fields.Count != Fields.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                throw new ArgumentException("Query fields cannot contain duplicates.", nameof(Fields));

            if (Filters == null) throw new ArgumentException("Filters cannot be null.", nameof(Filters));
            if (Filters.Count > maximumFilters) throw new ArgumentOutOfRangeException(nameof(Filters));
            foreach (var filter in Filters)
            {
                if (filter == null) throw new ArgumentException("Filter entries cannot be null.", nameof(Filters));
                filter.Validate();
            }

            if (Sorts == null) throw new ArgumentException("Sorts cannot be null.", nameof(Sorts));
            if (Sorts.Count > maximumSorts) throw new ArgumentOutOfRangeException(nameof(Sorts));
            foreach (var sort in Sorts)
            {
                if (sort == null) throw new ArgumentException("Sort entries cannot be null.", nameof(Sorts));
                sort.Validate();
            }

            if (Skip < 0 || Skip > maximumSkip)
                throw new ArgumentOutOfRangeException(nameof(Skip));
            if (Take < 1 || Take > maximumTake)
                throw new ArgumentOutOfRangeException(nameof(Take));
        }
    }
}

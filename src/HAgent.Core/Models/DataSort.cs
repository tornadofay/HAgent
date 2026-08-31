using System;

namespace HAgent.Models
{
    /// <summary>
    /// One explicit sort field. Sorting is a structured field selection, not an expression.
    /// </summary>
    public sealed class DataSort
    {
        public string Field { get; set; }
        public bool Descending { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Field))
                throw new ArgumentException("Sort field is required.", nameof(Field));
        }
    }
}

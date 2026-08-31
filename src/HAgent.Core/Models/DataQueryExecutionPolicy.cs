using System;

namespace HAgent.Models
{
    /// <summary>
    /// Host-owned execution limits for one structured data query.
    /// These limits bound query shape, result size, and execution time without changing query intent.
    /// </summary>
    public sealed class DataQueryExecutionPolicy
    {
        public DataQueryExecutionPolicy()
        {
            MaximumTake = 1000;
            MaximumSkip = 100000;
            MaximumFilters = 16;
            MaximumSorts = 8;
            MaximumResultRows = 1000;
            Timeout = TimeSpan.FromSeconds(30);
        }

        public int MaximumTake { get; set; }
        public int MaximumSkip { get; set; }
        public int MaximumFilters { get; set; }
        public int MaximumSorts { get; set; }
        public int MaximumResultRows { get; set; }
        public TimeSpan Timeout { get; set; }

        public void ValidateRequest(DataQueryRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateLimits();
            request.Validate(MaximumTake, MaximumSkip, MaximumFilters, MaximumSorts);
            if (request.Take > MaximumResultRows)
                throw new ArgumentOutOfRangeException(nameof(request.Take), "The requested page exceeds the maximum result-row budget.");
        }

        public System.Threading.CancellationTokenSource CreateCancellationSource(System.Threading.CancellationToken cancellationToken)
        {
            ValidateLimits();
            var linked = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked.CancelAfter(Timeout);
            return linked;
        }

        private void ValidateLimits()
        {
            if (MaximumTake < 1) throw new InvalidOperationException("MaximumTake must be greater than zero.");
            if (MaximumSkip < 0) throw new InvalidOperationException("MaximumSkip cannot be negative.");
            if (MaximumFilters < 0) throw new InvalidOperationException("MaximumFilters cannot be negative.");
            if (MaximumSorts < 0) throw new InvalidOperationException("MaximumSorts cannot be negative.");
            if (MaximumResultRows < 1) throw new InvalidOperationException("MaximumResultRows must be greater than zero.");
            if (MaximumResultRows > MaximumTake)
                throw new InvalidOperationException("MaximumResultRows cannot exceed MaximumTake.");
            if (Timeout <= TimeSpan.Zero) throw new InvalidOperationException("Timeout must be greater than zero.");
        }
    }
}

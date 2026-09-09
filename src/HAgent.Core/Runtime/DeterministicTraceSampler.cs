using System;
using System.Security.Cryptography;
using System.Text;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Deterministic provider-neutral sampler. Equivalent root inputs produce equivalent sampling decisions.
    /// Child spans inherit the root decision from the trace context.
    /// </summary>
    public sealed class DeterministicTraceSampler : ITraceSampler
    {
        private readonly TraceSamplingOptions _options;

        public DeterministicTraceSampler(TraceSamplingOptions options = null)
        {
            _options = options == null ? new TraceSamplingOptions() : options.Clone();
            _options.Validate();
        }

        public bool ShouldSample(TraceSpanStartOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (_options.SampleRate <= 0d)
                return false;
            if (_options.SampleRate >= 1d)
                return true;

            var correlation = options.Correlation ?? new TraceCorrelation();
            var stableKey = !string.IsNullOrWhiteSpace(correlation.ExecutionId)
                ? correlation.ExecutionId
                : !string.IsNullOrWhiteSpace(correlation.HostCorrelationId)
                    ? correlation.HostCorrelationId
                    : options.OperationName ?? string.Empty;

            var material = (_options.Salt ?? string.Empty) + "|" + stableKey;
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(material));
                var bucket = BitConverter.ToUInt32(bytes, 0) / (double)uint.MaxValue;
                return bucket < _options.SampleRate;
            }
        }
    }
}

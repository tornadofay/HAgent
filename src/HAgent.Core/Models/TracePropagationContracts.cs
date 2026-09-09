using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HAgent.Models
{
    public enum TracePropagationImportStatus
    {
        Accepted = 0,
        Missing = 1,
        RejectedAsUntrusted = 2,
        InvalidTraceContext = 3,
        InvalidCorrelation = 4
    }

    /// <summary>
    /// Controls how an incoming transport-neutral trace carrier is accepted at a host boundary.
    /// </summary>
    public sealed class TracePropagationImportOptions
    {
        public TracePropagationImportOptions()
        {
            AcceptUntrustedTraceContext = false;
            MaxValueLength = 128;
        }

        public bool AcceptUntrustedTraceContext { get; set; }
        public int MaxValueLength { get; set; }

        public void Validate()
        {
            if (MaxValueLength <= 0 || MaxValueLength > 128)
                throw new ArgumentOutOfRangeException(nameof(MaxValueLength));
        }

        public TracePropagationImportOptions Clone()
        {
            return new TracePropagationImportOptions
            {
                AcceptUntrustedTraceContext = AcceptUntrustedTraceContext,
                MaxValueLength = MaxValueLength
            };
        }
    }

    /// <summary>
    /// Transport-neutral bounded carrier for cross-process trace and correlation values.
    /// It intentionally does not imply HTTP headers, message headers, OpenTelemetry, or another transport.
    /// </summary>
    public sealed class TracePropagationCarrier
    {
        private const int MaxEntries = 32;
        private const int MaxKeyLength = 64;
        private const int MaxValueLength = 128;

        private readonly Dictionary<string, string> _values;

        public TracePropagationCarrier()
        {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public int Count { get { return _values.Count; } }

        public IReadOnlyDictionary<string, string> Values
        {
            get { return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(_values, StringComparer.OrdinalIgnoreCase)); }
        }

        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Propagation key is required.", nameof(key));
            if (key.Length > MaxKeyLength)
                throw new ArgumentOutOfRangeException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value.Length > MaxValueLength)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (!_values.ContainsKey(key) && _values.Count >= MaxEntries)
                throw new InvalidOperationException("Propagation carrier entry limit has been reached.");

            _values[key] = value;
        }

        public bool TryGetValue(string key, out string value)
        {
            return _values.TryGetValue(key, out value);
        }

        public TracePropagationCarrier Clone()
        {
            var clone = new TracePropagationCarrier();
            foreach (var pair in _values)
                clone._values[pair.Key] = pair.Value;
            return clone;
        }
    }

    /// <summary>
    /// Result of importing a bounded cross-process carrier. Invalid or untrusted trace context is never silently accepted.
    /// </summary>
    public sealed class TracePropagationImportResult
    {
        internal TracePropagationImportResult(
            TracePropagationImportStatus status,
            TraceContext context,
            TraceCorrelation correlation,
            string rejectionReason)
        {
            Status = status;
            Context = context;
            Correlation = correlation == null ? new TraceCorrelation() : correlation.Clone();
            RejectionReason = rejectionReason ?? string.Empty;
        }

        public TracePropagationImportStatus Status { get; private set; }
        public TraceContext Context { get; private set; }
        public TraceCorrelation Correlation { get; private set; }
        public string RejectionReason { get; private set; }

        public bool Accepted
        {
            get { return Status == TracePropagationImportStatus.Accepted; }
        }
    }
}

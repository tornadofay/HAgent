using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace HAgent.Models
{
    public enum TraceSpanStatus
    {
        Unset = 0,
        Succeeded = 1,
        Failed = 2,
        Cancelled = 3,
        Timeout = 4,
        Rejected = 5,
        Skipped = 6
    }

    /// <summary>
    /// Immutable provider-neutral trace propagation context.
    /// </summary>
    public sealed class TraceContext
    {
        public TraceContext(string traceId, string parentSpanId, bool sampled)
        {
            if (string.IsNullOrWhiteSpace(traceId))
                throw new ArgumentException("Trace id is required.", nameof(traceId));
            if (traceId.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(traceId));
            if (parentSpanId != null && parentSpanId.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(parentSpanId));

            TraceId = traceId;
            ParentSpanId = parentSpanId;
            Sampled = sampled;
        }

        public string TraceId { get; private set; }
        public string ParentSpanId { get; private set; }
        public bool Sampled { get; private set; }
    }

    /// <summary>
    /// Existing HAgent correlation and identity references associated with an observable operation.
    /// These values remain distinct from TraceId and SpanId.
    /// </summary>
    public sealed class TraceCorrelation
    {
        public TraceCorrelation()
        {
            DeploymentId = string.Empty;
            TenantId = string.Empty;
            PrincipalId = string.Empty;
            UserId = string.Empty;
            SessionId = string.Empty;
            WorkspaceId = string.Empty;
            AgentProfileId = string.Empty;
            RuntimeInstanceId = string.Empty;
            ExecutionId = string.Empty;
            ExecutionCorrelationId = string.Empty;
            HostCorrelationId = string.Empty;
            EventId = string.Empty;
            CausationId = string.Empty;
        }

        public string DeploymentId { get; set; }
        public string TenantId { get; set; }
        public string PrincipalId { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string WorkspaceId { get; set; }
        public string AgentProfileId { get; set; }
        public string RuntimeInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public string ExecutionCorrelationId { get; set; }
        public string HostCorrelationId { get; set; }
        public string EventId { get; set; }
        public string CausationId { get; set; }

        public TraceCorrelation Clone()
        {
            return new TraceCorrelation
            {
                DeploymentId = DeploymentId,
                TenantId = TenantId,
                PrincipalId = PrincipalId,
                UserId = UserId,
                SessionId = SessionId,
                WorkspaceId = WorkspaceId,
                AgentProfileId = AgentProfileId,
                RuntimeInstanceId = RuntimeInstanceId,
                ExecutionId = ExecutionId,
                ExecutionCorrelationId = ExecutionCorrelationId,
                HostCorrelationId = HostCorrelationId,
                EventId = EventId,
                CausationId = CausationId
            };
        }
    }

    /// <summary>
    /// Bounded metadata representation. Raw payloads and arbitrary objects are intentionally unsupported.
    /// </summary>
    public sealed class TraceMetadata
    {
        private const int MaxEntries = 32;
        private const int MaxKeyLength = 128;
        private const int MaxValueLength = 512;

        private readonly Dictionary<string, string> _values;

        public TraceMetadata()
        {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public int Count { get { return _values.Count; } }

        public IReadOnlyDictionary<string, string> Values
        {
            get { return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(_values, StringComparer.OrdinalIgnoreCase)); }
        }

        public void Add(string key, string value)
        {
            ValidateKey(key);
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value.Length > MaxValueLength)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (!_values.ContainsKey(key) && _values.Count >= MaxEntries)
                throw new InvalidOperationException("Trace metadata entry limit has been reached.");

            _values[key] = value;
        }

        public void AddRedacted(string key)
        {
            Add(key, "[Redacted]");
        }

        public void AddOmitted(string key)
        {
            Add(key, "[Omitted]");
        }

        public TraceMetadata Clone()
        {
            var clone = new TraceMetadata();
            foreach (var pair in _values)
                clone._values[pair.Key] = pair.Value;
            return clone;
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Trace metadata key is required.", nameof(key));
            if (key.Length > MaxKeyLength)
                throw new ArgumentOutOfRangeException(nameof(key));
        }
    }

    /// <summary>
    /// Deterministic sampling policy input. A rate of 1 samples every root trace and 0 samples none.
    /// Child spans inherit the root sampling decision.
    /// </summary>
    public sealed class TraceSamplingOptions
    {
        public TraceSamplingOptions()
        {
            SampleRate = 1d;
            Salt = string.Empty;
        }

        public double SampleRate { get; set; }
        public string Salt { get; set; }

        public void Validate()
        {
            if (double.IsNaN(SampleRate) || double.IsInfinity(SampleRate) || SampleRate < 0d || SampleRate > 1d)
                throw new ArgumentOutOfRangeException(nameof(SampleRate));
            if (Salt == null)
                throw new ArgumentNullException(nameof(Salt));
            if (Salt.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(Salt));
        }

        public TraceSamplingOptions Clone()
        {
            return new TraceSamplingOptions
            {
                SampleRate = SampleRate,
                Salt = Salt
            };
        }
    }

    /// <summary>
    /// Bounded in-memory retention policy for observable spans.
    /// </summary>
    public sealed class TraceRetentionOptions
    {
        public TraceRetentionOptions()
        {
            MaxTraceCount = 128;
            MaxSpanCount = 4096;
            MaxSpansPerTrace = 256;
            MaxAggregateMetadataCharacters = 262144;
            MaxAge = TimeSpan.FromHours(1);
        }

        public int MaxTraceCount { get; set; }
        public int MaxSpanCount { get; set; }
        public int MaxSpansPerTrace { get; set; }
        public int MaxAggregateMetadataCharacters { get; set; }
        public TimeSpan MaxAge { get; set; }

        public void Validate()
        {
            if (MaxTraceCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxTraceCount));
            if (MaxSpanCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxSpanCount));
            if (MaxSpansPerTrace <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxSpansPerTrace));
            if (MaxAggregateMetadataCharacters <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxAggregateMetadataCharacters));
            if (MaxAge <= TimeSpan.Zero && MaxAge != Timeout.InfiniteTimeSpan)
                throw new ArgumentOutOfRangeException(nameof(MaxAge));
        }

        public TraceRetentionOptions Clone()
        {
            return new TraceRetentionOptions
            {
                MaxTraceCount = MaxTraceCount,
                MaxSpanCount = MaxSpanCount,
                MaxSpansPerTrace = MaxSpansPerTrace,
                MaxAggregateMetadataCharacters = MaxAggregateMetadataCharacters,
                MaxAge = MaxAge
            };
        }
    }

    /// <summary>
    /// Provider-neutral sampling decision boundary.
    /// </summary>
    public interface ITraceSampler
    {
        bool ShouldSample(TraceSpanStartOptions options);
    }

    /// <summary>
    /// Bounded input required to create one observable span.
    /// </summary>
    public sealed class TraceSpanStartOptions
    {
        public TraceSpanStartOptions()
        {
            OperationName = string.Empty;
            Kind = string.Empty;
            Correlation = new TraceCorrelation();
            Metadata = new TraceMetadata();
            Sampled = null;
        }

        public TraceContext ParentContext { get; set; }
        public string OperationName { get; set; }
        public string Kind { get; set; }
        public TraceCorrelation Correlation { get; set; }
        public TraceMetadata Metadata { get; set; }

        /// <summary>
        /// Optional explicit sampling decision. When absent, a root may use the configured sampler and
        /// child spans inherit their parent sampled state.
        /// </summary>
        public bool? Sampled { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OperationName))
                throw new ArgumentException("Trace span operation name is required.", nameof(OperationName));
            if (OperationName.Length > 256)
                throw new ArgumentOutOfRangeException(nameof(OperationName));
            if (string.IsNullOrWhiteSpace(Kind))
                throw new ArgumentException("Trace span kind is required.", nameof(Kind));
            if (Kind.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(Kind));
            if (Correlation == null)
                throw new ArgumentNullException(nameof(Correlation));
            if (Metadata == null)
                throw new ArgumentNullException(nameof(Metadata));
        }
    }

    /// <summary>
    /// Provider-neutral observable span. Lifecycle completion is terminal and thread-safe.
    /// </summary>
    public sealed class TraceSpan
    {
        private readonly object _sync = new object();

        internal TraceSpan(string traceId, string spanId, string parentSpanId, string operationName, string kind,
            DateTimeOffset startedAt, bool sampled, TraceCorrelation correlation, TraceMetadata metadata, long sequence)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            OperationName = operationName;
            Kind = kind;
            StartedAt = startedAt;
            Sampled = sampled;
            Correlation = correlation == null ? new TraceCorrelation() : correlation.Clone();
            Metadata = metadata == null ? new TraceMetadata() : metadata.Clone();
            Sequence = sequence;
            Status = TraceSpanStatus.Unset;
        }

        public string TraceId { get; private set; }
        public string SpanId { get; private set; }
        public string ParentSpanId { get; private set; }
        public string OperationName { get; private set; }
        public string Kind { get; private set; }
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }
        public TraceSpanStatus Status { get; private set; }
        public bool Sampled { get; private set; }
        public TraceCorrelation Correlation { get; private set; }
        public TraceMetadata Metadata { get; private set; }
        public long Sequence { get; private set; }

        public TimeSpan? Duration
        {
            get
            {
                lock (_sync)
                    return CompletedAt.HasValue ? CompletedAt.Value - StartedAt : (TimeSpan?)null;
            }
        }

        public bool IsCompleted
        {
            get
            {
                lock (_sync)
                    return CompletedAt.HasValue;
            }
        }

        internal bool TryComplete(TraceSpanStatus status, DateTimeOffset completedAt)
        {
            lock (_sync)
            {
                if (CompletedAt.HasValue)
                    return false;
                if (completedAt < StartedAt)
                    throw new ArgumentOutOfRangeException(nameof(completedAt), "Trace span completion cannot precede its start.");

                Status = status;
                CompletedAt = completedAt;
                return true;
            }
        }
    }

    public interface ITraceSpan
    {
        TraceSpan Record { get; }
        TraceContext Context { get; }
        bool TryComplete(TraceSpanStatus status);
    }

    public interface ITraceRecorder
    {
        ITraceSpan StartSpan(TraceSpanStartOptions options);
        IReadOnlyList<TraceSpan> GetSpans();
    }
}

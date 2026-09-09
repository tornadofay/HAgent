using System;
using HAgent.Models;

namespace HAgent.Runtime
{
    /// <summary>
    /// Maps HAgent trace/correlation semantics to a transport-neutral bounded carrier.
    /// Hosts and transports remain responsible for choosing their actual wire format and trust boundary.
    /// </summary>
    public static class TracePropagation
    {
        private const string TraceIdKey = "hagent.trace.trace-id";
        private const string ParentSpanIdKey = "hagent.trace.parent-span-id";
        private const string SampledKey = "hagent.trace.sampled";
        private const string DeploymentIdKey = "hagent.correlation.deployment-id";
        private const string TenantIdKey = "hagent.correlation.tenant-id";
        private const string PrincipalIdKey = "hagent.correlation.principal-id";
        private const string UserIdKey = "hagent.correlation.user-id";
        private const string SessionIdKey = "hagent.correlation.session-id";
        private const string WorkspaceIdKey = "hagent.correlation.workspace-id";
        private const string AgentProfileIdKey = "hagent.correlation.agent-profile-id";
        private const string RuntimeInstanceIdKey = "hagent.correlation.runtime-instance-id";
        private const string ExecutionIdKey = "hagent.correlation.execution-id";
        private const string ExecutionCorrelationIdKey = "hagent.correlation.execution-correlation-id";
        private const string HostCorrelationIdKey = "hagent.correlation.host-correlation-id";
        private const string EventIdKey = "hagent.correlation.event-id";
        private const string CausationIdKey = "hagent.correlation.causation-id";

        public static TraceContext Current
        {
            get { return TraceAmbient.Current; }
        }

        public static IDisposable Push(TraceContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return TraceAmbient.Push(context);
        }

        public static IDisposable Push(TraceContext context, TraceCorrelation correlation)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return TraceAmbient.Push(context, correlation);
        }

        public static TracePropagationCarrier Export(TraceContext context, TraceCorrelation correlation)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var carrier = new TracePropagationCarrier();
            carrier.Set(TraceIdKey, context.TraceId);
            if (!string.IsNullOrEmpty(context.ParentSpanId))
                carrier.Set(ParentSpanIdKey, context.ParentSpanId);
            carrier.Set(SampledKey, context.Sampled ? "1" : "0");

            if (correlation == null)
                return carrier;

            AddIfPresent(carrier, DeploymentIdKey, correlation.DeploymentId);
            AddIfPresent(carrier, TenantIdKey, correlation.TenantId);
            AddIfPresent(carrier, PrincipalIdKey, correlation.PrincipalId);
            AddIfPresent(carrier, UserIdKey, correlation.UserId);
            AddIfPresent(carrier, SessionIdKey, correlation.SessionId);
            AddIfPresent(carrier, WorkspaceIdKey, correlation.WorkspaceId);
            AddIfPresent(carrier, AgentProfileIdKey, correlation.AgentProfileId);
            AddIfPresent(carrier, RuntimeInstanceIdKey, correlation.RuntimeInstanceId);
            AddIfPresent(carrier, ExecutionIdKey, correlation.ExecutionId);
            AddIfPresent(carrier, ExecutionCorrelationIdKey, correlation.ExecutionCorrelationId);
            AddIfPresent(carrier, HostCorrelationIdKey, correlation.HostCorrelationId);
            AddIfPresent(carrier, EventIdKey, correlation.EventId);
            AddIfPresent(carrier, CausationIdKey, correlation.CausationId);
            return carrier;
        }

        public static TracePropagationImportResult Import(
            TracePropagationCarrier carrier,
            TracePropagationImportOptions options = null)
        {
            options = options == null ? new TracePropagationImportOptions() : options.Clone();
            options.Validate();

            var correlationResult = ReadCorrelation(carrier, options.MaxValueLength);
            if (!correlationResult.Valid)
            {
                return new TracePropagationImportResult(
                    TracePropagationImportStatus.InvalidCorrelation,
                    null,
                    correlationResult.Correlation,
                    "Incoming correlation identity exceeded the host propagation bounds.");
            }

            string traceId;
            if (carrier == null || !carrier.TryGetValue(TraceIdKey, out traceId) || string.IsNullOrEmpty(traceId))
            {
                return new TracePropagationImportResult(
                    TracePropagationImportStatus.Missing,
                    null,
                    correlationResult.Correlation,
                    "No incoming trace context was provided.");
            }

            if (!options.AcceptUntrustedTraceContext)
            {
                return new TracePropagationImportResult(
                    TracePropagationImportStatus.RejectedAsUntrusted,
                    null,
                    correlationResult.Correlation,
                    "Incoming trace context was not accepted by the host trust boundary.");
            }

            if (traceId.Length > options.MaxValueLength || !IsValidIdentifier(traceId))
            {
                return new TracePropagationImportResult(
                    TracePropagationImportStatus.InvalidTraceContext,
                    null,
                    correlationResult.Correlation,
                    "Incoming trace identity is malformed or exceeds the host propagation bounds.");
            }

            string parentSpanId;
            if (carrier.TryGetValue(ParentSpanIdKey, out parentSpanId))
            {
                if (parentSpanId.Length > options.MaxValueLength || !IsValidIdentifier(parentSpanId))
                {
                    return new TracePropagationImportResult(
                        TracePropagationImportStatus.InvalidTraceContext,
                        null,
                        correlationResult.Correlation,
                        "Incoming parent span identity is malformed or exceeds the host propagation bounds.");
                }
            }

            string sampledValue;
            if (!carrier.TryGetValue(SampledKey, out sampledValue))
            {
                return new TracePropagationImportResult(
                    TracePropagationImportStatus.InvalidTraceContext,
                    null,
                    correlationResult.Correlation,
                    "Incoming sampled state is missing.");
            }

            bool sampled;
            if (sampledValue == "1")
                sampled = true;
            else if (sampledValue == "0")
                sampled = false;
            else
            {
                return new TracePropagationImportResult(
                    TracePropagationImportStatus.InvalidTraceContext,
                    null,
                    correlationResult.Correlation,
                    "Incoming sampled state is invalid.");
            }

            return new TracePropagationImportResult(
                TracePropagationImportStatus.Accepted,
                new TraceContext(traceId, parentSpanId, sampled),
                correlationResult.Correlation,
                string.Empty);
        }

        private static void AddIfPresent(TracePropagationCarrier carrier, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                carrier.Set(key, value);
        }

        private static bool IsValidIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            for (var i = 0; i < value.Length; i++)
            {
                var character = value[i];
                if (char.IsControl(character) || char.IsWhiteSpace(character))
                    return false;
            }

            return true;
        }

        private static CorrelationReadResult ReadCorrelation(TracePropagationCarrier carrier, int maxValueLength)
        {
            var correlation = new TraceCorrelation();
            if (carrier == null)
                return new CorrelationReadResult(correlation, true);

            var valid = true;
            valid &= ReadCorrelationValue(carrier, DeploymentIdKey, maxValueLength, value => correlation.DeploymentId = value);
            valid &= ReadCorrelationValue(carrier, TenantIdKey, maxValueLength, value => correlation.TenantId = value);
            valid &= ReadCorrelationValue(carrier, PrincipalIdKey, maxValueLength, value => correlation.PrincipalId = value);
            valid &= ReadCorrelationValue(carrier, UserIdKey, maxValueLength, value => correlation.UserId = value);
            valid &= ReadCorrelationValue(carrier, SessionIdKey, maxValueLength, value => correlation.SessionId = value);
            valid &= ReadCorrelationValue(carrier, WorkspaceIdKey, maxValueLength, value => correlation.WorkspaceId = value);
            valid &= ReadCorrelationValue(carrier, AgentProfileIdKey, maxValueLength, value => correlation.AgentProfileId = value);
            valid &= ReadCorrelationValue(carrier, RuntimeInstanceIdKey, maxValueLength, value => correlation.RuntimeInstanceId = value);
            valid &= ReadCorrelationValue(carrier, ExecutionIdKey, maxValueLength, value => correlation.ExecutionId = value);
            valid &= ReadCorrelationValue(carrier, ExecutionCorrelationIdKey, maxValueLength, value => correlation.ExecutionCorrelationId = value);
            valid &= ReadCorrelationValue(carrier, HostCorrelationIdKey, maxValueLength, value => correlation.HostCorrelationId = value);
            valid &= ReadCorrelationValue(carrier, EventIdKey, maxValueLength, value => correlation.EventId = value);
            valid &= ReadCorrelationValue(carrier, CausationIdKey, maxValueLength, value => correlation.CausationId = value);
            return new CorrelationReadResult(correlation, valid);
        }

        private static bool ReadCorrelationValue(
            TracePropagationCarrier carrier,
            string key,
            int maxValueLength,
            Action<string> assign)
        {
            string value;
            if (!carrier.TryGetValue(key, out value))
                return true;

            if (value.Length > maxValueLength || !IsValidIdentifier(value))
                return false;

            assign(value);
            return true;
        }

        private sealed class CorrelationReadResult
        {
            public CorrelationReadResult(TraceCorrelation correlation, bool valid)
            {
                Correlation = correlation;
                Valid = valid;
            }

            public TraceCorrelation Correlation { get; private set; }
            public bool Valid { get; private set; }
        }
    }
}

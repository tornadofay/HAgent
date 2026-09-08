using System;
using System.Collections.Generic;
using System.Linq;

namespace HAgent.Models
{
    public sealed class EventEnvelope
    {
        public EventEnvelope()
        {
            Id = Guid.NewGuid().ToString("N");
            Type = string.Empty;
            Source = EventSource.Application;
            SourceId = string.Empty;
            OccurredAt = DateTimeOffset.UtcNow;
            CorrelationId = string.Empty;
            CausationId = string.Empty;
            Importance = EventImportance.Normal;
            Scope = EventScope.Global;
            ScopeId = string.Empty;
            Identity = new AgentIdentityContext();
            PayloadJson = string.Empty;
            Context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            TraceContext = null;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public EventSource Source { get; set; }
        public string SourceId { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public string CorrelationId { get; set; }
        public string CausationId { get; set; }
        public EventImportance Importance { get; set; }
        public EventScope Scope { get; set; }
        public string ScopeId { get; set; }
        public AgentIdentityContext Identity { get; set; }
        public string PayloadJson { get; set; }
        public IDictionary<string, string> Context { get; set; }

        /// <summary>
        /// Optional provider-neutral trace propagation context. It is correlation metadata,
        /// not an authorization or domain payload field.
        /// </summary>
        public TraceContext TraceContext { get; set; }

        public EventEnvelope Clone()
        {
            var clone = new EventEnvelope
            {
                Id = Id,
                Type = Type,
                Source = Source,
                SourceId = SourceId,
                OccurredAt = OccurredAt,
                CorrelationId = CorrelationId,
                CausationId = CausationId,
                Importance = Importance,
                Scope = Scope,
                ScopeId = ScopeId,
                Identity = Identity == null ? new AgentIdentityContext() : Identity.Clone(),
                PayloadJson = PayloadJson ?? string.Empty,
                TraceContext = TraceContext == null
                    ? null
                    : new TraceContext(TraceContext.TraceId, TraceContext.ParentSpanId, TraceContext.Sampled),
                Context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };

            if (Context != null)
            {
                foreach (var pair in Context)
                    clone.Context[pair.Key] = pair.Value;
            }
            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 256);
            Require(Type, nameof(Type), 256);
            Require(SourceId, nameof(SourceId), 256, false);
            Require(CorrelationId, nameof(CorrelationId), 256, false);
            Require(CausationId, nameof(CausationId), 256, false);
            Require(ScopeId, nameof(ScopeId), 256, false);

            if (Identity != null)
                Identity.Validate();

            if (PayloadJson != null && PayloadJson.Length > 65536)
                throw new ArgumentException("Event payload must be at most 65536 characters.", nameof(PayloadJson));

            if (Context == null)
                throw new ArgumentException("Event context cannot be null.", nameof(Context));
            if (Context.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Context), "An event may contain at most 32 context entries.");

            foreach (var pair in Context.ToList())
            {
                Require(pair.Key, nameof(Context), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentException("Event context values must be at most 2048 characters.", nameof(Context));
            }

            if (Scope != EventScope.Global && string.IsNullOrWhiteSpace(ScopeId))
                throw new ArgumentException("Non-global events require a ScopeId.", nameof(ScopeId));
        }

        private static void Require(string value, string name, int maxLength, bool required = true)
        {
            if (required && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value != null && value.Length > maxLength)
                throw new ArgumentException(name + " must be at most " + maxLength + " characters.", name);
        }
    }

    public enum EventSource
    {
        Application,
        User,
        Timer,
        Tool,
        Provider,
        Memory,
        Goal,
        AgentMessage,
        External,
        Runtime,
        System
    }

    public enum EventImportance
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum EventScope
    {
        Global,
        Deployment,
        Tenant,
        User,
        Workspace,
        Agent,
        Runtime,
        Execution
    }
}

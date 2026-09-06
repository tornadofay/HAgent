using System;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IEventDispatcher : IDisposable
    {
        Task<EventPublishResult> PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken));
        IEventSubscription Subscribe(EventSubscription subscription);
    }

    public interface IEventSubscription : IDisposable
    {
        string Id { get; }
    }

    public sealed class EventSubscription
    {
        public EventSubscription()
        {
            Id = Guid.NewGuid().ToString("N");
            Filter = new EventFilter();
            Handler = null;
        }

        public string Id { get; set; }
        public EventFilter Filter { get; set; }
        public Func<EventEnvelope, CancellationToken, Task> Handler { get; set; }
    }

    public sealed class EventFilter
    {
        public EventFilter()
        {
            EventTypes = null;
            Sources = null;
            Scopes = null;
            MinimumImportance = null;
            ScopeId = null;
        }

        public System.Collections.Generic.IReadOnlyCollection<string> EventTypes { get; set; }
        public System.Collections.Generic.IReadOnlyCollection<EventSource> Sources { get; set; }
        public System.Collections.Generic.IReadOnlyCollection<EventScope> Scopes { get; set; }
        public EventImportance? MinimumImportance { get; set; }
        public string ScopeId { get; set; }

        public bool Matches(EventEnvelope envelope)
        {
            if (envelope == null) return false;
            if (EventTypes != null && EventTypes.Count > 0)
            {
                var matched = false;
                foreach (var type in EventTypes)
                {
                    if (string.Equals(type, envelope.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched) return false;
            }

            if (Sources != null && Sources.Count > 0 && !Contains(Sources, envelope.Source)) return false;
            if (Scopes != null && Scopes.Count > 0 && !Contains(Scopes, envelope.Scope)) return false;
            if (MinimumImportance.HasValue && envelope.Importance < MinimumImportance.Value) return false;
            if (!string.IsNullOrWhiteSpace(ScopeId) && !string.Equals(ScopeId, envelope.ScopeId, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static bool Contains<T>(System.Collections.Generic.IEnumerable<T> values, T value)
        {
            foreach (var item in values)
                if (Equals(item, value)) return true;
            return false;
        }
    }

    public enum EventPublishStatus
    {
        Accepted,
        RejectedDuplicate,
        RejectedFull,
        Expired
    }

    public sealed class EventPublishResult
    {
        public EventPublishResult(EventPublishStatus status, string eventId)
        {
            Status = status;
            EventId = eventId ?? string.Empty;
        }

        public EventPublishStatus Status { get; private set; }
        public string EventId { get; private set; }
        public bool Accepted { get { return Status == EventPublishStatus.Accepted; } }
    }
}

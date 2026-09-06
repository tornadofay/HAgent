using System;

namespace HAgent.Models
{
    public sealed class EventDispatcherOptions
    {
        public EventDispatcherOptions()
        {
            Capacity = 256;
            Retention = TimeSpan.FromMinutes(10);
            DeduplicationWindow = TimeSpan.FromMinutes(5);
            Overflow = EventOverflowBehavior.Wait;
            MaxConcurrentHandlers = 4;
        }

        public int Capacity { get; set; }
        public TimeSpan Retention { get; set; }
        public TimeSpan DeduplicationWindow { get; set; }
        public EventOverflowBehavior Overflow { get; set; }
        public int MaxConcurrentHandlers { get; set; }

        public void Validate()
        {
            if (Capacity < 1 || Capacity > 100000)
                throw new ArgumentOutOfRangeException(nameof(Capacity), "Capacity must be between 1 and 100000.");
            if (Retention <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(Retention), "Retention must be greater than zero.");
            if (DeduplicationWindow < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(DeduplicationWindow), "DeduplicationWindow cannot be negative.");
            if (MaxConcurrentHandlers < 1 || MaxConcurrentHandlers > 64)
                throw new ArgumentOutOfRangeException(nameof(MaxConcurrentHandlers), "MaxConcurrentHandlers must be between 1 and 64.");
        }
    }

    public enum EventOverflowBehavior
    {
        Wait,
        Reject
    }
}

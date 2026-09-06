using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;

namespace HAgent.Runtime
{
    public sealed class InMemoryEventDispatcher : IEventDispatcher
    {
        private sealed class PendingEvent
        {
            public EventEnvelope Envelope;
            public TaskCompletionSource<EventPublishResult> Completion;
        }

        private sealed class Subscription : IEventSubscription
        {
            private readonly InMemoryEventDispatcher _owner;
            private int _disposed;
            internal readonly EventSubscription Definition;

            public Subscription(InMemoryEventDispatcher owner, EventSubscription definition)
            {
                _owner = owner;
                Definition = definition;
            }

            public string Id { get { return Definition.Id; } }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                    _owner.RemoveSubscription(Id);
            }
        }

        private readonly EventDispatcherOptions _options;
        private readonly ConcurrentQueue<PendingEvent> _queue = new ConcurrentQueue<PendingEvent>();
        private readonly SemaphoreSlim _items = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _slots;
        private readonly ConcurrentDictionary<string, Subscription> _subscriptions = new ConcurrentDictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, DateTimeOffset> _dedupe = new ConcurrentDictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);
        private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
        private readonly List<Task> _workers = new List<Task>();
        private int _disposed;

        public InMemoryEventDispatcher(EventDispatcherOptions options = null)
        {
            _options = options ?? new EventDispatcherOptions();
            _options.Validate();
            _slots = new SemaphoreSlim(_options.Capacity, _options.Capacity);

            for (var i = 0; i < _options.MaxConcurrentHandlers; i++)
                _workers.Add(Task.Run(ProcessLoopAsync));
        }

        public IEventSubscription Subscribe(EventSubscription subscription)
        {
            ThrowIfDisposed();
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (subscription.Handler == null) throw new ArgumentException("Event subscription handler is required.", nameof(subscription));
            if (string.IsNullOrWhiteSpace(subscription.Id)) subscription.Id = Guid.NewGuid().ToString("N");
            if (subscription.Filter == null) subscription.Filter = new EventFilter();

            var live = new Subscription(this, new EventSubscription
            {
                Id = subscription.Id.Trim(),
                Filter = subscription.Filter,
                Handler = subscription.Handler
            });

            if (!_subscriptions.TryAdd(live.Id, live))
                throw new InvalidOperationException("An event subscription with the same ID already exists: " + live.Id);
            return live;
        }

        public async Task<EventPublishResult> PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            var copy = envelope.Clone();
            copy.Validate();

            var now = DateTimeOffset.UtcNow;
            PruneDedupe(now);
            DateTimeOffset existing;
            if (_dedupe.TryGetValue(copy.Id, out existing) && now - existing <= _options.DeduplicationWindow)
                return new EventPublishResult(EventPublishStatus.RejectedDuplicate, copy.Id);

            if (_options.Retention <= now - copy.OccurredAt)
                return new EventPublishResult(EventPublishStatus.Expired, copy.Id);

            if (_options.Overflow == EventOverflowBehavior.Reject)
            {
                if (!_slots.Wait(0))
                    return new EventPublishResult(EventPublishStatus.RejectedFull, copy.Id);
            }
            else
            {
                await _slots.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            _dedupe[copy.Id] = now;
            _queue.Enqueue(new PendingEvent
            {
                Envelope = copy,
                Completion = new TaskCompletionSource<EventPublishResult>(TaskCreationOptions.RunContinuationsAsynchronously)
            });
            _items.Release();

            return await Task.FromResult(new EventPublishResult(EventPublishStatus.Accepted, copy.Id)).ConfigureAwait(false);
        }

        private async Task ProcessLoopAsync()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                try
                {
                    await _items.WaitAsync(_shutdown.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                PendingEvent pending;
                if (!_queue.TryDequeue(out pending))
                    continue;

                _slots.Release();
                var envelope = pending.Envelope;
                if (DateTimeOffset.UtcNow - envelope.OccurredAt > _options.Retention)
                    continue;

                var handlers = _subscriptions.Values
                    .Where(x => x.Definition.Filter == null || x.Definition.Filter.Matches(envelope))
                    .ToArray();

                foreach (var subscription in handlers)
                {
                    try
                    {
                        await subscription.Definition.Handler(envelope.Clone(), _shutdown.Token).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Individual handlers cannot terminate the dispatcher or other subscriptions.
                    }
                }
            }
        }

        private void RemoveSubscription(string id)
        {
            Subscription ignored;
            _subscriptions.TryRemove(id, out ignored);
        }

        private void PruneDedupe(DateTimeOffset now)
        {
            if (_options.DeduplicationWindow <= TimeSpan.Zero)
            {
                _dedupe.Clear();
                return;
            }

            foreach (var pair in _dedupe)
            {
                if (now - pair.Value > _options.DeduplicationWindow)
                {
                    DateTimeOffset ignored;
                    _dedupe.TryRemove(pair.Key, out ignored);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(nameof(InMemoryEventDispatcher));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _shutdown.Cancel();
            foreach (var subscription in _subscriptions.Values)
                subscription.Dispose();
            try { Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(2)); } catch { }
            _items.Dispose();
            _slots.Dispose();
            _shutdown.Dispose();
        }
    }
}

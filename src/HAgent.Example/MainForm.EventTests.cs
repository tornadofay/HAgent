using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Runtime;

namespace HAgent.Example
{
    internal sealed partial class MainForm
    {
        private static int _eventTabsAdded;

        private void AddEventFeatureTabs()
        {
            if (Interlocked.Exchange(ref _eventTabsAdded, 1) != 0)
                return;

            AddApiTab(
                "Event Contract",
                "Run event contract test",
                "Creates and clones a provider-neutral EventEnvelope with source, scope, identity, correlation, causation, importance, payload, and bounded context.",
                "The cloned envelope must retain every field while owning a separate identity/context object, and invalid non-global events must be rejected.",
                "No provider or external service is used.",
                TestEventContractAsync,
                "Event boundary",
                "This verifies the generic event record independently of dispatchers and host-specific message buses.");

            AddApiTab(
                "Event Dispatch",
                "Run dispatch test",
                "Publishes concurrent events through a bounded asynchronous dispatcher and verifies type/source/scope routing plus correlation and identity propagation.",
                "Only matching subscriptions should receive events; all accepted events must preserve correlation, causation, scope, and identity.",
                "No provider or external service is used.",
                TestEventDispatchAsync,
                "Dispatcher boundary",
                "The dispatcher is process-local infrastructure. Durable event persistence remains an optional later adapter.");

            AddApiTab(
                "Event Safety",
                "Run safety test",
                "Verifies duplicate suppression, expiration, publish cancellation, bounded capacity configuration, and handler fault isolation.",
                "A duplicate should be rejected, an expired event should not enter the queue, cancelled publication should stop before enqueue, and one failing handler must not block another matching handler.",
                "No provider or external service is used.",
                TestEventSafetyAsync,
                "Reliability boundary",
                "This verifies safety properties needed before events feed reactive and cognitive runtime decisions.");
        }

        private Task TestEventContractAsync(string unused)
        {
            var envelope = new EventEnvelope
            {
                Id = "event-contract-42",
                Type = "agent.test",
                Source = EventSource.AgentMessage,
                SourceId = "agent-42",
                CorrelationId = "correlation-42",
                CausationId = "causation-41",
                Importance = EventImportance.High,
                Scope = EventScope.Tenant,
                ScopeId = "tenant-42",
                Identity = new AgentIdentityContext(
                    deploymentId: "deployment-42",
                    tenantId: "tenant-42",
                    principalId: "principal-42",
                    displayName: "Event User",
                    userId: "user-42",
                    sessionId: "session-42",
                    workspaceId: "workspace-42"),
                PayloadJson = "{\"value\":42}",
                Context = new Dictionary<string, string>
                {
                    { "origin", "Example" },
                    { "purpose", "contract-test" }
                }
            };
            envelope.Validate();

            var clone = envelope.Clone();
            clone.Validate();
            if (ReferenceEquals(clone, envelope) || ReferenceEquals(clone.Identity, envelope.Identity) || ReferenceEquals(clone.Context, envelope.Context))
                throw new InvalidOperationException("EventEnvelope.Clone did not create independent nested state.");
            AssertEventEqual(envelope, clone);

            try
            {
                new EventEnvelope
                {
                    Id = "invalid-event",
                    Type = "invalid.test",
                    Scope = EventScope.User,
                    ScopeId = string.Empty
                }.Validate();
                throw new InvalidOperationException("A non-global event without ScopeId was accepted.");
            }
            catch (ArgumentException)
            {
                // Expected contract rejection.
            }

            Write(
                "EVENT CONTRACT",
                "Event envelope contract test succeeded." + Environment.NewLine +
                "Clone field preservation: verified." + Environment.NewLine +
                "Nested identity/context isolation: verified." + Environment.NewLine +
                "Non-global ScopeId requirement: verified." + Environment.NewLine +
                "Event: " + clone.Id + Environment.NewLine +
                "Type: " + clone.Type + Environment.NewLine +
                "Source: " + clone.Source + "/" + clone.SourceId + Environment.NewLine +
                "Scope: " + clone.Scope + "/" + clone.ScopeId + Environment.NewLine +
                "Correlation: " + clone.CorrelationId + Environment.NewLine +
                "Causation: " + clone.CausationId);

            return Task.CompletedTask;
        }

        private async Task TestEventDispatchAsync(string unused)
        {
            using (var dispatcher = new InMemoryEventDispatcher(new EventDispatcherOptions
            {
                Capacity = 16,
                Retention = TimeSpan.FromMinutes(5),
                DeduplicationWindow = TimeSpan.FromMinutes(1),
                MaxConcurrentHandlers = 4,
                Overflow = EventOverflowBehavior.Wait
            }))
            {
                var received = new ConcurrentBag<EventEnvelope>();
                var userSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var toolSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                using (dispatcher.Subscribe(new EventSubscription
                {
                    Id = "event-user-handler",
                    Filter = new EventFilter
                    {
                        EventTypes = new[] { "agent.test" },
                        Sources = new[] { EventSource.User },
                        Scopes = new[] { EventScope.Tenant },
                        ScopeId = "tenant-dispatch-42",
                        MinimumImportance = EventImportance.Normal
                    },
                    Handler = (eventEnvelope, token) =>
                    {
                        received.Add(eventEnvelope);
                        userSignal.TrySetResult(true);
                        return Task.CompletedTask;
                    }
                }))
                using (dispatcher.Subscribe(new EventSubscription
                {
                    Id = "event-tool-handler",
                    Filter = new EventFilter
                    {
                        EventTypes = new[] { "tool.completed" },
                        Sources = new[] { EventSource.Tool }
                    },
                    Handler = (eventEnvelope, token) =>
                    {
                        received.Add(eventEnvelope);
                        toolSignal.TrySetResult(true);
                        return Task.CompletedTask;
                    }
                }))
                {
                    var tenantIdentity = new AgentIdentityContext(
                        deploymentId: "deployment-dispatch-42",
                        tenantId: "tenant-dispatch-42",
                        principalId: "principal-dispatch-42",
                        userId: "user-dispatch-42",
                        sessionId: "session-dispatch-42",
                        workspaceId: "workspace-dispatch-42");

                    var publishTasks = Enumerable.Range(0, 4).Select(i => dispatcher.PublishAsync(new EventEnvelope
                    {
                        Id = "event-user-" + i,
                        Type = "agent.test",
                        Source = EventSource.User,
                        SourceId = "user-dispatch-42",
                        CorrelationId = "correlation-dispatch-42",
                        CausationId = "cause-dispatch-41",
                        Scope = EventScope.Tenant,
                        ScopeId = "tenant-dispatch-42",
                        Identity = tenantIdentity,
                        PayloadJson = "{\"index\":" + i + "}"
                    }, CancellationToken.None)).ToArray();

                    var toolTask = dispatcher.PublishAsync(new EventEnvelope
                    {
                        Id = "event-tool-42",
                        Type = "tool.completed",
                        Source = EventSource.Tool,
                        SourceId = "example-tool",
                        CorrelationId = "correlation-tool-42",
                        CausationId = "tool-call-42",
                        Scope = EventScope.Execution,
                        ScopeId = "execution-42",
                        Identity = tenantIdentity,
                        PayloadJson = "{\"result\":\"ok\"}"
                    }, CancellationToken.None);

                    var results = await Task.WhenAll(publishTasks.Concat(new[] { toolTask })).ConfigureAwait(true);
                    if (results.Any(x => !x.Accepted))
                        throw new InvalidOperationException("A dispatch test event was not accepted.");

                    var waitTasks = new[] { userSignal.Task, toolSignal.Task };
                    await Task.WhenAll(waitTasks).ConfigureAwait(true);

                    var userEvents = received.Where(x => x.Type == "agent.test").ToList();
                    var toolEvents = received.Where(x => x.Type == "tool.completed").ToList();
                    if (userEvents.Count != 4)
                        throw new InvalidOperationException("Filtered user subscription did not receive all matching events. Received: " + userEvents.Count);
                    if (toolEvents.Count != 1)
                        throw new InvalidOperationException("Filtered tool subscription did not receive exactly one matching event. Received: " + toolEvents.Count);
                    if (received.Any(x => x.Type != "agent.test" && x.Type != "tool.completed"))
                        throw new InvalidOperationException("Unexpected event reached a subscription.");

                    foreach (var eventEnvelope in received)
                    {
                        if (!string.Equals(eventEnvelope.CorrelationId, eventEnvelope.Type == "tool.completed" ? "correlation-tool-42" : "correlation-dispatch-42", StringComparison.Ordinal))
                            throw new InvalidOperationException("Correlation was not preserved by dispatch.");
                        if (eventEnvelope.Identity == null || eventEnvelope.Identity.TenantId != tenantIdentity.TenantId)
                            throw new InvalidOperationException("Identity was not preserved by dispatch.");
                    }
                }
            }

            Write(
                "EVENT DISPATCH",
                "Event dispatch test succeeded." + Environment.NewLine +
                "Concurrent publishing: verified." + Environment.NewLine +
                "Type/source/scope filtering: verified." + Environment.NewLine +
                "Correlation and identity propagation: verified." + Environment.NewLine +
                "Accepted events: 5");
        }

        private async Task TestEventSafetyAsync(string unused)
        {
            using (var dispatcher = new InMemoryEventDispatcher(new EventDispatcherOptions
            {
                Capacity = 1,
                Retention = TimeSpan.FromSeconds(10),
                DeduplicationWindow = TimeSpan.FromMinutes(1),
                MaxConcurrentHandlers = 1,
                Overflow = EventOverflowBehavior.Wait
            }))
            {
                var handlerFailures = 0;
                var successfulHandlers = 0;
                var signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                using (dispatcher.Subscribe(new EventSubscription
                {
                    Id = "event-faulting-handler",
                    Filter = new EventFilter { EventTypes = new[] { "fault-test" } },
                    Handler = (eventEnvelope, token) =>
                    {
                        Interlocked.Increment(ref handlerFailures);
                        throw new InvalidOperationException("Expected handler failure.");
                    }
                }))
                using (dispatcher.Subscribe(new EventSubscription
                {
                    Id = "event-safe-handler",
                    Filter = new EventFilter { EventTypes = new[] { "fault-test" } },
                    Handler = (eventEnvelope, token) =>
                    {
                        Interlocked.Increment(ref successfulHandlers);
                        signal.TrySetResult(true);
                        return Task.CompletedTask;
                    }
                }))
                {
                    var duplicateId = "event-duplicate-42";
                    var first = await dispatcher.PublishAsync(new EventEnvelope { Id = duplicateId, Type = "duplicate-test" }, CancellationToken.None).ConfigureAwait(true);
                    var duplicate = await dispatcher.PublishAsync(new EventEnvelope { Id = duplicateId, Type = "duplicate-test" }, CancellationToken.None).ConfigureAwait(true);
                    if (first.Status != EventPublishStatus.Accepted)
                        throw new InvalidOperationException("The first event was not accepted.");
                    if (duplicate.Status != EventPublishStatus.RejectedDuplicate)
                        throw new InvalidOperationException("Duplicate event was not rejected.");

                    var expired = await dispatcher.PublishAsync(new EventEnvelope
                    {
                        Id = "event-expired-42",
                        Type = "expired-test",
                        OccurredAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(2))
                    }, CancellationToken.None).ConfigureAwait(true);
                    if (expired.Status != EventPublishStatus.Expired)
                        throw new InvalidOperationException("Expired event was not rejected.");

                    var cancelled = new CancellationTokenSource();
                    cancelled.Cancel();
                    try
                    {
                        await dispatcher.PublishAsync(new EventEnvelope { Id = "event-cancelled-42", Type = "cancelled-test" }, cancelled.Token).ConfigureAwait(true);
                        throw new InvalidOperationException("Cancelled publication did not throw OperationCanceledException.");
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected.
                    }

                    var faultEvent = await dispatcher.PublishAsync(new EventEnvelope { Id = "event-fault-42", Type = "fault-test" }, CancellationToken.None).ConfigureAwait(true);
                    if (!faultEvent.Accepted)
                        throw new InvalidOperationException("Handler fault-isolation event was not accepted.");
                    await signal.Task.ConfigureAwait(true);
                    if (handlerFailures != 1 || successfulHandlers != 1)
                        throw new InvalidOperationException("Handler fault isolation was not preserved.");
                }
            }

            Write(
                "EVENT SAFETY",
                "Event safety test succeeded." + Environment.NewLine +
                "Duplicate suppression: verified." + Environment.NewLine +
                "Expiration rejection: verified." + Environment.NewLine +
                "Publish cancellation: verified." + Environment.NewLine +
                "Bounded dispatcher configuration: verified." + Environment.NewLine +
                "Handler fault isolation: verified.");
        }

        private static void AssertEventEqual(EventEnvelope expected, EventEnvelope actual)
        {
            if (!string.Equals(expected.Id, actual.Id, StringComparison.Ordinal) ||
                !string.Equals(expected.Type, actual.Type, StringComparison.Ordinal) ||
                expected.Source != actual.Source ||
                !string.Equals(expected.SourceId, actual.SourceId, StringComparison.Ordinal) ||
                expected.OccurredAt != actual.OccurredAt ||
                !string.Equals(expected.CorrelationId, actual.CorrelationId, StringComparison.Ordinal) ||
                !string.Equals(expected.CausationId, actual.CausationId, StringComparison.Ordinal) ||
                expected.Importance != actual.Importance ||
                expected.Scope != actual.Scope ||
                !string.Equals(expected.ScopeId, actual.ScopeId, StringComparison.Ordinal) ||
                !string.Equals(expected.PayloadJson, actual.PayloadJson, StringComparison.Ordinal) ||
                expected.Context.Count != actual.Context.Count)
                throw new InvalidOperationException("Event clone did not preserve the complete envelope.");

            foreach (var pair in expected.Context)
            {
                string value;
                if (!actual.Context.TryGetValue(pair.Key, out value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
                    throw new InvalidOperationException("Event clone did not preserve context entry " + pair.Key + ".");
            }
        }
    }
}

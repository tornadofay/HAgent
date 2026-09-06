# First-Class Event Subsystem

## Purpose

HAgent events are provider-neutral envelopes used to carry state changes, requests, notifications, and external signals between the host, tools, providers, runtime infrastructure, and future cognitive/reactive subsystems.

The event subsystem is infrastructure. It is not a replacement for an enterprise message broker and does not assign host-domain meaning to event types.

## Event envelope

`EventEnvelope` contains the stable event identity and provenance needed to route and reason about an event:

```text
Id
Type
Source / SourceId
OccurredAt
CorrelationId
CausationId
Importance
Scope / ScopeId
Identity
PayloadJson
Context
```

`Identity` uses the same `AgentIdentityContext` propagated through execution. Correlation and causation are separate from identity and from the event ID.

Payload and context are deliberately bounded. A payload is serialized provider/host data; HAgent does not deserialize arbitrary executable objects as part of dispatch.

## Event sources

The first generic source categories are:

```text
Application
User
Timer
Tool
Provider
Memory
Goal
AgentMessage
External
Runtime
System
```

Additional categories may be introduced when they have generic HAgent meaning. Host-specific business events remain represented by the `Type` string and source metadata rather than new host-domain classes in Core.

## Scope

Events may be scoped to:

```text
Global
Deployment
Tenant
User
Workspace
Agent
Runtime
Execution
```

`Scope` describes the resource/context boundary represented by the event. It does not grant access. Authorization remains a separate policy decision using event identity, scope, requested operation, and applicable policy.

Non-global events require an explicit `ScopeId`.

## Dispatch boundary

`IEventDispatcher.PublishAsync(...)` is the canonical asynchronous publishing boundary.

Subscriptions provide bounded routing predicates over event type, source, scope, scope ID, and minimum importance. A handler receives a cloned event so a subscriber cannot mutate the dispatcher-owned event instance.

The first implementation is `InMemoryEventDispatcher`:

```text
Producer(s)
    |
    v
bounded queue
    |
    +--> worker 1 --> matching handlers
    +--> worker 2 --> matching handlers
    +--> ...
```

Queue capacity and handler concurrency are independent controls. A bounded queue limits events waiting for dispatch; `MaxConcurrentHandlers` limits simultaneous handler execution.

## Backpressure

The dispatcher supports:

- `Wait`: publishers asynchronously wait for queue capacity and honor cancellation.
- `Reject`: publishers receive `RejectedFull` when the queue is full.

Cancellation is checked before and after capacity acquisition so a cancelled producer does not leak a reserved queue slot.

## Deduplication

Event IDs are the deduplication key. Within the configured deduplication window, concurrent publishers attempting the same ID are resolved atomically so only one publication is accepted.

Deduplication is an in-process delivery protection mechanism. It is not a globally distributed exactly-once guarantee.

## Retention and expiration

`EventDispatcherOptions.Retention` defines the maximum age of an event accepted for dispatch. Expired events are rejected before queueing. Events that age past retention while queued are discarded rather than dispatched late.

The current in-memory implementation keeps no durable event history.

## Failure isolation

A failing subscriber handler does not terminate the dispatcher or prevent other matching subscribers from processing the same event.

Handler failure reporting/observability will be integrated with the later observability and policy phases rather than becoming part of the event payload contract.

## Persistence boundary

Live event queues/subscriptions are process-local runtime infrastructure. Durable event records are a separate concern and may be implemented later for selected storage backends.

This distinction prevents durable storage from becoming an implicit live message broker.

## Runtime integration

Future runtime/cognitive layers consume the same envelope:

```text
Host / Provider / Tool / Memory / Timer / Goal
                 |
                 v
           EventEnvelope
                 |
          Event Dispatcher
             /       \
        reactive    cognitive
         handling    reasoning
                 |
                 v
          AgentExecution
```

When an event starts or influences an execution, correlation and causation should be preserved into the execution request/snapshot and later audit/trace records.

## Security boundary

Events do not grant capabilities. Event identity, scope, and source are context for policy decisions. A model-generated event must never bypass authorization, tool permissions, budgets, or approval requirements.

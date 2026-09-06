# Phase 0.952 — Event Subsystem

## Status

**Planned architectural foundation before Phase 0.96 and Phase 0.97.**

## Goal

Make events a first-class provider-neutral HAgent concept so hosts, tools, runtimes, workflows, and the future Persistent Cognitive Runtime can use one generic event model.

## Requirements

1. [ ] Define `EventEnvelope` with stable event ID, type, source, timestamp, correlation/causation metadata, optional importance, and bounded payload/context.
2. [ ] Define event source and scope semantics without assuming a specific host domain.
3. [ ] Support user, application, timer, tool, provider, memory, goal, agent-message, and external events through the same generic contract.
4. [ ] Define bounded event queues, retention, expiration, and deduplication semantics.
5. [ ] Define an asynchronous event dispatch boundary with cancellation and backpressure.
6. [ ] Preserve event provenance and correlation into runtime decisions and executions.
7. [ ] Support event filtering/routing without making the event subsystem a domain-specific message bus.
8. [ ] Define persistence as optional and keep live queues/process-local handlers separate from durable event records.
9. [ ] Ensure event delivery is safe under concurrent producers and consumers.
10. [ ] Add deterministic Example verification for publishing, filtering, deduplication, expiration, bounded queues, cancellation, and correlation propagation.

## Architectural outcome

```text
Host / Provider / Tool / Runtime
            |
            v
      EventEnvelope
            |
      Event Dispatcher
       /           \
   reactive      cognitive
    handler       runtime
```

The subsystem provides generic event infrastructure; it does not become a replacement for a host's enterprise message broker.
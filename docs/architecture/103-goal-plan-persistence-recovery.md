# Goal, Intention, Plan, and Recovery Architecture

## Purpose

Phase 0.9591 establishes durable provider-neutral authority for long-lived agent goals, intentions, plans, checkpoints, and recovery state without persisting transient execution machinery.

The phase is built on the existing runtime identity and execution revision model. It does not create a second agent identity or execution system.

## Slice 1 — Goal and intention contracts

Goals and intentions are distinct durable concepts.

```text
Goal      = desired durable outcome/state
Intention = an adopted commitment/choice to pursue a goal
```

A goal has a stable identity independent of any intention that pursues it. An intention references its goal by stable goal ID and has its own stable identity and revision history.

### Goal contract

A goal carries:

- stable ID;
- title and description;
- explicit lifecycle status;
- priority;
- host/runtime-relevant constraints;
- provenance;
- creation/update timestamps;
- monotonic revision metadata.

### Intention contract

An intention carries:

- stable ID;
- referenced goal ID;
- description;
- explicit lifecycle status;
- priority;
- constraints;
- provenance;
- creation/update timestamps;
- optional adoption timestamp;
- monotonic revision metadata.

### Authority and provenance

Host-supplied goal state and agent-inferred goal state must remain distinguishable.

```text
HostSupplied   = authoritative host input
AgentInferred  = agent-derived proposition/evidence
```

Agent inference must never silently acquire host-authoritative meaning merely because it is represented by the same goal contract.

### Intention status history

An intention status change is an explicit immutable record containing:

- intention ID;
- resulting revision;
- previous and new status;
- mandatory reason;
- optional evidence;
- timestamp;
- authority/provenance of the change.

This provides an attributable explanation for adoption, suspension, revision, completion, failure, abandonment, and supersession without overwriting the historical reason.

## Durable-state boundary

The phase will persist durable cognitive authority through the existing HAgent storage abstraction in later slices. Transient process machinery is not durable cognitive state:

```text
live Tasks
CancellationToken / synchronization primitives
HTTP clients
provider sessions
active sockets
in-process delegates
live runtime objects
```

A newer durable revision must be able to invalidate stale asynchronous work. Recovery never revives obsolete provider or execution authority.

## Host neutrality

Goals and intentions remain provider-neutral. HAgent may represent host-supplied state and agent-derived proposals, but authorization and host side effects remain outside Core unless exposed through an explicit generic host-owned contract.

## Validation invariants

Contracts fail closed for missing stable IDs, required descriptions/titles, unsupported enum values, missing provenance, invalid negative revisions, missing timestamps, and invalid adoption ordering.

Identity, provenance, revision, and status metadata are part of the public contract and remain independent of persistence implementation details.

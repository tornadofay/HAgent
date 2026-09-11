# Phase 0.97 — Persistent Cognitive Runtime

## Status

**Planned after 0.96 and before higher-level autonomous-agent features.**

## Purpose

Add an optional long-lived cognitive runtime above ordinary HAgent executions.

A Persistent Cognitive Runtime lets one runtime agent remain active over time: receive observations/events, maintain state, pursue goals, use plans and Skills, remember relevant experience, decide when deterministic behavior is sufficient, request bounded reasoning when necessary, execute through the existing execution engine, and incorporate validated outcomes.

It is a production mechanism layer, not an implementation of BDI, SOAR, ACT-R, Global Workspace Theory, LIDA, ReAct, Reflexion, MemGPT, Voyager, consciousness, or AGI.

## Core production invariant — single authoritative owner

Each runtime agent instance owns its authoritative cognitive state.

```text
Agent A runtime instance
    ├── authoritative state owner A
    ├── event queue A
    ├── asynchronous work A
    └── state mutations applied only by owner A

Agent B runtime instance
    ├── authoritative state owner B
    ├── event queue B
    ├── asynchronous work B
    └── state mutations applied only by owner B
```

Asynchronous LLM/tool/retrieval work may run concurrently, but it never writes cognitive state directly. It returns a typed result/event to the owning runtime, which validates lifecycle, revision, policy, and applicability before applying the mutation.

Per-agent state mutation is serialized. Independent runtime agents remain concurrently schedulable and must not become a single shared cognitive bottleneck.

This is the authoritative concurrency model for V1.

## Architecture boundary

```text
Host / Environment
        ↓
Events / Observations
        ↓
Persistent Cognitive Runtime
        ├── state ownership
        ├── decision workspace
        ├── goals / intentions
        ├── plans / methods
        ├── reactive decisions
        ├── bounded deliberation
        ├── learning integration
        └── runtime lifecycle
        ↓
Reasoning Requirement
        ↓
Phase 0.96 Execution Planner
        ↓
Existing Execution Engine
```

The host remains authoritative over domain truth, host scheduling policy, permissions, external side effects, and business state.

## Delivery slices

### Slice 1 — Single-owner cognitive state and revisions

- Define the authoritative cognitive-state contract owned by one runtime agent.
- Provide immutable/read-only snapshots for consumers.
- Define a monotonic cognitive revision.
- Serialize state mutation per runtime agent.
- Allow asynchronous work to return events/results to the owner.
- Reject stale, cancelled, retired, shutdown, or superseded results.
- Preserve source/cause/correlation metadata.
- Verify multiple independent runtime agents can mutate concurrently without sharing state.

**Important:** V1 does not require a general multi-writer proposal/merge engine. The owner is the sole state authority.

### Slice 2 — Observations, beliefs, and bounded decision workspace

- Distinguish host observations/events from inferred beliefs.
- Preserve provenance, confidence/quality, scope, freshness, validity, and revision.
- Represent ambiguity and insufficient evidence explicitly.
- Define bounded `DecisionWorkspace` selection for the current decision.
- Use deterministic relevance signals such as urgency, novelty, goal relevance, uncertainty, risk, freshness, relationship relevance, and policy importance where supplied.
- Keep workspace selection separate from prompt construction.
- Prevent event storms and workspace growth from becoming unbounded.

### Slice 3 — Goals, intentions, and reconsideration

- Define durable Goal and Intention usage over the 0.9591 persistence contracts.
- Keep Goal, Intention, and Plan/Method distinct.
- Support multiple active goals with deterministic priority/constraint policy.
- Record why an intention was adopted, retained, revised, suspended, completed, failed, abandoned, or superseded.
- Define reconsideration triggers such as invalid assumptions, failure, changed constraints, higher-priority goals, resource/policy changes, deadlines, or host intervention.
- Add anti-thrashing limits such as cooldown, reconsideration budgets, repeated-proposal detection, or no-progress thresholds.

### Slice 4 — Reactive processing and deterministic fast path

The runtime should attempt deterministic processing before model reasoning.

```text
Event / current state
        ↓
Check current plan/operators/Skills
        ↓
Check preconditions + policy + resources
        ↓
Safe deterministic action?
    yes → apply
    no  → reasoning assessment
```

Deterministic behavior may advance a plan, update working state, emit an event, invoke a governed tool, mark a resource stale, wait/sleep/wake, or create an impasse.

Routine events should not consume an LLM merely because the runtime is active.

### Slice 5 — Deliberation and Reasoning Requirement

- Define provider-neutral reasoning requirements.
- Support required/preferred capabilities, context needs, structured output, tools, latency tolerance, cost policy, and bounded reasoning depth.
- Support staged escalation from deterministic processing to bounded reasoning.
- Allow explicit `NoModelRequired` outcomes.
- Keep provider/model names out of cognition.
- Send concrete execution selection only through 0.96.
- Bound model calls, time, tokens/usage, retrieval, and recursion.
- Treat incomplete deliberation as a typed outcome rather than false success.

### Slice 6 — Plans, methods, execution, and recovery integration

- Consume durable Goal/Plan/Checkpoint/Recovery contracts from 0.9591.
- Support plan steps, assumptions, preconditions, expected effects, checkpoints, and explicit outcome states.
- Continue valid plans without unnecessary re-deliberation.
- Reconsider when assumptions/resources/policy change.
- Distinguish `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`.
- Propagate stale-result and lifecycle protection into plan execution.
- Use existing execution/runtime intervention boundaries rather than creating a second execution engine.

### Slice 7 — Experience, learning, and learned-resource reliability

- Capture bounded experience records from meaningful completed interactions.
- Keep Experience distinct from Memory, Knowledge, Skill, Policy, and raw execution logs.
- Produce procedural learning candidates conservatively from sufficient evidence.
- Preserve positive/negative evidence, applicability conditions, provenance, expected effects, and source execution/runtime identity.
- Send candidates through 0.9575 learning governance and promotion.
- Consume 0.9576 reliability/applicability results after promotion.
- Never make a single successful trajectory silently authoritative.

### Slice 8 — Lifecycle, persistence, observability, and production verification

- Consume 0.958 lifecycle/health semantics.
- Consume 0.9591 durable goals/plans/recovery.
- Support cancellation, suspension, retirement, shutdown, restart recovery, and post-retirement stale-result rejection.
- Preserve per-agent state ownership during all asynchronous work.
- Emit structured telemetry for event intake, workspace selection, deterministic actions, deliberation, reasoning requirements, plan progress, execution correlation, learning candidates, interventions, and recovery.
- Support evaluation of correctness, unnecessary LLM use, latency, cost, failure/recovery, and learning reliability.
- Verify concurrency with many independent runtime agents, not only one shared state under contention.

## Production V1 invariants

1. One runtime agent instance has one authoritative state owner.
2. Only the owner applies authoritative state mutations.
3. Background work returns results/events; it never writes owner state directly.
4. State mutation for one agent is serialized.
5. Different agent instances may operate concurrently without sharing mutable runtime state.
6. No stale asynchronous result may overwrite newer state.
7. Retirement and shutdown invalidate outstanding cognitive authority.
8. Request-oriented `ExecuteAsync` remains a first-class public API.
9. Deterministic processing is preferred when sufficient.
10. 0.96 is the only concrete provider/model execution-selection layer.
11. Learned resources are governed by 0.9575/0.9576 rather than a second learning architecture.
12. Host/domain truth and external side effects remain host-authoritative.
13. All queues, workspaces, deliberation, recursion, and resource growth are bounded.
14. Model output is evidence/request input, never authorization.

## Dependency graph

```text
0.9575 governed resources + learning
        ↓
0.9576 learned-resource reliability
        ↓
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
        ↓
0.959 intervention
        ↓
0.9592 provider/adapters
        ↓
0.96.x configuration/storage
        ↓
0.96 capability-aware execution
        ↓
0.97 persistent cognitive runtime
```

## Relationship to HWorld and other hosts

HWorld may host many independent runtime agents, for example one agent per NPC. The runtime architecture must support concurrent operation of those independent agents without embedding HWorld concepts into HAgent.

The same runtime also supports ordinary desktop applications, automation, analysis, or other hosts that need a persistent agent.

## Exit criterion

A host can create a long-lived runtime agent that independently owns its cognitive state, receives and processes events, uses deterministic behavior before unnecessary model calls, pursues persistent goals and plans, requests bounded reasoning through 0.96, learns through governed candidates, survives cancellation/restart/lifecycle transitions, and operates concurrently with many other independent runtime agents without shared-state corruption or a shared cognitive bottleneck.

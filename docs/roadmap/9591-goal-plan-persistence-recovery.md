# Phase 0.9591 — Goal/Plan Persistence and Recovery

## Status

**Ordered before 0.959 Human-in-the-Loop / Intervention; planned foundation.**

This phase is intentionally moved ahead of 0.959 in the roadmap because durable goals, intentions, plans, plan-step revisions, checkpoints, and recovery state provide the persistent authority that later goal/plan intervention should govern safely.

## Goal

Make long-lived agent goals, intentions, plans, checkpoints, and recovery state durable without making transient executions or provider sessions part of persistent cognitive state.

## Requirements

1. [ ] Define durable Goal, Intention, Plan, PlanStep, checkpoint, and recovery metadata contracts.
2. [ ] Separate durable cognitive state from live execution tasks, cancellation tokens, provider sessions, HTTP state, and synchronization primitives.
3. [ ] Define plan revision/version semantics so stale executions cannot overwrite newer goals or plans.
4. [ ] Support partial plan execution and explicit step states.
5. [ ] Define checkpoint boundaries and durable progress records.
6. [ ] Define idempotency semantics for retried plan steps and externally observable actions.
7. [ ] Distinguish safe retry, unknown outcome, and completed outcome states.
8. [ ] Support recovery after process restart, crash, timeout, cancellation, or provider failure.
9. [ ] Reconcile in-flight executions during recovery and invalidate obsolete execution authority.
10. [ ] Support plan suspension, resumption, replacement, abandonment, and rollback/compensation metadata where applicable.
11. [ ] Keep host side effects authoritative; HAgent may persist intent and requested action state but must not claim external side effects occurred without evidence.
12. [ ] Support optional persistence backends through the HAgent storage abstraction.
13. [ ] Add deterministic Example verification for checkpoints, restart recovery, stale revisions, duplicate/retry handling, unknown outcomes, and plan supersession.

## Architectural outcome

```text
Goal / Intention
      ↓
     Plan
      ↓
 checkpoints / revisions
      ↓
 Execution
      ↓
 outcome evidence
      ↓
 durable progress / recovery state
```

Durability provides recovery semantics; it does not guarantee exactly-once execution of arbitrary host side effects.

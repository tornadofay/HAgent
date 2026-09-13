# Phase 0.9591 — Goal/Plan Persistence and Recovery

## Status

**PLANNED after 0.958 and before 0.959 intervention.**

Slice 1 goal/intention contracts are **BUILT AHEAD OF ROADMAP — VERIFICATION COMPLETE; PHASE ENTRY PENDING**. The implementation remains preserved in the repository, but this does not make 0.9591 current or close the phase. When 0.9591 becomes the active phase, Slice 1 should be revisited for any phase-entry revisions and the remaining phase-level persistence/recovery verification before the phase is considered complete.

## Purpose

Make long-lived agent goals, intentions, plans, checkpoints, and recovery state durable without persisting transient execution machinery.

This phase establishes the durable authority that later intervention and Persistent Cognitive Runtime consume.

## V1 boundary

Persist:

```text
Goal
Intention
Plan
PlanStep
Checkpoint
Recovery record
Revision metadata
Outcome evidence
```

Do not persist as cognitive state:

```text
live Tasks
CancellationToken / synchronization primitives
HTTP clients
provider sessions
active sockets
in-process delegates
live runtime objects
```

## Delivery slices

### Slice 1 — Durable goal/intention contracts — BUILT AHEAD OF ROADMAP; VERIFICATION COMPLETE; PHASE ENTRY PENDING

- Define stable IDs, status, priority, constraints, provenance, timestamps, and revision metadata.
- Keep goal identity separate from intention identity.
- Record why an intention was adopted, suspended, revised, completed, failed, abandoned, or superseded.
- Preserve host-supplied goal state without pretending inferred belief is host truth.

Implementation surface: `src/HAgent.Core/Models/AiGoalContracts.cs`, focused tests in `tests/HAgent.Tests/GoalIntentionContractsTests.cs`, and matching Example `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS`.

Verification checkpoint completed on 2026-09-13:

- .NET Framework 4.8.1 Example: `GOAL & INTENTION CONTRACTS` succeeded.
- .NET 9 Example: `GOAL & INTENTION CONTRACTS` succeeded.
- Full `.NET 9` `HAgent.Tests`: **270/270 passed, 0 failed, 0 skipped**.
- Verified boundaries include separate host/inferred goal identity, explicit `HostSupplied`/`AgentInferred` authority, adopted intention status, revision metadata, and preservation of intention status-change reason.

This checkpoint does **not** close 0.9591 Slice 1 as a phase milestone. It records that the implementation exists and has passed its current contract verification ahead of the ordered roadmap. When 0.9591 becomes current, review the contract against the then-authoritative architecture and complete any required phase-entry revision before advancing to Slice 2.

### Slice 2 — Durable plans and steps

- Define plan identity/version and ordered or explicitly related steps.
- Capture preconditions, assumptions, expected effects, dependencies, status, and provenance.
- Define step states sufficient for partial progress.
- Record plan revisions without mutating history invisibly.

### Slice 3 — Checkpoints and outcome semantics

- Define explicit checkpoint boundaries.
- Persist durable progress at safe points.
- Distinguish `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`.
- Never convert timeout/provider failure into success without evidence.

### Slice 4 — Retry and idempotency

- Define stable operation/step identity for retry correlation.
- Distinguish safe retry from unknown external outcome.
- Record whether an action was requested, observed completed, or remains unknown.
- Keep external side effects host-authoritative; HAgent cannot claim exactly-once execution of arbitrary host actions.

### Slice 5 — Restart and recovery

- Recover the latest durable goal/plan revision after process restart or crash.
- Invalidate in-flight authority belonging to the previous process/runtime execution.
- Reconcile incomplete steps into safe states such as retryable, unknown, blocked, or requiring host review.
- Preserve enough evidence to explain recovery decisions.

### Slice 6 — Persistence backends and verification

- Reuse the existing HAgent storage abstraction.
- Keep File, SQL Server, and MySQL behavior aligned where each backend is supported by the current milestone.
- Verify checkpoint creation, restart recovery, stale revisions, duplicate retries, unknown outcomes, plan supersession, cancellation, and crash-safe recovery.

## Architectural rules

1. Durable state is owned by the runtime agent; storage is only the persistence mechanism.
2. A newer goal/plan revision invalidates stale asynchronous work.
3. Recovery never revives obsolete provider/execution authority.
4. Persistence does not guarantee exactly-once external side effects.
5. Recovery decisions are attributable to evidence and policy.
6. Do not introduce a second plan model for intervention or cognition.
7. Keep the phase host-neutral; host side effects remain outside Core.

## Dependency order

```text
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
        ↓
0.959 intervention
        ↓
0.97 persistent cognition consumes these contracts
```

## Exit criterion

Goals, intentions, plans, steps, checkpoints, and recovery state survive process restart through the supported storage boundary, stale work cannot overwrite newer durable revisions, unknown external outcomes remain explicit, and recovery produces a safe self-consistent state without claiming unsupported exactly-once guarantees.

# Phase 0.9591 — Goal/Plan Persistence and Recovery

## Status

**CURRENT — Slices 1–3 verified/closed; Slice 4 current.**

Phase 0.958 Agent Lifecycle and Health Management is closed/verified. Slice 1 of 0.9591 was built ahead of roadmap and is now formally accepted as the phase foundation after phase-entry review.

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

### Slice 1 — Durable goal/intention contracts — CLOSED / VERIFIED

- Define stable IDs, status, priority, constraints, provenance, timestamps, and revision metadata.
- Keep goal identity separate from intention identity.
- Record why an intention was adopted, suspended, revised, completed, failed, abandoned, or superseded.
- Preserve host-supplied goal state without pretending inferred belief is host truth.

Implementation surface: `src/HAgent.Core/Models/AiGoalContracts.cs`, focused tests in `tests/HAgent.Tests/GoalIntentionContractsTests.cs`, and matching Example `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS`.

**Verified on 2026-09-13:** .NET Framework 4.8.1 Example and .NET 9 Example both succeeded; full `.NET 9` `HAgent.Tests` reported **270/270 passed, 0 failed, 0 skipped**.

### Slice 2 — Durable plans and steps — CLOSED / VERIFIED

- Define stable plan identity, goal/intention linkage, status, revision metadata, provenance, and revision reason.
- Define explicit ordered/dependency-linked `AiPlanStep` records.
- Capture preconditions, assumptions, expected effects, failure conditions, completion criteria, step status, and step provenance.
- Validate duplicate step identities, foreign plan ownership, and self-dependencies at the plan boundary.
- Preserve nested plan/step state through detached cloning.

Implementation surface: `src/HAgent.Core/Models/AiPlanContracts.cs`; focused tests: `tests/HAgent.Tests/PlanContractsTests.cs`; Example scenario: `src/HAgent.Example/MainForm.PlanContractsTests.cs`.

**Verified on 2026-09-13:** .NET Framework 4.8.1 Example and .NET 9 Example both succeeded. Full `.NET 9` `HAgent.Tests` reported **290/290 passed, 0 failed, 0 skipped**.

### Slice 3 — Checkpoints and outcome semantics — CLOSED / VERIFIED

- Define explicit checkpoint boundaries.
- Define durable progress evidence at safe points through provider-neutral checkpoint contracts; actual backend persistence remains later phase scope.
- Distinguish `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`.
- Never convert timeout/provider failure into success without evidence.

Implementation surface: `src/HAgent.Core/Models/AiCheckpointOutcomeContracts.cs`; focused tests: `tests/HAgent.Tests/CheckpointOutcomeContractsTests.cs`; Example: `src/HAgent.Example/MainForm.CheckpointOutcomeContractsTests.cs`.

**Verified on 2026-09-13:** .NET Framework 4.8.1 Example and .NET 9 Example both succeeded. The full `.NET 9` `HAgent.Tests` regression reported **294/294 passed, 0 failed, 0 skipped**.

### Slice 4 — Retry and idempotency — CURRENT / IMPLEMENTING

- Define stable operation identity linked to plan, step, and plan revision; retry attempts do not create a new operation identity.
- Distinguish safe retry from unknown external outcome.
- Record whether an action is requested, observed completed, failed, cancelled, superseded, or remains unknown.
- Require explicit host confirmation before retrying a failed operation; requested and unknown outcomes require reconciliation first.
- Keep external side effects host-authoritative; HAgent cannot claim exactly-once execution of arbitrary host actions.

Implementation surface: `src/HAgent.Core/Models/AiPlanRetryIdempotencyContracts.cs`; focused tests: `tests/HAgent.Tests/PlanRetryIdempotencyContractsTests.cs`; Example: `HAgent.Example → Cognition → Goals & Plans → RETRY & IDEMPOTENCY`.

**Verification checkpoint:** Run the matching Example on .NET Framework 4.8.1 and .NET 9 Windows, then the focused Slice 4 tests followed by the full `HAgent.Tests` regression suite.

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

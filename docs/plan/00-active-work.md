# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 6 implementation in progress
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`
- **Scope:** Establish durable goal, intention, plan, checkpoint, outcome, retry, idempotency, restart/recovery persistence across the supported HAgent storage boundary without persisting transient execution machinery.

## 0.958 checkpoint closed

Phase 0.958 is fully verified and closed. The final full .NET 9 regression suite reported **286/286 tests passed, 0 failed, 0 skipped**. Slice 4 is recorded as **CLOSED / VERIFIED** in `docs/roadmap/958-agent-lifecycle-health.md`.

## 0.9591 Slice 1 checkpoint closed

Durable goal/intention contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> GOAL & INTENTION CONTRACTS`. Full .NET 9 checkpoint: **270/270 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 2 checkpoint closed

Durable plan/step contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> PLAN CONTRACTS`. Full .NET 9 checkpoint: **290/290 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 3 checkpoint closed

Checkpoint and outcome contracts are fully verified on .NET Framework 4.8.1 and .NET 9. Example: `HAgent.Example -> Cognition -> Goals & Plans -> CHECKPOINT & OUTCOME CONTRACTS`. Full .NET 9 checkpoint: **294/294 passed, 0 failed, 0 skipped**.

The verified contract explicitly distinguishes `Completed` from `UnknownOutcome`; unknown external outcomes are not treated as success.

## 0.9591 Slice 4 checkpoint closed

Retry and idempotency contracts are fully verified on .NET Framework 4.8.1 and .NET 9. Example: `HAgent.Example -> Cognition -> Goals & Plans -> RETRY & IDEMPOTENCY`. Full .NET 9 checkpoint: **300/300 passed, 0 failed, 0 skipped**.

The verified boundary keeps operation identity stable across attempts, allows failed-operation retry only with explicit host safety confirmation, requires reconciliation for requested/unknown outcomes, and does not claim exactly-once host side effects.

## 0.9591 Slice 5 checkpoint closed

Restart and recovery contracts are fully verified on .NET Framework 4.8.1 and .NET 9. Example: `HAgent.Example -> Cognition -> Goals & Plans -> RESTART & RECOVERY`. Full .NET 9 regression: **306/306 passed, 0 failed, 0 skipped**.

The verified recovery boundary preserves plan revision, invalidates previous runtime/execution authority, requires host review for requested/unknown external outcomes, and never revives terminal work.

## 0.9591 Slice 6 - Persistence backends

Current implementation boundary:
- Reuse the existing HAgent provider-neutral storage pattern rather than introducing a second plan model.
- Extend persistence for the canonical goal/intention/plan/checkpoint/outcome/retry/recovery contracts.
- Keep File, SQL Server, and MySQL behavior aligned where supported.
- Preserve optimistic revision/authority semantics so stale state cannot overwrite newer durable state.
- Verify restart recovery, stale revisions, duplicate retries, unknown outcomes, supersession, cancellation, and crash-safe recovery.

**Current status:** Slice 6 implementation in progress. The first code sub-slice is the provider-neutral durable cognition storage contract; backend adapters and Example verification follow within this Slice 6 boundary.

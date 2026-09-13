# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 5 implementation in progress
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`
- **Scope:** Establish durable goal, intention, plan, checkpoint, outcome, retry, and idempotency contracts, then safe restart/recovery and persistence without persisting transient execution machinery.

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

## 0.9591 Slice 5 - Restart and recovery

Current implementation boundary:
- Recover the latest durable goal/plan revision after process restart or crash without reviving obsolete execution/provider authority.
- Invalidate work owned by the previous process/runtime execution.
- Reconcile incomplete steps into safe states such as retryable, unknown, blocked, or requiring host review.
- Preserve evidence explaining the recovery decision.
- Keep the recovery boundary host-neutral; persistence backend implementation remains Slice 6.

**Current status:** Slice 5 is the active implementation slice. Build the focused contracts/tests and matching Example first; do not begin Slice 6 until Slice 5 verification is recorded.

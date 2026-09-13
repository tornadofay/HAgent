# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 4 implementation in progress
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`
- **Scope:** Establish durable goal, intention, plan, checkpoint, outcome, retry, and idempotency contracts, then persistence and recovery without persisting transient execution machinery.

## 0.958 checkpoint closed

Phase 0.958 is fully verified and closed. The final full .NET 9 regression suite reported **286/286 tests passed, 0 failed, 0 skipped**. Slice 4 is recorded as **CLOSED / VERIFIED** in `docs/roadmap/958-agent-lifecycle-health.md`.

## 0.9591 Slice 1 checkpoint closed

Durable goal/intention contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> GOAL & INTENTION CONTRACTS`. Full .NET 9 checkpoint: **270/270 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 2 checkpoint closed

Durable plan/step contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> PLAN CONTRACTS`. Full .NET 9 checkpoint: **290/290 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 3 checkpoint closed

Checkpoint and outcome contracts are fully verified on .NET Framework 4.8.1 and .NET 9. Example: `HAgent.Example -> Cognition -> Goals & Plans -> CHECKPOINT & OUTCOME CONTRACTS`. Full .NET 9 checkpoint: **294/294 passed, 0 failed, 0 skipped**.

The verified contract explicitly distinguishes `Completed` from `UnknownOutcome`; unknown external outcomes are not treated as success.

## 0.9591 Slice 4 - Retry and idempotency

The current implementation defines stable plan-step operation identity, attempt metadata, explicit operation states, and bounded retry decisions. A failed operation requires host confirmation of retry safety; requested and unknown outcomes require reconciliation; completed, cancelled, and superseded operations are not retryable.

Implementation:
- `src/HAgent.Core/Models/AiPlanRetryIdempotencyContracts.cs`
- `tests/HAgent.Tests/PlanRetryIdempotencyContractsTests.cs`
- Example: `HAgent.Example -> Cognition -> Goals & Plans -> RETRY & IDEMPOTENCY`

## Verification checkpoint

**Example to run:** `HAgent.Example -> Cognition -> Goals & Plans -> RETRY & IDEMPOTENCY` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/PlanRetryIdempotencyContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite.

**Current status:** Slice 4 implementation is in progress and awaits user verification. Do not start Slice 5 until the Example and regression results are recorded.

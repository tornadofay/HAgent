# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 3 implementation complete; verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`
- **Scope:** Establish durable goal, intention, plan, checkpoint, and outcome contracts, then persistence and recovery without persisting transient execution machinery.

## 0.958 checkpoint closed

Phase 0.958 is fully verified and closed. The final full .NET 9 regression suite reported **286/286 tests passed, 0 failed, 0 skipped**. Slice 4 is recorded as **CLOSED / VERIFIED** in `docs/roadmap/958-agent-lifecycle-health.md`.

## 0.9591 Slice 1 checkpoint closed

Durable goal/intention contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> GOAL & INTENTION CONTRACTS`. Full .NET 9 checkpoint: **270/270 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 2 checkpoint closed

Durable plan/step contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> PLAN CONTRACTS`. Full .NET 9 checkpoint: **290/290 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 3 - Checkpoints and outcome semantics

Implemented the provider-neutral checkpoint/outcome contract surface in `src/HAgent.Core/Models/AiCheckpointOutcomeContracts.cs` with focused coverage in `tests/HAgent.Tests/CheckpointOutcomeContractsTests.cs` and matching Example `src/HAgent.Example/MainForm.CheckpointOutcomeContractsTests.cs`.

The contracts distinguish explicit checkpoint boundaries and the required terminal outcomes: `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`. A completed outcome requires evidence; unknown external outcomes remain explicitly non-success.

## Verification checkpoint

**Example to run:** `HAgent.Example -> Cognition -> Goals & Plans -> CHECKPOINT & OUTCOME CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/CheckpointOutcomeContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite.

**Current status:** Slice 3 is implementation-complete and awaits user verification. Do not start Slice 4 until the Example and regression results are recorded.

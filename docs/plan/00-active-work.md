# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 2 implementation complete; verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`
- **Scope:** Establish durable goal, intention, and plan contracts, then persistence and recovery without persisting transient execution machinery.

## 0.958 checkpoint closed

Phase 0.958 is fully verified and closed. The final full .NET 9 regression suite reported **286/286 tests passed, 0 failed, 0 skipped**.

## 0.9591 Slice 1 checkpoint closed

Durable goal/intention contracts are fully verified. Example: `HAgent.Example -> Cognition -> Goals & Plans -> GOAL & INTENTION CONTRACTS`. Full .NET 9 checkpoint: **270/270 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 2 - Durable plans and steps

Implemented `src/HAgent.Core/Models/AiPlanContracts.cs` with focused coverage in `tests/HAgent.Tests/PlanContractsTests.cs` and a matching scenario in `src/HAgent.Example/MainForm.PlanContractsTests.cs`.

The Example shell now explicitly registers and classifies the scenario under `Cognition -> Goals & Plans`.

## Verification checkpoint

**Example to run:** `HAgent.Example -> Cognition -> Goals & Plans -> PLAN CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/PlanContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite.

**Current status:** Slice 2 is implementation-complete and awaits user verification. Do not start Slice 3 until the Example and regression results are recorded.

# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9591 Goal/Plan Persistence and Recovery
- **Status:** Slice 2 implementation complete; Example registration/verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/16-cognitive-runtime.md` and `docs/architecture/17-cognitive-algorithms.md`
- **Scope:** Establish durable goal/intention/plan contracts and then persistence/recovery without persisting transient execution machinery.

## 0.958 checkpoint closed

Phase 0.958 Agent Lifecycle and Health Management is fully verified and closed. The user verified all four slices on .NET Framework 4.8.1 and .NET 9. The final full `.NET 9` regression suite reported **286/286 tests passed, 0 failed, 0 skipped**. Slice 4 Example path is `HAgent.Example → Runtime → Diagnostics → RUNTIME OBSERVABILITY`.

## 0.9591 Slice 1 checkpoint closed

Durable goal/intention contracts are fully verified. The user verified `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS` on both supported targets. The full `.NET 9` `HAgent.Tests` checkpoint for Slice 1 was **270/270 passed, 0 failed, 0 skipped**.

## 0.9591 Slice 2 — Durable plans and steps

The provider-neutral plan model is implemented in `src/HAgent.Core/Models/AiPlanContracts.cs` with focused coverage in `tests/HAgent.Tests/PlanContractsTests.cs` and a matching Example scenario in `src/HAgent.Example/MainForm.PlanContractsTests.cs`.

The contract covers goal/intention linkage, plan status, revision metadata, provenance, plan/step preconditions, assumptions, expected effects, completion criteria, failure conditions, explicit step sequence/dependencies, validation, and detached cloning.

## Verification checkpoint

**Example to run:** intended path `HAgent.Example → Cognition → Goals & Plans → PLAN CONTRACTS`; Example organization/registration must be completed before releasing this checkpoint for user execution.

**Tests to run:** `tests/HAgent.Tests/PlanContractsTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the phase.

**Current status:** Slice 2 implementation is committed; do not start Slice 3 until Example registration and verification are complete and recorded.

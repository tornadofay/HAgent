# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.955 Context Engineering
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Continue the ordered 0.955 context-engineering work from the Slice 2 implementation checkpoint.

## Current checkpoint

Phase 0.954 Prompt and Instruction Governance was verified and closed by the user on 2026-09-08.

- `COGNITION INSTRUCTIONS` passed on .NET Framework 4.8.1 and .NET 9.
- .NET 9 deterministic runtime coverage passed: `RUNTIME TERMINAL STATE`, `RUNTIME CONCURRENCY`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and corrected `RUNTIME SHUTDOWN`.
- .NET Framework 4.8.1 deterministic runtime coverage passed: `RUNTIME INSTANCES`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and `RUNTIME SHUTDOWN`.
- The deterministic runtime Examples corrected during Slice 5 no longer require manually selected configured-agent UI state.
- Configuration-driven `RUNTIME EXECUTION` remains a live host example and was not used as a deterministic 0.954 milestone gate.

## Current run

**Verified checkpoint/blocker — 0.955 Slice 2 implementation complete; local contract verification remains pending.**

Implemented `src/HAgent.Core/Models/ContextContracts.cs` with provider-neutral context item, provenance, scope, budget, and bounded source-request contracts, plus `src/HAgent.Core/Abstractions/IContextSource.cs` for candidate-source acquisition. Added `tests/HAgent.Tests/ContextContractTests.cs` covering structured payloads, validation bounds, metadata clone isolation, budget/request validation, and missing metadata rejection.

A preliminary `ContextSnapshot` type was deliberately removed from Slice 2 because immutable assembled execution snapshots belong to Slice 3. No ranking, compaction, caching, WinForms integration, or provider transport was implemented.

## Required next action

Run the focused `HAgent.Tests` contract tests/build locally against the repository state after commit `09712dfc1fc732f04d82673783cf4e3e79c4cc15`. Record the actual result in `docs/plan/20-active.md`. Do not advance to 0.955 Slice 3 until Slice 2 verification passes.

## Current blockers

This connected session can inspect and modify repository sources but cannot execute the local .NET/WinForms test environment. Therefore no Slice 2 local test success is claimed here.

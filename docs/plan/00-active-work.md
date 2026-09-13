# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.958 Agent Lifecycle and Health Management
- **Status:** Slice 1 implementation corrected after local verification failures; verification pending rerun
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/102-runtime-lifecycle-health.md`
- **Scope:** Extend the existing runtime lifecycle for long-lived operation with `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`; preserve execution identity, lifecycle revision, and stale-result protection; prevent non-operational runtimes from originating ordinary new work; keep health, authorization, provider health, and durable cognitive recovery as separate concerns.

## 0.9576 checkpoint closed

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. The user verified the five matching Learning Examples on both .NET Framework 4.8.1 and .NET 9. The user reported the full `.NET 9` `HAgent.Tests` suite at **260/260 passed, 0 failed, 0 skipped** after Slice 5.

Verified boundaries include applicability, reliability evidence, adaptation/revalidation, archival/forgetting, and runtime learned-resource integration. Published resource versions remain unchanged and learned-resource reliability remains distinct from authorization.

## 0.958 implementation checkpoint

Slice 1 extends the existing `AgentRuntimeInstance` rather than introducing a parallel runtime model. Lifecycle revision is a distinct authority from per-execution revision and is persisted with runtime state across File, SQL Server, and MySQL stores.

Lifecycle transition and admission contracts are covered by `tests/HAgent.Tests/RuntimeLifecycleTests.cs`. The Example is registered through the explicit architecture path `HAgent.Example → Runtime → Runtime Instances → RUNTIME LIFECYCLE` and exercises transitions, non-active admission rejection, stale-result invalidation, persistence/restore, and shutdown cancellation.

Local verification exposed two defects in the Slice 1 verification implementation: runtime executions were not carrying the lifecycle revision captured at admission, and the invalid-transition test incorrectly treated `Active → Retired` as invalid even though the authoritative lifecycle architecture defines it as valid. Both were corrected without changing the documented lifecycle transition contract.

A dedicated `.github/workflows/verify-phase-0-958-slice-1.yml` workflow builds Core and Example for .NET Framework 4.8.1 and .NET 9 Windows, runs the focused lifecycle tests, and runs the full .NET 9 regression suite on `master` pushes.

## Verification checkpoint

**Example to run:** `HAgent.Example → Runtime → Runtime Instances → RUNTIME LIFECYCLE` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeLifecycleTests.cs` focused first, then the full `HAgent.Tests` regression suite required by 0.958.

**Current status:** corrected implementation is committed directly to `master`, but Slice 1 is not closed until the focused tests, full regression suite, and both required Example targets are actually rerun and recorded.

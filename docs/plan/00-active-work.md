# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.958 Agent Lifecycle and Health Management
- **Status:** Slice 1 ready to implement
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/102-runtime-lifecycle-health.md`
- **Scope:** Extend the existing runtime lifecycle for long-lived operation with `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`; preserve execution identity, lifecycle revision, and stale-result protection; prevent non-operational runtimes from originating ordinary new work; keep health, authorization, provider health, and durable cognitive recovery as separate concerns.

## 0.9576 checkpoint closed

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. The user verified the five matching Learning Examples on both .NET Framework 4.8.1 and .NET 9. The user reported the full `.NET 9` `HAgent.Tests` suite at **260/260 passed, 0 failed, 0 skipped** after Slice 5.

Verified boundaries include applicability, reliability evidence, adaptation/revalidation, archival/forgetting, and runtime learned-resource integration. Published resource versions remain unchanged and learned-resource reliability remains distinct from authorization.

## 0.958 documentation checkpoint

The lifecycle/health target architecture is now documented in `docs/architecture/102-runtime-lifecycle-health.md`. `docs/architecture/10-runtime.md` and `docs/roadmap/30-agent-runtime.md` explicitly distinguish the foundational 0.9 lifecycle from the long-lived 0.958 extension.

## Slice 1 verification checkpoint

**Example to run:** `HAgent.Example →` the new architecture-classified runtime lifecycle Example for Slice 1 on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 runtime-lifecycle test class/file, followed by the full `HAgent.Tests` regression suite required by 0.958.

The exact final Example title and focused test file must be recorded here before Slice 1 is closed.

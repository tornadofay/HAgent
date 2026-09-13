# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.958 Agent Lifecycle and Health Management
- **Status:** Slice 2 implementation pending
- **Primary source:** `docs/plan/20-active.md`
- **Architecture source:** `docs/architecture/102-runtime-lifecycle-health.md`
- **Scope:** Add provider-neutral runtime health state and bounded evidence while preserving the canonical runtime identity, lifecycle authority, execution revision, and stale-result rules.

## 0.9576 checkpoint closed

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. The user verified the five matching Learning Examples on both .NET Framework 4.8.1 and .NET 9 and reported the full `.NET 9` `HAgent.Tests` suite at **260/260 passed, 0 failed, 0 skipped** after Slice 5.

## 0.958 Slice 1 checkpoint closed

Phase 0.958 Slice 1 is fully verified. The user verified `HAgent.Example → Runtime → Runtime Instances → RUNTIME LIFECYCLE` on both .NET Framework 4.8.1 and .NET 9 Windows. The user reported **266/266 tests passed, 0 failed, 0 skipped**.

Verified boundaries include lifecycle transitions, lifecycle revision capture, non-active admission rejection, stale-result invalidation, runtime-state persistence/restore, and shutdown cancellation.

## 0.958 Slice 2 — Health state

Implement normalized runtime-health states `Healthy`, `Degraded`, `Failed`, and `Unknown` as provider-neutral evidence. Health must remain separate from lifecycle and authorization, must include bounded reason/source metadata, and must not infer failure from latency alone.

No Slice 3 or Slice 4 work is active.

## Verification checkpoint

**Example to run:** `HAgent.Example → Runtime → Runtime Instances → RUNTIME HEALTH` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeHealthTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the phase.

**Current status:** Slice 2 has not yet been implemented or verified.

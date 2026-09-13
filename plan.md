# HAgent Development Plan

> This file is a generated view from `docs/plan/`.
> Detailed durable decisions, current state, and active implementation instructions remain in the source documents.

## Current task

- **Phase:** 0.958 Agent Lifecycle and Health Management
- **Status:** Slice 1 ready to implement
- **Primary source:** `docs/plan/20-active.md`
- **Current-state source:** `docs/plan/00-current-state.md`
- **Active-work source:** `docs/plan/00-active-work.md`
- **Architecture source:** `docs/architecture/102-runtime-lifecycle-health.md`

## Entry condition

Phase 0.9576 Learned Resource Reliability + Adaptation is closed and verified through all five slices. The user verified the five matching Examples on .NET Framework 4.8.1 and .NET 9 and reported **260/260 passed, 0 failed, 0 skipped** for the full `.NET 9` `HAgent.Tests` suite after Slice 5.

## 0.958 Slice 1

Extend the existing `AgentRuntimeInstance` lifecycle for long-lived operation without creating a second runtime identity or changing execution identity semantics.

Target lifecycle:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

Slice 1 owns valid/invalid transitions, terminal shutdown behavior, work admission, lifecycle revision/stale-result protection, and preservation of runtime-owned durable state during suspension/recovery.

Health is deliberately deferred to Slice 2. Provider/adapter health remains owned by 0.9592, and durable goals/plans/recovery remain later 0.9591 work.

**Example to run:** the new 0.958 Slice 1 runtime-lifecycle Example on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 runtime-lifecycle test class/file, followed by the full `HAgent.Tests` regression suite.

## Source-of-truth map

- Architecture: `docs/architecture/`
- Active implementation: `docs/plan/`
- Roadmap: `docs/roadmap/`
- Storage: `docs/storage.md`

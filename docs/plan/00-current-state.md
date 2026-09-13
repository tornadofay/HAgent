# Current State

## Current phase

**0.958 — Agent Lifecycle and Health Management**

Slice 1 is the only active implementation slice and is **implemented; verification pending**.

## 0.9576 completion

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. The five matching Examples were verified by the user on .NET Framework 4.8.1 and .NET 9.

The user reported the full `.NET 9` `HAgent.Tests` regression suite at **260/260 passed, 0 failed, 0 skipped** after Slice 5.

The completed boundaries are:

- applicability and validity;
- reliability evidence and validated outcome feedback;
- staleness, contradiction, drift, revalidation, and replacement candidates;
- quarantine, retirement, archival, and forgetting;
- learned-resource runtime admission and immutable execution snapshots.

Published resource versions remain unchanged. Reliability remains separate from authorization.

## 0.958 documentation checkpoint

The target lifecycle/health architecture is authoritative in `docs/architecture/102-runtime-lifecycle-health.md`.

`docs/architecture/10-runtime.md` remains authoritative for runtime identity, execution identity, snapshots, cancellation, runtime-state persistence, and stale-result protection; it now explicitly defers the long-lived lifecycle extension to 0.958.

`docs/roadmap/30-agent-runtime.md` records the same ownership boundary: Phase 0.9 owns foundational runtime lifecycle, while 0.958 owns the long-lived lifecycle/health extension.

## Slice 1 implementation checkpoint

The runtime lifecycle now uses the existing `AgentRuntimeInstance` with:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

A dedicated lifecycle revision is advanced only by valid lifecycle transitions. Each admitted execution captures both its per-execution revision and the runtime lifecycle revision at admission. `IsExecutionCurrent` requires both revisions to remain current and the runtime to remain `Active`, preventing an older execution from regaining authority after a lifecycle transition.

Runtime-state persistence preserves lifecycle state and lifecycle revision. File, SQL Server, and MySQL stores all bind/read the new revision, and existing SQL schemas receive a default-zero column when upgraded.

The existing Example path now exposes `RUNTIME LIFECYCLE`, and `tests/HAgent.Tests/RuntimeLifecycleTests.cs` covers transitions, invalid transitions, non-active admission rejection, stale-result invalidation, restore, and file persistence.

## Verification checkpoint

**Example to run:** `HAgent.Example → RUNTIME LIFECYCLE` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeLifecycleTests.cs` focused first, followed by the full `HAgent.Tests` regression suite required by 0.958.

A dedicated `.github/workflows/verify-phase-0-958-slice-1.yml` performs the repository-side builds and .NET 9 test commands on `master` pushes. Slice 1 remains open until those tests and both Example targets are actually executed and the results are recorded.

# Current State

## Current phase

**0.958 — Agent Lifecycle and Health Management**

Slice 1 is the only active implementation slice and is ready to implement after the documentation reconciliation checkpoint.

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

## Slice 1 target

The runtime lifecycle becomes:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

The runtime identity remains the existing `AgentRuntimeInstance`. `Suspended` and `Recovering` do not create a second runtime identity or agent class. Ordinary new runtime-originated work is admitted only from `Active`, subject to existing policy/scheduling/execution boundaries.

Valid lifecycle transitions and lifecycle revision semantics must prevent obsolete asynchronous work from regaining authority after suspension, recovery, retirement, or shutdown. `Shutdown` remains terminal.

Health is deliberately not part of Slice 1 implementation yet. 0.958 Slice 2 will define `Healthy`, `Degraded`, `Failed`, and `Unknown` as runtime evidence separate from lifecycle and authorization.

## Verification checkpoint

**Example to run:** the new architecture-classified runtime-lifecycle Example for 0.958 Slice 1 on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 runtime-lifecycle test class/file, followed by the full `HAgent.Tests` regression suite required by 0.958.

The exact final Example title and focused test file are to be recorded here when implementation creates them.

# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.958 Agent Lifecycle and Health Management — CURRENT

Phase 0.9576 Learned Resource Reliability + Adaptation is **VERIFIED through all five slices**.

### Entry condition

0.9576 is complete and user-verified on .NET Framework 4.8.1 and .NET 9, including runtime integration. The user reported the full `.NET 9` `HAgent.Tests` regression result at **260/260 passed, 0 failed, 0 skipped**. The five 0.9576 Examples were verified on both supported targets.

### Slice 1 — Lifecycle state extension — CURRENT

Implement the long-lived runtime lifecycle extension against the existing `AgentRuntimeInstance` foundation.

- Preserve the existing runtime-instance identity and execution identity.
- Extend the runtime lifecycle to `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`.
- Define explicit valid/invalid transitions and terminal behavior.
- Prevent `Suspended`, `Recovering`, `Retired`, and `Shutdown` runtimes from originating ordinary new work.
- Preserve existing execution terminal-state handling and stale-result protection.
- Advance lifecycle authority/revision on valid transitions so obsolete asynchronous work cannot become authoritative again after lifecycle changes.
- Preserve runtime-owned durable state during suspension/recovery; do not introduce durable goals/plans or provider-health routing here.
- Use the canonical 0.959 intervention boundary when an authorized intervention causes a lifecycle transition; 0.958 owns the target transition, not a second approval mechanism.
- Add deterministic focused tests and a matching public-API Example.

### Authoritative architecture

`docs/architecture/102-runtime-lifecycle-health.md` defines the target lifecycle/health architecture. `docs/architecture/10-runtime.md` remains authoritative for runtime identity, execution, snapshots, cancellation, persistence, and stale-result protection.

### Example and verification checkpoint

**Example to run:** `HAgent.Example →` the new architecture-classified runtime lifecycle Example for Slice 1, on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 runtime-lifecycle test class/file, then the full `HAgent.Tests` regression suite required by the phase.

Record the exact Example title and focused test file here before closing the slice if implementation changes their final names.

### Run rule

Slice 1 is the only active implementation slice. Do not begin Slice 2 until Slice 1 has focused tests, both required Example targets, and the required regression verification recorded as complete.

# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.958 Agent Lifecycle and Health Management — CURRENT

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices.

### Entry condition

0.9576 is complete and user-verified on .NET Framework 4.8.1 and .NET 9, including runtime integration. The user reported the full `.NET 9` `HAgent.Tests` regression result at **260/260 passed, 0 failed, 0 skipped** after Slice 5. The five 0.9576 Examples were verified on both supported targets.

### Slice 1 — Lifecycle state extension — VERIFIED / CLOSED

The existing `AgentRuntimeInstance` foundation now owns lifecycle state and lifecycle revision without a second runtime identity or execution model.

Verified by the user on both .NET Framework 4.8.1 and .NET 9:

- Lifecycle transitions: `Active -> Suspended -> Recovering -> Active -> Suspended -> Active -> Retired`.
- Lifecycle revision advanced to 6 after valid transitions.
- An execution admitted before a lifecycle transition became stale.
- Non-active execution admission was rejected.
- Persisted lifecycle state was restored successfully.
- Shutdown cancelled outstanding work and reached terminal `Shutdown`.
- Full `HAgent.Tests` regression suite reported **266/266 passed, 0 failed, 0 skipped**.

The focused Slice 1 test class is `tests/HAgent.Tests/RuntimeLifecycleTests.cs` and the matching manual scenario is `HAgent.Example -> Runtime -> Runtime Instances -> RUNTIME LIFECYCLE`.

### Slice 2 — Health state — VERIFIED / CLOSED

The runtime-health dimension is implemented as provider-neutral evidence owned by `AgentRuntimeInstance` without changing lifecycle authority or authorization semantics.

Verified by the user on both supported Example targets:

- Initial health is `Unknown`.
- `Degraded` is explicitly transient.
- Health updates do not advance lifecycle revision.
- Slow valid inference remains `Healthy`.
- Terminal failure is explicitly `Failed`.
- Health persistence/restoration succeeds.
- Health source and reason are preserved.
- Full `.NET 9` `HAgent.Tests` regression suite reported **276/276 passed, 0 failed, 0 skipped**.

The focused Slice 2 test class is `tests/HAgent.Tests/RuntimeHealthTests.cs` and the matching manual scenario is `HAgent.Example -> Runtime -> Runtime Instances -> RUNTIME HEALTH`.

### Slice 3 — Progress and recovery signals — CURRENT / IMPLEMENTATION COMPLETE; VERIFICATION PENDING

Implement bounded runtime progress/heartbeat evidence and deterministic stall assessment, then make recovery outcome explicit while preserving runtime identity, lifecycle revision safety, and durable runtime state.

Required surface now implemented:

- bounded progress/heartbeat snapshots with monotonic sequence numbers;
- optional bounded percent/detail/evidence metadata;
- configured stall policy using a maximum progress/heartbeat silence interval;
- no stall classification when there is no progress evidence;
- stall assessment is descriptive and never mutates lifecycle automatically;
- explicit recovery result with success/failure/cancellation and bounded reason/evidence;
- recovery completion is allowed only from `Recovering` and advances lifecycle revision;
- failed/cancelled recovery cannot return directly to `Active`;
- runtime identity remains unchanged through recovery;
- recovery does not delete or replace runtime durable state;
- provider-specific health/routing remains outside this slice.

### Example and verification checkpoint

**Example to run:** `HAgent.Example -> Runtime -> Runtime Instances -> RUNTIME PROGRESS & RECOVERY` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeProgressRecoveryTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the phase.

### Run rule

Slice 3 is the only active implementation slice. Do not begin Slice 4 until Slice 3 focused tests, both required Example targets, and the required regression verification are recorded as complete.

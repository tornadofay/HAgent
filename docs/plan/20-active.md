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

### Slice 2 — Health state — IMPLEMENTED / VERIFICATION PENDING

Implement the runtime-health dimension as a provider-neutral evidence contract owned by `AgentRuntimeInstance` without changing lifecycle authority or authorization semantics.

Required surface now implemented:

- normalized health states: `Healthy`, `Degraded`, `Failed`, `Unknown`;
- bounded reason/evidence metadata;
- explicit source category for runtime observation, recovery result, host signal, or equivalent provider-neutral evidence;
- explicit transient degradation versus terminal failure classification;
- deterministic validation and detached snapshots/cloning across runtime boundaries;
- no failure classification based only on elapsed inference time;
- lifecycle and health remain separate dimensions;
- provider-specific health remains outside Core/provider-neutral runtime health;
- health round-trips through the existing runtime-state persistence boundary.

### Authoritative architecture

`docs/architecture/102-runtime-lifecycle-health.md` is authoritative for the lifecycle/health boundary. `docs/architecture/10-runtime.md` remains authoritative for runtime identity, execution snapshots, cancellation, persistence, and stale-result protection.

### Example and verification checkpoint

**Example to run:** `HAgent.Example -> Runtime -> Runtime Instances -> RUNTIME HEALTH` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeHealthTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the phase.

### Run rule

Slice 2 is the only active implementation slice. Do not begin Slice 3 until Slice 2 focused tests, both required Example targets, and the required regression verification are recorded as complete.

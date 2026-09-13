# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.958 Agent Lifecycle and Health Management — CURRENT

Phase 0.9576 Learned Resource Reliability + Adaptation is **VERIFIED through all five slices**.

### Entry condition

0.9576 is complete and user-verified on .NET Framework 4.8.1 and .NET 9, including runtime integration. The user reported the full `.NET 9` `HAgent.Tests` regression result at **260/260 passed, 0 failed, 0 skipped** after Slice 5. The five 0.9576 Examples were verified on both supported targets.

### Slice 1 — Lifecycle state extension — IMPLEMENTED, VERIFICATION PENDING

The existing `AgentRuntimeInstance` foundation has been extended without creating a second runtime identity or execution model.

Implemented:

- Runtime states are `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`.
- Valid/invalid lifecycle transitions are enforced explicitly; invalid and duplicate transitions do not advance lifecycle revision.
- Non-active runtimes cannot originate ordinary new execution.
- Lifecycle transitions advance a dedicated lifecycle revision; executions capture both execution revision and lifecycle revision.
- `IsExecutionCurrent` requires the execution to belong to the instance, remain under the current execution revision, remain under the current lifecycle revision, and remain in `Active` state.
- Runtime-state restoration preserves lifecycle state and lifecycle revision.
- File, SQL Server, and MySQL runtime-state persistence preserve lifecycle revision; existing database schemas are extended in place with a default zero revision.
- The canonical runtime-instance execution overload propagates all provider-neutral request snapshot fields while binding runtime identity, overrides, shutdown cancellation, and lifecycle/execution revisions.
- The existing lifecycle Example is now `RUNTIME LIFECYCLE` and covers transition/admission/revision/stale-result/persistence/shutdown behavior.
- A focused `tests/HAgent.Tests/RuntimeLifecycleTests.cs` test class covers deterministic lifecycle and persistence contracts.
- `.github/workflows/verify-phase-0-958-slice-1.yml` verifies Core and Example builds for both supported targets, focused lifecycle tests, and the full .NET 9 test suite on `master` pushes.

No health, provider-health, durable-goal/plan, or later recovery-cognition work was started.

### Authoritative architecture

`docs/architecture/102-runtime-lifecycle-health.md` defines the target lifecycle/health architecture. `docs/architecture/10-runtime.md` remains authoritative for runtime identity, execution, snapshots, cancellation, persistence, and stale-result protection.

### Example and verification checkpoint

**Example to run:** `HAgent.Example → RUNTIME LIFECYCLE` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeLifecycleTests.cs` focused first, then the full `HAgent.Tests` regression suite required by the phase.

**Verification status:** implementation is committed to `master`; repository-side verification is configured by the new 0.958 workflow, but the Example remains a manual WinForms run and Slice 1 is not closed until focused tests, the full regression suite, and both Example targets are actually executed and recorded.

### Run rule

Slice 1 is the only active implementation slice. Do not begin Slice 2 until Slice 1 has focused tests, both required Example targets, and the required regression verification recorded as complete.

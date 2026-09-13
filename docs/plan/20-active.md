# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.958 Agent Lifecycle and Health Management — CURRENT

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices.

### Slice 1 — Lifecycle state extension — VERIFIED / CLOSED

Verified on both .NET Framework 4.8.1 and .NET 9, including lifecycle transitions, lifecycle revision capture, non-active admission rejection, stale-result protection, persistence/restore, and shutdown cancellation. Full regression at that checkpoint: **266/266 passed, 0 failed, 0 skipped**.

### Slice 2 — Health state — VERIFIED / CLOSED

Verified on both supported Example targets. Full regression at that checkpoint: **276/276 passed, 0 failed, 0 skipped**.

### Slice 3 — Progress and recovery signals — VERIFIED / CLOSED

Verified on both supported Example targets. Full regression at that checkpoint: **283/283 passed, 0 failed, 0 skipped**.

### Slice 4 — Observability and verification — CURRENT / IMPLEMENTED

Implemented surface:

- `AiRuntimeObservation` and `AiRuntimeObservationEventArgs` provide detached provider-neutral observations for lifecycle, health, progress, and recovery evidence.
- `AiRuntimeDiagnosticsSnapshot` and `AiRuntimeDiagnosticsService` provide a bounded point-in-time runtime diagnostic projection.
- `AiRuntimeObservationPublisher` adapts detached runtime observations into the existing `IEventDispatcher` / `EventEnvelope` boundary using `EventSource.Runtime` and `EventScope.Runtime`.
- Observability does not authorize work, mutate lifecycle, or become a provider-health router.
- Matching Example: `HAgent.Example → Runtime → Diagnostics → RUNTIME OBSERVABILITY`.
- Focused tests: `tests/HAgent.Tests/RuntimeObservabilityTests.cs` and `tests/HAgent.Tests/RuntimeObservationPublisherTests.cs`.

### Verification checkpoint

**Example to run:** `HAgent.Example → Runtime → Diagnostics → RUNTIME OBSERVABILITY` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeObservabilityTests.cs` and `tests/HAgent.Tests/RuntimeObservationPublisherTests.cs` focused first, then the full `HAgent.Tests` regression suite.

**Run rule:** Do not begin 0.9591 until Slice 4 verification is complete and recorded.

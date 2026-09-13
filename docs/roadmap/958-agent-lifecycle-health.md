# Phase 0.958 — Agent Lifecycle and Health Management

## Status

**CURRENT — Slice 1 implementation complete; verification pending.**

0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. 0.958 is the active implementation phase.

## Purpose

Make the lifecycle and health of a live HAgent runtime explicit and observable without duplicating the runtime-instance identity and execution lifecycle already established by earlier phases.

The phase does **not** create a new runtime-agent class. It extends the existing runtime-instance foundation with the operational state needed by long-running agents and Persistent Cognitive Runtime.

## Authoritative architecture

The stable target architecture for this phase is defined in `docs/architecture/102-runtime-lifecycle-health.md`. `docs/architecture/10-runtime.md` remains the foundation document for runtime identity, execution, snapshots, cancellation, persistence, and stale-result protection and defers the long-lived lifecycle extension to this document.

## V1 outcome

HAgent distinguishes:

```text
Lifecycle state = whether the runtime may operate
Health state    = whether the runtime is operating normally
Execution state = what one specific execution is doing
```

These concerns remain separate.

### Lifecycle

The existing runtime foundation is authoritative for `Active`, `Retired`, and terminal `Shutdown`.

0.958 extends that same runtime lifecycle with the operational states needed for persistent operation:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

`Suspended` preserves runtime identity and durable state while ordinary new work is prevented or host-controlled work is paused. `Recovering` is an explicit recovery transition and does not originate ordinary new work. `Retired` prevents new work and participates in the existing runtime result-authority rules. `Shutdown` is terminal.

Lifecycle transitions advance the applicable runtime/lifecycle revision. Work admitted under an obsolete lifecycle revision cannot regain authority over newer runtime state.

### Health

Health is orthogonal:

```text
Healthy
Degraded
Failed
Unknown
```

Health is evidence, not authorization. Lifecycle and policy decide whether work may continue, wait, recover, or stop.

Health evidence must be bounded and must include enough source/reason metadata to explain why a runtime is considered degraded, failed, or unknown. Slow but valid inference is not a failure solely because it is long-running.

## Ownership boundary

0.958 owns **runtime-agent lifecycle and runtime health**.

The provider ecosystem phase 0.9592 owns provider/adapter lifecycle and provider/target operational evidence. The capability-aware execution phase 0.96 consumes that provider/target evidence for execution admission.

0.958 must not turn provider failures, rate limits, or target outages into a second provider health/routing authority. Likewise, 0.9592 must not create a second runtime-agent lifecycle model.

Human/host intervention is consumed through the canonical 0.959 intervention boundary; 0.958 owns the target transition itself.

## Delivery slices

### Slice 1 — Lifecycle state extension — IMPLEMENTED, VERIFICATION PENDING

- Extend the existing runtime lifecycle only where long-lived operation requires it.
- Define valid/invalid transitions and terminal behavior.
- Keep `Shutdown` terminal and prevent ordinary new runtime-originated work from `Suspended`, `Recovering`, `Retired`, or `Shutdown` states.
- Preserve existing execution identity, execution terminal-state handling, and stale-result protection.
- Advance lifecycle/revision authority on valid lifecycle transitions so obsolete asynchronous work cannot become authoritative again after suspension, recovery, retirement, or shutdown.
- Preserve runtime durable state during suspension/recovery; do not introduce goals/plans persistence here.
- Add focused tests and a matching Example through the normal Example architecture/registration path.

**Implemented surface:** the existing `AgentRuntimeInstance` now owns lifecycle state and lifecycle revision authority; execution admission captures both execution and lifecycle revisions; runtime-state persistence preserves lifecycle revision across File/SQL Server/MySQL stores; the lifecycle Example and focused test class are present; a dedicated 0.958 Slice 1 GitHub Actions verification workflow builds both supported Core/Example targets and runs the focused plus full .NET 9 tests.

**Architecture:** `docs/architecture/102-runtime-lifecycle-health.md`.

**Example to run:** `HAgent.Example → RUNTIME LIFECYCLE`, on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/RuntimeLifecycleTests.cs` focused first, then the full `HAgent.Tests` regression suite.

### Slice 2 — Health state

- Define normalized health status and bounded reason metadata.
- Record the source of a health determination: runtime observation, provider failure, recovery failure, host signal, or equivalent evidence.
- Distinguish transient degradation from terminal failure.
- Do not classify slow but valid inference as failed merely because it is long-running.

### Slice 3 — Progress and recovery signals

- Provide bounded progress/heartbeat metadata where a host needs it.
- Detect clearly stalled work only when configured evidence supports that conclusion.
- Support transition into `Recovering` without deleting durable state.
- Make recovery outcome explicit.

### Slice 4 — Observability and verification

- Emit lifecycle and health transitions through existing event/tracing boundaries.
- Expose diagnostics explaining why a runtime is active, suspended, recovering, degraded, failed, retired, or shutdown.
- Verify valid/invalid transitions, suspension/resume, degradation, recovery, stall handling, intervention, and shutdown safety.

## Architectural rules

1. Do not duplicate `AgentRuntimeInstance` identity or execution identity.
2. Lifecycle state is not health state.
3. Health is evidence, not authorization.
4. Recovery never makes obsolete asynchronous work authoritative again.
5. Suspension and recovery preserve durable runtime state.
6. Host lifecycle/scheduling policy remains authoritative where the host controls runtime admission.
7. Provider/model-specific lifecycle semantics do not belong in Core; provider health evidence is normalized by 0.9592 and consumed by 0.96.
8. Runtime intervention uses 0.959; this phase does not create a second approval/intervention mechanism.

## Dependencies

```text
0.9575 governed resources + learning
        ↓
0.9576 learned-resource reliability
        ↓
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
```

## Exit criterion

A long-lived HAgent runtime has explicit lifecycle and runtime-health state, safe suspension/recovery semantics, observable progress/failure reasons, and deterministic protection against work becoming authoritative after retirement, shutdown, recovery invalidation, or newer revisions, while provider health remains owned by the adapter/execution layers.

# Phase 0.958 — Agent Lifecycle and Health Management

## Status

**Planned after 0.9576 and before durable goal/plan recovery.**

## Purpose

Make the lifecycle and health of a live HAgent runtime explicit and observable without duplicating the runtime-instance identity and execution lifecycle already established by earlier phases.

The phase does **not** create a new runtime-agent class. It extends the existing runtime-instance foundation with the operational state needed by long-running agents and Persistent Cognitive Runtime.

## V1 outcome

HAgent distinguishes:

```text
Lifecycle state = whether the runtime may operate
Health state    = whether the runtime is operating normally
Execution state = what one specific execution is doing
```

These concerns remain separate.

### Lifecycle

The existing runtime foundation remains authoritative for `Active`, `Retired`, and `Shutdown`.

0.958 adds only the operational states needed for persistent operation:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

`Suspended` preserves durable state while new work is prevented or host-controlled work is paused. It is not retirement.

### Health

Health is orthogonal:

```text
Healthy
Degraded
Failed
Unknown
```

Health is evidence, not authorization. Lifecycle and policy decide whether work may continue, wait, recover, or stop.

## Ownership boundary

0.958 owns **runtime-agent lifecycle and runtime health**.

The provider ecosystem phase 0.9592 owns provider/adapter lifecycle and provider/target operational evidence. The capability-aware execution phase 0.96 consumes that provider/target evidence for execution admission.

0.958 must not turn provider failures, rate limits, or target outages into a second provider health/routing authority. Likewise, 0.9592 must not create a second runtime-agent lifecycle model.

Human/host intervention is consumed through the canonical 0.959 intervention boundary; 0.958 owns the target transition itself.

## Delivery slices

### Slice 1 — Lifecycle state extension

- Extend the existing runtime lifecycle only where long-lived operation requires it.
- Define valid transitions and terminal behavior.
- Prevent suspended, retired, recovering, or shutdown runtimes from originating work that policy disallows.
- Preserve existing revision and stale-result protection.

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
5. Suspension and recovery preserve durable state.
6. Host lifecycle/scheduling policy remains authoritative where the host controls runtime admission.
7. Provider/model-specific lifecycle semantics do not belong in Core; provider health evidence is normalized by 0.9592 and consumed by 0.96.
8. Runtime intervention uses 0.959; this phase does not create a second approval/intervention mechanism.

## Not part of V1

- distributed actor supervision;
- cluster orchestration;
- automatic fleet healing;
- universal heartbeat semantics for every execution;
- autonomous process management;
- replacing the host scheduler with an HAgent scheduler.

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

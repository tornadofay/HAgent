# Phase 0.958 — Agent Lifecycle and Health Management

## Status

**CURRENT — Slices 1–3 verified/closed; Slice 4 implemented, verification pending.**

0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. 0.958 is the active implementation phase.

## Purpose

Make the lifecycle and health of a live HAgent runtime explicit and observable without duplicating the runtime-instance identity and execution lifecycle already established by earlier phases.

The phase does **not** create a new runtime-agent class. It extends the existing runtime-instance foundation with the operational state needed by long-running agents and Persistent Cognitive Runtime.

## Authoritative architecture

The stable target architecture for this phase is defined in `docs/architecture/102-runtime-lifecycle-health.md`. `docs/architecture/10-runtime.md` remains the foundation document for runtime identity, execution snapshots, cancellation, persistence, and stale-result protection and defers the long-lived lifecycle extension to this document.

## V1 outcome

HAgent distinguishes lifecycle, health, bounded progress evidence, explicit recovery, observability, and execution as separate concerns.

### Delivery slices

### Slice 1 — Lifecycle state extension — CLOSED / VERIFIED

Implemented without introducing a second runtime identity or execution model. The user verified lifecycle transitions, lifecycle revision capture, non-active admission rejection, stale-result invalidation, persistence/restore, and shutdown cancellation on both supported Example targets.

**Verified:** full `.NET 9` `HAgent.Tests` reported **266/266 passed, 0 failed, 0 skipped**.

### Slice 2 — Health state — CLOSED / VERIFIED

Implemented normalized `Healthy`, `Degraded`, `Failed`, and `Unknown` states with bounded source/reason/evidence metadata, transient/terminal classification, detached snapshots, and existing runtime-state persistence.

**Verified:** the user verified `HAgent.Example → Runtime → Runtime Instances → RUNTIME HEALTH` on .NET Framework 4.8.1 and .NET 9. Full `.NET 9` `HAgent.Tests` reported **276/276 passed, 0 failed, 0 skipped**.

### Slice 3 — Progress and recovery signals — CLOSED / VERIFIED

Implemented bounded progress/heartbeat evidence, monotonic sequencing, explicit configured stall assessment, and explicit recovery outcomes with lifecycle-revision-safe completion.

**Verified:** the user verified `HAgent.Example → Runtime → Runtime Instances → RUNTIME PROGRESS & RECOVERY` on .NET Framework 4.8.1 and .NET 9. Full `.NET 9` `HAgent.Tests` reported **283/283 passed, 0 failed, 0 skipped**.

### Slice 4 — Observability and verification — IMPLEMENTED / VERIFICATION PENDING

Implemented bounded runtime observability without changing runtime authority:

- `AiRuntimeObservation` and `AiRuntimeObservationEventArgs` carry detached lifecycle, health, progress, and recovery evidence.
- `AiRuntimeDiagnosticsSnapshot` and `AiRuntimeDiagnosticsService` expose bounded point-in-time diagnostics.
- `AiRuntimeObservationPublisher` maps detached observations into the existing `IEventDispatcher` / `EventEnvelope` boundary as Runtime-scoped provider-neutral events.
- Focused verification exists in `tests/HAgent.Tests/RuntimeObservabilityTests.cs` and `tests/HAgent.Tests/RuntimeObservationPublisherTests.cs`.
- Matching Example: `HAgent.Example → Runtime → Diagnostics → RUNTIME OBSERVABILITY`.

**Verification checkpoint:** run the RUNTIME OBSERVABILITY Example on .NET Framework 4.8.1 and .NET 9, then the two focused test classes and the full `HAgent.Tests` regression suite.

No 0.9591 work begins until Slice 4 verification is recorded.

## Architectural rules

1. Do not duplicate `AgentRuntimeInstance` identity or execution identity.
2. Lifecycle state is not health state.
3. Health is evidence, not authorization.
4. Recovery never makes obsolete asynchronous work authoritative again.
5. Suspension and recovery preserve durable runtime state.
6. Progress/stall assessment is evidence, not authorization and not an automatic lifecycle command.
7. Observability is detached evidence and does not authorize, route, or mutate runtime state.
8. Host lifecycle/scheduling policy remains authoritative where the host controls runtime admission.
9. Provider/model-specific lifecycle semantics do not belong in Core; provider health evidence is normalized by 0.9592 and consumed by 0.96.
10. Runtime intervention uses 0.959; this phase does not create a second approval/intervention mechanism.

## Dependencies

```text
0.9575 governed resources + learning
        ↓
0.9576 learned-resource reliability
        ↓
0.958 lifecycle + health + progress/recovery + observability
        ↓
0.9591 durable goals/plans/recovery
```

## Exit criterion

A long-lived HAgent runtime has explicit lifecycle and runtime-health state, safe suspension/recovery semantics, bounded progress/failure reasons, deterministic configured stall evidence, explicit recovery outcomes, bounded diagnostics/event-boundary adaptation, and deterministic protection against work becoming authoritative after retirement, shutdown, recovery invalidation, or newer revisions, while provider health remains owned by adapter/execution layers.

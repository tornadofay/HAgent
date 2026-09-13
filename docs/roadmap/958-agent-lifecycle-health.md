# Phase 0.958 — Agent Lifecycle and Health Management

## Status

**CURRENT — Slice 1 and Slice 2 verified/closed; Slice 3 implemented, verification pending.**

0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. 0.958 is the active implementation phase.

## Purpose

Make the lifecycle and health of a live HAgent runtime explicit and observable without duplicating the runtime-instance identity and execution lifecycle already established by earlier phases.

The phase does **not** create a new runtime-agent class. It extends the existing runtime-instance foundation with the operational state needed by long-running agents and Persistent Cognitive Runtime.

## Authoritative architecture

The stable target architecture for this phase is defined in `docs/architecture/102-runtime-lifecycle-health.md`. `docs/architecture/10-runtime.md` remains the foundation document for runtime identity, execution snapshots, cancellation, persistence, and stale-result protection and defers the long-lived lifecycle extension to this document.

## V1 outcome

HAgent distinguishes:

```text
Lifecycle state = whether the runtime may operate
Health state    = whether the runtime is operating normally
Progress        = bounded evidence that runtime work is advancing or alive
Recovery        = explicit host/runtime transition and outcome
Execution state = what one specific execution is doing
```

These concerns remain separate.

### Lifecycle

The runtime lifecycle is:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

`Suspended` preserves runtime identity and durable state while ordinary new work is prevented or host-controlled work is paused. `Recovering` is an explicit recovery transition and does not originate ordinary new work. `Retired` prevents new work and participates in the existing runtime result-authority rules. `Shutdown` is terminal.

### Health

Health is orthogonal:

```text
Healthy
Degraded
Failed
Unknown
```

Health is evidence, not authorization. Lifecycle and policy decide whether work may continue, wait, recover, or stop.

Health evidence is normalized through `AiRuntimeHealth` with explicit source and failure-kind metadata and bounded reason/evidence text. Slow but valid inference is not a failure solely because it is long-running.

### Progress and recovery

Progress/heartbeat evidence is bounded and sequence-ordered. Stall detection is deterministic and requires an actual progress/heartbeat observation plus an explicit host-configured silence threshold. A stall assessment is descriptive and does not mutate lifecycle automatically.

Recovery is explicit. `BeginRecovery()` advances lifecycle into `Recovering`; `AiRuntimeRecoveryResult` records whether recovery succeeded, failed, or was cancelled; `CompleteRecovery(...)` advances lifecycle revision again and applies an explicitly selected allowed non-terminal result state. Failed or cancelled recovery cannot return directly to `Active`.

## Ownership boundary

0.958 owns **runtime-agent lifecycle, runtime health, bounded runtime progress evidence, stall assessment, and explicit recovery outcomes**.

The provider ecosystem phase 0.9592 owns provider/adapter lifecycle and provider/target operational evidence. The capability-aware execution phase 0.96 consumes that provider/target evidence for execution admission.

0.958 must not turn provider failures, rate limits, or target outages into a second provider health/routing authority. Likewise, 0.9592 must not create a second runtime-agent lifecycle model.

Human/host intervention is consumed through the canonical 0.959 intervention boundary; 0.958 owns the target transition itself.

## Delivery slices

### Slice 1 — Lifecycle state extension — CLOSED / VERIFIED

The existing runtime lifecycle was extended without introducing a second runtime identity or execution model. Valid/invalid transitions, lifecycle revision capture, non-active admission rejection, stale-result invalidation, persistence/restore, and shutdown cancellation were implemented and verified by the user.

**Verified:** `HAgent.Example → Runtime → Runtime Instances → RUNTIME LIFECYCLE` on .NET Framework 4.8.1 and .NET 9; full `.NET 9` `HAgent.Tests` reported **266/266 passed, 0 failed, 0 skipped**.

### Slice 2 — Health state — CLOSED / VERIFIED

- Define normalized health status and bounded reason/evidence metadata.
- Record the source of a health determination through provider-neutral source categories.
- Distinguish transient degradation from terminal failure.
- Do not classify slow but valid inference as failed merely because it is long-running.
- Keep health separate from lifecycle and authorization.
- Persist health through the existing runtime-state record and File/SQL Server/MySQL stores without creating a parallel repository.
- Provide detached health snapshots so external callers cannot mutate live runtime state indirectly.

**Implemented surface:** `src/HAgent.Core/Models/AiRuntimeHealthContracts.cs`, `AgentRuntimeInstance.Health/SetHealth(...)`, `AgentRuntimeStateRecord.Health`, File/SQL Server/MySQL runtime-state persistence, focused tests in `tests/HAgent.Tests/RuntimeHealthTests.cs`, and matching Example `HAgent.Example → Runtime → Runtime Instances → RUNTIME HEALTH`.

**Verified:** the user verified the matching RUNTIME HEALTH Example on .NET Framework 4.8.1 and .NET 9. Full `.NET 9` `HAgent.Tests` reported **276/276 passed, 0 failed, 0 skipped**.

### Slice 3 — Progress and recovery signals — IMPLEMENTED / VERIFICATION PENDING

- Provide bounded progress/heartbeat metadata where a host needs it.
- Require strictly increasing progress evidence sequence numbers.
- Detect clearly stalled work only when a host-configured silence threshold and an actual progress/heartbeat observation support that conclusion.
- Do not mutate lifecycle automatically from a stall assessment.
- Support explicit transition into `Recovering` without creating a second runtime identity or deleting durable runtime state.
- Make recovery outcome explicit as succeeded, failed, or cancelled.
- Complete recovery only from `Recovering`, advancing lifecycle revision and preserving runtime identity.
- Do not allow failed/cancelled recovery to return directly to `Active`.

**Implemented surface:** `src/HAgent.Core/Models/AiRuntimeProgressContracts.cs`, `AgentRuntimeInstance.Progress/ReportProgress/EvaluateStall/CompleteRecovery`, focused tests in `tests/HAgent.Tests/RuntimeProgressRecoveryTests.cs`, matching Example `HAgent.Example → Runtime → Runtime Instances → RUNTIME PROGRESS & RECOVERY`, and dedicated workflow `.github/workflows/verify-phase-0-958-slice-3.yml`.

**Verification checkpoint:** pending user execution of the Example on both supported targets, focused `RuntimeProgressRecoveryTests`, and the full regression suite.

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
6. Progress/stall assessment is evidence, not authorization and not an automatic lifecycle command.
7. Host lifecycle/scheduling policy remains authoritative where the host controls runtime admission.
8. Provider/model-specific lifecycle semantics do not belong in Core; provider health evidence is normalized by 0.9592 and consumed by 0.96.
9. Runtime intervention uses 0.959; this phase does not create a second approval/intervention mechanism.

## Dependencies

```text
0.9575 governed resources + learning
        ↓
0.9576 learned-resource reliability
        ↓
0.958 lifecycle + health + progress/recovery
        ↓
0.9591 durable goals/plans/recovery
```

## Exit criterion

A long-lived HAgent runtime has explicit lifecycle and runtime-health state, safe suspension/recovery semantics, bounded progress/failure reasons, deterministic configured stall evidence, explicit recovery outcomes, and deterministic protection against work becoming authoritative after retirement, shutdown, recovery invalidation, or newer revisions, while provider health remains owned by the adapter/execution layers.

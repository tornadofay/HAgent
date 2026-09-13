# Runtime Lifecycle and Health

## Purpose

Phase 0.958 extends the existing `AgentRuntimeInstance` foundation so long-lived runtimes have explicit operational lifecycle, runtime-health, progress, recovery, and bounded observability without creating a second runtime identity model or changing execution identity semantics.

The canonical identity remains the existing runtime instance. This document defines the lifecycle/health/progress/recovery/observability contract owned by 0.958; provider/adapter health remains outside this model.

## Separation of concerns

```text
Lifecycle state = whether the runtime may originate new work
Health state    = evidence about whether the runtime is operating normally
Progress        = bounded evidence that runtime work is advancing or alive
Recovery        = explicit host/runtime transition and outcome
Observability   = detached evidence/diagnostics about runtime state
Execution state = what one execution is doing
```

Lifecycle, health, progress, recovery, observability, and execution are independent dimensions.

Health, progress, and observability do not grant or deny authorization. Lifecycle admission and host policy decide whether work may start, wait, recover, or stop.

## Lifecycle states

The provider-neutral runtime lifecycle is:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

`Active` is the normal operating state. `Suspended` preserves runtime identity and durable state while new runtime-originated work is prevented or paused by lifecycle/host policy. `Recovering` means the runtime is performing an explicit recovery transition and must not originate ordinary new work. `Retired` is a non-terminal operational state for the runtime instance that prevents new work and invalidates result authority according to the existing runtime revision model. `Shutdown` is terminal and cannot be resumed.

The existing `AgentRuntimeInstance` identity remains authoritative. No new runtime-agent class is introduced.

## Lifecycle transition contract

Normal transitions are explicit and revision-safe:

```text
Active      -> Suspended | Recovering | Retired | Shutdown
Suspended   -> Active | Recovering | Retired | Shutdown
Recovering  -> Active | Suspended | Retired | Shutdown
```

`Retired` and `Shutdown` do not originate new work. `Shutdown` is terminal. `Retired` remains non-terminal only in the sense required by the existing runtime contract; 0.958 must not silently reinterpret retirement as resumable operation. Any host-approved recovery from retirement must use an explicit lifecycle revision/transition contract and may not restore authority to obsolete asynchronous work.

Invalid or duplicate transitions fail deterministically and do not advance lifecycle revision.

Lifecycle mutation increments the runtime lifecycle revision. Execution admission captures the applicable runtime revision. A result produced from an obsolete lifecycle revision cannot regain authority over newer runtime state.

## Work admission

The runtime may originate ordinary new execution only while `LifecycleState == Active`, subject to the existing policy, scheduler, authorization, capability, reliability, and execution-admission boundaries.

`Suspended`, `Recovering`, `Retired`, and `Shutdown` reject ordinary new runtime-originated work. Existing executions are handled according to their own execution lifecycle and the host/runtime stale-result rules; changing runtime lifecycle does not retroactively rewrite an execution's immutable request snapshot.

Suspension and recovery do not delete or mutate durable runtime state merely because the runtime is temporarily unable to originate work.

## Revision and stale-result protection

Lifecycle revision is an authority boundary, not a replacement for provider cancellation.

For a runtime-bound execution:

```text
execution start
    -> capture runtime instance ID + applicable lifecycle revision

lifecycle transition
    -> advance runtime lifecycle revision

late provider completion
    -> may complete provider work
    -> must not become authoritative if its runtime revision is stale
```

This composes with the existing execution terminal-state gate and `AgentRuntimeInstance.IsExecutionCurrent(...)` semantics.

Recovery creates a newer runtime lifecycle revision rather than making an obsolete execution current again.

## Runtime health

Health is normalized provider-neutral runtime evidence:

```text
Healthy
Degraded
Failed
Unknown
```

The canonical health contract is `AiRuntimeHealth`. A newly created runtime always has a health snapshot and starts at `Unknown` because no health observation has yet been recorded. Health is owned by the existing `AgentRuntimeInstance` through `Health` and `SetHealth(...)`; no parallel runtime-health owner exists.

Each health snapshot contains `Status`, `Source`, `FailureKind`, bounded `Reason`/`Evidence`, and optional `ObservedAt` metadata. `Degraded` is transient, `Failed` is terminal, and `Unknown`/`Healthy` carry no failure kind.

`SetHealth(...)` validates and clones the supplied contract before storing it under the runtime instance lock. Reading `AgentRuntimeInstance.Health` returns a detached clone, so health evidence crossing the runtime boundary cannot mutate live internal state. Health updates do not advance lifecycle revision and do not grant authorization.

A slow but valid inference may remain `Healthy`; elapsed time by itself does not create a `Failed` health state. Failure requires explicit terminal evidence represented by the health contract.

Health is persisted through the existing `AgentRuntimeStateRecord` and the existing File, SQL Server, and MySQL runtime-state stores. Older persisted rows/files without health fields restore as `Unknown`; no second health repository is introduced.

Provider-specific operational health remains owned by provider/adapter layers and is consumed through their provider-neutral contracts in later execution-admission work. 0.958 does not create a provider router or provider-health authority.

## Progress and stall assessment

Runtime progress is bounded evidence, not an authorization or lifecycle mechanism. The canonical contract is `AiRuntimeProgressSnapshot` and it is owned by the existing `AgentRuntimeInstance` through `Progress` and `ReportProgress(...)`.

A progress snapshot contains a strictly increasing sequence, `Progress`/`Heartbeat` kind, optional percentage, bounded detail/evidence text, and a report timestamp. Snapshots are validated and detached. Reusing an old or out-of-order sequence is rejected.

Stall detection is explicit and deterministic through `AiRuntimeStallPolicy` and `AiRuntimeProgressMonitor`. The host supplies a maximum allowed progress/heartbeat silence interval. A stall is reported only when actual progress/heartbeat evidence exists and its age exceeds that threshold. Missing evidence and elapsed execution time alone are not proof of a stall.

`AgentRuntimeInstance.EvaluateStall(...)` returns descriptive evidence through `AiRuntimeStallAssessment`. The assessment never transitions the runtime automatically; the host/runtime control path must explicitly request recovery.

## Recovery outcome

Recovery begins explicitly through the existing `BeginRecovery()` lifecycle transition. Recovery does not create a second runtime identity and does not delete or replace durable runtime state.

`AiRuntimeRecoveryResult` provides explicit `Succeeded`, `Failed`, or `Cancelled` outcomes with bounded reason/evidence and a completion timestamp. `AgentRuntimeInstance.CompleteRecovery(...)` is valid only from `Recovering`, advances lifecycle revision, preserves runtime identity, and forbids failed/cancelled recovery from returning directly to `Active`.

Recovery outcome validation is deterministic. Recovery never silently resurrects obsolete asynchronous results.

## Observability and verification boundary

Runtime observability is detached evidence. The canonical contracts are `AiRuntimeObservation`, `AiRuntimeObservationEventArgs`, and `AiRuntimeDiagnosticsSnapshot`.

`AiRuntimeDiagnosticsService.Capture(...)` provides a bounded point-in-time diagnostic projection containing runtime identity, lifecycle state/revision, execution revision, health, and progress. The contained health/progress objects are detached snapshots and cannot mutate live runtime state.

`AiRuntimeObservationPublisher.PublishAsync(...)` adapts an explicitly supplied detached runtime observation into the existing provider-neutral `IEventDispatcher` / `EventEnvelope` boundary. Published events use `EventSource.Runtime` and `EventScope.Runtime` with the runtime instance ID as `ScopeId`. Publication is asynchronous and cancellation-aware. The publisher does not perform authorization, mutate lifecycle, or choose providers.

Synchronous lifecycle APIs do not perform hidden fire-and-forget event publication. Hosts or runtime orchestration code explicitly construct the detached observation and publish it through the existing event boundary, preserving the separation between runtime authority and observability infrastructure.

The Slice 4 Example and focused tests verify detached diagnostics, lifecycle/health/progress/recovery observation content, and Runtime-scoped event-envelope publication.

## Intervention boundary

Human/host intervention uses the canonical 0.959 intervention boundary. 0.958 owns the lifecycle transition applied after an authorized intervention; it does not create a second approval or intervention subsystem.

## Persistence boundary

Existing runtime-state persistence remains the persistence boundary for runtime identity/lifecycle and health metadata. Progress/heartbeat observations, stall assessments, and observability events are operational runtime evidence and do not introduce a second durable repository in 0.958. Durable event history remains governed by the broader event subsystem rather than by runtime health.

## Ownership

- **0.9 / 0.95:** runtime identity, execution identity, cancellation, retirement/shutdown foundations, and stale-result protection.
- **0.958:** runtime lifecycle, runtime health, progress evidence, stall assessment, explicit recovery outcomes, and bounded runtime observability.
- **0.959:** canonical human/host intervention decision boundary.
- **0.9591:** durable goals/plans/checkpoints and restart recovery.
- **0.9592:** provider/adapter lifecycle and provider operational evidence.
- **0.96:** execution-target admission, including consumption of provider health evidence.
- **0.97:** persistent cognitive state and cognitive progress.

## Non-goals

0.958 does not introduce a second runtime identity or agent class, a provider/model health router, provider-specific stall/routing authority, durable goal/plan persistence, a replacement authorization system, a second intervention mechanism, or automatic lifecycle mutation based solely on model-generated text or elapsed inference time.

## Verification expectations

The phase must verify deterministic valid/invalid lifecycle transitions, admission rejection while non-operational, suspension/resume semantics, recovery invalidation of stale work, preservation of durable state, health-state evidence boundaries, bounded progress/heartbeat reporting, deterministic configured stall assessment, explicit recovery outcome handling, detached observability diagnostics, Runtime-scoped event publication, and terminal shutdown behavior on both supported targets.

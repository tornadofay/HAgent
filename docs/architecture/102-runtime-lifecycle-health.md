# Runtime Lifecycle and Health

## Purpose

Phase 0.958 extends the existing `AgentRuntimeInstance` foundation so long-lived runtimes have explicit operational lifecycle, runtime-health, progress, and recovery state without creating a second runtime identity model or changing execution identity semantics.

The canonical identity remains the existing runtime instance. This document defines the lifecycle/health/progress/recovery contract owned by 0.958; provider/adapter health remains outside this model.

## Separation of concerns

```text
Lifecycle state = whether the runtime may originate new work
Health state    = evidence about whether the runtime is operating normally
Progress        = bounded evidence that runtime work is advancing or alive
Recovery        = explicit host/runtime transition and outcome
Execution state = what one execution is doing
```

Lifecycle, health, progress, recovery, and execution are independent dimensions.

Health and progress do not grant or deny authorization. Lifecycle admission and host policy decide whether work may start, wait, recover, or stop.

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

Each health snapshot contains:

- `Status`: `Unknown`, `Healthy`, `Degraded`, or `Failed`.
- `Source`: `RuntimeObservation`, `RecoveryResult`, `HostSignal`, or `ExternalEvidence`.
- `FailureKind`: `None`, `Transient`, or `Terminal`. `Degraded` requires `Transient`; `Failed` requires `Terminal`; `Unknown` and `Healthy` require `None`.
- `Reason`: normalized/bounded text, maximum 512 characters. Degraded and failed states require a non-empty reason.
- `Evidence`: normalized/bounded descriptive text, maximum 2048 characters.
- `ObservedAt`: optional timestamp. It is omitted for the initial unknown state and may be supplied for an actual observation.

`SetHealth(...)` validates and clones the supplied contract before storing it under the runtime instance lock. Reading `AgentRuntimeInstance.Health` returns a detached clone, so health evidence crossing the runtime boundary cannot mutate live internal state. Health updates do not advance lifecycle revision and do not grant authorization.

A slow but valid inference may remain `Healthy`; elapsed time by itself does not create a `Failed` health state. Failure requires explicit terminal evidence represented by the health contract.

Health is persisted through the existing `AgentRuntimeStateRecord` and the existing File, SQL Server, and MySQL runtime-state stores. Older persisted rows/files without health fields restore as `Unknown`; no second health repository is introduced.

Provider-specific operational health remains owned by provider/adapter layers and is consumed through their provider-neutral contracts in later execution-admission work. 0.958 does not create a provider router or provider-health authority.

## Progress and stall assessment

Runtime progress is bounded evidence, not an authorization or lifecycle mechanism. The canonical contract is `AiRuntimeProgressSnapshot` and it is owned by the existing `AgentRuntimeInstance` through `Progress` and `ReportProgress(...)`.

A progress snapshot contains:

- `Sequence`: a strictly increasing runtime-local sequence number;
- `Kind`: `Progress` or `Heartbeat`;
- optional completion percentage from 0 to 100;
- bounded detail and evidence text;
- the timestamp at which the progress/heartbeat was reported.

Snapshots are validated and detached. Reusing an old or out-of-order sequence is rejected rather than silently replacing newer evidence.

Stall detection is explicit and deterministic through `AiRuntimeStallPolicy` and `AiRuntimeProgressMonitor`. The host supplies a maximum allowed progress/heartbeat silence interval. A stall is reported only when an actual progress/heartbeat observation exists and its age exceeds that configured threshold. Missing progress evidence is not itself treated as proof of a stall, and elapsed execution time alone does not mutate runtime lifecycle.

`AgentRuntimeInstance.EvaluateStall(...)` returns descriptive evidence through `AiRuntimeStallAssessment`. The assessment never transitions the runtime automatically; the host/runtime control path must explicitly request recovery.

Progress metadata is runtime operational evidence and is not a replacement for execution identity, execution terminal state, health, or provider health.

## Recovery outcome

Recovery begins explicitly through the existing `BeginRecovery()` lifecycle transition. Recovery does not create a second runtime identity and does not delete or replace durable runtime state.

The explicit recovery result contract is `AiRuntimeRecoveryResult` with:

- `Status`: `Succeeded`, `Failed`, or `Cancelled`;
- bounded reason and evidence text;
- a required completion timestamp.

`AgentRuntimeInstance.CompleteRecovery(...)` accepts the result plus an explicitly host-selected resulting lifecycle state. Completion is valid only from `Recovering` and always advances lifecycle revision. Failed or cancelled recovery cannot directly return the runtime to `Active`; the host must keep it `Suspended` or `Retired` (or use terminal `Shutdown` through the normal shutdown operation).

Recovery outcome validation is deterministic. Recovery never silently resurrects obsolete asynchronous results. Runtime identity, durable state, and ownership remain intact while lifecycle authority moves to the newer revision.

## Intervention boundary

Human/host intervention uses the canonical 0.959 intervention boundary. 0.958 owns the lifecycle transition applied after an authorized intervention; it does not create a second approval or intervention subsystem.

## Persistence boundary

Existing runtime-state persistence remains the persistence boundary for runtime identity/lifecycle and health metadata. Progress/heartbeat observations and stall assessments are operational runtime metadata and are not introduced as a second durable repository by this slice. 0.958 does not introduce durable goals, plans, checkpoints, or cognitive state; those remain 0.9591/0.97 responsibilities.

## Ownership

- **0.9 / 0.95:** runtime identity, execution identity, cancellation, retirement/shutdown foundations, and stale-result protection.
- **0.958:** runtime lifecycle extension, runtime health, runtime progress evidence, stall assessment, and explicit recovery outcomes.
- **0.959:** canonical human/host intervention decision boundary.
- **0.9591:** durable goals/plans/checkpoints and restart recovery.
- **0.9592:** provider/adapter lifecycle and provider operational evidence.
- **0.96:** execution-target admission, including consumption of provider health evidence.
- **0.97:** persistent cognitive state and cognitive progress.

## Non-goals

0.958 does not introduce:

- a second runtime identity or agent class;
- a provider/model health router;
- a provider-specific stall or routing authority;
- durable goal/plan persistence;
- a replacement authorization system;
- a second intervention/approval mechanism;
- automatic lifecycle mutation based solely on model-generated text or elapsed inference time.

## Verification expectations

The phase must verify deterministic valid/invalid lifecycle transitions, admission rejection while non-operational, suspension/resume semantics, recovery invalidation of stale work, preservation of durable state, health-state evidence boundaries, bounded progress/heartbeat reporting, deterministic configured stall assessment, explicit recovery outcome handling, and terminal shutdown behavior on both supported targets where the implementation is multi-targeted.

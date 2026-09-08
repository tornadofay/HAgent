# Human Intervention Architecture

## Purpose

Human intervention is a provider-neutral control boundary between an active HAgent runtime and an authorized human or host operator. It allows an external authority to inspect or request changes to agent behavior without becoming a second execution engine and without bypassing policy, authorization, capability, budget, cancellation, or host validation.

Phase 0.959 builds on the bounded approval/defer workflow introduced by the unified policy milestone. The existing approval workflow is the foundation for the intervention model; future work must generalize that boundary rather than create a parallel approval or intervention subsystem.

## Core model

An intervention request describes **what is being controlled**, **which action is requested**, **why the request exists**, and **the identities/correlations needed to trace it**.

The canonical request model should contain these conceptual dimensions:

```text
Request identity
    request ID
    lifecycle status
    created / resolved timestamps

Target
    execution
    tool
    plan step
    goal
    learning candidate
    consequential action

Requested action
    inspect
    approve
    reject
    pause
    resume
    cancel
    retire
    shutdown
    redirect
    defer

Execution context
    agent profile
    runtime instance
    execution
    tool
    operation
    resource type / resource ID

Target state evidence
    observed target state
    observed target state version

Trace
    HAgent correlation ID
    host correlation ID
    requester identity
    responder identity

Human metadata
    policy reason
    operator resolution reason
```

The request object is metadata and control state. It is never executable instructions and its free-form comments must never be interpreted as commands by the runtime.

`TargetState` and `TargetStateVersion` are optimistic-concurrency evidence captured when a request is created. Target-specific boundaries may use these fields to determine whether a request is still applicable when it is later approved.

## Lifecycle

An intervention request has an explicit lifecycle. A request must never be silently rewritten from one terminal meaning to another.

```text
Pending
  |
  +--> Approved ------> Completed (when the requested intervention is actually applied)
  |         |
  |         +----------> Expired (when the approved target is discovered to be stale before completion)
  |
  +--> Rejected
  |
  +--> Cancelled
  |
  +--> Expired
```

`Approved` means that the authorized responder accepted the requested intervention. It does not itself execute the protected operation or resume paused work. `Completed` is reserved for a later boundary that can prove the requested intervention was actually applied.

`Approved -> Expired` is reserved for a race where the intervention was accepted but the target became no longer applicable before the control could be completed. This preserves the fact that the request was approved while making the failed application explicit and terminal. A terminal request cannot accept another resolution.

Stale requests, including requests referring to an execution that has already completed, cancelled, retired, or shut down, must remain diagnosable and must not be applied retroactively.

## Approval and policy relationship

Policy remains the authority that determines whether approval, deferral, or another intervention boundary is required. The intervention workflow records and manages the resulting human/host decision.

```text
Model / Runtime request
        |
        v
   Policy Engine
        |
        +---- Allow ----------> continue
        |
        +---- Deny -----------> stop
        |
        +---- RequireApproval -> intervention request
        |
        +---- Defer -----------> intervention request
```

An approved intervention does not bypass a later authorization or capability check. Every consequential operation must still pass its normal runtime, policy, authorization, capability, budget, and host-side validation boundaries.

## Execution control integration

Execution intervention is applied by the existing `DefaultAgentRuntime`; the intervention coordinator does not execute providers or create a second runtime. Each active execution receives one private control state and one linked cancellation source owned by that runtime's intervention coordinator.

The control model is cooperative:

```text
Running
  |
  +--> Paused --------> Running
  |
  +--> Cancelling ----> terminal Cancelled execution
```

Every meaningful execution-control transition advances a monotonic control-state version. An intervention request captures both the state and version that were observed at request creation. This prevents a request created against an earlier state from silently applying after another intervention has already changed the target.

Pause does not attempt hard preemption of an in-flight provider call. The runtime checks the control gate at interruptible execution boundaries and again before committing a provider response. This means a pause requested while transport is already in progress is applied before the response becomes authoritative.

Cancellation is different: it propagates through the execution's linked cancellation token immediately. The runtime therefore completes the caller-facing execution without waiting for a non-cooperative provider task, while the existing terminal-state protection prevents any late provider response from overwriting the cancelled execution.

The public host-facing path is:

```text
HAgentClient
    |
    +--> ExecutionChanged
    +--> RequestExecutionInterventionAsync
    +--> ResolveInterventionRequestAsync
    +--> GetExecutionControlStateAsync
    |
    v
DefaultAgentRuntime
    |
    v
AiInterventionCoordinator
    |
    +--> canonical intervention workflow
    +--> execution control state/version
    +--> per-execution resolution serialization
    +--> linked cancellation token
```

The host supplies requester/responder identity and remains responsible for authenticating and authorizing those principals. Direct mutation helpers remain runtime-internal so a host cannot bypass the intervention request lifecycle through the coordinator.

## Concurrency and stale state

Interventions must be safe when they race with execution state changes. The runtime compares the intervention against the target's current identity/state/version before applying it.

For execution intervention, approvals are serialized through one resolution gate per active execution. This establishes a deterministic ordering for competing intervention requests while leaving the execution engine itself unchanged.

A request is stale when the target is no longer active or when its observed state/version no longer matches the current execution-control state. The canonical result is `Expired`; the request remains queryable with the resolver identity and stale reason.

The design distinguishes:

```text
Request created for execution E
    captures state + control version

E running       -> matching request may be applicable
E paused        -> matching request may be applicable
E control changed -> older request becomes stale
E completing    -> intervention may become stale
E completed     -> intervention is stale
E cancelled     -> intervention is stale
E retired       -> intervention is stale
```

A stale intervention is not silently accepted merely because the request was once valid. The runtime returns a deterministic stale/no-longer-applicable outcome and preserves the request for diagnostics.

Concurrent intervention is idempotent with respect to request lifecycle: only a pending request can be approved/rejected/cancelled/expired, and only an approved request can be completed. A duplicate responder racing on the same request therefore cannot apply a second terminal transition. Different requests are serialized per execution and compete through the captured state/version, so a later conflicting request becomes stale instead of reversing an already applied control without a fresh request.

## Identity and traceability

Every intervention preserves requester and responder identity separately. Requester identity identifies the principal or host that caused the intervention to be created. Responder identity identifies the authorized principal that resolved it.

Correlation identifiers link the intervention to the HAgent execution and, when applicable, to the host application's own operation trace.

HAgent authenticates neither party. It evaluates the identity supplied by its host/security boundary and must not treat an arbitrary identity string as proof of authorization.

## Security and enforcement boundary

Intervention is not a bypass path. The following remain authoritative after an intervention is accepted:

```text
Intervention decision
        |
        v
Policy / permissions
        |
        v
Capability / budget checks
        |
        v
Host authorization / validation
        |
        v
Side effect or runtime state transition
```

The intervention system may pause, approve, reject, cancel, retire, or redirect work, but it cannot directly grant host authorization or invent a capability that the runtime does not have.

## Persistence boundary

The current approval workflow is bounded and process-local. Phase 0.959 should separate the provider-neutral intervention contract from the storage implementation so that later durable intervention can persist requests without changing their meaning.

Future durable storage must preserve the full request identity, target, lifecycle status, target state/version evidence, correlation metadata, requester/responder identity, and resolution metadata. Persistence must not contain executable handlers or authorization callbacks.

## Management and diagnostics UI

The eventual management UI should present pending and historical interventions as human-readable records, not raw infrastructure fields. A useful view should show:

```text
Status        Pending / Approved / Rejected / ...
Target        Tool / Execution / Plan step / ...
Action        Approve / Pause / Cancel / ...
Agent         readable configured name
Operation     readable operation
Reason        policy/runtime reason
Target state  observed state + version when applicable
Requested by  requester identity
Responded by  responder identity
Created       timestamp
Resolved      timestamp when applicable
```

Actions presented by the UI must be constrained by the request lifecycle and the same authorization policy as programmatic intervention. The UI is not an authority of its own.

## Implementation sequence

The phase should evolve in this order:

1. Generalize the existing approval/defer request into the canonical provider-neutral intervention request/lifecycle contract.
2. Define target/action/state semantics for inspect, approve, reject, pause, resume, cancel, retire, shutdown, redirect, and defer.
3. Integrate intervention application at execution/tool boundaries first, then extend to plan, goal, learning, and consequential-action boundaries.
4. Add concurrency-safe stale-request handling and deterministic state transitions.
5. Add durable persistence only after lifecycle semantics are stable.
6. Expose management UI and diagnostics from the same canonical state.
7. Add deterministic Example verification for every lifecycle and race-sensitive transition.

The execution-control portion of step 3 is implemented. Step 4 now has an implementation for execution targets: target state/version capture, per-execution resolution serialization, duplicate-request protection through lifecycle state, and explicit stale expiry. The remaining future work must not assume that this solves additional intervention targets, persistence, or UI semantics.

## Invariants

- There is one intervention contract and one intervention lifecycle model.
- Approval/defer remains a policy outcome; intervention records the human/host control decision.
- Intervention never grants authorization by itself.
- Intervention never executes a tool or provider call merely because it was approved.
- Terminal requests cannot be resolved again.
- Approved requests can become `Expired` only when stale application is detected; `Expired` is terminal.
- Stale requests remain visible and diagnosable.
- Execution intervention requests carry the observed target control state/version needed for stale detection.
- Competing execution interventions are serialized per execution and cannot silently reverse an already-changed target state.
- Free-form operator text is metadata, never executable instructions.
- No UI-specific intervention semantics are allowed to diverge from Core contracts.
- No provider adapter owns human-intervention policy or lifecycle.
- Execution pause/resume/cancel remain controls over the existing runtime execution path; they never create a second execution engine.

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

## Lifecycle

An intervention request has an explicit lifecycle. A request must never be silently rewritten from one terminal meaning to another.

```text
Pending
  |
  +--> Approved ------> Completed (when the requested intervention is actually applied)
  |
  +--> Rejected
  |
  +--> Cancelled
  |
  +--> Expired
```

`Approved` means that the authorized responder accepted the requested intervention. It does not itself execute the protected operation or resume paused work. `Completed` is reserved for a later boundary that can prove the requested intervention was actually applied.

A request in a terminal state cannot accept another resolution. Stale requests, including requests referring to an execution that has already completed, cancelled, retired, or shut down, must remain diagnosable and must not be applied retroactively.

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

## Intervention targets

The model must support intervention at boundaries rather than assuming that every intervention controls a whole agent process:

- **Execution:** control one agent execution.
- **Tool:** control a pending or consequential tool invocation.
- **Plan step:** control one selected step in a multi-step plan.
- **Goal:** control a persistent or active goal.
- **Learning candidate:** control candidate review/promotion.
- **Consequential action:** control a host-defined action with meaningful external effects.

The target-specific execution semantics belong to the runtime boundary that owns the target. The intervention contract only identifies and authorizes the requested control; it does not duplicate those execution mechanisms.

## Concurrency and stale state

Interventions must be safe when they race with execution state changes. The runtime must compare the intervention against the target's current identity/state before applying it.

The design must distinguish:

```text
Request created for execution E

E running       -> intervention may be applicable
E waiting       -> intervention may be applicable
E completing    -> intervention may become stale
E completed     -> intervention is stale
E cancelled     -> intervention is stale
E retired       -> intervention is stale
```

A stale intervention is not silently accepted merely because the request was once valid. The runtime returns a deterministic stale/no-longer-applicable outcome and preserves the request for diagnostics.

Concurrent intervention must be idempotent with respect to the request identity. Two responders must not be able to produce conflicting terminal transitions from the same pending request.

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

Future durable storage must preserve the full request identity, target, lifecycle status, correlation metadata, requester/responder identity, and resolution metadata. Persistence must not contain executable handlers or authorization callbacks.

## Management and diagnostics UI

The eventual management UI should present pending and historical interventions as human-readable records, not raw infrastructure fields. A useful view should show:

```text
Status        Pending / Approved / Rejected / ...
Target        Tool / Execution / Plan step / ...
Action        Approve / Pause / Cancel / ...
Agent         readable configured name
Operation     readable operation
Reason        policy/runtime reason
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

## Invariants

- There is one intervention contract and one intervention lifecycle model.
- Approval/defer remains a policy outcome; intervention records the human/host control decision.
- Intervention never grants authorization by itself.
- Intervention never executes a tool or provider call merely because it was approved.
- Terminal requests cannot be resolved again.
- Stale requests remain visible and diagnosable.
- Free-form operator text is metadata, never executable instructions.
- No UI-specific intervention semantics are allowed to diverge from Core contracts.
- No provider adapter owns human-intervention policy or lifecycle.

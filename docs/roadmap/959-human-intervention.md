# Phase 0.959 — Human-in-the-Loop and Intervention

## Status

**Planned after 0.9591; some execution and learning-candidate intervention already exists ahead of roadmap.**

## Purpose

Provide one provider-neutral intervention boundary through which an authorized human or host application can inspect and change agent operation without bypassing execution ownership, policy, authorization, capability, budget, lifecycle, or host-side validation.

Existing ahead-of-roadmap execution and learning-candidate intervention code is evidence to consume, not a second subsystem.

## V1 intervention model

```text
Host / Operator
      ↓
Intervention Request
      ↓
Identity + Policy + Target-State Validation
      ↓
Owning Runtime / Durable Boundary
      ↓
Applied | Rejected | Expired | Superseded
```

Approval is never itself authorization. An approved intervention still has to pass the owning boundary's enforcement rules.

## Supported action families

V1 covers only actions that have a clear owning boundary:

```text
Inspect
Approve
Reject
Pause
Resume
Cancel
Retire
Shutdown
Defer
Redirect where explicitly supported by the target
```

Not every action applies to every target.

## Target boundaries

The same intervention model may target:

```text
Execution
Tool invocation
Learning candidate
Plan step
Goal / intention
Consequential host action
Runtime lifecycle
```

The target owner remains responsible for applying the transition.

## Delivery slices

### Slice 1 — Canonical intervention contract

- Normalize request identity, target identity, action, reason, requester identity, correlation, expected target revision/state, and expiry.
- Keep intervention state separate from target state.
- Define deterministic outcomes such as applied, denied, expired, superseded, invalid, and failed.

### Slice 2 — Execution and learning boundaries

- Consume existing execution intervention behavior.
- Consume existing learning-candidate intervention behavior.
- Verify stale intervention requests cannot act on newer target revisions.
- Verify concurrent intervention requests serialize at the owning target.

### Slice 3 — Durable goal/plan intervention

- Add intervention at plan-step and goal/intention boundaries after 0.9591 durable contracts exist.
- Preserve durable revision semantics.
- Pause/resume/reject/redirect only through the owning durable state boundary.
- Do not mutate a plan by editing an unrelated intervention record.

### Slice 4 — Consequential-action boundary

- Allow host-defined consequential actions to request intervention using a generic HAgent contract.
- Keep the host authoritative over actual side effects.
- HAgent records requested/approved state but never fabricates completion evidence.

### Slice 5 — Persistence, UI, diagnostics

- Persist intervention records where the owning target is durable.
- Add management UI and diagnostics using existing WinForms conventions.
- Preserve requester/approver identity, reason, correlation, target revision, and outcome.
- Verify restart behavior and stale intervention expiry.

## Architectural rules

1. There is one intervention model, not separate approval engines.
2. Intervention never grants authority that policy did not already grant.
3. Target ownership remains authoritative.
4. Intervention requests become stale when their target revision/state changes.
5. A rejected or expired intervention never mutates the target.
6. Host side effects remain host-authoritative.
7. Intervention state does not replace lifecycle, plan, execution, or learning state.

## Not part of V1

- a general workflow engine;
- autonomous operator simulation;
- distributed human approval consensus;
- arbitrary UI automation;
- intervention as a replacement for normal host scheduling.

## Dependency order

```text
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
        ↓
0.959 intervention
        ↓
0.96 execution planning
```

## Existing ahead-of-roadmap evidence

Execution intervention and learning-candidate intervention already exist and have deterministic Examples/tests. They remain in source while the ordered roadmap catches up. They do not mark the entire 0.959 phase complete.

## Exit criterion

Authorized operators/hosts can safely inspect and intervene at supported execution, learning, plan, goal, lifecycle, and consequential-action boundaries through one policy-governed contract, with durable target revisions, stale-request protection, persistence where appropriate, diagnostics, and no bypass of host authority.

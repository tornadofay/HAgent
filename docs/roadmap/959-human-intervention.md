# Phase 0.959 — Human-in-the-Loop and Intervention

## Status

**In progress — canonical intervention request/lifecycle contract is implemented; runtime integration and full lifecycle controls remain.**

The bounded approval/defer workflow from 0.953 is the starting point for this phase. Phase 0.959 generalizes that boundary into one provider-neutral intervention model rather than creating a parallel approval subsystem.

## Goal

Allow authorized humans or host applications to inspect, pause, resume, approve, reject, redirect, or otherwise intervene in agent behavior without bypassing the HAgent execution and policy model.

## Requirements

1. [x] Define a provider-neutral intervention/approval request and lifecycle model.
2. [ ] Support inspect, approve, reject, pause, resume, cancel, retire, shutdown, redirect, and defer actions where applicable.
3. [ ] Allow intervention at execution, tool, plan-step, goal, learning-candidate, and consequential-action boundaries.
4. [x] Preserve who requested and who approved/rejected an intervention through identity and trace metadata.
5. [x] Make intervention policy-driven rather than prompt-driven.
6. [ ] Ensure an intervention cannot bypass permissions, authorization, budgets, capability requirements, or host-side validation at every intervention target.
7. [ ] Define behavior when intervention arrives while work is executing, waiting, completing, or concurrently changing state.
8. [x] Support operator comments/reasons as bounded metadata without treating them as trusted executable instructions.
9. [ ] Expose intervention state through management UI and diagnostics.
10. [ ] Add deterministic Example verification for approval, rejection, pause/resume, cancellation, concurrent intervention, and stale intervention requests.

## Completed slice

- Canonical `AiInterventionRequest` with request identity, target kind, requested action, lifecycle status, execution/resource context, HAgent/host correlation, requester/responder identity, policy reason, and resolution metadata.
- `IAiInterventionWorkflow` and bounded in-memory implementation with cloned request boundaries and terminal-state protection.
- Tool execution now creates canonical intervention requests for `RequireApproval` and `Defer`, with explicit tool target and `Approve`/`Defer` action semantics.
- Tool execution results expose the intervention request directly.
- Deterministic approval/defer Example verification was moved to the canonical intervention API.

## Implementation sequence

1. Stabilize the Core intervention request/lifecycle contract using the current approval/defer implementation as its foundation.
2. Integrate intervention application at the execution and tool boundaries without introducing a second execution engine.
3. Add deterministic concurrency/stale-request handling before durable persistence.
4. Extend the same contract to plan steps, goals, learning candidates, and consequential actions.
5. Add durable persistence for intervention state.
6. Add management UI and diagnostics over the canonical intervention state.
7. Complete deterministic Example verification for all supported transitions and race-sensitive cases.

The detailed target lifecycle, target model, concurrency rules, persistence boundary, and management UI behavior are defined in `docs/architecture/92-human-intervention.md` and are authoritative for subsequent implementation work.

## Architectural outcome

```text
Agent Runtime
     ↕
Intervention Boundary
     ↕
Human / Authorized Host
```

Intervention controls agent operation; it does not become a second execution engine. Approval or intervention acceptance never by itself grants authorization, bypasses capability/budget checks, or resumes protected work without the owning runtime boundary applying the requested transition.

## Verification rule

A requirement becomes complete only when the implementation exists, deterministic Example verification passes locally, and the architecture/roadmap documentation reflects the verified behavior. Do not claim local build/test success without actually performing it.

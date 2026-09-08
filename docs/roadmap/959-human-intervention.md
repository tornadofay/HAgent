# Phase 0.959 — Human-in-the-Loop and Intervention

## Status

**Ahead-of-roadmap implementation state — execution intervention slices 1 and 2 and the learning-candidate intervention slice are verified; the phase is not the current milestone.**

The repository intentionally retains intervention implementation that was built before the ordered foundational phases 0.954–0.958 and 0.9591 were completed. That implementation is treated as ahead-of-roadmap work, not as permission to skip the foundations.

The bounded approval/defer workflow from 0.953 is the starting point for this phase. Phase 0.959 generalizes that boundary into one provider-neutral intervention model rather than creating a parallel approval subsystem.

## Goal

Allow authorized humans or host applications to inspect, pause, resume, approve, reject, redirect, or otherwise intervene in agent behavior without bypassing the HAgent execution and policy model.

## Requirements

1. [x] Define a provider-neutral intervention/approval request and lifecycle model.
2. [ ] Support inspect, approve, reject, pause, resume, cancel, retire, shutdown, redirect, and defer actions where applicable.
3. [ ] Allow intervention at execution, tool, plan-step, goal, learning-candidate, and consequential-action boundaries. Execution and learning-candidate boundaries are implemented ahead of roadmap; plan-step, goal, and consequential-action coverage remain future work.
4. [x] Preserve who requested and who approved/rejected an intervention through identity and trace metadata.
5. [x] Make intervention policy-driven rather than prompt-driven.
6. [ ] Ensure an intervention cannot bypass permissions, authorization, budgets, capability requirements, or host-side validation at every intervention target.
7. [x] Define behavior when intervention arrives while work is executing, waiting, completing, or concurrently changing state.
8. [x] Support operator comments/reasons as bounded metadata without treating them as trusted executable instructions.
9. [ ] Expose intervention state through management UI and diagnostics.
10. [x] Add deterministic Example verification for the currently implemented approval, rejection, pause/resume, cancellation, concurrent intervention, stale intervention, and learning-candidate scenarios. Final phase-wide coverage remains pending for all supported target types.

## Verified ahead-of-roadmap slices

### Slice 1 — Execution intervention control

Verified through the public `HAgentClient` intervention API and deterministic local provider adapter:

- execution intervention requests capture target/action/correlation metadata;
- approved pause requests reach `Completed` only after the execution-control transition is applied;
- paused executions remain incomplete until explicit resume;
- approved resume requests restore execution;
- approved cancellation produces terminal `Cancelled` execution state;
- late provider responses cannot overwrite a terminal cancelled execution.

### Slice 2 — Concurrency and stale-state hardening

Verified through deterministic local execution scenarios:

- an intervention created before terminal completion becomes `Expired` rather than acting retroactively;
- target control state/version is captured when the request is created;
- conflicting concurrent intervention requests serialize so only one transition applies and the stale request expires;
- duplicate responder resolution cannot apply a second transition;
- a paused execution remains blocked until an explicit fresh resume intervention is approved.

### Slice 3 — Learning-candidate intervention

Verified in `HAgent.Example` through the public `HAgentClient` intervention API:

- approval moves `PendingReview -> Approved`;
- rejection moves `PendingReview -> Rejected`;
- target state and candidate revision are captured at request creation;
- an approval made stale by candidate rejection resolves as `Expired`;
- concurrent approve/reject requests produce one applied intervention and one stale request;
- a proposed candidate can be rejected through the same intervention lifecycle.

This does not make the whole 0.959 phase complete. Plan-step, goal, consequential-action, durable persistence, management UI, and final cross-target verification remain future work after the preceding roadmap phases are completed.

## Implementation sequence

1. Stabilize the Core intervention request/lifecycle contract using the current approval/defer implementation as its foundation.
2. Integrate intervention application at the execution and tool boundaries without introducing a second execution engine.
3. Add deterministic concurrency/stale-request handling before durable persistence.
4. Extend the same contract to plan steps, goals, learning candidates, and consequential actions.
5. Add durable persistence for intervention state.
6. Add management UI and diagnostics over the canonical intervention state.
7. Complete deterministic Example verification for all supported transitions and race-sensitive cases.

The detailed target lifecycle, target model, concurrency rules, persistence boundary, and management UI behavior are defined in `docs/architecture/92-human-intervention.md` and are authoritative for subsequent implementation work.

## Relationship to 0.9591 Goal/Plan Persistence and Recovery

The ordered roadmap places 0.9591 before 0.959. Durable goals, intentions, plans, plan-step revisions, checkpoints, and recovery state provide the persistent authority that later goal/plan intervention can govern safely. Execution-level intervention remains useful independently, so the existing ahead-of-roadmap execution intervention implementation can remain in source while the ordered roadmap catches up.

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

# Phase 0.959 — Human-in-the-Loop and Intervention

## Status

**Planned architectural foundation for safe persistent and autonomous agents.**

## Goal

Allow authorized humans or host applications to inspect, pause, resume, approve, reject, redirect, or intervene in agent behavior without bypassing the HAgent execution and policy model.

## Requirements

1. [ ] Define a provider-neutral intervention/approval request and lifecycle model.
2. [ ] Support inspect, approve, reject, pause, resume, cancel, retire, and shutdown actions where applicable.
3. [ ] Allow intervention at execution, tool, plan-step, goal, learning-candidate, and consequential-action boundaries.
4. [ ] Preserve who requested and who approved/rejected an intervention through identity and trace metadata.
5. [ ] Make intervention policy-driven rather than prompt-driven.
6. [ ] Ensure an intervention cannot bypass permissions, authorization, budgets, capability requirements, or host-side validation.
7. [ ] Define behavior when intervention arrives while work is executing, waiting, or completing concurrently.
8. [ ] Support operator comments/reasons as bounded metadata without treating them as trusted executable instructions.
9. [ ] Expose intervention state through management UI and diagnostics.
10. [ ] Add deterministic Example verification for approval, rejection, pause/resume, cancellation, concurrent intervention, and stale intervention requests.

## Architectural outcome

```text
Agent Runtime
     ↕
Intervention Boundary
     ↕
Human / Authorized Host
```

Intervention controls agent operation; it does not become a second execution engine.
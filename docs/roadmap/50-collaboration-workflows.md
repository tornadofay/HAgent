# Phase 1.0 — Collaboration + Workflows

## Goal
Turn basic workspace messaging into reliable multi-agent collaboration and then into bounded task/workflow execution.

## Collaboration steps

1. First-class delegation/handoff operations.
2. Shared/private workspace context policies.
3. Parallel specialist work with bounded collaboration budgets.
4. Human intervention and approval points.
5. Explicit runtime/participant lifecycle states.
6. Cross-agent memory sharing only through explicit policy.
7. Collaboration history, audit, and traceability.

## Workflow steps

1. Task/job model and lifecycle.
2. Planning, execution, and verification stages.
3. Multi-step and branching workflows.
4. Background execution and scheduling.
5. Pause/resume and durable checkpoints.
6. Event-triggered execution.
7. Per-step timeout, cancellation, retry, approval, and budget policies.

## Boundary

These are generic orchestration facilities. HAgent does not become the authority for business rules, simulation state, or host-side side effects.

## Exit criterion

A host can coordinate multiple agents and long-running work with bounded execution, explicit authority, resumable state where required, and observable collaboration.

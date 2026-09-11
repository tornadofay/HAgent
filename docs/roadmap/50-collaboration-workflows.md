# Phase 1.0 — Collaboration + Workflows

## Status

**Deferred until the 0.10 workspace surface and the 0.97 persistent runtime foundations are mature.**

## Goal

Turn basic workspace messaging into reliable multi-agent collaboration and then into bounded task/workflow execution, while reusing the existing runtime, intervention, policy, persistence, execution, and workspace contracts.

## Collaboration steps

1. First-class delegation/handoff operations over the existing workspace routing contracts.
2. Shared/private workspace context policies using the existing resource, identity, and authorization boundaries.
3. Parallel specialist work with bounded collaboration budgets and independent runtime-agent ownership.
4. Human intervention and approval points using the canonical 0.959 intervention contract; do not create a second approval engine.
5. Explicit runtime/participant lifecycle states by consuming the existing runtime/lifecycle contracts.
6. Cross-agent memory sharing only through explicit resource scope and policy.
7. Collaboration history, audit, and traceability through the existing observability/audit boundaries.

## Workflow steps

1. Task/job model and lifecycle over the durable goal/plan/recovery foundations established by 0.9591.
2. Planning, execution, and verification stages using the 0.97 cognitive planning contracts where cognition is involved.
3. Multi-step and branching workflows without replacing the persistent cognitive runtime's plan model.
4. Background execution and scheduling through the existing host-controlled runtime/scheduler boundaries.
5. Pause/resume and durable checkpoints through 0.9591 recovery contracts.
6. Event-triggered execution through the 0.952 event subsystem.
7. Per-step timeout, cancellation, retry, approval, intervention, and budget policies through existing runtime/policy/intervention contracts.

## Ownership boundaries

- **0.10** owns the host-facing workspace/routing/chat product surface.
- **0.959** owns the provider-neutral intervention boundary.
- **0.9591** owns durable goal/plan/checkpoint/recovery state.
- **0.96** owns concrete execution-target selection.
- **0.97** owns one runtime agent's persistent cognition and plan/goal use.
- **1.0** owns multi-agent collaboration and generic workflow orchestration built on those contracts.

No 1.0 feature may silently recreate a parallel runtime lifecycle, approval system, execution router, durable plan store, or cognitive-state ownership model.

## Boundary

These are generic orchestration facilities. HAgent does not become the authority for business rules, simulation state, or host-side side effects.

## Exit criterion

A host can coordinate multiple independent agents and long-running work with bounded execution, explicit authority, reusable intervention/policy boundaries, durable state where required, and observable collaboration without duplicating HAgent's lower-level runtime, execution, cognition, or authorization systems.

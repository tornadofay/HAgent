# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, current implementation state belongs under `docs/plan/`, and this directory defines the ordered delivery sequence.

## Current position

- 0.957 — Evaluation + Quality Measurement — completed and verified
- 0.9575 — Knowledge, Skills, Memory Governance + Learning — **CLOSED / VERIFIED through Slice 13**
- 0.9576 — Learned Resource Reliability + Adaptation — **CLOSED / VERIFIED through all five slices**
- 0.958 — Agent Lifecycle + Health — **CURRENT; Slice 1 CLOSED / VERIFIED, Slice 2 current**
- 0.9591 — Goal/Plan Persistence + Recovery — planned after 0.958
- 0.959 — Human-in-the-Loop + Intervention — planned
- 0.9592 — Provider Ecosystem + Adapter Lifecycle — planned
- 0.9593 — Reasoning Requirement + Boundary Foundation — planned prerequisite for reasoning-related 0.96/0.97 design
- 0.96.x — Configuration, Storage + Portability — cross-cutting foundation before 0.96
- 0.96 — Capability-Aware Execution — planned major execution foundation
- 0.97 — Persistent Cognitive Runtime — planned production V1 cognitive layer
- 0.98 — Model Reasoning Engineering — planned bounded reasoning-engineering layer
- 0.10 — Workspaces, Routing + Chat — deferred user-facing product surface
- 1.0 — Collaboration + Workflows — deferred orchestration layer

## Ordered V1 dependency chain

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Events
        ↓
0.953 Unified Policy
        ↓
0.954 Prompt / Instruction Governance
        ↓
0.955 Context Engineering
        ↓
0.956 Observability / Tracing
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 Knowledge / Skills / Memory Governance + Learning
        ↓
0.9576 Learned Resource Reliability + Adaptation
        ↓
0.958 Agent Lifecycle + Health
        ↓
0.9591 Goal / Plan Persistence + Recovery
        ↓
0.959 Human Intervention
        ↓
0.9592 Provider Ecosystem + Adapters
        ↓
0.9593 Reasoning Requirement / Boundary Foundation
        ↓
0.96.x Configuration / Storage / Portability
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
        ↓
0.98 Model Reasoning Engineering
        ↓
0.10 Workspaces / Routing / Chat
        ↓
1.0 Collaboration / Workflows
```

The ordering is dependency-driven. Ahead-of-roadmap implementation may exist, but it does not silently reorder or complete a phase.

## Phase ownership

- 0.9575 owns Skill/Knowledge/Memory governance and learning promotion.
- 0.9576 owns post-promotion learned-resource applicability, reliability, adaptation, retention, forgetting, and runtime reliability integration.
- 0.958 owns runtime-agent lifecycle and runtime health.
- 0.9591 owns durable goals/plans/checkpoints/recovery.
- 0.959 owns the canonical human/host intervention boundary.
- 0.9592 owns provider/adapter lifecycle and provider operational evidence.
- 0.9593 owns the provider-neutral reasoning requirement boundary.
- 0.96 owns concrete execution-target selection/admission.
- 0.97 owns persistent per-agent cognition.
- 0.98 owns bounded model reasoning engineering.

Later phases consume earlier contracts instead of recreating them.

## Runtime lifecycle boundary

Phase 0.9 established foundational runtime identity, execution lifecycle, retirement, shutdown, snapshots, and stale-result protection. Phase 0.958 extends the same runtime identity with long-lived lifecycle states and health. The authoritative lifecycle/health architecture is `docs/architecture/102-runtime-lifecycle-health.md`.

The 0.958 lifecycle target is:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

Health is separate:

```text
Healthy
Degraded
Failed
Unknown
```

Provider/adapter health remains a 0.9592 responsibility, and 0.96 consumes provider operational evidence for execution admission.

## Generated-view rule

Detailed phase history, verification evidence, and future scope remain in the numbered files under `docs/roadmap/`. This overview is intentionally a synchronized index rather than a second source of truth.

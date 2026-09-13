# HAgent Roadmap

> This file is a generated view. Source directory: `docs/roadmap`.
> Detailed phase history and authoritative ordering remain in the numbered files under `docs/roadmap/`.
> V2/research-only material belongs in `roadmapv2.md`.

## Current position

- 0.957 — Evaluation + Quality Measurement — complete
- 0.9575 — Knowledge, Skills, Memory Governance + Learning — **CLOSED / VERIFIED**
- 0.9576 — Learned Resource Reliability + Adaptation — **CLOSED / VERIFIED through all five slices**
- 0.958 — Agent Lifecycle + Health — **CURRENT; Slice 1 ready to implement**
- 0.9591 — Goal/Plan Persistence + Recovery — planned
- 0.959 — Human-in-the-Loop + Intervention — planned
- 0.9592 — Provider Ecosystem + Adapter Lifecycle — planned
- 0.9593 — Reasoning Requirement + Boundary Foundation — planned
- 0.96.x — Configuration, Storage + Portability — planned foundation
- 0.96 — Capability-Aware Execution — planned
- 0.97 — Persistent Cognitive Runtime — planned
- 0.98 — Model Reasoning Engineering — planned
- 0.10 — Workspaces, Routing + Chat — deferred
- 1.0 — Collaboration + Workflows — deferred

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

## Current phase handoff

0.9576 is complete and user-verified on .NET Framework 4.8.1 and .NET 9, including runtime integration. The user reported **260/260 passed, 0 failed, 0 skipped** for the full `.NET 9` `HAgent.Tests` suite after the fifth slice.

0.958 Slice 1 extends the existing runtime instance lifecycle without creating another runtime identity model. The target lifecycle is:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

The authoritative target architecture is `docs/architecture/102-runtime-lifecycle-health.md`.

**Example to run:** the new 0.958 Slice 1 runtime-lifecycle Example on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 runtime-lifecycle test class/file, followed by the full `HAgent.Tests` regression suite.

## Source-of-truth map

- Stable architecture: `docs/architecture/`
- Active implementation state: `docs/plan/`
- Ordered delivery path: `docs/roadmap/`
- Storage-specific details: `docs/storage.md`

This root file is only a synchronized generated view and must not become a second roadmap authority.

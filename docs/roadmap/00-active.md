# HAgent Active Roadmap

## Current

### 0.9575 — Knowledge, Skills, Memory Governance + Learning

**Current milestone — Slice 12 management work continues.**

- Complete the canonical learning lifecycle gate.
- Persist learning candidates with retention and expiry.
- Add Learning Review workflow and durable review state.
- Promote approved Memory candidates into authoritative Memory.
- Promote Knowledge candidates as new authoritative versions.
- Promote Skill candidates as new immutable versions.
- Persist promotion provenance, evaluation, and audit records.
- Integrate governed learned resources with Context and Instruction.
- Capture execution outcomes and observations as learning input.
- Add learning/resource observability.
- Add Learning Review, Knowledge/Wiki, Skill, and Agent Configuration UI.
- Verify persistence, restart/recovery, lifecycle, authorization, concurrency, context integration, UI, and audit behavior on both supported targets.

#### Learning lifecycle

```text
Proposed
   ↓ lifecycle / policy evaluation
PendingReview
   ├──→ Rejected
   ↓
Approved
   ↓ governed authoritative promotion
Promoted
```

- `Proposed`: candidate formed; review has not necessarily been required yet.
- `PendingReview`: lifecycle/policy requires the review boundary.
- `Approved`: accepted through the review boundary, but still a candidate rather than an authoritative resource.
- `Rejected`: not accepted for authoritative promotion.
- `Promoted`: approved candidate converted into the authoritative resource through the separate promotion capability.

Learning mode and policy control the permitted paths. Review and promotion are intentionally separate boundaries.

### Slice 12 — Management UI — VERIFIED through Learning Review promotion

Verified by user on 2026-09-11 on both .NET Framework 4.8.1 and .NET 9:

- durable Learning Review list and review actions;
- host-supplied read-only reviewer identity;
- candidate-store injection aligned with host storage configuration;
- filterable candidate workspace by lifecycle status and candidate type;
- read-only candidate details including payload, provenance/evidence, lifecycle, policy, source execution/runtime, and review evidence;
- governed Promote action for selected `Approved` candidates through the injected `AiLearningPromotionService`;
- fresh `learning.promote` authorization remains inside the existing promotion service;
- successful promotion refreshes the candidate to `Promoted` revision `3`;
- `Learning Review Seed` → real WinForms configuration flow → Approve → Promote → `Learning Review Verify` succeeded on both targets;
- fresh candidate-store reopen and persisted reviewer identity/policy evidence were verified on both targets.

The Learning Review management boundary is closed.

### Next Slice 12 management work — Authoritative Resource Inventory

- Establish one provider-neutral authoritative resource inventory boundary.
- Inventory authoritative Memory using the existing memory-store contract.
- Inventory authoritative Knowledge/Wiki through provider-neutral resource-source/query contracts.
- Inventory authoritative Skills through provider-neutral definition-source/query contracts.
- Expose bounded resource metadata and explicit resource scope.
- Connect effective agent resource visibility back into management views without duplicating authoritative resource models.
- Keep specialized known-resource panels possible while preserving generic inventory for future resource types.
- Add focused WinForms management pages under `src/HAgent.WinForms/UI/Configuration/`.
- Add matching `HAgent.Tests` contract/boundary verification and a dedicated `HAgent.Example` scenario.
- Do not implement SQL Server/MySQL storage in this increment.
- Keep resource reliability/adaptation separate for `0.9576`, where staleness, contradiction, drift, revalidation, quarantine, retirement, archival, forgetting, and replacement are planned.

## Planned order

### 0.9576 — Learned Resource Reliability + Adaptation

- Applicability and validity outcomes.
- Reliability evidence from validated outcomes.
- Staleness, contradiction, drift, and revalidation handling.
- Quarantine, retirement, archival, and forgetting.
- Replacement candidates without in-place mutation of published resources.
- Runtime integration and verification.

### 0.958 — Agent Lifecycle + Health

- Extend runtime lifecycle for long-lived agents.
- Add runtime health state.
- Add bounded progress and recovery signals.
- Add lifecycle/health observability and verification.

### 0.9591 — Goal/Plan Persistence + Recovery

- Durable goals and intentions.
- Durable plans and plan steps.
- Checkpoints and outcome states.
- Retry/idempotency records.
- Restart recovery and stale-revision protection.
- File, SQL Server, and MySQL verification.

### 0.959 — Human Intervention

- Canonical intervention request/result contract.
- Execution and learning-candidate intervention integration.
- Plan-step and goal/intention intervention.
- Host-defined consequential-action intervention boundary.
- Persistence, UI, diagnostics, stale-request protection, and verification.

### 0.9592 — Provider Ecosystem + Adapter Lifecycle

- Provider adapter lifecycle contract.
- Provider/model/execution-target discovery normalization.
- Capability, quota, rate, usage, and health evidence.
- Adapter compatibility and replacement handling.
- Deterministic adapter verification.

### 0.96.x — Configuration, Storage + Portability

- Authoritative configuration model.
- Encrypted provider credentials at rest.
- Resource and relationship persistence.
- Runtime configuration snapshots and invalidation.
- Versioned export/import.
- File, SQL Server, MySQL parity and multi-process behavior.

### 0.96 — Capability-Aware Execution

- Concrete execution-target model.
- Capability and constraint evaluation.
- Provider discovery evidence integration.
- Cost and selection policy.
- Rate, quota, concurrency, and capacity admission.
- Health, latency, fallback, and long-running execution handling.
- Execution planning diagnostics.
- Management UI and verification.

### 0.97 — Persistent Cognitive Runtime

- Single-owner cognitive state and revisions.
- Observations, beliefs, and bounded DecisionWorkspace.
- Goals, intentions, and reconsideration.
- Deterministic/reactive processing before unnecessary model calls.
- Provider-neutral reasoning requirements and bounded deliberation.
- Plan execution and recovery integration.
- Experience capture, governed learning, and learned-resource reliability consumption.
- Lifecycle, persistence, observability, evaluation, management workbench, and production verification.

### 0.10 — Workspaces, Routing + Chat

- Complete workspace message execution through runtime agents.
- Workspace addressing and loop protection.
- Persistent lobby/private chat and participant state.
- Workspace UI and configuration.
- Approval presentation/resolution.
- File, SQL Server, and MySQL persistence verification.

### 1.0 — Collaboration + Workflows

- First-class agent delegation and handoff.
- Shared/private context policy.
- Bounded parallel specialist work.
- Collaboration history, audit, and traceability.
- Task/workflow model and lifecycle.
- Multi-step workflow execution.
- Background execution and scheduling.
- Durable checkpoints, pause/resume, cancellation, retry, approval, and budget handling.

## Dependency order

```text
0.9575
  ↓
0.9576
  ↓
0.958
  ↓
0.9591
  ↓
0.959
  ↓
0.9592
  ↓
0.96.x
  ↓
0.96
  ↓
0.97
  ↓
0.10
  ↓
1.0
```

Only current and planned implementation work belongs here. V2 and research candidates are maintained separately in `roadmapv2.md`.

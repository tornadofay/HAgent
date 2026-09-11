# HAgent Active Roadmap

## Current

### 0.9575 — Knowledge, Skills, Memory Governance + Learning

**Current milestone — Slice 12 in progress.**

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

### Slice 12 current work — Learning Review management UI

- Durable Learning Review list and review actions.
- Host-supplied read-only reviewer identity.
- Candidate-store injection aligned with host storage configuration.
- Filterable candidate workspace by lifecycle status and candidate type.
- Read-only candidate details including payload, provenance/evidence, lifecycle, policy, source execution/runtime, and review evidence.
- Manual .NET Framework 4.8.1 and .NET 9 verification of the management workflow.

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

# HAgent Active Roadmap

## Current

### 0.958 — Agent Lifecycle + Health

**Slice 1 — Lifecycle state extension — CURRENT / ready to implement.**

Phase 0.9576 Learned Resource Reliability + Adaptation is fully verified through all five slices. The user verified its matching Examples on .NET Framework 4.8.1 and .NET 9 and reported the full `.NET 9` `HAgent.Tests` suite at **260/260 passed, 0 failed, 0 skipped** after runtime integration.

0.958 establishes explicit long-lived runtime lifecycle and health without creating a second runtime identity model. Its target architecture is `docs/architecture/102-runtime-lifecycle-health.md`.

#### Slice 1 — Lifecycle state extension

- Extend the existing runtime lifecycle to `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`.
- Define valid/invalid transitions and terminal behavior.
- Reject ordinary new runtime-originated work while suspended, recovering, retired, or shutdown.
- Preserve execution identity, execution terminal-state handling, and stale-result protection.
- Use lifecycle/revision changes to prevent obsolete asynchronous work from regaining authority.
- Preserve runtime-owned durable state during suspension/recovery.
- Add focused tests and a matching Example through the normal Example registration/classification path.

**Example to run:** the new Slice 1 runtime-lifecycle Example on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** the focused Slice 1 runtime-lifecycle test class/file, followed by the full `HAgent.Tests` regression suite.

## Closed immediately before current phase

### 0.9576 — Learned Resource Reliability + Adaptation — CLOSED / VERIFIED

All five slices are complete:

1. Applicability and validity.
2. Reliability evidence and validated outcome feedback.
3. Adaptation, staleness, contradiction, and revalidation.
4. Forgetting and archival.
5. Runtime integration.

Authoritative detail: `docs/roadmap/9576-learned-resource-reliability-adaptation-consolidation.md`.

### 0.9575 — Knowledge, Skills, Memory Governance + Learning — CLOSED

Slices 1–13 are verified. Authoritative detail: `docs/roadmap/9575-resource-governance-learning.md`.

## Planned order

### 0.9591 — Goal/Plan Persistence + Recovery

- Durable goals and intentions.
- Durable plans and plan steps.
- Checkpoints and outcome states.
- Retry/idempotency records.
- Restart recovery and stale-revision protection.

### 0.959 — Human-in-the-Loop + Intervention

Consumes the canonical intervention boundary while 0.958 owns runtime lifecycle transitions caused by authorized intervention.

### 0.9592 — Provider Ecosystem + Adapter Lifecycle

Provider/adapter lifecycle and provider operational evidence remain separate from runtime-agent lifecycle and health.

### 0.9593 — Reasoning Requirement + Boundary Foundation

Provider-neutral reasoning requirement contract before later capability-aware execution and reasoning engineering.

### Later V1 order

`0.96.x → 0.96 → 0.97 → 0.98 → 0.10 → 1.0`

The ordered dependency chain and full historical roadmap remain authoritative in the numbered `docs/roadmap/` phase documents and `docs/roadmap/00-overview.md`.

# HAgent Active Roadmap

## Current

### 0.9591 — Goal/Plan Persistence + Recovery

**Slice 1 — Durable goal/intention contracts — CURRENT / implemented, verification pending.**

Phase 0.958 Agent Lifecycle + Health is closed at Slice 1 after user verification on .NET Framework 4.8.1 and .NET 9 Windows and a full .NET 9 `HAgent.Tests` result of **266/266 passed, 0 failed, 0 skipped**.

0.9591 establishes durable provider-neutral authority for long-lived goals, intentions, plans, checkpoints, and recovery without persisting transient execution machinery. Its ordered delivery boundary is `docs/roadmap/9591-goal-plan-persistence-recovery.md` and the detailed Slice 1 contract architecture is `docs/architecture/103-goal-plan-persistence-recovery.md`.

#### Slice 1 — Durable goal/intention contracts

- Define stable goal and intention IDs as distinct identities.
- Define explicit goal/intention statuses and priority metadata.
- Preserve constraints, provenance, timestamps, and revision metadata.
- Distinguish `HostSupplied` goal state from `AgentInferred` state.
- Define intention adoption metadata.
- Preserve attributable reasons/evidence/authority for intention status changes.
- Add focused tests and a matching Example through the normal Example registration/classification path.

**Example to run:** `HAgent.Example → Cognition → Goals & Plans → GOAL & INTENTION CONTRACTS` on .NET Framework 4.8.1 and .NET 9 Windows.

**Tests to run:** `tests/HAgent.Tests/GoalIntentionContractsTests.cs` focused first, followed by the full `HAgent.Tests` regression suite.

## Closed immediately before current phase

### 0.958 — Agent Lifecycle + Health — CLOSED / VERIFIED

Slice 1 is verified on both supported Example targets. The lifecycle Example verified state transitions, lifecycle revision capture, non-active admission rejection, stale-result invalidation, runtime-state persistence/restore, and shutdown cancellation. The full `.NET 9` regression suite passed **266/266**.

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

### 0.959 — Human-in-the-Loop + Intervention

Consumes the canonical intervention boundary while 0.959 owns runtime lifecycle transitions caused by authorized intervention.

### 0.9592 — Provider Ecosystem + Adapter Lifecycle

Provider/adapter lifecycle and provider operational evidence remain separate from runtime-agent lifecycle and health.

### 0.9593 — Reasoning Requirement + Boundary Foundation

Provider-neutral reasoning requirement contract before later capability-aware execution and reasoning engineering.

### Later V1 order

`0.96.x → 0.96 → 0.97 → 0.98 → 0.10 → 1.0`

The ordered dependency chain and full historical roadmap remain authoritative in the numbered `docs/roadmap/` phase documents and `docs/roadmap/00-overview.md`.

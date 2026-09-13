# HAgent Active Roadmap

## Current

### 0.958 — Agent Lifecycle + Health

**Slice 2 — Health state — IMPLEMENTED / VERIFICATION PENDING.**

0.958 owns runtime-agent lifecycle and runtime health on the existing `AgentRuntimeInstance` foundation. Slice 1 is closed and verified on both supported Example targets and the full `.NET 9` regression suite passed **266/266**.

#### Slice 1 — Lifecycle state extension — CLOSED / VERIFIED

- Extend the existing runtime lifecycle to `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`.
- Define valid/invalid transitions and terminal behavior.
- Reject ordinary new runtime-originated work while non-active.
- Preserve execution identity, lifecycle revision, and stale-result protection.
- Preserve runtime-owned durable state during suspension/recovery.
- Add focused tests and the matching lifecycle Example.

#### Slice 2 — Health state — IMPLEMENTED / VERIFICATION PENDING

- Define normalized health status: `Healthy`, `Degraded`, `Failed`, `Unknown`.
- Bound health reason/evidence metadata.
- Record source category for runtime observations, recovery outcomes/failures, host signals, or equivalent provider-neutral evidence.
- Distinguish transient degradation from terminal failure.
- Do not treat slow but valid inference as failed from elapsed time alone.
- Keep health separate from lifecycle and authorization.
- Persist health through the existing runtime-state persistence boundary without creating a parallel repository.
- Add focused tests and a matching public Example through the normal Example registration/classification path.

**Implementation checkpoint:** `AiRuntimeHealth` is owned by `AgentRuntimeInstance`; health is persisted through the existing File/SQL Server/MySQL runtime-state stores; focused tests are in `tests/HAgent.Tests/RuntimeHealthTests.cs`; Example is `HAgent.Example → Runtime → Runtime Instances → RUNTIME HEALTH`.

**Verification to run:** the RUNTIME HEALTH Example on .NET Framework 4.8.1 and .NET 9 Windows, then focused and full `HAgent.Tests` regression verification.

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

Follows completion of all 0.958 slices. Slice 1 contracts were built and verified ahead of roadmap but remain phase-entry pending.

### 0.959 — Human-in-the-Loop + Intervention

Consumes the canonical intervention boundary while 0.959 owns runtime lifecycle transitions caused by authorized intervention.

### 0.9592 — Provider Ecosystem + Adapter Lifecycle

Provider/adapter lifecycle and provider operational evidence remain separate from runtime-agent lifecycle and health.

### 0.9593 — Reasoning Requirement + Boundary Foundation

Provider-neutral reasoning requirement contract before later capability-aware execution and reasoning engineering.

### Later V1 order

`0.96.x → 0.96 → 0.97 → 0.98 → 0.10 → 1.0`

The ordered dependency chain and full historical roadmap remain authoritative in the numbered `docs/roadmap/` phase documents and `docs/roadmap/00-overview.md`.

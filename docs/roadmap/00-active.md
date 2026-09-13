# HAgent Active Roadmap

## Current

### 0.958 — Agent Lifecycle + Health

**Slice 3 — Progress and recovery signals — IMPLEMENTED / VERIFICATION PENDING.**

0.958 owns runtime-agent lifecycle, runtime health, bounded runtime progress evidence, stall assessment, and explicit recovery outcomes on the existing `AgentRuntimeInstance` foundation.

#### Slice 1 — Lifecycle state extension — CLOSED / VERIFIED

- Extend the existing runtime lifecycle to `Active`, `Suspended`, `Recovering`, `Retired`, and terminal `Shutdown`.
- Define valid/invalid transitions and terminal behavior.
- Reject ordinary new runtime-originated work while non-active.
- Preserve execution identity, lifecycle revision, and stale-result protection.
- Preserve runtime-owned durable state during suspension/recovery.
- Add focused tests and the matching lifecycle Example.

#### Slice 2 — Health state — CLOSED / VERIFIED

- Define normalized health status: `Healthy`, `Degraded`, `Failed`, `Unknown`.
- Bound health reason/evidence metadata.
- Record source category for runtime observations, recovery outcomes/failures, host signals, or equivalent provider-neutral evidence.
- Distinguish transient degradation from terminal failure.
- Do not treat slow but valid inference as failed from elapsed time alone.
- Keep health separate from lifecycle and authorization.
- Persist health through the existing runtime-state persistence boundary without creating a parallel repository.

**Verified:** both required RUNTIME HEALTH Examples succeeded on .NET Framework 4.8.1 and .NET 9; the full `.NET 9` `HAgent.Tests` suite reported **276/276 passed, 0 failed, 0 skipped**.

#### Slice 3 — Progress and recovery signals — IMPLEMENTED / VERIFICATION PENDING

- Provide bounded progress/heartbeat metadata where a host needs it.
- Require strictly increasing progress evidence sequence numbers.
- Detect clearly stalled work only when a host-configured silence threshold and an actual progress/heartbeat observation support that conclusion.
- Do not mutate lifecycle automatically from a stall assessment.
- Support explicit transition into `Recovering` without creating a second runtime identity or deleting durable runtime state.
- Make recovery outcome explicit as succeeded, failed, or cancelled.
- Complete recovery only from `Recovering`, advancing lifecycle revision and preserving runtime identity.
- Do not allow failed/cancelled recovery to return directly to `Active`.

**Implementation checkpoint:** `AiRuntimeProgressSnapshot`, `AiRuntimeStallPolicy`, `AiRuntimeProgressMonitor`, `AiRuntimeRecoveryResult`, and `AgentRuntimeInstance` progress/recovery APIs are implemented; focused tests are in `tests/HAgent.Tests/RuntimeProgressRecoveryTests.cs`; Example is `HAgent.Example → Runtime → Runtime Instances → RUNTIME PROGRESS & RECOVERY`.

**Verification to run:** the RUNTIME PROGRESS & RECOVERY Example on .NET Framework 4.8.1 and .NET 9 Windows, then focused and full `HAgent.Tests` regression verification.

## Closed immediately before current phase

### 0.9576 — Learned Resource Reliability + Adaptation — CLOSED / VERIFIED

All five slices are complete.

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

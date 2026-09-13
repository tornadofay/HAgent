# HAgent Active Roadmap

## Current

### 0.958 — Agent Lifecycle + Health

**Slice 4 — Observability and verification — IMPLEMENTED / VERIFICATION PENDING.**

0.958 owns runtime-agent lifecycle, runtime health, bounded runtime progress evidence, explicit recovery outcomes, and bounded runtime observability on the existing `AgentRuntimeInstance` foundation.

#### Slice 1 — Lifecycle state extension — CLOSED / VERIFIED

Verified on both required Example targets; full `.NET 9` `HAgent.Tests`: **266/266 passed, 0 failed, 0 skipped**.

#### Slice 2 — Health state — CLOSED / VERIFIED

Verified on both required Example targets; full `.NET 9` `HAgent.Tests`: **276/276 passed, 0 failed, 0 skipped**.

#### Slice 3 — Progress and recovery signals — CLOSED / VERIFIED

Verified on both required Example targets; full `.NET 9` `HAgent.Tests`: **283/283 passed, 0 failed, 0 skipped**.

#### Slice 4 — Observability and verification — IMPLEMENTED / VERIFICATION PENDING

- Provide detached `AiRuntimeObservation` evidence for lifecycle, health, progress, and recovery.
- Provide bounded `AiRuntimeDiagnosticsSnapshot` through `AiRuntimeDiagnosticsService`.
- Adapt detached runtime observations into the existing `IEventDispatcher` / `EventEnvelope` boundary using Runtime source/scope.
- Keep observability descriptive only; it does not authorize, route, or mutate runtime state.
- Focused tests: `tests/HAgent.Tests/RuntimeObservabilityTests.cs` and `tests/HAgent.Tests/RuntimeObservationPublisherTests.cs`.
- Example: `HAgent.Example → Runtime → Diagnostics → RUNTIME OBSERVABILITY`.

**Verification to run:** the RUNTIME OBSERVABILITY Example on .NET Framework 4.8.1 and .NET 9 Windows, then both focused test classes and the full `HAgent.Tests` regression suite.

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

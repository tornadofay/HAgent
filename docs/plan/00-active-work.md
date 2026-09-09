# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.956 Observability and Distributed Tracing
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Build the ordered 0.956 observability/tracing foundation after verified completion of 0.955 Context Engineering.

## Current checkpoint

0.956 Slice 4 sampling and bounded retention controls are complete at the implementation/test/Example verification boundary. Deterministic sampling, unsampled inheritance/suppression, bounded trace/span retention, aggregate metadata limits, and lifecycle independence are verified.

## Current run

**0.956 Slice 5 integrated trace sinks and safe export boundary — CURRENT.**

This slice introduces a provider-neutral sink boundary for completed trace spans. Sink delivery must remain observational only: sink latency/failure cannot change execution correctness, and Core must remain independent of telemetry vendors, remote transports, and persistence technologies.

## Implemented in Slice 4

- Added `TraceSamplingOptions`, `ITraceSampler`, and `TraceRetentionOptions` to the provider-neutral trace contracts.
- Added `DeterministicTraceSampler` using stable correlation/operation material and a configurable sample rate/salt.
- Extended `InMemoryTraceRecorder` with optional sampling and bounded retention while preserving the existing default constructor behavior.
- Sampling is decided for root spans and inherited through child `TraceContext`; unsampled spans still receive normal trace identity and can complete normally, but are not retained.
- Retention bounds include maximum trace count, maximum span count, maximum spans per trace, aggregate metadata characters, and maximum age.
- Retention eviction is trace-aware and avoids evicting the active trace simply to admit another child span when the configured span/metadata bound is reached.
- Added focused tests in `tests/HAgent.Tests/ObservabilitySamplingRetentionTests.cs` and the matching public-API Example in `src/HAgent.Example/MainForm.ObservabilitySamplingRetention.cs`.

## Slice 4 verification

- **User verification — .NET 9, 2026-09-09:** full `HAgent.Tests` completed with **67/67 tests passed**.
- **User Example verification — .NET Framework 4.8.1, 2026-09-09 04:16:56:** sampling/retention scenario succeeded with deterministic sampling, inheritance/suppression, lifecycle independence, trace/span/metadata bounds, no provider transport, and no real provider request.
- **User Example verification — .NET 9, 2026-09-09 04:17:25:** same scenario succeeded with the same checks.
- The latest supplied message did not separately report a solution-build result, so no separate build claim is recorded here.

## Slice 5 boundary

- Define the provider-neutral trace sink/export contract for completed spans.
- Preserve default-deny/redaction and bounded metadata semantics at the sink boundary.
- Support deterministic in-process sink tests, ordering, and completion behavior.
- Ensure slow/rejecting sinks cannot alter execution correctness.
- Keep remote telemetry transport, durable trace persistence, management UI, and broad diagnostic projection out of this slice.
- Add focused `HAgent.Tests` coverage and a matching public-API `HAgent.Example` under `Diagnostics → Observability → Observability Sinks`.
- Verify with solution build, full `HAgent.Tests`, and the exact Example scenario on .NET Framework 4.8.1 and .NET 9 using deterministic in-process sinks/fakes only.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.956 Observability and Distributed Tracing
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Build the ordered 0.956 observability/tracing foundation after verified completion of 0.955 Context Engineering.

## Current checkpoint

0.956 Slice 3 trace-producing runtime instrumentation and propagation are complete and verified. Provider-neutral execution, policy, provider, tool, context, and event tracing producers are implemented; explicit trace propagation and event trace-context propagation are covered; payload exclusion and nested ambient-context restoration are verified.

## Current run

**0.956 Slice 4 sampling and bounded retention controls — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING.**

This slice adds provider-neutral deterministic sampling and bounded in-memory retention. Sampling and retention remain observability concerns only and do not alter execution correctness, authorization, event delivery, or execution audit semantics.

## Implemented in Slice 4

- Added `TraceSamplingOptions`, `ITraceSampler`, and `TraceRetentionOptions` to the provider-neutral trace contracts.
- Added `DeterministicTraceSampler` using stable correlation/operation material and a configurable sample rate/salt.
- Extended `InMemoryTraceRecorder` with optional sampling and bounded retention while preserving the existing default constructor behavior.
- Sampling is decided for root spans and inherited through child `TraceContext`; unsampled spans still receive normal trace identity and can complete normally, but are not retained.
- Retention bounds include maximum trace count, maximum span count, maximum spans per trace, aggregate metadata characters, and maximum age.
- Retention eviction is trace-aware and avoids evicting the active trace simply to admit another child span when the configured span/metadata bound is reached.
- Added `tests/HAgent.Tests/ObservabilitySamplingRetentionTests.cs` covering deterministic sampling, unsampled inheritance/suppression, retention bounds, per-trace limits, aggregate metadata limits, and lifecycle independence.
- Added `src/HAgent.Example/MainForm.ObservabilitySamplingRetention.cs` and registered it under `Diagnostics → Observability → Observability Sampling & Retention`.

## Slice 4 verification boundary

- Build the solution after pulling the current branch.
- Run the full `HAgent.Tests` suite.
- Run `HAgent.Example → Diagnostics → Observability → Observability Sampling & Retention → Run sampling & retention test` on .NET Framework 4.8.1.
- Run the same Example on .NET 9.
- Confirm sampling/retention bounds and deterministic behavior without any real provider request.
- Do not mark Slice 4 verified until all required local results are supplied.
- Do not begin Slice 5 in the same run.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.956 Observability and Distributed Tracing
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Build the ordered 0.956 observability/tracing foundation after verified completion of 0.955 Context Engineering.

## Current checkpoint

0.956 Slice 5 integrated trace sinks and the safe export boundary are complete at the implementation/test/Example verification boundary. Provider-neutral sinks, bounded asynchronous delivery, failure isolation, queue saturation handling, sampling suppression, and retention-boundary suppression are verified on both supported Example targets.

## Current run

**0.956 Slice 6 safe human-readable diagnostic projection — CURRENT.**

This slice adds a bounded provider-neutral projection for management/diagnostic UI consumers. It must consume retained trace data through a safe projection rather than exposing raw trace storage or sink payloads, and must preserve the existing default-deny sensitive-data boundary.

## Slice 6 scope

- Define a bounded diagnostic projection over retained `TraceSpan` data.
- Expose only safe status, operation, timing, correlation, parent/trace identity, and bounded metadata needed for diagnostic display.
- Preserve deterministic ordering and bounded result/text sizes.
- Explicitly omit or mark redacted sensitive values; never surface prompts, responses, tool arguments/results, host raw context, secrets, credentials, connection strings, or arbitrary serialized payloads.
- Keep projection concerns separate from raw trace storage and sink/export contracts.
- Add focused `tests/HAgent.Tests` coverage for ordering, projection bounds, metadata safety, correlation visibility, and sensitive-data exclusion.
- Add matching public-API `HAgent.Example` coverage under `Diagnostics → Observability → Observability Diagnostic Projection` using deterministic in-process trace data only.

## Slice 6 verification boundary

- Build the solution after implementation.
- Run the full `HAgent.Tests` suite.
- Run `HAgent.Example → Diagnostics → Observability → Observability Diagnostic Projection` on .NET Framework 4.8.1.
- Run the same Example on .NET 9.
- Confirm no real provider request or remote telemetry transport is contacted.
- Do not mark Slice 6 verified until all required local results are supplied.
- Do not begin Slice 7 in the same run.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

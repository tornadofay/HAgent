# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.956 Observability and Distributed Tracing
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Build the ordered 0.956 observability/tracing foundation after verified completion of 0.955 Context Engineering.

## Current checkpoint

0.956 Slice 6 safe human-readable diagnostic projection is complete and verified on both supported Example targets. The provider-neutral bounded projection exposes diagnostic trace structure without exposing raw trace storage, prompts, provider responses, tool payloads, host context, secrets, or arbitrary objects.

## Current run

**0.956 Slice 7 cross-process trace context and correlation boundary — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING.**

Slice 7 now provides a bounded provider-neutral host/transport boundary for moving `TraceContext` and existing correlation identities across process boundaries. It does not implement a wire transport or remote telemetry service.

## Implemented in Slice 7

- Added bounded `TracePropagationCarrier`, `TracePropagationImportOptions`, `TracePropagationImportStatus`, and `TracePropagationImportResult` contracts.
- Extended `TracePropagation` with deterministic export/import of trace context and bounded deployment, tenant, principal, user, session, workspace, agent-profile, runtime, execution, execution-correlation, host-correlation, event, and causation identities.
- Preserved distinct trace, execution, host, event, and causation identities rather than collapsing them into one cross-process value.
- Added explicit host trust acceptance for incoming trace context; the default rejects untrusted incoming trace context.
- Added safe handling for missing, malformed, incomplete, unsampled, and oversized incoming propagation values.
- Kept Core independent of HTTP headers, W3C/OpenTelemetry types, vendor SDKs, message buses, and remote telemetry delivery.
- Added focused `tests/HAgent.Tests/ObservabilityTracePropagationTests.cs` covering round-trip propagation, identity separation, unsampled state, missing context, explicit trust rejection, malformed trace input, invalid sampled state, oversized correlation input, and carrier clone/bounds behavior.
- Added `src/HAgent.Example/MainForm.ObservabilityTracePropagation.cs` and registered it under `Diagnostics → Observability → Observability Trace Propagation`.
- Updated `docs/architecture/22-observability.md` with the cross-process propagation boundary and its trust/validation semantics.

## Slice 7 verification boundary

- Build the solution after pulling the current branch.
- Run the full `HAgent.Tests` suite.
- Run `HAgent.Example → Diagnostics → Observability → Observability Trace Propagation` on .NET Framework 4.8.1.
- Run the same Example on .NET 9.
- Confirm no real network transport, remote telemetry service, or real provider request is contacted.
- Do not mark Slice 7 verified until all required local results are supplied.
- Do not begin Slice 8 in the same run.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

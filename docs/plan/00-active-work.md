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

**0.956 Slice 7 cross-process trace context and correlation boundary — CURRENT.**

This slice defines the provider-neutral host/transport boundary for moving trace context and existing correlation identity across process boundaries without coupling HAgent.Core to HTTP, OpenTelemetry, a message format, or a telemetry vendor.

## Completed immediately before Slice 7

- Slice 6 added bounded `TraceDiagnosticProjectionOptions`, `TraceDiagnosticMetadataItem`, `TraceDiagnosticSpan`, and `TraceDiagnosticProjection` contracts.
- Slice 6 added `TraceDiagnosticProjector` with deterministic ordering and bounded trace/span identity, timing, status, parentage, correlation, and safe diagnostic metadata.
- Slice 6 added focused tests for safe allowlisting, omission accounting, explicit redaction markers, parent/correlation relationships, bounds, status/duration, and unsampled suppression.
- Slice 6 added the public-API Example under `Diagnostics → Observability → Observability Diagnostic Projection`.
- **User verification — 2026-09-09:** full `HAgent.Tests` completed with **77/77 tests passed**.
- **User Example verification — .NET Framework 4.8.1, 2026-09-09 05:08:18:** diagnostic projection succeeded with all stated safety and determinism checks.
- **User Example verification — .NET 9, 2026-09-09 05:09:01:** same scenario succeeded with the same checks.

## Slice 7 scope

- Define the bounded provider-neutral import/export representation needed for `TraceContext` and relevant existing correlation identifiers at a host/transport boundary.
- Preserve the distinction between `TraceId`/`SpanId`/`ParentSpanId` and `ExecutionId`/`ExecutionCorrelationId`/`HostCorrelationId`/`EventId`/`CausationId`.
- Define deterministic handling for missing, malformed, incomplete, unsampled, or otherwise untrusted incoming trace context.
- Keep Core independent of HTTP headers, W3C/OpenTelemetry types, vendor SDKs, message buses, and remote telemetry services.
- Add focused `HAgent.Tests` coverage and a matching public-API `HAgent.Example` using deterministic in-process transport fakes only.
- Do not implement real network transport or remote telemetry delivery in this slice.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

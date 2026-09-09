# Phase 0.956 — Observability and Distributed Tracing

## Status

**Completed and verified on .NET Framework 4.8.1 and .NET 9.**

## Goal

Turn HAgent execution, resource use, policy decisions, cognition, tools, provider activity, and lifecycle changes into a coherent structured trace that can be correlated across operations and processes.

## Verified outcome

All requirements in this phase are implemented and verified through the ordered Slice 1–8 sequence.

- Provider-neutral trace/span concepts and deterministic lifecycle semantics are implemented.
- Execution, runtime, host, event, policy, tool, provider, and other applicable identities remain distinct and correlatable.
- Start/end, duration, status, hierarchy, parentage, bounded metadata, redaction, sampling, retention, and sink boundaries are implemented.
- Context, policy, provider, tool, event, execution, and outcome boundaries emit trace information without duplicating or replacing their authoritative subsystems.
- Secrets, raw prompts/responses, provider payloads, tool payloads, host raw context, and arbitrary serialized objects remain excluded by default.
- Local tracing, bounded asynchronous sinks, diagnostic projection, sampling/retention, and cross-process propagation are implemented without forcing a telemetry vendor or transport.
- Failure, retry, retry-wait, recovery, fallback, cancellation, policy denial, and stale-result observability are represented from the subsystem that owns each decision. In particular, the execution runtime publishes authoritative outcome facts and tracing consumes them; provider adapters do not reconstruct execution state with `AsyncLocal`, call counting, or exception-message inspection.
- Observability remains diagnostic and cannot alter authorization, execution decisions, cancellation, retry, or terminal-state authority.

## Verification

**User verification — 2026-09-09:** `HAgent.Tests` completed with **89/89 tests passed**.

**User Example verification — .NET Framework 4.8.1, 2026-09-09 06:11:32:** `Observability Outcome Tracing` succeeded, verifying authoritative retry, retry-wait, recovery, fallback decision representation, stale-result rejection, absence of provider-side retry inference, unchanged execution terminal authority, no raw prompts/responses/provider payloads, no remote telemetry, and no real provider request.

**User Example verification — .NET 9, 2026-09-09 06:12:14:** same public-API scenario succeeded with the same checks.

## Architectural invariants

```text
Trace context                     Execution outcome state
-------------                     -----------------------
Where am I?                       What did runtime decide?
TraceId / ParentSpanId            Attempt / retry number
Sampled state                     Fallback transition
                                  Wait/backpressure decision
                                  Recovery
                                  Stale-result acceptance/rejection
```

- Trace context is ambient relationship state. It must not be used to infer authoritative runtime behavior.
- The logical execution runtime remains authoritative for retry classification, retry count, fallback selection, waits, cancellation/timeout, terminal completion, and stale-result acceptance/rejection.
- `IExecutionObservationSource` is the provider-neutral boundary through which an execution runtime publishes bounded facts that tracing may consume.
- `TracingProviderAdapter` records provider operation boundaries only. It does not reconstruct retry/fallback state with `AsyncLocal`, call counting, or exception-message inspection.
- Fallback is observable only when the execution runtime actually selects a fallback target; multiple provider spans alone do not prove fallback.
- Observability remains diagnostic and cannot change execution, authorization, cancellation, retry, or terminal-state behavior.

## Architectural outcome

```text
Event / Request
      ↓
Trace identity/context
      ↓
Root execution span
      │
      ├── Policy
      ├── Context
      ├── Planning
      ├── Provider invocation spans
      ├── Tools
      └── Outcome observations
             ↑
             │ explicit runtime facts
      Execution runtime
```

Tracing is observability, not authorization and not transcript storage.
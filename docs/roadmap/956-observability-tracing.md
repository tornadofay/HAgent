# Phase 0.956 — Observability and Distributed Tracing

## Status

**Planned architectural foundation before capability-aware execution and persistent cognition.**

## Goal

Turn HAgent execution, resource use, policy decisions, cognition, tools, provider activity, and lifecycle changes into a coherent structured trace that can be correlated across operations and processes.

## Requirements

1. [ ] Define provider-neutral trace/span concepts for HAgent operations.
2. [ ] Correlate deployment, tenant, user/session, workspace, agent, runtime, execution, tool-call, provider-target, event, policy, and evaluation activity where applicable.
3. [ ] Represent operation start/end, duration, status, parent relationship, decision reason, and safe metadata.
4. [ ] Trace context assembly, resource retrieval, policy evaluation, candidate selection, admission, provider execution, tool execution, learning, and cognitive transitions.
5. [ ] Support configurable redaction of prompts, responses, arguments, host context, and other sensitive data.
6. [ ] Keep secrets, credentials, raw connection strings, and sensitive payloads out of traces by default.
7. [ ] Support local/in-memory tracing plus host-integrated sinks without forcing one telemetry vendor or transport.
8. [ ] Define bounded trace retention and sampling controls.
9. [ ] Preserve cross-process correlation for network/database-backed deployments where identity is available.
10. [ ] Make stale-result rejection, policy denial, fallback, waiting, retry, and recovery decisions observable from the subsystem that owns each decision.
11. [ ] Provide a safe human-readable diagnostic projection for management UI.
12. [ ] Add deterministic Example verification for trace hierarchy, correlation propagation, redaction, sampling, failures, cancellation, and outcome paths.

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
- `IExecutionObservationSource` is the provider-neutral boundary through which an execution runtime can publish bounded facts that tracing may consume.
- `TracingProviderAdapter` records provider operation boundaries only. It must not reconstruct retry/fallback state with `AsyncLocal`, call counting, or exception-message inspection.
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
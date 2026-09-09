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

**0.956 Slice 4 sampling and bounded retention controls — CURRENT.**

The next slice adds provider-neutral, deterministic sampling policy and bounded in-memory retention. Sampling and retention remain observability concerns only and must not alter execution correctness, authorization, event delivery, or execution audit semantics.

## Implemented and verified through Slice 3

- Slice 2 trace identity/span lifecycle contracts and bounded in-memory recorder are verified.
- `TracingAgentRuntime` creates and completes execution-root spans from canonical runtime lifecycle events and restores outer ambient trace context after nested execution.
- `TracingPolicyEngine` records policy evaluation outcome/rule/reason as bounded metadata without changing policy authority.
- `TracingProviderAdapter` records provider invocation and preserves execution correlation without sending trace state in provider payloads.
- `TracingAgentTool` records tool execution while explicitly omitting raw arguments/results and preserving ambient correlation.
- `TracingContextAssembler` records context assembly metadata without copying context payloads and preserves ambient correlation.
- `TracingEventDispatcher` traces event publication and handler execution and propagates trace context through the cloned `EventEnvelope` without copying payload/context.
- `TracePropagation` exposes explicit provider-neutral propagation scopes.
- `EventEnvelope.TraceContext` carries optional trace propagation metadata while retaining event correlation and causation separately.
- Focused `HAgent.Tests` and the matching `HAgent.Example` scenario cover success, hierarchy, correlation, tool/context/event propagation, payload exclusion, failure, cancellation, and nested propagation restoration.

## Slice 3 verification evidence

- User `HAgent.Tests`: **63/63 passed** on 2026-09-09.
- User `HAgent.Example` `.NET Framework 4.8.1`: **Observability Runtime Instrumentation succeeded** at 2026-09-09 03:49:21.
- User `HAgent.Example` `.NET 9`: **Observability Runtime Instrumentation succeeded** at 2026-09-09 03:50:11.
- Both Examples verified execution/policy/provider hierarchy, distinct execution/host correlation, tool/context/event propagation, event publication→handler parentage, sensitive payload omission, failure and cancellation terminal statuses, deterministic fake provider transport, and no real provider request.

## Current Slice 4 boundary

- Define deterministic provider-neutral sampling decisions for trace/span capture.
- Define bounded in-memory retention/eviction behavior using the limits already established by the observability architecture.
- Preserve trace/span lifecycle semantics and default-deny metadata regardless of sampling/retention configuration.
- Add focused tests for deterministic sampling, retention bounds/eviction, per-trace limits, and isolation from execution correctness.
- Add matching public-API Example verification under `Diagnostics → Observability → Sampling & Retention`.
- Do not begin Slice 5 in the same run.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

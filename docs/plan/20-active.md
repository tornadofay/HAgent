# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.956 Observability and Distributed Tracing — CURRENT

Phase 0.955 Context Engineering is complete and verified on .NET Framework 4.8.1 and .NET 9. The next ordered foundational milestone is 0.956 Observability and Distributed Tracing.

### 0.955 Context Engineering — VERIFIED

The complete 0.955 implementation and verification sequence is complete. Verified work includes provider-neutral context contracts, bounded acquisition, deterministic ranking/deduplication, compaction, reusable caching, execution/provider integration, bounded multi-resource retrieval, policy/capability admission, host authorization for protected data-backed sources, and end-to-end assembly.

### 0.956 Run-sized execution plan

1. **Observability contract inventory and architecture review — VERIFIED**
   - Reconciled existing execution/correlation identity, identity context, execution audit, lifecycle state, event correlation/causation, policy decisions, tool-call context, context/admission diagnostics, and provider boundaries against the 0.956 requirements.
   - Added the authoritative provider-neutral observability architecture in `docs/architecture/22-observability.md`.
   - Established that trace/span identity is a new observability relationship and does not replace `ExecutionId`, `AgentExecution.CorrelationId`, `HostCorrelationId`, runtime identity, event IDs, or causation IDs.
   - Established bounded default-deny trace metadata, explicit redaction before sinks, vendor-neutral sampling/retention semantics, and the separation between tracing, event dispatch, authorization, execution audit, and transcript/payload storage.
   - Identified the exact first implementation boundary: **0.956 Slice 2 — Trace identity and span lifecycle contracts**, with focused `HAgent.Tests` and matching public-API `HAgent.Example` verification.
   - No tracing implementation, exporter, persistence, UI, or broad runtime instrumentation was started in this slice.

2. **Trace identity and span lifecycle contracts — VERIFIED**
   - Implemented the provider-neutral Core trace context/span contracts and in-memory recorder boundary defined by `docs/architecture/22-observability.md`.
   - Added focused `HAgent.Tests` coverage for hierarchy, correlation propagation, bounded redacted/omitted metadata, terminal completion protection, and deterministic ordering.
   - Added and classified the matching public-API `HAgent.Example` scenario under `Diagnostics → Observability → Observability Tracing`.
   - **User verification — 2026-09-09:** solution build succeeded after pull; `HAgent.Tests` completed with **59/59 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 01:37:** `Observability Tracing` succeeded, verifying hierarchy, execution/host/runtime/event correlation, redaction/omitted metadata, terminal statuses/late completion rejection, deterministic recorder order, and no provider request.
   - **User Example verification — .NET 9, 2026-09-09 01:38:** same public-API scenario succeeded with the same contract checks.
   - No Slice 3 implementation was started in this run.

3. **Trace-producing runtime instrumentation and propagation — VERIFIED**
   - Added provider-neutral tracing producers/decorators for the canonical execution lifecycle, policy evaluation, provider invocation, tool execution, context assembly, and event publication/handling boundaries.
   - Added `TracePropagation` as the explicit host/internal propagation scope and `EventEnvelope.TraceContext` as the optional provider-neutral event propagation field; neither replaces existing correlation/causation identity.
   - Kept runtime lifecycle as the sole execution producer so policy/context tracing is not duplicated when their dedicated tracing boundaries are composed.
   - Preserved default-deny payload handling: tool arguments, event payload/context, prompts, provider responses, and raw host payloads are not copied into trace metadata.
   - Hardened nested execution restoration so an outer trace context/correlation is restored after a traced execution completes or fails.
   - Added focused `HAgent.Tests` coverage for execution/policy/provider hierarchy, tool/context/event parent propagation, correlation preservation, payload exclusion, failure and cancellation terminal statuses, and nested ambient-context restoration.
   - Added and classified the matching public-API `HAgent.Example` scenario under `Diagnostics → Observability → Observability Runtime Instrumentation`; it exercises success, provider failure, cancellation, policy/provider hierarchy, tool/context/event propagation, event handler parentage, correlation preservation, and payload omission using only deterministic in-process fakes.
   - **User verification — 2026-09-09:** `HAgent.Tests` completed with **63/63 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 03:49:21:** `Observability Runtime Instrumentation` succeeded, verifying execution/policy/provider hierarchy, distinct execution/host correlation, tool/context/event propagation, event publication→handler parentage, sensitive payload omission, failure terminal status, cancellation terminal status, deterministic fake provider transport, and no real provider request.
   - **User Example verification — .NET 9, 2026-09-09 03:50:11:** same public-API scenario succeeded with the same checks.
   - A final Slice 3 test correction was required for ambient correlation propagation through tool/context/event boundaries; the resulting full suite passed 63/63.

4. **Sampling and bounded retention controls — VERIFIED**
   - Added provider-neutral `TraceSamplingOptions`, `ITraceSampler`, and `TraceRetentionOptions` contracts.
   - Added deterministic root sampling through `DeterministicTraceSampler`; sampled state is inherited by child spans through `TraceContext` and unsampled spans remain lifecycle-capable without being retained.
   - Extended `InMemoryTraceRecorder` with optional sampling and bounded retention while preserving the existing no-argument recorder behavior.
   - Added retention bounds for maximum trace count, maximum span count, maximum spans per trace, aggregate metadata characters, and maximum age; eviction operates on whole older traces where possible and avoids evicting the active trace merely to admit a child span.
   - Sampling/retention do not alter execution, authorization, event delivery, or span completion semantics, and existing default-deny metadata behavior remains unchanged.
   - Added focused `HAgent.Tests/ObservabilitySamplingRetentionTests.cs` covering deterministic sampling, unsampled inheritance/suppression, retention bounds, per-trace limits, aggregate metadata limits, and lifecycle independence.
   - Added and classified the matching public-API `HAgent.Example/MainForm.ObservabilitySamplingRetention.cs` scenario under `Diagnostics → Observability → Observability Sampling & Retention`.
   - **User verification — .NET 9, 2026-09-09:** full `HAgent.Tests` completed with **67/67 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 04:16:56:** `Observability Sampling & Retention` succeeded, verifying deterministic sampling stability, unsampled child inheritance/suppression, lifecycle independence from sampling/retention, maximum retained traces/spans, per-trace bounds, aggregate metadata bound, no provider transport, and no real provider request.
   - **User Example verification — .NET 9, 2026-09-09 04:17:25:** same public-API scenario succeeded with the same checks.
   - The supplied verification establishes the Slice 4 implementation/test/Example boundary as passing. A separate solution-build result was not restated in the latest verification message, so this plan records only the results explicitly supplied.

5. **Integrated trace sinks and safe export boundary — CURRENT**
   - Define the next provider-neutral sink/export boundary for completed trace spans without coupling Core to OpenTelemetry, a vendor SDK, network transport, or persistence technology.
   - Preserve the existing default-deny/redaction guarantees at the sink boundary; sinks receive only trace data already admitted by the trace contract and bounded retention/sampling rules.
   - Support deterministic in-process sink testing, ordering, completion semantics, and safe behavior when a sink is slow or rejects a span.
   - Keep execution correctness independent from sink availability or sink failure; telemetry failure must not turn a successful execution into a failed execution.
   - Do not introduce remote telemetry transport, durable trace storage, management UI, or broad diagnostic projection in this slice.
   - Add focused `HAgent.Tests` coverage and a matching public-API `HAgent.Example` scenario under `Diagnostics → Observability → Observability Sinks`.
   - Verification boundary: solution build, full `HAgent.Tests`, and the exact Example scenario on .NET Framework 4.8.1 and .NET 9 using deterministic in-process sinks/fakes only.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

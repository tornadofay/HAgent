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

3. **Trace-producing runtime instrumentation and propagation — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING**
   - Added provider-neutral tracing producers/decorators for the canonical execution lifecycle, policy evaluation, provider invocation, tool execution, context assembly, and event publication/handling boundaries.
   - Added `TracePropagation` as the explicit host/internal propagation scope and `EventEnvelope.TraceContext` as the optional provider-neutral event propagation field; neither replaces existing correlation/causation identity.
   - Kept runtime lifecycle as the sole execution producer so policy/context tracing is not duplicated when their dedicated tracing boundaries are composed.
   - Preserved default-deny payload handling: tool arguments, event payload/context, prompts, provider responses, and raw host payloads are not copied into trace metadata.
   - Hardened nested execution restoration so an outer trace context/correlation is restored after a traced execution completes or fails.
   - Added focused `HAgent.Tests` coverage for execution/policy/provider hierarchy, tool/context/event parent propagation, correlation preservation, payload exclusion, failure and cancellation terminal statuses, and nested ambient-context restoration.
   - Added and classified the matching public-API `HAgent.Example` scenario under `Diagnostics → Observability → Observability Runtime Instrumentation`; it exercises success, provider failure, cancellation, policy/provider hierarchy, tool/context/event propagation, event handler parentage, correlation preservation, and payload omission using only deterministic in-process fakes.
   - **Local verification required:** after pull, build the solution, run the `HAgent.Tests` suite, then run the exact Example scenario on .NET Framework 4.8.1 and .NET 9. Do not mark Slice 3 verified until those results are supplied.
   - Do not begin Slice 4 in the same run.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

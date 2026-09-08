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

3. **Trace-producing runtime instrumentation and propagation — CURRENT**
   - Add the first runtime producers that create/propagate trace context through canonical execution, provider, tool, event, policy, and context boundaries without duplicating those subsystem contracts.
   - Preserve execution, host, runtime, event, and identity correlation independently from TraceId/SpanId relationships.
   - Add focused deterministic `HAgent.Tests` coverage and matching public-API `HAgent.Example` verification for propagation across the first integrated boundaries, including terminal/failure/cancellation paths exposed by those producers.
   - Do not begin Slice 4 in the same run.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

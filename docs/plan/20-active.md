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
   - The supplied latest verification did not separately restate a solution-build result; therefore only the explicitly supplied verification results are recorded here.

5. **Integrated trace sinks and safe export boundary — VERIFIED**
   - Added provider-neutral `ITraceSink` and bounded `TraceSinkOptions` contracts.
   - Added `TraceSinkDispatcher` with a bounded non-blocking enqueue boundary, FIFO processing, asynchronous sink delivery, flush support for deterministic tests, and isolated sink-failure accounting.
   - Integrated sink dispatch into `InMemoryTraceRecorder` only after a sampled span completes and only while that span remains retained by the recorder; sampled-out and retention-rejected spans never cross the sink boundary.
   - Preserved the existing trace metadata/redaction contract; sinks receive the canonical bounded `TraceSpan` rather than prompts, provider payloads, tool payloads, host raw context, or arbitrary serialized objects.
   - Sink latency, queue saturation, and sink exceptions remain telemetry concerns and do not alter span lifecycle completion or execution correctness. One failing sink does not prevent other registered sinks from receiving the same span.
   - Added focused `tests/HAgent.Tests/ObservabilitySinksTests.cs` covering FIFO delivery, sink-failure isolation, slow asynchronous sink behavior, bounded queue saturation, sampled-out suppression, and retention-boundary suppression.
   - Added and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilitySinks.cs` scenario under `Diagnostics → Observability → Observability Sinks`.
   - **User verification — 2026-09-09:** full `HAgent.Tests` completed with **72/72 tests passed**.
   - **User Example verification — .NET 9, 2026-09-09 04:59:10:** `Observability Sinks` succeeded, verifying retained sampled span delivery order, sink-failure isolation, slow-sink non-blocking span completion, asynchronous flush behavior, bounded queue saturation/drop behavior, sampled-out suppression, retention-rejected suppression, no remote telemetry transport, and no real provider request.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 04:59:52:** same public-API scenario succeeded with the same checks.
   - The prior Example-only duplicate-helper compiler error was fixed before these successful runs.

6. **Safe human-readable diagnostic projection — VERIFIED**
   - Added provider-neutral `TraceDiagnosticProjectionOptions`, `TraceDiagnosticMetadataItem`, `TraceDiagnosticSpan`, and `TraceDiagnosticProjection` contracts.
   - Added `TraceDiagnosticProjector` as the bounded management/UI projection boundary. It consumes retained sampled spans only, orders them deterministically by trace sequence/identity, and bounds result counts, identifiers, operation/kind text, metadata entries, metadata keys, and metadata values.
   - The projector allowlists diagnostic metadata namespaces (`admission`, `agent`, `context`, `event`, `evaluation`, `execution`, `failure`, `knowledge`, `learning`, `lifecycle`, `memory`, `outcome`, `planning`, `policy`, `provider`, `resource`, `runtime`, `sampling`, and `tool`) plus the explicit `decision` key. Unknown/custom metadata is omitted and counted rather than rendered.
   - Existing `[Redacted]`/`[Omitted]` markers are preserved only for metadata that is otherwise safe to expose; the projection never exposes raw prompts, responses, tool payloads, host context, credentials, connection strings, or arbitrary serialized objects.
   - The projection exposes bounded trace/span identity, operation/kind, start/duration/status, parent relationship, execution correlation, host correlation, and runtime identity suitable for diagnostic UI consumption.
   - Added focused `tests/HAgent.Tests/ObservabilityDiagnosticProjectionTests.cs` covering deterministic ordering, span/text bounds, safe metadata allowlisting, omission accounting, explicit redaction markers, parent/correlation visibility, status/duration, and unsampled suppression.
   - Added and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilityDiagnosticProjection.cs` under `Diagnostics → Observability → Observability Diagnostic Projection`.
   - **User verification — 2026-09-09 05:08:** .NET Framework 4.8.1 Example succeeded, verifying deterministic ordering, bounds, correlation/parent relationship, status/duration, metadata allowlisting, redaction, omission accounting, unsampled suppression, and no provider/remote telemetry transport or real provider request.
   - **User verification — 2026-09-09 05:09:** .NET 9 Example succeeded with the same checks.
   - **User verification — 2026-09-09:** full `HAgent.Tests` completed with **77/77 tests passed** after the Slice 6 Example helper correction.

7. **Cross-process trace context and correlation boundary — VERIFIED**
   - Added provider-neutral `TracePropagationCarrier`, `TracePropagationImportOptions`, `TracePropagationImportStatus`, and `TracePropagationImportResult` contracts with bounded key/value counts and lengths.
   - Extended `TracePropagation` to export `TraceContext` plus the existing bounded correlation identities into the carrier and import them deterministically without coupling Core to HTTP, message buses, OpenTelemetry, or vendor-specific telemetry.
   - Preserved the distinction between `TraceId`, `ParentSpanId`, sampled state, and `ExecutionId`, `ExecutionCorrelationId`, `HostCorrelationId`, `EventId`, `CausationId`, plus the other bounded identity dimensions.
   - Incoming trace context is explicitly rejected as untrusted by default; missing context returns `Missing`, malformed trace data returns `InvalidTraceContext`, malformed correlation data returns `InvalidCorrelation`, and accepted unsampled state remains unsampled.
   - Correlation values remain diagnostic identity only and do not become authentication or authorization authority. Host trust remains outside Core.
   - Added focused `tests/HAgent.Tests/ObservabilityTracePropagationTests.cs` covering round-trip identity preservation, unsampled propagation, missing context, explicit trust rejection, malformed trace input, invalid sampled state, oversized correlation input, and carrier clone/bounds behavior.
   - Added and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilityTracePropagation.cs` under `Diagnostics → Observability → Observability Trace Propagation`.
   - **User verification — 2026-09-09 05:15:** full `HAgent.Tests` completed with **85/85 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 05:15:46:** cross-process trace/correlation propagation succeeded, including round-trip identity preservation, explicit trust acceptance/rejection, missing/malformed handling, unsampled preservation, carrier bounds, and no HTTP/message/OpenTelemetry/remote telemetry/provider transport.
   - **User Example verification — .NET 9, 2026-09-09 05:16:23:** same public-API scenario succeeded.

8. **Failure, retry, fallback, waiting, and stale-result observability — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING**
   - Added provider-neutral `TraceObservation` as the bounded runtime decision-observation helper. When no traced recorder is active, observations are suppressed.
   - Added bounded `AgentExecutionObservation`, `ExecutionObservationKinds`, and `IExecutionObservationSource` as the provider-neutral boundary for authoritative execution outcome facts.
   - Extended `DefaultAgentRuntime` to publish retry, retry-wait, recovery, and stale-result facts at the point where it already owns those decisions.
   - Simplified `TracingProviderAdapter` so it records provider invocation spans only. It no longer infers attempt/retry state using `AsyncLocal`, call counting, or exception-message matching.
   - Extended `TracingAgentRuntime` to consume `IExecutionObservationSource` and translate authoritative runtime facts into trace observations without becoming another execution-state authority.
   - Fallback is not inferred from multiple provider spans. A fallback observation is valid only when the authoritative execution runtime explicitly reports a fallback decision.
   - Kept execution terminal state authoritative; tracing never commits, retries, cancels, approves, rejects, or overwrites execution outcomes.
   - Added focused `tests/HAgent.Tests/ObservabilityOutcomeTracingTests.cs` covering disabled observation, child/correlation propagation, authoritative retry/wait/recovery facts, exact attempt/retry ordinals, and trace translation.
   - Updated and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilityOutcomeTracing.cs` under `Diagnostics → Observability → Observability Outcome Tracing`.
   - Updated `docs/architecture/22-observability.md` and `docs/roadmap/956-observability-tracing.md` with the explicit separation between ambient trace context and authoritative execution outcome state.
   - **Local verification required:** after pull, build the solution, run the full `HAgent.Tests` suite, then run the exact Example scenario on .NET Framework 4.8.1 and .NET 9. Confirm no remote telemetry or real provider request is used. Verify that retry/wait/recovery facts originate from the execution runtime and are translated into trace observations.
   - Do not mark Slice 8 verified until all required local results are supplied.
   - Do not begin Slice 9 in the same run.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

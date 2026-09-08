# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.956 Observability and Distributed Tracing — CURRENT

Phase 0.955 Context Engineering is complete and verified on .NET Framework 4.8.1 and .NET 9. The next ordered foundational milestone is 0.956 Observability and Distributed Tracing.

### 0.955 Context Engineering — VERIFIED

The complete 0.955 implementation and verification sequence is complete. Verified work includes provider-neutral context contracts, bounded acquisition, deterministic ranking/deduplication, compaction, reusable caching, execution/provider integration, bounded multi-resource retrieval, policy/capability admission, host authorization for protected data-backed sources, and end-to-end assembly.

### 0.956 Run-sized execution plan

1. **Observability contract inventory and architecture review — CURRENT CHECKPOINT**
   - Reconcile the existing diagnostics, execution audit, correlation IDs, lifecycle events, policy decisions, provider execution boundaries, tool activity, context assembly, and runtime state against the authoritative 0.956 observability requirements.
   - Define the smallest provider-neutral trace/span contract needed to correlate HAgent operations without creating a second event system or coupling Core to a telemetry vendor.
   - Establish the canonical relationship between existing execution/correlation identifiers and future trace/span/parent relationships.
   - Define the safe metadata boundary for traces: secrets, credentials, sensitive payloads, and raw prompts/responses remain excluded by default; redaction must be explicit and bounded.
   - Identify which existing observability mechanisms are retained as producers/adapters and which missing contracts must be introduced in later slices.
   - This slice is architecture/contract reconciliation only; do not implement the full tracing system in the same run.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

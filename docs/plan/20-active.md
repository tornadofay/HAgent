# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.955 Context Engineering — CURRENT

Phase 0.954 Prompt and Instruction Governance is complete and verified. The next ordered foundational milestone is 0.955 Context Engineering.

### 0.954 Prompt and Instruction Governance — VERIFIED

The complete 0.954 implementation and verification sequence is complete.

### 0.955 Run-sized execution plan

1. **Context contract inventory and architecture review — VERIFIED CHECKPOINT**
   - The authoritative context architecture was reconciled with the current execution/context contracts, legacy conversation context implementation, and WinForms UI/data context adapters.
   - Existing mechanisms remain producer/input boundaries rather than competing canonical context architectures.

2. **Provider-neutral context contract foundation — CURRENT**
   - Scope: Core-only provider-neutral context item metadata/payload representation, explicit budget dimensions, bounded provenance/scope/trust/importance/freshness/size metadata, and the minimal candidate-source boundary. No ranking, compaction, cache, WinForms changes, or provider transport in this slice.
   - Current implementation: context contracts and focused contract tests are implemented; deterministic public-API Example coverage has been added in `src/HAgent.Example/MainForm.ContextContracts.cs`.
   - Required verification before completion: local targeted build/test plus deterministic Example execution. Per repository rule, the new public capability is not considered verified without matching Example coverage.

3. **Bounded context acquisition and canonical context snapshots — PLANNED**
   - Scope: implement bounded host/source acquisition and produce the immutable execution context snapshot, including cancellation and isolation from caller-owned mutable state.

4. **Ranking, deterministic prioritization, and deduplication — PLANNED**
5. **Compaction, truncation, and provenance-preserving diagnostics — PLANNED**
6. **Cache-safe reusable context components — PLANNED**
7. **Execution/provider integration and deterministic Example verification — PLANNED**

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

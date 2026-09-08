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

2. **Provider-neutral context contract foundation — VERIFIED**
   - Core contracts define provider-neutral context items, structured payloads, bounded provenance/scope/quality/size metadata, explicit item/character/token budget dimensions, bounded source requests, and clone isolation.
   - Matching deterministic Example coverage was executed successfully on both .NET Framework 4.8.1 and .NET 9 on 2026-09-08.
   - Verified Example results included contract validation, bounded metadata, structured payload preservation, explicit budget/source bounds, nested clone isolation, and no provider request.

3. **Bounded context acquisition and canonical context snapshots — VERIFIED**
   - Core provides bounded source acquisition and an execution-owned `ContextSnapshot` with explicit used/remaining budget dimensions.
   - Acquisition preserves deterministic source order, propagates cancellation, clones request/item state, and prevents snapshot mutation through defensive copies.
   - `HAgent.Tests` completed with 20/20 tests passing on 2026-09-08.
   - Deterministic public-API `HAgent.Example` Context Acquisition verification passed on both .NET Framework 4.8.1 and .NET 9 on 2026-09-08.

4. **Ranking, deterministic prioritization, and deduplication — VERIFIED**
   - Core provides provider-neutral candidate ranking and deterministic prioritization using available relevance/importance/trust/freshness/estimated-cost evidence.
   - Duplicate IDs are eliminated with stable tie-breaking while preserving the highest-ranked candidate's metadata.
   - `HAgent.Tests` completed with 24/24 tests passing on 2026-09-08.
   - Deterministic public-API `HAgent.Example` Context Ranking verification passed on 2026-09-08, including ranking order, deterministic ties, duplicate retention, provenance/scope preservation, and no provider request.

5. **Compaction, truncation, and provenance-preserving diagnostics — CURRENT**
   - Scope: bounded compaction over already ranked provider-neutral candidates, deterministic truncation to an explicit target budget, and safe inclusion/exclusion diagnostics that retain source/provenance metadata.
   - The first strategy is deliberately deterministic and tokenizer-free: it may exclude lower-ranked candidates that do not fit, but it must not rewrite or summarize payloads or invent provider-specific token counts.
   - Verification target: focused Core tests plus matching deterministic public-API Example coverage for bounded selection, item/character/token exclusions, unknown-token handling, provenance preservation, and diagnostic reasons.

6. **Cache-safe reusable context components — PLANNED**
7. **Execution/provider integration and deterministic Example verification — PLANNED**

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

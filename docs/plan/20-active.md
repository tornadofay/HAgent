# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.955 Context Engineering — CURRENT

Phase 0.954 Prompt and Instruction Governance is complete and verified. The next ordered foundational milestone is 0.955 Context Engineering.

### 0.954 Prompt and Instruction Governance — VERIFIED

The complete 0.954 implementation and verification sequence is complete.

- Example UI prerequisite verified on 2026-09-08 through `LEARNING INTERVENTION`, `CONTEXT BUDGET`, and `RUNTIME INSTANCES`.
- Slices 1–4 verified on 2026-09-08 through `COGNITION INSTRUCTIONS`, covering instruction source/authority contracts, deterministic composition/conflict handling, resource/external boundaries, and execution integration.
- Deterministic runtime Example coverage verified on both supported targets: `RUNTIME SHUTDOWN`, `RUNTIME OVERRIDES`, and `RUNTIME STALE RESULTS` on .NET Framework 4.8.1 and .NET 9; `RUNTIME INSTANCES` on .NET Framework 4.8.1; `RUNTIME CONCURRENCY` and `RUNTIME TERMINAL STATE` on .NET 9.
- `COGNITION INSTRUCTIONS` was verified on both .NET Framework 4.8.1 and .NET 9.
- The corrected deterministic runtime Examples no longer depend on manually selected configured-agent UI state.

### 0.955 Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach its stated completion condition before the next slice begins.

1. **Context contract inventory and architecture review — VERIFIED CHECKPOINT**
   - Scope completed: inspected the authoritative context architecture, current execution/context contracts, legacy conversation context implementation, and WinForms UI/data context adapters; reconciled them with the 0.955 requirements.
   - Findings: `AgentExecutionRequest.HostContext` is a bounded string-map convenience input; `ConversationContextBuilder` is a conversation-history limiter; WinForms `IUiContext` / `WinFormsUiContext` is a host-specific discovery/data adapter; none is the canonical multi-source context model.
   - Architectural resolution: `docs/architecture/20-context.md` now defines the canonical provider-neutral context item, budget, source/retrieval/assembly separation, policy boundary, ranking, compaction, provenance, caching, diagnostics, and immutable execution-snapshot target. Existing mechanisms remain producer/input boundaries rather than competing architectures.
   - Completion: achieved in repository state by architecture reconciliation plus recording the bounded implementation slice below. No local .NET build/test is claimed for this planning-only checkpoint.

2. **Provider-neutral context contract foundation — CURRENT**
   - Scope: add the smallest Core-only contract layer required by 0.955: provider-neutral context item metadata/payload representation, explicit context budget dimensions, bounded provenance/scope/trust/importance/freshness/size metadata, and the source boundary needed to supply candidate context. Do not implement ranking, compaction, cache, WinForms changes, or provider transport in this slice.
   - Expected files: focused `src/HAgent.Core/Models/` context contract file(s) and the minimal `src/HAgent.Core/Abstractions/` source contract(s); update project references only if required by the existing project structure.
   - Design constraints: no WinForms types, no SQL/provider-specific types, no raw SQL, no vendor tokenizer dependency, explicit validation/bounds, provider-neutral structured payload support, provenance retained as data, and no implication that context metadata grants authority or authorization.
   - Entry: 0.955 Slice 1 verified checkpoint is present; the architecture document above is authoritative for the target contract.
   - Completion: contracts compile on targeted frameworks, focused contract-level tests verify validation/bounds/clone-or-snapshot safety appropriate to the implemented types, and this active plan records the exact result. No Example expansion is required yet unless the public contract is usable enough to warrant a focused deterministic Example in the same bounded slice.

3. **Bounded context acquisition and canonical context snapshots — PLANNED**
   - Scope: implement bounded host/source acquisition and produce the immutable execution context snapshot, including cancellation and isolation from caller-owned mutable state.

4. **Ranking, deterministic prioritization, and deduplication — PLANNED**
   - Scope: implement provider-neutral relevance/ranking inputs and deterministic tie-breaking over candidate context.

5. **Compaction, truncation, and provenance-preserving diagnostics — PLANNED**
   - Scope: add explicit budget enforcement strategies while preserving required policy/instruction authority and provenance.

6. **Cache-safe reusable context components — PLANNED**
   - Scope: add bounded, ownership/version/freshness-aware cache contracts and reuse behavior.

7. **Execution/provider integration and deterministic Example verification — PLANNED**
   - Scope: integrate the canonical context snapshot into execution/provider mapping and add focused public-API Example verification for the completed 0.955 capability across supported targets, including success, boundary, cancellation/isolation, budget, ranking, compaction, provenance, cache, and policy-exclusion cases as applicable.

### Verification rule

A slice becomes complete only after its implementation exists, the matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

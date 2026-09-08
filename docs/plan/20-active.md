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

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **Context contract inventory and architecture review — CURRENT**
   - Scope: inspect the authoritative context architecture and current implementation, establish the complete intended 0.955 context model, identify reusable existing contracts, and define the first bounded implementation slice.
   - Entry: 0.954 Prompt and Instruction Governance verified.
   - Completion: the context architecture/current implementation is reconciled and the smallest implementation slice is recorded in this active plan.

2. **Bounded context acquisition and canonical context snapshots — PLANNED**
   - Scope: implement the next verified context capability after Slice 1 review, preserving bounded host context, generic observations, snapshot isolation, cancellation, and provider neutrality.

3. **Deterministic Example verification — PLANNED**
   - Scope: add and verify public-API Example coverage for the implemented 0.955 context capability on supported targets, including success, boundary, cancellation, and isolation cases appropriate to the design.

### Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example or focused test verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

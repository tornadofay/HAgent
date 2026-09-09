# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — CURRENT

Phase 0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime remains authoritative for outcome facts while tracing consumes those facts diagnostically.

### 0.957 Run-sized execution plan

1. **Provider-neutral evaluation contracts and evaluator boundary — VERIFIED**
   - Established the provider-neutral evaluation model required by the 0.957 roadmap before implementing scoring engines or persistence.
   - Defined `AiEvaluationTargetKind` for execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning-candidate quality targets.
   - Defined `AiEvaluationRequest`, `AiEvaluationInputReference`, and `AiEvaluation` with bounded validation, provenance, correlation, score/label/outcome semantics, and owned clone behavior.
   - Defined asynchronous `IAiEvaluator` with explicit evaluator identity, kind, and version. The abstraction is independent of a specific LLM vendor or grading service.
   - Kept evaluation evidence separate from authorization, execution terminal state, persistent configuration, memory, knowledge, skills, and learning promotion.
   - Avoided raw prompts, responses, tool arguments, credentials, or arbitrary host objects in the contracts; bounded references/metadata are used instead.
   - Added focused `tests/HAgent.Tests/EvaluationContractsTests.cs` covering target identity, clone isolation, score/confidence bounds, provenance, correlation identities, cancellation-aware evaluator behavior, and bounded collections.
   - Added and classified `src/HAgent.Example/MainForm.EvaluationContracts.cs` as `Diagnostics → Other Diagnostics → Evaluation Contracts` using a deterministic in-process evaluator and no provider/model transport.
   - Added `docs/architecture/23-evaluation-quality.md` as the authoritative evaluation ownership boundary.
   - **User verification — 2026-09-09:** full `HAgent.Tests` completed with **96/96 tests passed** on .NET 9.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 06:26:48:** `Evaluation Contracts` succeeded, verifying provider-neutral target kinds, outcome/score/label representation, evaluator provenance, execution/runtime/goal/plan/trace correlation, bounded input/evidence references, owned clone isolation, no agent-state mutation or authorization side effect, no remote grading/provider transport, and no real provider request.
   - **User Example verification — .NET 9, 2026-09-09 06:26:18:** same public-API scenario succeeded with the same checks.
   - Slice 1 is complete and no longer awaits local verification.

### Next checkpoint

2. **Deterministic evaluators and evaluation evidence — NOT STARTED**
   - This is the next numbered slice after the verified Slice 1 boundary.
   - Do not implement this slice until a new run begins.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

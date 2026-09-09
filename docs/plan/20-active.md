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
   - User verification — 2026-09-09: full `HAgent.Tests` completed with 96/96 tests passed on .NET 9.
   - User Example verification — .NET Framework 4.8.1 and .NET 9 both succeeded with the same public-API checks.
   - Slice 1 is complete and no longer awaits local verification.

2. **Deterministic evaluators and evaluation evidence — VERIFIED**
   - Established bounded host-computed `AiEvaluationObservation` values so deterministic evaluators inspect explicit facts instead of raw payloads.
   - Covered deterministic schema validity, required-field completeness, policy compliance, tool success, latency, cost, and task-completion signals through `AiDeterministicEvaluationRuleKind`.
   - Implemented `AiDeterministicEvaluationEvaluator` behind `IAiEvaluator` with deterministic pass/fail scoring, evaluator provenance, correlation preservation, bounded evidence references, threshold handling, cancellation, and explicit `Inconclusive` outcomes for missing/mismatched evidence.
   - Rejected ambiguous duplicate signal observations rather than selecting an arbitrary value.
   - Added focused `tests/HAgent.Tests/DeterministicEvaluationTests.cs` and matching public `src/HAgent.Example/MainForm.DeterministicEvaluation.cs` verification.
   - Classified the Example as `Diagnostics → Evaluation → Deterministic Evaluation`.
   - User verification — 2026-09-09: `HAgent.Tests` completed with **109/109 tests passed**.
   - User Example verification — .NET Framework 4.8.1: `Deterministic Evaluation` succeeded.
   - User Example verification — .NET 9: `Deterministic Evaluation` succeeded.
   - Slice 2 is complete and no longer awaits local verification.

3. **Human/application ratings and labeled evaluation evidence — VERIFIED**
   - Establish bounded externally supplied rating data for Human and Application evaluators without introducing a second evaluation result model.
   - Use one provider-neutral evaluator implementation for supplied ratings while requiring the evaluator kind to be `Human` or `Application`.
   - Preserve outcome, score, confidence, label, reason, bounded evidence references, metadata, evaluator identity/version, and execution/runtime/agent/goal/plan/trace correlation.
   - Clone supplied rating data on evaluator construction and produced evaluation data so later caller mutation cannot alter the evaluation result.
   - Observe cancellation before producing externally supplied evaluation evidence.
   - Added focused `tests/HAgent.Tests/SuppliedEvaluationTests.cs` and matching public `src/HAgent.Example/MainForm.SuppliedEvaluation.cs` verification.
   - Explicitly classified the Example as `Diagnostics → Evaluation → Supplied Evaluation Ratings`.
   - User verification — 2026-09-09: full `HAgent.Tests` completed with **115/115 tests passed**.
   - User Example verification — .NET Framework 4.8.1 at **2026-09-09 06:53:44**: `Supplied Evaluation Ratings` succeeded with Human/Application outcome, score, label, provenance, evidence ownership, correlation, non-authoritative behavior, and no provider/model transport verified.
   - User Example verification — .NET 9 at **2026-09-09 06:52:58**: `Supplied Evaluation Ratings` succeeded with the same public-API checks.
   - Slice 3 is complete and no longer awaits local verification.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test/Example success unless actually executed or supplied as user local verification evidence.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

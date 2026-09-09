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

2. **Deterministic evaluators and evaluation evidence — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING**
   - Establish bounded host-computed `AiEvaluationObservation` values so deterministic evaluators inspect explicit facts instead of raw payloads.
   - Cover deterministic schema validity, required-field completeness, policy compliance, tool success, latency, cost, and task-completion signals through `AiDeterministicEvaluationRuleKind`.
   - Implement `AiDeterministicEvaluationEvaluator` behind `IAiEvaluator` with deterministic pass/fail scoring, evaluator provenance, correlation preservation, bounded evidence references, threshold handling, cancellation, and explicit `Inconclusive` outcomes for missing/mismatched evidence.
   - Reject ambiguous duplicate signal observations rather than selecting an arbitrary value.
   - Add focused `tests/HAgent.Tests/DeterministicEvaluationTests.cs` and matching public `src/HAgent.Example/MainForm.DeterministicEvaluation.cs` verification.
   - Explicitly classify the Example as `Diagnostics → Evaluation → Deterministic Evaluation`.
   - This slice does not add model-assisted grading, human-rating workflows, aggregation, regression suites, persistence, or management UI.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test/Example success unless actually executed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

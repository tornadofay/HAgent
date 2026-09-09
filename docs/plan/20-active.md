# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — CURRENT

Phase 0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime remains authoritative for outcome facts while tracing consumes those facts diagnostically.

### 0.957 Run-sized execution plan

1. **Provider-neutral evaluation contracts and evaluator boundary — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING**
   - Establish the provider-neutral evaluation model required by the 0.957 roadmap before implementing scoring engines or persistence.
   - Define `AiEvaluationTargetKind` for execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning-candidate quality targets.
   - Define `AiEvaluationRequest`, `AiEvaluationInputReference`, and `AiEvaluation` with bounded validation, provenance, correlation, score/label/outcome semantics, and owned clone behavior.
   - Define asynchronous `IAiEvaluator` with explicit evaluator identity, kind, and version. The abstraction is independent of a specific LLM vendor or grading service.
   - Keep evaluation evidence separate from authorization, execution terminal state, persistent configuration, memory, knowledge, skills, and learning promotion.
   - Avoid raw prompts, responses, tool arguments, credentials, or arbitrary host objects in the contracts; use bounded references/metadata instead.
   - Add focused `tests/HAgent.Tests/EvaluationContractsTests.cs` covering target identity, clone isolation, score/confidence bounds, provenance, correlation identities, cancellation-aware evaluator behavior, and bounded collections.
   - Add and classify `src/HAgent.Example/MainForm.EvaluationContracts.cs` as `Diagnostics → Other Diagnostics → Evaluation Contracts` using a deterministic in-process evaluator and no provider/model transport.
   - Add `docs/architecture/23-evaluation-quality.md` as the authoritative evaluation ownership boundary.
   - **Local verification required:** after pull, build the solution, run the full `HAgent.Tests` suite, then run the Example on .NET Framework 4.8.1 and .NET 9. Confirm there is no remote evaluator/provider request, remote telemetry, or state mutation.
   - Do not mark Slice 1 verified until all required local results are supplied.
   - Do not begin Slice 2 in the same run.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

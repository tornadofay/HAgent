# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — CURRENT

Phase 0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime remains authoritative for outcome facts while tracing consumes those facts diagnostically.

### Completed evaluation slices

1. **Provider-neutral evaluation contracts and evaluator boundary — VERIFIED**
   - Provider-neutral target, request, evidence-reference, result, provenance, correlation, bounded validation, and clone contracts are established.
   - `IAiEvaluator` is the asynchronous evaluator boundary independent of a specific model vendor or grading service.
   - User verification — 2026-09-09: full `HAgent.Tests` completed with 96/96 tests passed on .NET 9; required Example checks succeeded on .NET Framework 4.8.1 and .NET 9.

2. **Deterministic evaluators and evaluation evidence — VERIFIED**
   - Deterministic host-computed observations and rules for schema validity, required fields, policy compliance, tool success, task completion, latency, and cost are implemented with bounded evidence, provenance, cancellation, threshold handling, ambiguity rejection, and `Inconclusive` outcomes.
   - User verification — 2026-09-09: full `HAgent.Tests` completed with 109/109 tests passed.
   - User Example verification — .NET Framework 4.8.1: `Diagnostics → Evaluation → Deterministic Evaluation` succeeded.
   - User Example verification — .NET 9: `Diagnostics → Evaluation → Deterministic Evaluation` succeeded.

3. **Human/application ratings and labeled evaluation evidence — VERIFIED**
   - `AiEvaluationRating` and `AiSuppliedRatingEvaluator` provide bounded externally supplied Human/Application evidence through the same evaluator boundary with owned cloning and non-authoritative semantics.
   - User verification — 2026-09-09: full `HAgent.Tests` completed with 115/115 tests passed.
   - User Example verification — .NET Framework 4.8.1 and .NET 9: `Diagnostics → Evaluation → Supplied Evaluation Ratings` succeeded.

4. **Model-assisted evaluators and non-authoritative judge boundary — VERIFIED**
   - `IAiEvaluationJudge`, detached `AiEvaluationJudgeRequest`, and `AiModelAssistedEvaluationEvaluator` provide provider-neutral model-backed grading without putting transport, credentials, model selection, or evidence resolution in Core.
   - Model-assisted output is explicitly non-authoritative and fail-closed for cancellation, null output, invalid rating, and judge failure.
   - User verification — 2026-09-09: `HAgent.Tests` completed with **123/123 tests passed**; Slice 4 verification passed on **.NET Framework 4.8.1 and .NET 9**.

## Current Slice 5 — Evaluation aggregation and alternative-target comparison

**Objective:** add the provider-neutral measurement boundary needed to aggregate bounded evaluation samples by target variant and compare alternative variants using explicit metric direction, without introducing routing, authorization, persistence, regression-suite orchestration, or management UI.

### Expected files / assemblies

- `src/HAgent.Core/Models/AiEvaluationAggregationContracts.cs`
- `tests/HAgent.Tests/EvaluationAggregationTests.cs`
- `src/HAgent.Example/MainForm.EvaluationAggregation.cs`
- `src/HAgent.Example/MainForm.ExampleOrganization.cs`
- `.github/workflows/verify-phase-0-957-slice-5.yml`
- `docs/architecture/23-evaluation-quality.md`
- `docs/roadmap/957-evaluation-quality-measurement.md`
- `docs/plan/00-active-work.md`
- `docs/plan/00-current-state.md`
- `docs/plan/00-decisions.md`

### Implemented boundary

- `AiEvaluationMetric` defines bounded standard/custom metric values and explicit direction for custom metrics.
- `AiEvaluationSample` binds a stable case ID, target variant ID, evaluation result, and bounded metric observations.
- `AiEvaluationAggregationRequest` limits aggregate input to 256 samples and clones the active snapshot before aggregation.
- `AiEvaluationAggregator.Aggregate` computes outcome counts, success rate, average evaluation score, and bounded metric average/minimum/maximum values for each variant.
- `AiEvaluationAggregator.Compare` compares two completed aggregates, computes left-minus-right deltas, and reports a preferred variant only where both sides have a value and the metric's higher/lower direction gives a strict result.
- Standard metric direction is provider-neutral: success/quality/tool-success/plan-completion are higher-is-better; latency/cost/fallback frequency are lower-is-better. Custom metrics must declare direction.
- Comparison preference is measurement evidence only. It must not be used as implicit authorization, execution routing, configuration mutation, learning promotion, or cognitive authority.
- Cancellation is checked at aggregation and comparison boundaries. Aggregation uses detached sample clones so caller mutation after invocation cannot alter produced aggregates.
- The matching Example uses only public HAgent.Core contracts and deterministic in-process data; no provider or remote grading service is contacted.

**Example to run:** `HAgent.Example → Diagnostics → Evaluation → Evaluation Aggregation` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/EvaluationAggregationTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9** before marking Slice 5 verified.

### Verification checkpoint

The implementation and matching Example/test scenario are committed to the dedicated Slice 5 branch. CI/build/test verification and manual Example execution remain pending. Until those checks are confirmed, Slice 5 is an **implementation checkpoint**, not a completed slice.

### Explicit boundary for the next slice

Regression-suite execution/repetition remains unimplemented. Slice 6 should define the provider-neutral regression case/suite execution contract and deterministic orchestration over alternative target variants. It must consume the aggregation/comparison contracts rather than introduce another metric/result model.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, the supported-target build checks pass, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test/Example success unless actually executed or supplied as user local verification evidence.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

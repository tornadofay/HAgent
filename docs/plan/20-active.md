# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — CURRENT

Phase 0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime remains authoritative for outcome facts while tracing consumes those facts diagnostically.

### Completed evaluation slices

1. **Provider-neutral evaluation contracts and evaluator boundary — VERIFIED**
   - Provider-neutral target, request, evidence-reference, result, provenance, correlation, bounded validation, and clone contracts are established.
   - `IAiEvaluator` is the asynchronous evaluator boundary independent of a specific model vendor or grading service.

2. **Deterministic evaluators and evaluation evidence — VERIFIED**
   - Deterministic host-computed observations and rules for schema validity, required fields, policy compliance, tool success, task completion, latency, and cost are implemented with bounded evidence, provenance, cancellation, threshold handling, ambiguity rejection, and `Inconclusive` outcomes.
   - User verification — 2026-09-09: full `HAgent.Tests` completed with 109/109 tests passed; matching Examples succeeded on .NET Framework 4.8.1 and .NET 9.

3. **Human/application ratings and labeled evaluation evidence — VERIFIED**
   - `AiEvaluationRating` and `AiSuppliedRatingEvaluator` provide bounded externally supplied Human/Application evidence through the same evaluator boundary with owned cloning and non-authoritative semantics.
   - User verification — 2026-09-09: full `HAgent.Tests` completed with 115/115 tests passed; matching Examples succeeded on .NET Framework 4.8.1 and .NET 9.

4. **Model-assisted evaluators and non-authoritative judge boundary — VERIFIED**
   - `IAiEvaluationJudge`, detached `AiEvaluationJudgeRequest`, and `AiModelAssistedEvaluationEvaluator` provide provider-neutral model-backed grading without putting transport, credentials, model selection, or evidence resolution in Core.
   - Model-assisted output is explicitly non-authoritative and fail-closed for cancellation, null output, invalid rating, and judge failure.
   - User verification — 2026-09-09: `HAgent.Tests` completed with **123/123 tests passed**; Slice 4 Example succeeded on **.NET Framework 4.8.1 and .NET 9**.

5. **Evaluation aggregation and alternative-target comparison — VERIFIED**
   - `AiEvaluationMetric`, `AiEvaluationSample`, aggregate/comparison contracts, and `AiEvaluationAggregator` provide bounded measurement-only aggregation and explicit higher/lower comparison semantics.
   - User verification — 2026-09-09: **130/130 HAgent.Tests passed**; `Diagnostics → Evaluation → Evaluation Aggregation` succeeded on **.NET Framework 4.8.1 and .NET 9**.
   - Verified results included baseline success rate `0.333333`, candidate success rate `1`, candidate average quality `0.85`, and candidate average latency `110 ms`, with no authoritative routing or authorization decision.

## Current Slice 6 — Evaluation regression suites and repeated target execution

**Objective:** provide a provider-neutral, host-owned regression-suite boundary that repeats bounded evaluation cases across alternative targets with controlled concurrency, deterministic reporting, failure isolation, cancellation/late-result protection, and direct reuse of Slice 5 aggregation/comparison contracts.

### Expected files / assemblies

- `src/HAgent.Core/Models/AiEvaluationRegressionContracts.cs`
- `tests/HAgent.Tests/EvaluationRegressionTests.cs`
- `src/HAgent.Example/MainForm.EvaluationRegression.cs`
- `src/HAgent.Example/MainForm.ExampleOrganization.cs`
- `.github/workflows/verify-phase-0-957-slice-6.yml`
- `docs/architecture/23-evaluation-quality.md`
- `docs/roadmap/957-evaluation-quality-measurement.md`
- `docs/plan/00-active-work.md`
- `docs/plan/00-current-state.md`
- `docs/plan/00-decisions.md`

### Implemented boundary

- `AiEvaluationRegressionCase` defines bounded case identity, name, input references, and host-defined parameters. It contains no executable code.
- `AiEvaluationRegressionTarget` defines bounded comparison-target identity, name, and metadata. Provider/model/agent descriptors remain host metadata rather than Core authority.
- `AiEvaluationRegressionSuite` owns a validated case-target matrix, unique IDs, `MaxConcurrency` from 1 to 32, and a maximum of 1024 case-target executions.
- `IAiEvaluationRegressionExecutor` is the host-owned execution boundary. It receives detached case/target descriptors and returns the existing `AiEvaluationSample` evidence contract.
- `AiEvaluationRegressionRunner.RunAsync` clones/validates the suite, schedules each case against each target under the concurrency bound, validates returned samples, isolates executor failures, propagates cancellation, and discards samples returned after cancellation.
- `AiEvaluationRegressionRun` reports explicit completed/failed/canceled case-target results in deterministic order and exposes only completed samples to `CreateAggregationRequest()`.
- Failed/canceled executions do not fabricate `AiEvaluation` outcomes and do not enter aggregation input.
- Regression orchestration does not route production work, select credentials, authorize actions, persist authoritative state, promote learning, or mutate cognitive state.

**Example to run:** `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/EvaluationRegressionTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

### Verification checkpoint

Slice 6 implementation and matching Example/test coverage are committed directly to `master`. Supported-target builds and automated tests must pass, and the Example must be manually executed on both supported targets before requirement 9 or Slice 6 is marked verified.

### Verification rule

A slice becomes complete only after its implementation exists, matching focused verification passes, supported-target builds pass, and the matching Example succeeds on every required target. User-supplied local results count as verification evidence. Do not claim success unless actually executed or supplied by the user.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

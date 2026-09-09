# Phase 0.957 — Evaluation and Quality Measurement

## Status

**Verified through Slice 6 on 2026-09-09.**

## Goal

Give HAgent a provider-neutral way to measure whether executions, tool use, plans, learning changes, and agent outcomes achieved their intended quality or task goals.

## Requirements

1. [x] Define evaluation contracts independent of any specific LLM vendor or grading service.
2. [x] Support evaluation targets including execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning candidate quality.
3. [x] Support deterministic evaluators such as schema validity, required-field checks, policy compliance, tool success, latency, cost, and task completion signals.
4. [x] Support externally supplied human/application ratings and labels.
5. [x] Support model-assisted evaluators without treating evaluator-model output as unquestionable truth.
6. [x] Preserve evaluation provenance, evaluator identity/type, input references, timestamp, and confidence where meaningful.
7. [x] Correlate evaluations with execution/runtime/agent/goal/plan/trace identities.
8. [x] Keep evaluation data separate from authoritative agent state; an evaluation does not automatically mutate configuration, memory, skill, or knowledge.
9. [x] Support repeated test cases and regression suites for provider/model/agent comparisons.
10. [x] Support aggregate metrics such as success rate, quality score, latency, cost, fallback frequency, tool success, and plan completion.
11. [x] Add deterministic Example verification for evaluation creation, aggregation, human rating, failed evaluations, and comparison of alternative execution targets.

## Slice 1 — Provider-neutral evaluation contracts and evaluator boundary

**Verified on 2026-09-09.**

- Added provider-neutral target, request, evidence-reference, and result contracts with bounded validation and clone behavior.
- Added `IAiEvaluator` as the asynchronous evaluator boundary with explicit evaluator identity, kind, and version.
- Kept evaluation separate from authorization and authoritative state.
- Added focused `EvaluationContractsTests.cs` and public Example coverage.
- User verification: **96/96 tests passed** on .NET 9; required Example checks succeeded on .NET Framework 4.8.1 and .NET 9.

## Slice 2 — Deterministic evaluators and evaluation evidence

**Verified on 2026-09-09.**

- Added bounded `AiEvaluationObservation` values with explicit Boolean, Decimal, and Text value kinds.
- Added deterministic schema validity, required-field completeness, policy compliance, tool success, cost, latency, and task-completion rules.
- Added `AiDeterministicEvaluationEvaluator` with deterministic scoring, bounded evidence, provenance/correlation, threshold handling, cancellation, ambiguity rejection, and `Inconclusive` outcomes.
- Added focused `DeterministicEvaluationTests.cs` and matching public `Deterministic Evaluation` Example.
- User verification: **109/109 tests passed**; .NET Framework 4.8.1 and .NET 9 Example scenarios succeeded.

## Slice 3 — Human/application ratings and labeled evaluation evidence

**Verified on 2026-09-09.**

- Added bounded `AiEvaluationRating` for externally supplied outcome, score, confidence, label, reason, evidence references, and metadata.
- Added `AiSuppliedRatingEvaluator` through `IAiEvaluator`, accepting only `Human` or `Application` evaluator kinds.
- Preserved supplied rating values, evaluator provenance, correlation, bounded evidence/metadata, and owned clone isolation.
- Added focused `SuppliedEvaluationTests.cs` and matching public `Supplied Evaluation Ratings` Example.
- User verification: **115/115 tests passed** on .NET 9.
- User Example verification: `Supplied Evaluation Ratings` succeeded on both .NET Framework 4.8.1 and .NET 9.

## Slice 4 — Model-assisted evaluators and non-authoritative judge boundary

**Verified on 2026-09-09.**

- Added `IAiEvaluationJudge` as the provider-neutral asynchronous judge boundary.
- Added `AiEvaluationJudgeRequest` as a detached clone of `AiEvaluationRequest`, protecting active judging from caller mutation and preserving provider-neutral bounded inputs, observations, and criteria.
- Added `AiModelAssistedEvaluationEvaluator` implementing `IAiEvaluator` with explicit `ModelAssisted` provenance and bounded evaluator identity/version.
- Reused `AiEvaluationRating` as the bounded judge result instead of introducing a second evaluation-result model; the evaluator maps it into normal `AiEvaluation` evidence while retaining judge provenance.
- Explicitly records `evaluation.source=model-assisted` and `evaluation.authoritative=false`.
- Provider transport, credentials, model selection, retries, and host-specific evidence resolution remain outside Core in the injected judge implementation/owning subsystem.
- Cancellation is checked before judge invocation and after judge completion so a late judge result cannot become an evaluation after cancellation.
- Judge failure, null output, and invalid bounded rating are rejected rather than converted into fabricated evaluation evidence.
- Added focused `ModelAssistedEvaluationTests.cs` and public `Model-Assisted Evaluation` Example.
- User verification: **123/123 HAgent.Tests passed** and the Slice 4 Example succeeded on **.NET Framework 4.8.1 and .NET 9**.

## Slice 5 — Evaluation aggregation and alternative-target comparison

**Verified on 2026-09-09.**

- Added `AiEvaluationMetricKind` for success rate, quality score, latency, cost, fallback frequency, tool success, plan completion, and custom metrics.
- Added bounded `AiEvaluationMetric` with provider-neutral direction semantics; custom metrics require an explicit higher-is-better/lower-is-better declaration.
- Added `AiEvaluationSample` and `AiEvaluationAggregationRequest` with stable case/variant identity, bounded sample count, validation, and detached clone ownership.
- Added `AiEvaluationAggregate` / `AiEvaluationAggregateMetric` for outcome counts and average/minimum/maximum metric summaries.
- Added `AiEvaluationComparison` / `AiEvaluationMetricComparison` for left/right metric averages, left-minus-right deltas, and strictly preferred variants where both sides are comparable.
- Added `AiEvaluationAggregator` with cancellation checks, deterministic ordering, bounded input, detached aggregation snapshots, and no authoritative side effects.
- Added focused `EvaluationAggregationTests.cs` and public `Evaluation Aggregation` Example verification.
- User verification: **130/130 HAgent.Tests passed**, with 0 failed and 0 skipped.
- User Example verification on .NET Framework 4.8.1 and .NET 9 produced baseline success rate `0.333333`, candidate success rate `1`, candidate average quality `0.85`, candidate average latency `110 ms`, and no authoritative routing or authorization decision.

## Slice 6 — Evaluation regression suites and repeated target execution

**Verified on 2026-09-09.**

- Added `AiEvaluationRegressionCase` for bounded reusable test-case identity, input references, and host-defined parameters.
- Added `AiEvaluationRegressionTarget` for bounded alternative target identity, name, and metadata without hard-coding provider/model semantics into Core.
- Added `AiEvaluationRegressionSuite` for bounded case/target matrices, unique identifiers, `MaxConcurrency` from 1 to 32, and a maximum of 1024 case-target executions.
- Added `AiEvaluationRegressionCaseResult` and `AiEvaluationRegressionRun` with explicit completed/failed/canceled lifecycle states, bounded failure codes, deterministic ordering, validation, and completed-sample ownership.
- Added `IAiEvaluationRegressionExecutor` as the host-owned execution boundary. The runner does not select providers, credentials, authorization, or production routing.
- Added `AiEvaluationRegressionRunner.RunAsync` with detached suite snapshots, semaphore-bounded concurrency, complete case-target repetition, cancellation propagation, executor failure isolation, invalid/null sample rejection, case/variant identity protection, and late-result discard after cancellation.
- Added `AiEvaluationRegressionRun.CreateAggregationRequest()` to expose only completed, validated `AiEvaluationSample` evidence through the existing Slice 5 aggregation contract.
- Added focused `EvaluationRegressionTests.cs` covering suite validation, matrix execution, deterministic ordering, concurrency bounds, failure isolation, identity mismatch, cancellation/late-result protection, snapshot isolation, and aggregation handoff ownership.
- Added public `MainForm.EvaluationRegression.cs` and registered it as `Diagnostics → Evaluation → Evaluation Regression Suites`.
- User verification: **139/139 HAgent.Tests passed** with 0 failed and 0 skipped.
- User Example verification on **.NET Framework 4.8.1 and .NET 9** succeeded with 3 cases × 2 targets, 6 completed executions, maximum observed concurrency of 2, isolated failure handling, late-result cancellation protection, and successful-sample-only aggregation handoff.

## Architectural invariants

```text
Evaluation state                  Authoritative runtime/cognitive state
-----------------                 ------------------------------------
Outcome / score / label           Execution terminal state
Evaluator provenance              Authorization decision
Evidence references               Configuration / memory / knowledge
Diagnostic correlation            Cognitive revision / learning promotion
```

- Evaluation is evidence about behavior, not hidden authorization.
- Model-assisted evaluations are explicitly non-authoritative and retain evaluator provenance.
- Evaluation contracts use bounded references, observations, ratings, metrics, regression cases/targets, and metadata instead of raw prompts, responses, tool payloads, credentials, or arbitrary host objects.
- Evaluation must not mutate authoritative agent state merely because an evaluation passes.
- Deterministic rules evaluate explicit host-computed facts; they do not independently inspect or authorize host-domain state.
- Human/Application ratings are externally supplied evidence and never become authorization decisions by virtue of evaluator kind.
- Model-assisted judging stays behind an injected provider-neutral judge contract; Core does not become a hidden model router.
- Aggregation and comparison are measurement-only and do not route execution, authorize actions, alter configuration, promote learning, or mutate cognitive state.
- Regression suites orchestrate repeated measurement only through an injected host-owned executor; they do not become provider routers or production schedulers.
- Executor failures and cancellation are lifecycle outcomes of the regression run, not fabricated evaluation truth.

## Architectural outcome

```text
Execution / Response / Tool / Goal / Plan / Memory-Knowledge / Learning Candidate
        ↓
   AiEvaluationRequest
        ↓
      IAiEvaluator
        ↓
     AiEvaluation
        ↓
 outcome + score + label + evidence + provenance
        ↓
 bounded regression case × target execution
        ↓
 completed AiEvaluationSample evidence
        ↓
 bounded aggregation by target variant
        ↓
 alternative-target comparison evidence
```

Evaluation measures behavior; later policy-controlled subsystems may consume evaluation evidence, but evaluation itself does not become a decision-maker for authorization or execution routing.

The next ordered milestone is **0.9575 Knowledge, Skills, Memory Governance + Learning**. The active implementation work for that milestone is maintained in `docs/plan/20-active.md`.

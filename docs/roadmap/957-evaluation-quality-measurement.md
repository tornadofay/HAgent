# Phase 0.957 — Evaluation and Quality Measurement

## Status

**In progress — Slice 5 implementation checkpoint; build/test/Example verification pending.**

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
9. [ ] Support repeated test cases and regression suites for provider/model/agent comparisons.
10. [ ] Support aggregate metrics such as success rate, quality score, latency, cost, fallback frequency, tool success, and plan completion.
11. [ ] Add deterministic Example verification for evaluation creation, aggregation, human rating, failed evaluations, and comparison of alternative execution targets.

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
- Added deterministic schema validity, required-field completeness, policy compliance, tool success, latency, cost, and task-completion rules.
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
- This slice did not add model-assisted grading, aggregation, regression suites, persistence, or management UI.

## Slice 4 — Model-assisted evaluators and non-authoritative judge boundary

**Verified on 2026-09-09.**

- Added `IAiEvaluationJudge` as the provider-neutral asynchronous judge boundary.
- Added `AiEvaluationJudgeRequest` as a detached clone of `AiEvaluationRequest`, protecting active judging from caller mutation and preserving provider-neutral bounded inputs, observations, and criteria.
- Added `AiModelAssistedEvaluationEvaluator` implementing `IAiEvaluator` with explicit `ModelAssisted` provenance and bounded evaluator identity/version.
- Reused `AiEvaluationRating` as the bounded judge result instead of introducing a second evaluation-result model; the evaluator maps it into normal `AiEvaluation` evidence while retaining judge provenance.
- Explicitly records `evaluation.source=model-assisted` and `evaluation.authoritative=false`.
- Provider transport, credentials, model selection, retries, and host-specific evidence resolution remain outside Core in the injected judge implementation/owning subsystem.
- Cancellation is checked before judge invocation and after judge completion so a late result cannot become an evaluation after cancellation.
- Judge failure, null output, and invalid bounded rating are rejected rather than converted into fabricated evaluation evidence.
- Added focused `ModelAssistedEvaluationTests.cs` and public `Model-Assisted Evaluation` Example.
- User verification: **123/123 HAgent.Tests passed** and the Slice 4 Example succeeded on **.NET Framework 4.8.1 and .NET 9**.

## Slice 5 — Evaluation aggregation and alternative-target comparison

**Implementation checkpoint — verification pending.**

- Added `AiEvaluationMetricKind` for success rate, quality score, latency, cost, fallback frequency, tool success, plan completion, and custom metrics.
- Added bounded `AiEvaluationMetric` with provider-neutral direction semantics; custom metrics require an explicit higher-is-better/lower-is-better declaration.
- Added `AiEvaluationSample` and `AiEvaluationAggregationRequest` with stable case/variant identity, bounded sample count, validation, and detached clone ownership.
- Added `AiEvaluationAggregate` / `AiEvaluationAggregateMetric` for outcome counts and average/minimum/maximum metric summaries.
- Added `AiEvaluationComparison` / `AiEvaluationMetricComparison` for left/right averages, left-minus-right deltas, and strictly preferred variants where both sides are comparable.
- Added `AiEvaluationAggregator.Aggregate` and `.Compare` with cancellation checks, deterministic ordering, bounded input, detached aggregation snapshots, and no authoritative side effects.
- Added focused `EvaluationAggregationTests.cs` covering validation, grouping, outcome counts, success/quality averages, explicit metrics, comparison directions, one-sided metrics, cancellation, and snapshot isolation.
- Added public `MainForm.EvaluationAggregation.cs` and registered it as `Diagnostics → Evaluation → Evaluation Aggregation`.
- Slice 5 implements the aggregate-metric requirement but does not yet mark requirement 10 or the Example verification requirement 11 complete until the supported-target build/tests and both manual Example runs are confirmed.
- Requirement 9 (repeated test cases/regression-suite orchestration) remains deliberately outside Slice 5 and is the next distinct evaluation slice after Slice 5 verification.

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
- Evaluation contracts use bounded references, observations, ratings, and metadata instead of raw prompts, responses, tool payloads, credentials, or arbitrary host objects.
- Evaluation must not mutate authoritative agent state merely because an evaluation passes.
- Deterministic rules evaluate explicit host-computed facts; they do not independently inspect or authorize host-domain state.
- Human/Application ratings are externally supplied evidence and never become authorization decisions by virtue of evaluator kind.
- Model-assisted judging stays behind an injected provider-neutral judge contract; Core does not become a hidden model router.
- Aggregation and comparison are measurement-only and do not route execution, authorize actions, alter configuration, promote learning, or mutate cognitive state.

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
 bounded aggregation by target variant
        ↓
 alternative-target comparison evidence
```

Evaluation measures behavior; later policy-controlled subsystems may consume evaluation evidence, but evaluation itself does not become a decision-maker for authorization or execution routing.

# Phase 0.957 — Evaluation and Quality Measurement

## Status

**In progress — Slice 3 is verified; the next numbered slice is not yet started.**

## Goal

Give HAgent a provider-neutral way to measure whether executions, tool use, plans, learning changes, and agent outcomes achieved their intended quality or task goals.

## Requirements

1. [ ] Define evaluation contracts independent of any specific LLM vendor or grading service.
2. [ ] Support evaluation targets including execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning candidate quality.
3. [ ] Support deterministic evaluators such as schema validity, required-field checks, policy compliance, tool success, latency, cost, and task completion signals.
4. [ ] Support externally supplied human/application ratings and labels.
5. [ ] Support model-assisted evaluators without treating evaluator-model output as unquestionable truth.
6. [ ] Preserve evaluation provenance, evaluator identity/type, input references, timestamp, and confidence where meaningful.
7. [ ] Correlate evaluations with execution/runtime/agent/goal/plan/trace identities.
8. [ ] Keep evaluation data separate from authoritative agent state; an evaluation does not automatically mutate configuration, memory, skill, or knowledge.
9. [ ] Support repeated test cases and regression suites for provider/model/agent comparisons.
10. [ ] Support aggregate metrics such as success rate, quality score, latency, cost, fallback frequency, tool success, and plan completion.
11. [ ] Add deterministic Example verification for evaluation creation, aggregation, human rating, failed evaluations, and comparison of alternative execution targets.

## Slice 1 — Provider-neutral evaluation contracts and evaluator boundary

**Verified on 2026-09-09.**

- Added `AiEvaluationTargetKind` for execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning-candidate quality targets.
- Added `AiEvaluationInputReference`, `AiEvaluationRequest`, and `AiEvaluation` with bounded validation, provenance, correlation, score/label/outcome semantics, and owned clone behavior.
- Added `IAiEvaluator` as the asynchronous evaluator boundary with explicit evaluator identity, kind, and version.
- Kept evaluation evidence separate from authorization, terminal execution state, persistent configuration, memory, knowledge, skills, and learning promotion.
- Added focused `tests/HAgent.Tests/EvaluationContractsTests.cs` coverage and a public deterministic Example under `Diagnostics → Other Diagnostics → Evaluation Contracts`.
- Added `docs/architecture/23-evaluation-quality.md` as the authoritative evaluation ownership boundary.
- User verification: full `HAgent.Tests` completed with **96/96 tests passed** on .NET 9.
- User Example verification: **.NET Framework 4.8.1 at 2026-09-09 06:26:48** succeeded, verifying provider-neutral target kinds, outcome/score/label representation, evaluator provenance, execution/runtime/goal/plan/trace correlation, bounded input/evidence references, owned clone isolation, no agent-state mutation or authorization side effect, no remote grading/provider transport, and no real provider request.
- User Example verification: **.NET 9 at 2026-09-09 06:26:18** succeeded with the same public-API checks.

## Slice 2 — Deterministic evaluators and evaluation evidence

**Verified on 2026-09-09.**

- Added bounded `AiEvaluationObservation` values with explicit `Boolean`, `Decimal`, and `Text` value kinds and optional bounded units.
- Extended `AiEvaluationRequest` with bounded, owned observations and clone/validation behavior.
- Added `AiDeterministicEvaluationRuleKind` for schema validity, required-field completeness, policy compliance, tool success, latency, cost, and task completion.
- Added `AiDeterministicEvaluationEvaluator` implementing `IAiEvaluator` with deterministic pass/fail scoring, evaluator provenance, execution/runtime/agent/goal/plan/trace correlation, bounded evidence references, threshold handling, cancellation, and `Inconclusive` outcomes for missing/mismatched signals.
- Duplicate observations for the same deterministic signal are rejected as ambiguous rather than arbitrarily selecting one.
- Added focused `tests/HAgent.Tests/DeterministicEvaluationTests.cs` and matching public `src/HAgent.Example/MainForm.DeterministicEvaluation.cs` coverage.
- Classified the Example as `Diagnostics → Evaluation → Deterministic Evaluation`.
- User verification: **109/109 `HAgent.Tests` passed**.
- User Example verification: **.NET Framework 4.8.1** and **.NET 9** `Deterministic Evaluation` scenarios succeeded.
- This slice does not implement model-assisted grading, human-rating workflows, aggregation, regression suites, persistence, or management UI.

## Slice 3 — Human/application ratings and labeled evaluation evidence

**Verified on 2026-09-09.**

- Added bounded `AiEvaluationRating` for externally supplied outcome, score, confidence, label, reason, evidence references, and metadata.
- Added `AiSuppliedRatingEvaluator` through the existing `IAiEvaluator` boundary; it accepts only `Human` or `Application` evaluator kinds and requires explicit evaluator identity/version.
- Preserved supplied rating values plus execution/runtime/agent/goal/plan/trace correlation and bounded evidence/metadata.
- Cloned supplied rating data at evaluator construction and cloned its evidence/metadata into each produced evaluation so caller mutation cannot alter prior results.
- Cancellation is checked before producing supplied evaluation evidence.
- Added focused `tests/HAgent.Tests/SuppliedEvaluationTests.cs` and matching public `src/HAgent.Example/MainForm.SuppliedEvaluation.cs` coverage.
- Classified the Example as `Diagnostics → Evaluation → Supplied Evaluation Ratings`.
- User verification: **115/115 `HAgent.Tests` passed** on .NET 9.
- User Example verification — **.NET Framework 4.8.1 at 2026-09-09 06:53:44**: `Supplied Evaluation Ratings` succeeded, verifying Human/Application outcome, score, label, provenance, evidence ownership, execution/response correlation, non-authoritative behavior, and no provider/model transport.
- User Example verification — **.NET 9 at 2026-09-09 06:52:58**: `Supplied Evaluation Ratings` succeeded with the same public-API checks.
- This slice does not add model-assisted grading, aggregation, regression suites, persistence, or management UI.

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
```

Evaluation measures behavior; later policy-controlled subsystems may consume evaluation evidence, but evaluation itself does not become a decision-maker for authorization.

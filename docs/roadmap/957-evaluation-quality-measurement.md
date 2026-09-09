# Phase 0.957 — Evaluation and Quality Measurement

## Status

**In progress — Slice 2 is the next implementation checkpoint.**

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

## Next checkpoint

2. **Deterministic evaluators and evaluation evidence — NOT STARTED**
- Implement deterministic evaluator contracts/implementations such as bounded schema validity, required-field checks, policy-compliance signals, tool success, latency, cost, and task-completion signals.
- Keep evaluator results non-authoritative and preserve evidence/provenance without mutating runtime or cognitive state.
- Add focused tests and a public Example for deterministic evaluation evidence.
- Do not implement later model-assisted grading, aggregation, regression-suite, or management-UI slices in the same run.

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
- Evaluation contracts use bounded references/metadata instead of raw prompts, responses, tool payloads, credentials, or arbitrary host objects.
- Evaluation must not mutate authoritative agent state merely because an evaluation passes.

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

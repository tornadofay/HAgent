# Phase 0.957 — Evaluation and Quality Measurement

## Status

**Planned architectural foundation for reliable agent behavior and later optimization.**

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

## Architectural outcome

```text
Execution / Goal / Plan
        ↓
    Evaluation
        ↓
 score / label / evidence
        ↓
  metrics / regression
```

Evaluation measures behavior; it does not become a hidden decision-maker for authorization.
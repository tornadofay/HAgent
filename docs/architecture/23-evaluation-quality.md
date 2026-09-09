# Evaluation and Quality Architecture

HAgent evaluation measures behavior without becoming an execution or authorization authority. The evaluation layer is provider-neutral and must work with deterministic evaluators, human/application ratings, and model-assisted evaluators without assuming that any evaluator is infallible.

## Boundary

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

`IAiEvaluator` describes how an evaluation is produced. `AiEvaluation` describes the resulting evidence. The evaluation subsystem does not grant authorization, change configuration, promote learning candidates, or mutate authoritative cognitive state merely because an evaluation passes.

## Target identity

`AiEvaluationTargetKind` is provider-neutral and currently supports:

- `Execution`
- `Response`
- `ToolOutcome`
- `GoalOutcome`
- `PlanOutcome`
- `MemoryKnowledgeUsefulness`
- `LearningCandidate`

The target is identified separately from the evaluation itself so repeated evaluations can be performed against the same target by different evaluators or at different times.

## Provenance and correlation

Every evaluation identifies the evaluator and, where applicable, evaluator version, timestamp, outcome, score, confidence, label, reason, and bounded evidence references. Execution, runtime, agent, goal, plan, and trace identities are carried as diagnostic correlation fields rather than authorization authority.

Model-assisted evaluation is therefore evidence with provenance, not unquestionable truth. Human/application ratings fit the same contract and remain distinguishable by evaluator kind.

## Deterministic evaluation input

Deterministic evaluators consume host-computed `AiEvaluationObservation` values rather than raw prompts, responses, tool arguments, credentials, or arbitrary host objects. An observation is bounded by:

- a stable kind and evidence ID;
- one explicit value kind (`Boolean`, `Decimal`, or `Text`);
- a bounded value; and
- an optional bounded unit.

`AiEvaluationRequest.Observations` is bounded to 32 entries and is cloned with owned observation instances. This keeps deterministic evaluation provider-neutral while allowing the host to perform domain-specific inspection before HAgent receives the measurement.

The current deterministic signal vocabulary is deliberately small and extensible through the rule enum:

```text
schema.valid                 -> Boolean
required-fields.complete     -> Boolean
policy.compliant             -> Boolean
tool.success                 -> Boolean
task.completed               -> Boolean
latency.ms                   -> Decimal + max criterion
cost                         -> Decimal + max criterion
```

The required-fields, policy, tool, and task signals represent host-computed facts. HAgent evaluates the declared fact; it does not scrape business objects, authorize operations, or reinterpret host-domain semantics.

## Deterministic evaluator semantics

`AiDeterministicEvaluationEvaluator` implements `IAiEvaluator` for the current deterministic rule set. Each evaluator instance has explicit deterministic rule identity plus evaluator ID/version provenance.

Boolean rules map `true` to `Passed` with score `1` and `false` to `Failed` with score `0`. Latency and cost rules compare the bounded decimal observation against the invariant-culture `max` criterion and use the same pass/fail scoring.

Missing observations, value-kind mismatches, and missing/invalid threshold criteria produce `Inconclusive` with no score rather than inventing success or failure. Duplicate observations for one deterministic signal are rejected as ambiguous instead of selecting an arbitrary value.

Each successful deterministic evaluation carries a bounded `AiEvaluationInputReference` pointing to the evaluated observation plus bounded metadata for the rule, observed value, unit, and threshold where applicable. Evaluation output therefore preserves evidence/provenance without copying a raw payload into the evaluation result.

Cancellation is checked at the evaluator boundary before producing the deterministic result.

## Safety and ownership

Evaluation contracts intentionally contain bounded references, observations, and metadata rather than raw prompts, responses, tool arguments, credentials, or arbitrary host objects. Storage and retention are separate concerns and must apply their own governance.

Evaluation results remain separate from authoritative state. Any later policy-controlled learning or cognitive revision must explicitly consume evaluation evidence through its owning subsystem; evaluation itself never performs that mutation.

## Slice boundary

Phase 0.957 Slice 1 established only the provider-neutral contract and evaluator boundary.

Phase 0.957 Slice 2 adds the deterministic observation contract and deterministic evaluator implementation for schema validity, required-field completeness, policy compliance, tool success, task completion, latency, and cost. It does not add model-assisted grading, human-rating workflows, aggregation, regression suites, persistence, or management UI.

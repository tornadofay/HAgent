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

## Supplied human/application ratings

Externally supplied ratings use `AiEvaluationRating` as bounded input data rather than introducing a second evaluation result type. A rating may carry an outcome, optional score, optional confidence, bounded evidence references, and bounded metadata.

`AiSuppliedRatingEvaluator` adapts that supplied rating through the existing `IAiEvaluator` contract. It accepts only `Human` or `Application` evaluator kinds and requires explicit evaluator identity/version. The resulting `AiEvaluation` preserves the supplied rating plus request correlation and marks the rating source in bounded metadata.

The evaluator clones the supplied rating on construction and clones evidence/metadata into each produced evaluation. A caller therefore cannot mutate an already-produced evaluation by modifying the original rating object.

Cancellation is checked before producing the supplied evaluation. Supplied ratings are evidence only; a Human or Application evaluator does not grant authorization or mutate agent, memory, knowledge, skill, configuration, or cognitive state.

## Model-assisted evaluation

Model-assisted evaluation uses one additional provider-neutral boundary: `IAiEvaluationJudge`. The judge is a grading component, not an authorization or cognitive authority. `AiModelAssistedEvaluationEvaluator` remains the `IAiEvaluator` implementation visible to callers and delegates model-specific judging to an injected `IAiEvaluationJudge`.

The evaluator constructs an `AiEvaluationJudgeRequest` from a detached clone of the caller's `AiEvaluationRequest`. This prevents caller mutation during an asynchronous judge call from changing the active evaluation target, criteria, inputs, or observations. Each invocation owns its request snapshot; the evaluator stores no mutable per-execution state and can therefore be used concurrently when the injected judge is concurrency-safe.

The judge returns the existing bounded `AiEvaluationRating` contract rather than a second evaluation-result type. This keeps the model-produced outcome/score/confidence/label/reason/evidence semantics aligned with externally supplied evaluation evidence while evaluator provenance remains on the final `AiEvaluation`.

A model-backed judge may live in an application or provider adapter assembly and may use its own host-owned evidence resolver and provider transport. Core does not receive raw prompts, raw responses, tool payloads, credentials, or arbitrary host objects through the evaluation contract. Bounded `AiEvaluationInputReference` values identify the material being judged without turning those references into authorization.

`AiModelAssistedEvaluationEvaluator` preserves the judge rating, request correlation, evaluator identity/version, and bounded judge provenance. It adds explicit `evaluation.source=model-assisted` and `evaluation.authoritative=false` metadata. `NeedsReview` remains a valid model-assisted outcome, and evaluator code does not convert low confidence or disagreement into authorization decisions.

Failure is fail-closed at the evaluator boundary: a missing judge, null judge rating, invalid bounded rating, or canceled operation does not produce a fabricated evaluation. Cancellation is checked both before the judge invocation and after it returns, so a late judge result cannot become a successful evaluation after cancellation.

The model-assisted boundary therefore has this shape:

```text
AiEvaluationRequest
        ↓ cloned snapshot
AiEvaluationJudgeRequest
        ↓
   IAiEvaluationJudge
        ↓ bounded AiEvaluationRating
AiModelAssistedEvaluationEvaluator
        ↓
     AiEvaluation
        ↓
 outcome + score + confidence + label + evidence + provenance
```

The model judge may be deterministic in tests or backed by a real model in an adapter implementation. The evaluation layer does not treat either as inherently authoritative.

## Safety and ownership

Evaluation contracts intentionally contain bounded references, observations, ratings, and metadata rather than raw prompts, responses, tool arguments, credentials, or arbitrary host objects. Storage and retention are separate concerns and must apply their own governance.

Evaluation results remain separate from authoritative state. Any later policy-controlled learning or cognitive revision must explicitly consume evaluation evidence through its owning subsystem; evaluation itself never performs that mutation.

A model-assisted evaluator must not become a hidden provider router. Provider selection, credentials, network transport, model capability, and retry behavior belong to the injected judge implementation or other owning subsystem. The Core evaluator remains responsible only for the evaluation contract, snapshot isolation, cancellation, validation, and evidence/provenance mapping.

## Slice boundary

Phase 0.957 Slice 1 established only the provider-neutral contract and evaluator boundary.

Phase 0.957 Slice 2 adds the deterministic observation contract and deterministic evaluator implementation for schema validity, required-field completeness, policy compliance, tool success, task completion, latency, and cost.

Phase 0.957 Slice 3 adds bounded externally supplied Human/Application ratings through the same provider-neutral evaluator boundary.

Phase 0.957 Slice 4 adds the provider-neutral `IAiEvaluationJudge` boundary and `AiModelAssistedEvaluationEvaluator`, including detached request snapshots, judge/evaluator provenance, cancellation/late-result protection, bounded rating/evidence ownership, and explicit non-authoritative semantics. It does not add model-specific provider adapters, evaluation aggregation, regression suites, persistence, or management UI.

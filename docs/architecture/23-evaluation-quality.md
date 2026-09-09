# Evaluation and Quality Architecture

HAgent evaluation measures behavior without becoming an execution or authorization authority. The evaluation layer is provider-neutral and must work with deterministic evaluators, human/application ratings, model-assisted evaluators, and regression-suite orchestration without assuming that any evaluator is infallible.

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

## Aggregation and alternative-target comparison

Evaluation aggregation consumes completed `AiEvaluation` evidence plus bounded metric observations; it does not run providers or reinterpret host-domain payloads. `AiEvaluationSample` binds a stable `CaseId` to a `VariantId`, one evaluation, and bounded metrics. `AiEvaluationAggregationRequest` is limited to 256 samples and clones its samples before aggregation so caller mutation cannot affect active work.

`AiEvaluationMetricKind` provides the canonical initial measurement vocabulary:

```text
SuccessRate          -> higher is better
QualityScore         -> higher is better
LatencyMilliseconds  -> lower is better
Cost                 -> lower is better
FallbackFrequency    -> lower is better
ToolSuccessRate      -> higher is better
PlanCompletionRate   -> higher is better
Custom               -> direction required explicitly
```

`AiEvaluationAggregator.Aggregate` groups samples by variant and computes outcome counts, passed/sample success rate, average non-null evaluation score, and average/minimum/maximum values for explicit metrics. Standard ratio metrics are bounded to `[0,1]`; other standard metrics remain non-negative. Results are deterministic and bounded, and cancellation is checked before and during work.

`AiEvaluationAggregator.Compare` consumes two validated aggregates and produces one `AiEvaluationComparison` containing left/right averages, left-minus-right delta, metric direction, and an optional `PreferredVariantId`. A preferred variant is present only when both sides have a value and the comparison is strictly better for one side; ties and one-sided metrics do not produce a preference. This field is comparative evidence only, not a routing recommendation, authorization decision, policy decision, configuration mutation, learning promotion, or cognitive authority.

Derived success/quality metrics are part of the canonical aggregate and comparison shape rather than ad-hoc example calculations. Explicit custom metrics retain their declared direction. The aggregation layer does not persist results, schedule regression runs, invoke human review, or call provider/model transport.

## Regression suites and repeated target execution

Regression suites build repeated, comparable measurements from the same bounded case definitions across alternative target variants. They are provider-neutral orchestration rather than a hidden provider router.

`AiEvaluationRegressionCase` identifies a reusable test case and carries only bounded input references and host-defined parameters. `AiEvaluationRegressionTarget` identifies one comparison variant and may carry bounded provider/model/agent descriptors as metadata without making those concepts mandatory Core semantics. This allows a host to compare a provider, model, agent profile, or another explicitly named target without hard-coding a vendor taxonomy into Core.

`AiEvaluationRegressionSuite` owns the case/target matrix and a bounded `MaxConcurrency`. The suite is validated before execution, detached by cloning, and capped at 1024 case-target executions. It contains no executable delegates, credentials, live runtime instances, or persistent state.

`IAiEvaluationRegressionExecutor` is the host-owned execution boundary. For each case-target pair it receives detached case/target objects and returns the existing `AiEvaluationSample` contract. HAgent.Core therefore does not need to know whether the host invokes an `HAgentClient`, a runtime instance, a deterministic test double, a remote adapter, or another host-owned execution mechanism.

`AiEvaluationRegressionRunner.RunAsync` schedules the complete case-target matrix with the suite concurrency bound. Results are returned in deterministic case/target order. A successful executor result is cloned and validated before entering the run report. Null results, invalid samples, identity mismatches, and executor exceptions become bounded regression case failures and never become fabricated evaluations.

Cancellation is cooperative and late-result safe. A cancellation request prevents new work from entering the executor when possible, is passed to in-flight execution, and causes a completed executor result returned after cancellation to be discarded rather than recorded. Canceled case results are explicit and distinct from `AiEvaluationOutcome.Canceled` because cancellation is a regression-execution lifecycle state, not an evaluation truth value.

`AiEvaluationRegressionRun.CreateAggregationRequest` exposes only completed, validated samples through the existing `AiEvaluationAggregationRequest`. No parallel metric or comparison model is introduced. Failed or canceled case-target executions remain visible in the regression report so suite completeness can be distinguished from evaluation quality.

Regression orchestration never routes production execution, selects credentials, authorizes operations, persists authoritative state, promotes learning, or mutates cognitive state. The host-owned executor remains responsible for those concerns and for any provider/model-specific selection.

## Safety and ownership

Evaluation contracts intentionally contain bounded references, observations, ratings, metrics, regression cases/targets, and metadata rather than raw prompts, responses, tool arguments, credentials, or arbitrary host objects. Storage and retention are separate concerns and must apply their own governance.

Evaluation results remain separate from authoritative state. Any later policy-controlled learning or cognitive revision must explicitly consume evaluation evidence through its owning subsystem; evaluation itself never performs that mutation.

A model-assisted evaluator must not become a hidden provider router. Provider selection, credentials, network transport, model capability, and retry behavior belong to the injected judge implementation or other owning subsystem. The Core evaluator remains responsible only for the evaluation contract, snapshot isolation, cancellation, validation, and evidence/provenance mapping.

Regression execution similarly must not become a hidden provider router. The suite describes what to repeat and compare; the injected executor describes how the host executes one target. This keeps evaluation reusable across providers, models, agent profiles, and deterministic test doubles without expanding Core's authority.

## Slice boundary

Phase 0.957 Slice 1 established only the provider-neutral contract and evaluator boundary.

Phase 0.957 Slice 2 adds the deterministic observation contract and deterministic evaluator implementation for schema validity, required-field completeness, policy compliance, tool success, task completion, latency, and cost.

Phase 0.957 Slice 3 adds bounded externally supplied Human/Application ratings through the same provider-neutral evaluator boundary.

Phase 0.957 Slice 4 adds the provider-neutral `IAiEvaluationJudge` boundary and `AiModelAssistedEvaluationEvaluator`, including detached request snapshots, judge/evaluator provenance, cancellation/late-result protection, bounded rating/evidence ownership, and explicit non-authoritative semantics.

Phase 0.957 Slice 5 adds bounded aggregation and alternative-target comparison over completed evaluation evidence. It does not add provider/model adapters, regression-suite orchestration, persistence, or management UI.

Phase 0.957 Slice 6 adds repeated case-target regression execution through `IAiEvaluationRegressionExecutor`, bounded concurrency, deterministic results, failure isolation, cancellation/late-result protection, and direct handoff of completed samples into Slice 5 aggregation/comparison.

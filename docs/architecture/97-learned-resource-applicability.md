# Learned Resource Applicability

## Scope

Phase 0.9576 Slice 1 defines the post-promotion applicability and validity boundary for learned Skills, Knowledge, Memory-derived resources, and future learned resource families.

Applicability is separate from authorization. A resource may be authorized and still be inapplicable, uncertain, or invalidated.

## Boundary

```text
promoted resource version
        |
        v
IAiApplicabilityEvaluator
        |
        +--> deterministic applicability decision
        |      Applicable
        |      NotApplicable
        |      Uncertain
        |      Invalidated
        |
        +--> bounded condition results
        +--> evidence references
        +--> resource version identity
```

The evaluator does not publish, mutate, authorize, or retire resources.

## Deterministic evaluation

`AiDeterministicApplicabilityEvaluator` uses only bounded host-supplied facts, resource scope, and explicit preconditions. When those inputs are sufficient, applicability is decided without requesting model reasoning.

Supported condition operators are deliberately small and deterministic:

- `Exists`
- `Equals`
- `NotEquals`
- `OneOf`

A missing fact required to establish applicability produces `Uncertain`, not `Applicable`. A failed condition produces `NotApplicable`. An explicitly invalidated target produces `Invalidated` before normal applicability checks.

## Scope

Scope is part of the applicability input but is not an authorization decision. A supplied context scope that differs from the resource scope produces `NotApplicable`. A scope-dependent resource evaluated without a scope produces `Uncertain`.

## Evidence

Targets may carry bounded evidence references and each precondition may identify the evidence reference supporting it. The returned `AiApplicabilityDecision` copies these references and records per-condition observed values and evidence availability so the decision can be captured by the existing observability/diagnostic layers without reconstructing the evaluation later.

## Validity

`IsInvalidated` and its bounded reason are explicit target state. Invalidated resources are never converted back into `Applicable` by deterministic applicability evaluation.

This slice does not implement reliability scoring, staleness, contradiction detection, quarantine, retirement, archival, forgetting, or runtime fallback. Those remain later 0.9576 slices.

## Governance invariants

1. Applicability never grants authorization.
2. `Uncertain` never means `Applicable`.
3. Invalidated resources are not usable through the applicability boundary.
4. Published resource versions are not mutated by applicability evaluation.
5. Applicability evaluation is provider-neutral and requires no GPU, embeddings, vector database, or model invocation.
6. Applicability decisions retain enough bounded evidence to be observable and auditable.

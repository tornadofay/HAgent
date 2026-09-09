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

## Safety and ownership

Evaluation contracts intentionally contain bounded references and metadata rather than raw prompts, responses, tool arguments, credentials, or arbitrary host objects. Storage and retention are separate concerns and must apply their own governance.

Evaluation results remain separate from authoritative state. Any later policy-controlled learning or cognitive revision must explicitly consume evaluation evidence through its owning subsystem; evaluation itself never performs that mutation.

## Slice boundary

Phase 0.957 Slice 1 establishes only the provider-neutral contract and evaluator boundary. Deterministic grading rules, human-rating workflows, model-assisted evaluation, aggregation, regression suites, persistence, and management UI belong to later slices.

# Model Reasoning Engineering

## Purpose

Phase 0.98 defines the engineering boundary between HAgent's deterministic runtime knowledge and provider/model-based reasoning.

The provider-neutral pre-execution reasoning contract is established by Phase 0.9593. Persistent cognition remains owned by `docs/architecture/16-cognitive-runtime.md` and its executable algorithm specification. Execution-target selection remains owned by `docs/architecture/07-execution-planning.md`. Instruction authority remains owned by `docs/architecture/11-instruction-governance.md`, and context retrieval/assembly remains owned by `docs/architecture/20-context.md`.

## Core principle

> Do not ask the model to infer deterministic facts when HAgent can provide those facts directly, unless inferring them is itself the requested task.

This is a responsibility boundary, not a model-quality assumption. A stronger model may solve unnecessary work better than a weaker model, but HAgent should still avoid delegating deterministic facts that its own contracts already know.

The complementary rule is:

> Inference is appropriate when inference is the declared task, the required evidence is available, and the result remains inside an explicit authority boundary.

## Responsibility model

```text
Deterministic HAgent state/facts
        ↓
Authoritative structured evidence/context
        ↓
Provider-neutral reasoning requirement (0.9593)
        ↓
Bounded model reasoning
        ↓
Provider-neutral result contract
        ↓
HAgent validation
        ↓
Policy / authorization / cognitive state transition / host action
```

Examples of deterministic HAgent-owned facts include execution identity, runtime identity/state, source/provenance metadata, capability state, configured permissions, policy decisions, and other values already established by authoritative contracts.

Examples of legitimate model responsibility include semantic interpretation, extraction, inference, planning, and generation when those are the requested operations.

## Reasoning responsibility is not authority

A model may propose:

- an interpretation;
- an inferred fact;
- a classification required by the task;
- a plan;
- a tool request;
- a learning candidate;
- another structured result.

The proposal remains non-authoritative until it passes the existing relevant HAgent boundary. Model output cannot by itself grant permissions, authorize tools, publish learned resources, change configuration, or commit authoritative cognitive state.

## Deterministic-before-inferential behavior

When the current cognitive/runtime state and deterministic operators are sufficient, cognition should progress without model invocation. When probabilistic reasoning is genuinely required, the cognitive layer produces the provider-neutral reasoning requirement defined by 0.9593 and the execution planner selects an admitted target.

0.98 does not create a second model-selection path and does not move cognition's deterministic-vs-probabilistic decision into provider adapters.

## Reasoning task composition

A reasoning request should represent the semantic responsibility that actually requires model judgment.

HAgent should avoid bundling independent responsibilities such as:

```text
source identification
+ policy authorization
+ memory promotion
+ semantic extraction
+ execution decision
```

into one model-owned decision merely because a single prompt can request all of them.

Deterministic facts should be supplied as structured evidence where practical. Instructions can describe the reasoning job, but prompt text is not the authority for runtime facts or permissions.

## Result and uncertainty

Structured output is a contract, not proof of semantic correctness. Reasoning results may need to distinguish successful results from insufficient evidence, ambiguity, conflicts, or unsupported inference. These outcomes should be represented by provider-neutral contracts where the specific reasoning capability requires them.

A model-reported confidence value is evidence produced by the model; it is not automatically a calibrated probability and never becomes authorization or truth merely because it is high.

## Reuse of existing architecture

0.98 must reuse:

- Prompt + Instruction Governance for instruction authority and provenance;
- Context Engineering for retrieval, admission, ranking, and bounded context snapshots;
- Evaluation + Quality Measurement for correctness and provider/model comparison;
- Policy and authorization boundaries for enforcement;
- Capability-Aware Execution for concrete target selection/admission;
- Persistent Cognitive Runtime for deciding when probabilistic reasoning is needed;
- 0.9593 for the provider-neutral reasoning requirement and responsibility boundary.

A new subsystem is justified only when an existing contract cannot express a requirement that belongs to the 0.98 responsibility.

## Contract ownership

Phase 0.9593 owns the **pre-execution reasoning requirement contract** and decides its canonical shape (`ReasoningTask`, `ReasoningRequirement`, or another equivalent provider-neutral contract).

Phase 0.98 consumes that contract and owns the additional engineering required to constrain, validate, evaluate, and safely handle model reasoning results.

This separation ensures 0.98 does not recreate the cognitive runtime, execution planner, context system, instruction system, or the basic reasoning-request contract.

## Non-goals

- No Cognitive Runtime v2.
- No universal LLM framework.
- No model-specific architectural semantics.
- No mandatory LLM use.
- No automatic multi-model voting or debate subsystem.
- No authorization through prompts or model confidence.
- No exposure of private model chain-of-thought as an architectural requirement.

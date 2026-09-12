# Phase 0.98 — Model Reasoning Engineering

## Status

**Planned after 0.97 Persistent Cognitive Runtime.**

## Purpose

Define and implement the bounded engineering boundary between HAgent's deterministic runtime knowledge and model-based reasoning.

This phase does **not** create a second cognitive runtime, replace Prompt/Instruction Governance, replace Context Engineering, or replace the 0.96 Execution Planner. It establishes how HAgent requests probabilistic reasoning without asking the model to rediscover facts that HAgent already knows deterministically and without giving model output authority over runtime state or external effects.

The phase is provider-neutral and model-neutral. The model remains a replaceable reasoning component.

## Core principle

> Do not ask the model to infer deterministic facts when HAgent can provide those facts directly, unless inferring them is itself the requested task.

The complementary rule is:

> Permit inference when inference is the declared task, the required evidence is available, and the result remains inside its explicit authority boundary.

## Responsibility boundary

```text
HAgent knows / derives deterministically
        ↓
HAgent supplies authoritative evidence and relevant context
        ↓
Model performs only the required interpretation / inference / planning / generation
        ↓
HAgent validates the returned contract and evidence relationship
        ↓
Policy / authorization / runtime commits decide what may actually happen
```

The model is not responsible for re-identifying execution/runtime facts, permissions, provenance, authority, or other deterministic facts already available to HAgent.

## Scope

### Slice 1 — Reasoning responsibility and deterministic-before-inference boundary

- Define the provider-neutral semantic distinction between deterministic runtime facts and model-derived reasoning.
- Establish that source identity, execution identity, runtime state, provenance, permissions, capability state, and other deterministic facts remain HAgent-owned when already available.
- Ensure cognition may choose deterministic progress/no-model behavior before requesting probabilistic reasoning.
- Ensure reasoning requests describe the semantic problem that actually requires model judgment rather than bundling unrelated deterministic classification work.

### Slice 2 — Reasoning task decomposition and bounded model responsibility

- Establish bounded reasoning responsibilities for interpretation, inference, planning, and generation.
- Prevent one model request from silently combining source classification, policy authorization, learning promotion, and other separate authorities merely because the prompt asks for them together.
- Keep authoritative facts explicit in structured context/contracts rather than depending on prompt rediscovery.
- Preserve provider/model neutrality while allowing different models to satisfy the same semantic requirement with different capability levels.

### Slice 3 — Reasoning result contract and uncertainty handling

- Define the contract required for a reasoning result to be useful to HAgent.
- Distinguish valid structured output from semantically acceptable reasoning.
- Support bounded outcomes such as successful result, insufficient evidence, ambiguity, conflict, and unsupported inference where the task requires them.
- Treat model-reported confidence as evidence rather than authoritative truth or calibrated probability.
- Preserve provenance/evidence references needed to evaluate the result.

### Slice 4 — Validation, authority boundary, and verification

- Validate reasoning results before they influence cognitive state, learning candidates, tool requests, or external execution.
- Keep policy/authorization/approval/budget enforcement outside model output.
- Reuse existing structured-output, policy, context, observability, and evaluation infrastructure rather than creating parallel validators or metric systems.
- Add deterministic tests and public-API Examples for representative reasoning responsibilities, including deterministic facts supplied directly and genuinely inferential tasks delegated to a model.
- Measure semantic correctness, unsupported inference, over-inference, and provider/model variance where the capability is introduced.

## Explicitly out of scope

- A new cognitive architecture or Cognitive Runtime v2.
- A replacement for Phase 0.954 Prompt + Instruction Governance.
- A replacement for Phase 0.955 Context Engineering.
- A replacement for Phase 0.957 Evaluation + Quality Measurement.
- A replacement for Phase 0.96 Capability-Aware Execution.
- A new authorization or policy engine.
- A universal model-orchestration framework.
- Mandatory multi-model voting, debate, or ensemble reasoning.
- A requirement that every reasoning task use an LLM.
- A requirement to expose internal model chain-of-thought as an HAgent architectural contract.

## Ownership boundary

0.97 owns persistent cognition and decides whether the agent needs deterministic progress, bounded probabilistic deliberation, information acquisition, or another cognitive action.

0.98 owns the engineering discipline and contracts for the model-based reasoning portion once such reasoning is requested.

0.96 owns concrete execution-target selection and admission.

0.954 owns instruction authority/provenance and composition.

0.955 owns context retrieval/assembly/bounds.

0.957 owns evaluation evidence and quality measurement.

Policy, authorization, approval, budgets, tools, host state, and external side effects remain governed by their existing authorities.

## Future contract question intentionally left open

The phase does not pre-decide whether the canonical implementation should introduce a `ReasoningTask` type. That design is a separate architectural decision to be resolved before implementation of the first 0.98 slice.

## Dependency chain

```text
0.954 Prompt / Instruction Governance
        ↓
0.955 Context Engineering
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 / 0.9576 governed learning + reliability
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
        ↓
0.98 Model Reasoning Engineering
        ↓
0.10 Workspaces / Routing / Chat
```

0.98 consumes earlier contracts; it must not recreate them.

## Exit criterion

HAgent has a provider-neutral, testable boundary for model reasoning in which deterministic facts are supplied by HAgent when already known, inference is requested only where it is actually needed, model results remain non-authoritative until validated and governed, uncertainty/unsupported inference can be represented without forcing fabricated answers, and reasoning behavior can be evaluated across providers/models without creating a second cognitive or execution architecture.

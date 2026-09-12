# Phase 0.9593 — Reasoning Requirement and Boundary Foundation

## Status

**Planned after 0.9592 and before 0.96.x configuration/storage and 0.96 capability-aware execution.**

## Purpose

Establish the provider-neutral reasoning boundary that later execution and cognition phases can consume without inventing their own reasoning semantics.

This phase is intentionally a **contract and architecture foundation**, not the full model-reasoning implementation of Phase 0.98. It defines what HAgent means by a reasoning requirement, what deterministic information belongs to HAgent, what semantic work may be delegated to a reasoning capability, and how that requirement crosses into execution without naming a provider or model.

The phase exists so 0.96 and 0.97 do not independently invent incompatible reasoning request shapes, while 0.98 can later implement the complete engineering discipline around that contract.

## Core principle

> Do not ask a reasoning provider to infer deterministic facts when HAgent can provide those facts directly, unless inferring them is itself the requested task.

The complementary rule is:

> A reasoning requirement may request inference only when the semantic responsibility is explicit, the required evidence is identified, and the result remains non-authoritative until the existing validation, policy, authorization, and runtime boundaries accept it.

## What this phase defines

### 1. Reasoning responsibility boundary

Separate:

```text
HAgent-owned deterministic facts
        ↓
authoritative evidence/context
        ↓
provider-neutral reasoning requirement
        ↓
reasoning capability / execution target
        ↓
non-authoritative reasoning result
```

The requirement must not delegate responsibility for identity, permissions, provenance, runtime state, capability state, policy decisions, or other deterministic values already owned by HAgent.

### 2. Reasoning requirement semantics

Define the semantic information needed to describe a reasoning need without binding it to an LLM or provider. The contract should be able to express, as applicable:

- the reasoning responsibility/problem;
- required evidence and evidence provenance;
- deterministic facts already established by HAgent;
- allowed inference or interpretation scope;
- expected result shape/contract;
- permitted uncertainty outcomes;
- validation expectations;
- authority boundary;
- relevant execution/capability requirements without selecting a provider.

The canonical type name is deliberately not prescribed by this roadmap item. `ReasoningRequirement`, `ReasoningTask`, or another equivalent contract may be selected by the architectural decision made during the phase.

### 3. Deterministic-before-reasoning rule

Define when a reasoning requirement should not be created because deterministic cognition or an existing HAgent mechanism can complete the work directly.

Representative deterministic cases include:

- identity and execution-state lookup;
- permission/policy decisions;
- exact comparisons and arithmetic;
- known configuration/capability facts;
- authoritative resource metadata;
- deterministic operators already available to cognition.

A reasoning mechanism remains appropriate when the requested operation genuinely requires semantic interpretation, ambiguous inference, bounded planning, extraction, classification, or generation.

### 4. Execution boundary

The requirement must remain provider-neutral:

```text
Cognitive Runtime / host need
        ↓
0.9593 reasoning requirement
        ↓
0.96 execution planner
        ↓
concrete execution target
        ↓
provider adapter / model
```

0.9593 does not select models, perform routing, own provider transport, or replace the execution planner.

### 5. Result and authority boundary

Define the minimum boundary for a returned reasoning result before Phase 0.98 adds richer result validation and evaluation.

The result remains a proposal/evidence-bearing output. It cannot directly:

- grant authorization;
- bypass policy;
- publish learning;
- change persistent configuration;
- commit authoritative cognitive state;
- perform external side effects.

## Delivery slices

### Slice 1 — Contract definition

- Define the semantic vocabulary of a reasoning requirement.
- Define the deterministic-fact versus inferred-result distinction.
- Define evidence and provenance expectations.
- Define provider/model neutrality.

### Slice 2 — Integration boundaries

- Map the contract to existing `AgentExecutionRequest`, context, instruction, capability, policy, and execution-planner boundaries.
- Ensure the reasoning requirement can be consumed by 0.96 without duplicating execution-selection semantics.
- Ensure 0.97 can emit the requirement when cognition determines probabilistic reasoning is necessary.

### Slice 3 — Deterministic-before-reasoning rules

- Define representative cases that must remain deterministic.
- Define representative cases that legitimately require probabilistic reasoning.
- Define explicit fallback to deterministic mechanisms where applicable.
- Prevent provider adapters from deciding whether cognition needs reasoning.

### Slice 4 — Contract verification

Verify with deterministic/fake infrastructure that:

- deterministic facts remain HAgent-owned;
- provider-neutral requirements can be expressed without provider names;
- execution selection remains exclusively owned by 0.96;
- reasoning results remain non-authoritative;
- insufficient/ambiguous reasoning needs can be represented without forcing an answer;
- the same requirement can be mapped to different concrete execution targets without changing its semantic meaning.

## Explicitly out of scope

- Full Phase 0.98 model-reasoning engineering.
- A required `ReasoningTask` class by name.
- Model selection or provider routing.
- Provider adapters or transport implementation.
- A second cognitive runtime.
- A new prompt/context/policy framework.
- Mandatory LLM use.
- Chain-of-thought exposure.
- Multi-model voting or ensemble reasoning.
- Full reasoning quality/evaluation infrastructure beyond contract-level verification.

## Ownership

0.9593 owns the **pre-execution reasoning contract and responsibility boundary**.

0.96 owns concrete execution-target selection and admission.

0.97 owns the cognitive decision of whether/why probabilistic reasoning is needed.

0.98 owns the full engineering discipline for model reasoning, including richer result validation, uncertainty handling, semantic evaluation, and cross-provider/model quality evidence.

0.954 owns instruction authority/provenance.

0.955 owns context retrieval/assembly/bounds.

0.957 owns evaluation/quality evidence.

Policy, authorization, persistence, intervention, and external side-effect authorities remain with their existing phases.

## Dependency chain

```text
0.9592 Provider Ecosystem / Adapters
        ↓
0.9593 Reasoning Requirement / Boundary Foundation
        ↓
0.96.x Configuration / Storage / Portability
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
        ↓
0.98 Model Reasoning Engineering
```

0.9593 is therefore a prerequisite for the reasoning-related architectural portions of 0.96 and 0.97, but it does not move the full 0.98 implementation earlier in the roadmap.

## Exit criterion

HAgent has one provider-neutral, documented reasoning-boundary contract that 0.96 can execute and 0.97 can request, deterministic facts remain HAgent-owned, probabilistic reasoning is requested only for genuinely inferential responsibilities, and later Phase 0.98 work can extend the contract without replacing or duplicating the cognitive runtime or execution planner.

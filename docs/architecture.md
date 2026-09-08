# HAgent architecture

HAgent is a general-purpose, provider-neutral cognition and execution library for software that needs LLM-driven behavior. It must support different host environments without requiring HAgent.Core to understand the host's domain model.

## System model

```text
Host
  -> generic execution/context
  -> runtime agent instances
  -> HAgent
       +-- Cognitive Kernel
       +-- Cognitive Strategies
       +-- Providers
       +-- Skills / Skill Library
       +-- Knowledge / Wiki / Retrieval
       +-- Memory
       +-- Learning
       +-- Tools
       +-- Workspaces
       +-- execution/telemetry
  -> host-owned authorization, scheduling, state, persistence, side effects
```

## Responsibility boundary

`HAgent.Core` owns provider-neutral agent profiles, runtime instances, persistent cognitive runtime contracts, cognitive strategies, execution, context, skills, knowledge/memory abstractions, learning contracts, tools, workspaces/coordination primitives, structured-output contracts, capability-policy evaluation, and execution telemetry.

Provider assemblies own transport and provider-specific behavior. Storage assemblies own persistence. Optional integration assemblies own platform-specific adapters. Host applications own domain objects, authoritative state, scheduling policy, host-state persistence, authorization rules, and side effects.

## Core concepts

### Agent Profile

Reusable persistent configuration: provider/model preferences, system prompt, generation settings, capability references, and learning/memory policy defaults.

### Runtime Agent Instance

One live agent identity created from a profile. It has its own runtime ID, scope, runtime overrides, memory ownership, and execution lifecycle. Many runtime instances may come from one profile.

### Persistent Cognitive Runtime

The long-lived cognitive layer above individual executions. It maintains provider-neutral cognitive state for a runtime instance, consumes environment events, manages beliefs, attention, goals, intentions, plans, memory, experiences, and learning, and decides whether deterministic cognition is sufficient or probabilistic reasoning is warranted.

### Cognitive Kernel

The stable runtime substrate for cognitive state, lifecycle, event activation, versioning/revision, history, governance, and persistence boundaries. It must not assume that one cognitive architecture is universally correct.

### Cognitive Strategy

A replaceable implementation of how cognitive state is interpreted and cognitive actions are selected. The first reference strategy is **Adaptive Hybrid Cognition (AHC)**. Future research-derived strategies may be added, independently evaluated, versioned, and replaced without redesigning the cognitive kernel.

### Reasoning Requirement

A provider-neutral description of the reasoning capability currently needed by cognition. It separates the cognitive decision about *what kind of reasoning is needed* from the Execution Planner decision about *where/how that reasoning executes*.

### Agent Profile

Reusable persistent configuration: provider/model preferences, system prompt, generation settings, capability references, and learning/memory policy defaults.

### Execution Request

The generic host-to-HAgent boundary carrying host-supplied input/context, host correlation metadata, execution options, and optional structured-output requirements. Plain string messages are a convenience form.

### Skill

A reusable executable capability/procedure with stable identity and versioning. Skills are shared resources referenced by agent profiles rather than copied into runtime instances. Persisted definitions contain contracts/metadata; executable handlers remain runtime registrations and are never serialized.

### Knowledge / Wiki

Knowledge is reusable retrievable information. A Wiki is a managed persistent knowledge source within the broader knowledge system. Knowledge resources have identity, scope, provenance, lifecycle/status, metadata, versioning, and relationships where applicable. Retrieval implementation is separate from the logical knowledge contract.

### Memory

Memory is scoped experience or runtime state. Working memory is execution-specific. Episodic, semantic, procedural, and future memory families may be owned by an execution, runtime instance, logical agent, user, tenant, or another explicit scope. Shared storage does not remove logical ownership or authorization.

### Learning

Learning analyzes execution experience and creates typed candidates for memory, knowledge, or skill improvement. It may also produce governed cognitive improvement candidates or partial proceduralization. It is not model-weight training. Candidates are subject to provenance, validation, authorization, evaluation, and learning policy before promotion.

### Capability Policy

Capability policy determines which skills, knowledge resources, memory families, cognitive capabilities, and future resource types are effectively enabled. Profile configuration supplies defaults; runtime overrides are tri-state (`Inherit`, `Enabled`, `Disabled`) and apply to execution snapshots without mutating the persistent profile.

## Cognitive model

```text
environment / host observations
          -> events
          -> belief interpretation + revision
          -> attention / global workspace
          -> goals
          -> intentions
          -> plans / operators
          -> deterministic action
                 or
          -> impasse
          -> deliberation through selected cognitive strategy
          -> reasoning requirement
          -> execution planner
          -> provider/model
          -> outcome
          -> experience / memory / learning
          -> belief + plan revision
```

The LLM is a replaceable reasoning component inside the cognitive runtime, not the cognitive runtime itself. A strategy may choose no LLM when deterministic cognition is sufficient.

## Execution flow

```text
host execution request
        -> runtime agent instance
        -> effective capability policy
        -> execution snapshot
        -> retrieve enabled knowledge/memory and bind enabled skills
        -> provider/model execution
        -> normalized response / structured output / tool request
        -> trusted tool handling / host result handling
        -> memory/experience capture according to policy
        -> optional learning candidate generation
        -> caller
```

Execution is asynchronous, cancellable, bounded, correlated, and protected against conflicting late completion.

## Learning flow

```text
execution
   -> observations / outcomes / events
   -> learning engine
   -> MemoryCandidate / KnowledgeCandidate / SkillCandidate
   -> optional PolicyCandidate / CognitiveImprovementCandidate
   -> validation + provenance + evaluation + policy
   -> review or automatic promotion
   -> scoped memory / knowledge / skill / governed strategy improvement
```

Learning modes are `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, and `FullyAutomatic`. `SuggestOnly` is the recommended governance mode; fully automatic promotion is never implied by enabling learning.

## Runtime and capability isolation

Runtime instances isolate mutable runtime state, runtime overrides, execution state, shutdown signaling, and private memory ownership. They may share stores and provider/tool infrastructure only through concurrently safe contracts.

A runtime inherits its profile's capability policy and may override individual capabilities/resources without mutating the profile. Every execution receives an immutable effective snapshot so later configuration/runtime changes cannot corrupt running work.

## Future-proof agent knowledge view

HAgent exposes a generic capability/resource inventory with resource ID, type ID, display metadata, scope, effective enabled state, provenance/source metadata, and relationships/dependencies where applicable. Known resource types can receive specialized UI views, while unknown/future types remain visible through the generic inventory.

The selected-agent management view should therefore expose the agent's effective Skills, Knowledge/Wiki access, Memory families, cognitive strategy, and any other future resource types without requiring a new hard-coded agent model for every new type.

## Research Foundations

HAgent's cognitive-runtime design is informed by established cognitive-architecture and modern language-agent research. The research mapping, adaptation decisions, and recommended architectural changes are documented in:

- `docs/architecture/15-research-foundations.md` — research lineage and direct mapping of HAgent concepts to BDI, SOAR, ACT-R, CoALA, Generative Agents, Global Workspace/LIDA, ReAct, Reflexion, MemGPT, Voyager, and recent persistent-agent research.
- `docs/architecture/16-cognitive-runtime.md` — stable HAgent cognitive-kernel, strategy, reasoning-requirement, belief, planning, learning, and live-workbench architecture.
- `docs/research/2026-09-persistent-cognitive-runtime-comparison.md` — comprehensive comparison and recommended evolution of the HAgent Persistent Cognitive Runtime.

These documents are architectural guidance, not claims that HAgent invented the underlying cognitive concepts. They are intended to prevent accidental reinvention, make research-derived decisions explicit, and identify the parts of the architecture that should remain HAgent-specific.

## Live cognition workbench

`HAgent.WinForms` provides the intended operator-facing runtime view for active cognition. Authorized users should be able to inspect the current strategy/version, beliefs, goals, intentions, attention, global workspace, plans and current step, memory, knowledge, skills, experiences, events, executions, reasoning decisions, learning candidates, and full cognitive history. Live intervention must occur through runtime APIs with authorization, revision checks, provenance, auditability, atomicity, and stale-result protection.

## Architecture references

- `docs/architecture/05-identity.md` — identity, tenancy, ownership, and user context.
- `docs/architecture/06-events.md` — first-class provider-neutral event infrastructure.
- `docs/architecture/07-execution-planning.md` — capability-aware execution planning.
- `docs/architecture/10-runtime.md` — runtime agents and execution.
- `docs/architecture/15-research-foundations.md` — research foundations and cognitive-architecture mapping.
- `docs/architecture/16-cognitive-runtime.md` — persistent cognitive runtime and extensible cognition.
- `docs/architecture/20-context.md` — bounded host context.
- `docs/architecture/30-tools.md` — structured tools.
- `docs/architecture/40-security.md` — authorization and guardrails.
- `docs/architecture/50-workspaces.md` — workspace communication.
- `docs/architecture/70-external-host-integration.md` — generic host integration.
- `docs/architecture/80-knowledge-memory-learning.md` — detailed knowledge, skills, memory, learning, capability policy, and management architecture.
- `docs/research/2026-09-persistent-cognitive-runtime-comparison.md` — detailed research comparison and recommended HAgent changes.
- `docs/storage.md` — persistence and storage boundaries.

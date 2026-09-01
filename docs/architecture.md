# HAgent architecture

HAgent is a general-purpose, provider-neutral cognition and execution library for software that needs LLM-driven behavior. It must support different host environments without requiring HAgent.Core to understand the host's domain model.

## System model

```text
Host
  -> generic execution/context
  -> runtime agent instances
  -> HAgent
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

`HAgent.Core` owns provider-neutral agent profiles, runtime instances, execution, context, skills, knowledge/memory abstractions, learning contracts, tools, workspaces/coordination primitives, structured-output contracts, capability-policy evaluation, and execution telemetry.

Provider assemblies own transport and provider-specific behavior. Storage assemblies own persistence. Optional integration assemblies own platform-specific adapters. Host applications own domain objects, authoritative state, scheduling policy, host-state persistence, authorization rules, and side effects.

## Core concepts

### Agent Profile

Reusable persistent configuration: provider/model preferences, system prompt, generation settings, capability references, and learning/memory policy defaults.

### Runtime Agent Instance

One live agent identity created from a profile. It has its own runtime ID, scope, runtime overrides, memory ownership, and execution lifecycle. Many runtime instances may come from one profile.

### Execution Request

The generic host-to-HAgent boundary carrying host-supplied input/context, host correlation metadata, execution options, and optional structured-output requirements. Plain string messages are a convenience form.

### Skill

A reusable executable capability/procedure with stable identity and versioning. Skills are shared resources referenced by agent profiles rather than copied into runtime instances. Persisted definitions contain contracts/metadata; executable handlers remain runtime registrations and are never serialized.

### Knowledge / Wiki

Knowledge is reusable retrievable information. A Wiki is a managed persistent knowledge source within the broader knowledge system. Knowledge resources have identity, scope, provenance, lifecycle/status, metadata, versioning, and relationships where applicable. Retrieval implementation is separate from the logical knowledge contract.

### Memory

Memory is scoped experience or runtime state. Working memory is execution-specific. Episodic, semantic, procedural, and future memory families may be owned by an execution, runtime instance, logical agent, user, tenant, or another explicit scope. Shared storage does not remove logical ownership or authorization.

### Learning

Learning analyzes execution experience and creates typed candidates for memory, knowledge, or skill improvement. It is not model-weight training. Candidates are subject to provenance, validation, authorization, and learning policy before promotion.

### Capability Policy

Capability policy determines which skills, knowledge resources, memory families, and future resource types are effectively enabled. Profile configuration supplies defaults; runtime overrides are tri-state (`Inherit`, `Enabled`, `Disabled`) and apply to execution snapshots without mutating the persistent profile.

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
   -> validation + provenance + policy
   -> review or automatic promotion
   -> scoped memory / knowledge source / new skill version
```

Learning modes are `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, and `FullyAutomatic`. `SuggestOnly` is the recommended governance mode; fully automatic promotion is never implied by enabling learning.

## Runtime and capability isolation

Runtime instances isolate mutable runtime state, runtime overrides, execution state, shutdown signaling, and private memory ownership. They may share stores and provider/tool infrastructure only through concurrently safe contracts.

A runtime inherits its profile's capability policy and may override individual capabilities/resources without mutating the profile. Every execution receives an immutable effective snapshot so later configuration/runtime changes cannot corrupt running work.

## Future-proof agent knowledge view

HAgent exposes a generic capability/resource inventory with resource ID, type ID, display metadata, scope, effective enabled state, provenance/source metadata, and relationships/dependencies where applicable. Known resource types can receive specialized UI views, while unknown/future types remain visible through the generic inventory.

The selected-agent management view should therefore expose the agent's effective Skills, Knowledge/Wiki access, Memory families, and any other future resource types without requiring a new hard-coded agent model for every new type.

## Architecture references

- `docs/architecture/10-runtime.md` — runtime agents and execution.
- `docs/architecture/20-context.md` — bounded host context.
- `docs/architecture/30-tools.md` — structured tools.
- `docs/architecture/40-security.md` — authorization and guardrails.
- `docs/architecture/50-workspaces.md` — workspace communication.
- `docs/architecture/70-external-host-integration.md` — generic host integration.
- `docs/architecture/80-knowledge-memory-learning.md` — detailed knowledge, skills, memory, learning, capability policy, and management architecture.
- `docs/storage.md` — persistence and storage boundaries.

# Skills

## Purpose

Skills are reusable provider-neutral capability definitions. A Skill describes what a reusable procedure requires and produces; it does not contain an executable handler, provider SDK object, delegate, or host callback.

## Canonical model

A `AiSkillDefinition` is one version of a reusable Skill. Its canonical identity is `Id + Version`; scope and owner use the existing `AgentResourceScope` and ownership model. Lifecycle is explicit: `Draft`, `Published`, or `Archived`. Only a published definition is authoritative for execution binding.

A Skill definition contains bounded, cloneable contracts for:

- inputs and outputs;
- preconditions;
- ordered procedure steps;
- required Knowledge and Tool dependencies, with optional version pins;
- constraints, metadata, provenance, and relationships.

Definitions are descriptive data. Executable handlers remain a separate runtime concern and must never be serialized into the definition or skill set.

## References and skill sets

`AiSkillReference` identifies a reusable definition by Skill ID, optional version, explicit scope, and explicit owner. An unpinned reference may resolve the currently authoritative version; once resolved, the concrete definition version is captured in the execution snapshot. `AiSkillSet` is a bounded collection of reusable references and may be shared by multiple agents without copying the underlying definition.

Agent profiles expose one canonical `AiSkillSet` through `AiAgent.Skills`. The profile reference is configuration, not an authorization grant.

## Governance

Skill access uses the existing `AiResourceGovernanceEvaluator`; there is no Skill-specific authorization engine. Resolution evaluates operation `skill.invoke`, resource type `skill`, the referenced Skill ID, its explicit scope/owner, the profile/runtime/execution identities, and the effective capability snapshot before reading the definition source.

Resource capability, HAgent policy, and host authorization remain distinct boundaries. A denied, deferred, approval-required, or disabled Skill does not reach the definition source. Required references fail the resolution operation when their admission or resolution cannot be satisfied; optional references may be omitted.

## Definition source boundary

`IAiSkillDefinitionSource` is the provider/storage-neutral read boundary for versioned definitions. The source returns definitions only; it does not own authorization. `AiGovernedSkillResolver` composes source resolution with the canonical governance evaluator and validates the returned definition against the requested reference before producing an execution binding.

No File/SQL/MySQL Skill repository is introduced in this slice. Persistence of Skill versions and relationships remains a later storage concern and must serialize only the provider-neutral definition/reference data.

## Execution snapshot

`AiSkillExecutionSnapshot` owns deep clones of concrete `AiSkillBinding` values. `AgentExecutionSnapshot.Skills` captures that execution-owned snapshot. Consequently, a profile edit, source mutation, later Skill version, or caller-owned object mutation cannot alter the Skill definition or version already bound to an active execution.

A Skill binding must have matching ID, scope, owner, and pinned version when supplied, and only published definitions can enter an execution snapshot.

## Lifecycle and learning boundary

Skill lifecycle is descriptive state only. This slice does not implement Skill promotion or version creation from `AiLearningCandidate`; the existing learning boundary remains authoritative and later work will connect candidate validation/evaluation to version admission. A model-produced candidate cannot directly replace an authoritative Skill definition.

## Non-goals for Slice 3

This slice does not implement Skill execution handlers, tool/knowledge invocation orchestration, Skill persistence backends, management UI, learning promotion, or context-budget integration. Those remain later roadmap work.

## Compatibility

Contracts remain under `HAgent.Core`, use only provider-neutral types, are asynchronous at source boundaries, honor cancellation, and avoid Framework-specific APIs so they remain compatible with .NET Framework 4.8.1 and .NET 9 targets supported by the core project.

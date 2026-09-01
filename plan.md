# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.95 Generic External Host Integration — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage foundations are substantially implemented and manually verified across supported storage backends; remaining internal repository parity is intentionally deferred.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.10 Workspaces, Routing + Chat has an implemented and locally verified routing foundation, but workspace execution/chat remains deferred until the 0.95 host boundary is stable.

0.95 Generic External Host Integration is the current active hardening milestone.

The next major capability layer after the generic host boundary is **Knowledge + Skills + Memory Governance + Learning**, including management UI and profile/runtime capability controls.

## Verified implementation

The repository currently contains verified foundations for:

- provider/agent configuration and routing;
- execution lifecycle, timeout, cancellation, retries, diagnostics, and failure reporting;
- memory, persistent sessions, context budgeting, automatic/episodic/task memory;
- capability discovery and response normalization;
- streaming contracts and live streaming;
- tool definitions, registry, schema validation, provider transport, bounded tool loops, persistence, and per-agent assignment;
- WinForms UI Context with Form/UserControl attachment;
- semantic control and bound/native data-source discovery;
- CurrencyManager/current-item/source relationships;
- control-to-source relationship discovery;
- convention-based custom control adaptation;
- bounded application-object discovery;
- provider-neutral structured data projection/query contracts;
- HAgent-owned storage configuration for File, SQL Server, and MySQL backends;
- application-specific File storage layout;
- HAgent-owned SQL Server/MySQL database bootstrap foundations;
- bounded internal inventory, memory, conversation, and execution-audit read tools;
- automatic payload-free execution auditing with configurable bounded retention;
- runtime-instance identity, scope, runtime-only overrides, independent memory ownership, concurrent execution, stale-result protection, host-controlled scheduling, shutdown semantics, and optional runtime-state persistence;
- provider-neutral workspace participants, message metadata, and default-recipient routing;
- canonical generic host execution requests with multiple messages, host correlation identity, and bounded host context (pending local verification).

## Planned Knowledge / Skills / Learning layer

The next capability layer must establish one coherent model rather than treating Wiki, Skills, and Memory as unrelated features:

```text
Skills    = reusable executable capabilities
Knowledge = reusable retrievable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = controlled transformation of experience into typed candidates
```

Learning candidates are typed as `MemoryCandidate`, `KnowledgeCandidate`, or `SkillCandidate` and retain provenance/evidence. Candidate creation does not automatically make information authoritative.

`LearningMode` is configurable as:

- `Disabled` — no learning candidates;
- `SuggestOnly` — candidates are reviewable but not promoted automatically;
- `AutomaticWithPolicy` — explicit policy may promote approved candidate classes/scopes;
- `FullyAutomatic` — explicit advanced opt-in for automatic promotion.

The default governance target is `SuggestOnly` unless the existing configuration model establishes a safer default during implementation.

## Capability configuration target

Agent profiles provide reusable defaults. A runtime instance inherits those defaults and can apply runtime-only tri-state overrides (`Inherit`, `Enabled`, `Disabled`). The policy must support at least:

- skill capability and individual skill resources;
- Wiki/knowledge capability and individual knowledge resources;
- memory capability and individual memory families/types;
- future resource types through a generic resource/type ID model.

Execution snapshots capture the effective policy so configuration or runtime changes cannot alter already-running work.

## Management UI target

`HAgent.WinForms` must add:

- Learning Review: pending suggestions, provenance/evidence, source execution/runtime, proposed scope, approve/reject;
- Wiki/Knowledge Manager: new/edit/delete, search/filter, relationships, and which agents use/access each resource;
- Skill Manager: new/edit/delete, version/status, relationships, and which agents use each skill;
- Agent Configuration knowledge view: when an agent is selected, show effective Skills, Wiki/Knowledge access, Memory families, and all other resource types exposed by the generic inventory;
- profile-level enable/disable and runtime-instance-level overrides for skills, Wiki/knowledge, and individual memory types.

The agent knowledge view must be future-proof: known types may have specialized UI, but unknown/new types remain visible through the generic resource inventory rather than requiring a new hard-coded agent property.

## Boundaries

Knowledge, skills, memory, learning, and their management UI remain HAgent-owned generic infrastructure. No host-specific application or HWorld type may be introduced into Core.

Storage implementations remain responsible for persistence. WinForms owns administration surfaces. The model is never the authorization authority; learning promotion and capability enforcement occur through code/policy.

## Planned generic integration hardening

Phase 0.95 completes the generic host boundary required for a broad class of LLM-driven software:

- arbitrary bounded host execution input/context;
- host-supplied correlation identity;
- host-defined structured-output contracts and validation;
- race-safe terminal execution semantics against late provider completion;
- runtime/execution identity propagation into tool execution;
- stronger isolation of mutable runtime overrides;
- deterministic verification of concurrent independent runtime instances.

These changes remain provider-neutral and domain-neutral. Host state, lifecycle, scheduling policy, persistence, authorization, and side effects remain host-owned.

## Active implementation

The active implementation plan is `docs/plan/20-active.md`. It contains only work being implemented now. Knowledge/Skills/Learning remains planned until the generic host boundary and subsequent capability-governance work are ready.

## Verification rule

A capability becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the project documentation reflects the result.

Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — non-negotiable engineering and repository rules.
- `docs/architecture/` — stable architectural design and boundaries.
- `docs/plan/` — master direction, current state, and active implementation only.
- `docs/roadmap/` — ordered path from completed foundations to the long-term target.
- `docs/storage.md` — persistence/backend details.

The root `plan.md` and `roadmap.md` are generated from their source directories. They are views, not independent sources of truth.

## HAgent Master Plan

## Purpose

HAgent is a general-purpose, provider-neutral cognition and execution library that makes connecting software to LLMs practical. Its goal is to provide reusable infrastructure for any software project that needs LLM-driven behavior without importing host-specific domain models into HAgent.Core.

A host may be a conversational program, business software, service, game, simulation, automation system, developer tool, or another environment.

## End-state goal

A host should be able to add HAgent and choose how much intelligence it wants to expose. HAgent should provide generic infrastructure for model invocation, context, tools, memory, reusable skills, knowledge/Wiki, controlled learning, structured output, multi-agent coordination, and asynchronous execution.

The host remains authoritative over real domain state, lifecycle, scheduling, host persistence, authorization, and side effects.

## Core model

```text
Provider profile
    -> connection/model configuration

Agent profile
    -> reusable behavior + capability policy defaults

Runtime agent instance
    -> one live agent identity created from a profile
    -> runtime-only capability/memory overrides

Execution request
    -> host input/context + correlation + execution requirements

Skills
    -> reusable executable capabilities/procedures

Knowledge
    -> reusable retrievable information
    -> Wiki is a managed persistent knowledge source

Memory
    -> scoped experience/state
    -> working / episodic / semantic / procedural / future families

Learning
    -> execution experience -> typed candidates -> policy -> promotion

Execution
    -> bounded asynchronous model/tool work with lifecycle and correlation
```

The distinction between persistent profiles and runtime instances remains fundamental. One profile can produce many independent runtime instances. Shared resources are referenced; private runtime state is not copied across instances.

## Knowledge, Skills, Memory, and Learning

HAgent must keep the following distinctions explicit:

```text
Skill     = reusable executable capability/procedure
Knowledge = reusable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = controlled transformation of experience into candidates
```

Knowledge and Skills are reusable resources with explicit scope and authorization rather than private copies owned by every agent. Memory has explicit ownership/scope and may be private to a runtime instance, shared at logical-agent/user/tenant level, or execution-local.

Learning is not model-weight training. It may use deterministic code, LLM reasoning, or both. Code controls candidate typing, provenance, policy, authorization, retention, and promotion.

### Learning modes

```text
Disabled
SuggestOnly
AutomaticWithPolicy
FullyAutomatic
```

`SuggestOnly` is the recommended governance mode. `AutomaticWithPolicy` permits promotion only under explicit policy. `FullyAutomatic` is an explicit advanced opt-in and never follows merely from enabling learning.

Learning candidates are typed (`MemoryCandidate`, `KnowledgeCandidate`, `SkillCandidate`) and preserve source execution/runtime identity, proposed scope, provenance, and evidence/confidence where available.

### Promotion rules

LLM output must never write authoritative Wiki/knowledge or mutate a published Skill directly merely because it was generated. Normal promotion is:

```text
experience
  -> candidate
  -> validation / policy / authorization
  -> memory, managed knowledge, or new skill version
```

Skill improvements produce new versions; already-running executions use their immutable skill/configuration snapshots.

## Capability policy

Agent profiles establish reusable capability defaults. Runtime instances inherit them and can override them without mutating the profile.

The effective state for each capability/resource is tri-state:

```text
Inherit
Enabled
Disabled
```

The policy must support at least:

- skills and individual skill resources;
- Wiki/knowledge and individual knowledge resources;
- memory and individual memory families/types;
- future resource types by stable type/resource identifiers.

Capability enforcement occurs before retrieval or invocation. Prompt instructions are not authorization.

## Runtime and memory target

A host may keep a runtime instance alive and execute against it repeatedly for an arbitrary lifetime. Private runtime memory must remain independent across runtime instances created from the same profile.

Working memory is execution-local. Long-term memory ownership is explicit and may be runtime-, agent-, user-, tenant-, or another host-approved scope. The physical store may be shared when its contract is concurrency-safe.

Effective profile/runtime capability policy and memory access are captured in execution snapshots so configuration changes cannot alter already-running work.

## Generic execution request

The canonical execution boundary accepts generic host input/context, host correlation identity, execution options, and optional structured-output requirements. Plain strings remain convenience APIs.

## System-prompt model

System prompts are additive layers. Lower layers can add narrower instructions/restrictions but cannot erase higher layers. Prompt layering is behavioral composition, not a security boundary.

## Context target

Context remains bounded, generic, and host-supplied. HAgent may normalize/project/serialize host context but does not assign domain meaning.

## Structured output target

A host may define its own schema. HAgent carries it through provider invocation, validates the returned structure, and exposes validation metadata. Valid JSON alone does not prove schema compliance.

## Tool target

Tool definitions describe what may be requested; trusted runtime handlers define what executes. Handler delegates are never serialized. Tool execution preserves execution/runtime/host correlation for authorization and telemetry.

## Learning and management UI target

`HAgent.WinForms` must provide:

```text
Learning Review
    pending candidates
    inspect provenance/evidence/source
    approve / reject

Wiki / Knowledge Manager
    new / edit / delete
    search/filter
    relationships
    which agents use/access it

Skill Manager
    new / edit / delete
    version/status
    relationships
    which agents use it

Agent Configuration
    selected agent -> effective skills
                     -> knowledge/wiki access
                     -> memory families
                     -> any future resource types
    profile enable/disable
    runtime-instance overrides
```

The agent knowledge overview is based on a generic resource inventory. Known types may have specialized panels, but unknown/new types remain visible without adding a new hard-coded Agent property.

## Generic external-host requirement

HAgent must be capable of serving as the generic LLM cognition/execution layer for different project types. Host state, lifecycle, scheduling, persistence, authorization, and side effects remain host-owned.

## Security target

No model instruction is an authorization boundary. Retrieval, memory recall, skill use, learning, and learning promotion are independently enforceable policy boundaries where meaningful.

## Development principles

- Keep Core provider-neutral and dependency-light.
- Preserve .NET Framework 4.8.1 compatibility where targeted and support .NET 9 where supported.
- Design for low RAM and no GPU assumption.
- Keep runtime work cancellable, bounded, correlated, concurrent, and safe against stale results.
- Keep persistent configuration separate from live runtime state.
- Use generic contracts for future extensibility rather than hard-coded host concepts.
- Verify completed capabilities through `HAgent.Example` before marking them complete.
- Keep authoritative documentation synchronized with implementation.

## What success looks like

A developer can start with:

```csharp
await ai.SendAsync("assistant", "Hello");
```

and later grow the integration into:

```text
host
  -> generic execution/context requests
  -> multiple runtime agent instances
  -> private/shared memory
  -> reusable skills
  -> scoped Wiki/knowledge
  -> controlled learning
  -> authorized tools
  -> structured model output
  -> workspace routing
  -> asynchronous background work
```

without replacing HAgent or introducing application-specific types into `HAgent.Core`.

## Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.95 Generic External Host Integration

### Objective
Complete the generic host boundary required for a broad class of LLM-driven software. HAgent must accept host-owned execution input/context and correlation identity, preserve them in immutable execution state, support structured-output contracts with real validation, and remain independent of any host domain, UI, scheduler, persistence model, or side-effect system.

### Current slices

- [x] Reusable runtime-agent profiles and live runtime instances remain separate and verified through the 0.9 runtime foundation.
- [x] Provider-neutral workspace/routing foundation exists for later host coordination; workspace UI/execution remains deferred to Phase 0.10.
- [x] Canonical `AgentExecutionRequest` supports multiple messages, host correlation identity, bounded host context, runtime overrides, and normal execution options.
- [x] Preserve host correlation identity through relevant tool execution/correlation metadata.
- [x] Define host-owned structured-output request and validation contract.
- [x] Validate structured output independently of provider claims.
- [x] Strengthen terminal-state protection against late provider completion after cancellation, timeout, retirement, shutdown, or another terminal outcome. Implementation and deterministic Example verification are complete.
- [x] Verify execution snapshots fully isolate mutable runtime overrides and host context.
- [x] Add deterministic Example verification for the generic host execution request.
- [x] Add deterministic Example verification for structured-output schema validation.
- [x] Define a distinct provider-facing `ProviderExecutionRequest` boundary.
- [x] Route normal, tool-calling, and streaming provider interfaces through `ProviderExecutionRequest`.
- [x] Migrate in-repository Example and Core test adapters to the `ProviderExecutionRequest` contract.
- [x] Add deterministic Example verification that provider-facing requests preserve host-owned structured-output requirements.
- [x] Use provider-facing structured-output requirements for provider-native constrained generation where supported. Implementation and deterministic native/fallback transport verification are complete.
- [x] Add a standalone external-consumer smoke sample that references only `HAgent.Core` and verifies canonical execution plus concurrent runtime consumption. Local execution verification pending.
- [ ] Run the full 0.95 verification pass and close the phase only after all cross-cutting slices pass.

### Generic host boundary

`AgentExecutionRequest` is the canonical host-facing provider-neutral execution request. It identifies the target reusable agent profile, carries an ordered message list, accepts a host-supplied correlation identity, carries a bounded host context dictionary, optional structured-output requirements, and reuses `AgentExecutionOptions` for execution policies and runtime overrides.

Host correlation is distinct from HAgent execution correlation and runtime-instance identity. It is never embedded into prompt text and is retained through tool execution metadata as a separate identity.

Host context is copied into the immutable execution snapshot. HAgent does not assign domain meaning to the context and does not use it as an authorization mechanism.

### Provider boundary

`ProviderExecutionRequest` is the provider-facing contract. It contains the resolved provider, execution agent snapshot, secret material needed only for the adapter call, composed system prompt, bounded messages, optional structured-output requirement, optional tools, and optional streaming progress sink.

This boundary is intentionally separate from `AgentExecutionRequest`. Host callers do not need to know provider transport details, while provider adapters can evolve their supported capabilities without expanding the host-facing model.

The normal, tool-calling, and streaming provider adapter contracts consume `ProviderExecutionRequest`. All in-repository adapter implementations and deterministic test doubles now use the request-object contract. The current OpenAI-compatible implementation retains legacy overloads only as internal compatibility helpers; the adapter interfaces themselves use the request object.

Provider adapters must normalize responses into `AIResponse`, and HAgent remains responsible for provider-neutral validation of host-owned structured-output contracts after normalization. The OpenAI-compatible implementation also attempts native `response_format`/JSON Schema transport when requested; explicit unsupported-feature responses fall back to the ordinary request shape, while HAgent continues to validate the normalized result.

### Runtime terminal outcome

`AgentExecution` owns the first-terminal-outcome-wins gate. HAgent may finish caller-visible cancellation or timeout before a non-cooperative provider task returns. Such a late provider task cannot replace the terminal execution state or response, and provider faults from detached late work are observed so they do not surface as unobserved task failures. This behavior is verified by the `RUNTIME TERMINAL STATE` Example test.

### External consumer boundary

A standalone sample under `samples/HAgent.ExternalConsumer` references only `HAgent.Core` and implements its own in-memory host store, secret store, and provider adapter. It exercises the public canonical request and runtime-instance APIs without HAgent WinForms, HAgent storage-provider, or HWorld dependencies. The sample's local execution is the remaining verification required for the external-consumer slice.

### HWorld boundary

HWorld is an external consumer, not an HAgent dependency. HWorld should reference HAgent normally and use the public canonical request/runtime APIs. HAgent must not add HWorld-specific adapters, world types, physics, simulation scheduling, or action authority.

### Next phases

Workspace execution/chat belongs to Phase 0.10. Knowledge, Skills, Wiki/content management, and Learning governance belong to the dedicated Phase 0.11 and are no longer treated as part of the 0.95 implementation milestone.

## Verification rule

A 0.95 slice becomes complete only after its implementation exists, its matching Example or external-consumer verification passes locally, and the authoritative documentation reflects the result.

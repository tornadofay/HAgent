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

**0.8 Data Access + Authorization — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.9 Runtime Agent Instances foundations are complete and documented.

0.10 Workspaces, Routing + Chat is the current active feature milestone in `docs/plan/20-active.md`.

0.95 Generic External Host Integration remains a planned cross-cutting hardening phase.

The next major capability layer after the current workspace work is **Knowledge + Skills + Memory Governance + Learning**, including management UI and profile/runtime capability controls.

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
- runtime-instance foundations including independent runtime identity, lifecycle, memory ownership, execution revisions, scheduling, and optional runtime-state persistence;
- HAgent.Example verification for completed capabilities.

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

Phase 0.95 will complete the generic host boundary required for a broad class of LLM-driven software:

- arbitrary bounded host execution input/context;
- host-supplied correlation identity;
- host-defined structured-output contracts and validation;
- race-safe terminal execution semantics against late provider completion;
- runtime/execution identity propagation into tool execution;
- stronger isolation of mutable runtime overrides;
- deterministic verification of concurrent independent runtime instances.

These changes must remain provider-neutral and domain-neutral. Host state, lifecycle, scheduling policy, persistence, authorization, and side effects remain host-owned.

## Active implementation

The active implementation plan is the current file `docs/plan/20-active.md`. It must contain only work being implemented now; the Knowledge/Skills/Learning layer is planned until the workspace milestone changes the active slice.

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

## 0.10 Workspaces, Routing + Chat

### Objective
Introduce a provider-neutral workspace and routing foundation so a host can place users and runtime-agent participants in one bounded collaboration context without coupling Core to WinForms, a provider, HWorld, or a host application's business database.

### Current slices

- [x] Reusable runtime-agent profiles and live runtime instances remain separate and verified through the 0.9 runtime foundation.
- [x] Provider-neutral workspace participant model with explicit participant lifecycle state.
- [x] Provider-neutral workspace message metadata with sender, recipient, correlation, causation, sequence, and timestamp.
- [x] Provider-neutral routing contract and implementation enforcing default-recipient and explicit-recipient rules.
- [ ] Deterministic Example verification for workspace routing.
- [ ] Direct workspace routing into runtime execution.
- [ ] Addressed agent-to-agent delegation lifecycle.
- [ ] Workspace message history/persistence policy.
- [ ] Collaboration budgets and loop protection.
- [ ] WinForms chat surface.

### Routing rules

An unaddressed user message routes only to the workspace's active default agent participant. Explicit user-to-agent messages name an active agent participant. Agent-to-agent delegation also names an active agent participant. Broadcast is not part of the base routing operation and must be an explicit later policy.

Routing is separate from execution. `IWorkspaceRouter` creates an authoritative routing result but does not invoke providers, mutate agent profiles, perform host side effects, or bypass permissions.

### Participant identity

A workspace participant has its own participant ID. An agent participant may reference both a persistent profile ID and a live runtime instance ID. Workspace membership does not mutate the persistent `AiAgent` profile or take ownership of the runtime instance lifecycle.

### Message identity

`WorkspaceMessage` preserves workspace ID, message ID, sender, recipient, message kind, correlation ID, causation ID, monotonic sequence, content, and creation time. Correlation identifies the broader operation; causation links a message to the message or event that caused it.

### HWorld boundary

HWorld is not a dependency of this milestone. HWorld can reference HAgent normally and use the public runtime/workspace APIs. HAgent contains no HWorld-specific adapter, world type, physics, simulation scheduling, or action authority.

### Deferred work

Skills, Wiki/content integration, and any remaining 0.8 internal-repository parity remain explicitly deferred and must not be implemented as part of this workspace slice until the runtime/workspace contracts are stable.

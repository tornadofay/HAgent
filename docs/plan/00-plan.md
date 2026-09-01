# HAgent Master Plan

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

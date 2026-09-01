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

The next cross-cutting runtime milestone is **0.95 Generic External Host Integration**. It is a planned architectural hardening phase, not a host-specific integration.

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
- `HAgent.Example` verification for completed capabilities.

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

The active implementation plan is the current file `docs/plan/20-active.md`. It must contain only the work being implemented now.

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

HAgent is a general-purpose, provider-neutral cognition and execution library that makes connecting software to LLMs practical. Its goal is not to serve one application category; it must provide the reusable infrastructure required by any software project that needs LLM-driven behavior.

A host may be a simple conversational program, business software, a service, a game, a simulation, an automation system, a developer tool, or another environment. HAgent supports these through generic capabilities rather than application-specific domain classes.

HAgent must remain useful at both ends of the range:

```text
simple host
    -> one provider
    -> one agent
    -> one execution

advanced host
    -> many runtime agents
    -> independent memories and contexts
    -> tools and host capabilities
    -> shared workspaces and routing
    -> structured input/output
    -> concurrent execution
    -> persistent or ephemeral runtime state
```

## End-state goal

A host should be able to add HAgent and choose how much intelligence it wants to expose. HAgent should provide the generic infrastructure required for LLM-driven software to provide context, invoke models, maintain memory, use authorized tools, produce structured outputs, coordinate agents, and execute asynchronously without forcing the host into HAgent-specific domain classes.

The host remains authoritative over its real state, domain rules, side effects, scheduling, persistence, and authorization.

## Core model

```text
Provider profile
    -> connection/model configuration

Agent profile
    -> reusable behavior/configuration

Runtime agent instance
    -> one live agent identity created from a profile

Execution request
    -> host-supplied input/context + correlation + execution requirements

Context
    -> host-supplied information available to an agent

Tool
    -> structured capability request with trusted host-owned execution

Memory
    -> owned and scoped information used across executions

Workspace
    -> optional shared communication context for users and agents

Execution
    -> bounded asynchronous model/tool work with lifecycle and correlation
```

The distinction between persistent profiles and runtime instances is fundamental. A host may create many runtime agents from one configured profile without turning every live instance into a permanent configuration record.

## Generic external-host requirement

HAgent must be capable of serving as the generic LLM cognition/execution layer for different project types.

The host owns:

- domain objects and authoritative state;
- observations and events;
- application lifecycle;
- scheduling policy;
- host-state persistence;
- authorization and approval rules;
- external side effects.

HAgent owns:

- agent profiles and runtime identity;
- provider/model invocation;
- execution lifecycle and snapshots;
- generic context ingestion and bounded model-facing representation;
- memory abstractions and ownership;
- structured tool contracts;
- structured output request/validation;
- execution correlation and observability;
- cancellation, timeout, concurrency, and stale-result protection;
- optional workspace and multi-agent coordination.

HAgent must not require a host-specific domain model, event framework, scheduler, persistence system, authorization model, or side-effect API.

## Generic execution request

The canonical execution boundary must accept a generic request/context payload instead of making plain string message the only integration model.

The request must support:

```text
host-supplied input/context
host correlation identity
execution options
optional structured-output requirements
```

The host payload is opaque to HAgent at the domain level. HAgent may bound, normalize, project, or serialize it through generic mechanisms but must not assign host-specific meaning to it.

Plain string messages remain a convenience API layered over this generic request model.

## System-prompt model

System prompts are composed from additive layers. A higher layer establishes broader policy and may add restrictions that every lower layer must preserve. A lower layer may add narrower instructions for its own scope, but it must not replace, erase, or weaken a higher layer.

The intended hierarchy is:

```text
Higher policy
    Provider
      ↓
    Agent profile
      ↓
    Runtime / context / execution additions
Lower policy
```

Prompt layering improves behavioral consistency but is not security. Authorization, permissions, approvals, budgets, and host-side validation remain authoritative outside the prompt.

## Context target

An agent should be able to receive a compact, bounded context snapshot containing only what the host has chosen to expose. Context may originate from observations, state snapshots, events, records, objects, resources, or other host information.

The representation should prefer native, lazy, projected, paged, and bounded forms over unnecessary materialization.

Automatic discovery, where supported, describes evidence rather than granting authority. Explicit host semantics may enrich or constrain the model-facing representation.

## Structured output target

A host may define its own output schema. HAgent must provide the generic mechanism to:

```text
host-defined schema
    -> provider structured-output request
    -> provider response
    -> schema validation
    -> validated structured result
```

HAgent must not contain host-domain schemas.

Provider capability support must remain explicit as Supported, Unsupported, or Unknown. Valid JSON text alone does not establish that a structured-output contract was satisfied.

## Tool target

Tool definitions describe what a model may request. Trusted handlers define what the host actually executes.

Tool permissions, authorization callbacks, approvals, budgets, and guardrails must be enforced outside model instructions.

Tool execution context must preserve enough generic identity for authorization and observability, including execution identity, runtime-instance identity, host correlation, tool identity, tool-call identity, arguments, and cancellation.

## Memory target

Memory ownership must be separable from the reusable agent profile. Two runtime instances created from the same profile must be able to maintain independent private memories.

Shared memory is a separate, explicitly governed scope.

Memory should remain lightweight and work without requiring a local GPU, embedding model, vector database, or large resident index.

## Runtime and concurrency target

A host must be able to create one runtime instance, execute against it repeatedly for an arbitrary lifetime, and explicitly retire or shut it down.

Multiple runtime instances must remain independently addressable and safe to execute concurrently. Runtime state such as lifecycle, execution revision, shutdown signaling, overrides, and private memory ownership must not leak across instances.

Execution must remain asynchronous, cancellable, bounded, and safe against conflicting late completion when a provider ignores cooperative cancellation.

Host scheduling remains host-controlled. HAgent may provide focused admission primitives but must not take ownership of host timing or event-loop policy.

## External-consumer target

HAgent must remain independent of the applications that consume it. External hosts adapt their own state, context, capabilities, scheduling, and side effects to HAgent's generic contracts.

No external consumer should require HAgent.Core to import or understand the consumer's domain types.

## Security target

No model instruction is an authorization boundary.

The architecture must distinguish, wherever meaningful:

```text
discovery
read
projection/query
export
write
invoke
approval
```

Database access must use restricted structured queries and parameterized execution. Raw model-generated SQL is outside the target design.

## Development principles

- Keep Core provider-neutral and dependency-light.
- Prefer small adapters over framework-sized dependencies.
- Preserve .NET Framework 4.8.1 compatibility where currently targeted and support .NET 9 where supported.
- Design for low RAM and no GPU assumption.
- Keep runtime work cancellable, bounded, correlated, concurrent, and safe against stale results.
- Keep persistent configuration separate from live runtime state.
- Treat host execution input and runtime configuration as separate concepts.
- Make structured output a real request/validation contract rather than JSON detection.
- Add one coherent implementation slice at a time.
- Verify completed capabilities through `HAgent.Example` before marking them complete.
- Keep documentation synchronized with implementation state.

## What success looks like

A developer should be able to add HAgent to a host and start with a simple call:

```csharp
await ai.SendAsync("assistant", "Hello");
```

and later grow the same integration into:

```text
host
  -> generic execution/context requests
  -> multiple runtime agent instances
  -> private/shared memory
  -> bounded host context
  -> authorized tools
  -> structured model output
  -> workspace routing
  -> visible agent collaboration
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

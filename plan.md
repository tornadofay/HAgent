# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

HAgent is a lightweight, provider-neutral .NET agent runtime. It can be used for simple chat or embedded into a host application, simulation, game, or other environment.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.8 Data Access + Authorization — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

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
- `HAgent.Example` verification for the completed 0.7 UI/data capabilities.

## Active implementation

The active implementation plan is `docs/plan/20-active.md` and contains only the current milestone.

## Verification rule

A capability becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the project documentation reflects the result.

Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — non-negotiable engineering and repository rules.
- `docs/architecture/` — stable architectural design and boundaries.
- `docs/plan/` — master direction, current state, and active implementation only.
- `docs/roadmap/` — ordered implementation path, including completed phases and future work.
- `docs/storage.md` — persistence/backend details.

The root `plan.md` and `roadmap.md` are generated views, not independent sources of truth.

## Purpose

HAgent is a general-purpose, provider-neutral runtime that makes connecting software to LLMs practical. A host may use it for simple chat, embedded business software, games, simulations, or other applications.

HAgent must remain useful at both ends of that range:

```text
simple host
    -> one provider
    -> one agent
    -> one conversation

advanced host
    -> many runtime agents
    -> independent memories and contexts
    -> tools and host capabilities
    -> shared workspaces and routing
    -> concurrent execution
    -> persistent or ephemeral runtime state
```

## End-state goal

A host should be able to provide HAgent with an application environment and choose how much intelligence it wants to expose. HAgent should provide the generic infrastructure required for agents to understand supplied context, communicate with models, remember information, use authorized tools, coordinate with other agents, and execute asynchronously without forcing the host into HAgent-specific domain classes.

The host remains authoritative over its real state, domain rules, side effects, and authorization.

## Core model

```text
Provider profile
    -> connection/model configuration

Agent profile
    -> reusable behavior/configuration

Runtime agent instance
    -> one live agent identity created from a profile

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

## Host context

HAgent supports two complementary ways for hosts to describe their environment.

### Explicit developer abstraction

The host can deliberately expose semantic concepts such as `Customer`, `Invoice`, or a domain-specific service. This is the highest-control path.

### Automatic discovery/adaptation

HAgent may inspect live host objects through adapters when the host enables the capability. It can discover controls, bindings, native data sources, application objects, and other structural information without requiring HAgent to reference the host's concrete types.

Discovery is evidence about what exists. It is never authorization and must not silently grant access to perform an operation.

## Multi-agent target

Multiple runtime agents are first-class. A common host pattern is:

```text
Workspace
    |
    +-- User
    +-- Coordinator
    +-- Specialist A
    +-- Specialist B
```

The coordinator is simply a policy-selected default recipient; HAgent must not hard-code business roles.

An unaddressed user message goes to the workspace default recipient. An explicitly addressed message goes to its target. Agent-to-agent delegation is explicit. Agent-to-agent work may be visible in the shared workspace when the host enables that visibility.

Specialists may represent an entire domain, table, subsystem, simulation capability, or other host-defined responsibility. They are not inherently tied to one record.

## Memory target

Memory ownership is separable from the reusable agent profile. Two runtime instances created from the same profile can maintain independent private memories.

Shared workspace/application memory is a separate, explicitly governed scope.

Memory remains lightweight and works without a local GPU, embedding model, vector database, or large resident index.

## Context target

An agent can receive a compact, bounded context snapshot containing only what the host has chosen to expose. Context sources may include UI state, data-source structure and selected data, application-owned objects, task/event information, external observations, and explicit developer-provided semantic abstractions.

The representation should prefer native, lazy, projected, paged, and bounded forms over unnecessary materialization.

## Tool target

Tool definitions describe what a model may request. Trusted handlers define what the host actually executes. Executable handlers are runtime-owned and never serialized.

Permissions, authorization callbacks, approvals, budgets, and guardrails are enforced outside model instructions.

Initial tool taxonomy:

```text
BuiltIn
Application
Declarative
UI
SqlServer
MySql
```

## Security target

No model instruction is an authorization boundary. The architecture distinguishes, wherever meaningful, discovery, read, projection/query, export, write, invoke, and approval.

Database access uses restricted structured queries and parameterized execution. Raw model-generated SQL is outside the target design.

## External-consumer target

HAgent remains independent of its host applications.

HWorld is a primary architectural consumer. HWorld owns world state, simulation time, physics, perception, scheduling, rendering, and action validation. HAgent supplies generic agent/runtime capabilities. HAgent must not import HWorld types or rules.

The same principle applies to business applications and future HControl/BaseForm, GDI, DirectX, Unity, or other host adapters.

## Development principles

- Keep Core provider-neutral and dependency-light.
- Prefer small adapters over framework-sized dependencies.
- Preserve .NET Framework 4.8.1 compatibility where currently targeted and support .NET 9 where supported.
- Design for low RAM and no GPU assumption.
- Keep runtime work cancellable, bounded, correlated, and safe against stale results.
- Keep persistent configuration separate from live runtime state.
- Implement one coherent slice at a time.
- Verify completed capabilities through `HAgent.Example` before marking them complete.
- Keep documentation synchronized with implementation state.

## End-state integration

A developer should be able to start with:

```csharp
await ai.SendAsync("assistant", "Hello");
```

and grow the same integration into:

```text
host
  -> multiple runtime agent instances
  -> private/shared memory
  -> UI/data/application context
  -> authorized tools
  -> workspace routing
  -> visible agent collaboration
  -> asynchronous background work
```

without replacing HAgent or introducing application-specific types into `HAgent.Core`.

## 0.8 Data Access + Authorization

### Objective
Turn the verified structured-query contracts into safe application and database access. No arbitrary SQL, unrestricted reflection, or implicit authorization.

### Current slices

- [ ] Application-owned adapter implementing `IDataQuerySource` for explicitly approved sources.
- [ ] Authoritative schema/field allow-list independent of model requests.
- [ ] Data permissions separated into discovery, projection/query, export, and write operations.
- [ ] Host authorization callback contract.
- [ ] Query limits, cancellation, timeout, and resource budgets.
- [ ] Restricted SQL Server adapter using generated parameterized commands only.
- [ ] Restricted MySQL adapter using generated parameterized commands only.
- [ ] Read-only database tools and result/audit metadata before database writes.

### Live Example

When the SQL Server adapter is ready, `HAgent.Example` will provide runtime-only test fields:

```text
Server Name
User Name
Password
Database
```

The Example will target an explicitly disposable/read-only test database and verify connection, authorized schema/fields, structured queries, bounded results, cancellation/timeout, and unauthorized-operation rejection.

Connection values must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- SQL fragments embedded in `DataQueryRequest`.
- Implicit permission to every table or column.
- Treating UI binding, `TableInfo`, object provenance, or model instructions as authorization.
- Persisting test database passwords as ordinary configuration.

## Definition of done

0.8 is complete only after the restricted application/database path is implemented and the matching `HAgent.Example` verification passes locally.

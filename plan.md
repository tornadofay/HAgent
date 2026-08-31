# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Current project state

## Project

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

## System-prompt model

System prompts are composed from additive layers. A higher layer establishes the broader policy and may add restrictions that every lower layer must preserve. A lower layer may add narrower instructions for its own scope, but it must not replace, erase, or weaken a higher layer.

The intended hierarchy is:

```text
Higher policy
    Provider
      ↓
    Agent profile
      ↓
    Runtime / workspace / context / task additions
Lower policy
```

All applicable layers remain present in the final system prompt. The current implementation provides provider + agent + per-execution layers; future runtime/context/workspace layers must use the same composition mechanism rather than introducing replacement semantics.

Prompt layering improves behavioral consistency but is not security. Authorization, permissions, approvals, budgets, and host-side validation remain authoritative outside the prompt.

## Host context

HAgent must support two complementary ways for hosts to describe their environment.

### Explicit developer abstraction

The host can deliberately expose semantic concepts such as `Customer`, `Invoice`, or a domain-specific service. This is the highest-control path.

### Automatic discovery/adaptation

HAgent may inspect live host objects through adapters when the host enables the capability. The implementation can discover controls, bindings, native data sources, application objects, and other structural information without requiring HAgent to reference the host's concrete types.

Discovery is evidence about what exists. It is never authorization and it must not silently grant access to perform an operation.

## Multi-agent target

Multiple runtime agents must be first-class rather than a collection of unrelated special cases.

A common host pattern is:

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

Memory ownership must be separable from the reusable agent profile. Two runtime instances created from the same profile must be able to maintain independent private memories.

Shared workspace/application memory is a separate, explicitly governed scope.

Memory should remain lightweight and work without a local GPU, embedding model, vector database, or large resident index.

## Context target

An agent should be able to receive a compact, bounded context snapshot containing only what the host has chosen to expose. Context sources may include:

- UI state;
- data-source structure and selected data;
- application-owned objects;
- task/event information;
- external environment observations;
- explicit developer-provided semantic abstractions.

The representation should prefer native, lazy, projected, paged, and bounded forms over unnecessary materialization.

## Tool target

Tool definitions describe what a model may request. Trusted handlers define what the host actually executes.

Executable handlers are runtime-owned and never serialized.

Tool permissions, authorization callbacks, approvals, budgets, and guardrails must be enforced outside model instructions.

The initial taxonomy remains:

```text
BuiltIn
Application
Declarative
UI
SqlServer
MySql
```

Extension tooling is a later platform concern.

## Security target

No model instruction is an authorization boundary.

The architecture must eventually distinguish, wherever meaningful:

```text
discovery
read
projection/query
export
write
invoke
approval
```

UI bindings, application-object metadata, provenance, and inferred semantics may help describe capabilities but must never by themselves authorize them.

Database access must use restricted structured queries and parameterized execution. Raw model-generated SQL is outside the target design.

## External-consumer target

HAgent must remain independent of the applications that consume it.

HWorld is a primary architectural consumer example. HWorld owns world state, simulation time, physics, perception, scheduling, rendering, and action validation. HAgent supplies generic agent/runtime capabilities. HAgent must not import HWorld types or rules.

The same principle applies to business applications and future adapters for HControl/BaseForm, GDI, DirectX, Unity, or other host surfaces.

## Development principles

- Keep Core provider-neutral and dependency-light.
- Prefer small adapters over framework-sized dependencies.
- Preserve .NET Framework 4.8.1 compatibility where currently targeted and support .NET 9 where supported.
- Design for low RAM and no GPU assumption.
- Keep runtime work cancellable, bounded, correlated, and safe against stale results.
- Keep persistent configuration separate from live runtime state.
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
  -> multiple runtime agent instances
  -> private/shared memory
  -> UI/data/application context
  -> authorized tools
  -> workspace routing
  -> visible agent collaboration
  -> asynchronous background work
```

without replacing HAgent or introducing application-specific types into `HAgent.Core`.

## Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.8 Data Access + Authorization + Internal Storage

### Objective
Provide bounded structured data contracts while making HAgent's persistence an explicitly HAgent-owned storage boundary. HAgent must never use its internal storage connection as an implicit gateway to a host application's business database.

### Current slices

- [x] Application-owned structured-query contract and authoritative field schema.
- [x] Data permissions separated into discovery, projection/query, export, and write operations.
- [x] Host authorization callback contract.
- [x] Query limits, cancellation, timeout, and resource budgets.
- [x] HAgent storage backend configuration for File, SQL Server, and MySQL.
- [x] Application-specific File storage layout.
- [x] SQL Server HAgent database creation and schema bootstrap foundation.
- [x] MySQL HAgent database creation and schema bootstrap foundation.
- [x] Example agent/provider/prompt loading follows the selected internal storage backend.
- [ ] Wire all internal repositories to the selected storage backend.
- [ ] Versioned schema migrations beyond the initial bootstrap version.
- [ ] HAgent internal storage credentials/secret lifecycle and connection testing UI.
- [ ] Read-only HAgent internal data tools and result/audit metadata before any writes.

### Current slice: configured storage backend resolution

The Example now resolves its `IAiStore` and tool-definition store from `HAgentStorageOptions` rather than hardcoding the File backend. File, SQL Server, and MySQL are therefore distinct runtime storage choices.

The selected backend is used consistently for agent/provider loading, provider-system-prompt resolution, configuration display, and client creation. This prevents the UI from displaying one backend's agents while runtime execution uses another backend.

SQL Server and MySQL resolution bootstraps the HAgent-owned database before creating the corresponding internal repositories. No host application database is used by this resolution path.

### Storage foundation

`HAgent.Core` now provides `HAgentStorageOptions` with `File`, `SqlServer`, and `MySql` backends, host application naming, application-specific database naming, and non-secret connection metadata. Database passwords remain outside this ordinary configuration model.

`HAgent.Storage.File` now provides an application-specific `HAgentData` directory layout for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime state, cache, and logs.

`HAgent.Storage.SqlServer` and `HAgent.Storage.MySql` now provide HAgent-owned database bootstrappers. They connect to the configured server, create the derived HAgent database when absent, then create only HAgent-owned tables and a schema-version record. The initial schema covers providers, agents, tools, memory entries, conversations, skills, wiki documents/chunks, and future migration metadata.

The WinForms AI Configuration surface now includes a Storage page for selecting the backend and configuring application name, file root, database name, server name, and username. The password field is transient and is cleared after saving; it is not serialized as ordinary configuration. The default File paths now live beneath the host executable's application-specific `HAgentData` directory.

The previously implemented SQL Server `IDataQuerySource` path against arbitrary host tables was removed because it violated the internal-storage boundary. The provider-neutral structured-query contract remains independent and must not be interpreted as permission to use a host application's business database.

### Live verification

The manual Example must verify the selected internal storage backend. For File storage it should verify the application-specific directory structure and internal repositories. For SQL Server/MySQL it should verify connection, database creation when absent, schema initialization, idempotent re-open, and safe refusal to operate against unrelated host application tables.

Database credentials must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- Implicit access to the host application's business database.
- Treating a provider connection as permission to inspect arbitrary host tables.
- Persisting database passwords as ordinary configuration.
- Treating UI discovery, provenance, or model instructions as authorization.

## Definition of done

0.8 is complete only after HAgent internal persistence is selectable and operational across the supported storage backends, schema upgrades are deterministic, and the Example verifies that HAgent storage remains isolated from host application data.

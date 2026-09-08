# HAgent

**Lightweight, provider-neutral AI cognition and execution runtime for .NET applications.**

HAgent provides reusable infrastructure for connecting software to LLMs and building long-lived agent behavior without forcing a specific application architecture or domain model. It is intended for conversational software, business applications, services, games, simulations, automation, developer tools, and other host environments.

> Status: **0.954 Prompt / Instruction Governance is the current implementation milestone.**
>
> Completed major foundation: **0.95 Generic External Host Integration**, verified on .NET Framework 4.8.1 and .NET 9.
>
> Next foundations: **Context Engineering, Observability, Evaluation, Knowledge/Skills/Memory Governance + Learning, Agent Lifecycle, Goal/Plan Recovery, Human Intervention, Provider Adapter Lifecycle, Configuration / Storage / Portability, and Capability-Aware Execution**.
>
> Longer-term direction: **Persistent Cognitive Runtime with an extensible Cognitive Kernel and pluggable Cognitive Strategies**.
>
> Targets: **.NET Framework 4.8.1 and .NET 9**.

## What HAgent is

HAgent separates reusable cognition/execution infrastructure from the domain logic of the host application.

```text
Host application
        |
        |  generic request + bounded context
        v
+------------------------------------------------+
|                     HAgent                     |
|                                                |
|  Cognitive Kernel + Cognitive Strategies       |
|  Agent Runtime Instances                       |
|  Execution / Providers / Model Targets        |
|  Skills / Knowledge / Memory / Learning        |
|  Tools / Structured Output / Policies         |
|  Events / Context / Telemetry                  |
|  Workspaces / Coordination                     |
+------------------------------------------------+
        |
        v
Host-owned domain state, authorization,
scheduling, persistence, and side effects
```

The host remains authoritative for its own business state and security decisions. HAgent provides the generic cognition and execution layer around that state.

## Architecture direction

HAgent is intentionally built as two complementary levels:

```text
HAgent
│
├── Execution Engine
│   ├── provider/model execution
│   ├── context and memory integration
│   ├── structured tools and output
│   ├── capability-aware target planning
│   └── cancellation / timeout / lifecycle safety
│
└── Persistent Cognitive Runtime
    ├── persistent cognitive state
    ├── event-driven activation
    ├── beliefs + revision
    ├── attention / global workspace
    ├── goals / intentions
    ├── plans / operators / impasses
    ├── deterministic vs deliberative decisions
    ├── experience / memory / learning
    └── replaceable cognitive strategies
```

The execution engine remains useful directly. Persistent cognition is a higher-level runtime built on the same provider-neutral execution contracts.

### Cognitive Kernel vs Cognitive Strategy

The **Cognitive Kernel** is the stable runtime substrate for cognitive state, lifecycle, history, revision, persistence boundaries, and governance.

A **Cognitive Strategy** determines how that state is interpreted and which cognitive actions are selected. The first reference strategy is **Adaptive Hybrid Cognition (AHC)**. It is deliberately not treated as the only or final cognitive architecture; future strategies can be evaluated and replaced without redesigning the kernel.

The cognitive runtime can choose deterministic behavior when rules, state, memory, or learned procedures are sufficient, and request probabilistic reasoning only when needed. The LLM is therefore a replaceable reasoning component, not the entire agent architecture.

## Basic usage

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

Plain string messaging is the convenience entry point. The canonical generic execution boundary is `AgentExecutionRequest`, which can carry multiple messages, bounded host context, host correlation identity, execution options, and structured-output requirements without embedding host-domain concepts in HAgent.Core.

## First-class Knowledge / Skills / Memory / Learning

HAgent treats Knowledge, Skills, Memory, and Learning as first-class architecture from the foundation upward rather than as a late product feature.

```text
Early foundation
    Resource identity / scope / ownership
    Provenance / version / lifecycle
    Skill definitions and references
    Knowledge / Wiki contracts and retrieval boundaries
    Memory family/type foundations
    Typed learning-candidate foundations

Mature governance
    Policy / authorization
    Capability inheritance and runtime overrides
    Effective execution resource snapshots
    Bounded retrieval / retention
    Learning modes and learning policy
    Evaluation / validation / approval / promotion
    Resource management UI
```

The four concepts remain distinct:

- **Skills** are reusable executable capabilities/procedures with stable identity and versioning.
- **Knowledge** is reusable retrievable information. A **Wiki** is one managed persistent knowledge source within the broader knowledge system.
- **Memory** is scoped experience/state, including working, episodic, semantic, procedural, and future memory families.
- **Learning** turns execution experience into typed candidates and, only when governed and authorized, promotes those candidates into Memory, Knowledge, Skills, or other explicitly supported targets.

Resource foundations are established early and reused by Context, Instruction Governance, Runtime, Evaluation, and Persistent Cognition. Mature resource governance is completed later once identity, policy, context, and evaluation boundaries are available. No later phase may introduce a parallel resource architecture merely because mature governance is not yet complete.

Resources are scope-aware. Runtime instances inherit profile configuration and can apply runtime-only `Inherit` / `Enabled` / `Disabled` overrides without mutating the persistent profile.

Learning promotion is governed by policy, provenance, validation, authorization, and evaluation rather than treating model output as automatically authoritative.

## Provider and execution capability model

HAgent does not assume that every provider or model supports every feature exposed by the generic API. Execution targets can differ by provider, account/project, endpoint, model/deployment, capability, limits, quotas, rate limits, concurrency capacity, health, latency, and policy.

Requested features are evaluated against the selected execution target before provider transport. Capabilities such as structured output, tool calling, reasoning, image/audio/video input or output, embeddings, and future task types may be `Supported`, `Unsupported`, or `Unknown`.

Required capability failures must produce an explicit failure or use an explicitly configured fallback/degradation policy; incompatible requests are not silently sent to a target.

The architecture distinguishes three decisions:

```text
Cognitive Strategy
    = how should the agent reason and manage cognitive state?

Reasoning Requirement
    = what kind of reasoning capability is needed now?

Execution Planner
    = where/how should that reasoning execute?
```

This separation lets cognition request the type of reasoning it needs without hard-coding a provider or model into the cognitive architecture.

## Roadmap position

The current roadmap is intentionally ordered as architectural foundations first, then capability-aware execution, then persistent cognition:

```text
0.95   Generic External Host Integration                    complete
0.951  Identity / Tenancy / User Context                    complete
0.952  First-Class Event Subsystem                          complete
0.953  Unified Policy Engine                                complete
0.954  Prompt / Instruction Governance                       current
0.955  Context Engineering
0.956  Observability / Distributed Tracing
0.957  Evaluation / Quality Measurement
0.9575 Knowledge / Skills / Memory Governance + Learning
0.958  Agent Lifecycle / Health
0.9591 Goal / Plan Persistence / Recovery
0.959  Human-in-the-Loop / Intervention
0.9592 Provider Ecosystem / Adapter Lifecycle
0.96.x Configuration / Storage / Portability Evolution
0.96   Capability-Aware Execution
0.97   Persistent Cognitive Runtime
0.10   Workspaces / Routing / Chat                          paused
1.0    Collaboration / Workflows
```

The early 0.8 storage/resource foundation and the later 0.9575 mature governance phase deliberately split the old Knowledge / Skills / Memory Governance + Learning feature block according to architectural dependency. `0.9591` is ordered before `0.959` because durable goal/plan revisions and recovery state provide the persistent authority for later goal/plan intervention.

The roadmap is dependency-driven rather than permanently locked. When architectural understanding changes, the authoritative roadmap may be reordered deliberately rather than forcing new requirements into obsolete sequencing.

The detailed ordered roadmap is in [`roadmap.md`](roadmap.md), with modular source documents under [`docs/roadmap/`](docs/roadmap/).

## Current capabilities

The verified foundation currently includes:

- provider/agent configuration and routing;
- execution lifecycle, timeout, cancellation, retries, diagnostics, and failure reporting;
- persistent sessions and multiple memory forms;
- context budgeting and memory retrieval;
- capability discovery and normalized responses;
- streaming contracts and live streaming;
- structured tool definitions, registry, schema validation, provider transport, bounded tool loops, persistence, and per-agent assignment;
- WinForms UI Context with Form/UserControl attachment;
- semantic and bound/native data-source discovery;
- CurrencyManager/current-item/source relationships;
- control-to-source relationship discovery;
- bounded application-object discovery;
- provider-neutral structured data projection/query contracts;
- HAgent-owned storage configuration for File, SQL Server, and MySQL;
- HAgent-owned SQL Server/MySQL database bootstrap foundations;
- bounded internal inventory, memory, conversation, and execution-audit read tools;
- payload-free execution auditing with configurable bounded retention;
- runtime-instance identity, scope, runtime-only overrides, independent memory ownership, concurrent execution, stale-result protection, host-controlled scheduling, shutdown semantics, and optional runtime-state persistence;
- provider-neutral workspace participants, message metadata, default-recipient routing, and coordinator/specialist role policy;
- canonical generic host execution requests with bounded host context and host correlation;
- provider-facing request isolation and structured-output transport/fallback;
- verified external-consumer compatibility on .NET Framework 4.8.1 and .NET 9.

These are implementation foundations, not a claim that the complete roadmap is finished. The current milestone remains Prompt / Instruction Governance and the subsequent hardening phases are still being developed.

## Generic host integration

HAgent is designed to be embedded into host software without taking ownership of the host's domain model.

The generic integration model supports:

- arbitrary bounded host input and context;
- host-supplied correlation identities;
- host-defined structured output schemas with validation;
- cancellation, timeout, and safe late-completion handling;
- host-owned tools and capability execution;
- concurrent execution across independent runtime instances;
- optional persistence of generic runtime identity and lifecycle metadata;
- optional multi-agent coordination and workspace communication;
- scoped knowledge, skills, memory, and controlled learning.

The host remains responsible for domain state, scheduling, authorization, host persistence, and side effects. HAgent does not require the host to adopt a particular domain event system, command system, scheduler, authorization framework, or UI framework.

## HAgent storage

HAgent persists its own internal data separately from the host application's business database.

Supported storage backends are:

- **File** — HAgent-owned files beneath the host executable in `HAgentData`;
- **SQL Server** — a dedicated HAgent-owned database, normally named `<application-name>-ai`;
- **MySQL** — a dedicated HAgent-owned database, normally named `<application-name>-ai`.

HAgent storage is for providers, models, execution targets, agents, tools, memory, conversations, skills, knowledge/wiki, learning candidates, policies, runtime metadata, execution audit data, and other HAgent-owned records. A storage backend must never be treated as permission to inspect or modify the host application's business database.

Provider API keys are persisted with provider configuration and encrypted at rest. Shared SQL Server/MySQL configuration can therefore be used by multiple authorized HAgent processes or machines connected to the same HAgent database. Configuration export/import is planned as a versioned portable representation; normal export excludes provider credentials.

## WinForms Context

`HAgent.WinForms` can attach to a Form or arbitrary control tree such as a UserControl. It can inspect controls, bindings, native data sources, relationships, custom-control metadata, and bounded application objects when the host's permission policy allows it.

The public concept is **UI Context / Control Adapters**, not generic form serialization.

`DataTable` is optional. Native/bound sources, lazy adapters, paging, projections, and bounded extraction are preferred.

## Management UI direction

The planned `HAgent.WinForms` administration surface includes:

```text
General
    system/default policies and discovery settings

Providers / Models
    connection settings, credentials, discovery, targets, capabilities,
    limits, health, cost, and execution compatibility

Agents
    AI selection, Skills, Knowledge, Memory, Learning, effective configuration,
    cognitive strategy and runtime state

Cognition Workbench
    live beliefs, attention, goals, intentions, plans, memory,
    experience, reasoning decisions, history, and governed intervention

Learning Review
    pending suggestions -> inspect -> approve/reject

Wiki / Knowledge Manager
    create / edit / delete / search / relationships / used-by agents

Skill Manager
    create / edit / delete / version / relationships / used-by agents

Storage
    File / SQL Server / MySQL configuration and verification

Configuration Export / Import
    versioned package, compatibility validation, optional encrypted credentials
```

Known resource types may receive specialized panels while future/unknown resource types remain visible through a generic resource inventory contract.

## Tools

Initial tool categories are:

| Type | Purpose |
|---|---|
| BuiltIn | HAgent-provided capabilities |
| Application | Host application capabilities |
| Declarative | Restricted configuration-driven capabilities |
| UI | WinForms capabilities |
| SqlServer | HAgent SQL Server capabilities |
| MySql | HAgent MySQL capabilities |

Tool definitions are separate from executable handlers. Handlers remain runtime-owned and are never serialized.

## Security model

The model is a requester, not an authority.

Permissions, authorization, approvals, limits, cancellation, instruction trust, and host-side validation remain outside model output. HAgent database storage is dedicated to HAgent's own persistence and does not provide implicit access to host application tables. Structured data contracts are not raw SQL access. Learning promotion and cognitive intervention are likewise controlled through explicit policy and authorization.

## Example application

`HAgent.Example` is the manual developer/verification application, separate from `HAgent.Tests`.

Meaningful capabilities should have runnable Example verification using public APIs and reproducible C# snippets.

## Project structure

- `HAgent.Core` — provider-neutral models, runtime, cognition contracts, context, memory, knowledge, skills, learning, tools, events, policy, tracing, evaluation, and coordination contracts.
- `HAgent.Providers.OpenAICompatible` — OpenAI-compatible provider transport and capabilities.
- `HAgent.Storage.File` — file configuration, encrypted provider credentials, memory, conversations, skills/wiki, and learning persistence.
- `HAgent.Storage.SqlServer` — HAgent-owned SQL Server persistence and schema bootstrap.
- `HAgent.Storage.MySql` — HAgent-owned MySQL persistence and schema bootstrap.
- `HAgent.WinForms` — management UI, UI Context/control adapters, and the planned Cognitive Runtime Workbench.
- `HAgent.Example` — manual verification host.
- `HAgent.Tests` — automated tests.
- `samples/HAgent.ExternalConsumer` — broad external-host integration smoke sample.

## Documentation

- [`docs/architecture/`](docs/architecture/) — stable architecture and boundaries.
- [`docs/plan/`](docs/plan/) — master direction, current state, and active implementation.
- [`docs/roadmap/`](docs/roadmap/) — ordered implementation path, including completed foundations and future phases.
- [`docs/research/`](docs/research/) — research and comparative architectural analysis.
- [`docs/storage.md`](docs/storage.md) — storage-specific details.
- [`AGENTS.md`](AGENTS.md) — engineering invariants.

Important architectural references include:

- [`docs/architecture/16-cognitive-runtime.md`](docs/architecture/16-cognitive-runtime.md) — persistent cognitive runtime, Cognitive Kernel, Cognitive Strategies, beliefs, planning, learning, and workbench architecture.
- [`docs/architecture/15-research-foundations.md`](docs/architecture/15-research-foundations.md) — research lineage and cognitive-architecture mapping.
- [`docs/research/2026-09-persistent-cognitive-runtime-comparison.md`](docs/research/2026-09-persistent-cognitive-runtime-comparison.md) — detailed research comparison and recommended evolution.
- [`docs/architecture/80-knowledge-memory-learning.md`](docs/architecture/80-knowledge-memory-learning.md) — first-class Knowledge, Skills, Memory, and Learning architecture and boundaries.

Root [`plan.md`](plan.md) and [`roadmap.md`](roadmap.md) are generated views from the modular source documents; update the source documents rather than editing those generated files directly.

## Supported targets

- .NET Framework 4.8.1
- .NET 9

.NET 10 is deferred until the development environment and compatibility plan are ready.

## License

MIT — see [LICENSE](LICENSE).

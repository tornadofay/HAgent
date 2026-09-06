# HAgent

**Lightweight, provider-neutral AI cognition and execution runtime for .NET applications.**

HAgent provides the reusable infrastructure needed to connect software to LLMs without forcing a specific application architecture or domain model. The project is intended to support any software environment that requires LLM-driven behavior, from simple conversational programs to business software, services, games, simulations, automation, developer tools, and other hosts.

> Status: **Foundational architecture hardening is now planned before Phase 0.96.**
>
> Completed foundation: **0.95 Generic External Host Integration**.
>
> Next architectural foundations: **Identity, Events, Policy, Prompt Governance, Context Engineering, Observability, Evaluation, Agent Lifecycle, Human Intervention, Goal/Plan Recovery, and Provider Adapter Lifecycle**.
>
> Targets: **.NET Framework 4.8.1 and .NET 9**.

## What HAgent provides

```text
Host application
        |
Generic execution/context
        |
Runtime agent instances
        |
+-- provider/model execution
+-- skills and reusable skill sets
+-- knowledge/wiki retrieval
+-- scoped memory
+-- controlled learning and review
+-- structured tools
+-- structured model output
+-- asynchronous lifecycle/cancellation
+-- workspaces and multi-agent coordination
```

A host may use one configured agent or create many independent runtime instances from reusable profiles.

## Architecture direction

HAgent is being built as two complementary layers:

```text
HAgent
│
├── Execution Engine
│   ├── provider/model execution
│   ├── context and memory integration
│   ├── tools and structured output
│   ├── capability-aware target selection
│   └── cancellation / timeout / lifecycle safety
│
└── Persistent Cognitive Runtime
    ├── events
    ├── attention
    ├── goals / intentions
    ├── plans
    ├── reactive / deliberative decisions
    └── long-lived agent state
```

The execution engine remains usable directly. Persistent cognition is an optional higher-level runtime built on the same generic execution contracts.

## Basic usage

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

Plain string messaging is the convenience entry point. The canonical generic execution boundary is `AgentExecutionRequest`, which can carry multiple messages, host-supplied bounded context, host correlation identity, execution options, and structured-output requirements without embedding host-domain concepts in HAgent.Core.

## Capability model

HAgent separates four related concepts:

- **Skills** are reusable executable capabilities/procedures. They are shared definitions referenced by agents, not copied into every runtime instance.
- **Knowledge** is reusable retrievable information. A **Wiki** is one managed persistent knowledge source within the broader knowledge system.
- **Memory** is scoped experience/state, including working, episodic, semantic, procedural, and future memory families.
- **Learning** analyzes execution experience and creates typed candidates for memory, knowledge, or skill improvement. Promotion is controlled by `LearningMode` and policy rather than treating LLM output as automatically authoritative.

Resources are scope-aware. Runtime instances inherit profile capability configuration and may apply runtime-only `Inherit`/`Enabled`/`Disabled` overrides without mutating the persistent profile.

## Provider/model capability model

HAgent must not assume that a provider or model supports every feature exposed by the generic API. Execution targets can differ by provider, deployment/account policy, model/version, capabilities, context/output limits, quotas, rate limits, concurrency limits, and current availability.

The capability-aware execution design treats requested features as requirements and evaluates them against the selected execution target before sending a provider request. Capabilities such as structured output, tool calling, reasoning, image input/output, audio, embeddings, and future modalities may be `Supported`, `Unsupported`, or `Unknown`, with runtime/account policy distinguished from model-declared capability.

When a required capability is unavailable, HAgent must fail or apply an explicitly configured fallback/degradation policy rather than silently sending an incompatible request.

## Foundational architecture work

Before Phase 0.96, the roadmap now establishes these cross-cutting foundations:

```text
0.951 Identity / Tenancy / User Context
0.952 First-Class Event Subsystem
0.953 Unified Policy Engine
0.954 Prompt / Instruction Governance
0.955 Context Engineering
0.956 Observability / Distributed Tracing
0.957 Evaluation / Quality Measurement
0.958 Agent Lifecycle / Health Management
0.959 Human-in-the-Loop / Intervention
0.9591 Goal / Plan Persistence / Recovery
0.9592 Provider Ecosystem / Adapter Lifecycle
0.96 Capability-Aware Execution
0.97 Persistent Cognitive Runtime
```

These phases strengthen the generic platform around execution and cognition rather than tying HAgent to a particular host application.

## Current capabilities

The verified foundation includes:

- provider/model routing and capability discovery;
- execution lifecycle, retries, timeout, cancellation, diagnostics, and stale-result protection;
- persistent sessions and multiple memory forms;
- context budgeting and lightweight memory retrieval;
- normalized responses and streaming;
- structured tool definitions, validation, transport, loops, persistence, and per-agent assignment;
- WinForms UI Context and control adapters;
- semantic and bound/native data-source discovery;
- application-object discovery with bounded inspection;
- provider-neutral structured data projection/query contracts;
- HAgent-owned storage configuration for File, SQL Server, and MySQL backends;
- application-specific File storage layout;
- HAgent-owned SQL Server/MySQL database bootstrap foundations;
- bounded internal HAgent data inspection and payload-free execution audit;
- runtime agent instances with independent memory, concurrency, stale-result, shutdown, scheduling, and optional runtime-state persistence;
- provider-neutral workspace participants, message metadata, default-recipient routing, and coordinator/specialist role policy;
- canonical generic host execution requests with bounded host context and host correlation;
- distinct provider-facing execution requests and native/fallback structured-output transport;
- verified external-consumer compatibility on .NET Framework 4.8.1 and .NET 9.

The remaining architecture phases add a unified event model, policy evaluation, trusted instruction composition, mature context assembly, tracing, evaluation, agent health/control, durable plan recovery, and a stronger provider adapter lifecycle before higher-level persistent cognition depends on them.

## Generic host integration target

HAgent is designed to be the generic LLM cognition/execution layer for host software. The host remains authoritative for domain state, lifecycle, scheduling, persistence, authorization, and side effects.

The generic integration target includes:

- arbitrary bounded host input/context;
- host-supplied correlation identities;
- host-defined structured output schemas with validation;
- cancellation, timeout, and safe late-completion handling;
- host-owned tools and capability execution;
- concurrent execution across independent runtime instances;
- optional persistence of generic runtime identity and lifecycle metadata;
- optional multi-agent coordination and workspace communication;
- scoped knowledge, skills, memory, and controlled learning.

HAgent provides its own provider-neutral event and cognitive contracts, but does not require a host to adopt a particular domain event system, command system, scheduler, authorization framework, or UI framework.

## HAgent storage

HAgent persists its own internal data separately from the host application's business database.

Supported storage backends are:

- **File** — HAgent-owned files beneath the host executable in `HAgentData`;
- **SQL Server** — a dedicated HAgent-owned database, normally named `<application-name>-ai`;
- **MySQL** — a dedicated HAgent-owned database, normally named `<application-name>-ai`.

HAgent storage is for providers, models, execution targets, agents, tools, memory, conversations, skills, knowledge/wiki, learning candidates, policies, runtime metadata, execution audit data, and other HAgent-owned records. A storage backend must never be treated as permission to inspect or modify the host application's business database.

Provider API keys are persisted with provider configuration and encrypted at rest. There is no separate provider secret-reference or vault architecture. SQL Server/MySQL configuration can therefore be shared by multiple authorized HAgent processes or machines connected to the same HAgent database. Storage-server passwords remain part of the runtime connection configuration rather than ordinary HAgent provider records.

Configuration export/import is planned as a versioned portable representation of HAgent-owned configuration. Normal export excludes provider credentials; an explicitly requested credential-bearing export can include encrypted API keys protected by the export mechanism.

## WinForms Context

`HAgent.WinForms` can attach to a Form or arbitrary control tree such as a UserControl. It can inspect controls, bindings, native data sources, relationships, custom-control metadata, and bounded application objects when the host's permission policy allows it.

The public concept is **UI Context / Control Adapters**, not generic form serialization.

`DataTable` is optional. Native/bound sources, lazy adapters, paging, projections, and bounded extraction are preferred.

## Management UI target

`HAgent.WinForms` will provide administration for:

```text
General
    system/default policies and discovery settings

Providers
    connection settings, API credentials, discovery status

Models
    logical models, execution targets, capabilities, limits, health, cost

Agents
    AI selection, Skills, Knowledge, Memory, Learning, effective configuration

Learning Review
    pending suggestions -> inspect -> approve/reject

Wiki / Knowledge Manager
    new / edit / delete / search / relationships / used-by agents

Skill Manager
    new / edit / delete / version / relationships / used-by agents

Storage
    File / SQL Server / MySQL configuration and verification

Configuration Export / Import
    versioned package, compatibility validation, optional encrypted credentials
```

The agent knowledge view is extensible: known resource types may have specialized panels while future/unknown resource types remain visible through a generic resource inventory contract.

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

Permissions, authorization, approvals, limits, cancellation, prompt/instruction trust, and host-side validation remain outside model output. HAgent database storage is dedicated to HAgent's own persistence and does not provide implicit access to host application tables. Structured data contracts are not raw SQL access. Learning promotion is likewise controlled outside the model by policy and authorization.

## Example application

`HAgent.Example` is the manual developer/verification application, separate from `HAgent.Tests`.

Meaningful capabilities should have runnable Example verification using public APIs and a reproducible C# snippet.

## Project structure

- `HAgent.Core` — provider-neutral models, runtime, context, memory, knowledge, skills, learning, tools, event, policy, tracing, evaluation, and coordination contracts.
- `HAgent.Providers.OpenAICompatible` — OpenAI-compatible provider transport and capabilities.
- `HAgent.Storage.File` — file configuration, encrypted provider credentials, memory, conversations, skills/wiki, and learning persistence.
- `HAgent.Storage.SqlServer` — HAgent-owned SQL Server persistence and schema bootstrap.
- `HAgent.Storage.MySql` — HAgent-owned MySQL persistence and schema bootstrap.
- `HAgent.WinForms` — management UI and WinForms UI Context/control adapters.
- `HAgent.Example` — manual verification host.
- `HAgent.Tests` — automated tests.
- `samples/HAgent.ExternalConsumer` — broad external-host integration smoke sample.

## Documentation

- `docs/architecture/` — stable architecture and boundaries.
- `docs/plan/` — master direction, current state, and active implementation.
- `docs/roadmap/` — ordered implementation path, including completed foundations and future phases.
- `docs/storage.md` — storage-specific details.
- `AGENTS.md` — engineering invariants.

Root `plan.md` and `roadmap.md` are generated views from the modular source documents.

## Supported targets

- .NET Framework 4.8.1
- .NET 9

.NET 10 is deferred until the development environment and compatibility plan are ready.

## License

MIT — see [LICENSE](LICENSE).

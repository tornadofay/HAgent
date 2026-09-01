# HAgent

**Lightweight, provider-neutral AI cognition and execution runtime for .NET applications.**

HAgent provides the reusable infrastructure needed to connect software to LLMs without forcing a specific application architecture or domain model. The project is intended to support any software environment that requires LLM-driven behavior, from simple conversational programs to business software, services, games, simulations, automation, developer tools, and other hosts.

> Status: **0.8 Data Access + Authorization + Internal Storage — active**
>
> Next architecture milestone: **0.95 Generic External Host Integration**.
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
+-- memory/context
+-- structured tools
+-- structured model output
+-- asynchronous lifecycle/cancellation
+-- workspaces and multi-agent coordination
```

A host may use one configured agent or create many independent runtime instances from reusable profiles.

## Basic usage

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

Plain string messaging is the convenience entry point. The architecture is moving toward a canonical generic execution request that can carry host-supplied context, host correlation, execution requirements, and optional structured-output contracts without embedding host-domain concepts in HAgent.Core.

See the architecture and plan documents for runtime, memory, tools, context, authorization, storage, and multi-agent design.

## Current capabilities

The verified foundation includes:

- provider/model routing and capability discovery;
- execution lifecycle, retries, timeout, cancellation, and diagnostics;
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
- HAgent-owned SQL Server/MySQL database bootstrap foundations.

## Generic host integration target

HAgent is designed to be the generic LLM cognition/execution layer for host software. The host remains authoritative for domain state, lifecycle, scheduling, persistence, authorization, and side effects.

The generic integration target includes:

- arbitrary bounded host input/context;
- long-lived independent runtime instances;
- separate runtime memory ownership;
- host-supplied correlation identities;
- cancellation, timeout, and safe late-completion handling;
- host-defined structured output schemas with validation;
- host-owned tools and capability execution;
- concurrent execution across independent runtime instances;
- optional persistence of generic runtime identity and lifecycle metadata;
- optional multi-agent coordination and workspace communication.

HAgent must not require a host-specific domain object model, event system, command system, scheduler, authorization framework, or UI framework.

## HAgent storage

HAgent persists its own internal data separately from the host application's business database.

Supported storage backends are:

- **File** — HAgent-owned files beneath the host executable in `HAgentData`;
- **SQL Server** — a dedicated HAgent-owned database, normally named `<application-name>-ai`;
- **MySQL** — a dedicated HAgent-owned database, normally named `<application-name>-ai`.

HAgent storage is for providers, agents, tools, memory, conversations, skills, wiki/content, runtime metadata, and other HAgent-owned records. A storage backend must never be treated as permission to inspect or modify the host application's business database.

Database passwords are kept outside ordinary configuration through the secret boundary.

## WinForms Context

`HAgent.WinForms` can attach to a Form or arbitrary control tree such as a UserControl. It can inspect controls, bindings, native data sources, relationships, custom-control metadata, and bounded application objects when the host's permission policy allows it.

The public concept is **UI Context / Control Adapters**, not generic form serialization.

`DataTable` is optional. Native/bound sources, lazy adapters, paging, projections, and bounded extraction are preferred.

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

Permissions, authorization, approvals, limits, cancellation, and host-side validation remain outside model instructions. HAgent database storage is dedicated to HAgent's own persistence and does not provide implicit access to host application tables. Structured data contracts are not raw SQL access.

## Example application

`HAgent.Example` is the manual developer/verification application, separate from `HAgent.Tests`.

Meaningful capabilities should have runnable Example verification using public APIs and a reproducible C# snippet.

## Project structure

- `HAgent.Core` — provider-neutral models, runtime, context, memory, tools, and future coordination.
- `HAgent.Providers.OpenAICompatible` — OpenAI-compatible provider transport and capabilities.
- `HAgent.Storage.File` — file configuration, protected secrets, memory, conversations, and tool definitions.
- `HAgent.Storage.SqlServer` — HAgent-owned SQL Server persistence and schema bootstrap.
- `HAgent.Storage.MySql` — HAgent-owned MySQL persistence and schema bootstrap.
- `HAgent.WinForms` — management UI and WinForms UI Context/control adapters.
- `HAgent.Example` — manual verification host.
- `HAgent.Tests` — automated tests.

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

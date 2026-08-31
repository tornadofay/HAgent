# HAgent

**Lightweight, provider-neutral AI agent runtime for .NET applications.**

HAgent provides the infrastructure needed to connect applications to LLMs without forcing a specific application architecture. It supports simple chat today and is being extended for business applications, games, simulations, and multi-agent environments.

> Status: **0.8 Data Access + Authorization + Internal Storage — active**
>
> Targets: **.NET Framework 4.8.1 and .NET 9**.

## What HAgent provides

```text
Providers / models
        |
Agent profiles
        |
Runtime agent instances
        |
+-- memory/context
+-- structured tools
+-- UI/data/application context
+-- asynchronous execution
+-- future workspaces and multi-agent coordination
```

A host may use one configured agent or create many independent runtime instances from reusable profiles.

## Basic usage

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

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
- HAgent-owned SQL Server/MySQL database bootstrap foundations with schema-version metadata.

## HAgent storage

HAgent persists its own internal data separately from the host application's business database.

Supported storage backends are:

- **File** — application-specific files beneath the host executable in `HAgentData`;
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

## HWorld

HWorld is an external consumer target. It owns its world, physics, simulation time, sensors, scheduling, rendering, and action validation. HAgent supplies generic agent execution, context, memory, tools, and future coordination.

HAgent must not depend on HWorld or contain HWorld-specific types or simulation logic.

The minimum HWorld integration point is the asynchronous runtime-agent boundary: caller-supplied observation/context in, provider-neutral decision/tool output out, with cancellation, timeout, correlation, and stale-result handling.

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

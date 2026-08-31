# HAgent

**Lightweight, provider-neutral AI agent runtime for .NET applications.**

HAgent is designed to make connecting applications to LLMs easier without forcing the application into one AI architecture. It can be used for ordinary chat, business applications, games, simulations, or other software that needs one or many independent agents.

> Status: **0.8 planning — 0.7 WinForms UI Context + Data Discovery complete; 0.8 Data Access + Authorization next**
>
> Current development targets: **.NET Framework 4.8.1 and .NET 9**.

## Core concepts

```text
Provider
 ├─ endpoint / connection
 ├─ protected secret reference
 ├─ default model
 └─ optional shared system prompt

Agent profile
 ├─ provider preferences
 ├─ model override
 ├─ system prompt
 ├─ generation settings
 └─ tool references

Runtime agent instance
 ├─ unique runtime identity
 ├─ profile reference
 ├─ scope / host context
 ├─ independent memory ownership
 └─ independent execution lifecycle

Workspace
 ├─ user participants
 ├─ agent participants
 ├─ default recipient
 ├─ direct addressing
 └─ optional agent-to-agent coordination

Runtime
 ├─ routing
 ├─ capabilities
 ├─ execution lifecycle
 ├─ retry / timeout / cancellation
 ├─ snapshots / diagnostics
 └─ bounded tool loop

Memory
 ├─ explicit facts/preferences
 ├─ task/event records
 ├─ episodic experiences
 ├─ bounded context selection
 └─ replaceable storage

Tools
 ├─ definition
 ├─ type
 ├─ JSON Schema
 ├─ registry / handler
 └─ result / observation

WinForms Context
 ├─ explicit domain abstractions
 ├─ automatic UI/data discovery
 ├─ permission policy
 ├─ stable form/control-tree identity
 ├─ safe read capabilities
 └─ future write/invoke automation

Data Access
 ├─ explicit field projection
 ├─ structured filters/sorts
 ├─ bounded paging
 ├─ application-owned adapters
 └─ future restricted SQL Server/MySQL adapters
```

## General-purpose usage

A host may use only a single agent:

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

Or it may create multiple runtime agent instances from reusable configured profiles. Runtime instances are intended for live application contexts and do not automatically become permanent configuration records.

## Shared workspaces

A future HAgent workspace can contain a user and multiple agent participants. The workspace does not broadcast each user message to every agent.

```text
User message without an explicit target
        ↓
workspace default recipient

User message with an explicit target
        ↓
that agent only

Manager/coordinator
        ↓
explicit delegation
        ↓
specialist agent
```

Addressing syntax is a presentation concern; the Core model represents sender, recipient, correlation, causation, and routing policy explicitly.

## Capabilities and response normalization

HAgent does not assume a model supports a feature simply because a provider returns its model ID. Capabilities are `Supported`, `Unsupported`, or `Unknown` and can carry source, confidence, observation time, and notes.

Normalized responses can contain:

```text
Text
Reasoning
RawText
StructuredOutputJson
ToolCalls
NormalizedUsage
Usage
ProviderMetadata
```

Explicit provider reasoning fields such as `reasoning_content` are kept separate from ordinary assistant text. Embedded `<think>` markup is diagnostic evidence, not proof of native reasoning support.

OpenAI-compatible providers support real SSE streaming through `HAgentClient.StreamAsync(...)`.

## Tools

Tool configuration does **not** execute arbitrary C# code. The tool definition and executable handler are separate.

```text
Tool definition
   = what the model is allowed to request

Tool handler
   = what the host is actually permitted to execute
```

Initial tool types:

| Type | Implementation source |
|---|---|
| Built-in | HAgent |
| Application tool | Host application registration |
| Declarative tool | Restricted configuration-driven handler |
| UI tool | HAgent.WinForms |
| SQL Server tool | Restricted SQL Server layer |
| MySQL tool | Restricted MySQL layer |

Executable handlers are runtime-owned and are never persisted as configuration data.

## Memory and low-resource design

Memory is application state managed by HAgent; it is not a claim that the model itself has persistent memory.

The design must work without:

- a local GPU;
- a local embedding model;
- a vector database;
- a large resident memory index.

Vector/semantic memory is optional.

## WinForms UI Context

“Form serialization” is not the public subsystem name. HAgent supports both explicit domain abstractions and automatic UI/data discovery.

Automatic mode can inspect a live form or attached control tree, including a `UserControl`, controls, bindings, data sources, shared-source relationships, custom-control metadata, and bounded application-object context when the host enables the relevant permission policy.

The UI layer also provides a provider-neutral structured data-query contract with explicit fields, scalar filters, sorting, and bounded paging. The contract contains no SQL, arbitrary expressions, or executable callbacks. Real database execution remains a later restricted adapter layer.

The same boundary is intended to support future HControl/BaseForm adapters and other interactive surfaces such as GDI, DirectX, or Unity without making those platforms dependencies of `HAgent.Core`.

## HWorld compatibility

HWorld is a planned external consumer of HAgent. HWorld owns world state, physics, simulation time, perception, action validation, rendering, and world scheduling. HAgent supplies generic cognition infrastructure such as model/provider execution, tools, memory/context integration, asynchronous execution, and agent coordination.

HAgent must not depend on HWorld or contain HWorld-specific types, actions, physics, rendering, or simulation time. The HWorld integration belongs in HWorld at its external cognition/decision boundary.

The HAgent roadmap therefore treats HWorld as a concrete compatibility target for:

- multiple concurrent agent runtime instances;
- independent profiles, models, and memories;
- asynchronous external scheduling;
- immutable caller-provided observations;
- cancellation, timeouts, correlation, and stale-result protection;
- structured tools/actions with host-side validation;
- compact context and usage telemetry.

## Example application

`HAgent.Example` is the manual integration/verification host and is separate from `HAgent.Tests`.

Every Example feature is intended to show editable input where relevant, expected behavior, and a copyable C# reproduction snippet beside the test.

The Example form is split into focused partial files so it can continue growing without becoming a monolithic test harness.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Models, abstractions, sessions, runtime, memory/context, tools |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible transport, model discovery, capabilities, normalization, streaming, tool transport |
| `HAgent.Storage.File` | JSON configuration, DPAPI secrets, JSONL memory, persistent conversations, tool definitions |
| `HAgent.Storage.SqlServer` | SQL Server persistence foundation and future restricted database tools |
| `HAgent.Storage.MySql` | MySQL persistence foundation and future restricted database tools |
| `HAgent.WinForms` | HAgent management UI and WinForms UI Context/control adapters |
| `HAgent.Example` | Manual feature verification application |
| `HAgent.Tests` | Automated tests |

## Supported targets

- .NET Framework 4.8.1
- .NET 9

.NET 10 is intentionally deferred until migration to a compatible Visual Studio environment.

## Project-state documents

Documentation is maintained as source files under `docs/plan/` and `docs/roadmap/`. GitHub Actions rebuild the generated root `plan.md` and `roadmap.md` from those smaller source files. `README.md`, architecture docs, `plan.md`, `roadmap.md`, and `AGENTS.md` are treated as part of the project state.

When a milestone changes, update the relevant small source document in the same change as the implementation. Do not wait for a later cleanup pass.

## License

MIT — see [LICENSE](LICENSE).

# HAgent

**Lightweight, provider-neutral AI agent runtime for .NET desktop applications.**

HAgent is designed for applications that need more than a single AI API call: multiple providers, agents, sessions, persistent memory, tools, controlled WinForms automation, collaboration, and long-running work without forcing a heavyweight AI framework into every deployment.

> Status: **0.5 — Tools and Agent Loop / active development**
>
> Current development targets: **.NET Framework 4.8.1 and .NET 9**.

## Core concepts

```text
Provider
 ├─ endpoint / connection
 ├─ protected secret reference
 ├─ default model
 └─ optional shared system prompt

Agent
 ├─ provider preferences
 ├─ model override
 ├─ system prompt
 ├─ generation settings
 └─ tool references

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
 ├─ safe read capabilities
 └─ future write/invoke automation
```

## Basic API

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

Persistent sessions:

```csharp
var session = ai.CreateSession("assistant", "conversation-42");
await session.SendAsync("Remember that our project code name is HAgent.");

var reopened = await ai.OpenSessionAsync("assistant", "conversation-42");
var history = await reopened.ReadAsync();
```

Explicit memory:

```csharp
await ai.RememberAsync(
    "assistant",
    "The customer's preferred language is Arabic.");

var memories = await ai.RecallAsync("assistant", "preferred language");
```

Controlled execution:

```csharp
var execution = await ai.ExecuteAsync(
    "assistant",
    "Perform this task.",
    new AgentExecutionOptions
    {
        Timeout = TimeSpan.FromSeconds(60),
        MaxProviderAttempts = 3,
        MaxRetriesPerProvider = 1
    });
```

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

Extension tools are deliberately deferred to a future extensibility milestone.

The tool foundation includes:

- `AiTool` + explicit `AiToolType`.
- `IAgentTool` and `IToolRegistry`.
- `InMemoryToolRegistry` and `DelegateAgentTool`.
- Tool definition persistence through File/SQL Server/MySQL stores.
- Per-agent tool assignment through `AiAgent.ToolIds`.
- Dependency-free JSON Schema validation before handlers run.
- Provider-neutral tool-call transport and bounded multi-turn execution.
- Live OpenAI-compatible/Groq tool-loop verification.

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

Explicit mode lets a host expose concepts such as:

```text
Customer
 ├─ Name
 ├─ Contact
 └─ Invoices
```

Automatic mode can inspect a live form, its controls, bindings, and data sources when the host enables the relevant permission policy.

```csharp
var host = HAgentHost.Attach(form, registry, true, permissions);
```

Current read-only tools include:

```text
ui.inspect
ui.read_control
ui.read_data
```

The permission policy distinguishes automatic discovery, control reads, data reads, writes, and invocation. Safe defaults do not grant write/invoke access.

For `DataGridView`, HAgent prefers bound/native data sources and adapts lazily. `DataTable` is optional and must not be the architecture default when another lighter representation is better.

The same abstraction boundary is intended to support future HControl/BaseForm adapters and other interactive surfaces such as GDI, DirectX, or Unity without making those platforms dependencies of `HAgent.Core`.

## Example application

`HAgent.Example` is the manual integration/verification host and is separate from `HAgent.Tests`.

Every Example tab is intended to show:

- editable input/message;
- expected behavior and notes;
- a copyable C# reproduction snippet beside the input;
- global agent selection where applicable.

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

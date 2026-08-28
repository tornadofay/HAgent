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
 └─ future tool loop

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

WinForms Host
 ├─ UI Context / Control Adapters
 ├─ safe read/write capabilities
 ├─ floating assistant
 └─ form-aware memory/session binding
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

The current tool foundation provides:

- `AiTool` + explicit `AiToolType`.
- `IAgentTool` and `IToolRegistry`.
- `InMemoryToolRegistry` and `DelegateAgentTool`.
- `HAgentClient` tool registration, lookup, definition inspection, and direct execution.
- Dependency-free JSON Schema validation on tool execution.
- Provider-neutral `SendWithToolsAsync(...)`.
- OpenAI-compatible tool-definition transport.

Invalid tool schemas are rejected during registration, and model-provided arguments are validated before the handler runs.

## Memory and low-resource design

Memory is application state managed by HAgent; it is not a claim that the model itself has persistent memory.

The design must work without:

- a local GPU;
- a local embedding model;
- a vector database;
- a large resident memory index.

Vector/semantic memory is optional.

For WinForms data extraction, HAgent will choose the lightest useful representation. It should prefer native/bound data sources over scraping visible cells, adapt lazily, and avoid unnecessary copies. `DataTable` is an available representation, not an architectural requirement.

## WinForms direction

The desktop-specific layer is **UI Context / Control Adapters**, not merely “form serialization”. The target API is conceptually:

```csharp
var attached = HAgent.WinForms.HAgentHost.Attach(ai, this);
```

The bridge will eventually support safe inspection, read/write/invoke capabilities, form-aware sessions, cross-form memory, and an HAgent floating assistant.

Attaching an agent never automatically grants write or execute permission.

## Example application

`HAgent.Example` is the manual integration/verification host and is separate from `HAgent.Tests`.

Current examples cover configuration, messaging, sessions, persistent sessions, memory, task/event memory, episodic memory, runtime execution, capabilities, response normalization, streaming, live streaming, tool registry, tool schema validation, and provider tool-definition transport.

The Example form is split into focused partial files so it can continue growing without becoming a monolithic test harness.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Models, abstractions, sessions, runtime, memory/context, tools |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible transport, model discovery, capabilities, normalization, streaming, tool transport |
| `HAgent.Storage.File` | JSON configuration, DPAPI secrets, JSONL memory, persistent conversations |
| `HAgent.Storage.SqlServer` | SQL Server persistence foundation |
| `HAgent.Storage.MySql` | MySQL persistence foundation |
| `HAgent.WinForms` | HAgent management UI and future UI Context/control adapters |
| `HAgent.Example` | Manual feature verification application |
| `HAgent.Tests` | Automated tests |

## Supported targets

- .NET Framework 4.8.1
- .NET 9

.NET 10 is intentionally deferred until migration to a compatible Visual Studio environment.

## Development state

Completed foundation milestones:

```text
0.1  Foundation
0.2  Runtime Foundation
0.3  Memory + Context
0.4  Provider Capabilities + Response Normalization
```

Active:

```text
0.5  Tools + Agent Loop
```

Current 0.5 focus:

```text
JSON Schema validation
        ↓
provider tool transport
        ↓
assistant tool-call + tool-result messages
        ↓
provider-neutral multi-turn tool loop
        ↓
permissions / budgets / audit
```

See [roadmap.md](roadmap.md) for the full dependency-ordered roadmap and [plan.md](plan.md) for the implementation ledger.

## Project-state documents

`README.md`, `roadmap.md`, `plan.md`, and `AGENTS.md` are maintained together as part of the project state.

## License

MIT — see [LICENSE](LICENSE).

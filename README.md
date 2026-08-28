# HAgent

**Lightweight, provider-neutral AI agent runtime for .NET desktop applications.**

HAgent is designed for applications that need more than a single AI API call: multiple providers, agents, sessions, persistent memory, tools, controlled automation, collaboration, and long-running work—without forcing a heavyweight AI framework into every deployment.

> Status: **0.5 — Tools and Agent Loop / active development**
>
> Current development targets: **.NET Framework 4.8.1 and .NET 9**.

## Why HAgent exists

AI integration quickly becomes application-specific plumbing: API keys, endpoints, model names, system prompts, provider switching, persistence, conversations, tools, UI automation, permissions, approvals, and task execution end up scattered through the application.

HAgent centralizes that foundation behind a small set of separable concepts:

```text
Provider
  ├─ connection / endpoint
  ├─ secret reference
  ├─ default model
  └─ optional shared system prompt

Agent profile
  ├─ preferred/additional providers
  ├─ optional model override
  ├─ system prompt
  ├─ generation settings
  └─ tool references

Runtime
  ├─ execution snapshot
  ├─ provider routing
  ├─ capabilities
  ├─ timeout / cancellation
  ├─ retry / backoff
  ├─ budgets
  ├─ lifecycle state
  └─ diagnostics

Conversation
  ├─ ordered session history
  ├─ optional persistent session ID
  ├─ reopenable conversation store
  └─ context-window policy

Memory
  ├─ explicit remember / recall / forget
  ├─ automatic-memory policy
  ├─ task / event memory
  ├─ compact episodic experiences
  ├─ task / agent / user / application scopes
  ├─ provenance
  └─ replaceable storage/retrieval providers

Tools
  ├─ capability definition
  ├─ schema / validation
  ├─ executable host handler
  ├─ registry
  └─ observation/result

Host integration
  ├─ WinForms UI context
  ├─ control adapters
  ├─ explicit UI tools
  └─ attached assistant UI
```

## Basic API

Applications talk to an agent rather than directly to a vendor:

```csharp
var response = await ai.SendAsync(
    agentId: "assistant",
    message: "Summarize this text in three sentences.");

Console.WriteLine(response.Text);
```

For conversational use:

```csharp
var session = ai.CreateSession("assistant");
await session.SendAsync("Hello");
await session.SendAsync("Now explain recursion simply.");

var history = await session.ReadAsync();
```

For persistent conversations:

```csharp
var conversations = new HAgent.Storage.File.FileConversationStore(
    @"C:\MyApp\ai\conversations");

var ai = new HAgent.Runtime.HAgentClient(
    store,
    secrets,
    adapters,
    router: null,
    memory: null,
    conversations: conversations);

var session = ai.CreateSession("assistant", "conversation-42");
await session.SendAsync("Remember that our project code name is HAgent.");

var reopened = await ai.OpenSessionAsync("assistant", "conversation-42");
var history = await reopened.ReadAsync();
```

For explicit durable memory:

```csharp
var memory = new HAgent.Storage.File.FileMemoryStore(
    @"C:\MyApp\ai\memory.jsonl");

var ai = new HAgent.Runtime.HAgentClient(
    store,
    secrets,
    adapters,
    router: null,
    memory: memory);

await ai.RememberAsync(
    "assistant",
    "The customer's preferred language is Arabic.");

var memories = await ai.RecallAsync(
    "assistant",
    "preferred language");
```

For controlled runtime execution:

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

For model capabilities:

```csharp
var capabilities = await ai.GetModelCapabilitiesAsync(
    providerId: "groq",
    model: "qwen/qwen3.6-27b");

if (capabilities.Get(AiCapability.ToolCalling) == CapabilitySupport.Supported)
{
    // Tool calling is explicitly supported by the adapter/provider.
}
```

A capability can be `Supported`, `Unsupported`, or `Unknown`. `Unknown` is intentional: HAgent must not infer that a discovered model supports tools, vision, reasoning, embeddings, streaming, or structured output merely because its model ID exists.

Capability results also carry evidence describing the source and confidence of the determination. Sources can include provider metadata, adapter knowledge, explicit user configuration, and runtime observation.

## Current status

HAgent 0.1 established the configuration, storage, WinForms management UI, provider adapter, session, provider testing, and Example-host foundation.

HAgent 0.2 established the runtime foundation: execution state, snapshots, provider routing, cancellation/timeouts, retries/backoff, lifecycle events, diagnostics, and structured provider/model/account failure reporting.

HAgent 0.3 added persistent conversations, low-resource memory, explicit/automatic memory policy, typed task/event records, compact episodic experiences, relevance ranking, and bounded context selection.

HAgent 0.4 established provider capability and response-normalization foundations: tri-state capabilities, evidence/provenance, capability caching, suitability checks, separate reasoning/raw/structured/tool-call/usage metadata, provider error diagnostics, provider-neutral streaming, OpenAI-compatible SSE streaming, and live Example verification.

HAgent 0.5 is now active. The first tool foundation is implemented: `AiTool`, `IAgentTool`, `IToolRegistry`, `InMemoryToolRegistry`, `DelegateAgentTool`, plus `HAgentClient` registration/lookup/direct-execution APIs and a deterministic Example verification tab. The next work is JSON Schema validation, provider tool-definition transport, and the model↔tool execution loop.

## Architecture principles

### Provider versus agent

A provider answers **“how do I connect?”**.

An agent profile answers **“how should this AI behave?”**.

Agents can reference multiple providers, and the runtime can route between compatible candidates. Shared provider prompts are inherited only when explicitly enabled by the agent model; there is no hidden precedence chain.

### Capabilities are explicit

A model identifier is not a capability contract. Capability support is represented as `Supported`, `Unsupported`, or `Unknown` for each feature.

```text
Chat
Streaming
ToolCalling
StructuredOutput
Vision
AudioInput
AudioOutput
Embeddings
Reasoning
```

Capability evidence additionally records support, source, confidence, observation time, and an optional note.

### Agent profile versus scope

HAgent does not need separate incompatible agent classes for “global agent”, “form agent”, “session agent”, or “task agent”. The agent profile and its runtime binding/lifetime are separate concepts.

Planned bindings include:

```text
Application / Global
Form-bound
Session-bound
Task-bound
Ephemeral execution
```

### Runtime versus configuration

Configuration can change while an execution is running. An execution captures an `AgentExecutionSnapshot` so running work is not silently altered by later edits or deletions.

### Tools are capabilities, not arbitrary access

A tool definition describes a permitted capability. The host owns the executable handler and real-world side effect.

The intended loop is:

```text
User
 ↓
Agent
 ↓
Model
 ↓
Tool call?
 ├─ No → Response
 │
 └─ Yes
      ↓
 Validate
      ↓
 Permission / Guardrail
      ↓
 Optional human approval
      ↓
 Execute
      ↓
 Tool result / observation
      ↓
 Model
      ↓
 ...
```

The current tool layer already separates persisted tool definitions from executable handlers. Provider transport and the full multi-turn tool loop are the next implementation pieces.

### Prompts are not security boundaries

A system prompt can describe behavior, but it must never be the enforcement mechanism for destructive or sensitive actions. Permissions, guardrails, approval, validation, and budgets are the actual boundaries.

### Memory is application state

HAgent does not claim that a model itself permanently remembers anything. Memory is explicit state stored by HAgent, retrieved according to policy, and supplied to the model when appropriate.

The memory layers distinguish facts/preferences from task/event records and compact episodic experiences. The default memory design works without a local GPU, vector database, or heavy local embedding model.

### Stored history versus context

A long conversation can be stored completely while each provider request receives a bounded context selected by policy. This prevents history growth from automatically becoming unbounded prompt growth.

### Provider responses are normalized

`AIResponse` remains backward-compatible through `Text`, while normalized response state can additionally contain:

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

Provider-exposed reasoning remains separate from ordinary assistant text when explicitly supplied. `<think>` markup is diagnostic evidence only; it is not universally assumed to be native reasoning metadata.

### Streaming

Streaming is optional at the provider boundary. OpenAI-compatible providers currently use SSE through `IProviderStreamingAdapter`. Non-streaming adapters continue to use normal `SendAsync`.

### Observability is part of the runtime

Agentic work needs more than one final exception. HAgent is designed around correlation-based diagnostics for provider calls, model turns, tools, memory, guardrails, approvals, and agent handoffs while keeping secrets and sensitive payloads redacted by default.

## WinForms-specific architecture

WinForms gives HAgent a capability that generic server/web agent libraries cannot provide: the agent can understand and safely interact with the actual desktop application's UI.

This is implemented as a **UI Context / Control Adapter** layer inside `HAgent.WinForms`, not inside `HAgent.Core`.

“Form serialization” is only one operation of this system. The broader architecture is:

```text
Form / Control tree
       ↓
UI Context
       ↓
Control adapters
       ↓
Neutral state / data representation
       ↓
AI context
```

For actions:

```text
AI tool request
       ↓
validated UI capability
       ↓
control adapter
       ↓
UI-thread execution
       ↓
structured result
```

### Data representation rule

Always prefer the lightest representation that preserves the information required for the operation. Preserve bound/native data sources where practical, adapt lazily, avoid unnecessary copies, and materialize `DataTable` only when it is already the native source, explicitly required, or actually the most efficient representation.

For `DataGridView`, prefer the underlying bound source over scraping visible cells. For large datasets, paging, streaming, projection, or native source access should win over eager duplication.

### Form attachment

The target developer experience is conceptually:

```csharp
var attached = HAgent.WinForms.HAgentHost.Attach(ai, this);
```

The bridge should support form/control discovery, safe read access, approved write/invoke operations, form-aware context, agent/session selection, and an HAgent floating assistant/flyout.

Attaching an assistant never automatically grants write or execute access.

### Cross-form memory

Forms can contribute information to memory with provenance such as form/session/task/application identifiers. Other forms can recall that information only when the relevant scope and policy permit it. Provenance is not authorization.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Core models, abstractions, sessions, runtime, memory/context, and tool foundations |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible transport, model discovery, capability baseline, response normalization, and SSE streaming |
| `HAgent.Storage.File` | JSON configuration, DPAPI secrets, JSONL memory, persistent conversations |
| `HAgent.Storage.SqlServer` | SQL Server persistence/schema foundation |
| `HAgent.Storage.MySql` | MySQL persistence/schema foundation |
| `HAgent.WinForms` | Configuration/development UI, HAgent controls, future UI context/control adapters |
| `HAgent.Example` | Manual integration and feature verification application |
| `HAgent.Tests` | Automated test project |

## Supported targets

Current targets:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is intentionally not a current target. It is planned after moving development to a Visual Studio release that supports it.

## HAgent.Example

`HAgent.Example` is the manual integration/verification host and is intentionally different from `HAgent.Tests`.

- `HAgent.Tests` verifies behavior automatically.
- `HAgent.Example` lets a developer run completed features manually and inspect actual behavior.

Current examples include provider-backed messaging, sessions, persistent sessions, memory, automatic memory, context budgeting, task/event memory, episodic memory, runtime execution, capabilities, response normalization, streaming contracts, live streaming, and tool registry execution.

The Example form uses focused partial files rather than one monolithic test form. New large feature areas should get their own focused Example source/component.

## Storage and low-resource design

### File configuration

Structured configuration is stored as JSON. Secrets are stored separately and protected through Windows DPAPI.

### File memory

`FileMemoryStore` uses one JSON object per line and searches it incrementally. It does not require the entire memory store to be resident in RAM.

### Conversations

`FileConversationStore` stores each persistent session as its own JSON document. Opening a conversation reads that conversation only.

### SQL Server / MySQL

The database storage assemblies use direct ADO.NET rather than an ORM to keep the persistence layer predictable and small.

### Vector memory

Vector/embedding memory is optional. The base HAgent memory path must work without a GPU, local embedding model, vector database, or large in-memory index.

## Roadmap and tracking

See [roadmap.md](roadmap.md) for the ordered roadmap and [plan.md](plan.md) for the implementation ledger.

The immediate dependency chain is:

```text
Memory/context
   ↓
Provider capabilities + response normalization
   ↓
Tools + agent loop
   ↓
Guardrails + permissions + approval + budgets + observability
   ↓
WinForms UI Context + control adapters
   ↓
Agent scopes + chat
   ↓
Agent collaboration
   ↓
Tasks/workflows/autonomy
```

`README.md`, `roadmap.md`, `plan.md`, and `AGENTS.md` are maintained as part of project state.

## Contributing

Read [AGENTS.md](AGENTS.md) before modifying the repository.

## License

MIT — see [LICENSE](LICENSE).

# HAgent

**Lightweight, provider-neutral AI agent runtime for .NET desktop applications.**

HAgent is designed for applications that need more than a single AI API call: multiple providers, agents, sessions, persistent memory, tools, controlled automation, collaboration, and long-running work—without forcing a heavyweight AI framework into every deployment.

> Status: **0.4 — Provider Capabilities and Response Normalization / active development**
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
  ├─ permissions / approval
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

Capability results also carry evidence describing the source and confidence of the determination. Sources can include provider metadata, adapter knowledge, explicit user configuration, and runtime observation. This allows a host application to distinguish a documented provider capability from an adapter baseline or an unverified observation.

## Current status

HAgent 0.2 established the runtime foundation: execution state, snapshots, provider routing, cancellation/timeouts, retries/backoff, lifecycle events, diagnostics, and lightweight memory abstractions.

HAgent 0.3 added persistent conversations, persistent low-resource memory, explicit/automatic memory policy, task/event records, compact episodic experiences, relevance ranking, and bounded context selection.

The active 0.4 milestone now adds explicit model/provider capabilities and normalized response fields. The first slices provide tri-state capabilities, capability caching/suitability checks, capability evidence/provenance, and separate reasoning/raw/provider metadata, while preserving the existing `AIResponse.Text` API. The OpenAI-compatible adapter reports Chat as supported using adapter evidence and leaves optional capabilities unknown unless established. Explicit provider `reasoning_content` is kept separate; `<think>` markup is detected for diagnostics but is not automatically treated as native reasoning.

The next steps are richer provider capability discovery, capability-aware selection for tools/vision/structured output/audio/reasoning, structured output and tool-call normalization, streaming, and application-level reasoning visibility policy before the Tools milestone.

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

Capability evidence additionally records:

```text
Support
Source
Confidence
ObservedAt
Optional note
```

This prevents a discovered classifier or guard model from being accidentally treated as a conversational model and prevents the framework from hiding how a capability claim was established.

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

A single global agent may therefore participate in many forms while each form/session maintains separate state and policy.

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

The runtime will enforce limits such as execution time, tool calls, turns, memory retrieval, handoffs, and workflow depth.

### Prompts are not security boundaries

A system prompt can describe behavior, but it must never be the enforcement mechanism for destructive or sensitive actions.

Permissions, guardrails, approval, validation, and budgets are the actual boundaries.

### Memory is application state

HAgent does not claim that a model itself permanently remembers anything.

Memory is explicit state stored by HAgent, retrieved according to policy, and supplied to the model when appropriate.

The current memory layers distinguish ordinary facts/preferences from task/event records and compact episodic experiences. Episodes summarize meaningful completed work without requiring the full conversation or event stream to be sent to the model again.

The default memory design is intentionally usable on machines with no GPU and only a few gigabytes of RAM. Vector memory is optional, not a prerequisite.

### Stored history versus context

A long conversation can be stored completely while each provider request receives a bounded context selected by policy.

This prevents history growth from automatically becoming unbounded prompt growth.

### Provider responses are normalized

`AIResponse` remains backward-compatible through `Text`, while normalized response state can additionally contain:

```text
Text
Reasoning
RawText
Usage
ProviderMetadata
```

Provider-exposed reasoning must remain separate from ordinary assistant text when the provider explicitly exposes it. HAgent does not assume that `<think>...</think>` markup universally represents native reasoning metadata.

### Observability is part of the runtime

Agentic work needs more than one final exception. HAgent will expose correlation-based diagnostics around provider calls, model turns, tools, memory, guardrails, approvals, and agent handoffs while keeping secrets and sensitive payloads redacted by default.

## WinForms-specific architecture

WinForms gives HAgent a capability that generic web/server agent frameworks do not have: the agent can understand and safely interact with the actual desktop application UI.

This will be implemented as a **UI Context / Control Adapter** layer inside `HAgent.WinForms`, not inside `HAgent.Core`.

### It is not just serialization

“Serialization” is one operation of the system—not the name of the whole feature.

The broader architecture is:

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

and for actions:

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

### Automatic WinForms data handling

The adapter layer should understand common controls and data sources without applications having to repeatedly write conversion code.

For example, a `DataGridView` should prefer its bound data source when one exists, and use the lightest representation that is appropriate for the requested operation.

Known sources should be adapted lazily. `DataTable` is a compatibility representation, not an architectural requirement. For large datasets, paging, streaming, projection, or a native/source representation should be preferred whenever it is more efficient and avoids unnecessary copies.

Only when a supported bound data source is unavailable should the adapter fall back to reading visible grid state.

Planned built-in adapters include:

```text
Form
UserControl
Custom controls (explicit adapter)
TextBox / RichTextBox
ComboBox
Button
CheckBox / RadioButton
DateTimePicker
NumericUpDown
ListBox / ListView
TreeView
DataGridView
DataTable
BindingSource
CurrencyManager
IList / collection sources
```

### Form attachment

The target experience is conceptually:

```csharp
var attached = HAgent.WinForms.HAgentHost.Attach(ai, this);
```

The exact API name may change, but the design goal is stable:

```text
Form1
 └─ HAgent assistant button / flyout
       ├─ current agent
       ├─ current session
       ├─ what the agent can read
       ├─ what the agent can change
       ├─ context preview
       └─ tool activity
```

Attaching an AI assistant does **not** automatically grant write/execute access.

### UI capabilities

Planned built-in capabilities/tools include:

```text
ui.inspect
ui.read_control
ui.read_data
ui.write_control
ui.move_control
ui.resize_control
ui.invoke
ui.enable_control
ui.disable_control
```

These operations will be permission-controlled, cancellable, observable, and optionally approval-gated.

### Cross-form memory

A form can contribute information to memory, but the information should carry provenance such as:

```text
Application ID
Form ID
Session ID
Task ID
Source type
Timestamp
```

Another form can recall that information only when the relevant scope and policy permit it.

Example:

```text
Form1
  ↓
explicit memory / application scope
  ↓
Form2
  ↓
allowed agent recall
```

Provenance describes where data came from; it is not an authorization mechanism.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Core models, abstractions, sessions, runtime, memory/context foundations |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible transport, model discovery, and capability baseline |
| `HAgent.Storage.File` | JSON configuration, DPAPI secrets, JSONL memory, persistent conversations |
| `HAgent.Storage.SqlServer` | SQL Server persistence/schema foundation |
| `HAgent.Storage.MySql` | MySQL persistence/schema foundation |
| `HAgent.WinForms` | Configuration/development UI, shared HAgent controls, future UI context/control adapters |
| `HAgent.Example` | Manual integration and feature verification application |
| `HAgent.Tests` | Automated test project |

## Supported targets

Current targets:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is intentionally not a current target. It is planned after moving the development environment to a Visual Studio release that supports it.

## HAgent.Example

`HAgent.Example` is the manual integration/verification host and is intentionally different from `HAgent.Tests`.

- `HAgent.Tests` verifies behavior automatically.
- `HAgent.Example` lets a developer run completed features manually and inspect actual behavior.

Current examples include configuration, provider-backed messaging, session history, persistent sessions, explicit memory, automatic memory, context budgeting, task/event memory, episodic memory, runtime execution, and model capability inspection.

The Example form uses focused partial files rather than one monolithic test form. New large feature areas should get their own focused Example source file/component.

## Storage and low-resource design

### File configuration

Structured configuration is stored as JSON. Secrets are stored separately and protected through the default Windows DPAPI implementation.

### File memory

`FileMemoryStore` uses one JSON object per line in a JSONL file and searches it incrementally. It does not require the entire memory store to be resident in RAM.

### Conversations

`FileConversationStore` stores each persistent session as its own JSON document. Opening a conversation reads that conversation only.

### SQL Server / MySQL

The database storage assemblies use direct ADO.NET rather than an ORM to keep the persistence layer predictable and small.

### Vector memory

Vector/embedding memory is optional. HAgent's normal execution path must work without:

- a GPU;
- a local embedding model;
- a vector database;
- a large in-memory index.

Remote embeddings and optional companion packages can be added when an application actually needs semantic retrieval.

## Roadmap

See [roadmap.md](roadmap.md) for the full ordered roadmap and [plan.md](plan.md) for the implementation ledger.

The near-term dependency chain is:

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
Chat + agent scopes
   ↓
Agent collaboration
   ↓
Tasks/workflows/autonomy
```

## Project tracking

`README.md`, `roadmap.md`, `plan.md`, and `AGENTS.md` are maintained as part of the project state. Meaningful architectural or milestone changes must keep them synchronized with the implementation.

## Contributing

Read [AGENTS.md](AGENTS.md) before modifying the repository.

## License

MIT — see [LICENSE](LICENSE).

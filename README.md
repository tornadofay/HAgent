# HAgent

**Lightweight AI provider and agent runtime for .NET desktop applications.**

HAgent gives a desktop application one small, provider-neutral API for configuring AI providers, defining agents, securely storing credentials, executing agent requests, and building toward memory, tools, collaboration, and application automation.

> Status: **0.3 — memory foundation / active development**
>
> Current development targets: **.NET Framework 4.8.1 and .NET 9**.

## Why HAgent exists

AI integration quickly becomes application-specific plumbing: API keys, endpoints, model names, system prompts, provider switching, persistence, settings screens, conversations, tools, and task execution end up scattered through the application.

HAgent centralizes that foundation behind a small model:

```text
Provider
  ├─ connection / endpoint
  ├─ secret reference
  ├─ default model
  └─ optional shared system instruction

Agent
  ├─ preferred provider
  ├─ optional additional providers
  ├─ optional model override
  ├─ system instruction
  ├─ generation settings
  └─ tool references

Runtime
  ├─ execution snapshot
  ├─ provider routing
  ├─ timeout / cancellation
  ├─ retry / backoff
  ├─ lifecycle state
  └─ diagnostics

Conversation
  ├─ in-memory session history
  ├─ optional persistent session ID
  ├─ reopenable conversation store
  └─ replaceable persistence provider

Memory
  ├─ explicit remember / recall / forget
  ├─ optional explicit conversation-memory policy
  ├─ scoped entries + metadata
  └─ replaceable storage providers
```

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

A persistent session saves successful turns after each exchange. Existing `CreateSession(agentId)` remains ephemeral and requires no conversation-store dependency.

For controlled execution:

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

Memory is explicit by design: HAgent does not silently persist every conversation message as a permanent fact.

## Current status

HAgent 0.2 establishes the runtime foundation needed for later agent capabilities. It includes execution state, execution snapshots, provider routing, cancellation/timeouts, retry/backoff, lifecycle events, diagnostics, and a lightweight memory abstraction.

The current 0.3 line adds persistent low-memory file memory plus optional persistent conversation sessions. File-backed conversation persistence uses one JSON document per session so reopening one conversation does not require loading unrelated conversations into memory. The default automatic-memory policy is deliberately conservative: it only promotes a user message when the user explicitly uses a memory trigger such as `Remember this:`. Context budgeting is also applied before provider submission so stored history can remain complete while provider requests stay bounded.

## Design principles

### Small core
`HAgent.Core` contains domain objects, abstractions, runtime, sessions, and lightweight in-memory infrastructure. It does not depend on WinForms, SQL Server, MySQL, a particular AI vendor, or a vector database.

### Provider versus agent
A provider answers **“how do I connect?”**.

An agent answers **“how should this AI behave?”**.

Providers can expose shared defaults such as a shared system instruction. Agents explicitly decide whether that instruction is inherited; there is no hidden prompt hierarchy.

### Runtime versus configuration
Configuration objects can change or be deleted while an execution is running. An execution captures an `AgentExecutionSnapshot` so runtime work operates against a stable view of the relevant agent/providers.

### Provider fallback is policy, not vendor logic
The runtime knows how to order candidate providers, limit attempts, classify broad failures, and back off. Provider adapters remain responsible for translating their own API protocol.

### Conversations are explicit state
An `AgentSession` owns the ordered user/assistant history. Persistence is opt-in through `IConversationStore`; when enabled, a stable session ID allows an application to reopen the same conversation after the original session/store instance is gone.

### Memory is externalized state
HAgent does not claim that an AI model permanently remembers anything. Memory is explicit application state retrieved and supplied as context. Automatic conversation-memory promotion is policy-driven and conservative; ordinary chat does not become long-term memory unless a policy decides it should. The default memory direction must work without a local GPU and without loading a large store entirely into RAM.

### Tools are controlled capabilities
A tool definition describes a capability; it is not executable code. The host application owns the actual handler and side effects. An AI model must never receive arbitrary access to controls, processes, files, databases, or reflection.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Core models, abstractions, sessions, runtime, and lightweight infrastructure |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible chat and model-catalog adapter |
| `HAgent.Storage.File` | JSON settings, DPAPI secrets, JSONL memory store, and persistent conversation store |
| `HAgent.Storage.SqlServer` | SQL Server persistence + schema bootstrap |
| `HAgent.Storage.MySql` | MySQL persistence + schema bootstrap |
| `HAgent.WinForms` | Designer-free configuration/development UI and shared HAgent UI helpers |
| `HAgent.Example` | Manual integration and feature verification application |
| `HAgent.Tests` | Automated test project; separate from the manual example host |

## Supported targets

Current targets:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is intentionally not a current build target. It is planned after the development environment moves to a Visual Studio release that supports it.

## Quick start

For the configuration/development UI, add `HAgent.WinForms` and call:

```csharp
HAgent.WinForms.AISettings.ShowMainAISettingsForm(this);
```

For application-owned runtime usage, add the assemblies you need:

```text
HAgent.Core
HAgent.Providers.OpenAICompatible
HAgent.Storage.File
```

Then:

```csharp
var store = new HAgent.Storage.File.FileAiStore(
    @"C:\MyApp\ai\settings.json");

var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(
    @"C:\MyApp\ai\secrets");

var ai = new HAgent.Runtime.HAgentClient(
    store,
    secrets,
    new[]
    {
        new HAgent.Providers.OpenAICompatible.OpenAICompatibleProviderAdapter()
    });

var result = await ai.SendAsync(
    "assistant",
    "Give me a concise status message.");
```

Default local storage uses:

```text
%LOCALAPPDATA%\HAgent\settings.json
%LOCALAPPDATA%\HAgent\secrets\
```

## HAgent.Example

`HAgent.Example` is the manual integration host for the project. It is intentionally different from `HAgent.Tests`:

- `HAgent.Tests` is for automated tests.
- `HAgent.Example` is for running the library and seeing how completed features behave.

Set `HAgent.Example` as the startup project and press **F5**. The example host is organized into feature tabs so new runtime capabilities can be added without turning the main form into a wall of controls.

Current examples include:

- **Messaging** — sends a real request through the configured agent/provider.
- **Session** — exercises in-memory conversation history forwarding.
- **Persistent Session** — creates a persistent session, discards the original store object, reopens the conversation by session ID, and verifies the history survives.
- **Runtime 0.2** — exercises execution IDs, state, timeout, provider-attempt limits, retries, and diagnostics.
- **Configuration** — verifies provider/agent persistence can be read by the host.
- **Memory** — verifies explicit persistent `remember` / `recall` behavior using the low-memory file store.
- **Automatic Memory** — verifies the default explicit-trigger policy promotes only messages that explicitly request memory.
- **Context Budget** — verifies deterministic bounded context selection without a provider tokenizer or AI call.

A shared **Global output** area displays the latest result across all examples. Each feature tab describes what it tests and the expected result.

Every major feature added to HAgent should eventually have a corresponding manual example here so it can be tested directly on a developer machine.

## Provider adapters

The first adapter is OpenAI-compatible. It supports the common `/chat/completions` request shape and `/models` discovery endpoint.

Model discovery currently returns model IDs supplied by the provider. It does not yet guarantee that every discovered model is suitable for chat, tools, embeddings, moderation, or other specific capabilities. Capability negotiation is a later provider milestone. A model such as Meta's Llama Prompt Guard is a classifier rather than a general conversational model, so it should not be used for the conversational examples.

The architecture intentionally permits separate adapters for providers such as Azure OpenAI, Anthropic, Google/Gemini, Ollama, LM Studio, local services, and custom enterprise endpoints. Provider capabilities will be negotiated through optional adapter interfaces instead of being assumed globally.

## Storage

### File storage

Use file storage for a local desktop configuration profile. Structured settings are stored as JSON. Secrets are kept separately through the configured secret store and, in the default implementation, protected using Windows DPAPI under the current Windows user context.

### File memory

`FileMemoryStore` stores one JSON memory entry per line in a JSONL file. Searches stream through the file and retain only the bounded top results, avoiding the need to load the complete memory file into RAM. This is the default persistent memory direction for low-resource desktop applications.

### File conversations

`FileConversationStore` stores each persistent session in its own JSON file. Saving replaces the session document atomically through a temporary file. Opening one session reads only that session's file, so a large number of unrelated conversations does not become resident in memory.

### SQL Server / MySQL

Use database storage when configuration needs to be centralized or shared between installations and administrative workflows. The storage packages use direct ADO.NET access rather than an ORM to keep them small and predictable.

## UI conventions

HAgent WinForms uses a custom borderless shell and shared HAgent controls rather than standard Windows chrome for the management experience.

Important UI rules:

- `Header` is the shared window header.
- `HMessage` is used for HAgent information, questions, delete confirmation, errors, and exceptions.
- `HButton` is the application action button.
- Forms use explanatory labels and task-oriented navigation rather than a giant property grid.

## Memory and low-resource design

Memory is a first-class runtime abstraction, but vector storage is optional.

The default design is intended to remain useful on machines with no GPU and as little as a few gigabytes of RAM. Lightweight text/metadata retrieval can be used without local embedding models. Optional vector memory may use a remote embedding service or a companion package when an application actually needs it.

HAgent must not make an embedding model, GPU, vector database, or large in-memory index a prerequisite for normal agent execution.

The current persistent file memory implementation uses JSONL streaming rather than materializing the entire store, keeping the basic retrieval path small and predictable.

Context budgeting is provider-neutral and tokenizer-free by default. Stored conversation history can remain complete while each model call receives only the bounded subset selected by the active context policy.

## Tools and application automation

Tools are planned as explicit, typed capabilities that the host registers and executes. For example, a host could expose:

```text
ui.move_control
ui.set_text
ui.read_control
```

The model requests a named tool with structured arguments; HAgent validates the request; the host executes the permitted side effect; and the result becomes an observation for the agent.

The actual provider-neutral tool-call execution loop is planned for the Tools milestone and is not forced into the 0.2 core.

## Security boundary

HAgent does not place raw API keys into normal agent/provider configuration records. The runtime resolves secrets through `ISecretStore` before calling an adapter.

Tool execution is intentionally bounded by explicit host registration and, in later milestones, permission/approval policies. Runtime failures and diagnostics must not expose secrets.

## Roadmap and project tracking

- [plan.md](plan.md) — current implementation state and active milestone.
- [roadmap.md](roadmap.md) — long-term product/architecture direction.
- [AGENTS.md](AGENTS.md) — repository engineering and UI rules.

These documents are part of the project state and must be updated with meaningful milestone or architecture changes. `HAgent.Example` is the manual verification surface for completed features.

## Contributing

Read [AGENTS.md](AGENTS.md) before modifying the repository.

## License

MIT — see [LICENSE](LICENSE).

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

Memory
  └─ explicit context supplied to the agent
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

For controlled execution:

```csharp
var execution = await ai.ExecuteAsync(
    "assistant",
    "Perform this task.",
    new AgentExecutionOptions
    {
        Timeout = TimeSpan.FromSeconds(60),
        MaxProviderAttempts = 3,
        RetryCountPerProvider = 1
    });
```

## Current status

HAgent 0.2 establishes the runtime foundation needed for later agent capabilities. It includes execution state, execution snapshots, provider routing, cancellation/timeouts, retry/backoff, lifecycle events, diagnostics, and a lightweight memory abstraction.

The current 0.3 line focuses on memory while preserving a lightweight execution model. Persistent memory, full autonomous tool calling, multi-agent collaboration, and the user chat experience are separate milestones.

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

### Memory is externalized state
HAgent does not claim that an AI model permanently remembers anything. Memory is explicit application state retrieved and supplied as context. The default memory direction must work without a local GPU and without loading a large store entirely into RAM.

### Tools are controlled capabilities
A tool definition describes a capability; it is not executable code. The host application owns the actual handler and side effects. An AI model must never receive arbitrary access to controls, processes, files, databases, or reflection.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Core models, abstractions, sessions, runtime, and lightweight infrastructure |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible chat and model-catalog adapter |
| `HAgent.Storage.File` | JSON settings + Windows DPAPI secret store |
| `HAgent.Storage.SqlServer` | SQL Server persistence + schema bootstrap |
| `HAgent.Storage.MySql` | MySQL persistence + schema bootstrap |
| `HAgent.WinForms` | Designer-free configuration/development UI and shared HAgent UI helpers |
| `HAgent.Example` | Manual integration and feature test application |
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

Set `HAgent.Example` as the startup project and press **F5**. The window provides:

- **Configuration** — opens the HAgent management UI.
- **Read configuration** — verifies provider/agent persistence can be read by the host.
- **Send message** — sends a real request through the configured agent/provider.
- **Test session** — exercises conversation history forwarding.
- **Test runtime** — exercises execution IDs, state, timeout, provider-attempt limits, and diagnostics.

Every major feature added to HAgent should eventually have a small manual example in this project so it can be tested directly on a developer machine.

## Provider adapters

The first adapter is OpenAI-compatible. It supports the common `/chat/completions` request shape and `/models` discovery endpoint.

The architecture intentionally permits separate adapters for providers such as Azure OpenAI, Anthropic, Google/Gemini, Ollama, LM Studio, local services, and custom enterprise endpoints. Provider capabilities will be negotiated through optional adapter interfaces instead of being assumed globally.

## Storage

### File storage

Use file storage for a local desktop configuration profile. Structured settings are stored as JSON. Secrets are kept separately through the configured secret store and, in the default implementation, protected using Windows DPAPI under the current Windows user context.

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

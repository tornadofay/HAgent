# HAgent

**Lightweight AI provider and agent management for .NET desktop applications.**

HAgent gives a WinForms application one small API for managing AI providers, defining agents, storing configuration securely, and sending messages without hard-coding a specific AI vendor into the application.

> Status: **0.1.0 — foundation / early test release**

## Why HAgent exists

AI integration often starts simple and quickly becomes application-specific plumbing: API keys, endpoints, model names, system prompts, provider switching, database tables, and settings screens end up scattered through the application.

HAgent centralizes that plumbing behind a small model:

```text
Provider
  ├─ connection / endpoint
  ├─ secret reference
  ├─ default model
  └─ optional shared system instruction

Agent
  ├─ provider
  ├─ optional model override
  ├─ system instruction
  ├─ generation settings
  └─ enabled state
```

An application eventually talks to an agent rather than directly to a provider:

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

## Design principles

### Small core
`HAgent.Core` contains the domain objects, store abstractions, provider adapter abstraction, session API, and runtime. It does not depend on WinForms, SQL Server, MySQL, or a particular AI vendor.

### Provider versus agent
A provider answers **“how do I connect?”**.

An agent answers **“how should this AI behave?”**.

The provider may define an optional shared system instruction. Agents inherit it by default and append their own system instruction. Every agent also has a visible setting to disable provider-prompt inheritance. This keeps the feature useful without creating a hidden prompt hierarchy.

### Secrets never live beside normal settings
The default file implementation stores structured settings as JSON and secrets separately through Windows DPAPI. This is deliberately preferable to putting API keys into an `.ini` file or ordinary JSON.

### Optional persistence
Applications can start with local files and later move the same domain model to SQL Server or MySQL. Storage is an implementation detail behind `IAiStore`.

### Provider adapters are plugins
The first transport is OpenAI-compatible HTTP because it covers a large family of APIs with the same request shape. More adapters can be added without changing agents or storage.

## Assemblies

| Assembly | Purpose |
|---|---|
| `HAgent.Core` | Core models, abstractions, runtime, sessions |
| `HAgent.Providers.OpenAICompatible` | OpenAI-compatible `/chat/completions` adapter |
| `HAgent.Storage.File` | JSON settings + DPAPI secret store |
| `HAgent.Storage.SqlServer` | SQL Server persistence + schema bootstrap |
| `HAgent.Storage.MySql` | MySQL persistence + schema bootstrap |
| `HAgent.WinForms` | Designer-free settings UI |
| `HAgent.Sample` | Testable WinForms sample application |

## Supported targets

The current release targets:

- .NET Framework 4.8.1
- .NET 9

.NET 10 is intentionally not a build target yet. It is planned for a later upgrade once the development environment moves to a Visual Studio version that supports it.

## Quick start

Add:

```text
HAgent.Core
HAgent.Providers.OpenAICompatible
HAgent.Storage.File
HAgent.WinForms
```

Then open the management UI with:

```csharp
HAgent.WinForms.AISettings.ShowMainAISettingsForm(this);
```

The parameterless storage path uses:

```text
%LOCALAPPDATA%\HAgent\settings.json
%LOCALAPPDATA%\HAgent\secrets\
```

For a custom application-owned store:

```csharp
var store = new HAgent.Storage.File.FileAiStore(
    @"C:\MyApp\ai\settings.json");

var secrets = new HAgent.Storage.File.ProtectedDataSecretStore(
    @"C:\MyApp\ai\secrets");

HAgent.WinForms.AISettings.ShowMainAISettingsForm(
    store,
    secrets,
    this);
```

Runtime:

```csharp
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

## UI philosophy

The settings window is deliberately not a typical giant property grid. It uses task-oriented navigation:

- **Overview** — what is configured and what needs attention.
- **Providers** — connections, authentication, defaults, and which agents use each provider.
- **Agents** — behavior, provider, model overrides, and generation controls.
- **About** — explains the mental model inside the application itself.

Controls use plain-language descriptions beside settings instead of relying on tooltips alone. The first release is English-first and is built without a WinForms designer file so the visual structure stays under source control.

High-DPI behavior should be configured by the host application. Modern WinForms supports explicit high-DPI modes, while .NET Framework 4.7+ requires opt-in configuration. citeturn553369search1turn553369search2

## Storage

### File storage

Use the file store when the application is a desktop product with one local configuration profile.

Normal settings:

```json
{
  "Providers": [],
  "Agents": []
}
```

Secrets are not serialized into that JSON.

### SQL Server / MySQL

Use the database stores when several installations or administrative workflows need a centralized configuration source. Each storage package exposes `EnsureSchemaAsync(...)` so an application can bootstrap HAgent's tables during startup or setup.

HAgent does not use an ORM for these stores. It uses direct ADO.NET providers to keep the storage assemblies predictable and small.

## Security boundary

The first release protects local secrets using Windows DPAPI `CurrentUser`. That means the secret is tied to the Windows user context that created it. It is not a replacement for a server-side secret manager.

For centrally managed deployments, the architecture intentionally leaves room for a custom `ISecretStore` implementation backed by a vault, Windows Credential Manager, or another enterprise secret provider.

## Current provider support

`HAgent.Providers.OpenAICompatible` sends chat requests to:

```text
{BaseUrl}/chat/completions
```

This intentionally covers APIs that expose the common OpenAI-style chat contract. It should not be interpreted as a guarantee that every provider using similar URLs implements every request field identically.

Microsoft's `Microsoft.Extensions.AI.IChatClient` is a useful ecosystem reference and is available to .NET Framework through a package-provided `netstandard2.0` surface, but HAgent does not require it in the core in order to keep the first version small. citeturn553369search0

## What is intentionally not in 0.1

No vector database, RAG pipeline, tool calling framework, autonomous loop, workflow engine, remote configuration service, telemetry backend, or secret-vault dependency is forced into the core.

Those features can be layered on later. The point of HAgent is to make the first 80% of application integration boring and stable.

## Roadmap

See [roadmap.md](roadmap.md).

## Contributing

See [AGENTS.md](AGENTS.md) for repository conventions and safe extension points.

## License

MIT — see [LICENSE](LICENSE).

## Development host

`HAgent.WinForms` is an executable development host. Set it as the Visual Studio startup project and press F5 to launch the HAgent configuration UI directly. The host registers the built-in OpenAI-compatible adapter; additional provider adapters can be supplied by application code or future companion packages.

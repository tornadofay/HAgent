# Architecture

HAgent separates provider transport, agent behavior, memory, tools, UI context, and host-owned side effects so applications can opt into only the capabilities they need.

```text
                         +----------------------+
                         |   HAgent.WinForms    |
                         | configuration + UI   |
                         | context / adapters   |
                         +----------+-----------+
                                    |
                                    v
+----------------+       +----------------------+       +---------------------------+
| Storage        |------>|      HAgent.Core     |<------| Provider Adapters         |
| File           |       | models + abstractions|       | OpenAI-compatible / future|
| SQL Server     |       | runtime + sessions   |       +---------------------------+
| MySQL          |------>| memory + context     |
+----------------+       | tools + agent loop   |
                         +----------+-----------+
                                    |
                                    v
                              Host application
                                    |
                    +---------------+----------------+
                    |                                |
             Explicit domain                 Automatic UI/data
             abstractions                    discovery + tools
             Customer/Invoice                when policy permits
```

## Runtime flow

1. Application selects an agent.
2. Core resolves that agent and its provider candidates.
3. Core checks known provider/model capabilities and routing constraints.
4. The adapter performs provider-specific transport.
5. Core normalizes the provider result into the stable `AIResponse` contract.
6. Optional tools can execute through the registry and return tool observations for subsequent model turns.

## Tool architecture

A persisted tool definition is not executable code.

```text
Tool Definition
    name
    description
    input schema
    type
    enabled
       |
       v
Tool Registry
       |
       +---- executable handler supplied by host/HAgent
```

Initial tool types are BuiltIn, Application, Declarative, UI, SqlServer, and MySql. Extension/DLL tools are deferred.

Tool arguments are validated before execution. Tool handlers remain application-owned runtime capabilities; executable delegates are never serialized into configuration files or databases.

## UI Context architecture

“Form serialization” is treated as one possible implementation technique, not the public subsystem name.

Two modes are intentional:

### Explicit domain abstraction

The host can expose a semantic object or custom adapter:

```text
Customer
 ├─ Name
 ├─ Contact
 └─ Invoices
```

This gives the developer maximum control and is the preferred path for sensitive or highly specialized applications.

### Automatic UI/data discovery

The host can attach a live UI context:

```csharp
var host = HAgentHost.Attach(form, registry, true, permissions);
```

The context can discover controls and bound data and expose read-only tools such as `ui.inspect`, `ui.read_control`, and `ui.read_data` when permission policy allows them.

Automatic discovery is convenience, not authority. Attaching a form never grants write or invoke access by itself.

## UI permissions

The coarse policy is:

```text
Automatic discovery
Read controls
Read data
Write controls
Invoke controls
```

Safe defaults are automatic discovery off, read access on, and write/invoke off.

Applications can disable automatic behavior and supply their own authorization/semantic abstraction. Future SQL Server/MySQL query permissions remain separate from UI permissions.

## Data representation rule

Use the lightest representation that preserves the information required by the operation. Prefer bound/native data sources, adapt lazily, and avoid unnecessary materialization. For `DataGridView`, a usable bound source is preferred over scraping visible cells. `DataTable` is optional, not the architectural default.

This same abstraction boundary is intended to allow future adapters for custom HControl/BaseForm components and other interactive surfaces such as GDI, DirectX, or Unity without moving those platform concerns into `HAgent.Core`.

## Example host

`HAgent.Example` is the manual integration/verification host. Each example exposes:

- editable input/message;
- expected behavior and notes;
- a copyable C# reproduction snippet;
- global agent selection where the example needs an agent.

The Example application is split into focused partial files so new demonstrations do not recreate the original monolithic form problem.

## Prompt model

The effective system instruction is intentionally simple:

```text
Provider shared instruction

Agent instruction
```

The agent may opt out of using the provider shared instruction. This avoids deep inheritance chains that are difficult for users to understand.

# HAgent architecture

`HAgent` is a general-purpose agent runtime. It connects LLM/provider execution and reusable agent capabilities to a host application, game, simulation, or simple chat experience.

The architecture has five stable concerns:

```text
Host Application
      |
      v
Context  <---->  Runtime Agents  <---->  Workspace
   |                    |                   |
 UI / Data / App     Execution           Messages
 External context      Memory            Routing
      |                    |                   |
      +--------------------+-------------------+
                           |
                         Tools
                           |
                     Providers / LLMs
```

## Core boundaries

- `HAgent.Core` owns provider-neutral models, runtime execution, memory/context abstractions, tools, workspaces, and authorization contracts.
- Provider adapters own provider-specific transport and capability discovery.
- Storage assemblies own persistence.
- `HAgent.WinForms` owns WinForms UI Context and control/data adapters.
- Host applications own their business/domain types and real-world side effects.
- HWorld is an external consumer. HAgent never references HWorld.

## Agent profile vs runtime instance

A persisted agent profile is reusable configuration:

```text
Agent Profile
    provider/model
    system prompt
    tools
    defaults
```

A runtime agent instance is one live identity created from a profile:

```text
Runtime Agent Instance
    instance ID
    profile ID
    scope
    memory owner
    execution state
    host/context bindings
```

Many runtime instances may come from one profile. Runtime instances are not permanent configuration entries by default.

## Context

HAgent accepts context supplied by the host. The host decides what the agent is allowed to know.

Supported context patterns include:

- UI Context / control adapters.
- Data-source and structured query context.
- Live application-object context with bounded inspection.
- Caller-created observation/context snapshots.
- Future platform-specific adapters.

Discovery is descriptive. Authorization is separate.

## Tools

Tool definitions describe what an agent may request. Trusted runtime handlers decide what the host actually executes.

Tool categories currently defined are BuiltIn, Application, Declarative, UI, SqlServer, and MySql. Extension tools are deferred.

Executables are never persisted as tool configuration.

## Workspace and communication

A workspace is optional. It allows several participants to share one conversation/context while keeping model execution targeted.

```text
User -> default recipient
User -> explicit recipient
Agent -> explicit recipient
```

Messages are not automatically broadcast. Visible agent-to-agent dialogue is a workspace feature; only the addressed participant starts an LLM execution for that message unless an explicit policy says otherwise.

## Memory

Memory ownership follows runtime identity rather than profile identity when isolated agent state is required. Shared memory must be explicitly scoped and authorized.

## Security rule

The model is never the security boundary. Permissions, authorization callbacks, approvals, limits, and host-side validation remain outside system-prompt instructions.

## External consumers

The same HAgent runtime can serve:

- simple one-agent chat;
- business applications with coordinator and contextual specialists;
- games and simulations;
- HWorld actors.

The host controls domain truth and side effects. HAgent supplies generic cognition/runtime capabilities.

## Detailed architecture

- [Runtime and agents](architecture/10-runtime.md)
- [Context and application understanding](architecture/20-context.md)
- [Tools](architecture/30-tools.md)
- [Security and authorization](architecture/40-security.md)
- [Storage](../storage.md)
- [HWorld integration](architecture/60-hworld.md)

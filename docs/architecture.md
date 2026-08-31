# HAgent architecture

HAgent is a general-purpose, provider-neutral agent runtime for .NET applications. A host may use it for simple chat, business software, games, simulations, or other environments.

## System model

```text
Host application
      |
      +--> Context -----------------------+
      |      UI / Data / Application     |
      |                                   v
      +--> Runtime agents ----------> Workspace
      |        |                          |
      |        v                          v
      |     Providers                  Messages
      |     Memory                     Routing
      |     Tools
      |
      +--> Host-side authorization and side effects
```

## Stable boundaries

`HAgent.Core` owns provider-neutral runtime models, execution, memory/context abstractions, tools, and future workspace/coordination primitives.

Provider assemblies own transport and provider-specific behavior. Storage assemblies own persistence. `HAgent.WinForms` owns WinForms UI Context/control/data adapters. Host applications own business/domain types and authoritative side effects.

HWorld is an external consumer, not a HAgent dependency.

## Core concepts

### Agent Profile

Reusable persistent configuration: provider/model preferences, system prompt, generation settings, and tool references.

### Runtime Agent Instance

One live agent identity created from a profile. It has its own runtime ID, scope, context bindings, memory ownership, and execution lifecycle. Many runtime instances may come from one profile.

### Context

Host-supplied information that tells an agent what it is allowed to know. Context may come from UI, data sources, application objects, tasks, or external environments.

### Tool

A structured capability request. Definitions describe the contract; trusted handlers perform execution. Tool handlers are never serialized.

### Workspace

An optional shared communication context containing users and agent participants. Messages have explicit senders and recipients. An unaddressed user message goes only to the workspace default recipient.

### Memory

Memory belongs to explicit ownership/scope. Private runtime-agent memory and shared workspace/application memory are separate concepts.

### Authorization

Discovery is not authority. Permissions, authorization, approval, budgets, and host-side validation determine what operations may actually occur.

## Execution flow

```text
caller context snapshot
        -> runtime agent instance
        -> provider/model execution
        -> normalized response / tool request
        -> trusted tool handler or host-side result
        -> caller
```

Execution is asynchronous, cancellable, bounded, and correlated. External schedulers do not need to block while a provider responds.

## Detailed documents

- [Runtime and agents](architecture/10-runtime.md)
- [Context and application understanding](architecture/20-context.md)
- [Tools](architecture/30-tools.md)
- [Security and authorization](architecture/40-security.md)
- [Storage](storage.md)
- [HWorld integration](architecture/60-hworld.md)

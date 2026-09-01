# HAgent architecture

HAgent is a general-purpose, provider-neutral cognition and execution library for software that needs LLM-driven behavior. It must support different host environments without requiring HAgent.Core to understand the host's domain model.

A host may be a simple conversational program, business software, a service, a game, a simulation, an automation system, a developer tool, or another environment. These are host integration contexts, not HAgent domain concepts.

## System model

```text
Host application
      |
      +--> Generic execution/context ------------+
      |                                          v
      +--> Runtime agent instances ----------> HAgent
      |                                          |
      |                        +-----------------+----------------+
      |                        |                                  |
      |                     Providers                         Memory / Tools
      |                        |                                  |
      |                        +-----------------+----------------+
      |                                          |
      +--> Host-owned authorization, scheduling, persistence,
           state management, and side effects
```

## Responsibility boundary

`HAgent.Core` owns provider-neutral agent profiles, runtime instances, execution, context abstractions, memory abstractions, tools, workspaces/coordination primitives, structured-output contracts, and execution telemetry.

Provider assemblies own transport and provider-specific behavior. Storage assemblies own persistence. Optional integration assemblies own platform-specific adapters. Host applications own domain objects, authoritative state, scheduling policy, persistence of host state, authorization rules, and side effects.

HAgent must remain usable across project types without importing host-domain types or assuming a particular UI, service, game, simulation, workflow, or application architecture.

## Core concepts

### Agent Profile

Reusable persistent configuration: provider/model preferences, system prompt, generation settings, and tool references.

### Runtime Agent Instance

One live agent identity created from a profile. It has its own runtime ID, scope, runtime overrides, memory ownership, and execution lifecycle. Many runtime instances may come from one profile.

### Execution Request

The generic host-to-HAgent boundary. A request carries host-supplied input/context, host correlation metadata, execution options, and optional structured-output requirements. Plain string messages are a convenience form, not the fundamental integration model.

### Context

Host-supplied information available to an agent. Context is generic data from the host; HAgent does not assign host-specific meaning to it.

### Tool

A structured capability request. Definitions describe the contract; trusted handlers perform execution. Tool handlers are never serialized.

### Workspace

An optional shared communication context containing participants and explicit message routing. Workspace semantics are provider-neutral and do not define business roles.

### Memory

Memory belongs to explicit ownership/scope. Private runtime-agent memory and shared memory are separate concepts.

### Structured Output

A host-defined output contract, represented by a schema that HAgent carries through the execution pipeline, validates, and exposes as structured result data. HAgent does not define host-domain schemas.

### Authorization

Discovery is not authority. Permissions, authorization, approval, budgets, and host-side validation determine what operations may actually occur.

## Execution flow

```text
host execution request
        -> runtime agent instance
        -> execution snapshot
        -> provider/model execution
        -> normalized response / structured output / tool request
        -> trusted tool handler or host-side result handling
        -> caller
```

Execution is asynchronous, cancellable, bounded, correlated, and protected against conflicting late completion. Hosts may supply their own scheduling policy.

## Stability and isolation requirements

Runtime instances must remain independently addressable and must isolate mutable runtime state, runtime overrides, execution state, shutdown signaling, and private memory ownership.

A host may create one long-lived runtime instance and execute against it repeatedly for an arbitrary lifetime. The host explicitly retires or shuts it down when appropriate.

Each execution has an HAgent execution identity. A host may additionally supply its own correlation identity without embedding correlation information into prompt text.

Cancellation and timeout are execution-control mechanisms. Once an execution reaches a terminal outcome, late provider completion must not publish a conflicting terminal result.

Provider, store, adapter, and tool implementations supplied by the host may be shared between instances when their contracts support concurrent use. HAgent must not introduce hidden global mutable runtime state that couples independent instances.

## Integration boundary

The canonical integration shape is:

```text
Host
  -> AgentExecutionRequest
  -> AgentRuntimeInstance
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
  -> host-owned result handling / side effects
```

The host remains authoritative for its own state. HAgent provides generic LLM cognition/execution infrastructure rather than becoming an application framework.

## Architecture references

- `docs/architecture/10-runtime.md` — provider-neutral execution and runtime-agent architecture.
- `docs/architecture/20-context.md` — context supplied by hosts and bounded context handling.
- `docs/architecture/30-tools.md` — structured tool definitions and host-owned execution.
- `docs/architecture/40-security.md` — authorization, permissions, approval, budgets, and guardrails.
- `docs/architecture/50-workspaces.md` — shared communication and workspace behavior.
- `docs/architecture/70-external-host-integration.md` — generic external host integration contract.
- `docs/storage.md` — persistence backends, secrets, and stored versus runtime state.

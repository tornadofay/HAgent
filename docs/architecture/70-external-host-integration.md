# External Host Integration

## Purpose

HAgent is a reusable cognition and execution library for software that needs LLM-driven behavior. It must support a wide range of host environments without requiring HAgent.Core to understand the host's domain model.

A host may be a simple conversational program, a business application, a service, a game, a simulation, an automation system, a developer tool, or another software environment. These are integration contexts, not HAgent domain concepts.

## Responsibility boundary

The host owns:

- domain objects and authoritative state;
- observations and external events;
- application lifecycle;
- scheduling policy and admission policy;
- persistence of host state;
- authorization and approval rules;
- real-world or application side effects.

HAgent owns generic cognition/execution capabilities:

- agent profiles;
- runtime agent instances;
- execution lifecycle;
- provider routing and model invocation;
- context ingestion and bounded context handling;
- memory abstractions;
- structured tool requests and trusted tool execution boundaries;
- structured model output contracts;
- workspace and agent-to-agent coordination primitives;
- execution correlation and observability;
- cancellation, timeout, and stale-result protection.

HAgent must not require the host to adopt any particular domain object, event model, command model, scheduler, persistence mechanism, or authorization framework.

## Generic execution request

The canonical execution boundary must accept a generic host request rather than requiring the host to encode all external information as a plain string message.

The execution request should be capable of carrying:

```text
host-supplied input/context
host correlation ID
execution options
optional structured-output requirements
```

The supplied input is opaque to HAgent at the domain level. HAgent may normalize, bound, project, or serialize it for model consumption through generic mechanisms, but it must not assign host-specific meaning to the data.

Plain string messages remain a supported convenience form built on the generic execution boundary.

## Runtime identity and lifetime

`AgentRuntimeInstance` is the long-lived execution identity created from a reusable `AiAgent` profile.

A host may:

```text
create instance
    -> execute repeatedly
    -> keep instance alive for an arbitrary lifetime
    -> retire or shut down explicitly
```

The runtime instance is independent from the persistent profile and from individual executions.

Multiple instances created from the same profile must remain independently addressable and must not share mutable runtime identity, runtime overrides, execution state, or private memory ownership.

## Execution identity and correlation

Every execution has its own HAgent execution ID.

A host may additionally provide a host correlation ID. These identifiers have separate meanings:

```text
AgentExecution.Id
    HAgent-owned execution identity

HostCorrelationId
    host-owned request/correlation identity

AgentRuntimeInstance.InstanceId
    long-lived runtime identity
```

Host correlation must travel through the execution contract and must not require embedding correlation data into prompt text.

## Cancellation, timeout, and late completion

Execution must support both caller cancellation and configured timeout.

Cancellation and timeout are execution outcomes, not prompt instructions.

A provider may complete late when cancellation is cooperative. HAgent must protect the execution state machine so that once a terminal outcome has won, a late provider completion cannot publish a conflicting result.

Runtime-instance retirement invalidates result authority for subsequently completed work. Runtime-instance shutdown additionally requests cancellation of outstanding instance-bound executions.

The host may still use execution identity/revision information to decide whether a completed result is authoritative for its own state.

## Structured output

A host may define a structured output schema for an execution.

HAgent provides the generic mechanism to:

```text
accept schema
    -> request structured output from a capable provider
    -> receive structured output
    -> validate the returned document
    -> expose validation/result metadata
```

The schema belongs to the host. HAgent must not contain domain-specific schemas.

Provider capability is explicit. A provider may report structured output as supported, unsupported, or unknown. HAgent must not silently treat arbitrary valid JSON text as proof that a structured-output contract was honored.

## Tools

Tools are the generic capability boundary between model reasoning and host-owned operations.

A host registers tools by providing:

```text
tool definition
trusted executable handler
```

Tool definitions describe the callable contract. Handlers perform the actual operation.

Tool execution context should carry sufficient generic identity for authorization and observability, including execution identity, runtime-instance identity, host correlation, tool identity, tool-call identity, arguments, and cancellation.

HAgent must not define what a tool means or what side effects are permitted. Host authorization remains authoritative.

## Memory ownership

Private memory is associated with runtime ownership rather than the reusable profile.

A host may therefore create multiple runtime instances from one profile while maintaining separate private memory for each instance.

Shared memory remains an explicitly scoped capability and must not be inferred merely because instances use the same profile or HAgent client.

## Concurrency

Long-lived runtime instances must be able to execute concurrently.

Runtime state such as lifecycle, execution revision, shutdown signaling, and memory ownership must remain isolated per instance. Shared infrastructure such as provider adapters, stores, and tool registries may be reused when their contracts support concurrent use.

HAgent must not require a single global execution loop or a host-specific scheduler.

Host scheduling remains optional and external. HAgent may provide focused scheduling primitives for admission control without taking ownership of the host's timing model.

## Persistence

Runtime-state persistence is optional.

When enabled, HAgent may persist generic runtime identity and lifecycle metadata through `IAgentRuntimeStateStore`. Host-owned domain state remains outside HAgent's runtime-state contract unless explicitly represented through a generic host extension boundary.

Persisted runtime identity must remain distinct from:

```text
persistent agent profile
runtime instance
individual execution
host correlation identity
host domain state
```

## Integration principle

The generic integration surface should remain small:

```text
Host
  -> AgentExecutionRequest
  -> AgentRuntimeInstance
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
  -> host-owned result handling / side effects
```

Everything the host needs to express about its own environment should cross this boundary as generic data or explicit host-owned adapters. HAgent should provide the reusable LLM cognition/execution infrastructure instead of becoming an application framework.

## Non-goals

HAgent must not become:

- a domain object model;
- an application state manager;
- a universal event bus;
- a host scheduler;
- an authorization authority for host operations;
- a replacement persistence system for host-owned state;
- a framework tied to one UI, game, simulation, service, or application type.

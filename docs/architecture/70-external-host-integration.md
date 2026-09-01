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
- scoped memory;
- reusable Skills;
- reusable Knowledge and managed Wiki sources;
- learning candidates and policy-governed promotion;
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

A host may create an instance, execute repeatedly, keep it alive for an arbitrary lifetime, then retire or shut it down explicitly.

Multiple instances created from the same profile must remain independently addressable and must not share mutable runtime identity, runtime overrides, execution state, or private memory ownership.

## Capability inheritance

The runtime inherits the profile's effective capability policy and may apply runtime-only tri-state overrides:

```text
Inherit
Enabled
Disabled
```

The policy applies to Skills, Knowledge/Wiki, Memory families/types, individual resources, and future resource types. The effective policy is captured in each execution snapshot.

This allows two runtime instances from the same profile to use different capability sets without creating duplicate profiles or mutating shared configuration.

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

## Learning and authority

Learning is part of HAgent's generic capability surface but is not automatic model training.

```text
execution experience
    -> learning analysis
    -> typed candidate
    -> validation/policy/authorization
    -> memory / knowledge / new skill version
```

The host can choose `LearningMode`:

```text
Disabled
SuggestOnly
AutomaticWithPolicy
FullyAutomatic
```

Model output does not directly grant authority to publish Wiki knowledge or mutate a published Skill. Candidates preserve provenance and source runtime/execution identity.

## Structured output

A host may define a structured output schema for an execution. HAgent provides the generic request, provider invocation, validation, and result metadata path. The schema belongs to the host.

## Tools

Tools are the generic capability boundary between model reasoning and host-owned operations. A host registers a tool definition plus trusted executable handler. Handlers remain runtime registrations and are never serialized.

Tool execution context carries sufficient generic identity for authorization and observability, including execution identity, runtime-instance identity, host correlation, tool identity, tool-call identity, arguments, and cancellation.

## Memory ownership

Private memory is associated with explicit runtime ownership rather than the reusable profile. Independent runtime instances can therefore maintain separate private memories even when they share the same profile, provider, stores, or skill library.

Working memory is execution-local. Episodic, semantic, procedural, and future memory families may use different explicit scopes. Shared memory remains an explicitly authorized capability.

## Concurrency

Long-lived runtime instances must be able to execute concurrently. Runtime state such as lifecycle, execution revision, shutdown signaling, capability overrides, and memory ownership remains isolated per instance.

Shared infrastructure such as provider adapters, stores, and tool registries may be reused when their contracts support concurrent use. HAgent must not require a single global execution loop or host-specific scheduler.

## Persistence

Runtime-state persistence is optional. When enabled, it may persist generic runtime identity/lifecycle metadata and runtime-only capability overrides through `IAgentRuntimeStateStore`. It must not silently convert runtime state into the persistent profile.

## Management and discoverability

HAgent's management layer can inspect the effective resource inventory of an agent or runtime. The inventory is extensible so known resource types can have specialized presentation while future/unknown types remain visible through generic resource/type identifiers.

## Integration principle

The generic integration surface should remain small:

```text
Host
  -> AgentExecutionRequest
  -> AgentRuntimeInstance
  -> effective capability/memory/knowledge context
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
  -> host-owned result handling / side effects
```

HAgent provides reusable LLM cognition/execution infrastructure rather than becoming an application framework.

## Non-goals

HAgent must not become:

- a domain object model;
- an application state manager;
- a universal event bus;
- a host scheduler;
- an authorization authority for host operations;
- a replacement persistence system for host-owned state;
- a framework tied to one UI, game, simulation, service, or application type.

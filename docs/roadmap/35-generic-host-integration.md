# Phase 0.95 — Generic External Host Integration

## Status

**Complete — verified on .NET Framework 4.8.1 and .NET 9.**

## Goal

Complete the provider-neutral execution boundary required by arbitrary host applications without coupling HAgent.Core to a host domain, UI framework, scheduler, persistence model, or side-effect system.

## Requirements

1. [x] Introduce a canonical `AgentExecutionRequest` carrying multiple messages, host-supplied bounded context, runtime overrides, and execution options.
2. [x] Preserve plain string message execution as a convenience overload over the canonical request boundary.
3. [x] Preserve host-supplied correlation identity separately from `AgentExecution.Id` and `AgentRuntimeInstance.InstanceId`.
4. [x] Propagate execution, runtime-instance, tool-call, tool, and host correlation identity through tool execution metadata.
5. [x] Define a host-owned `StructuredOutputOptions` request contract with HAgent-side validation.
6. [x] Request structured output through capable provider adapters rather than relying on post-generation JSON detection alone.
7. [x] Validate normalized structured output against the requested schema and expose normalized validation/result metadata.
8. [x] Preserve provider capability distinction for structured output (`Supported`, `Unsupported`, `Unknown`).
9. [x] Make execution terminal-state transitions race-safe so late provider completion cannot overwrite cancellation, timeout, retirement, shutdown, or another terminal outcome.
10. [x] Preserve independent runtime-instance identity, overrides, execution state, shutdown lifecycle, and private memory ownership when multiple instances originate from one profile.
11. [x] Snapshot runtime overrides and host context at the execution/instance boundary so mutable caller-owned state cannot create cross-instance coupling.
12. [x] Keep host scheduling external; HAgent provides only focused admission-control primitives.
13. [x] Preserve host ownership of domain state, persistence, authorization, scheduling policy, and side effects.
14. [x] Add deterministic Example verification covering generic execution input, host correlation, structured output, late completion protection, tool identity propagation, concurrent runtime instances, and memory isolation.
15. [x] Define and verify a provider-facing `ProviderExecutionRequest` boundary separate from the host-facing request.
16. [x] Route normal, tool-calling, and streaming provider adapter contracts through `ProviderExecutionRequest`.
17. [x] Use provider-facing structured-output requirements for provider-native constrained generation where supported, with controlled fallback and continued HAgent validation.
18. [x] Add and verify an external-consumer smoke sample representing a host consuming the HAgent production surface on both supported target frameworks.
19. [x] Compose a long-lived `AgentRuntimeInstance` with the canonical `AgentExecutionRequest` through `HAgentClient.ExecuteAsync(instance, request, cancellationToken)`, preserving request input/context/correlation/structured-output semantics while the instance supplies runtime identity, revision, overrides, lifecycle, and private-memory ownership. Verified by `RUNTIME INSTANCE REQUEST` on the Example application.

## API direction

The public integration shape is:

```text
Host
  -> AgentRuntimeInstance (optional long-lived execution identity)
  + AgentExecutionRequest (execution input/context)
  -> HAgentClient.ExecuteAsync(instance, request, ...)
  -> AgentExecution
```

The runtime instance and execution request are orthogonal. The runtime instance answers **who is executing**; the request answers **what is being executed**. The request must not absorb the runtime instance because runtime ownership, lifecycle, revision, overrides, shutdown signaling, and private-memory ownership belong to the instance boundary.

After HAgent resolves the agent/provider/runtime state, the provider adapter receives:

```text
AgentExecutionRequest + runtime-derived execution options
  -> ProviderExecutionRequest
  -> provider adapter
  -> normalized AIResponse
```

The request boundaries remain generic. HAgent does not define host-domain schemas, event types, command types, domain objects, or lifecycle policy.

## Runtime invariants

One reusable profile may produce many long-lived runtime instances. Every runtime instance remains independently addressable and owns its own runtime lifecycle, execution revision, override snapshot, shutdown signaling, and private memory ownership.

When an execution is started from a runtime instance, `request.AgentId` must match `instance.ProfileId`. The caller's request/options objects are not mutated to attach runtime identity; HAgent creates the effective execution request/options internally.

An execution that has reached a terminal outcome cannot later publish a conflicting outcome because a provider completed late. Non-cooperative providers may continue executing after HAgent has completed cancellation/timeout handling, but their late results cannot regain authority over the terminal execution state.

## Structured-output invariant

A structured-output response is valid only when the requested contract is successfully honored and validated. Arbitrary JSON text is not sufficient evidence that the contract was satisfied.

Provider-native constrained generation is opportunistic. When an OpenAI-compatible endpoint supports the native `response_format`/JSON Schema request shape, the adapter uses it. If the endpoint explicitly reports that feature as unsupported or unknown, the adapter may retry without the native field. HAgent validation remains authoritative in either path.

## External consumer verification

`samples/HAgent.ExternalConsumer` is a standalone host sample that references the broad HAgent production surface available to an application: Core, the OpenAI-compatible provider transport, File storage, SQL Server storage, MySQL storage, and WinForms. It owns its own host-side test data/provider and does not introduce HWorld-specific domain logic into HAgent. The sample has been executed successfully on both `.NET Framework 4.8.1` and `.NET 9`.

A real host is not required to reference every HAgent assembly in production; it selects the modules it needs. The sample is intentionally broad so this milestone verifies the public HAgent system surface rather than only `HAgent.Core`.

## HWorld boundary

HWorld is an external consumer. HAgent does not contain an HWorld dependency, adapter, physics, rendering, simulation-time, or action-authority code. HWorld references the HAgent modules it needs and owns its own domain lifecycle, scheduling, state, authorization, and side effects.

## Exit criterion

A host can submit a complete provider-neutral execution request with bounded context, host correlation, and optional structured-output requirements; HAgent can execute that request either directly or through a long-lived runtime instance without losing request semantics or runtime ownership. HAgent resolves the request into a provider-facing request, invokes an adapter, normalizes the response, validates host-owned contracts, preserves execution identity, protects terminal state, and isolates runtime snapshots without coupling to host or provider-specific domain models. A standalone external consumer representing the HAgent production surface demonstrated the public boundary on both supported target frameworks, and runtime-instance execution composes the canonical request through the verified instance/request API.

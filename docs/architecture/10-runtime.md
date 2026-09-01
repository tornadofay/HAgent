# Runtime and agents

## Profile

`AiAgent` is persistent reusable configuration: identity, provider preferences, model, system prompt, generation settings, and tool references.

## Runtime instance

A runtime instance is the live execution identity created from a reusable profile. `AgentRuntimeInstance` has its own stable `InstanceId`, keeps the source `ProfileId`, records its `AgentRuntimeScope`, and has an explicit active/retired/shutdown lifecycle.

Creating or retiring a runtime instance never mutates the reusable `AiAgent` profile and does not make the instance a persistent configured agent by default.

Runtime-instance identity is intentionally separate from `AgentExecution.Id` and host correlation identity. An instance may own many executions over its lifetime, while each execution retains its own immutable execution identity and host correlation when supplied.

Runtime instances may carry `AgentRuntimeOverrides`. These are runtime-only values applied to a cloned execution snapshot and never written back to the persistent profile. Overrides are configuration, not the generic execution-input channel.

Each runtime instance also has an independent `MemoryOwnerId`, currently equal to its `InstanceId`. Instance-created sessions and explicit memory operations use that owner so multiple runtime instances created from the same persistent profile cannot collide in private agent-scoped memory.

Executing an `AgentRuntimeInstance` is exposed through `HAgentClient.ExecuteAsync(AgentRuntimeInstance, ...)`. A retired or shutdown instance cannot start new execution. Existing executions retain their snapshots if the instance is retired after work has started.

Each instance maintains a monotonically increasing execution revision. An instance-bound `AgentExecution` captures the instance ID and revision at execution start. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to determine whether a completed result is still authoritative for that runtime instance. Stale protection is an authority mechanism and must not be confused with provider cancellation.

`AgentRuntimeInstance.Shutdown()` is terminal for the instance. It prevents new execution and requests cancellation of outstanding instance-bound work. Retirement stops new execution and invalidates result authority without cancelling already-running work.

## Generic execution request

The runtime execution boundary must be generic enough for a host to provide arbitrary external context or observations without converting everything into a plain string message.

The canonical request should carry:

```text
host-supplied input/context
host correlation identity
execution options
optional structured-output contract
```

The input/context is opaque to HAgent at the domain level. HAgent may bound, normalize, project, or serialize it through generic mechanisms for model consumption, but it must not assign host-specific meaning to the data.

Plain string execution remains a convenience overload built on the generic request boundary.

## Execution identity and correlation

Every execution has a HAgent-owned execution ID.

A host may additionally provide a correlation ID. These identities have different responsibilities:

```text
AgentExecution.Id
    HAgent execution identity

HostCorrelationId
    host-owned request identity

AgentRuntimeInstance.InstanceId
    long-lived runtime identity
```

Correlation must be carried through the execution contract rather than encoded into prompt text.

Tool execution should inherit the relevant execution/runtime/host correlation identities so host authorization and telemetry do not require passing those identities as model arguments.

## Cancellation, timeout, and late completion

Execution supports caller cancellation and configured timeout.

Cancellation and timeout are execution-control semantics, not prompt instructions.

Provider cancellation is cooperative. A provider may therefore complete after a caller cancellation or timeout request. The runtime must treat execution completion as a guarded state transition: once cancellation, timeout, retirement, shutdown, or another terminal outcome has won, a late provider result cannot publish a conflicting terminal result.

Hosts may use runtime-instance revision checks to reject results that are stale for their own state. HAgent should additionally prevent stale provider completion from mutating its own execution outcome.

## Structured output

A host may define a structured output schema for an execution.

The runtime must provide a generic path to:

```text
host schema
    -> provider structured-output request
    -> provider response
    -> schema validation
    -> structured result + validation metadata
```

The schema is host-owned. HAgent must not embed host-domain schemas.

Provider capability support remains explicit and may be `Supported`, `Unsupported`, or `Unknown`. Valid JSON text alone is not evidence that a structured-output contract was honored.

## Scheduling

Scheduling is host-controlled and optional. `IAgentExecutionScheduler` and `AgentExecutionScheduler` provide a focused admission boundary that can limit concurrent runtime executions without taking ownership of the host's timing model.

The scheduler is not a second execution engine. Hosts may use it, replace it, or schedule direct calls to `HAgentClient.ExecuteAsync(...)` themselves.

## Runtime state persistence

Runtime-state persistence is a separate optional boundary from `IAiStore`. `IAgentRuntimeStateStore` persists only generic runtime identity and lifecycle metadata. Host-owned domain state remains outside this contract.

`AgentRuntimeStatePersistence` provides explicit save, restore, and delete operations. Restore requires the corresponding persistent `AiAgent` profile and verifies profile identity before recreating the runtime instance.

Runtime creation is non-persistent by default. Persisted runtime metadata remains distinct from the persistent profile, the live instance, individual executions, and host-owned state.

## Scope

`AgentRuntimeScope` describes where a runtime instance belongs. The scope value is metadata and lifecycle context; it must not be used to create domain-specific agent classes.

## System-prompt composition

System prompts are **additive layers**, not replacement values.

The current composition order is:

```text
Higher priority
    Provider policy
        ↓
    Agent profile
        ↓
    Runtime / execution additions
Lower priority
```

A lower layer may add narrower instructions or restrictions but must not erase, replace, or contradict a higher-priority layer.

Prompt composition is not an authorization boundary. Permissions, authorization callbacks, approvals, budgets, and host-side validation remain authoritative outside model instructions.

## Execution model

The host supplies an execution request snapshot. Runtime resolves the profile/provider, applies runtime-only overrides to an execution snapshot, composes applicable system-prompt layers, invokes the provider, normalizes the response, validates structured output when requested, and reports lifecycle/usage metadata.

The host may schedule executions independently of its own application timing or event model.

## Design invariant

HAgent is the reusable LLM cognition/execution layer. A host remains responsible for understanding its own environment and for deciding what state, capabilities, and side effects are exposed through the generic HAgent boundary.

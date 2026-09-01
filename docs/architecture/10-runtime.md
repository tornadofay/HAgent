# Runtime and agents

## Profile

`AiAgent` is persistent reusable configuration: identity, provider preferences, model, system prompt, generation settings, and tool references.

## Runtime instance

A runtime instance is the live execution identity created from a reusable profile. `AgentRuntimeInstance` has its own stable `InstanceId`, keeps the source `ProfileId`, records its `AgentRuntimeScope`, and has an explicit active/retired/shutdown lifecycle.

Creating or retiring a runtime instance never mutates the reusable `AiAgent` profile and does not make the instance a persistent configured agent by default.

Runtime-instance identity is intentionally separate from `AgentExecution.Id` and `AgentExecution.CorrelationId`. An instance may own many executions over its lifetime, while each execution retains its own immutable execution identity and correlation anchor.

Runtime instances may carry `AgentRuntimeOverrides`. These are runtime-only values applied to a cloned execution snapshot and never written back to the persistent profile. Supported overrides currently include provider ID, model, temperature, maximum output tokens, an optional runtime system-prompt value, and a bounded host-supplied string context dictionary. Runtime context is captured immutably in the execution snapshot; it is context data, not an authorization mechanism.

Each runtime instance also has an independent `MemoryOwnerId`, currently equal to its `InstanceId`. Instance-created sessions and explicit memory operations use that owner so multiple runtime instances created from the same persistent profile cannot collide in agent-scoped automatic or explicit memory.

Executing an `AgentRuntimeInstance` is exposed through `HAgentClient.ExecuteAsync(AgentRuntimeInstance, ...)`. A retired or shutdown instance cannot start new execution. Existing executions retain their snapshots if the instance is retired after work has started.

Each instance maintains a monotonically increasing execution revision. An instance-bound `AgentExecution` captures the instance ID and revision at execution start. Hosts can call `AgentRuntimeInstance.IsExecutionCurrent(execution)` to determine whether a result is still authoritative. A result becomes stale when a newer execution has started on that instance or when the instance is retired or shutdown. Stale protection does not cancel or discard provider work; it gives the host a deterministic authority check for late results.

`AgentRuntimeInstance.Shutdown()` is terminal for the instance. It prevents new execution and cancels outstanding instance-bound execution through the instance shutdown token. Retirement and shutdown are distinct: retirement stops new execution and invalidates result authority without cancelling already-running work, while shutdown additionally requests cancellation of outstanding instance-bound work.

## Scheduling

Scheduling is host-controlled and optional. `IAgentExecutionScheduler` and `AgentExecutionScheduler` provide a focused admission boundary that can limit concurrent runtime executions without taking ownership of application timing, simulation ticks, or external scheduling policy. The scheduler waits for an available slot, delegates to `HAgentClient.ExecuteAsync(AgentRuntimeInstance, ...)`, honors caller cancellation while queued or running, and releases its slot when execution finishes.

The scheduler is not a second execution engine and does not alter provider routing, timeout, cancellation, correlation, stale-result, or runtime-instance semantics. Hosts may use their own scheduler instead.

Runtime instances must support:

- concurrent independent execution;
- host-controlled scheduling or direct asynchronous execution;
- cancellation and timeout;
- execution snapshots;
- stale-result protection;
- explicit retirement and shutdown;
- optional persistence for recovery/collaboration.

## Scope

`AgentRuntimeScope` describes where a runtime instance belongs. The current provider-neutral scope vocabulary is:

```text
Application
Workspace
ContextForm
Session
Task
Ephemeral
```

Scope is metadata and lifecycle context; it must not be encoded as different agent classes.

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

A layer may add instructions or restrictions for the layer below it, but it must not erase, replace, or contradict a higher-priority layer. Lower layers may add narrower constraints; they do not obtain authority to weaken an earlier layer.

The provider layer is included when `AiAgent.UseProviderSystemPrompt` is enabled. Disabling that layer is an explicit configuration choice; it does not turn the agent prompt into a replacement mechanism for another layer.

`SystemPromptLayer.Priority` provides deterministic composition order. Future runtime/context/workspace layers should use reserved priority ranges rather than inventing separate prompt-merging logic.

Prompt composition is not an authorization boundary. Permissions, authorization callbacks, approvals, budgets, and host-side validation remain authoritative outside model instructions.

## Execution

The host supplies a request/context snapshot. Runtime resolves the profile/provider, applies any runtime-only overrides to an execution clone, creates an execution snapshot, composes the applicable system-prompt layers, invokes the provider, normalizes the result, and reports lifecycle/usage metadata.

The host may schedule executions independently of application or simulation timing.

`AgentExecution.CorrelationId` is an execution-level identifier. It is not a runtime-instance identifier and remains unique per execution.

## Sessions

A session is conversation state. It is related to an agent runtime but is not the same concept as an agent profile or runtime instance.

## HWorld

HWorld uses runtime instances at its external cognition boundary. HWorld owns simulation time and scheduling; HAgent owns generic execution. HAgent must not require HWorld.

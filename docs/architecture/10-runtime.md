# Runtime and agents

## Profile

`AiAgent` is persistent reusable configuration: identity, provider preferences, model, system prompt, generation settings, capability references, and learning/memory policy defaults.

## Runtime instance

A runtime instance is the live execution identity created from a reusable profile. `AgentRuntimeInstance` has its own stable `InstanceId`, keeps the source `ProfileId`, records its `AgentRuntimeScope`, and has an explicit active/retired/shutdown lifecycle.

Creating or retiring a runtime instance never mutates the reusable `AiAgent` profile and does not make the instance a persistent configured agent by default.

Runtime-instance identity is intentionally separate from `AgentExecution.Id` and host correlation identity. An instance may own many executions over its lifetime, while each execution retains its own immutable execution identity and host correlation when supplied.

Runtime instances may carry `AgentRuntimeOverrides`. These are runtime-only values applied to a cloned execution snapshot and never written back to the persistent profile. Runtime capability overrides follow tri-state inheritance (`Inherit`, `Enabled`, `Disabled`) so an instance can selectively change Skills, Knowledge/Wiki, Memory, individual resources, or individual memory types without copying the complete profile.

Each runtime instance also has an independent `MemoryOwnerId`, currently equal to its `InstanceId`. Instance-created sessions and explicit memory operations use that owner so multiple runtime instances created from the same persistent profile cannot collide in private agent-scoped memory.

Executing an `AgentRuntimeInstance` is exposed through `HAgentClient.ExecuteAsync(AgentRuntimeInstance, ...)`. A retired or shutdown instance cannot start new execution. Existing executions retain their snapshots if the instance is retired after work has started.

Each instance maintains a monotonically increasing execution revision. An instance-bound `AgentExecution` captures the instance ID and revision at execution start. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to determine whether a completed result is still authoritative for that runtime instance. Stale protection is an authority mechanism and must not be confused with provider cancellation.

`AgentRuntimeInstance.Shutdown()` is terminal for the instance. It prevents new execution and requests cancellation of outstanding instance-bound work. Retirement stops new execution and invalidates result authority without cancelling already-running work.

## Effective capability snapshot

Before provider execution, HAgent resolves the effective capability policy from host/system policy, the persistent profile, and runtime overrides, then captures it in the immutable execution snapshot.

```text
host/system policy
    -> profile defaults
        -> runtime override
            -> execution snapshot
```

The snapshot determines which Skills, Knowledge/Wiki resources, Memory families, and future resource types are available to the execution. Later edits to the profile or runtime instance cannot change an execution already in progress.

Capability policy is enforced by code before retrieval or invocation. Prompt text is not used as authorization.

## Generic execution request

`AgentExecutionRequest` is the canonical provider-neutral host execution request. It can carry multiple ordered `AIMessage` inputs, a host-supplied correlation identity, a bounded string context dictionary, and `AgentExecutionOptions`.

The request validates required agent/message identity, limits the message count, and bounds host-context entry count, key length, and value length. Host context is copied into `AgentExecutionSnapshot.HostContext` and is therefore immutable for the lifetime of the execution snapshot.

Host correlation is copied to `AgentExecution.HostCorrelationId`. It remains distinct from the HAgent execution correlation ID and runtime-instance ID and is never encoded into prompt text.

The legacy `ExecuteAsync(agentId, message, ...)` overload remains a convenience compatibility path and delegates to the canonical request boundary.

## Execution identity and correlation

Every execution has a HAgent-owned execution ID and HAgent-owned execution correlation ID. A host may additionally provide a correlation ID. Runtime identity, execution identity, and host correlation identity remain distinct.

Tool execution should inherit relevant execution/runtime/host correlation identities so host authorization and telemetry do not require passing those identities as model arguments.

## Memory and learning lifecycle

An execution may use working memory plus any memory families allowed by its effective capability policy. Execution outcomes and explicitly captured observations can be passed to the learning subsystem according to the configured `LearningMode`.

Learning does not own runtime identity and must not mutate an active runtime's profile. It creates typed candidates (`MemoryCandidate`, `KnowledgeCandidate`, `SkillCandidate`) that are later accepted, rejected, or automatically promoted by policy.

## Cancellation, timeout, and late completion

Execution supports caller cancellation and configured timeout. Cancellation and timeout are execution-control semantics, not prompt instructions.

Provider cancellation is cooperative, but HAgent does not require provider cooperation to finalize the caller-facing execution. When cancellation or timeout wins, the execution transitions atomically to `Cancelled` with the appropriate `AgentExecutionFailureKind` and completes without waiting for a non-cooperative provider task to return. A late provider task is detached from the execution lifecycle; its fault is observed so it cannot produce an unobserved task failure, and its eventual response cannot overwrite the already-terminal execution.

`AgentExecution` owns the terminal-state gate. Success, failure, and cancellation/timeout use atomic first-terminal-outcome-wins transitions. Later terminal attempts are ignored, preventing conflicting lifecycle notifications, response replacement, or duplicate audit finalization.

Retirement and shutdown also invalidate runtime result authority. A provider response that arrives after an execution is no longer current may still be a provider completion, but it must not regain authority over host state. Hosts use runtime-instance revision checks to reject stale results.

## Structured output

A host may define a structured output schema for an execution. The runtime carries it through provider execution and validates the result. The validation contract remains independent of any provider capability claim.

## Scheduling

Scheduling is host-controlled and optional. `IAgentExecutionScheduler` and `AgentExecutionScheduler` provide a focused admission boundary that can limit concurrent runtime executions without taking ownership of host timing.

## Runtime state persistence

Runtime-state persistence is a separate optional boundary from `IAiStore`. The current persisted runtime-state record contains generic identity, host/user/workspace/session metadata, scope, lifecycle state, and timestamps. It does not persist prompts, runtime context, secrets, execution history, or mutable provider responses.

Restore requires the corresponding persistent `AiAgent` profile and verifies profile identity before recreating the runtime instance. Runtime-only overrides are not part of the current persisted record and must be supplied again by the host when required.

## Scope and isolation

`AgentRuntimeScope` describes where a runtime instance belongs. Scope is metadata and lifecycle context, not a host-specific agent class.

Private runtime memory remains independently owned. Shared memory must be an explicit scope with authorization. Stores can be shared concurrently, but mutable private runtime state cannot be shared implicitly.

## System-prompt composition

System prompts are additive layers, not replacement values. A lower layer may add narrower instructions or restrictions but must not erase or contradict a higher-priority layer.

## Execution model

The host supplies a canonical execution request. Runtime resolves profile/provider, applies runtime-only overrides, resolves the effective capability snapshot, composes applicable prompt layers, retrieves only permitted knowledge/memory, binds only enabled skills/tools, invokes the provider, normalizes the response, validates structured output when requested, captures configured memory/observations, optionally invokes learning, and reports lifecycle/usage metadata.

## Design invariant

HAgent is the reusable LLM cognition/execution layer. A host remains responsible for understanding its own environment and for deciding what state, capabilities, and side effects are exposed through the generic HAgent boundary.

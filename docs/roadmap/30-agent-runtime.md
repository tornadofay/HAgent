# Phase 0.9 — Runtime Agent Instances

## Goal
Make live agents first-class runtime objects separate from reusable agent profiles.

## Steps

1. [x] Introduce a provider-neutral runtime agent instance with its own stable instance ID and profile reference.
2. [x] Define explicit provider-neutral runtime scopes.
3. [x] Allow runtime-specific context and provider/model overrides without mutating stored profiles.
4. [x] Give each runtime instance an independent memory owner.
5. [x] Support multiple runtime instances executing concurrently.
6. [x] Expose asynchronous scheduling, cancellation, timeout, correlation, and stale-result protection foundations.
7. [x] Define explicit active/retired/shutdown lifecycle behavior.
8. [x] Keep dynamically created agents out of persistent configuration by default.
9. [x] Add optional runtime-state persistence for recovery, collaboration, or multi-process deployments.
10. [x] Verify the runtime contract with deterministic Example coverage.
11. [ ] Complete generic external-host execution boundary hardening in Phase 0.95.

## Runtime rule

One configured profile can produce many live instances. Runtime roles are host policy over the same generic runtime model, not separate agent classes.

Runtime-only provider, model, generation, system-prompt, context, and capability overrides are applied to execution snapshots created from the persistent profile. They never mutate the stored profile. Runtime configuration remains distinct from per-execution host input.

Each runtime instance owns private memory through its `MemoryOwnerId`, keeping private agent-scoped memory separate across instances created from the same profile. Shared memory is possible only through an explicit shared scope and authorization policy.

Each instance-bound execution receives a monotonically increasing instance revision. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to reject late results after a newer execution starts or the instance is retired. The generic execution hardening phase additionally ensures late provider completion cannot overwrite a terminal execution outcome.

Retirement and shutdown are separate lifecycle operations. Retirement prevents new executions and invalidates result authority while allowing already-running work to finish or be cancelled by the host. Shutdown is terminal, prevents new executions, invalidates result authority, and requests cancellation of outstanding instance-bound executions.

`IAgentExecutionScheduler` and the default `AgentExecutionScheduler` provide an optional host-controlled admission boundary with a configurable concurrency limit. The scheduler does not own host timing or replace runtime execution semantics.

Capability policy and learning are layered above this runtime foundation. Phase 0.11 resolves profile capability defaults plus runtime tri-state overrides (`Inherit`, `Enabled`, `Disabled`) into each execution snapshot. Learning operates on execution experience and never mutates runtime identity directly.

## External-host relationship

Phase 0.9 establishes the runtime-instance foundation. Phase 0.95 completes the generic execution boundary required for external hosts: arbitrary host input/context, host correlation, structured output contracts, terminal execution semantics, and tool identity propagation. Phase 0.11 consumes these runtime guarantees for scoped knowledge, Skills, Memory, Learning, and management UI.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, private-memory, or execution-state collisions. Later phases may layer reusable Skills, Knowledge/Wiki, Memory governance, and Learning without weakening runtime isolation.

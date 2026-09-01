# Phase 0.9 — Runtime Agent Instances

## Goal
Make live agents first-class runtime objects separate from reusable agent profiles.

## Steps

1. [x] Introduce a provider-neutral runtime agent instance with its own stable instance ID and profile reference.
2. [x] Define explicit runtime scopes: Application, Workspace, Context/Form, Session, Task, and Ephemeral.
3. [x] Allow runtime-specific context and provider/model overrides without mutating stored profiles.
4. [x] Give each runtime instance an independent memory owner.
5. [x] Support multiple runtime instances executing concurrently.
6. [ ] Expose asynchronous scheduling, cancellation, timeout, correlation, and stale-result protection.
7. [ ] Define explicit active/retired/shutdown lifecycle behavior.
8. [ ] Keep dynamically created agents out of persistent configuration by default.
9. [ ] Add optional runtime-state persistence for recovery, collaboration, or multi-process deployments.
10. [ ] Verify the runtime contract with deterministic Example coverage.
11. [ ] Add the first HWorld adapter verification at this boundary.

## Runtime rule

One configured profile can produce many live instances. Roles such as coordinator and specialist are host policy over the same runtime model, not separate agent classes.

Runtime-only provider, model, generation, system-prompt, and context overrides are applied to execution snapshots created from the persistent profile. They never mutate the stored profile.

Each runtime instance owns memory through its `MemoryOwnerId`, keeping agent-scoped memory separate across instances created from the same profile.

Each instance-bound execution receives a monotonically increasing instance revision. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to reject late results after a newer execution starts or the instance is retired. This authority check does not cancel provider work.

## HWorld gate

HWorld can begin consuming HAgent when the runtime exposes independent agent instances, asynchronous execution, caller-supplied observation/context, structured tool requests, cancellation/timeout, and stale-result protection. HWorld remains responsible for world state and action validation.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, memory, or execution-state collisions.

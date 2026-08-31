# Agent runtime and multi-agent platform

## 0.9 Agent Runtime + Scope

HAgent must work both as a simple one-agent chat library and as a runtime hosting many independent agent instances.

The foundational distinction is:

```text
Agent profile
    = reusable configured definition

Runtime agent instance
    = one live agent created from a profile
```

Planned capabilities:

- [ ] Runtime agent instance identity separate from profile identity.
- [ ] Application, workspace, context/form, session, task, and ephemeral scopes.
- [ ] Concurrent independent agent execution.
- [ ] External scheduling, cancellation, timeout, and stale-result protection.
- [ ] Independent provider/model/settings per runtime instance.
- [ ] Runtime retirement without deleting persistent configuration.
- [ ] Optional persistence of runtime state for restart/recovery scenarios.

## 0.9 Shared Workspaces

A workspace is a shared communication context that can contain a human user and multiple agent participants.

The default routing rule is:

```text
unaddressed user message -> workspace default recipient
addressed user message   -> named target participant
agent message            -> addressed participant
```

The default recipient is commonly a coordinator/manager agent, but the runtime API should use generic role/binding concepts rather than hard-code business terminology.

A workspace must not send an unaddressed message to every participant.

Planned capabilities:

- [ ] Workspace abstraction independent of WinForms.
- [ ] Participant registration/removal.
- [ ] Default recipient.
- [ ] Direct addressing.
- [ ] Host-configurable addressing syntax.
- [ ] Agent-to-agent messaging.
- [ ] Optional explicit broadcast.
- [ ] Message correlation, causation, sender and recipient metadata.
- [ ] Turn, hop, token, time and agent-invocation budgets.
- [ ] Loop/recursive-delegation protection.

## 0.9 Coordinator + Specialist Pattern

The common desktop pattern is a coordinator plus contextual specialists:

```text
User
  |
  v
Coordinator
 /        \
Specialist  Specialist
Invoice    Purchases
```

A contextual specialist can be spawned from a configured default profile when a host context appears. Its system prompt and context are assembled at runtime from the host environment and do not mutate the stored profile.

The coordinator may delegate work to a specialist. A specialist may return an addressed result to the coordinator. The user can directly address a specialist. There is no requirement that every participant receive every message.

## 0.9 Memory and context isolation

Every runtime instance must have its own memory ownership identity.

```text
profile: Invoice Specialist
    |
    +-- instance A -> private memory A
    +-- instance B -> private memory B
```

Workspace/shared memory must be separate from private agent memory and explicitly authorized.

Planned capabilities:

- [ ] Runtime-instance memory ownership.
- [ ] Workspace/shared memory policy.
- [ ] Private/shared/group scopes.
- [ ] Memory provenance by runtime instance.
- [ ] Explicit cross-agent memory sharing.

## 0.9 Multi-process and network deployments

Persistent configuration should remain reusable while runtime identities remain process/user/workspace specific.

Planned identity dimensions:

```text
host instance
user/session
workspace
runtime agent instance
```

Local deployments may keep ephemeral agents in memory. Database-backed deployments may persist runtime instances when collaboration, auditing or recovery requires it. Dynamic agents are not persistent configuration entries by default.

## 0.9 HWorld compatibility requirement

HWorld is a target external consumer of HAgent. HWorld owns simulation state, time, physics, sensors, world scheduling and action validation; HAgent supplies generic cognition/agent execution infrastructure.

HWorld must remain runnable without HAgent, and HAgent must not reference HWorld-specific types, actions, physics or rendering.

The HAgent runtime must therefore support:

- multiple independent agents concurrently;
- independent memory and context per agent;
- external asynchronous scheduling;
- immutable caller-provided observation snapshots;
- structured tool/action requests;
- provider/model diversity;
- cancellation/timeouts and stale-result handling;
- compact context and usage telemetry.

The HWorld integration adapter belongs in HWorld, at its external cognition/decision boundary.

## 0.9 Example verification

Planned deterministic Example coverage:

- [ ] Two or more runtime agent instances execute concurrently.
- [ ] One profile can produce multiple independent instances.
- [ ] Different instances have separate memory owners.
- [ ] Workspace sends unaddressed messages only to the default recipient.
- [ ] Direct addressing reaches only the selected agent.
- [ ] Coordinator delegation reaches the selected specialist.
- [ ] Specialist response returns to the intended recipient.
- [ ] Routing loops and excessive delegation are bounded.
- [ ] Runtime instances can be retired independently.
- [ ] Optional persisted runtime identity remains isolated across host/user/workspace instances.

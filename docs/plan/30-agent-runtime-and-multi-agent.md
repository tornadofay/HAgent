# Agent runtime, scopes, workspaces, and multi-agent coordination

## Goal

HAgent is a general-purpose agent runtime for applications that need to connect one or more LLM-backed agents to an application, simulation, game, or other host environment. A host may use HAgent for simple one-agent chat or for many concurrent agents with different roles, models, tools, memories, and lifetimes.

The configuration model and the runtime model must remain separate:

```text
Agent profile / definition
    = reusable configuration

Runtime agent instance
    = one live execution identity created from a profile
```

A host must be able to create many runtime instances from one default/configured profile without storing every runtime instance as a permanent configured agent.

## 0.9 Agent Runtime + Scope

### Problem

The current `AiAgent` model represents persistent agent configuration well, but the long-term platform needs a separate runtime identity for agents that exist because a form, task, world actor, user session, or other host object currently exists.

### Required capabilities

- [ ] Introduce a provider-neutral runtime agent instance abstraction.
- [ ] Keep reusable `AiAgent`/agent profile definitions separate from runtime instances.
- [ ] Support runtime scopes at minimum:
  - Application
  - Workspace
  - Form/Context
  - Session
  - Task
  - Ephemeral
- [ ] Give every runtime instance a stable unique instance ID.
- [ ] Preserve profile ID separately from runtime instance ID.
- [ ] Allow a host to create/retire instances without creating permanent configuration records.
- [ ] Support independent provider/model settings per runtime instance through explicit profile or override policy.
- [ ] Preserve execution snapshots so configuration edits/deletion do not corrupt active work.
- [ ] Support multiple runtime instances executing concurrently.
- [ ] Support cancellation, timeout, stale-result rejection, and independent latency per instance.

### Design rule

Do not create different agent classes for Manager, Specialist, Global, Form, or Session. These are roles/scopes/bindings over the same generic runtime agent abstraction.

## 0.9 Shared Workspace / Conversation

### Problem

A normal conversation assumes one assistant. HAgent must also support a shared workspace in which a human user and multiple agents participate while only the intended recipient receives an LLM request.

### Terminology

- **Workspace**: a shared communication context containing participants, messages, routing policy, and optional persistence.
- **Agent participant**: a runtime agent instance attached to a workspace.
- **Default recipient**: the participant that receives an unaddressed user message.
- **Direct addressing**: a message explicitly addressed to one participant.
- **Delegation**: an agent sends an addressed instruction to another agent.
- **Broadcast**: an explicit host-approved operation that sends one event/message to multiple participants. It is never the default routing behavior.

### Example behavior

```text
User: "Show me unpaid invoices"
        ↓
Default recipient = Manager
        ↓
Manager reasons and may delegate
        ↓
Manager -> Invoice Specialist
        ↓
Invoice Specialist -> Manager
        ↓
Manager -> User
```

If the user explicitly addresses the Invoice Specialist, that specialist receives the request instead of the Manager.

An unaddressed message must not be sent to every participant merely because they share a workspace.

### Required capabilities

- [ ] Shared workspace abstraction independent of WinForms.
- [ ] Participant registration/removal.
- [ ] Default-recipient policy.
- [ ] Direct agent addressing.
- [ ] Host-configurable address syntax; `@name`/`@id` is one possible presentation, not a Core requirement.
- [ ] Agent-to-agent addressed messages.
- [ ] User-to-agent addressed messages.
- [ ] Optional explicit broadcast capability.
- [ ] Message correlation IDs and conversation ordering.
- [ ] Per-message sender/recipient/causation metadata.
- [ ] Protection against routing loops and accidental recursive delegation.
- [ ] Workspace budgets for turns, hops, agent invocations, tokens, and time.

## 0.9 Manager / Specialist role model

### Manager agent

The host may designate one runtime participant as the default manager/coordinator for a workspace. HAgent should not hard-code the word "manager" into the runtime API.

The configured role should express responsibilities such as:

- receive unaddressed user requests;
- understand available specialists;
- delegate work explicitly;
- combine specialist results;
- answer the user;
- coordinate multi-step work.

### Specialist agent

A specialist is another runtime participant with its own profile, system prompt, tools, context, and memory policy.

A specialist may be created dynamically from a host-defined default specialist profile when a relevant form/context/task appears.

The specialist must not receive every workspace message automatically. It becomes active when directly addressed or explicitly invoked by the coordination policy.

## 0.9 Dynamic contextual agents

### Goal

Hosts should be able to create a specialist automatically when a runtime context appears, without adding permanent agent definitions for every form or object.

Example:

```text
Configured profile:
    Invoice Specialist

User opens Invoice window
        ↓
Host creates runtime agent instance
        ↓
Instance receives:
    UI context
    data-source context
    application object context
    allowed tools
    specialist system prompt
        ↓
Instance becomes a workspace participant
```

The runtime instance may disappear when its host context closes, unless the host explicitly persists its runtime state.

### Required capabilities

- [ ] Create runtime agent instances from configured default profiles.
- [ ] Apply host-generated system/context information at runtime without mutating the stored profile.
- [ ] Attach UI/application/data/task context according to explicit permissions.
- [ ] Associate the instance with its source context ID.
- [ ] Retire the instance when the host context closes.
- [ ] Optionally persist runtime state when the host requires restart/recovery.

## 0.9 Memory isolation and ownership

Every runtime agent instance must be able to use a distinct memory ownership identity even when several instances were created from the same profile.

Example:

```text
Invoice profile
    ├── Invoice agent instance #184
    │     └── private memory
    ├── Invoice agent instance #219
    │     └── private memory
    └── Invoice agent instance #305
          └── private memory
```

Memory sharing must be explicit through workspace/application/group scopes. One agent must not read another agent's private memory merely because both use the same profile.

### Required capabilities

- [ ] Runtime-instance memory owner identity.
- [ ] Private/shared memory policy.
- [ ] Workspace memory policy distinct from agent-private memory.
- [ ] Provenance identifying which instance produced a memory.
- [ ] Clear behavior for retired/deleted instances.

## 0.9 Multi-process / multi-user persistence

A host may run several application processes against the same database.

Configuration records represent reusable definitions. Runtime instance records, when persistence is enabled, must carry enough identity to avoid collisions between users/processes.

At minimum the runtime identity model should be able to distinguish:

```text
Application installation / host instance
User/session
Workspace
Runtime agent instance
```

A local-file deployment may keep runtime instances in memory. A networked database deployment may persist runtime instances when restart/recovery, collaboration, audit, or cross-process visibility requires it.

Do not automatically persist every dynamically created agent.

## 0.9 HWorld consumer requirements

HWorld is an explicit external consumer of HAgent and must remain independent of HAgent at its core boundary.

HWorld currently defines the world as authoritative for world state, simulation time, physics, perception, action validation, and scheduling, while external cognition such as HAgent supplies model/provider execution, tool routing, memory/knowledge integrations, and decisions. HWorld's current `HWorld.Core` targets `netstandard2.0`; its WinForms Example targets `net481`. HAgent integration therefore belongs at the external cognition/decision boundary rather than inside `HWorld.Core`.

HAgent must support HWorld without knowing any HWorld-specific type or action name.

HWorld requires HAgent to support:

- multiple independent runtime agents concurrently;
- different providers/models/settings for different agents;
- independent agent memories;
- asynchronous execution that does not block external simulation time;
- immutable caller-provided observation/context snapshots;
- cancellation, timeout, and stale-result handling;
- generic structured tool calls with external validation/application of real state;
- compact context construction and token/usage telemetry;
- external scheduling of reasoning requests;
- deterministic correlation between an observation/context version and a decision result where the host requires it;
- optional multimodal context without assuming images are always available.

HAgent must explicitly remain ignorant of:

- HWorld world state;
- HWorld physics/collision;
- HWorld camera geometry;
- HWorld simulation time;
- HWorld rendering;
- HWorld-specific actions/entities;
- HWorld generational rules.

The HWorld integration itself should be implemented in HWorld as an adapter around the HAgent runtime, not by adding HWorld references to HAgent.

## 0.9 Testing requirements

Each major runtime capability must have a deterministic Example verification that does not require HWorld or a real provider unless a live-provider scenario is specifically intended.

At minimum cover:

- two or more runtime instances executing concurrently;
- separate profiles producing separate instance identities;
- independent memories;
- workspace routing with one default recipient;
- direct user-to-specialist routing;
- manager-to-specialist delegation;
- specialist-to-manager response routing;
- routing-loop protection;
- cancellation/stale-result protection;
- runtime-instance retirement;
- optional persistence isolation for multiple host/user instances.

## Non-goals

- [ ] HAgent does not become a workflow engine merely because it can coordinate agents.
- [ ] HAgent does not hard-code business roles such as invoice manager or customer specialist.
- [ ] HAgent does not require a chat UI to use multi-agent runtime features.
- [ ] HAgent does not require all workspace participants to be LLM-backed.
- [ ] HAgent does not broadcast every message to every agent.
- [ ] HAgent does not persist every runtime instance by default.
- [ ] HAgent does not import HWorld or any game/business application types into Core.

# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

This directory is the authoritative implementation ledger. The root `plan.md` is generated from these files.

## Current state

- Target frameworks: `.NET Framework 4.8.1` and `.NET 9`.
- `HAgent.Example` is the manual integration/verification host.
- User-facing WinForms uses the project's `Header`, `HMessage`, and `HButton` helpers.
- Core remains provider-neutral and lightweight.
- Base memory does not require GPU, vector database, or a large resident model.
- Initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Extension tools are deferred.

## Working rule

A feature is marked complete only after its implementation exists and matching `HAgent.Example` verification has passed locally.

## Development workflow

1. Implement one focused slice.
2. Add or update an Example test for that slice.
3. User builds and tests locally.
4. Record the actual result here.
5. Only then mark the slice complete.
6. Keep root `plan.md` and `roadmap.md` generated from these smaller source files.

## 0.1 Foundation
- [x] Multi-target .NET 4.8.1 and .NET 9.
- [x] Provider/agent models and multi-provider references.
- [x] OpenAI-compatible adapter.
- [x] File/SQL Server/MySQL persistence foundations.
- [x] Protected local secrets.
- [x] Provider/agent/tool management UI.
- [x] HAgent.Example integration host and modular tests.

## 0.2 Runtime
- [x] Execution lifecycle and stable execution IDs.
- [x] Provider routing, attempts, retries, timeout, cancellation.
- [x] Diagnostics and structured failure categories.
- [x] Actionable provider/model/account errors.
- [x] System-prompt resolution and failure detail preservation.

## 0.3 Memory + Context
- [x] Persistent JSONL memory and bounded search.
- [x] Explicit remember/recall/forget and scopes.
- [x] Typed Task/Event/Fact/Preference records.
- [x] Conversation store and persistent sessions.
- [x] Context budgets and tokenizer-free estimate.
- [x] Conservative automatic memory policy.
- [x] Lightweight relevance ranking.
- [x] Episodic memory with provenance.

## 0.4 Capabilities + Response Normalization
- [x] Tri-state capabilities and evidence/provenance.
- [x] Capability cache.
- [x] Normalized text/reasoning/raw/structured/tool/usage metadata.
- [x] `<think>` detection without assuming native reasoning.
- [x] Provider error classification/advice.
- [x] Streaming delta contract and OpenAI-compatible SSE.
- [x] Streaming cancellation.

## 0.5 Verified tool foundation
- [x] Six initial tool types.
- [x] Tool definition/handler separation.
- [x] Tool registry and application handler.
- [x] JSON Schema validation.
- [x] OpenAI-compatible tool transport.
- [x] Bounded multi-turn tool loop.
- [x] File tool-definition persistence.
- [x] Agent `ToolIds` assignment model.
- [x] Live Groq tool loop verification.

## 0.7 WinForms UI Context + Data Discovery
- [x] `IUiContext` and `WinFormsUiContext`.
- [x] Form and arbitrary control-tree/UserControl attachment with stable root identity.
- [x] UI-thread-safe read-only inspection and control reads.
- [x] Bound/native `DataGridView` data-source extraction without mandatory `DataTable` normalization.
- [x] Semantic control discovery.
- [x] Data-source discovery for `DataTable`, `DataView`, `BindingSource`, `IList`, arrays, and compatible collections.
- [x] CurrencyManager, current-item, position/count, list/source metadata.
- [x] Control-to-source relationship discovery based on actual bindings and source identity.
- [x] Convention-based `IUiControlAdapter`.
- [x] Reflection adaptation of external `IHyperControl`-style controls using `DbFieldName`, `GetValue()`, and `SetValue(object)`.
- [x] Live application-object attachment by stable ID.
- [x] Bounded structural discovery of application-owned objects such as `TableInfo` without compile-time type knowledge.
- [x] `maxDepth` and `maxCollectionItems` resource bounds documented and enforced.
- [x] Provider-neutral structured data query contract with explicit fields, scalar filters, sorting, and bounded paging.
- [x] `HAgent.Example` verification for UI Context, UserControl, native `IList`, shared data relationships, custom control adaptation, application-object context, and structured data-query semantics.

## 0.5 Tools + Agent Loop

### Verified
- [x] Definition/handler separation.
- [x] Registry and direct execution.
- [x] JSON Schema validation before execution.
- [x] Provider tool-definition transport.
- [x] Bounded multi-turn loop.
- [x] File definition persistence.
- [x] Six initial tool categories.
- [x] Live Groq tool loop.
- [x] Per-agent tool assignment persisted through `AiAgent.ToolIds`.
- [x] Agent assignment Example verification.

### Remaining
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Application tool registration guidance/API conventions.
- [ ] Declarative execution engine.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history.
- [ ] Tool budgets and stronger loop detection.
- [ ] More capability negotiation around tool calling.

## 0.6 Safety
- [x] General permission configuration UI for the current WinForms policy.
- [x] Persist current UI permission policy through the public settings path.
- [ ] Read/write/invoke/export permissions across all tool types.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.8 Data Access + Authorization

### Objective
Build a safe data-access layer on top of the verified UI/data-discovery foundation. The model should be able to request structured data operations without receiving arbitrary SQL, reflection, application-object access, or database credentials as an implicit capability.

### Implemented foundation
- [x] Provider-neutral `DataProjectionRequest` with explicit field allow-list and bounded paging.
- [x] Provider-neutral `IDataProjectionSource` abstraction.
- [x] Provider-neutral `DataQueryRequest` with explicit fields, scalar filters, sorting, and bounded paging.
- [x] Provider-neutral `IDataQuerySource` abstraction.
- [x] Deterministic Example verification of structured query semantics.

### Next implementation slices
- [ ] Application-owned data adapter that maps approved application data sources to `IDataQuerySource`.
- [ ] Query schema/field allow-list policy independent of the model's requested field names.
- [ ] Separate data permissions for discovery, projection, query, export, and writes.
- [ ] SQL Server restricted query adapter using parameterized generated commands only.
- [ ] MySQL restricted query adapter using parameterized generated commands only.
- [ ] SQL Server/MySQL Example integration tests with developer-entered connection settings (server, user, password, database) and an explicitly disposable/read-only test database.
- [ ] Credential handling so test connection fields never become persistent agent/tool configuration or logs.
- [ ] Query result limits, command timeout, cancellation, and resource budgets.
- [ ] Read-only database tools before any database write tool.
- [ ] Explicit host authorization callbacks for database operations.

### Non-goals
- [ ] Raw SQL execution from model input.
- [ ] Arbitrary SQL fragments in `DataQueryRequest`.
- [ ] Implicit access to every table/column in a database.
- [ ] Treating UI bindings or `TableInfo` metadata as authorization.
- [ ] Persisting database passwords in normal configuration or tool definitions.

### Design decisions to preserve
- UI and application discovery remain convenience/introspection, never authorization.
- Application-owned objects remain live runtime references and are bounded during inspection.
- `DataTable` remains optional; native/bound/paged/streaming sources remain preferred.
- SQL Server and MySQL implementations stay outside `HAgent.Core`.

## Example host
- [x] Every current Example tab has editable input and expected-output guidance.
- [x] Every current Example tab has a copyable C# reproduction snippet beside its input.
- [ ] Every public-API snippet should become self-contained or link to a clearly identified shared setup snippet.
- [ ] Keep snippets synchronized whenever a public API used by an example changes.
- [x] Custom-control adapter and application-object context Examples are present and verified.
- [x] Structured data query Example verification is present and verified locally.
- [ ] Add live SQL Server integration Example when the restricted adapter is implemented.
- [ ] Maintain focused partial test files instead of returning to one monolithic `MainForm` implementation.

## Documentation workflow
- [x] Small source files under `docs/plan/` and `docs/roadmap/`.
- [x] Generated root `plan.md` and `roadmap.md` workflow.
- [x] Documentation source changes are part of implementation state.

## 0.9 Agent Scope + Chat
- [ ] Agent profile separated from runtime binding.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat with global/form agent selector.
- [ ] Persistent conversations and conversation switching/search.
- [ ] Streaming UI and tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Safe handling of deleted/disabled agents.
- [ ] Cross-form memory governed by scope and authorization policy.

## 0.10 Collaboration
- [ ] Agents-as-tools.
- [ ] Handoffs/delegation.
- [ ] Agent-to-agent messaging board/channels.
- [ ] Shared/private memory policies.
- [ ] Parallel execution and collaboration budgets.

## 0.11 Tasks + workflows
- [ ] Task/job lifecycle.
- [ ] Planning/execution/verification.
- [ ] Durable checkpoints and restart recovery.
- [ ] Scheduling/events/background work.
- [ ] Workflow budgets and observability.

## Later
- [ ] More provider adapters and multimodal support.
- [ ] Extension/provider/tool/UI-adapter DLL ecosystem.
- [ ] SQL/MySQL memory stores.
- [ ] Optional vector/MCP integrations.
- [ ] SDKs and developer diagnostics.
- [ ] Stable 1.0 contracts/NuGet.
- [ ] .NET 10 after migration to compatible Visual Studio.

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

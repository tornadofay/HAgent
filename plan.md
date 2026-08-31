# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

HAgent is a lightweight, provider-neutral .NET agent runtime. It can be used for simple chat or embedded into a host application, simulation, game, or other environment.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.8 Data Access + Authorization — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

## Verified platform foundations

- Provider/agent configuration and routing.
- Execution lifecycle, timeout, cancellation, retries, and diagnostics.
- Memory, persistent sessions, context budgeting, automatic/episodic/task memory.
- Capability discovery and response normalization.
- Streaming contracts and live streaming.
- Tool definitions, registry, schema validation, provider tool transport, bounded tool loops, persistence, and per-agent assignment.
- WinForms UI Context with Form/UserControl attachment.
- Semantic control and bound/native data-source discovery.
- CurrencyManager/current-item/source relationships.
- Convention-based custom control adapters.
- Bounded live application-object discovery.
- Provider-neutral structured data projection/query contracts.

## Development rule

A capability is complete only when implementation exists, the relevant `HAgent.Example` verification passes locally, and project documentation reflects the result.

Implement one focused slice at a time. Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — engineering invariants and non-negotiable rules.
- `docs/architecture/` — stable architectural design.
- `docs/plan/` — current implementation state and completed ledger.
- `docs/roadmap/` — future phases and ordering.
- `docs/storage.md` — persistence/storage-specific design.

Root `plan.md` and `roadmap.md` are generated from the corresponding source directories.

Only completed work belongs here. Future work is not listed as planned items.

## 0.1 Foundation

- Provider/agent configuration models.
- OpenAI-compatible provider foundation.
- File, SQL Server, and MySQL persistence foundations.
- Protected local secrets.
- Provider/agent/tool management UI.
- HAgent.Example integration host.

## 0.2 Runtime

- Stable execution IDs and lifecycle state.
- Provider routing and fallback candidates.
- Retries, timeout, and cancellation.
- Provider error classification and actionable failures.
- Execution snapshots.
- System-prompt resolution.

## 0.3 Memory + Context

- Persistent JSONL memory.
- Explicit remember/recall/forget.
- Agent/task/event/fact/preference memory records.
- Persistent conversations and sessions.
- Context budgets and tokenizer-free estimation.
- Conservative automatic memory.
- Lightweight relevance ranking.
- Episodic memory with provenance.

## 0.4 Capabilities + Response Normalization

- Tri-state provider/model capabilities with evidence.
- Capability caching.
- Normalized text, reasoning, structured output, tool calls, usage, and provider metadata.
- Reasoning separation and diagnostic `<think>` handling.
- Provider error advice.
- Streaming contract, OpenAI-compatible SSE, cancellation, and live streaming verification.

## 0.5 Tool Foundation

- BuiltIn, Application, Declarative, UI, SqlServer, and MySql tool types.
- Definition/handler separation.
- Tool registry and application handlers.
- JSON Schema validation.
- Provider tool-definition transport.
- Bounded multi-turn tool loop.
- Tool-definition persistence.
- Agent `ToolIds` assignment.
- Live Groq tool-loop verification.

## 0.7 UI Context + Data Discovery

- Form and arbitrary WinForms control-tree/UserControl attachment.
- Stable logical root identity.
- UI-thread-safe read-only inspection and control reads.
- Native/bound `DataGridView` data extraction without mandatory `DataTable` normalization.
- Standard semantic control discovery.
- Data-source discovery for DataTable, DataView, BindingSource, IList, arrays, and compatible collections.
- CurrencyManager/current-item/position/count metadata.
- Control-to-source relationship discovery.
- Convention-based custom control adapters.
- External `IHyperControl`-style adaptation without assembly dependency.
- Live application-object attachment and bounded structural discovery.
- `maxDepth` and `maxCollectionItems` resource bounds.
- Provider-neutral structured data projection/query contracts.
- Verified HAgent.Example coverage for the complete 0.7 slice.

## Verification rule

A completed milestone is based on actual local verification, not merely code existence.

Only the current implementation milestone belongs here. Completed work is recorded in `10-completed.md`; future work belongs under `docs/roadmap/`.

## 0.8 Data Access + Authorization

### Objective
Turn the verified structured-query contracts into safe application and database access. No arbitrary SQL, unrestricted reflection, or implicit authorization.

### Current slices

- [ ] Application-owned adapter implementing `IDataQuerySource` for explicitly approved sources.
- [ ] Authoritative schema/field allow-list independent of model requests.
- [ ] Data permissions separated into discovery, projection/query, export, and write operations.
- [ ] Host authorization callback contract.
- [ ] Query limits, cancellation, timeout, and resource budgets.
- [ ] Restricted SQL Server adapter using generated parameterized commands only.
- [ ] Restricted MySQL adapter using generated parameterized commands only.
- [ ] Read-only database tools and result/audit metadata before database writes.

### Live Example

When the SQL Server adapter is ready, `HAgent.Example` will provide runtime-only test fields:

```text
Server Name
User Name
Password
Database
```

The Example will target an explicitly disposable/read-only test database and verify connection, authorized schema/fields, structured queries, bounded results, cancellation/timeout, and unauthorized-operation rejection.

Connection values must never become persisted agent/tool configuration or normal logs.

### Non-goals

- Raw SQL from model input.
- SQL fragments embedded in `DataQueryRequest`.
- Implicit permission to every table or column.
- Treating UI binding, `TableInfo`, object provenance, or model instructions as authorization.
- Persisting test database passwords as ordinary configuration.

## Definition of done

0.8 is complete only after the restricted application/database path is implemented and the matching `HAgent.Example` verification passes locally.

## Goal

HAgent is a general-purpose agent runtime for applications that need to connect one or more LLM-backed agents to an application, simulation, game, or other host environment. A host may use HAgent for simple one-agent chat or for many concurrent agents with different roles, models, tools, memories, contexts, and lifetimes.

The configuration model and runtime model must remain separate:

```text
Agent profile / definition
    = reusable configuration

Runtime agent instance
    = one live execution identity created from a profile
```

A host must be able to create many runtime instances from one default/configured profile without storing every runtime instance as a permanent configured agent.

## 0.9 Agent Runtime + Scope

### Problem

The current `AiAgent` model represents persistent agent configuration well, but the long-term platform needs a separate runtime identity for agents that exist because a form, workspace, task, world actor, user session, or other host context currently exists.

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

### Lifetime rule

A runtime agent instance does not automatically retire merely because a related form/workspace is no longer active. The host decides its lifecycle. An instance normally remains a workspace participant until one of these occurs:

- the host explicitly retires it;
- the host application/process closes;
- a higher-level lifecycle policy explicitly expires it.

Automatic context creation and explicit retirement are separate operations.

### Design rule

Do not create different agent classes for Manager, Specialist, Global, Form, or Session. These are roles/scopes/bindings over the same generic runtime agent abstraction.

## 0.9 Shared Workspace / Conversation

### Problem

A normal conversation assumes one assistant. HAgent must also support a shared workspace in which a human user and multiple agents participate while requests are routed only to the intended recipient.

### Terminology

- **Workspace**: a shared communication context containing participants, messages, routing policy, and optional persistence.
- **Agent participant**: a runtime agent instance attached to a workspace.
- **Coordinator**: the participant designated by workspace policy as the default recipient for unaddressed user messages. HAgent must not hard-code a business role named “manager”.
- **Specialist**: a participant assigned a narrower responsibility, context, or capability by the host.
- **Default recipient**: the participant that receives an unaddressed user message.
- **Direct addressing**: a message explicitly addressed to one participant.
- **Delegation**: an agent sends an addressed instruction to another agent.
- **Broadcast**: an explicit host-approved operation that sends a message/event to multiple participants. It is never the default routing behavior.

### Visible collaboration rule

Agent-to-agent work is part of the workspace conversation when the host enables it. The user should be able to see the coordinator delegate to a specialist, the specialist respond, and the coordinator continue the work. These are real workspace messages with sender/recipient metadata; they are not hidden internal calls that appear only as a final answer.

Example:

```text
User: What invoices are overdue?
        ↓
Coordinator
        ↓ visible workspace message
Invoice Specialist: Review the authorized invoice data and identify overdue invoices.
        ↓ visible workspace response
Invoice Specialist: I found 8 overdue invoices totaling ...
        ↓
Coordinator
        ↓
User: There are 8 overdue invoices ...
```

Only the addressed recipient starts an LLM request for that message. Other participants observe the message through the workspace history according to workspace visibility policy; they do not independently execute a model request unless explicitly addressed or invoked by policy.

If the user explicitly addresses a specialist, that specialist receives the user request. The coordinator does not automatically execute another LLM request for the same message.

### Required capabilities

- [ ] Shared workspace abstraction independent of WinForms.
- [ ] Participant registration/removal/retirement.
- [ ] Default-recipient policy.
- [ ] Direct agent addressing.
- [ ] Host-configurable address syntax; `@name`/`@id` is one possible presentation, not a Core requirement.
- [ ] Agent-to-agent addressed messages.
- [ ] User-to-agent addressed messages.
- [ ] Visible agent-to-agent conversation events.
- [ ] Optional explicit broadcast capability.
- [ ] Message correlation IDs and conversation ordering.
- [ ] Per-message sender/recipient/causation metadata.
- [ ] Protection against routing loops and accidental recursive delegation.
- [ ] Workspace budgets for turns, hops, agent invocations, tokens, and time.
- [ ] Visibility policy so hosts can choose which internal messages are shown to the user.

## 0.9 Coordinator / Specialist role model

### Coordinator

The host may designate one runtime participant as the default coordinator for a workspace. The coordinator receives unaddressed user requests, understands available specialists, delegates explicitly, combines results, and answers the user. HAgent should not hard-code business-specific coordinator behavior.

### Specialist

A specialist is another runtime participant with its own profile, system prompt, tools, context, and memory policy.

A specialist may represent an entire application domain, table, subsystem, world capability, or other host-defined responsibility. For example, an “Invoice Specialist” can understand and operate over the authorized invoice data source as a whole; it is not inherently tied to one invoice record.

A specialist should be able to inspect the context made available to it and give its honest assessment of what it knows, what it inferred, what remains unknown, and what it is not authorized to access. It must not be forced to claim certainty merely because its profile names a domain.

The specialist does not receive every workspace message automatically. It becomes active when directly addressed or explicitly invoked by the coordination policy.

## 0.9 Dynamic contextual agents

### Goal

Hosts should be able to create a specialist automatically when a runtime context appears, without adding permanent agent definitions for every form, table, object, or instance.

Example:

```text
Configured default profile:
    Invoice Specialist

User opens Invoice/Purchases area
        ↓
Host creates a runtime specialist instance
        ↓
Instance receives according to policy:
    UI context
    data-source context
    application-object context
    authorized tools
    specialist system prompt
        ↓
Instance joins the workspace
```

The instance normally remains active after the form/window that caused its creation closes if the host has chosen to keep it in the workspace. The host explicitly retires it when appropriate, or the application shutdown policy retires all live instances.

### Required capabilities

- [ ] Create runtime agent instances from configured default profiles.
- [ ] Apply host-generated system/context information at runtime without mutating the stored profile.
- [ ] Attach UI/application/data/task context according to explicit permissions.
- [ ] Associate the instance with its source context ID.
- [ ] Keep the instance alive independently from the source form when the host policy requires it.
- [ ] Explicitly retire an instance.
- [ ] Optionally persist runtime state when the host requires restart/recovery.

## 0.9 Memory isolation and ownership

Every runtime agent instance must be able to use a distinct memory ownership identity even when several instances were created from the same profile.

Example:

```text
Invoice Specialist profile
    ├── runtime instance A
    │     └── private memory
    ├── runtime instance B
    │     └── private memory
    └── runtime instance C
          └── private memory
```

Because a specialist may represent a whole data domain rather than one record, its private memory can contain durable observations about the domain, schema, workflow, and previous work according to the host's policy. Record-specific memories require an additional context/ownership boundary when necessary.

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
- visible coordinator-to-specialist delegation and specialist-to-coordinator response;
- only intended recipients executing model requests;
- routing-loop protection;
- cancellation/stale-result protection;
- explicit runtime-instance retirement;
- instance persistence isolation for multiple host/user instances;
- specialist inspection of a whole domain/data source without assuming a single record identity.

## Non-goals

- [ ] HAgent does not become a workflow engine merely because it can coordinate agents.
- [ ] HAgent does not hard-code business roles such as invoice manager or customer specialist.
- [ ] HAgent does not require a chat UI to use multi-agent runtime features.
- [ ] HAgent does not require all workspace participants to be LLM-backed.
- [ ] HAgent does not broadcast every message to every agent.
- [ ] HAgent does not persist every runtime instance by default.
- [ ] HAgent does not import HWorld or any game/business application types into Core.

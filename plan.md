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

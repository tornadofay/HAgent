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

## 0.5 Verified tool loop foundation
- [x] Six initial tool types.
- [x] Tool definition/handler separation.
- [x] Tool registry and application handler.
- [x] JSON Schema validation.
- [x] OpenAI-compatible tool transport.
- [x] Bounded multi-turn tool loop.
- [x] File tool-definition persistence.
- [x] Agent `ToolIds` assignment model.
- [x] Live Groq tool loop verification.

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
- [ ] SQL Server tool execution.
- [ ] MySQL tool execution.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history.
- [ ] Tool budgets and stronger loop detection.
- [ ] More capability negotiation around tool calling.

## 0.6 Safety
- [ ] Read/write/invoke/export permissions across all tool types.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.7 WinForms UI Context

### Implemented foundation
- [x] `IUiContext` and `WinFormsUiContext`.
- [x] Stable control lookup by WinForms control name.
- [x] UI-thread dispatch for context reads.
- [x] `UiControlSnapshot` inspection model.
- [x] `ui.inspect`, `ui.read_control`, and `ui.read_data` read-only tools.
- [x] Bound/native `DataGridView` source preference.
- [x] Bounded data extraction with lazy adaptation.
- [x] `DataTable` treated as optional rather than the default representation.
- [x] Coarse `UiAutomationPermissions` policy.
- [x] Built-in UI tools enforce the permission policy.
- [x] Permission policy defaults to no automatic discovery/write/invoke.

### Current design
- [ ] Automatic UI discovery should be optional convenience behavior, never implicit authority.
- [ ] Developers may replace the coarse policy with application-specific authorization logic.
- [ ] “Form serialization” is treated as UI context/adapter behavior, not as arbitrary object serialization.
- [ ] Explicit developer abstractions remain a supported path for domain concepts such as Customer, Invoice, and Contact.
- [ ] Automatic semantic discovery should be able to identify useful controls and bound data sources without forcing developers to write wrappers.
- [ ] Automatic data querying against application/SQL sources requires explicit permissions and restricted query tools; never give the model arbitrary database access by default.
- [ ] Cross-form discovery/memory requires explicit scope and policy.

### Next
- [ ] Permission configuration UI in AI Settings.
- [ ] Persist permission policy.
- [ ] Form/UserControl attachment and stable logical form identity.
- [ ] Semantic control discovery beyond exact `Name` lookup.
- [ ] BindingSource/CurrencyManager/IList/native collection adapters.
- [ ] Safe data projection/query abstraction.
- [ ] SQL Server/MySQL read/query tools under separate restricted permissions.
- [ ] UI write/invoke tools only after permission/approval foundation.
- [ ] `HAgentHost.Attach(ai, form)` bridge and floating assistant/flyout.

## 0.8 Chat + scopes
- [ ] Agent profile separated from runtime binding.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat with global/form agent selector.
- [ ] Persistent conversations and conversation switching/search.
- [ ] Streaming UI and tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Safe handling of deleted/disabled agents.
- [ ] Cross-form memory governed by scope and authorization policy.

## 0.9 Collaboration
- [ ] Agents-as-tools.
- [ ] Handoffs/delegation.
- [ ] Agent-to-agent messaging board/channels.
- [ ] Shared/private memory policies.
- [ ] Parallel execution and collaboration budgets.

## 0.10 Tasks + workflows
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

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
- [x] Per-agent tool assignment via persisted `ToolIds`.
- [x] Agent/tool persistence verification.

### Current work
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers beyond the initial UI read-only tools.
- [ ] Declarative execution engine.
- [ ] SQL Server tool execution.
- [ ] MySQL tool execution.
- [ ] Tool timeout/cancellation/progress policy.
- [ ] Tool audit/history.
- [ ] Tool budgets and stronger loop detection.
- [ ] More capability negotiation around tool calling.

## 0.6 Safety
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.7 WinForms UI Context

### Implemented foundation
- [x] `IUiContext` contract.
- [x] `WinFormsUiContext` attachment/read/inspection path.
- [x] Stable control lookup by WinForms control name.
- [x] UI state snapshots for form/control trees.
- [x] TextBox/ComboBox/CheckBox/RadioButton/NumericUpDown/DateTimePicker/ListBox/ListView/Label value extraction.
- [x] DataGridView bound-source extraction.
- [x] DataTable/DataView/native enumerable handling where naturally available.
- [x] Bounded row reads and cancellation checks.
- [x] Read-only `ui.inspect`, `ui.read_control`, and `ui.read_data` tools.
- [x] `HAgentHost.Attach(form, registry)` bridge.
- [x] Provider-independent Example UI Context test.

### Current work
- [ ] Data source adapters for BindingSource/CurrencyManager/IList and richer collection types.
- [ ] Public attach/detach lifecycle suitable for application use.
- [ ] Form/UserControl/custom-control semantic identity improvements.
- [ ] Floating assistant/flyout.
- [ ] `ui.write_control`, `ui.move_control`, `ui.resize_control`, `ui.invoke`.
- [ ] UI-thread dispatch integrated with host cancellation.
- [ ] Dry-run/preview and undo hooks.
- [ ] Per-control permissions and human approval integration.

### Mandatory representation rule
- [x] Prefer the lightest representation that preserves required information.
- [x] Prefer bound/native data sources over visible-cell scraping.
- [x] Avoid eager copying/materialization.
- [x] `DataTable` is optional, not the architectural default.
- [ ] Add paging/projection/streaming adapters for large sources where appropriate.

## 0.8 Chat + scopes
- [ ] Global/form/session/task/ephemeral scopes.
- [ ] User ↔ agent chat.
- [ ] Global/form agent selector.
- [ ] Persistent conversations.
- [ ] Streaming and tool activity UI.
- [ ] Reasoning visibility policy.
- [ ] Deleted/disabled agent handling.

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

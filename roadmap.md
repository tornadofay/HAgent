# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

HAgent is a lightweight, provider-neutral agent platform for .NET desktop applications. The roadmap is dependency-ordered so simple applications stay small while advanced deployments can add memory, tools, WinForms automation, collaboration, and workflows.

## Current position

- 0.1 Foundation — complete
- 0.2 Runtime — complete
- 0.3 Memory + Context — foundation complete
- 0.4 Provider Capabilities + Response Normalization — foundation complete
- 0.5 Tools + Agent Loop — active
- 0.7 WinForms UI Context is the next major platform layer after the tool foundation

The initial supported tool categories are **BuiltIn, Application, Declarative, UI, SQL Server, and MySQL**. Extension tools are intentionally deferred to the later extensibility milestone.

## 0.1 Foundation

Provider/agent models, multi-provider agent relationships, OpenAI-compatible provider adapter, File/SQL Server/MySQL configuration foundations, protected secrets, management UI, model discovery, connection testing, deletion rules, HAgent.Example, global agent selector/output, and modular Example tests.

## 0.2 Runtime

Execution lifecycle, stable IDs, provider routing, ordered candidates, retries, timeout/cancellation, diagnostics, provider failure classification, system-prompt resolution, and low-RAM/no-GPU constraints.

## 0.3 Memory + Context

Persistent JSONL memory, scopes, typed Task/Event/Fact/Preference records, explicit recall/forget, lightweight relevance ranking, persistent conversations/sessions, context budgets, tokenizer-free estimation, conservative automatic memory, task/event memory, and episodic memory.

## 0.4 Capabilities + Response Normalization

Tri-state capabilities with evidence/confidence, capability cache, response normalization, reasoning separation and `<think>` handling, structured JSON, normalized tool calls, normalized usage, provider-error advice, streaming deltas, OpenAI-compatible SSE, cancellation, and live streaming verification.

## 0.5 Tool foundation verified

Tool definitions, six initial tool types, handler/definition separation, registry and delegate tools, JSON Schema validation, OpenAI-compatible tool transport, bounded multi-turn tool loop, persisted File tool definitions, Agent `ToolIds`, and successful live Groq tool-loop verification.

## 0.5 Tools + Agent Loop

Tool definitions, validation, persistence, per-agent assignment, provider tool transport, deterministic tool loops, and live Groq tool calling are implemented.

Remaining tool work:

- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Declarative execution engine.
- [ ] SQL Server tool execution layer.
- [ ] MySQL tool execution layer.
- [ ] Tool aliases/versioning.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history and budgets.
- [ ] Stronger loop detection and provider/tool capability negotiation.

## 0.6 Safety + Permissions

- [ ] General permission configuration UI.
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

## 0.7 WinForms UI Context + Automation

The WinForms subsystem uses **UI Context**, not generic “form serialization”. Two supported development modes are intentional:

1. **Explicit domain abstraction** — the host can expose typed concepts such as Customer, Contact, Invoice, or a custom view-model/tool instead of allowing HAgent to inspect arbitrary controls.
2. **Automatic UI Context** — HAgent can discover controls, bound data, and useful relationships automatically when the host explicitly enables the appropriate permission policy.

Automatic discovery is convenience, not authority. Attaching a form must never automatically grant write or invoke access.

### Implemented foundation

- [x] `IUiContext` / `WinFormsUiContext`.
- [x] Stable control lookup by WinForms control name.
- [x] UI-thread-safe inspection/read operations.
- [x] `UiControlSnapshot`.
- [x] `ui.inspect`.
- [x] `ui.read_control`.
- [x] `ui.read_data`.
- [x] Bound/native DataGridView source preference.
- [x] Bounded data extraction.
- [x] Light-weight representation rule: avoid unnecessary `DataTable` materialization.
- [x] `UiAutomationPermissions` with safe defaults.
- [x] Read-only UI tools enforce the permission policy.

### Automatic data understanding

- [ ] Semantic discovery of common WinForms controls and relationships.
- [ ] Domain-friendly labels/semantic IDs in addition to raw control names.
- [ ] BindingSource/CurrencyManager/IList/collection adapters.
- [ ] Lazy/native data projections.
- [ ] Safe identification of tabular data without scraping visible cells when a bound source exists.
- [ ] Optional application-defined semantic adapters for Customer/Invoice/etc.
- [ ] Restricted query abstraction for application/SQL data rather than arbitrary SQL execution.

### Permission model

The initial coarse permission groups are:

- Automatic discovery.
- Read controls.
- Read data.
- Write controls.
- Invoke controls.

Developers may disable automatic behavior and implement their own authorization/abstraction path. Future SQL Server/MySQL query permissions must remain separate from UI permissions.

### UI automation

- [ ] Permission configuration tab in the main HAgent settings UI.
- [ ] Persist permission policy.
- [ ] Form/UserControl attachment and stable logical identity.
- [ ] `HAgentHost.Attach(ai, form)` bridge.
- [ ] Floating assistant/flyout.
- [ ] `ui.write_control`.
- [ ] `ui.move_control`.
- [ ] `ui.resize_control`.
- [ ] `ui.invoke` / approved click.
- [ ] `ui.enable_control` / `ui.disable_control`.
- [ ] Batch operations.
- [ ] Dry-run/preview.
- [ ] Human approval.
- [ ] Per-control permissions.
- [ ] Optional undo/rollback hooks.

## 0.3 advanced memory

- [ ] Richer automatic-memory inference without saving ordinary conversation by default.
- [ ] Memory update/upsert semantics.
- [ ] Retention/expiration policies.
- [ ] Larger-store indexing improvements.
- [ ] Context compaction/summarization.
- [ ] SQL Server/MySQL memory stores.
- [ ] Conversation listing/search/metadata management.
- [ ] Optional vector-memory adapter.
- [ ] Remote embedding providers without local GPU or RAM-heavy resident models.

## 0.8 Agent Scope + Chat

- [ ] Separate agent profile from runtime binding/lifetime.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat window with global/form agent selector.
- [ ] Conversation switching/search/metadata/persistence.
- [ ] Streaming UI and tool activity.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Safe handling of deleted/disabled agents.
- [ ] Cross-form memory governed by explicit scope and policy.

## 0.9 Orchestration + Collaboration

- [ ] Agents-as-tools.
- [ ] Handoffs/delegation.
- [ ] Agent-to-agent messaging board, channels, direct messages and broadcasts.
- [ ] Shared workspace context.
- [ ] Routing and collaboration budgets.
- [ ] Maximum hops/depth and loop detection.
- [ ] Shared/private memory policies.
- [ ] Parallel execution and human intervention.
- [ ] Active/disabled/retired/deleted lifecycle.

## 0.6 Safety + Guardrails + Approval + Observability

- [ ] Input/output/tool guardrails.
- [ ] Termination/tripwire rules.
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval and approval lifecycle/audit.
- [ ] Execution/provider/tool/memory budgets.
- [ ] Tracing, spans, timings, correlation IDs.
- [ ] Sensitive-data redaction and no-secret diagnostics.

## 0.10 Tasks + Workflows + Autonomy

- [ ] Explicit task/job model and lifecycle.
- [ ] Planning, execution, verification.
- [ ] Multi-step workflows, branching, background execution, scheduling.
- [ ] Pause/resume, durable checkpoints, restart recovery.
- [ ] Event-triggered agents.
- [ ] Per-step retry, approval, cancellation, leases.
- [ ] Workflow observability and autonomy budgets.

## 0.11 Provider ecosystem

- [ ] Azure OpenAI.
- [ ] Anthropic.
- [ ] Google/Gemini.
- [ ] Ollama.
- [ ] LM Studio.
- [ ] Custom HTTP providers.
- [ ] Multimodal and embedding providers.
- [ ] Provider-specific capability negotiation.
- [ ] Streaming implementations.
- [ ] Provider contract harness.

## 0.12 Extensibility + storage ecosystem

- [ ] Provider/tool/UI-adapter/storage DLL loading.
- [ ] Extension validation and failure isolation.
- [ ] Conversation and memory persistence across File/SQL Server/MySQL.
- [ ] Optional vector/MCP companion integrations.
- [ ] External secret stores and rotation.
- [ ] Configuration profiles/workspaces.
- [ ] Multi-user authorization hooks.
- [ ] Audit logging.

Extension tools are intentionally deferred until this milestone.

## 0.13 Developer platform

- [ ] Optional DI integration.
- [ ] Optional `Microsoft.Extensions.AI` integration.
- [ ] Provider SDK.
- [ ] Tool SDK.
- [ ] UI-context/control-adapter SDK.
- [ ] Simulation/test mode.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public capabilities.

## 1.0

- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packages and signed releases.
- [ ] Comprehensive integration/security/provider/tool/memory tests.
- [ ] Documentation for providers, tools, memory, UI automation and workflows.
- [ ] .NET 10 after migration to compatible Visual Studio.

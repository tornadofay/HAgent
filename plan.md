# HAgent Development Plan

This file is the implementation ledger. Keep it synchronized with the repository. A feature is marked complete only after implementation and matching `HAgent.Example` verification exist.

## 0.1 Foundation — completed
- [x] .NET 4.8.1 + .NET 9 multi-targeting.
- [x] Provider-neutral provider/agent/tool models and persistence foundations.
- [x] OpenAI-compatible provider adapter.
- [x] Protected local secret storage.
- [x] File, SQL Server, and MySQL storage foundations.
- [x] Provider/agent/tool management UI with HAgent Header/HMessage/HButton conventions.
- [x] Provider testing and model catalog discovery.
- [x] Agent/provider relationship and deletion rules.
- [x] `HAgent.Example` integration host, global agent selector, global output, and split partial test files.

## 0.2 Runtime — completed
- [x] Agent runtime/execution lifecycle and stable execution IDs.
- [x] Provider routing, ordered candidates, attempts, retries, timeout, cancellation.
- [x] Execution diagnostics and structured failure categories.
- [x] Actionable provider/model/account error diagnostics.
- [x] System-prompt resolution and provider failure preservation.
- [x] Low-RAM/no-GPU design constraints.

## 0.3 Memory + Context — completed foundation
- [x] `IMemoryStore`, JSONL file memory, streaming search, remember/recall/forget.
- [x] Memory scopes: session, task, agent, user, application, shared.
- [x] Typed memory: Fact, Preference, Task, Event.
- [x] Metadata, task filtering, bounded recall, timestamps/provenance.
- [x] Conversation store, persistent sessions, reopening, rollback.
- [x] Context budgets, deterministic selection, tokenizer-free token estimate.
- [x] Conservative automatic memory policy.
- [x] Lightweight relevance ranking.
- [x] Compact episodic memory with task/session provenance.
- [x] `HAgent.Example` verification for all completed memory/context layers.

### Advanced memory — future
- [ ] Richer automatic-memory inference.
- [ ] Memory update/upsert semantics.
- [ ] Retention/expiration policies.
- [ ] Larger-store indexing improvements.
- [ ] Context compaction/summarization.
- [ ] SQL Server/MySQL memory stores.
- [ ] Conversation listing/search/metadata management.
- [ ] Optional vector-memory adapter.
- [ ] Remote embedding providers without requiring local GPU/RAM-heavy models.

## 0.4 Provider Capabilities + Response Normalization — completed foundation
- [x] Tri-state capabilities: Supported / Unsupported / Unknown.
- [x] Capability evidence: source, confidence, observed time, notes.
- [x] Capability cache with failure eviction and reset.
- [x] Basic capability-aware model suitability/routing.
- [x] Provider Editor and Agent Editor capability visibility.
- [x] Separate response text, reasoning, raw text, structured JSON, tool calls, usage, metadata.
- [x] Explicit `reasoning_content` normalization.
- [x] `<think>` detection without falsely claiming native reasoning.
- [x] Structured tool-call normalization.
- [x] Token usage normalization; no invented cost.
- [x] Provider error classification/advice for model/account/provider failures.
- [x] Provider-neutral streaming delta contract.
- [x] OpenAI-compatible SSE streaming.
- [x] `HAgentClient.StreamAsync(...)`.
- [x] Streaming cancellation.
- [x] Contract and live streaming Example verification.

### 0.4 remaining
- [ ] Capability override/configuration UI where explicit declaration is required.
- [ ] Rich provider capability discovery.
- [ ] Capability-aware suitability beyond Chat/Streaming for tools, vision, structured output, audio, embeddings, reasoning.
- [ ] Application reasoning policy: display/store/log/discard.

## 0.5 Tools + Agent Loop — active

### Implemented foundation
- [x] `AiTool` definition model.
- [x] Explicit `AiToolType`: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- [x] `IAgentTool` definition/handler separation.
- [x] `IToolRegistry` abstraction.
- [x] `InMemoryToolRegistry`.
- [x] `DelegateAgentTool` for code-defined custom tools.
- [x] `HAgentClient` tool registration, lookup, definition inspection, and direct execution.
- [x] Tool type selection in the Tool editor.
- [x] Deterministic Example tool-registry test.

### Next implementation
- [ ] JSON Schema validation and safe argument binding.
- [ ] Provider tool-definition transport.
- [ ] Provider-neutral tool-call loop.
- [ ] Tool result/observation messages.
- [ ] Multiple model/tool turns.
- [ ] Per-agent tool selection from persisted `ToolIds`.
- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Application tool registration guidance/API conventions.
- [ ] Declarative tool execution engine.
- [ ] WinForms UI tool handlers.
- [ ] SQL Server tool layer.
- [ ] MySQL tool layer.
- [ ] Tool aliases/versioning.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history.
- [ ] Tool-call and turn limits.
- [ ] Loop detection.
- [ ] Tool budgets.
- [ ] Tool/provider capability negotiation.
- [ ] Complete tool configuration UI behavior.
- [ ] Manual multi-step tool-loop Example.

### Initial tool types
- BuiltIn — supplied by HAgent.
- Application — executable handler registered by the host application.
- Declarative — safe configuration-driven operation, never arbitrary code execution.
- UI — supplied by `HAgent.WinForms` control/context adapters.
- SQL Server — supplied by the SQL Server tool layer with restricted database operations.
- MySQL — supplied by the MySQL tool layer with restricted database operations.
- Extension tools are deliberately deferred to a future extensibility milestone.

## 0.6 Safety + Guardrails + Approval + Budgets + Observability
- [ ] Input/output/tool guardrails.
- [ ] Termination/tripwire rules.
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval and approval lifecycle/audit.
- [ ] Execution/provider/tool/memory budgets.
- [ ] Tracing, spans, timings, correlation IDs.
- [ ] Sensitive-data redaction and no-secret diagnostics.

## 0.7 WinForms UI Context + Application Automation
- [ ] Form/UserControl/custom-control attachment.
- [ ] Stable control identity and discovery.
- [ ] Safe UI snapshots and provider-neutral context representation.
- [ ] Lazy/native data-source adapters.
- [ ] DataGridView, BindingSource, CurrencyManager, IList and tabular-source handling.
- [ ] TextBox/RichTextBox/ComboBox/Button/CheckBox/RadioButton/DateTimePicker/NumericUpDown/ListBox/ListView/TreeView adapters.
- [ ] `HAgentHost.Attach(ai, form)`-style bridge.
- [ ] Floating assistant/flyout.
- [ ] `ui.inspect`, `ui.read_control`, `ui.read_data`, `ui.write_control`, `ui.move_control`, `ui.resize_control`, `ui.invoke`, enable/disable, batch, dry-run, undo hooks.
- [ ] UI-thread dispatch and per-control permissions.

### Mandatory performance rule
- [ ] Prefer the lightest representation that preserves required information.
- [ ] Prefer bound/native data sources over scraping visible cells.
- [ ] Adapt lazily; avoid unnecessary copies/materialization.
- [ ] `DataTable` is optional, never the architectural default.
- [ ] Prefer paging/streaming/projection/native representations for large data.

## 0.8 Agent Scope + Chat
- [ ] Agent profile separated from runtime binding.
- [ ] Application/global, form, session, task, ephemeral scopes.
- [ ] User ↔ agent chat with global/form agent selector.
- [ ] Conversation switching/search/metadata/persistence.
- [ ] Streaming UI and tool activity.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Safe handling of deleted/disabled agents.
- [ ] Cross-form memory governed by scope/policy.

## 0.9 Agent Orchestration + Collaboration
- [ ] Agents-as-tools.
- [ ] Handoffs/delegation and specialist agents.
- [ ] Agent-to-agent messaging board/channels/direct/broadcast.
- [ ] Shared workspace context.
- [ ] Routing and collaboration budgets.
- [ ] Maximum hops/depth and loop detection.
- [ ] Shared/private memory policies.
- [ ] Parallel execution and human intervention.
- [ ] Active/disabled/retired/deleted lifecycle.

## 0.10 Tasks + Workflows + Autonomy
- [ ] Explicit task/job model and lifecycle.
- [ ] Planning/execution/verification.
- [ ] Multi-step workflows, branching, background execution, scheduling.
- [ ] Pause/resume, durable checkpoints, restart recovery.
- [ ] Event-triggered agents.
- [ ] Per-step retry, approval, cancellation, leases.
- [ ] Workflow observability and autonomy budgets.

## 0.11 Provider Ecosystem
- [ ] Azure OpenAI.
- [ ] Anthropic.
- [ ] Google/Gemini.
- [ ] Ollama.
- [ ] LM Studio.
- [ ] Custom HTTP providers.
- [ ] Multimodal and embedding providers.
- [ ] Provider-specific capability negotiation.
- [ ] Streaming implementations.
- [ ] Provider contract test harness.
- [ ] Versioned extension contract.

## 0.12 Extensibility + Storage Ecosystem
- [ ] Provider/tool/UI-adapter/storage DLL loading.
- [ ] Extension validation/failure isolation.
- [ ] Conversation and memory persistence across File/SQL Server/MySQL.
- [ ] Optional vector/MCP companion integrations.
- [ ] External secret stores and rotation.
- [ ] Configuration profiles/workspaces.
- [ ] Multi-user authorization hooks.
- [ ] Audit logging.

## 0.13 Developer Platform
- [ ] Optional DI integration.
- [ ] Optional `Microsoft.Extensions.AI` integration.
- [ ] Provider SDK.
- [ ] Tool SDK.
- [ ] UI-context/control-adapter SDK.
- [ ] Simulation/test mode.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public capabilities.

## 1.0 Stable Platform
- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packages and signed releases.
- [ ] Comprehensive integration/security/provider/tool/memory tests.
- [ ] Provider/tool/UI automation/memory/workflow documentation.
- [ ] .NET 10 after migration to compatible Visual Studio.

## Architecture rules
1. Core stays provider-neutral and dependency-light.
2. Provider transport, agent profile, runtime, memory, tools, and host side effects stay separate.
3. Capabilities are explicit; model names are never capability guarantees.
4. Capability claims preserve provenance/confidence where practical.
5. Prompts are not security boundaries.
6. Tools expose explicit capabilities; they never imply arbitrary host access.
7. Sensitive actions may require human approval.
8. Autonomous work is cancellable, observable, and budgeted.
9. Provider responses are normalized without destroying useful provider metadata.
10. Reasoning is optional separate response data when explicitly exposed; `<think>` markup alone is not treated as native reasoning.
11. UI Context describes state; tools define permitted actions.
12. “Form serialization” is a UI Context capability, not the name of the entire WinForms subsystem.
13. WinForms data access must prefer the lightest native/bound representation and avoid unnecessary materialization.
14. Cross-form memory uses explicit scopes and provenance; provenance is not authorization.
15. No local GPU, vector database, or heavy resident embedding model is required for the base memory design.
16. Tool configuration defines the contract and handler binding; it must never turn arbitrary configuration text into arbitrary code execution.
17. Initial tool categories are BuiltIn, Application, Declarative, UI, SqlServer, and MySql. Extension tools are deferred.
18. `HAgent.Example` is part of the development workflow and must cover meaningful completed public capabilities.

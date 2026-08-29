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

### Completed

- Definition/handler separation.
- Registry and direct execution.
- JSON Schema validation before execution.
- Provider tool-definition transport.
- Bounded multi-turn tool loop.
- File tool-definition persistence.
- Six initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Per-agent tool selection using persisted `ToolIds`.
- Agent/tool persistence verification.
- Live Groq tool-loop verification.

### Next

- Per-session temporary tools.
- Built-in tool handlers beyond the initial UI read-only tools.
- Declarative execution engine.
- SQL Server tool execution layer.
- MySQL tool execution layer.
- Tool timeout/cancellation/progress policies.
- Tool audit/history and budgets.
- Stronger loop detection and policy controls.
- Provider/tool capability negotiation beyond basic tool calling.

Extension tools are deliberately deferred to the later extensibility milestone.

## 0.7 WinForms UI Context + Automation

### Implemented foundation

- `IUiContext` contract.
- `WinFormsUiContext` attach/inspect/read operations.
- Stable lookup by WinForms control name.
- Lightweight form/control snapshots.
- Common scalar-control value extraction.
- DataGridView bound-source extraction.
- DataTable/DataView/enumerable handling when naturally available.
- Bounded row reads.
- Read-only `ui.inspect`, `ui.read_control`, and `ui.read_data` tools.
- `HAgentHost.Attach(form, registry)` bridge.
- Example verification for the UI Context layer.

### Next

- Rich BindingSource/CurrencyManager/IList adapters.
- Better semantic identities for UserControl/custom controls.
- Public attach/detach lifetime management.
- Floating assistant/flyout attached to a form.
- `ui.write_control`.
- `ui.move_control`.
- `ui.resize_control`.
- `ui.invoke` / approved click.
- Enable/disable operations.
- Batch operations.
- UI-thread dispatch with real host cancellation.
- Dry-run/preview and undo hooks.
- Per-control permissions and human approval.

### Data representation rule

Always use the lightest representation that preserves the information required by the current operation. Prefer bound/native sources; adapt lazily; avoid unnecessary copies/materialization. `DataTable` is optional, never the architectural default. Large sources should use paging, projection, or streaming when appropriate.

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

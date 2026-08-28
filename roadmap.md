# HAgent Roadmap

HAgent is intended to become a small, provider-neutral agent platform for .NET desktop applications. The roadmap is ordered by dependency so simple applications stay lightweight while advanced applications can add memory, tools, WinForms automation, collaboration, and workflows without forcing every feature into the base deployment.

## 0.1 — Foundation — Completed

- Provider/agent domain model and relationships.
- Multiple provider references on agents.
- OpenAI-compatible provider adapter.
- File persistence and Windows DPAPI secret protection.
- SQL Server persistence foundation.
- MySQL persistence foundation.
- Provider, agent, and tool management UI.
- Borderless rounded HAgent WinForms management UI.
- Shared HAgent `Header`, `HMessage`, and `HButton` conventions.
- Session API and history forwarding.
- Provider connection testing.
- Provider model discovery.
- Agent/provider deletion rules.
- `HAgent.Example` manual integration/verification host.
- Global Example agent selection and output.
- Example source split into focused partial files.

## 0.2 — Runtime Foundation — Completed

- Agent runtime abstraction.
- Execution state/lifecycle.
- Stable execution IDs and correlation identifiers.
- Agent/provider execution snapshots.
- Provider routing and ordered candidates.
- Provider attempt limits.
- Cancellation and timeout boundaries.
- Caller-cancellation versus timeout distinction.
- Retry count per provider.
- Conservative provider error classification.
- Exponential backoff with rate-limit-aware delay.
- Execution lifecycle events.
- Execution duration/provider diagnostics.
- Structured execution failure categories.
- No-GPU/low-RAM memory rule.
- Correct single system-prompt resolution.
- Provider failure detail preservation.

## 0.3 — Memory and Context — Foundation Complete

### Implemented

- Persistent file memory using JSONL.
- Streaming memory search without loading the complete store into RAM.
- Explicit remember/recall/forget.
- Memory scopes: session, task, agent, user, application, shared.
- Metadata filtering and bounded recall.
- Creation/occurrence timestamps and provenance.
- Persistent conversation store abstraction.
- File-backed persistent sessions and stable session IDs.
- Session reopening and transactional rollback.
- Deterministic context builder.
- Message/character context budgets.
- Approximate token estimation without tokenizer dependency.
- Conservative explicit-trigger automatic memory policy.
- Automatic-memory session/policy provenance.
- Lightweight phrase/term/metadata/recency ranking.
- Typed Task/Event memory and task filtering.
- Compact `EpisodicMemory` with task/session provenance.
- Manual Example coverage for the completed memory/context features.

### Advanced memory — Planned

- Richer automatic-memory inference without saving ordinary conversation by default.
- Memory update/upsert semantics.
- Retention/expiration policies.
- Improved lightweight indexing for larger stores.
- Context trimming and compaction/summarization.
- SQL Server memory store.
- MySQL memory store.
- Conversation listing/search/metadata management.
- Optional vector-memory adapter.
- Remote embedding-provider integration without requiring a local GPU or large resident model.

## 0.4 — Provider Capabilities and Response Normalization — Foundation Complete

- Explicit provider/model capability model.
- Tri-state support: `Supported`, `Unsupported`, `Unknown`.
- Capability evidence with source, confidence, observation time, and notes.
- Capability cache with failure eviction/reset.
- Basic capability-aware routing and known-unsupported Chat rejection.
- Provider/Agent editor capability visibility and evidence tooltips.
- Provider-neutral `AIResponse`.
- Separate normal text, provider-exposed reasoning, raw text, structured JSON, tool calls, usage, and provider metadata.
- Explicit `reasoning_content` normalization.
- `<think>` detection without claiming native reasoning.
- Provider/model/account error classification with actionable diagnostics.
- Normalized structured JSON output.
- Normalized provider-neutral tool calls.
- Normalized prompt/completion/total/cached/reasoning token usage.
- Provider-neutral streaming delta contract.
- OpenAI-compatible SSE streaming and `HAgentClient.StreamAsync(...)`.
- Streaming cancellation handling.
- Response-normalization, streaming-contract, and live-stream Example coverage.

### 0.4 — Remaining

- Capability override/configuration UI where applications explicitly need to declare a capability.
- Rich provider capability discovery.
- Advanced model suitability/routing for tool calling, vision, structured output, audio, embeddings, and reasoning.
- Application reasoning policy for display/store/log/discard.

## 0.5 — Tools and Agent Loop — Active

### Tool foundation — Completed

- `AiTool` definition model.
- Explicit `AiToolType`: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- `IAgentTool` definition/handler separation.
- `IToolRegistry` abstraction.
- `InMemoryToolRegistry`.
- `DelegateAgentTool` for code-defined custom tools.
- `HAgentClient` tool registration, lookup, definition inspection, and direct execution.
- Tool type selection in the Tool editor.
- Deterministic Example tool-registry test.

### Tool types

- **BuiltIn** — supplied by HAgent.
- **Application** — executable handler registered by the host application.
- **Declarative** — safe configuration-driven operation; configuration never becomes arbitrary code execution.
- **UI** — supplied by `HAgent.WinForms` control/context adapters.
- **SqlServer** — supplied by the SQL Server tool layer with restricted database operations.
- **MySql** — supplied by the MySQL tool layer with restricted database operations.
- **Extension** — future; not part of the initial implementation.

### Next

- JSON Schema validation and safe argument binding.
- Provider tool-definition transport.
- Provider-neutral tool-call execution loop.
- Tool result/observation messages.
- Multiple model/tool turns.
- Per-agent tool selection using persisted `ToolIds`.
- Per-session temporary tools.
- Built-in tool handlers.
- Application tool registration guidance/API conventions.
- Declarative tool execution engine.
- WinForms UI tool handlers.
- SQL Server tool layer.
- MySQL tool layer.
- Tool aliases/versioning.
- Tool timeout/cancellation/progress.
- Tool audit/history.
- Maximum tool calls/turns and loop detection.
- Tool budgets.
- Tool/provider capability negotiation.
- Complete predefined/custom tool UI behavior.
- Manual multi-step tool-loop Example coverage.

## 0.6 — Safety, Guardrails, Permissions, Approval, Budgets, and Observability

### Guardrails

- Input guardrails.
- Output guardrails.
- Tool input/output guardrails.
- Termination/tripwire rules.
- Configurable guardrail ordering.

### Permissions and approval

- Read/write/invoke/export separation.
- Agent/session/task policy scopes.
- Host authorization callbacks.
- Per-tool human approval.
- Approval request/result state machine.
- Approve/deny/cancel.
- Approval timeout and audit history.

### Budgets and observability

- Maximum execution duration.
- Maximum provider/model calls.
- Maximum tool calls/turns.
- Memory/context retrieval budgets.
- Handoff/workflow depth budgets.
- Optional token/cost budgets.
- Execution traces and turn spans.
- Provider/model/tool/memory timings.
- Guardrail/approval/handoff events.
- Correlation IDs throughout execution.
- Sensitive-data redaction.
- No secrets in diagnostics by default.
- Lightweight internal events with optional external exporters.

## 0.7 — WinForms UI Context and Application Automation

The WinForms integration is deliberately outside Core. “Form serialization” is only one operation of the broader **UI Context / Control Adapter** system.

### Performance rule

> Always prefer the lightest representation that preserves the information required for the current operation. Prefer native/bound sources, adapt lazily, avoid unnecessary copying/materialization, and materialize a tabular representation only when it is actually required or demonstrably the most efficient representation for the workload.

For `DataGridView`, prefer its bound data source. Resolve `BindingSource`, `CurrencyManager`, `IList`/collections, and known tabular sources through adapters. Do not scrape visible cells when the underlying source is available. Do not eagerly copy large datasets into `DataTable` merely for convenience.

### UI Context / adapters

- Form/UserControl/custom-control attachment.
- Stable control identity and semantic discovery.
- Safe UI state snapshots.
- Provider-neutral context representation.
- DataGridView/source extraction.
- BindingSource/CurrencyManager support.
- IList/collection support.
- TextBox/RichTextBox.
- ComboBox.
- Button.
- CheckBox/RadioButton.
- DateTimePicker.
- NumericUpDown.
- ListBox/ListView/TreeView.
- DataTable only when naturally present or actually required.

### Form bridge

A future public experience may resemble:

```csharp
var attached = HAgent.WinForms.HAgentHost.Attach(ai, this);
```

The bridge should support attach/detach, discovery, inspection, read access, approved write/invoke operations, form-aware context, agent/session selection, and an HAgent floating assistant/flyout.

Attaching a form must never automatically grant write access.

### UI tools

- `ui.inspect`.
- `ui.read_control`.
- `ui.read_data`.
- `ui.write_control`.
- `ui.move_control`.
- `ui.resize_control`.
- `ui.invoke` / approved click.
- `ui.enable_control` / `ui.disable_control`.
- Batch operations.
- UI-thread dispatch.
- Dry-run/preview.
- Human approval.
- Optional undo/rollback hooks.
- Per-control permissions.

## 0.8 — Agent Scope, Chat, and Interaction

Keep agent profile separate from runtime binding/lifetime.

### Scopes

- Application/global.
- Form-bound.
- Session-bound.
- Task-bound.
- Ephemeral execution.
- Parent/owner relationships.

### Chat

- User ↔ agent chat window.
- Global/form agent selector.
- Conversation switching.
- Persistent conversation UI.
- Conversation search/titles/metadata.
- Streaming UI.
- Tool activity visualization.
- Reasoning visibility policy.
- Cancel/stop.
- Multiple simultaneous conversations.
- Safe handling of deleted/disabled agents in active chats/tasks.

### Cross-form memory

Cross-form recall is allowed only when scope and policy permit it. Provenance can include form/session/task/application metadata, but provenance is never authorization.

## 0.9 — Agent Orchestration and Collaboration

- Agents as tools.
- Handoffs/delegation.
- Specialist agents.
- Agent-to-agent messaging board.
- Collaboration channels.
- Direct messages and broadcasts.
- Shared workspace context.
- Agent roles/capabilities.
- Routing policies.
- Maximum hops/depth.
- Loop detection.
- Shared/private memory policies.
- Collaboration transcript.
- Human intervention points.
- Parallel agent execution.
- Collaboration budgets.
- Active/disabled/retired/deleted lifecycle.

## 0.10 — Tasks, Workflows, and Autonomy

- Explicit task/job model.
- Task lifecycle/state machine.
- Planning/execution/verification.
- Multi-step workflows.
- Background execution and scheduling.
- Pause/resume.
- Durable checkpoints.
- Conditional/parallel branches.
- Event-triggered agents.
- Retry per workflow step.
- Human approval steps.
- Resource/autonomy budgets.
- Restart recovery.
- Task cancellation/cleanup.
- Long-running execution leases.
- Workflow observability.

## 0.11 — Provider Ecosystem

- Azure OpenAI.
- Anthropic.
- Google/Gemini.
- Ollama.
- LM Studio.
- Custom HTTP providers.
- Provider-specific capability negotiation.
- Model discovery/cache.
- Streaming.
- Multimodal support.
- Embedding providers.
- Provider contract test harnesses.
- Versioned provider extension contract.

## 0.12 — Extensibility and Storage Ecosystem

- Provider adapter DLL loading.
- Tool DLL loading.
- UI-control adapter DLL loading.
- Custom storage provider DLLs.
- Extension validation/failure isolation.
- Persistent multi-provider routing configuration.
- Conversation persistence across File/SQL Server/MySQL.
- Memory persistence across File/SQL Server/MySQL.
- Optional vector/semantic companion package.
- Optional MCP integration.
- External secret stores and rotation.
- Import/export with explicit secret handling.
- Configuration profiles/workspaces.
- Multi-user authorization hooks.
- Audit logging.

## 0.13 — Developer Platform

- Optional dependency-injection integration.
- Optional `Microsoft.Extensions.AI` integration.
- Provider SDK.
- Tool SDK.
- UI-context/control-adapter SDK.
- Agent simulation/test mode.
- Provider/tool/memory/security contract harnesses.
- Diagnostics/trace viewer.
- Examples for UI, database, document, workflow, and collaboration automation.
- `HAgent.Example` coverage for every meaningful public capability.

## 1.0 — Stable HAgent Platform

- Stable public contracts.
- Backward compatibility policy.
- Storage migration/versioning.
- NuGet packages.
- Signed releases.
- Comprehensive integration tests.
- Security/permission tests.
- Provider/tool/memory documentation.
- WinForms automation documentation.
- Custom provider/tool/control-adapter guides.
- .NET 10 target after migration to a compatible Visual Studio environment.

## Design principles

- The core runtime stays small.
- Provider transport is separate from agent behavior.
- Capabilities are explicit; model names are not capability guarantees.
- Capability claims preserve their source/confidence where practical.
- Tool definitions describe capabilities; they are not arbitrary executable access.
- Applications own real-world side effects.
- Prompts are not security boundaries; guardrails and permissions are.
- Sensitive actions can require human approval.
- Autonomous operations are observable, cancellable, and budgeted.
- Provider responses are normalized without destroying useful provider metadata.
- Reasoning/thinking is separate optional response content when explicitly exposed.
- Agent profile and agent/session/form/task scope are separate concepts.
- WinForms UI context is a host integration layer, not a Core dependency.
- UI context describes state; tools define permitted actions.
- Common WinForms data-source handling is centralized in adapters and must use the lightest practical representation.
- DataGridView extraction must prefer native/bound sources when available, adapt lazily, and avoid unnecessary copies/materialization.
- Memory works without a local GPU, local embedding model, vector database, or large resident RAM footprint.
- Advanced integrations belong in optional assemblies/packages where possible.
- `HAgent.Example` is part of the development workflow and demonstrates completed public capabilities.

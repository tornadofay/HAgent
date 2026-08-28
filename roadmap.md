# HAgent Roadmap

HAgent is intended to become a small, provider-neutral agent platform for .NET desktop applications. The architecture is ordered by dependency so a simple application stays lightweight while more advanced applications can add memory, tools, UI automation, collaboration, and workflows without forcing all of those features into the common deployment.

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
- Lightweight memory abstraction.
- No-GPU/low-RAM memory rule.
- Correct single system-prompt resolution.
- Provider failure detail preservation.

## 0.3 — Memory and Context — Active

### Implemented

- Persistent file memory using JSONL.
- Streaming memory search.
- Explicit remember/recall/forget.
- Session/task/agent/user/application/shared memory scopes.
- Metadata filtering.
- Memory provenance baseline.
- Bounded recall.
- Persistent conversation store abstraction.
- File-backed persistent sessions.
- Stable session IDs and reopening.
- Transactional session rollback.
- Deterministic working-context builder.
- Message/character context budgets.
- Approximate token estimation without tokenizer dependency.
- Conservative explicit conversation-memory policy.
- Session/policy provenance for automatic memory.
- Lightweight phrase/term/metadata/recency memory ranking.
- Typed task/event memory.
- Task ID filtering for task/event memory.
- Compact `EpisodicMemory` representation.
- Episodic outcome/task/session provenance metadata.
- Manual Example coverage for memory, context, task/event, and episodic behavior.

### Remaining

- Richer automatic-memory inference without saving ordinary conversation by default.
- Memory update/upsert semantics.
- Retention/expiration.
- Improved lightweight indexing for larger stores.
- Context trimming and compaction/summarization.
- SQL Server memory store.
- MySQL memory store.
- Conversation listing/search/metadata management.
- Optional vector-memory adapter.
- Remote embedding-provider integration without local GPU requirements.

## 0.4 — Provider Capabilities and Response Normalization

This milestone prevents HAgent from treating every discovered model as interchangeable.

- Explicit model/provider capability model.
- Chat capability.
- Streaming capability.
- Structured-output capability.
- Tool/function-calling capability.
- Vision capability.
- Audio input/output capability where supported.
- Embedding capability.
- Reasoning capability.
- Provider/model suitability evaluation.
- Capability caching.
- Provider-neutral response model.
- Separate ordinary assistant content from provider-exposed reasoning.
- Preserve reasoning metadata when explicitly supplied by the provider.
- Do not infer provider-native reasoning merely from `<think>...</think>` markup.
- Structured output representation.
- Tool-call representation.
- Token/usage/cost metadata where available.
- Raw provider metadata preservation.
- Plain-text provider compatibility.
- Streaming response abstraction.
- Manual capability and normalization examples in `HAgent.Example`.

## 0.5 — Tools and Agent Loop

### Tool model

- First-class tool definitions.
- JSON Schema arguments.
- Structured result/output contracts.
- Definition/handler separation.
- Predefined tools.
- Custom tools.
- Tool registry/discovery.
- Per-agent tool assignment.
- Per-session temporary tools.
- Tool aliases/versioning.

### Agent loop

- Provider-neutral tool-call execution loop.
- Multiple model/tool turns.
- Tool result/observation protocol.
- Typed argument binding.
- Validation.
- Tool cancellation.
- Tool timeout.
- Tool progress.
- Tool history/audit records.
- Maximum tool calls.
- Maximum loop turns.
- Loop detection.
- Tool budgets.

### Tool-provider negotiation

- Capability-aware tool invocation.
- Tool availability by model/provider capability.
- Tool error normalization.

### Example coverage

- Deterministic tool.
- Structured arguments.
- Tool result returned to model.
- Multi-step tool loop.
- Cancellation/timeout.

## 0.6 — Safety, Guardrails, Permissions, Approval, and Observability

### Guardrails

- Input guardrails.
- Output guardrails.
- Tool input guardrails.
- Tool output guardrails.
- Termination/tripwire rules.
- Configurable guardrail ordering.

### Permissions

- Tool permission model.
- Read/write/invoke/export separation.
- Agent policy.
- Session/task overrides.
- Host authorization callbacks.

### Human approval

- Per-tool approval requirement.
- Approval request/result protocol.
- Pending approval state.
- Approve/deny/cancel.
- Approval timeout.
- Approval audit history.

### Resource budgets

- Maximum execution duration.
- Maximum provider/model requests.
- Maximum tool calls/turns.
- Context budget.
- Memory retrieval budget.
- Maximum handoffs.
- Maximum workflow depth.
- Optional token/cost budget.

### Observability

- Execution traces.
- Turn spans.
- Provider/model timing.
- Tool timing.
- Memory retrieval timing.
- Guardrail events.
- Approval events.
- Handoff/delegation events.
- Correlation IDs throughout the pipeline.
- Configurable sensitive-data redaction.
- No secrets in diagnostics by default.
- Lightweight internal event model with optional external telemetry integration.

## 0.7 — WinForms UI Context and Application Automation

This milestone is where HAgent gains a desktop-specific capability that should remain outside Core.

### UI Context / Control Adapters

Do not model the whole feature as “form serialization.” Serialization is one operation produced by the UI context layer. The larger feature is an **UI Context / Control Adapter** system that can inspect forms, controls, and data sources and produce safe, provider-neutral descriptions.

The governing performance rule is:

> Prefer the lightest representation that preserves the native/source information needed for the current operation. Preserve bound/native data sources where practical, adapt lazily, avoid copying data unnecessarily, and materialize a tabular representation only when the operation actually requires it. `DataTable` is an available compatibility representation, not an architectural requirement.

For `DataGridView`, prefer its bound data source when one exists. Resolve `BindingSource`, `CurrencyManager`, `IList`/collection, and known tabular sources through adapters. Avoid scraping visible cells when the underlying source is accessible. Avoid eagerly materializing large datasets into `DataTable` or another duplicate structure when streaming, paging, projection, or a native representation is faster and uses less memory.

Planned adapters:

- `Form`.
- `UserControl`.
- Custom controls through explicit adapters.
- `TextBox` / `RichTextBox`.
- `ComboBox`.
- `Button`.
- `CheckBox` / `RadioButton`.
- `DateTimePicker`.
- `NumericUpDown`.
- `ListBox` / `ListView`.
- `TreeView`.
- `DataGridView`.
- `DataTable` when already present or explicitly required.
- `BindingSource`.
- `CurrencyManager`.
- Common `IList`/collection data sources.

### Form bridge

A future API should conceptually resemble:

```csharp
var attached = HAgent.WinForms.HAgentHost.Attach(ai, this);
```

The exact API may change, but `HAgentClient` itself must remain UI-independent.

The host bridge should support:

- Attach/detach a form.
- Discover controls.
- Inspect control state.
- Read values.
- Read bound/tabular data.
- Expose approved write operations.
- Expose approved invoke/click operations.
- Show an HAgent floating panel/flyout attached to the form.
- Select an agent/session for the form.
- Provide form-aware context to tools and memory.

Attaching a form must not automatically grant write access.

### UI tools

- `ui.inspect`.
- `ui.read_control`.
- `ui.read_data`.
- `ui.write_control`.
- `ui.move_control`.
- `ui.resize_control`.
- `ui.invoke` / approved click.
- `ui.enable_control` / `ui.disable_control`.
- Batch UI operations.
- UI-thread dispatch.
- Dry-run/preview mode.
- Human approval for sensitive actions.
- Optional undo/rollback hooks.
- Per-control permissions.

### Attached AI experience

- Floating assistant button/panel.
- Form-aware chat.
- Current agent/session indicator.
- Visible permission/capability state.
- Tool activity display.
- Context preview.
- Detach/disable control.

## 0.8 — Agent Scope, Chat, and Interaction

Do not create incompatible agent classes for every lifetime scenario. Separate the **agent profile** from the **runtime binding/scope**.

### Binding scopes

- Application/global.
- Form-bound.
- Session-bound.
- Task-bound.
- Ephemeral execution.
- Explicit parent/owner relationship.

A global agent can serve multiple forms while separate sessions maintain separate conversational state. A form-bound assistant can use form context without becoming a fundamentally different agent type.

### Chat

- User ↔ agent chat window.
- Agent selector.
- Conversation switching.
- Persistent conversations.
- Conversation search.
- Conversation titles/metadata.
- Attachments/multimodal messages where supported.
- Live execution status.
- Streaming.
- Tool-call visualization.
- Reasoning visibility policy.
- Cancel/stop.
- Multiple simultaneous conversations.
- Safe handling of deleted/disabled agents in open chats/tasks.

### Cross-form memory

An agent can recall information originating from another form only when policy and scope permit it.

```text
Form1
  ↓
explicit memory / session / application scope
  ↓
Form2
  ↓
agent recalls allowed information from Form1
```

Memory may carry form ID, session ID, task ID, and application metadata for provenance, but provenance is not authorization.

## 0.9 — Agent Orchestration and Collaboration

- Agents as tools.
- Handoffs/delegation.
- Specialist agents.
- Agent-to-agent messaging board.
- Collaboration channels.
- Direct messages.
- Broadcast messages.
- Shared workspace context.
- Roles/capabilities.
- Routing policies.
- Maximum hops/depth.
- Loop detection.
- Shared/private memory policies.
- Collaboration transcript.
- Human intervention points.
- Parallel agent execution.
- Collaboration resource budgets.
- Agent lifecycle states: active, disabled, retired, deleted.

## 0.10 — Tasks, Workflows, and Autonomy

- Explicit task/job objects.
- Task lifecycle/state machine.
- Planning / execution / verification.
- Multi-step workflows.
- Background execution.
- Scheduling.
- Pause/resume.
- Durable checkpoints.
- Conditional branches.
- Parallel branches.
- Event-triggered agents.
- Retry per workflow step.
- Human approval steps.
- Execution/resource budgets.
- Recovery after application restart.
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
- MCP integration as an optional extension.
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
- Provider/tool/memory contract test harnesses.
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
- Provider/tool/memory/security contract tests.
- Complete custom-provider guide.
- Complete custom-tool guide.
- WinForms UI automation guide.
- Memory/retrieval guide.
- Collaboration/workflow guide.
- .NET 10 target after migration to a compatible Visual Studio environment.

## Design principles

- The core runtime stays small.
- Provider transport is separate from agent behavior.
- Capabilities are explicit; model names are not capability guarantees.
- Tool definitions describe capabilities; they are not arbitrary executable access.
- Applications own real-world side effects.
- Prompts are not security boundaries; guardrails and permissions are.
- Sensitive actions can require human approval.
- Autonomous operations are observable, cancellable, and budgeted.
- Provider responses are normalized without destroying provider-specific metadata.
- Reasoning/thinking is separate optional response content when explicitly exposed.
- Agent profile and agent/session/form/task scope are separate concepts.
- WinForms UI context is a host integration layer, not a Core dependency.
- UI context describes state; tools define permitted actions.
- Common WinForms data-source handling is centralized in adapters and must use the lightest practical representation.
- DataGridView extraction must prefer native/bound sources when available, adapt lazily, and avoid unnecessary copies/materialization.
- Memory works without a local GPU, local embedding model, vector database, or large resident RAM footprint.
- Advanced integrations belong in optional assemblies/packages where possible.
- `HAgent.Example` is part of the development workflow and demonstrates completed public capabilities.

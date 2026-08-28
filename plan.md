# HAgent Development Plan

## Current milestone: 0.3 — Memory and Context

This file is the implementation ledger. It records what is actually present in the repository, what remains in the current milestone, and the dependency order for the next milestones.

### 0.1 Foundation — completed
- [x] Multi-target core for .NET Framework 4.8.1 and .NET 9.
- [x] Provider-neutral adapter contract.
- [x] OpenAI-compatible provider adapter.
- [x] File persistence with protected secret storage.
- [x] SQL Server and MySQL storage foundations.
- [x] Provider, agent, and tool management UI.
- [x] Shared HAgent `Header` and `HMessage` UI conventions.
- [x] `HButton` used for HAgent application actions.
- [x] Agent session history and context forwarding.
- [x] Provider connection testing.
- [x] Provider model discovery.
- [x] Agent/provider deletion rules.
- [x] `HAgent.Example` manual integration host.
- [x] Global Example agent selector and global output.
- [x] Example source split into focused partial files.
- [x] Example coverage for configuration, messaging, session, persistent session, memory, automatic memory, runtime, and context budgeting.

### 0.2 Runtime Foundation — completed
- [x] Agent runtime abstraction and default execution pipeline.
- [x] Execution lifecycle/state.
- [x] Stable execution IDs/correlation identifiers.
- [x] Agent/provider execution snapshots.
- [x] Provider routing abstraction and ordered candidates.
- [x] Provider attempt limits.
- [x] Cancellation and timeout boundaries.
- [x] Caller cancellation versus timeout distinction.
- [x] Configurable provider retry count.
- [x] Conservative provider error classification.
- [x] Exponential retry backoff with rate-limit-aware delay.
- [x] Execution lifecycle events.
- [x] Execution duration/provider diagnostics.
- [x] Structured execution failure categories.
- [x] No-GPU/low-RAM memory design rule.
- [x] Correct system-prompt resolution and single insertion into provider requests.
- [x] Provider failure detail preservation.

## 0.3 Memory and Context — active

### Implemented
- [x] `IMemoryStore` abstraction.
- [x] `FileMemoryStore` using append-oriented JSONL.
- [x] Streaming file search without loading the full memory file into RAM.
- [x] Explicit `RememberAsync`, `RecallAsync`, and `ForgetAsync`.
- [x] Memory scopes: session, task, agent, user, application, shared.
- [x] Metadata filtering.
- [x] Creation timestamp/provenance baseline.
- [x] Bounded recall results.
- [x] Persistent `IConversationStore` abstraction.
- [x] File-backed conversation store.
- [x] Stable persistent session IDs.
- [x] `OpenSessionAsync`.
- [x] Transactional rollback of a session turn when provider or persistence fails.
- [x] Deterministic `ConversationContextBuilder`.
- [x] Message and character context budgets.
- [x] Approximate token estimation without a tokenizer dependency.
- [x] Conservative `IConversationMemoryPolicy` abstraction.
- [x] Explicit-trigger automatic memory policy.
- [x] Automatic memory provenance including session and policy.
- [x] Lightweight memory relevance ranking.
- [x] Manual Example coverage for persistence, automatic memory, and context budgeting.

### Remaining
- [ ] Richer task/event memory.
- [ ] Episodic memory model.
- [ ] Memory update/upsert semantics.
- [ ] Retention/expiration policies.
- [ ] Improved lightweight retrieval/indexing for large stores.
- [ ] Context trimming and compaction/summarization.
- [ ] SQL Server memory store.
- [ ] MySQL memory store.
- [ ] Conversation listing/search/metadata management.
- [ ] Optional vector-memory adapter.
- [ ] Remote embedding-provider integration without local GPU requirements.
- [ ] Optional richer automatic-memory inference that never silently persists ordinary chat.

## 0.4 Provider Capabilities and Response Normalization — planned

- [ ] Explicit capability model for provider/model combinations.
- [ ] Chat, streaming, structured output, tool calling, vision, audio, embeddings, and reasoning capabilities.
- [ ] Capability cache and model suitability checks.
- [ ] Provider-neutral response object containing text plus optional structured/tool/reasoning/usage/raw metadata.
- [ ] Separate provider-exposed reasoning from ordinary assistant content.
- [ ] Do not treat `<think>...</think>` markup as universally provider-native reasoning.
- [ ] Allow applications to choose whether reasoning is displayed/stored/logged/discarded.
- [ ] Streaming response abstraction.
- [ ] Usage/token/cost metadata where available.
- [ ] Manual capability/normalization examples in `HAgent.Example`.

## 0.5 Tools and Agent Loop — planned

### Tool model
- [ ] First-class tool definition.
- [ ] JSON Schema arguments.
- [ ] Structured result/output contract.
- [ ] Separate definition from executable handler.
- [ ] Predefined and custom tools.
- [ ] Tool registry/discovery.
- [ ] Per-agent tool selection.
- [ ] Per-session temporary tools.
- [ ] Tool aliases/versioning.

### Agent loop
- [ ] Provider-neutral tool-call loop.
- [ ] Multiple model/tool turns.
- [ ] Tool result/observation protocol.
- [ ] Typed argument binding.
- [ ] Validation.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool execution history.
- [ ] Maximum tool calls/turns.
- [ ] Loop detection.
- [ ] Tool budgets.
- [ ] Tool/provider capability negotiation.
- [ ] Manual tool-loop examples in `HAgent.Example`.

## 0.6 Safety, Guardrails, Approval, Budgets, and Observability — planned

- [ ] Input guardrails.
- [ ] Output guardrails.
- [ ] Tool input/output guardrails.
- [ ] Termination/tripwire rules.
- [ ] Tool permissions.
- [ ] Read/write/invoke/data-export permission separation.
- [ ] Agent/session/task policy scopes.
- [ ] Host authorization callbacks.
- [ ] Human approval requirements per tool.
- [ ] Approval request/result state machine.
- [ ] Approval timeout and audit record.
- [ ] Maximum execution duration.
- [ ] Maximum provider/model calls.
- [ ] Maximum tool calls/turns.
- [ ] Memory retrieval budget.
- [ ] Handoff/workflow depth budgets.
- [ ] Optional token/cost budget.
- [ ] Execution traces and turn spans.
- [ ] Provider/model/tool/memory timings.
- [ ] Guardrail/approval/handoff events.
- [ ] Correlation IDs.
- [ ] Configurable redaction and no-secret diagnostics.
- [ ] Lightweight internal observability events with optional external exporters.

## 0.7 WinForms UI Context and Application Automation — planned

### UI Context / Control Adapters
- [ ] Form/UserControl/custom-control attachment model.
- [ ] Stable control identity.
- [ ] Control discovery and semantic descriptions.
- [ ] Readable control-state snapshot.
- [ ] Context serialization/export into provider-neutral representations.
- [ ] DataGridView data-source extraction.
- [ ] DataTable normalization.
- [ ] BindingSource/CurrencyManager handling.
- [ ] Common IList/collection data-source handling.
- [ ] TextBox/RichTextBox adapter.
- [ ] ComboBox adapter.
- [ ] Button adapter.
- [ ] CheckBox/RadioButton adapter.
- [ ] DateTimePicker/NumericUpDown adapters.
- [ ] ListBox/ListView/TreeView adapters.
- [ ] Explicit adapters for custom/user controls.

### Host bridge
- [ ] Form attach/detach.
- [ ] Form-aware context.
- [ ] Form-aware permissions.
- [ ] Attached agent/session binding.
- [ ] HAgent floating assistant panel/button.
- [ ] Show current agent/session/capabilities.
- [ ] Context preview.
- [ ] Detach/disable attachment.

### UI tools
- [ ] `ui.inspect`.
- [ ] `ui.read_control`.
- [ ] `ui.read_data`.
- [ ] `ui.write_control`.
- [ ] `ui.move_control`.
- [ ] `ui.resize_control`.
- [ ] `ui.invoke` / approved clicks.
- [ ] `ui.enable_control` / `ui.disable_control`.
- [ ] Batch UI operations.
- [ ] UI-thread dispatch.
- [ ] Dry-run/preview.
- [ ] Human approval for sensitive actions.
- [ ] Optional undo/rollback hooks.
- [ ] Per-control permissions.

## 0.8 Agent Scope, Chat, and Interaction — planned

- [ ] Separate agent profile from runtime binding/scope.
- [ ] Application/global agent binding.
- [ ] Form-bound agent binding.
- [ ] Session-bound agent binding.
- [ ] Task-bound agent binding.
- [ ] Ephemeral execution binding.
- [ ] Explicit parent/owner relationships.
- [ ] User ↔ agent chat window.
- [ ] Agent selector.
- [ ] Conversation switching.
- [ ] Persistent conversations UI.
- [ ] Conversation search/titles/metadata.
- [ ] Streaming display.
- [ ] Tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop response.
- [ ] Multiple simultaneous conversations.
- [ ] Safe deleted/disabled-agent handling.
- [ ] Cross-form memory according to scope and permission policy.

## 0.9 Agent Orchestration and Collaboration — planned

- [ ] Agents as tools.
- [ ] Handoffs/delegation.
- [ ] Specialist agents.
- [ ] Agent-to-agent messaging board.
- [ ] Collaboration channels.
- [ ] Direct messages.
- [ ] Broadcast messages.
- [ ] Shared workspace context.
- [ ] Role/capability descriptions.
- [ ] Routing policies.
- [ ] Maximum hops/depth.
- [ ] Loop detection.
- [ ] Shared/private memory policies.
- [ ] Collaboration transcript.
- [ ] Human intervention points.
- [ ] Parallel agent execution.
- [ ] Collaboration budgets.
- [ ] Agent lifecycle: active, disabled, retired, deleted.

## 0.10 Tasks, Workflows, and Autonomy — planned

- [ ] Explicit task/job model.
- [ ] Task lifecycle/state machine.
- [ ] Planning/execution/verification.
- [ ] Multi-step workflows.
- [ ] Background execution.
- [ ] Scheduling.
- [ ] Pause/resume.
- [ ] Durable checkpoints.
- [ ] Conditional/parallel branches.
- [ ] Event-triggered agents.
- [ ] Retry per workflow step.
- [ ] Human approval steps.
- [ ] Resource/autonomy budgets.
- [ ] Recovery after application restart.
- [ ] Task cancellation/cleanup.
- [ ] Long-running execution leases.
- [ ] Workflow observability.

## 0.11 Provider Ecosystem — planned

- [ ] Azure OpenAI adapter.
- [ ] Anthropic adapter.
- [ ] Google/Gemini adapter.
- [ ] Ollama adapter.
- [ ] LM Studio adapter.
- [ ] Custom HTTP provider adapter.
- [ ] Multimodal providers.
- [ ] Embedding providers.
- [ ] Provider-specific capability negotiation.
- [ ] Model discovery/cache.
- [ ] Streaming provider implementations.
- [ ] Provider contract test harness.
- [ ] Versioned provider extension contract.

## 0.12 Extensibility and Storage Ecosystem — planned

- [ ] Provider adapter DLL loading.
- [ ] Tool DLL loading.
- [ ] UI-control adapter DLL loading.
- [ ] Custom storage provider DLLs.
- [ ] Extension validation/failure isolation.
- [ ] Persistent multi-provider routing configuration.
- [ ] Conversation persistence across File/SQL Server/MySQL.
- [ ] Memory persistence across File/SQL Server/MySQL.
- [ ] Optional vector/semantic-memory companion package.
- [ ] MCP integration as an optional extension.
- [ ] External secret stores and rotation.
- [ ] Import/export with explicit secret handling.
- [ ] Configuration profiles/workspaces.
- [ ] Multi-user authorization hooks.
- [ ] Audit logging.

## 0.13 Developer Platform — planned

- [ ] Optional dependency-injection integration.
- [ ] Optional `Microsoft.Extensions.AI` integration.
- [ ] Provider SDK.
- [ ] Tool SDK.
- [ ] UI-context/control-adapter SDK.
- [ ] Agent simulation/test mode.
- [ ] Provider/tool/memory/security contract harnesses.
- [ ] Diagnostics/trace viewer.
- [ ] Examples for UI, database, document, workflow, and collaboration automation.
- [ ] `HAgent.Example` coverage for every meaningful public capability.

## 1.0 Stable Platform — planned

- [ ] Stable public contracts.
- [ ] Backward compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packages.
- [ ] Signed releases.
- [ ] Comprehensive integration tests.
- [ ] Security/permission tests.
- [ ] Provider/tool/memory contract documentation.
- [ ] WinForms UI automation guide.
- [ ] Custom provider/tool/control-adapter guides.
- [ ] .NET 10 target after migration to a compatible Visual Studio environment.

## Architecture decisions to preserve

1. **Provider-neutral Core.** Provider transport belongs in adapters. Core must not depend on WinForms, SQL Server, MySQL, a vendor SDK, or a vector database.
2. **Capabilities are explicit.** A model ID is not proof of chat, tools, vision, reasoning, embeddings, streaming, or structured-output support.
3. **Agent profile versus scope.** Global/form/session/task/ephemeral behavior is a binding/lifetime concern, not a reason to create incompatible agent classes.
4. **UI Context versus tools.** UI context/introspection describes the form/control state; tools define what actions the agent is allowed to perform. Serialization is an output of the context layer, not the whole architecture.
5. **Host owns side effects.** The model never gets arbitrary reflection, process, file, database, or control-tree access.
6. **Prompts are not security.** Permissions, guardrails, approvals, and budgets are enforcement boundaries.
7. **Memory stays lightweight.** No local GPU, local embedding model, vector database, or large resident RAM footprint may be required for normal operation.
8. **Stored history and model context are separate.** Full history can remain persisted while each provider call receives a bounded context.
9. **Observability is first-class but redacted.** Every autonomous action should be traceable by correlation ID without leaking secrets by default.
10. **Active work is isolated from configuration changes.** Execution snapshots protect running work from later edits/deletions.
11. **HAgent.Example is the manual verification surface.** `HAgent.Tests` remains the automated testing project.
12. **Documentation is project state.** Meaningful changes keep `README.md`, `roadmap.md`, `plan.md`, and `AGENTS.md` synchronized.

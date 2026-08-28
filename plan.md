# HAgent Development Plan

## Current milestone: 0.4 — Provider Capabilities and Response Normalization

This is the implementation ledger. Keep it synchronized with the repository. A milestone item is not complete until the implementation and corresponding manual Example coverage exist.

### 0.1 Foundation — completed
- [x] Multi-target core for .NET Framework 4.8.1 and .NET 9.
- [x] Provider-neutral adapter contract.
- [x] OpenAI-compatible provider adapter.
- [x] File persistence with protected secret storage.
- [x] SQL Server and MySQL storage foundations.
- [x] Provider, agent, and tool management UI.
- [x] Shared HAgent `Header` and `HMessage` UI conventions.
- [x] `HButton` used for HAgent actions.
- [x] Agent session history and context forwarding.
- [x] Provider connection testing.
- [x] Provider model catalog/discovery.
- [x] Agent/provider deletion rules.
- [x] `HAgent.Example` manual verification host.
- [x] Global Example agent selection and output.
- [x] Example form split into focused partial files.

### 0.2 Runtime Foundation — completed
- [x] Agent runtime abstraction and default execution pipeline.
- [x] Execution lifecycle/state.
- [x] Stable execution IDs/correlation IDs.
- [x] Agent/provider execution snapshots.
- [x] Provider routing and ordered candidates.
- [x] Provider attempt limits.
- [x] Cancellation and timeout boundaries.
- [x] Caller-cancellation versus timeout distinction.
- [x] Configurable provider retry count.
- [x] Conservative provider error classification.
- [x] Exponential retry backoff with rate-limit-aware delay.
- [x] Execution lifecycle events.
- [x] Execution duration/provider diagnostics.
- [x] Structured execution failure categories.
- [x] No-GPU/low-RAM memory design rule.
- [x] Correct system-prompt resolution and single insertion into provider requests.
- [x] Provider failure detail preservation.

## 0.3 Memory and Context — foundation completed

### Implemented
- [x] `IMemoryStore` abstraction.
- [x] Persistent `FileMemoryStore` using JSONL.
- [x] Streaming memory search without loading the complete store into RAM.
- [x] Explicit `RememberAsync`, `RecallAsync`, and `ForgetAsync`.
- [x] Memory scopes: session, task, agent, user, application, shared.
- [x] Typed memory kinds: Fact, Preference, Task, Event.
- [x] Task ID filtering for task/event memory.
- [x] `RememberTaskEventAsync` and `RecallTaskEventsAsync` public APIs.
- [x] Metadata filtering.
- [x] Creation/occurrence timestamp baseline.
- [x] Bounded recall count.
- [x] `IConversationStore` abstraction.
- [x] File-backed persistent conversation store.
- [x] Stable persistent session IDs.
- [x] `OpenSessionAsync`.
- [x] Transactional session rollback.
- [x] Deterministic `ConversationContextBuilder`.
- [x] Message and character context budgets.
- [x] Approximate token estimation without tokenizer dependency.
- [x] `IConversationMemoryPolicy` abstraction.
- [x] Conservative explicit-trigger automatic memory policy.
- [x] Automatic-memory session/policy provenance.
- [x] Lightweight phrase/term/metadata/recency ranking.
- [x] Compact `EpisodicMemory` model.
- [x] `RememberEpisodeAsync` and `RecallEpisodesAsync` APIs.
- [x] Episodic outcome, task, session, and occurrence provenance.
- [x] Manual Example coverage for memory, persistence, automatic memory, context budgeting, task/event memory, and episodic memory.

### Remaining advanced memory work
- [ ] Richer automatic-memory inference without silently saving ordinary conversation.
- [ ] Memory update/upsert semantics.
- [ ] Retention/expiration policies.
- [ ] Improved lightweight indexing for larger stores.
- [ ] Context trimming and compaction/summarization.
- [ ] SQL Server memory store.
- [ ] MySQL memory store.
- [ ] Conversation listing/search/metadata management.
- [ ] Optional vector-memory adapter.
- [ ] Remote embedding-provider integration without local GPU requirements.

## 0.4 Provider Capabilities and Response Normalization — active

### Implemented
- [x] Tri-state capability support model: Supported / Unsupported / Unknown.
- [x] `AiCapability` flags for chat, streaming, structured output, tool calling, vision, audio, embeddings, and reasoning.
- [x] Optional `IProviderModelCapabilities` adapter contract.
- [x] `HAgentClient.GetModelCapabilitiesAsync(...)` public lookup API.
- [x] OpenAI-compatible adapter reports Chat as Supported and optional features as Unknown unless established.
- [x] `AIResponse.Reasoning` for explicitly exposed reasoning content.
- [x] `AIResponse.RawText` for original provider text.
- [x] `AIResponse.ProviderMetadata` for provider-neutral metadata.
- [x] OpenAI-compatible parsing of explicit `reasoning_content` into the separate reasoning field.
- [x] Detection metadata for `<think>` markup without treating it as native reasoning.
- [x] Manual `HAgent.Example` capability inspection tab.
- [x] In-process provider/model capability cache keyed by provider kind, provider ID, endpoint, and model.
- [x] Cache eviction on failed capability discovery.
- [x] Known-unsupported Chat models are skipped during normal `SendAsync` provider routing.
- [x] Legacy adapters without capability discovery remain compatible and report Unknown.
- [x] `ClearModelCapabilityCache()` public cache reset operation.
- [x] Provider Editor shows capability summary for the selected default model.
- [x] Agent Editor shows capability summary for the effective selected model.
- [x] Full capability matrix available through editor status tooltips.
- [x] Editor warning when Chat capability is explicitly Unsupported.
- [x] Capability evidence/provenance with source, confidence, observation time, and explanatory note.
- [x] OpenAI-compatible adapter records adapter-derived evidence for its known Chat capability.
- [x] WinForms capability tooltip exposes support, source, and confidence.
- [x] Provider/model error categories include actionable model-access diagnostics.
- [x] `ModelTermsRequired`, `PermissionDenied`, and `ModelNotFound` provider error categories.
- [x] Runtime/Example exposure of structured provider error kind.

### Remaining 0.4 work
- [ ] Capability override/configuration UI for applications that explicitly need to declare or override a capability.
- [ ] Capability cache persistence/expiration policy if needed beyond per-client caching.
- [ ] Rich provider capability discovery where providers expose it.
- [ ] Model suitability requirements beyond the mandatory Chat check.
- [ ] Capability-aware provider/model selection in routing for tools, vision, structured output, audio, and reasoning.
- [ ] Provider-neutral structured-output representation.
- [ ] Provider-neutral tool-call representation.
- [ ] Usage/token/cost normalization beyond the current usage dictionary.
- [ ] Streaming response abstraction.
- [ ] Application policy for displaying/storing/logging/discarding reasoning.
- [ ] Manual response-normalization test with an explicit provider reasoning field.

## 0.5 Tools and Agent Loop — planned
- [ ] First-class tool definitions.
- [ ] JSON Schema argument contracts.
- [ ] Structured result/output contracts.
- [ ] Definition/handler separation.
- [ ] Predefined/custom tools.
- [ ] Tool registry/discovery.
- [ ] Per-agent tool selection.
- [ ] Per-session temporary tools.
- [ ] Tool aliases/versioning.
- [ ] Provider-neutral tool-call loop.
- [ ] Multiple model/tool turns.
- [ ] Tool result/observation protocol.
- [ ] Typed argument binding and validation.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool history/audit.
- [ ] Maximum tool calls/turns.
- [ ] Loop detection.
- [ ] Tool budgets.
- [ ] Provider/tool capability negotiation.
- [ ] Manual tool-loop examples.

## 0.6 Safety, Guardrails, Approval, Budgets, and Observability — planned
- [ ] Input guardrails.
- [ ] Output guardrails.
- [ ] Tool input/output guardrails.
- [ ] Termination/tripwire rules.
- [ ] Tool permissions.
- [ ] Separate read/write/invoke/data-export permissions.
- [ ] Agent/session/task policy scopes.
- [ ] Host authorization callbacks.
- [ ] Human approval per tool/action.
- [ ] Approval request/result state machine.
- [ ] Approval timeout and audit record.
- [ ] Maximum execution duration.
- [ ] Maximum provider/model calls.
- [ ] Maximum tool calls/turns.
- [ ] Memory retrieval budget.
- [ ] Handoff/workflow depth budgets.
- [ ] Optional token/cost budgets.
- [ ] Execution traces and turn spans.
- [ ] Provider/model/tool/memory timings.
- [ ] Guardrail/approval/handoff events.
- [ ] Correlation IDs across the entire execution.
- [ ] Configurable sensitive-data redaction.
- [ ] No-secret diagnostics by default.
- [ ] Lightweight internal observability with optional exporters.

## 0.7 WinForms UI Context and Application Automation — planned
### UI Context / Control Adapters
- [ ] Form/UserControl/custom-control attachment model.
- [ ] Stable control identity.
- [ ] Control discovery and semantic descriptions.
- [ ] Safe UI state snapshots.
- [ ] Context serialization/export into provider-neutral representations.
- [ ] DataGridView data-source extraction.
- [ ] BindingSource/CurrencyManager support.
- [ ] IList/collection source support.
- [ ] TextBox/RichTextBox adapter.
- [ ] ComboBox adapter.
- [ ] Button adapter.
- [ ] CheckBox/RadioButton adapters.
- [ ] DateTimePicker/NumericUpDown adapters.
- [ ] ListBox/ListView/TreeView adapters.
- [ ] Explicit custom/UserControl adapters.

### Performance and representation rule
- [ ] Prefer the lightest representation that preserves the information required for the current operation.
- [ ] Prefer native/bound data sources over scraping visible controls when the underlying source is available.
- [ ] Adapt lazily and avoid unnecessary copies/materialization.
- [ ] Use `DataTable` only when it is already the native source, is explicitly required by the operation, or is actually the most efficient representation for that workload.
- [ ] Prefer paging, streaming, projection, or lightweight/native representations for large datasets when they are more efficient.

### Form bridge
- [ ] Attach/detach form.
- [ ] Form-aware context.
- [ ] Form-aware permissions.
- [ ] Agent/session binding.
- [ ] HAgent floating assistant panel/button.
- [ ] Current agent/session/capability display.
- [ ] Context preview.
- [ ] Detach/disable attachment.

### UI capabilities/tools
- [ ] `ui.inspect`.
- [ ] `ui.read_control`.
- [ ] `ui.read_data`.
- [ ] `ui.write_control`.
- [ ] `ui.move_control`.
- [ ] `ui.resize_control`.
- [ ] `ui.invoke` / approved click.
- [ ] `ui.enable_control` / `ui.disable_control`.
- [ ] Batch operations.
- [ ] UI-thread dispatch.
- [ ] Dry-run/preview.
- [ ] Human approval for sensitive actions.
- [ ] Optional undo/rollback hooks.
- [ ] Per-control permissions.
- [ ] Manual examples for Form/TextBox/DataGridView/button automation.

## 0.8 Agent Scope, Chat, and Interaction — planned
- [ ] Separate agent profile from runtime binding/lifetime.
- [ ] Application/global binding.
- [ ] Form-bound binding.
- [ ] Session-bound binding.
- [ ] Task-bound binding.
- [ ] Ephemeral execution binding.
- [ ] Parent/owner relationships.
- [ ] User ↔ agent chat window.
- [ ] Global/form agent selector.
- [ ] Conversation switching.
- [ ] Persistent conversation UI.
- [ ] Conversation search/titles/metadata.
- [ ] Streaming UI.
- [ ] Tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop response.
- [ ] Multiple simultaneous conversations.
- [ ] Safe deleted/disabled-agent handling.
- [ ] Cross-form memory governed by scope and policy.

## 0.9 Agent Orchestration and Collaboration — planned
- [ ] Agents as tools.
- [ ] Handoffs/delegation.
- [ ] Specialist agents.
- [ ] Agent-to-agent messaging board.
- [ ] Collaboration channels.
- [ ] Direct messages.
- [ ] Broadcast messages.
- [ ] Shared workspace context.
- [ ] Agent roles/capabilities.
- [ ] Routing policies.
- [ ] Maximum hops/depth.
- [ ] Loop detection.
- [ ] Shared/private memory policies.
- [ ] Collaboration transcript.
- [ ] Human intervention points.
- [ ] Parallel agent execution.
- [ ] Collaboration budgets.
- [ ] Active/disabled/retired/deleted lifecycle.

## 0.10 Tasks, Workflows, and Autonomy — planned
- [ ] Explicit task/job model.
- [ ] Task lifecycle/state machine.
- [ ] Planning/execution/verification.
- [ ] Multi-step workflows.
- [ ] Background execution.
- [ ] Scheduling.
- [ ] Pause/resume.
- [ ] Durable checkpoints.
- [ ] Conditional branches.
- [ ] Parallel branches.
- [ ] Event-triggered agents.
- [ ] Retry per workflow step.
- [ ] Human approval steps.
- [ ] Resource/autonomy budgets.
- [ ] Recovery after application restart.
- [ ] Task cancellation/cleanup.
- [ ] Long-running execution leases.
- [ ] Workflow observability.

## 0.11 Provider Ecosystem — planned
- [ ] Azure OpenAI.
- [ ] Anthropic.
- [ ] Google/Gemini.
- [ ] Ollama.
- [ ] LM Studio.
- [ ] Custom HTTP providers.
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
- [ ] Optional vector/semantic companion package.
- [ ] Optional MCP integration.
- [ ] External secret stores and rotation.
- [ ] Import/export with explicit secret handling.
- [ ] Configuration profiles/workspaces.
- [ ] Multi-user authorization hooks.
- [ ] Audit logging.

## 0.13 Developer Platform — planned
- [ ] Optional dependency injection.
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
- [ ] Provider/tool/memory documentation.
- [ ] WinForms automation documentation.
- [ ] Custom provider/tool/control-adapter guides.
- [ ] .NET 10 target after migration to a compatible Visual Studio environment.

## Architecture decisions to preserve

1. Core remains provider-neutral and dependency-light.
2. Capabilities are explicit and tri-state; model IDs are not capability guarantees.
3. Capability evidence should preserve source, confidence, observation time, and optional notes when available.
4. Provider transport, agent profile, runtime, memory, tools, and host side effects remain separate.
5. Agent profile and runtime scope are separate. Global/form/session/task/ephemeral behavior does not require incompatible agent classes.
6. Prompt instructions are not security boundaries.
7. Tools expose explicitly registered capabilities; they do not grant arbitrary host access.
8. Sensitive actions can require human approval.
9. Autonomous work is cancellable, observable, and budgeted.
10. Provider responses are normalized without destroying provider-specific metadata.
11. Reasoning/thinking is optional separate response content when explicitly exposed by a provider; embedded `<think>` markup is not assumed to be native reasoning.
12. UI Context/introspection describes state; tools define permitted actions.
13. “Form serialization” is a UI Context capability, not the name of the entire WinForms subsystem.
14. WinForms data-source handling is centralized in control adapters.
15. Data representation must be selected for performance and memory efficiency, not convenience. DataTable is an optional compatibility representation, never a requirement.
16. Cross-form memory uses explicit scopes and provenance, not implicit global state.
17. Stored conversation history and provider context are separate resources.
18. Memory must work without a local GPU, embedding model, vector database, or large resident RAM footprint.
19. Advanced integrations belong in optional assemblies/packages when they would otherwise bloat common deployments.
20. `HAgent.Example` is the manual verification surface; `HAgent.Tests` is the automated testing surface.
21. README, roadmap, plan, and AGENTS are project-state artifacts and must remain synchronized.

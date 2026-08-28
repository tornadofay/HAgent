# HAgent Development Plan

## Current milestone: 0.3 — Memory

### 0.1 Foundation — completed
- [x] Multi-target core for .NET Framework 4.8.1 and .NET 9.
- [x] Provider-neutral adapter contract.
- [x] OpenAI-compatible provider adapter.
- [x] File persistence with protected secret storage.
- [x] SQL Server and MySQL storage foundations.
- [x] Provider, agent, and tool management UI.
- [x] Shared HAgent `Header` and `HMessage` UI conventions.
- [x] `HButton` used as the HAgent WinForms action control.
- [x] Agent conversation session history and context forwarding.
- [x] Provider connection test capability.
- [x] Provider model catalog capability.
- [x] Agent/provider deletion rules in UI and storage.
- [x] `HAgent.Example` manual integration host replacing the older sample host.
- [x] Tabbed Example test bench with global output and global agent selection.
- [x] Example coverage for configuration, provider-backed messaging, session history, configuration reads, runtime execution, and persistent memory.
- [x] `HAgent.Example` entry point and `MainForm` split into separate source files.

### 0.2 Runtime foundation — completed
- [x] Agent runtime abstraction and default execution pipeline.
- [x] Agent execution state model.
- [x] Stable execution IDs/correlation identifiers.
- [x] Agent/provider execution snapshots.
- [x] Default provider routing abstraction.
- [x] Multiple provider candidates and attempt limits.
- [x] Explicit cancellation and timeout boundaries.
- [x] Distinguish caller cancellation from timeout.
- [x] Configurable provider retry count.
- [x] Conservative provider error classification.
- [x] Exponential retry backoff with stronger delay for rate limits.
- [x] Execution lifecycle events.
- [x] Execution duration/provider diagnostics.
- [x] Structured execution failure categories.
- [x] Dependency-free lightweight memory abstraction/foundation.
- [x] No-GPU/low-RAM memory design rule.
- [x] Provider request path inserts the resolved system prompt exactly once.
- [x] `SendAsync` preserves provider failure details instead of returning only a generic compatibility error.

### Cross-cutting response handling — planned
- [ ] Normalize provider responses into a provider-neutral representation.
- [ ] Keep ordinary assistant content separate from provider-exposed reasoning/thinking content.
- [ ] Preserve explicitly exposed reasoning metadata without coupling core contracts to one provider.
- [ ] Do not assume `<think>...</think>` is universally provider-native reasoning.
- [ ] Keep plain-text providers fully compatible.
- [ ] Let applications choose whether exposed reasoning is displayed, stored, logged, or discarded.
- [ ] Prevent exposed reasoning from unexpectedly appearing as ordinary user-facing text.
- [ ] Add provider/model capability metadata before automatic reasoning handling.
- [ ] Add manual response-normalization coverage to `HAgent.Example`.

### Deferred from 0.2 into later milestones
- [ ] Persistent multi-provider routing configuration/UI.
- [ ] Provider-native tool-call loop.
- [ ] Strong runtime leases for coordinated lifecycle ownership.
- [ ] Provider/model capability metadata and automatic suitability filtering.

## 0.3 — Memory — current
- [x] Persistent `FileMemoryStore` using append-oriented JSONL records.
- [x] Streaming file search so the whole memory file does not need to be loaded into RAM.
- [x] Explicit `remember` through `HAgentClient.RememberAsync`.
- [x] Explicit `recall` through `HAgentClient.RecallAsync`.
- [x] Explicit `forget` through `HAgentClient.ForgetAsync`.
- [x] Memory scopes: session, task, agent, user, application, shared.
- [x] Memory metadata filtering.
- [x] Memory creation timestamps/provenance baseline.
- [x] Bounded recall result count.
- [x] Manual persistent-memory example in `HAgent.Example`.
- [ ] Persistent conversation memory integration.
- [ ] Automatic session-memory policy.
- [ ] Working memory/context-window budgeting.
- [ ] Short-term task/event memory.
- [ ] Episodic memory.
- [ ] Semantic/long-term memory.
- [ ] SQL Server memory store.
- [ ] MySQL memory store.
- [ ] Persistent lightweight indexing for faster large-store retrieval without a large RAM footprint.
- [ ] Optional vector-memory adapter.
- [ ] Remote embedding-provider support without local GPU requirements.
- [ ] Improved relevance/ranking.
- [ ] Context trimming and compaction.
- [ ] Memory update/upsert semantics.
- [ ] Richer memory provenance source/type fields.
- [ ] Retention/expiration policies.

## 0.4 — Tools
- [ ] Tool definitions with JSON Schema.
- [ ] Executable tool handlers.
- [ ] Predefined/custom tool registry.
- [ ] Per-agent tool selection.
- [ ] Provider capability negotiation for tool/function calling.
- [ ] Provider-neutral tool-call representation.
- [ ] Tool result/observation protocol.
- [ ] Typed argument binding and validation.
- [ ] Tool permissions and approval policies.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit history.
- [ ] Tool-call budgets and loop protection.
- [ ] UI tool execution support.
- [ ] Add manual tool-call examples to `HAgent.Example`.

## 0.5 — UI and application automation
- [ ] Host-registered `ui.*` tools.
- [ ] UI-thread dispatch abstraction.
- [ ] Set control position/size.
- [ ] Set/read control text/value.
- [ ] Approved click/invoke actions.
- [ ] Enable/disable controls.
- [ ] Batch UI operations.
- [ ] Dry-run/preview mode.
- [ ] Undo/rollback hooks where host supports them.
- [ ] Per-control permissions.
- [ ] Example scenarios in `HAgent.Example` for moving controls and setting control values.

## 0.6 — Chat and interaction
- [ ] User ↔ agent chat window.
- [ ] Agent selector.
- [ ] Conversation switching.
- [ ] Persistent conversations.
- [ ] Conversation search and metadata.
- [ ] Attachments/multimodal messages where supported.
- [ ] Live execution status.
- [ ] Tool-call visualization.
- [ ] Reasoning/thinking visibility policy where the provider exposes reasoning separately.
- [ ] Cancel/stop response.
- [ ] Multiple simultaneous conversations.
- [ ] Safe handling of deleted agents referenced in open chat/tasks.
- [ ] Add chat examples to `HAgent.Example`.

## 0.7 — Agent collaboration
- [ ] Agent-to-agent messaging board.
- [ ] Collaboration channels.
- [ ] Direct/broadcast messages.
- [ ] Shared workspace context.
- [ ] Handoff/delegation.
- [ ] Agent roles/capabilities.
- [ ] Routing policies.
- [ ] Maximum hops/depth.
- [ ] Loop detection.
- [ ] Collaboration transcript.
- [ ] Shared/private memory rules.
- [ ] Parallel agent execution.
- [ ] Add collaboration examples to `HAgent.Example`.

## 0.8 — Workflows and autonomy
- [ ] Explicit tasks/jobs.
- [ ] Multi-step workflows.
- [ ] Planning/execution/verification.
- [ ] Background execution.
- [ ] Scheduling.
- [ ] Pause/resume.
- [ ] Durable checkpoints.
- [ ] Conditional/parallel branches.
- [ ] Event-triggered agents.
- [ ] Execution budgets.
- [ ] Add workflow examples to `HAgent.Example`.

## 0.9 — Provider ecosystem
- [ ] Azure OpenAI adapter.
- [ ] Anthropic adapter.
- [ ] Google/Gemini adapter.
- [ ] Ollama/LM Studio adapters.
- [ ] Custom HTTP provider model.
- [ ] Provider capability matrix.
- [ ] Model discovery/cache.
- [ ] Streaming.
- [ ] Multimodal abstraction.
- [ ] Embedding-provider abstraction.
- [ ] Provider response normalization/capability metadata.

## 0.10 — Extensibility and storage ecosystem
- [ ] Provider adapter DLL loading.
- [ ] Tool DLL loading.
- [ ] Custom storage providers.
- [ ] Versioned extension contracts.
- [ ] Conversation persistence across file/SQL/MySQL.
- [ ] Memory persistence across file/SQL/MySQL.
- [ ] Optional vector/semantic companion package.
- [ ] External secret stores and rotation.
- [ ] Configuration profiles/workspaces.
- [ ] Audit logging.

## 0.11 — Developer platform
- [ ] Optional dependency injection.
- [ ] Optional `Microsoft.Extensions.AI` integration.
- [ ] Provider/tool SDKs.
- [ ] Provider/tool contract test harnesses.
- [ ] Agent simulation/test mode.
- [ ] Diagnostics/trace viewer.
- [ ] UI automation/database/document workflow examples.
- [ ] Expand `HAgent.Example` into the executable manual verification suite for every completed capability.

## 1.0 — Stable HAgent platform
- [ ] Stable public contracts.
- [ ] Storage migrations/versioning.
- [ ] NuGet packages.
- [ ] Comprehensive integration tests.
- [ ] Security/permission tests.
- [ ] Provider/tool extension guides.
- [ ] Memory/retrieval guides.
- [ ] Collaboration/workflow guides.
- [ ] .NET 10 target after the development environment is upgraded to a compatible Visual Studio release.

## Non-negotiable design rules
HAgent is provider-neutral. Providers supply model connectivity/capabilities; agents supply behavior; memory supplies context; tools supply controlled actions; the runtime coordinates execution; the host application owns real-world side effects.

Memory must not require a local GPU, local embedding model, vector database, or large resident RAM footprint. Vector memory is optional and belongs behind an adapter/companion package.

AI models must never receive arbitrary access to WinForms controls, processes, files, databases, reflection, or host resources. Applications expose such capabilities explicitly as validated tools.

Configuration deletion must not silently destroy active runtime work. Execution snapshots isolate running work from later configuration changes.

Provider responses must be normalized without destroying provider-specific metadata. Reasoning/thinking is an optional separate response component when explicitly exposed by a provider; markup-based guessing must not be the default assumption.

`HAgent.Example` is the manual integration and feature-verification application. `HAgent.Tests` remains the automated testing project; the two have different purposes.

Project-state documentation is part of the repository contract. Meaningful architecture or milestone changes must keep `README.md`, `roadmap.md`, `plan.md`, and `AGENTS.md` synchronized with the actual implementation.

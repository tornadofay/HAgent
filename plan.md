# HAgent Development Plan

## Current milestone: 0.3 — Memory

### 0.1 Foundation — completed
- [x] Multi-target core for .NET Framework 4.8.1 and .NET 9.
- [x] Provider-neutral adapter contract.
- [x] OpenAI-compatible provider adapter.
- [x] File storage with protected secret storage.
- [x] SQL Server and MySQL storage foundations.
- [x] Provider, agent, and tool management UI.
- [x] Shared HAgent `Header` and `HMessage` UI conventions.
- [x] HButton used as the HAgent WinForms action control.
- [x] Agent conversation session history and context forwarding.
- [x] Provider connection test capability.
- [x] Provider model catalog capability.
- [x] Agent/provider deletion rules in the UI and storage.

### 0.2 Runtime foundation — completed
- [x] Agent runtime abstraction and default execution pipeline.
- [x] Agent execution state model.
- [x] Execution correlation IDs via stable execution IDs.
- [x] Agent/provider execution snapshots so running work is independent from mutable configuration.
- [x] Default provider routing abstraction.
- [x] Multiple configured provider candidates per agent in the runtime.
- [x] Configurable provider-attempt limits.
- [x] Explicit cancellation boundary.
- [x] Explicit timeout boundary.
- [x] Distinguish caller cancellation from runtime timeout.
- [x] Configurable retry count per provider.
- [x] Conservative provider error classification.
- [x] Exponential retry backoff with stronger delay for rate-limit conditions.
- [x] Execution lifecycle events for host applications.
- [x] Execution duration and last-provider diagnostics.
- [x] Structured execution failure categories.
- [x] Dependency-free memory abstraction and in-memory lightweight text/metadata search foundation.
- [x] Memory design explicitly avoids requiring GPU, local embedding models, or large resident RAM.

### Deferred from 0.2 into later milestones
- [ ] Persistent multi-provider routing configuration/UI.
- [ ] Executable provider-native tool-call loop.
- [ ] Runtime leases beyond execution snapshots where host applications require coordinated lifecycle ownership.

## Milestone: 0.3 — Memory
- [ ] Persistent conversation memory.
- [ ] Working memory/context window management.
- [ ] Task memory.
- [ ] Episodic memory.
- [ ] Semantic/long-term memory abstraction.
- [ ] File/SQL memory stores.
- [ ] Lightweight indexed text retrieval for low-RAM systems.
- [ ] Optional vector-memory adapter abstraction.
- [ ] Remote embedding provider support without local GPU requirements.
- [ ] Memory scopes: session, task, agent, user, application, shared.
- [ ] Memory retrieval and relevance ranking.
- [ ] Context compaction and summarization.
- [ ] Explicit remember/forget APIs.
- [ ] Memory provenance and timestamps.
- [ ] Memory retention/expiration policies.
- [ ] Bounded memory loading so large stores are not fully loaded into RAM.

## Milestone: 0.4 — Tools
- [ ] Tool definitions with JSON Schema.
- [ ] Executable tool handlers.
- [ ] Predefined tool registry.
- [ ] Custom tool registry.
- [ ] Per-agent tool selection.
- [ ] Tool permission policies.
- [ ] Approval-required tools.
- [ ] Tool execution timeout/cancellation.
- [ ] Tool result/observation protocol.
- [ ] Tool audit history.
- [ ] Host UI-control tools for WinForms.
- [ ] Generic database/file/http/workflow tool abstractions.
- [ ] Tool DLL/plugin loading.
- [ ] Tool schema validation.
- [ ] Tool-call budget and loop protection.
- [ ] Typed argument binding and validation.
- [ ] Provider-native tool/function calling capability negotiation.

## Milestone: 0.5 — Agent collaboration
- [ ] Agent-to-agent messaging board.
- [ ] Agent delegation and handoff.
- [ ] Shared task context.
- [ ] Agent collaboration permissions.
- [ ] Maximum hop/depth limits.
- [ ] Loop detection.
- [ ] Parallel agent execution.
- [ ] Collaboration history.
- [ ] Agent roles/capabilities for delegation.
- [ ] Cancellation propagation across agent trees.

## Milestone: 0.6 — Chat and tasks
- [ ] User-to-agent chat window.
- [ ] Agent selector in chat.
- [ ] Persistent conversations.
- [ ] Conversation search.
- [ ] Attachments/multimodal messages.
- [ ] Long-running task UI.
- [ ] Progress/status events.
- [ ] Background execution.
- [ ] Pause/resume/cancel.
- [ ] Task checkpoints.
- [ ] Resume interrupted executions from checkpoints.
- [ ] Live tool-call/status visualization.
- [ ] Agent deletion semantics for open chat/task references.

## Milestone: 0.7 — Extensibility
- [ ] Provider adapter DLL loading.
- [ ] Tool DLL loading.
- [ ] Custom storage provider DLLs.
- [ ] Provider capability discovery.
- [ ] Provider model catalogs.
- [ ] Persistent multi-provider routing configuration/UI.
- [ ] Streaming responses.
- [ ] Multimodal provider abstractions.
- [ ] OpenAI, Anthropic, Google, Azure, Ollama, and custom adapters as separate packages where appropriate.
- [ ] Versioned extension contract.
- [ ] Extension isolation and failure handling.

## Milestone: 0.8 — Safety and operations
- [ ] Per-agent permissions.
- [ ] Tool-level allow/deny policies.
- [ ] Confirmation policies for destructive operations.
- [ ] Secret isolation.
- [ ] Audit log abstraction.
- [ ] Usage/token/cost tracking.
- [ ] Concurrency limits.
- [ ] Resource budgets.
- [ ] Rate-limit/backoff policies.
- [ ] Safe defaults for autonomous execution.
- [ ] Human approval checkpoints.
- [ ] Sensitive-data redaction in logs.

## Milestone: 0.9 — Developer platform
- [ ] Stable host integration API.
- [ ] Agent lifecycle events.
- [ ] Tool registration API for application controls/services.
- [ ] UI-thread dispatch abstraction for WinForms tools.
- [ ] Custom agent runtime hooks.
- [ ] Custom memory providers.
- [ ] Custom execution strategies.
- [ ] Diagnostics/trace viewer.
- [ ] Examples for UI automation, database tasks, document workflows, and multi-agent collaboration.

## Milestone: 1.0 — Stable HAgent runtime
- [ ] Stable public interfaces.
- [ ] Migration/versioning support for saved configuration.
- [ ] Backward-compatible storage format.
- [ ] Comprehensive unit/integration tests.
- [ ] Documentation and examples.
- [ ] NuGet packages.
- [ ] Provider/tool extension guide.
- [ ] .NET 10 target after the Windows 11 / VS 2026 development environment is adopted.

## Non-negotiable design rules
HAgent must remain provider-neutral. A provider supplies model connectivity and capabilities; an agent supplies behavior; memory supplies context; tools supply controlled actions; the runtime coordinates execution; the host application owns real-world side effects.

Memory must not require a local GPU, embedding model, vector database, or large resident RAM footprint. Vector memory is optional and must sit behind an adapter.

An AI model must never receive arbitrary access to WinForms controls, reflection, processes, files, databases, or other host resources. Those capabilities must be exposed explicitly as typed tools with validation and policy enforcement.

Deleting configuration must not silently destroy a running execution. Runtime work operates from an execution snapshot so an active task can finish independently of later configuration changes.

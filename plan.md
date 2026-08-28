# HAgent Development Plan

## Current milestone: 0.2 — Runtime foundation

### Foundation completed
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
- [x] Agent execution state model.
- [x] Agent/provider execution snapshot model.
- [x] Default provider routing abstraction.
- [x] Agent runtime abstraction and default execution pipeline.
- [x] Execution timeout/cancellation boundary.
- [x] Dependency-free in-memory memory store with lightweight text/metadata search.
- [x] Memory scopes model without requiring embeddings or a GPU.

### Remaining 0.2 work
- [ ] Unified Send/Read API with explicit execution context across all runtime paths.
- [ ] Execution correlation IDs and lifecycle events.
- [ ] Runtime diagnostics and structured failure reporting.
- [ ] Provider error classification.
- [ ] Retry/backoff policy separate from provider routing.
- [ ] Runtime leases so deleting an agent/provider cannot invalidate active executions.
- [ ] Real multi-provider configuration/persistence and routing UI.
- [ ] Finish visual/layout QA at different window sizes.
- [ ] Executable tool-call loop.

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

## Milestone: 0.7 — Extensibility
- [ ] Provider adapter DLL loading.
- [ ] Tool DLL loading.
- [ ] Custom storage provider DLLs.
- [ ] Provider capability discovery.
- [ ] Provider model catalogs.
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

Deleting configuration must not silently destroy a running execution. Runtime work must eventually operate from an execution snapshot/lease independent from mutable configuration.

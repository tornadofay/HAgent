# HAgent Development Plan

## Current milestone: 0.1.x — Foundation and desktop configuration

### Completed
- [x] Multi-target core for .NET Framework 4.8.1 and .NET 9.
- [x] Provider-neutral adapter contract.
- [x] OpenAI-compatible provider adapter.
- [x] File storage with protected secret storage.
- [x] SQL Server and MySQL storage foundations.
- [x] Provider, agent, and tool management UI.
- [x] Borderless rounded form shell using the shared HAgent `Header`.
- [x] Shared `HMessage` API adopted by HAgent WinForms UI.
- [x] Agent conversation session history and context forwarding.
- [x] Provider connection test capability.
- [x] Provider model catalog capability.
- [x] Agent/provider deletion rules in the management UI.
- [x] Provider dependency protection in storage.

### In progress
- [ ] Finish visual/layout QA at different window sizes.
- [ ] Make active runtime work independent from configuration deletion.
- [ ] Add real provider model selection persistence and multi-provider routing UI.
- [ ] Add executable tool-call loop.
- [ ] Define the stable runtime/execution API before adding long-running tasks.

## Milestone: 0.2 — Runtime foundation
- [ ] Unified Send/Read API with explicit execution context.
- [ ] Agent execution state machine.
- [ ] Provider fallback and routing policies.
- [ ] Request/response telemetry without leaking secrets.
- [ ] Cancellation, timeouts, retries, and provider error classification.
- [ ] Immutable agent/provider execution snapshots for running tasks.
- [ ] Runtime leases so deleting an agent/provider cannot invalidate an active execution.
- [ ] Execution correlation IDs and lifecycle events.
- [ ] Runtime diagnostics and structured failure reporting.

## Milestone: 0.3 — Memory
- [ ] Conversation memory.
- [ ] Working memory/context window management.
- [ ] Task memory.
- [ ] Episodic memory.
- [ ] Semantic/long-term memory abstraction.
- [ ] File/SQL memory stores.
- [ ] Vector-memory adapter abstraction.
- [ ] Memory scopes: session, agent, user, application, shared.
- [ ] Memory retrieval and relevance ranking.
- [ ] Context compaction and summarization.
- [ ] Explicit remember/forget APIs.
- [ ] Memory provenance and timestamps.
- [ ] Memory retention/expiration policies.

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

## Design rule
HAgent should remain provider-neutral. A provider supplies model connectivity and capabilities; an agent supplies behavior; memory supplies durable context; tools supply controlled actions; the runtime coordinates execution; the host application owns real-world side effects.

An AI model must never receive arbitrary access to WinForms controls, reflection, processes, files, databases, or other host resources. Those capabilities must be exposed explicitly as typed tools with validation and policy enforcement.

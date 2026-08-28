# HAgent Roadmap

HAgent is intended to become a small, provider-neutral agent runtime that applications can embed and then extend with memory, tools, collaboration, workflows, and UI/application automation without making the common deployment heavy.

## 0.1 — Foundation

Completed foundation capabilities:

- Provider and agent domain model.
- Provider/agent relationship and usage filtering.
- Multiple provider references on an agent model.
- OpenAI-compatible provider adapter.
- File persistence.
- Windows DPAPI secret protection.
- SQL Server persistence foundation.
- MySQL persistence foundation.
- Borderless rounded WinForms management UI.
- Providers, agents, and tools management UI.
- Session API with `SendAsync` and `ReadAsync`.
- Session history forwarding.
- Provider connection testing.
- Provider model discovery.
- Agent/provider deletion rules.
- Shared HAgent `Header`, `HMessage`, and `HButton` UI primitives.
- `HAgent.Example` manual integration host for developer-facing feature verification.

## 0.2 — Runtime Foundation — Completed

The 0.2 milestone establishes a provider-neutral execution layer without turning HAgent into a heavy orchestration framework.

- Agent runtime abstraction.
- Agent execution state model.
- Execution IDs/correlation identifiers.
- Execution snapshots of agent/provider configuration.
- Provider routing abstraction.
- Ordered provider candidates and attempt limits.
- Cancellation and timeout boundaries.
- Distinction between caller cancellation and timeout.
- Retry count per provider.
- Conservative provider error classification.
- Exponential backoff with rate-limit-aware delay.
- Execution lifecycle events.
- Execution duration and provider diagnostics.
- Structured execution failure categories.
- Lightweight memory abstraction and in-memory text/metadata retrieval foundation.
- Explicit no-GPU / low-RAM memory design rule.
- Resolved system prompt inserted exactly once into provider requests.
- Provider failure details preserved for diagnostics.

0.2 deliberately does **not** claim to have completed persistent memory, provider-native tool calling, long-running durable tasks, or full multi-provider configuration UI.

## Cross-cutting response handling

Provider responses must be normalized into a provider-neutral representation before application/UI consumption.

- Separate ordinary assistant content from provider-exposed reasoning/thinking content.
- Preserve reasoning metadata when the provider explicitly exposes it through a supported response field.
- Never assume that `<think>...</think>` markup is universally equivalent to provider-native reasoning metadata.
- Keep plain-text providers fully compatible.
- Allow applications to choose whether exposed reasoning is stored, displayed, logged, or discarded.
- Prevent reasoning content from unexpectedly appearing as ordinary user-facing assistant text.
- Preserve raw provider metadata where useful without coupling the core model to one vendor's schema.
- Add provider/model capability metadata before automatically classifying responses as reasoning-capable.
- Add manual response-normalization examples to `HAgent.Example`.

## 0.3 — Memory — Current

### Initial implementation completed

- Persistent file memory store using append-oriented JSONL records.
- Streaming file search without loading the entire memory file into RAM.
- Explicit `remember` operation through `HAgentClient.RememberAsync`.
- Explicit `recall` operation through `HAgentClient.RecallAsync`.
- Explicit `forget` operation through `HAgentClient.ForgetAsync`.
- Memory scopes: session, task, agent, user, application, shared.
- Metadata filtering.
- Creation timestamp/provenance baseline.
- Bounded recall result count.

### Remaining 0.3 work

- Persistent conversation memory.
- Automatic session-memory policy.
- Working memory and context-window budgeting.
- Short-term task/event memory.
- Episodic memory.
- Semantic/long-term memory.
- SQL Server memory store.
- MySQL memory store.
- Lightweight persistent indexing for faster large-store retrieval.
- Optional vector-memory adapter.
- Remote embedding-provider support without local GPU requirements.
- Improved relevance/ranking.
- Context trimming and compaction.
- Memory update/upsert semantics.
- Richer memory provenance/source/type fields.
- Retention/expiration policies.
- Manual Memory examples in `HAgent.Example`.

## 0.4 — Tools

### Tool model

- First-class tool definition.
- JSON Schema input validation.
- Output/result contract.
- Separate executable handler.
- Tool registry and discovery.
- Predefined and custom tools.
- Per-agent tool assignment.
- Per-conversation temporary tools.
- Tool aliases and versioning.

### Provider integration

- Provider capability negotiation for structured tool/function calls.
- Provider-neutral tool-call representation.
- Tool result/observation protocol.
- Streaming tool-call support where providers allow it.

### Tool execution

- Typed argument binding.
- Validation.
- Cancellation.
- Timeouts.
- Progress reporting.
- Tool execution history.
- Loop protection.
- Tool-call budgets.

### UI

- Tools management tab.
- Predefined tools section.
- Custom tool definition creator.
- Schema editor/validator.
- Test-tool interface.
- Tool health/diagnostics.
- Per-agent tool selection.
- Permission and confirmation policies.

## 0.5 — UI/application automation

This milestone enables scenarios such as:

`move GDI box to X=250,Y=120`

or:

`write "hello" into TextBox CustomerName`.

Architecture:

1. The host registers an explicit capability such as `ui.move_control`.
2. The tool definition describes the operation and typed arguments.
3. The model requests the tool.
4. HAgent validates arguments and policy.
5. The host executes the side effect on the correct UI thread.
6. The tool returns a structured result.
7. HAgent adds the result as an observation.
8. The agent continues or finishes.

Planned capabilities:

- Set position/size.
- Set text/value.
- Read selected control state.
- Click/invoke an approved control action.
- Enable/disable controls.
- Create/update/remove application objects through explicit host tools.
- Batch UI operations.
- Control-level permissions.
- Human approval for sensitive operations.
- Dry-run/preview mode.
- Undo/rollback hooks where the host can provide them.
- UI-thread marshaling helpers.
- No arbitrary reflection/process/memory access by default.

## 0.6 — Chat and interaction

- User ↔ agent chat window.
- Agent selector in chat.
- Conversation switching.
- Persistent conversations.
- Conversation search.
- Conversation metadata and titles.
- Attachments/multimodal messages where supported.
- Live execution status.
- Tool-call visualization.
- Reasoning/thinking visibility policy where the provider exposes reasoning separately.
- Cancel/stop response.
- Open multiple conversations.
- Agent deletion handling for open conversations.

## 0.7 — Agent collaboration

- Agent-to-agent messaging board.
- Collaboration channels.
- Direct agent messages.
- Broadcast messages.
- Shared workspace context.
- Agent handoff.
- Delegation.
- Roles and capabilities.
- Message routing policies.
- Maximum hops/depth.
- Loop detection.
- Collaboration transcript.
- Shared versus private memory rules.
- Human intervention points.
- Parallel agent execution.

## 0.8 — Workflows and autonomy

- Explicit agent tasks/jobs.
- Multi-step workflows.
- Task state machine.
- Planning / execution / verification pattern.
- Background execution.
- Scheduling.
- Pause/resume.
- Durable checkpoints.
- Retry policies per step.
- Human approval steps.
- Conditional branches.
- Parallel branches.
- Event-triggered agents.
- Queue-based execution adapter.
- Maximum autonomy and resource budgets.

## 0.9 — Broader provider ecosystem

- Additional provider adapters.
- Azure OpenAI.
- Anthropic.
- Google/Gemini.
- Ollama.
- LM Studio and other OpenAI-compatible local servers.
- Custom HTTP providers.
- Provider capability matrix.
- Provider-specific feature negotiation.
- Model discovery and caching.
- Embedding-provider abstraction.
- Multimodal provider abstraction.
- Streaming.
- Versioned provider extension contract.
- Provider response normalization/capability metadata.

## 0.10 — Extensibility and storage ecosystem

- Provider adapter DLL loading.
- Tool DLL loading.
- Custom storage provider DLLs.
- Discoverable extensions.
- Extension validation and isolation/failure handling.
- Persistent multi-provider routing configuration.
- Conversation persistence across file/SQL Server/MySQL.
- Memory persistence across file/SQL Server/MySQL.
- Optional vector/semantic-memory companion package.
- Secret rotation.
- External secret-store adapters.
- Import/export with explicit secret handling.
- Configuration profiles/workspaces.
- Multi-user ownership/authorization hooks.
- Audit logging.

## 0.11 — Developer platform

- Optional dependency-injection integration.
- Optional `Microsoft.Extensions.AI` integration.
- Example provider SDK.
- Example tool SDK.
- Provider contract test harness.
- Tool contract test harness.
- Agent simulation/test mode.
- Diagnostics/trace viewer.
- Developer examples for UI automation, database tasks, document workflows, and multi-agent collaboration.
- Expand `HAgent.Example` as the executable manual verification suite for every completed capability.

## 1.0 — Stable HAgent platform

- Stable public contracts.
- Backward compatibility policy.
- Storage migration/versioning.
- NuGet packages.
- Signed releases.
- Comprehensive integration tests.
- Provider contract tests.
- Tool contract tests.
- Memory/retrieval tests.
- Collaboration loop tests.
- Security/permission tests.
- Complete embedding documentation.
- Custom provider documentation.
- Custom tool documentation.
- Application/UI automation documentation.
- .NET 10 target after the development environment is upgraded to a compatible Visual Studio release.

## Design principles

- The core runtime stays small.
- Provider transport is separate from agent behavior.
- A tool definition is never executable code by itself.
- Applications explicitly expose capabilities; agents never receive arbitrary process access.
- Memory is explicit state supplied to the model, not a claim that the model itself permanently remembers.
- Provider responses are normalized without destroying provider-specific metadata.
- Reasoning/thinking is treated as a separate optional response component when explicitly exposed by a provider.
- Every autonomous action should be observable, cancellable, and policy-controlled.
- File/database/vector stores remain replaceable.
- Advanced functionality belongs in optional assemblies when it would otherwise bloat common deployments.
- The manual example host is part of the development workflow, not a substitute for automated tests.
- .NET Framework 4.8.1 and .NET 9 remain the current supported targets.
- No local GPU, local embedding model, vector database, or large resident RAM footprint may be a prerequisite for normal operation.

## Companion-package candidates

- RAG/vector database integrations.
- MCP client/server integration.
- Cloud configuration service.
- Centralized multi-user administration.
- Advanced workflow designer.
- Web dashboard.
- Sandboxed/container execution.
- Computer-use/desktop-vision automation.

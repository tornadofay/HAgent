# HAgent Roadmap

HAgent is intended to become a small, provider-neutral agent runtime that applications can embed and then extend with memory, tools, collaboration, workflows, and UI/application automation without making the core framework heavy.

## 0.1 — Foundation (current line)

- Provider and agent domain model.
- Provider/agent relationship and filtering.
- Multiple providers can be assigned to an agent for ordered fallback/routing.
- OpenAI-compatible provider adapter.
- File persistence.
- Windows DPAPI secret protection.
- SQL Server persistence.
- MySQL persistence.
- Borderless rounded WinForms management UI.
- Providers, agents, and tools management tabs.
- Session API with `SendAsync` and `ReadAsync`.
- Session history is now sent back to the provider instead of only the latest message.
- Tool definition/registry abstraction.
- Predefined and custom tool definitions in the development UI.

## 0.2 — Memory and conversations

- Dedicated user ↔ agent chat window.
- Agent selector inside the chat window.
- Conversation persistence independent from transient `AgentSession` objects.
- Conversation IDs, titles, timestamps, participants, and metadata.
- Working memory: current conversation state and recent observations.
- Short-term memory: recent task/event context with configurable limits.
- Episodic memory: durable records of important things that happened during tasks.
- Semantic memory: durable facts/knowledge explicitly saved for future retrieval.
- User/application memory versus agent-private memory.
- Agent memory scopes and isolation.
- Memory importance, expiration, pinning, and deletion.
- Context-window budgeting.
- Automatic history trimming.
- Summarization/compaction of older conversations.
- Memory search/retrieval abstraction so SQL, files, vector stores, or custom stores can be plugged in.
- Explicit `remember`, `recall`, and `forget` operations for application-controlled memory.

## 0.3 — Real agent runtime

- Agent execution loop separated from provider transport.
- Model response represented as text, structured content, tool calls, tool results, and metadata.
- Stop/cancel/timeout support.
- Retry and provider failover policies.
- Provider capability discovery.
- Streaming responses.
- Structured output / JSON mode where supported.
- Provider-neutral tool/function calling.
- Model/context/token accounting.
- Execution traces and step history.
- Pluggable policies for maximum steps, maximum cost, timeout, and failure behavior.

## 0.4 — Tools and application capabilities

### Tool model

- First-class tool definition: ID, name, description, category, input schema, output contract, version, and capabilities.
- First-class executable handler separate from the definition.
- Tool registry with discovery and lifetime management.
- Tool enable/disable state.
- Per-agent tool assignments.
- Per-conversation temporary tools.
- Tool aliases and versioning.

### Tool UI

- Tools management tab.
- Predefined tools section.
- Custom tool-definition creator.
- Input-schema editor and validation.
- Test-tool interface.
- Tool execution history.
- Tool health/diagnostic status.
- Per-agent tool selection.
- Permissions and confirmation policies.

### Application integration

- Host-registered delegate tools.
- Strongly typed C# tool handlers.
- Async tools.
- Cancellation-aware tools.
- Progress reporting.
- Tool results returned to the model as observations.
- Tool errors represented as recoverable observations.
- UI-thread-aware tools for WinForms.
- Cross-thread marshaling helpers.

## 0.5 — UI/application automation

This is the layer that enables scenarios such as:

`move GDI box to X=250,Y=120`

or:

`write "hello" into TextBox CustomerName`.

Architecture:

1. The application exposes a safe named capability such as `ui.move_control`.
2. The tool definition tells the model what arguments exist.
3. The agent requests the tool with structured arguments.
4. HAgent validates the arguments against the schema and policy.
5. The host handler performs the actual UI operation on the correct UI thread.
6. The handler returns a structured result.
7. The result is added to the agent's context as an observation.
8. The agent decides what to do next.

Planned capabilities:

- Set position/size.
- Set text/value.
- Read selected control state.
- Click/invoke an approved control action.
- Enable/disable controls.
- Create/remove/update application objects through explicit host tools.
- Batch UI operations.
- Tool permissions by control or capability.
- User approval prompts for sensitive operations.
- Dry-run/preview mode.
- Undo/rollback hooks where the host can provide them.
- No arbitrary reflection/process/memory access by default.

## 0.6 — Agent collaboration

- Agent-to-agent message board.
- Agent collaboration channels.
- Direct agent messages.
- Broadcast messages.
- Shared workspace context.
- Agent handoff.
- Delegation: one agent asks another agent to perform a task.
- Role/capability declarations.
- Message routing policies.
- Maximum-hop/loop protection.
- Collaboration transcript and audit trail.
- Shared versus private memory rules.
- Human intervention/approval points.
- Parallel agent tasks.

## 0.7 — Workflows and autonomy

- Explicit agent tasks/jobs.
- Multi-step workflows.
- Task state machine.
- Planning/execution/verification pattern.
- Background execution.
- Scheduling.
- Pausing/resuming jobs.
- Durable checkpoints.
- Retry policies per step.
- Human approval steps.
- Conditional branches.
- Parallel branches.
- Event-triggered agents.
- Queue-based execution adapter.
- Maximum autonomy / execution budgets.

## 0.8 — Broader provider ecosystem

- Additional major provider adapters.
- Azure OpenAI.
- Anthropic.
- Google/Gemini.
- Local OpenAI-compatible servers such as Ollama/LM Studio.
- Custom HTTP adapters.
- Provider capability matrix.
- Provider-specific feature negotiation.
- Model discovery and caching.
- Embedding provider abstraction.
- Multimodal input/output abstraction where supported.

## 0.9 — Storage and enterprise capabilities

- Versioned configuration migrations.
- Atomic/recoverable file persistence.
- Tool-definition persistence.
- Conversation persistence for file, SQL Server, and MySQL.
- Memory persistence for file and database stores.
- Optional vector/semantic-memory companion package.
- Secret rotation.
- External secret-store adapters.
- Import/export with explicit secret handling.
- Configuration profiles/workspaces.
- Multi-user ownership/authorization hooks.
- Audit logging.

## 0.10 — Developer ecosystem

- Dependency-injection integration without making DI mandatory.
- Optional `Microsoft.Extensions.AI` integration.
- Extension/plugin loading model.
- Discoverable provider DLLs.
- Discoverable tool DLLs.
- Version/capability metadata for extensions.
- Runtime extension validation.
- Example provider SDK.
- Example tool SDK.
- Test harness for custom providers and tools.
- Agent simulation/test mode.

## 1.0 — Stable agent platform

- Stable public contracts.
- Backward compatibility policy.
- NuGet packages with signed releases.
- Comprehensive integration tests.
- Provider contract tests.
- Tool contract tests.
- Memory/retrieval tests.
- Collaboration loop tests.
- Security/permission tests.
- Documentation for embedding HAgent into WinForms applications.
- Documentation for custom providers.
- Documentation for custom tools.
- Documentation for application/UI automation.

## Design principles

- The core runtime stays small.
- Provider transport is never mixed with agent behavior.
- A tool definition is never treated as executable code.
- Applications explicitly expose capabilities; agents do not receive arbitrary process access.
- Memory is explicit state supplied to the model, not a claim that the model itself permanently remembers.
- Every autonomous action should be observable, cancellable, and policy-controlled.
- File/database/vector stores remain replaceable.
- Advanced functionality belongs in optional assemblies when it would otherwise bloat the common deployment.
- .NET Framework 4.8.1 and .NET 9 remain the supported targets for the current development line; .NET 10 is a future target after the development environment is upgraded.

## Ideas that may remain companion packages

- RAG/vector databases.
- MCP client/server integration.
- Cloud configuration service.
- Centralized multi-user administration.
- Advanced workflow designer.
- Web dashboard.
- Sandbox/container execution.
- Computer-use/desktop-vision automation.

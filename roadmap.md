# HAgent Roadmap

## 0.1 — Foundation

- Provider and agent domain model.
- Provider/agent relationship and filtering.
- OpenAI-compatible provider adapter.
- File persistence.
- Windows DPAPI secret protection.
- SQL Server persistence.
- MySQL persistence.
- WinForms management window.
- Session API with `SendAsync` and `ReadAsync`.
- Sample application.

## 0.2 — Production hardening

- Better validation and diagnostics.
- Provider connection-test API that does not mutate configuration.
- Secret rotation workflow.
- Import/export with explicit secret handling.
- Atomic and recoverable file persistence.
- Configuration versioning/migrations.
- More precise error model with provider error metadata.
- Built-in logging abstraction.
- CI for .NET Framework 4.8.1 and .NET 9; add .NET 10 when the development toolchain is upgraded.

## 0.3 — More provider capability

- Add .NET 10 target after the development environment is upgraded to a supported Visual Studio/.NET SDK toolchain.

- Streaming responses.
- Provider model discovery.
- Request timeout and retry policies.
- Provider capability metadata.
- Structured output / JSON mode where supported.
- Tool/function calling abstraction.
- Provider adapters for additional major AI APIs.

## 0.4 — Conversation and collaboration layer

- Dedicated chat window between the user and a selected agent.
- Agent selector in the chat window for switching agents without leaving the conversation.
- Persistent conversations and message history.
- Provider/model-aware conversation context.
- Message board / collaboration channel where agents can publish messages to each other.
- Agent-to-agent conversations with routing, permissions, and loop protection.
- Multiple providers can participate in one agent workspace; agents are not limited to OpenAI.
- Provider failover and optional routing policies.
- Context window policies.
- Message trimming/summarization.
- Conversation export.
- Read APIs that can page large histories.

## 0.5 — Tools and application capabilities

- First-class tool abstraction with name, description, input schema, and execution handler.
- Tool creation/registration API.
- Tool management UI.
- Enable/disable tools per agent.
- Tool permission and approval policies.
- Tool execution history and diagnostics.
- Provider-neutral function/tool calling.
- Safe execution boundaries and cancellation.

## 0.6 — Application integration

- Dependency-injection integration without making DI mandatory.
- Optional `Microsoft.Extensions.AI` bridge.
- Optional streaming UI.
- Per-agent usage statistics.
- Simple health diagnostics.

## 1.0 — Stable contract

- Stable public abstractions.
- Backward compatibility policy.
- NuGet packages with signed releases.
- Database migration tooling.
- Local secret-store abstraction with pluggable enterprise backends.
- Comprehensive integration tests.
- Documentation for application embedding and custom adapters.

## Ideas deliberately outside the core

These may become companion packages rather than core functionality:

- RAG/vector stores.
- Autonomous agent loops.
- Workflow orchestration.
- MCP server/client integrations.
- Web dashboards.
- Cloud configuration service.
- Centralized multi-user administration.

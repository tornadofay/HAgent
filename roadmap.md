# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architecture lives in `docs/architecture/`; current implementation work lives in `docs/plan/`.

# Foundations — 0.1 through 0.7

This section records the completed path and the remaining hardening work that grows from it.

## 0.1 Foundation — complete

- Multi-target .NET Framework 4.8.1 and .NET 9 where supported.
- Provider/agent configuration and multi-provider relationships.
- OpenAI-compatible provider adapter.
- File, SQL Server, and MySQL persistence foundations.
- Protected local secrets.
- Provider/agent/tool management UI.
- Model discovery and connection testing.
- Dependency-aware deletion behavior.
- `HAgent.Example` integration host, global agent selection, output handling, and modular examples.

## 0.2 Runtime — complete

- Execution lifecycle and stable execution IDs.
- Provider routing and ordered candidates.
- Retries, timeout, cancellation, diagnostics, and structured failures.
- Actionable provider/model/account error reporting.
- System-prompt resolution.
- Execution snapshots.
- Low-RAM/no-GPU design constraints.

## 0.3 Memory + Context — foundation complete

- Persistent JSONL memory and bounded search.
- Explicit remember/recall/forget and scopes.
- Typed Fact/Preference/Task/Event records.
- Persistent conversations and sessions.
- Context budgets and tokenizer-free estimation.
- Conservative automatic memory.
- Lightweight relevance ranking.
- Episodic memory with provenance.

Deferred maturation: memory upsert/update, retention/expiration, context compaction, larger-store indexing, SQL Server/MySQL memory stores, conversation search/metadata, optional vector memory, and remote embeddings.

## 0.4 Provider Capabilities + Response Normalization — foundation complete

- Tri-state capabilities with evidence/confidence and caching.
- Normalized text, reasoning, structured output, tool calls, usage, and provider metadata.
- Reasoning separation and diagnostic `<think>` handling.
- Provider error classification/advice.
- Streaming contract, OpenAI-compatible SSE, cancellation, and live streaming verification.

## 0.5 Tools + Agent Loop — foundation complete

- BuiltIn, Application, Declarative, UI, SqlServer, and MySql tool types.
- Definition/handler separation.
- Tool registry and application handlers.
- JSON Schema validation.
- Provider tool transport.
- Bounded multi-turn tool loops.
- Tool-definition persistence.
- Agent `ToolIds` assignment.
- Live Groq tool-loop verification.

Hardening: per-session temporary tools, built-in handlers, declarative execution, aliases/versioning, timeout/cancellation/progress, audit/history/budgets, stronger loop detection, and capability negotiation.

## 0.6 Safety + Permissions — foundation complete

- General permission configuration UI.
- Persisted WinForms permission policy.
- Safe defaults for discovery/read/write/invoke.

Remaining: broader authorization across tool categories, host authorization callbacks, approval lifecycle, guardrails, budgets/observability, and sensitive-data redaction.

## 0.7 WinForms UI Context + Data Discovery — complete

Locally verified: Form/UserControl/control-tree attachment, stable identity, read-only inspection, semantic discovery, native/bound data-source discovery, CurrencyManager/current-item relationships, control-to-source relationships, external `IHyperControl`-style adaptation, bounded application-object discovery, `maxDepth`/`maxCollectionItems`, structured projection/query contracts, and the complete Example verification surface.

# Phase 0.8 — Data Access + Authorization

## Goal
Turn verified structured-query contracts into safe real application/database access.

## Steps

1. Application-owned `IDataQuerySource` adapter.
2. Authoritative schema/field allow-list.
3. Separate discovery/query/export/write permissions.
4. Host authorization callbacks.
5. Query/result limits, cancellation, timeout, and resource budgets.
6. Restricted SQL Server read adapter.
7. Restricted MySQL read adapter.
8. Database audit/correlation metadata.
9. Read-only database tools before writes.
10. Live Example with runtime-only connection fields.

## Boundaries

No raw SQL tool, unrestricted table/field access, or authorization inferred from UI/application metadata. Credentials remain outside normal agent/tool configuration.

# Phase 0.9 — Runtime Agent Instances

## Goal
Make live agents first-class runtime objects separate from reusable profiles.

## Steps

1. Runtime instance with stable instance ID and profile reference.
2. Application, Workspace, Context/Form, Session, Task, and Ephemeral scopes.
3. Runtime-specific context and provider/model overrides.
4. Independent memory ownership.
5. Concurrent execution of multiple instances.
6. External scheduling, cancellation, timeout, correlation, and stale-result protection.
7. Explicit active/retired/shutdown lifecycle.
8. Dynamic agents not persisted as configuration by default.
9. Optional runtime-state persistence for recovery/collaboration.
10. Deterministic Example verification.
11. HWorld adapter verification.

## HWorld gate

HWorld can begin integration when the runtime supports independent instances, asynchronous execution, caller-provided observation/context, structured tool requests, cancellation/timeout, and stale-result protection. HWorld remains authoritative for world state and action validation.

# Phase 0.10 — Workspaces, Routing + Chat

## Goal
Allow a user and multiple runtime agents to work visibly in one shared conversation while routing each model request deliberately.

## Steps

1. UI-independent workspace.
2. Participant registration and lifecycle.
3. Default recipient for unaddressed user messages.
4. Direct user-to-agent addressing.
5. Addressed agent-to-agent delegation and responses.
6. Coordinator/specialist roles as policy over generic runtime agents.
7. Whole-domain/table/subsystem specialists.
8. Sender/recipient/correlation/causation/order metadata.
9. Visible agent-to-agent work when enabled.
10. Host-configurable addressing syntax.
11. Loop protection and collaboration budgets.
12. Optional workspace persistence/shared-memory policy.
13. WinForms chat surface and global agent selection.

Unaddressed user messages go only to the default recipient. Addressed messages go to their target unless an explicit policy invokes others. Broadcast is opt-in.

# Phase 1.0 — Collaboration + Workflows

## Collaboration

- First-class delegation/handoff operations.
- Shared/private workspace policies.
- Parallel specialist work with bounded collaboration budgets.
- Human intervention and approval points.
- Explicit lifecycle states.
- Explicit cross-agent memory sharing.
- Collaboration history, audit, and traceability.

## Workflows

- Task/job lifecycle.
- Planning, execution, and verification.
- Multi-step and branching workflows.
- Background execution and scheduling.
- Pause/resume and durable checkpoints.
- Event-triggered execution.
- Per-step timeout, cancellation, retry, approval, and budget policies.

# Later — Platform, Extensibility + Release

- Additional provider adapters and multimodal/embedding adapters.
- Provider/tool/UI-adapter/storage extensibility and failure isolation.
- External secret stores and rotation.
- Optional MCP/vector integrations where justified.
- DI/interoperability integrations.
- Simulation/test mode for external consumers.
- Diagnostics/trace viewer and complete Example coverage.
- Stable public contracts, migration/versioning, NuGet packaging, security/integration hardening, and migration guidance.
- `.NET 10` after the development environment and compatibility policy are ready.

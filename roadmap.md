# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

HAgent is a lightweight, provider-neutral agent platform for .NET desktop applications. The roadmap is dependency-ordered so simple applications stay small while advanced deployments can add memory, tools, UI context, data access, authorization, collaboration, and workflows.

## Current position

- 0.1 Foundation — complete
- 0.2 Runtime — complete
- 0.3 Memory + Context — foundation complete
- 0.4 Provider Capabilities + Response Normalization — foundation complete
- 0.5 Tools + Agent Loop — foundation complete; hardening remains
- 0.6 Safety + Permissions — foundational policy UI complete; broader authorization/approval remains
- 0.7 WinForms UI Context + Data Discovery — complete
- 0.8 Data Access + Authorization — next major milestone
- 0.9 Agent Scope + Chat — follows the data/security foundation

The initial supported tool categories are **BuiltIn, Application, Declarative, UI, SQL Server, and MySQL**. Extension tools are intentionally deferred to the later extensibility milestone.

## 0.1 Foundation

Provider/agent models, multi-provider agent relationships, OpenAI-compatible provider adapter, File/SQL Server/MySQL configuration foundations, protected secrets, management UI, model discovery, connection testing, deletion rules, HAgent.Example, global agent selector/output, and modular Example tests.

## 0.2 Runtime

Execution lifecycle, stable IDs, provider routing, ordered candidates, retries, timeout/cancellation, diagnostics, provider failure classification, system-prompt resolution, and low-RAM/no-GPU constraints.

## 0.3 Memory + Context

Persistent JSONL memory, scopes, typed Task/Event/Fact/Preference records, explicit recall/forget, lightweight relevance ranking, persistent conversations/sessions, context budgets, tokenizer-free estimation, conservative automatic memory, task/event memory, and episodic memory.

## 0.4 Capabilities + Response Normalization

Tri-state capabilities with evidence/confidence, capability cache, response normalization, reasoning separation and `<think>` handling, structured JSON, normalized tool calls, normalized usage, provider-error advice, streaming deltas, OpenAI-compatible SSE, cancellation, and live streaming verification.

## 0.5 Tool foundation verified

Tool definitions, six initial tool types, handler/definition separation, registry and delegate tools, JSON Schema validation, OpenAI-compatible tool transport, bounded multi-turn tool loop, persisted File tool definitions, Agent `ToolIds`, and successful live Groq tool-loop verification.

## 0.5 Tools + Agent Loop

Tool definitions, validation, persistence, per-agent assignment, provider tool transport, deterministic tool loops, and live Groq tool calling are implemented.

Remaining tool hardening:

- [ ] Per-session temporary tools.
- [ ] Built-in tool handlers.
- [ ] Declarative execution engine.
- [ ] Tool aliases/versioning.
- [ ] Tool timeout/cancellation/progress.
- [ ] Tool audit/history and budgets.
- [ ] Stronger loop detection and provider/tool capability negotiation.

## 0.6 Safety + Permissions

The permission model is a shared authorization concept, not just a WinForms checkbox collection.

- [x] General permission configuration UI.
- [ ] Read/write/invoke/export permissions across all tool categories.
- [ ] Host authorization callbacks.
- [ ] Human approval lifecycle.
- [ ] Input/output/tool guardrails.
- [ ] Budgets and observability.
- [ ] Sensitive-data redaction.

The first WinForms policy UI persists coarse permissions with safe defaults. Database-specific permissions and approval workflows remain separate work.

## 0.7 WinForms UI Context + Data Discovery — COMPLETE

The public concept is **UI Context / Control Adapters**. “Form serialization” is only one possible implementation technique inside a broader system.

Completed capabilities include:

- Form and arbitrary control-tree/UserControl attachment with stable root identity.
- Read-only inspection and bounded control/data reads.
- Semantic control discovery.
- Bound/native data-source discovery for `DataTable`, `DataView`, `BindingSource`, `IList`, arrays, and compatible collections.
- CurrencyManager/current-item/position/count relationship metadata.
- Control-to-source relationship discovery based on actual bindings.
- Convention-based application control adapters, including external `IHyperControl`-style controls.
- Live application-object attachment and bounded structural discovery.
- `maxDepth` and `maxCollectionItems` limits.
- Provider-neutral structured data-query contract: fields, scalar filters, sorting, and bounded paging without SQL or executable expressions.
- Local `HAgent.Example` verification of the complete 0.7 slice.

## 0.8 Data Access + Authorization

This is the next major platform milestone. The goal is to convert the verified discovery/query contracts into safe, real application and database data access.

### Application data

- [ ] Application-owned data adapter implementing `IDataQuerySource`.
- [ ] Schema/field allow-list independent of model requests.
- [ ] Query authorization by source/table/field/operation.
- [ ] Projection, query, export, and write permissions separated.
- [ ] Query limits, cancellation, timeout, and resource budgets.

### SQL Server / MySQL

- [ ] Restricted SQL Server query adapter using generated parameterized commands only.
- [ ] Restricted MySQL query adapter using generated parameterized commands only.
- [ ] Schema discovery restricted to explicitly authorized databases/schemas.
- [ ] No arbitrary SQL tool.
- [ ] Read-only database operations before write operations.
- [ ] Database operation audit metadata and correlation IDs.

### Live Example verification

When the SQL adapter is ready, `HAgent.Example` should expose temporary connection fields for an explicitly disposable/read-only test database:

```text
Server Name
User Name
Password
Database
```

These are runtime test inputs only. They must not be persisted as agent/tool configuration or written to normal Example output/logging. The Example should verify connection, schema allow-listing, structured query execution, bounded results, cancellation/timeout, and rejection of unauthorized fields/operations.

### Authorization and safety

- [ ] Host authorization callbacks.
- [ ] Explicit approval lifecycle for sensitive database operations.
- [ ] Sensitive-field redaction policies.
- [ ] No authorization inferred from UI binding, object provenance, table metadata, or model instructions.

## 0.9 UI Automation + Agent Scope + Chat

UI write/invoke behavior should begin only after the 0.8 authorization foundation is established.

- [ ] `ui.write_control`.
- [ ] `ui.invoke` / approved click.
- [ ] Move/resize/enable/disable operations.
- [ ] Batch operations.
- [ ] Dry-run/preview.
- [ ] Human approval.
- [ ] Per-control permissions.
- [ ] Undo/rollback hooks where hosts support them.
- [ ] Agent profile separated from runtime binding/lifetime.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat with global/form agent selector.
- [ ] Persistent conversations and conversation switching/search.
- [ ] Streaming UI and tool activity visualization.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Cross-form memory governed by explicit scope and authorization policy.

## Cross-platform UI direction

The same UI-context concepts should later be available through adapters for:

- HControl/BaseForm and custom controls.
- GDI-rendered objects and scenes.
- DirectX interactive objects.
- Unity components/scenes.
- Other interactive application surfaces.

These platform implementations remain outside `HAgent.Core`.

## Data representation rule

Always use the lightest representation that preserves the required information. Prefer bound/native sources, lazy adapters, projections, paging, and streaming. Avoid unnecessary copying/materialization. `DataTable` is valid when naturally present or actually useful, but it is never the mandatory representation.

## Example developer experience

Every meaningful Example feature should provide:

- editable input/message when the capability has meaningful user input;
- expected behavior and explanation;
- copyable C# reproduction snippet beside the input;
- global agent selection where an agent is involved;
- a global output area when the result can be shared;
- a self-contained setup snippet or clearly identified shared setup section.

SQL connection fields are a future live-integration Example feature, not part of the provider-neutral Core contract.

## 0.3 Advanced Memory

- [ ] Richer automatic-memory inference without saving ordinary conversation by default.
- [ ] Memory update/upsert semantics.
- [ ] Retention/expiration policies.
- [ ] Larger-store indexing improvements.
- [ ] Context compaction/summarization.
- [ ] SQL Server/MySQL memory stores.
- [ ] Conversation listing/search/metadata management.
- [ ] Optional vector-memory adapter.
- [ ] Remote embedding providers without local GPU or RAM-heavy resident models.

## 0.9 Agent Scope + Chat

- [ ] Separate agent profile from runtime binding/lifetime.
- [ ] Application/global, form, session, task, and ephemeral scopes.
- [ ] User ↔ agent chat window with global/form agent selector.
- [ ] Conversation switching/search/metadata/persistence.
- [ ] Streaming UI and tool activity.
- [ ] Reasoning visibility policy.
- [ ] Cancel/stop and simultaneous conversations.
- [ ] Safe handling of deleted/disabled agents.
- [ ] Cross-form memory governed by explicit scope and policy.

## 0.10 Orchestration + Collaboration

- [ ] Agents-as-tools.
- [ ] Handoffs/delegation.
- [ ] Agent-to-agent messaging board, channels, direct messages and broadcasts.
- [ ] Shared workspace context.
- [ ] Routing and collaboration budgets.
- [ ] Maximum hops/depth and loop detection.
- [ ] Shared/private memory policies.
- [ ] Parallel execution and human intervention.
- [ ] Active/disabled/retired/deleted lifecycle.

## 0.6 Safety + Guardrails + Approval + Observability

- [ ] Input/output/tool guardrails.
- [ ] Termination/tripwire rules.
- [ ] Read/write/invoke/export permissions.
- [ ] Host authorization callbacks.
- [ ] Human approval and approval lifecycle/audit.
- [ ] Execution/provider/tool/memory budgets.
- [ ] Tracing, spans, timings, correlation IDs.
- [ ] Sensitive-data redaction and no-secret diagnostics.

## 0.10 Tasks + Workflows + Autonomy

- [ ] Explicit task/job model and lifecycle.
- [ ] Planning, execution, verification.
- [ ] Multi-step workflows, branching, background execution, scheduling.
- [ ] Pause/resume, durable checkpoints, restart recovery.
- [ ] Event-triggered agents.
- [ ] Per-step retry, approval, cancellation, leases.
- [ ] Workflow observability and autonomy budgets.

## 0.11 Provider ecosystem

- [ ] Azure OpenAI.
- [ ] Anthropic.
- [ ] Google/Gemini.
- [ ] Ollama.
- [ ] LM Studio.
- [ ] Custom HTTP providers.
- [ ] Multimodal and embedding providers.
- [ ] Provider-specific capability negotiation.
- [ ] Streaming implementations.
- [ ] Provider contract harness.

## 0.12 Extensibility + storage ecosystem

- [ ] Provider/tool/UI-adapter/storage DLL loading.
- [ ] Extension validation and failure isolation.
- [ ] Conversation and memory persistence across File/SQL Server/MySQL.
- [ ] Optional vector/MCP companion integrations.
- [ ] External secret stores and rotation.
- [ ] Configuration profiles/workspaces.
- [ ] Multi-user authorization hooks.
- [ ] Audit logging.

Extension tools are intentionally deferred until this milestone.

## 0.13 Developer platform

- [ ] Optional DI integration.
- [ ] Optional `Microsoft.Extensions.AI` integration.
- [ ] Provider SDK.
- [ ] Tool SDK.
- [ ] UI-context/control-adapter SDK.
- [ ] Simulation/test mode.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public capabilities.

## 1.0

- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packages and signed releases.
- [ ] Comprehensive integration/security/provider/tool/memory tests.
- [ ] Documentation for providers, tools, memory, UI automation and workflows.
- [ ] .NET 10 after migration to compatible Visual Studio.

# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

## HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. It describes phases and dependencies; stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage foundations substantially implemented; Skills/Wiki and remaining internal-repository parity are deferred
- 0.9 — Runtime Agent Instances foundations complete and locally verified
- 0.10 — Workspaces, Routing + Chat **active**
- 0.95 — Generic External Host Integration **planned cross-cutting hardening**
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

Phase 0.95 is a cross-cutting runtime/API hardening phase. It can be implemented alongside unfinished higher-level roadmap work, but its completion is required before HAgent can claim a complete generic external-host integration surface. It closes the remaining gap between the runtime architecture and a reusable LLM boundary: arbitrary host input/context, host correlation, structured output contracts, execution terminality, tool identity propagation, and strong runtime isolation. It does not introduce any host-specific domain dependency. Scheduling, authorization, host state, persistence, and side effects remain host-owned.

The roadmap therefore distinguishes feature phases from generic runtime hardening. Higher-level features may continue, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through the public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.

## Foundations — 0.1 through 0.7

This file records the completed foundation path and the remaining hardening work that grows directly out of those phases. It is the roadmap's implementation history; the master plan does not repeat these checklists.

## 0.1 Foundation — complete

Implemented:

- Multi-target .NET Framework 4.8.1 and .NET 9 where supported.
- Provider and agent configuration with multi-provider relationships.
- OpenAI-compatible provider adapter.
- File, SQL Server, and MySQL persistence foundations.
- Protected local secrets.
- Provider/agent/tool management UI.
- Model discovery and connection testing.
- Dependency-aware deletion behavior.
- `HAgent.Example` integration host and modular examples.
- Global agent selection and output handling.

## 0.2 Runtime — complete

Implemented:

- Execution lifecycle and stable execution IDs.
- Provider routing and ordered candidates.
- Retries, timeout, and cancellation.
- Diagnostics and structured failure categories.
- Actionable provider/model/account error reporting.
- System-prompt resolution.
- Execution snapshots so active work is isolated from later configuration changes.
- Low-RAM/no-GPU design constraints.

## 0.3 Memory + Context — foundation complete

Implemented:

- Persistent JSONL memory and bounded search.
- Explicit remember/recall/forget.
- Memory scopes.
- Typed Fact/Preference/Task/Event records.
- Persistent conversations and sessions.
- Context budgets and tokenizer-free estimation.
- Conservative automatic memory.
- Lightweight relevance ranking.
- Episodic memory with provenance.

Deferred maturation:

- Memory upsert/update semantics.
- Retention/expiration policies.
- Context compaction/summarization.
- Larger-store indexing improvements.
- SQL Server/MySQL memory stores.
- Conversation listing/search/metadata management.
- Optional vector-memory adapters and remote embeddings.

## 0.4 Provider Capabilities + Response Normalization — foundation complete

Implemented:

- Tri-state capability reporting with evidence/confidence.
- Capability caching.
- Normalized text, reasoning, raw text, structured output, tool calls, usage, and provider metadata.
- Separate reasoning handling and `<think>` diagnostics.
- Provider error classification/advice.
- Streaming delta contract.
- OpenAI-compatible SSE streaming.
- Streaming cancellation.
- Live streaming verification.

## 0.5 Tools + Agent Loop — foundation complete

Implemented:

- Six initial tool categories: BuiltIn, Application, Declarative, UI, SqlServer, MySql.
- Tool definition/handler separation.
- Tool registry and application-registered handlers.
- JSON Schema validation.
- Provider tool-definition transport.
- Bounded multi-turn tool loops.
- Persisted tool definitions.
- Per-agent tool assignment.
- Live Groq tool-loop verification.

Hardening remains:

- Per-session temporary tools.
- Built-in tool handlers.
- Declarative execution engine.
- Tool aliases/versioning.
- Tool timeout/cancellation/progress.
- Tool audit/history and budgets.
- Stronger loop detection and capability negotiation.

## 0.6 Safety + Permissions — foundation complete

Implemented:

- General permission configuration UI.
- Persisted current WinForms permission policy.
- Safe defaults for automatic discovery/read/write/invoke behavior.

Remaining platform safety work:

- Read/write/invoke/export authorization across tool categories.
- Host authorization callbacks.
- Human approval lifecycle.
- Input/output/tool guardrails.
- Execution/tool/memory budgets.
- Tracing and observability.
- Sensitive-data redaction.

## 0.7 WinForms UI Context + Data Discovery — complete

Implemented and locally verified:

- Form and arbitrary control-tree/UserControl attachment with stable root identity.
- Read-only inspection and control reads.
- Semantic control discovery.
- Bound/native data-source discovery for DataTable, DataView, BindingSource, IList, arrays, and compatible collections.
- CurrencyManager/current-item/position/count metadata.
- Control-to-source relationships based on actual bindings/source identity.
- Convention-based control adapters, including external `IHyperControl`-style controls using members such as `DbFieldName`, `GetValue()`, and `SetValue(object)`.
- Live application-object attachment and bounded structural inspection.
- `maxDepth` and `maxCollectionItems` resource limits.
- Provider-neutral structured data projection and query contracts with explicit fields, scalar filters, sorting, and bounded paging.
- Example verification for UI Context, UserControl, native IList, data relationships, custom control adaptation, application-object context, and query semantics.

## Foundation exit

These phases establish the base on which the remaining roadmap is built. The next work is not another UI discovery feature; it is safe real data access followed by the runtime-agent model required for multi-agent hosts and HWorld.

## Phase 0.8 — Data Access + Authorization + Internal Storage

## Goal
Provide bounded structured data contracts and establish HAgent-owned persistence across File, SQL Server, and MySQL backends without ever using HAgent storage as access to a host application's business database.

## Steps

1. [x] Application-owned structured-query contract and authoritative field schema.
2. [x] Separate data-operation permissions and request-specific host authorization contracts.
3. [x] Query/result limits, cancellation, timeout, and resource budgets.
4. [x] HAgent internal storage backend configuration for File, SQL Server, and MySQL.
5. [x] Application-specific File storage layout.
6. [x] SQL Server HAgent database creation and initial schema bootstrap.
7. [x] MySQL HAgent database creation and initial schema bootstrap.
8. [ ] Wire providers, agents, tools, memory, conversations, skills, wiki/content, and runtime repositories to the selected backend.
9. [x] Versioned schema migrations beyond the initial bootstrap version.
10. [ ] Read-only HAgent internal data tools, audit/correlation metadata, and live Example verification before any internal writes beyond repository persistence.

The read-only foundation now includes bounded provider/agent/tool inventory, memory inspection with scope/owner isolation, explicit-session conversation inspection, and execution-audit inspection. Execution audit persistence is available through File, SQL Server, and MySQL using a secret-safe payload-free record. Automatic terminal audit capture and retention policy remain open work.

## Internal database naming

The default HAgent database name is derived from the host application name using `<application-name>-ai`, for example `nap-ai` or `hworld-ai`. The database name is controlled by HAgent storage naming rules and is not a user-editable field in the Storage UI.

## File backend

File storage is application-specific and rooted beneath the host executable directory in `HAgentData`, with dedicated areas for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime, cache, logs, and audit data.

## Database backends

SQL Server and MySQL storage providers receive server name and username as persisted configuration metadata and a password through the secret/runtime boundary. They connect to the server, create the HAgent-owned database if it does not exist, and initialize only HAgent-owned tables. Schema version metadata supports deterministic migrations.

The relational bootstrappers use `HAgentSchemaInfo` as the migration boundary. They establish a baseline schema version, read the persisted version, apply ordered provider-specific migrations until the current version is reached, and update the version only after each migration succeeds. Unknown future schema versions are rejected rather than silently skipped.

Current relational schema versions are SQL Server `3` and MySQL `4`. SQL Server v2→v3 and MySQL v3→v4 establish `HAgentExecutionAudits` and its retrieval indexes. The MySQL bootstrap executes schema statements and migrations as separate commands so MariaDB deployments do not depend on multi-statement command execution.

The internal database schema includes provider, agent, tool, memory, conversation, execution-audit, skill, wiki document/chunk, and schema metadata tables. It must never inspect, alter, or query unrelated host application tables.

## Audit foundation

`AgentExecution` carries an execution-level correlation ID. `AgentExecutionAuditRecord` projects only execution/correlation identity, agent/provider/model metadata, lifecycle timing, state, and classified failure metadata. Prompts, responses, provider secrets, secret IDs, connection strings, raw exceptions, and other payloads are excluded.

`IExecutionAuditStore` provides bounded append/search persistence. File uses an HAgent-owned `audit/executions.jsonl` file; SQL Server and MySQL use the HAgent-owned `HAgentExecutionAudits` table. `HAgentInternalExecutionAuditTool` exposes bounded read-only inspection and constrains explicit agent filtering to the requesting agent identity when one is present.

Automatic runtime capture of terminal executions and retention/compaction are intentionally separate follow-up slices.

## Live Example

The Example storage verification will exercise File, SQL Server, and MySQL initialization where the corresponding backend is configured. It will verify database creation when absent, idempotent initialization when present, schema version reporting, persistence through the HAgent repositories, execution-audit round trips, and strict separation from host application data. Live backend switching is expected to work without restarting when the host supports storage rebinding.

Connection values must never become persisted agent/tool configuration or normal logs.

## Boundaries

- No raw SQL from model input.
- No implicit access to the host application's business database.
- HAgent storage providers are internal persistence providers, not host database adapters.
- Database passwords remain in the secret/runtime boundary.
- UI discovery, object provenance, and model instructions do not grant database authorization.

## Exit criterion

A host can select an HAgent-owned storage backend, initialize or upgrade it deterministically, and use HAgent repositories against it without HAgent gaining access to the host application's business database.

## Phase 0.9 — Runtime Agent Instances

## Goal
Make live agents first-class runtime objects separate from reusable agent profiles.

## Steps

1. [x] Introduce a provider-neutral runtime agent instance with its own stable instance ID and profile reference.
2. [x] Define explicit provider-neutral runtime scopes.
3. [x] Allow runtime-specific context and provider/model overrides without mutating stored profiles.
4. [x] Give each runtime instance an independent memory owner.
5. [x] Support multiple runtime instances executing concurrently.
6. [x] Expose asynchronous scheduling, cancellation, timeout, correlation, and stale-result protection foundations.
7. [x] Define explicit active/retired/shutdown lifecycle behavior.
8. [x] Keep dynamically created agents out of persistent configuration by default.
9. [x] Add optional runtime-state persistence for recovery, collaboration, or multi-process deployments.
10. [x] Verify the runtime contract with deterministic Example coverage.
11. [ ] Complete generic external-host execution boundary hardening in Phase 0.95.

## Runtime rule

One configured profile can produce many live instances. Runtime roles are host policy over the same generic runtime model, not separate agent classes.

Runtime-only provider, model, generation, system-prompt, and context overrides are applied to execution snapshots created from the persistent profile. They never mutate the stored profile. Runtime configuration must remain distinct from per-execution host input.

Each runtime instance owns memory through its `MemoryOwnerId`, keeping private agent-scoped memory separate across instances created from the same profile.

Each instance-bound execution receives a monotonically increasing instance revision. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to reject late results after a newer execution starts or the instance is retired. The generic execution hardening phase additionally ensures late provider completion cannot overwrite a terminal execution outcome.

Retirement and shutdown are separate lifecycle operations. Retirement prevents new executions and invalidates result authority while allowing already-running work to finish or be cancelled by the host. Shutdown is terminal, prevents new executions, invalidates result authority, and requests cancellation of outstanding instance-bound executions.

`IAgentExecutionScheduler` and the default `AgentExecutionScheduler` provide an optional host-controlled admission boundary with a configurable concurrency limit. The scheduler does not own host timing or replace runtime execution semantics.

The runtime contract is covered by deterministic Example verification for runtime instance identity, runtime-only overrides, independent memory ownership, concurrent execution, stale-result protection, scheduling, shutdown lifecycle, and optional runtime-state persistence. No external provider is required for those tests.

Dynamically created runtime instances are not persisted as `AiAgent` profile records by default. Optional runtime-state persistence is a separate host-controlled capability using the HAgent-owned runtime-state store; persisted state contains generic identity/scope/lifecycle metadata only and does not persist secrets or provider credentials.

## External-host relationship

Phase 0.9 establishes the runtime-instance foundation. Phase 0.95 completes the generic execution boundary required for external hosts: arbitrary host input/context, host correlation, structured output contracts, terminal execution semantics, and tool identity propagation. No external consumer is a dependency or special-case adapter in Core.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, memory, or execution-state collisions. Phase 0.95 extends this foundation into the complete generic LLM host integration contract.

## Phase 0.95 — Generic External Host Integration

## Goal
Make HAgent a complete generic LLM cognition/execution boundary for different project types without introducing application-specific domain concepts into Core.

## Requirements

1. [ ] Introduce a canonical generic `AgentExecutionRequest` that can carry host-supplied input/context without requiring plain string message as the fundamental execution model.
2. [ ] Keep plain string message execution as a convenience overload over the generic request.
3. [ ] Add host-supplied correlation identity distinct from `AgentExecution.Id` and `AgentRuntimeInstance.InstanceId`.
4. [ ] Propagate execution, runtime-instance, and host correlation identity into tool execution context.
5. [ ] Define a generic structured-output request contract with host-owned schema input.
6. [ ] Request structured output through capable provider adapters rather than merely detecting valid JSON after generation.
7. [ ] Validate returned structured output against the requested schema and expose normalized validation/result metadata.
8. [ ] Ensure provider capability reporting distinguishes structured output `Supported`, `Unsupported`, and `Unknown`.
9. [ ] Make execution terminal-state transitions race-safe so late provider completion cannot overwrite cancellation, timeout, retirement, shutdown, or another terminal outcome.
10. [ ] Preserve independent runtime-instance identity, overrides, execution state, and private memory ownership when multiple instances originate from one profile.
11. [ ] Snapshot runtime overrides at the instance boundary so mutable caller-owned override objects cannot create cross-instance state coupling.
12. [ ] Keep host scheduling external; HAgent may provide focused admission-control primitives only.
13. [ ] Preserve host ownership of domain state, persistence, authorization, scheduling policy, and side effects.
14. [ ] Add deterministic Example verification for generic execution input, host correlation, structured output, late completion protection, tool identity propagation, concurrent runtime instances, and memory isolation.

## API direction

The intended public integration shape is:

```text
Host
  -> AgentExecutionRequest
  -> AgentRuntimeInstance
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
```

The request should remain generic. HAgent must not define host-domain schemas, event types, command types, domain objects, or lifecycle policy.

## Runtime invariant

One reusable profile may produce many long-lived runtime instances. Every runtime instance remains independently addressable and owns its own runtime lifecycle, execution revision, override snapshot, shutdown signaling, and private memory ownership.

## Completion invariant

An execution that has reached a terminal outcome cannot later publish a conflicting outcome because a provider completed late.

## Structured-output invariant

A structured-output response is valid only when the requested contract is successfully honored and validated. Arbitrary JSON text is not sufficient evidence that the contract was satisfied.

## Exit criterion

A generic external host can provide arbitrary bounded context, correlate requests, request validated structured output, expose host-owned tools, run multiple long-lived runtime instances concurrently, control cancellation/timeout/lifecycle, and safely ignore stale or late results without introducing host-specific concepts into HAgent.Core.

## Phase 0.10 — Workspaces, Routing + Chat

## Goal
Provide an optional shared conversation where a user and multiple runtime agents can visibly work together while every model request is routed deliberately.

## Steps

1. [ ] Introduce a workspace abstraction independent of WinForms.
2. [ ] Register users and runtime-agent participants with explicit lifecycle state.
3. [ ] Define one workspace default recipient for unaddressed user messages.
4. [ ] Define direct user-to-agent addressing.
5. [ ] Define addressed agent-to-agent delegation and responses.
6. [ ] Make coordinator/specialist behavior a role/policy over generic runtime agents.
7. [ ] Allow specialists to represent whole domains, tables, subsystems, or other host responsibilities.
8. [ ] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
9. [ ] Make agent-to-agent work visible in the workspace when the host enables it.
10. [ ] Add configurable addressing syntax at the host/UI layer.
11. [ ] Add loop protection and collaboration budgets.
12. [ ] Add optional workspace persistence and shared-memory policy.
13. [ ] Add the WinForms chat surface and global agent selection.

### Current foundation

The provider-neutral foundation now contains `AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, and `WorkspaceRouter`. Participants are either users or runtime agents and have explicit Active/Suspended/Retired state. An active default recipient may be defined for unaddressed user messages. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

The `HAgent.Example` application contains deterministic `WORKSPACE ROUTING` verification. The implementation is pending local verification before Step 1 is marked complete.

## Routing rules

- Unaddressed user message: send only to the workspace default recipient.
- Explicitly addressed user message: send to that participant.
- Agent delegation: send only to the addressed participant unless an explicit policy invokes others.
- Broadcast: explicit opt-in operation, never the default.

The user can observe the coordinator ask a specialist to work and the specialist return its result before the coordinator answers.

## Specialist context

A contextual specialist may be created automatically from a configured profile. Its runtime prompt/context can contain UI, data, application-object, task, or other host context according to authorization policy. The specialist can report what it knows, inferred information, unknowns, and authorization limits honestly.

## HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public runtime/workspace APIs. HAgent does not add an HWorld-specific dependency, adapter, world type, physics, simulation scheduling, or action authority.

## Exit criterion

A host can run a visible multi-agent conversation in which messages reach only their intended recipients, coordinator/specialist delegation works, and workspace execution remains bounded and traceable.

## Phase 1.0 — Collaboration + Workflows

## Goal
Turn basic workspace messaging into reliable multi-agent collaboration and then into bounded task/workflow execution.

## Collaboration steps

1. First-class delegation/handoff operations.
2. Shared/private workspace context policies.
3. Parallel specialist work with bounded collaboration budgets.
4. Human intervention and approval points.
5. Explicit runtime/participant lifecycle states.
6. Cross-agent memory sharing only through explicit policy.
7. Collaboration history, audit, and traceability.

## Workflow steps

1. Task/job model and lifecycle.
2. Planning, execution, and verification stages.
3. Multi-step and branching workflows.
4. Background execution and scheduling.
5. Pause/resume and durable checkpoints.
6. Event-triggered execution.
7. Per-step timeout, cancellation, retry, approval, and budget policies.

## Boundary

These are generic orchestration facilities. HAgent does not become the authority for business rules, simulation state, or host-side side effects.

## Exit criterion

A host can coordinate multiple agents and long-running work with bounded execution, explicit authority, resumable state where required, and observable collaboration.

## Later — Platform, Extensibility + Release

These capabilities follow the core runtime, data, and collaboration milestones. They should not block the primary host-integration path.

## Provider ecosystem

- [ ] Additional provider adapters such as Azure OpenAI, Anthropic, Google/Gemini, Ollama, LM Studio, and custom HTTP providers where justified.
- [ ] Multimodal and embedding adapters.
- [ ] Provider capability/contract harness.

## Extensibility

- [ ] Provider, tool, UI-adapter, and storage extension model.
- [ ] Extension validation and failure isolation.
- [ ] External secret stores and secret rotation.
- [ ] Optional MCP/vector integrations where they fit the lightweight architecture.

## Developer platform

- [ ] Optional DI/interoperability integrations.
- [ ] Simulation/test mode for external consumers such as HWorld.
- [ ] Diagnostics/trace viewer.
- [ ] Complete Example coverage for meaningful public APIs.
- [ ] SDK guidance for provider, tool, UI-context, and host integrations.

## Release hardening

- [ ] Stable public contracts and compatibility policy.
- [ ] Storage migration/versioning.
- [ ] NuGet packaging and release process.
- [ ] Security/provider/tool/memory integration coverage.
- [ ] Documentation and migration guidance.

`.NET 10` remains a future target after the development environment and compatibility policy are ready.

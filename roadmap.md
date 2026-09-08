# HAgent Roadmap

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/roadmap`.

## HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage **and first-class resource foundations** — substantially implemented
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration **complete and verified on .NET Framework 4.8.1 and .NET 9**
- 0.951 — Identity, Tenancy + User Context — **completed and verified**
- 0.952 — First-Class Event Subsystem — completed and verified
- 0.953 — Unified Policy Engine — completed for its verified runtime/persistence/resource/learning-policy foundation
- 0.954 — Prompt + Instruction Governance — **completed and verified on .NET Framework 4.8.1 and .NET 9**
- 0.955 — Context Engineering — **current ordered milestone**
- 0.956 — Observability + Distributed Tracing — planned architectural foundation
- 0.957 — Evaluation + Quality Measurement — planned architectural foundation
- 0.9575 — Knowledge, Skills, Memory Governance + Learning — planned mature resource/governance phase
- 0.958 — Agent Lifecycle + Health Management — planned architectural foundation
- 0.9591 — Goal/Plan Persistence + Recovery — **ordered before 0.959**; planned foundation
- 0.959 — Human-in-the-Loop + Intervention — planned architectural foundation; ahead-of-roadmap execution intervention implementation exists
- 0.9592 — Provider Ecosystem + Adapter Lifecycle — planned provider-platform foundation
- 0.96.x — Configuration, Storage + Portability Evolution — cross-cutting foundation for 0.96/0.97
- 0.96 — Capability-Aware Execution — planned major execution foundation
- 0.97 — Persistent Cognitive Runtime — planned long-lived cognitive layer
- 0.10 — Workspaces, Routing + Chat — paused until the generic runtime/capability/cognitive foundations are sufficient
- 1.0 — Collaboration + Workflows
- Later — extensibility, developer platform, release hardening, and other ecosystem work

## First-class resource architecture

Knowledge, Skills, Memory, and Learning are now treated as a first-class architectural concern from the foundation upward rather than as a late product feature.

The roadmap deliberately separates two layers:

```text
0.8 Resource Foundations
    identity / scope / ownership metadata
    provenance / version / lifecycle metadata
    Skill definitions and references
    Knowledge / Wiki resource contracts and retrieval boundaries
    Memory family/type foundations
    typed learning-candidate foundations
    HAgent-owned persistence substrate

0.9575 Mature Resource Governance + Learning
    policy and authorization
    capability inheritance and runtime overrides
    effective execution resource snapshots
    bounded retrieval / retention governance
    Knowledge / Skill management
    Learning modes and learning policy
    candidate validation / evaluation / approval / promotion
    resource/version conflict handling
```

This separation is intentional. HAgent establishes canonical resource contracts early enough that Context, Instruction Governance, Runtime, Evaluation, and Persistent Cognition can consume them directly. Mature governance is completed only after the identity, policy, instruction, context, and evaluation boundaries required to govern those resources exist.

Learning is not treated as a synonym for memory or as a late model feature. It is a controlled lifecycle from experience to typed candidate to validation/policy/approval and finally promotion into an authoritative resource or scoped state.

The four concepts remain distinct:

```text
Skills    = reusable executable capabilities/procedures
Knowledge = reusable retrievable information
Memory    = scoped experience/state
Learning  = governed transformation of experience into candidates/promotions
```

No phase may introduce a parallel resource model merely because the mature governance phase has not yet been completed. Existing resource primitives may be consumed by earlier phases, while access, promotion, and authoritative mutation remain subject to the governance boundaries defined later.

## Foundational sequence

The current ordered foundations are:

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Event subsystem
        ↓
0.953 Unified Policy Engine
        ↓
0.954 Prompt / Instruction Governance — verified
        ↓
0.955 Context Engineering — current
        ↓
0.956 Observability / Tracing
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 Knowledge / Skills / Memory Governance + Learning
        ↓
0.958 Agent Lifecycle / Health
        ↓
0.9591 Goal / Plan Persistence / Recovery
        ↓
0.959 Human-in-the-Loop / Intervention
        ↓
0.9592 Provider Ecosystem / Adapter Lifecycle
        ↓
0.96.x Configuration / Storage / Portability Evolution
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
```

The placement of 0.9591 before 0.959 is deliberate: durable goal/plan revisions, checkpoints, and recovery state provide the persistent authority that later goal/plan-step intervention can govern. Execution-level intervention remains independently valid.

The pre-0.96 foundations define reusable identity, events, policy, resource foundations, instruction trust, context assembly, tracing, evaluation, resource governance, lifecycle, durable goal/plan recovery, human intervention, provider adapter boundaries, and configuration/storage evolution that later execution and cognition layers should consume rather than reinvent.

## Resource relationship to later cognition

Phase 0.97 Persistent Cognitive Runtime consumes Memory, Knowledge, and Skills as first-class resources and may consume existing resource contracts before every 0.9575 management surface is complete. It must not create a second memory/knowledge/skill architecture or bypass resource governance.

Persistent cognition can generate experiences and learning signals, but learned changes remain subject to typed candidates, provenance, evaluation, policy, authorization, versioning, and explicit promotion. The cognitive kernel itself remains independently versioned and must never be silently rewritten by model output or learning.

## Configuration and storage relationship

The configuration/storage evolution phase remains a cross-cutting foundation before 0.96 because capability-aware execution and persistent cognition depend on provider/model/target separation, encrypted provider credentials, global configuration, resource relationships, shared-database behavior, snapshot invalidation, and portable configuration contracts.

## Roadmap rules

The roadmap is dependency-driven rather than a permanent product-feature lock. A phase may be reordered when new architectural understanding reveals a genuine dependency change; such changes should update the authoritative roadmap and current-state documents together. Existing ahead-of-roadmap implementation remains implementation evidence, not milestone completion, until its ordered phase and full requirements are verified.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.

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

## Phase 0.8 — Data Access + Authorization + Internal Storage + Resource Foundations

## Goal

Provide bounded structured data contracts, HAgent-owned persistence, and the canonical resource foundations required by the first-class Knowledge, Skills, Memory, and Learning architecture across File, SQL Server, and MySQL backends without ever using HAgent storage as access to a host application's business database.

This phase establishes the **resource substrate**. Mature resource governance, capability inheritance, runtime overrides, learning review, promotion, and management UI belong to the later Phase 0.9575 and must build on these foundations rather than introduce a second resource architecture.

## Steps

1. [x] Application-owned structured-query contract and authoritative field schema.
2. [x] Separate data-operation permissions and request-specific host authorization contracts.
3. [x] Query/result limits, cancellation, timeout, and resource budgets.
4. [x] HAgent internal storage backend configuration for File, SQL Server, and MySQL.
5. [x] Application-specific File storage layout.
6. [x] SQL Server HAgent database creation and initial schema bootstrap.
7. [x] MySQL HAgent database creation and initial schema bootstrap.
8. [ ] Wire providers, agents, tools, memory, conversations, skills, wiki/content, learning candidates, and runtime repositories to the selected backend.
9. [x] Versioned schema migrations beyond the initial bootstrap version.
10. [ ] Read-only HAgent internal data tools, audit/correlation metadata, and live Example verification before any internal writes beyond repository persistence.

## Resource foundation

11. [ ] Establish one provider-neutral resource identity contract that can represent Skills, Knowledge/Wiki resources, Memory families/types, Learning candidates, and future resource types without adding hard-coded resource properties to the agent model.
12. [ ] Establish explicit resource scope/ownership metadata compatible with the canonical identity model: Global, Tenant, Domain, User, Agent, Runtime, and Execution where applicable.
13. [ ] Establish resource provenance/source metadata and lifecycle/status/version metadata as shared foundation concepts.
14. [ ] Define the stable distinction between Skills, Knowledge/Wiki, Memory, and Learning candidates; do not collapse them into one persisted object model.
15. [ ] Establish stable/versioned Skill definitions and references while keeping executable handlers outside persistence.
16. [ ] Establish provider-neutral Knowledge/Wiki resource/source contracts and bounded retrieval semantics independent of keyword/vector/index implementation.
17. [ ] Normalize Memory foundations around working, episodic, semantic, procedural, and future extensible types while keeping memory ownership separate from physical storage.
18. [ ] Establish typed learning-candidate contracts and provenance fields needed for later governed promotion without making the model authoritative.
19. [ ] Preserve immutable snapshot compatibility: resource definitions and references must be safe to capture into active execution snapshots without later configuration edits mutating running executions.
20. [ ] Ensure the resource foundation remains usable without GPU hardware, vector databases, embeddings, or large resident indexes.

## Deferred historical completion obligations

The following unfinished 0.8 items remain intentionally deferred. They are **not prerequisites for Phase 0.954** and must not be pulled into the active 0.954 implementation merely because they originated in this phase. They are retained here as durable completion obligations and are completed when their consuming architecture is ready.

### Item 8 — Repository/backend wiring

**Current state:** repository coverage exists for several HAgent-owned areas, but the original 0.8 acceptance criterion is broader than the currently verified implementation.

**Not a blocker for:** 0.954 Prompt / Instruction Governance, 0.955 Context Engineering, 0.956 Observability / Tracing, or 0.957 Evaluation / Quality Measurement.

**Consumed by:**
- **0.9575 Knowledge, Skills, Memory Governance + Learning** — complete persistence for knowledge-resource relationships, skill versions/relationships, learning candidates/review state, capability assignments/overrides, and extensible memory-type policy.
- **0.96.x Configuration, Storage + Portability Evolution** — aligned persistence of resources and relationships across File, SQL Server, and MySQL, plus portable configuration.
- **0.97 Persistent Cognitive Runtime** — durable cognitive/resource state must use the canonical storage/resource architecture rather than introducing a parallel persistence model.

**Completion rule:** close this obligation as part of the consuming storage/resource milestone, after the relevant repository contract, all supported backends, migrations, snapshot semantics, and Example verification are complete.

### Item 10 — Internal read-only data tools / audit / correlation / Example verification

**Current state:** bounded provider/agent/tool inventory, memory inspection with scope/owner isolation, explicit-session conversation inspection, execution-audit inspection, execution correlation IDs, and payload-safe audit persistence are already substantially implemented. The original item remains open because its acceptance criterion also covers the complete read-only internal tooling and live verification surface.

**Not a blocker for:** 0.954 Prompt / Instruction Governance or the other pre-0.956 foundations.

**Consumed primarily by:**
- **0.956 Observability / Distributed Tracing** — complete diagnostics, correlation, trace inspection, redaction, and deterministic Example verification.
- **0.9575 Knowledge, Skills, Memory Governance + Learning** — resource/learning inspection and governed management surfaces can consume the bounded internal inspection/audit foundation.
- Later management/diagnostic phases may extend these read-only surfaces without bypassing the same authorization and redaction boundaries.

**Completion rule:** close this obligation when the consuming observability/management work has complete read-only inspection, correlation/audit coverage, secret-safe diagnostics, supported backend behavior, and live Example verification.

These obligations are roadmap dependencies, not active-work items. The authoritative active-work document should contain only the currently selected implementation slice.

The read-only foundation now includes bounded provider/agent/tool inventory, memory inspection with scope/owner isolation, explicit-session conversation inspection, and execution-audit inspection. Execution audit persistence is available through File, SQL Server, and MySQL using a secret-safe payload-free record.

## Internal database naming

The default HAgent database name is derived from the host application name using `<application-name>-ai`, for example `nap-ai` or `hworld-ai`. The database name is controlled by HAgent storage naming rules and is not a user-editable field in the Storage UI.

## File backend

File storage is application-specific and rooted beneath the host executable directory in `HAgentData`, with dedicated areas for configuration, providers, agents, tools, skills, memory, conversations, wiki, runtime, cache, logs, and audit data. Resource foundation records remain HAgent-owned and must use additive schema/layout changes rather than host databases.

## Database backends

SQL Server and MySQL storage providers receive server name and username as persisted configuration metadata and a password through the secret/runtime boundary. They connect to the server, create the HAgent-owned database if it does not exist, and initialize only HAgent-owned tables. Schema version metadata supports deterministic migrations.

The internal database schema includes provider, agent, tool, memory, conversation, execution-audit, skill, wiki document/chunk, and schema metadata areas. Mature resource governance in Phase 0.9575 will add or complete persistence required for knowledge-resource relationships, skill versions/relationships, learning candidates/review state, capability assignments/overrides, and extensible memory-type policy where those foundations remain outstanding.

## Audit foundation

`AgentExecution` carries an execution-level correlation ID. `AgentExecutionAuditRecord` projects only execution/correlation identity, agent/provider/model metadata, lifecycle timing, state, and classified failure metadata. Prompts, responses, provider secrets, secret IDs, connection strings, raw exceptions, and other payloads are excluded.

`IExecutionAuditStore` provides bounded append/search persistence. File uses an HAgent-owned `audit/executions.jsonl` file; SQL Server and MySQL use the HAgent-owned `HAgentExecutionAudits` table.

## Live Example

The Example storage verification will exercise File, SQL Server, and MySQL initialization where the corresponding backend is configured. It will verify database creation when absent, idempotent initialization when present, schema version reporting, persistence through the HAgent repositories, execution-audit round trips, and strict separation from host application data. Resource-foundation Example coverage must also verify identity/scope/provenance/versioning boundaries for the canonical resource contracts as they become implemented.

## Boundaries

- No raw SQL from model input.
- No implicit access to the host application's business database.
- HAgent storage providers are internal persistence providers, not host database adapters.
- Database passwords remain in the secret/runtime boundary.
- UI discovery, object provenance, and model instructions do not grant database authorization.
- Knowledge, Skills, Memory, and Learning data remain HAgent-owned resources and do not grant host-business-data access.
- This phase establishes resource identity and persistence foundations; mature capability governance and learning promotion are deliberately later concerns.

## Exit criterion

A host can select an HAgent-owned storage backend, initialize or upgrade it deterministically, use HAgent repositories against it, and rely on canonical provider-neutral foundations for Knowledge, Skills, Memory, Learning candidates, scope, provenance, versioning, and future resource types without HAgent gaining access to the host application's business database. Deferred historical completion obligations must be consumed and verified by the later roadmap phases identified above rather than disappearing from project memory.

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

Runtime-only provider, model, generation, system-prompt, context, and capability overrides are applied to execution snapshots created from the persistent profile. They never mutate the stored profile. Runtime configuration remains distinct from per-execution host input.

Each runtime instance owns private memory through its `MemoryOwnerId`, keeping private agent-scoped memory separate across instances created from the same profile. Shared memory is possible only through an explicit shared scope and authorization policy.

Each instance-bound execution receives a monotonically increasing instance revision. Hosts can use `AgentRuntimeInstance.IsExecutionCurrent(execution)` to reject late results after a newer execution starts or the instance is retired. The generic execution hardening phase additionally ensures late provider completion cannot overwrite a terminal execution outcome.

Retirement and shutdown are separate lifecycle operations. Retirement prevents new executions and invalidates result authority while allowing already-running work to finish or be cancelled by the host. Shutdown is terminal, prevents new executions, invalidates result authority, and requests cancellation of outstanding instance-bound executions.

`IAgentExecutionScheduler` and the default `AgentExecutionScheduler` provide an optional host-controlled admission boundary with a configurable concurrency limit. The scheduler does not own host timing or replace runtime execution semantics.

First-class resource foundations are established by Phase 0.8. Mature capability governance, resource inheritance, runtime tri-state overrides, and governed learning are completed in Phase 0.9575. Runtime instances provide the isolation and immutable snapshot boundaries those later resource semantics depend on.

## External-host relationship

Phase 0.9 establishes the runtime-instance foundation. Phase 0.95 completes the generic execution boundary required for external hosts: arbitrary host input/context, host correlation, structured output contracts, terminal execution semantics, and tool identity propagation. The first-class resource model is already foundational infrastructure, while Phase 0.9575 consumes the runtime guarantees for mature Knowledge, Skills, Memory, Learning, and management governance.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, private-memory, or execution-state collisions. Resource governance and learning may then build on the stable runtime and snapshot boundaries without weakening runtime isolation.

## Phase 0.95 — Generic External Host Integration

## Status

**Complete — verified on .NET Framework 4.8.1 and .NET 9.**

## Goal

Complete the provider-neutral execution boundary required by arbitrary host applications without coupling HAgent.Core to a host domain, UI framework, scheduler, persistence model, or side-effect system.

## Requirements

1. [x] Introduce a canonical `AgentExecutionRequest` carrying multiple messages, host-supplied bounded context, runtime overrides, and execution options.
2. [x] Preserve plain string message execution as a convenience overload over the canonical request boundary.
3. [x] Preserve host-supplied correlation identity separately from `AgentExecution.Id` and `AgentRuntimeInstance.InstanceId`.
4. [x] Propagate execution, runtime-instance, tool-call, tool, and host correlation identity through tool execution metadata.
5. [x] Define a host-owned `StructuredOutputOptions` request contract with HAgent-side validation.
6. [x] Request structured output through capable provider adapters rather than relying on post-generation JSON detection alone.
7. [x] Validate normalized structured output against the requested schema and expose normalized validation/result metadata.
8. [x] Preserve provider capability distinction for structured output (`Supported`, `Unsupported`, `Unknown`).
9. [x] Make execution terminal-state transitions race-safe so late provider completion cannot overwrite cancellation, timeout, retirement, shutdown, or another terminal outcome.
10. [x] Preserve independent runtime-instance identity, overrides, execution state, shutdown lifecycle, and private memory ownership when multiple instances originate from one profile.
11. [x] Snapshot runtime overrides and host context at the execution/instance boundary so mutable caller-owned state cannot create cross-instance coupling.
12. [x] Keep host scheduling external; HAgent provides only focused admission-control primitives.
13. [x] Preserve host ownership of domain state, persistence, authorization, scheduling policy, and side effects.
14. [x] Add deterministic Example verification covering generic execution input, host correlation, structured output, late completion protection, tool identity propagation, concurrent runtime instances, and memory isolation.
15. [x] Define and verify a provider-facing `ProviderExecutionRequest` boundary separate from the host-facing request.
16. [x] Route normal, tool-calling, and streaming provider adapter contracts through `ProviderExecutionRequest`.
17. [x] Use provider-facing structured-output requirements for provider-native constrained generation where supported, with controlled fallback and continued HAgent validation.
18. [x] Add and verify an external-consumer smoke sample representing a host consuming the HAgent production surface on both supported target frameworks.
19. [x] Compose a long-lived `AgentRuntimeInstance` with the canonical `AgentExecutionRequest` through `HAgentClient.ExecuteAsync(instance, request, cancellationToken)`, preserving request input/context/correlation/structured-output semantics while the instance supplies runtime identity, revision, overrides, lifecycle, and private-memory ownership. Verified by `RUNTIME INSTANCE REQUEST` on the Example application.

## API direction

The public integration shape is:

```text
Host
  -> AgentRuntimeInstance (optional long-lived execution identity)
  + AgentExecutionRequest (execution input/context)
  -> HAgentClient.ExecuteAsync(instance, request, ...)
  -> AgentExecution
```

The runtime instance and execution request are orthogonal. The runtime instance answers **who is executing**; the request answers **what is being executed**. The request must not absorb the runtime instance because runtime ownership, lifecycle, revision, overrides, shutdown signaling, and private-memory ownership belong to the instance boundary.

After HAgent resolves the agent/provider/runtime state, the provider adapter receives:

```text
AgentExecutionRequest + runtime-derived execution options
  -> ProviderExecutionRequest
  -> provider adapter
  -> normalized AIResponse
```

The request boundaries remain generic. HAgent does not define host-domain schemas, event types, command types, domain objects, or lifecycle policy.

## Runtime invariants

One reusable profile may produce many long-lived runtime instances. Every runtime instance remains independently addressable and owns its own runtime lifecycle, execution revision, override snapshot, shutdown signaling, and private memory ownership.

When an execution is started from a runtime instance, `request.AgentId` must match `instance.ProfileId`. The caller's request/options objects are not mutated to attach runtime identity; HAgent creates the effective execution request/options internally.

An execution that has reached a terminal outcome cannot later publish a conflicting outcome because a provider completed late. Non-cooperative providers may continue executing after HAgent has completed cancellation/timeout handling, but their late results cannot regain authority over the terminal execution state.

## Structured-output invariant

A structured-output response is valid only when the requested contract is successfully honored and validated. Arbitrary JSON text is not sufficient evidence that the contract was satisfied.

Provider-native constrained generation is opportunistic. When an OpenAI-compatible endpoint supports the native `response_format`/JSON Schema request shape, the adapter uses it. If the endpoint explicitly reports that feature as unsupported or unknown, the adapter may retry without the native field. HAgent validation remains authoritative in either path.

## External consumer verification

`samples/HAgent.ExternalConsumer` is a standalone host sample that references the broad HAgent production surface available to an application: Core, the OpenAI-compatible provider transport, File storage, SQL Server storage, MySQL storage, and WinForms. It owns its own host-side test data/provider and does not introduce HWorld-specific domain logic into HAgent. The sample has been executed successfully on both `.NET Framework 4.8.1` and `.NET 9`.

A real host is not required to reference every HAgent assembly in production; it selects the modules it needs. The sample is intentionally broad so this milestone verifies the public HAgent system surface rather than only `HAgent.Core`.

## HWorld boundary

HWorld is an external consumer. HAgent does not contain an HWorld dependency, adapter, physics, rendering, simulation-time, or action-authority code. HWorld references the HAgent modules it needs and owns its own domain lifecycle, scheduling, state, authorization, and side effects.

## Exit criterion

A host can submit a complete provider-neutral execution request with bounded context, host correlation, and optional structured-output requirements; HAgent can execute that request either directly or through a long-lived runtime instance without losing request semantics or runtime ownership. HAgent resolves the request into a provider-facing request, invokes an adapter, normalizes the response, validates host-owned contracts, preserves execution identity, protects terminal state, and isolates runtime snapshots without coupling to host or provider-specific domain models. A standalone external consumer representing the HAgent production surface demonstrated the public boundary on both supported target frameworks, and runtime-instance execution composes the canonical request through the verified instance/request API.

## Phase 0.951 — Identity, Tenancy, and User Context

## Status

**Completed — verified in HAgent.Example.**

## Goal

Define provider-neutral identity and context contracts that allow HAgent to distinguish deployment, tenant, user, session, workspace, agent profile, runtime instance, execution, and related principals without implementing authentication itself.

## Requirements

1. [x] Define a provider-neutral identity context suitable for authorization, audit, evaluation, memory, knowledge, and runtime context. A separate `Principal` object is not required when the shared context is sufficient.
2. [x] Distinguish deployment/application identity from tenant, user, session, workspace, agent profile, runtime-instance, and execution identity through a shared `AgentIdentityContext` and existing runtime/execution identities.
3. [x] Define optional tenancy so single-tenant hosts remain simple while multi-tenant hosts can isolate HAgent resources through an explicit `TenantId` boundary.
4. [x] Propagate the shared identity context through execution snapshots and public execution results, with tool-execution and audit projections now carrying the same identity context. Extend the same context to memory, knowledge, learning, policy, events, tracing, and evaluation in their respective phases.
5. [x] Keep authentication and credential verification outside HAgent; the identity contract is host-supplied context only.
6. [x] Define stable resource scope semantics for Global, Tenant, User, Workspace, Agent, Runtime, and Execution through `AgentResourceScope`.
7. [x] Ensure private runtime memory and other private resources can be isolated by explicit owner identity through the canonical `AgentResourceOwnership` contract. The redesign rule requires subsystems to consume the canonical ownership model directly rather than preserving obsolete parallel mechanisms.
8. [x] Make identity context immutable within an execution snapshot by cloning the host-supplied identity when the snapshot is created.
9. [x] Define safe behavior when host identity information is absent; identity fields are optional and default to empty values rather than fabricated identities.
10. [x] Add deterministic Example verification for single-user, multi-user, and multi-tenant identity propagation and isolation.

## Initial implementation

The first implementation slice introduced:

```text
AgentExecutionRequest.Identity
        ↓
AgentExecutionSnapshot.Identity
        ↓
AgentExecution.Identity
        ├── ToolExecutionContext.Identity
        ├── ToolExecutionResult.Identity
        └── AgentExecutionAuditRecord identity projection
```

`AgentIdentityContext` is provider-neutral and carries:

```text
DeploymentId
TenantId
PrincipalId
DisplayName
UserId
SessionId
WorkspaceId
```

The execution snapshot keeps its own copy so caller-owned request state cannot mutate the identity associated with an active execution. Tool execution receives the same captured context, and audit projections retain identity dimensions without storing sensitive payloads.

## Resource ownership implementation

HAgent defines a canonical `AgentResourceScope` and `AgentResourceOwnership.GetOwnerId(...)` contract for HAgent-owned resource partitioning:

```text
Global
Tenant
User
Workspace
Agent
Runtime
Execution
```

The canonical owner key preserves deployment and, where applicable, tenant context. Therefore the same `UserId` in two tenants cannot produce the same user owner key.

Private runtime memory ownership remains distinct from execution ownership. Runtime instances use their runtime identity, while execution-scoped resources use the execution identity. These identities must not be collapsed.

Storage partitioning is not authorization. Authorization remains a policy decision using the supplied identity, operation, resource scope, and applicable policy.

## Verification

`HAgent.Example` verifies:

```text
Identity Snapshot
Identity Execution
Identity Tool
Identity Isolation
```

The verified isolation scenario covers:

- separate owner keys for different users;
- tenant-qualified user ownership;
- isolation of the same user ID across tenants;
- private memory visibility by owner;
- distinct runtime and execution resource scopes;
- preservation of unrestricted-store behavior as distinct from authorization.

## Architectural outcome

```text
Deployment
  -> Tenant (optional)
      -> User / Principal
          -> Session
              -> Workspace (optional)
                  -> Agent Profile
                      -> Runtime Instance
                          -> Execution
                              -> Resource Scope + Owner Key
                                  -> Tools / Memory / Knowledge / Audit / downstream subsystems
```

HAgent consumes identity context; the host remains responsible for authentication and authoritative user/account lifecycle.

## Phase 0.952 — Event Subsystem

## Status

**Completed — verified in the HAgent Example host on 2026-09-06.**

## Goal

Make events a first-class provider-neutral HAgent concept so hosts, tools, runtimes, workflows, and the future Persistent Cognitive Runtime can use one generic event model.

## Requirements

1. [x] Define `EventEnvelope` with stable event ID, type, source, timestamp, correlation/causation metadata, optional importance, and bounded payload/context.
2. [x] Define event source and scope semantics without assuming a specific host domain.
3. [x] Support user, application, timer, tool, provider, memory, goal, agent-message, and external events through the same generic contract.
4. [x] Define bounded event queues, retention, expiration, and deduplication semantics.
5. [x] Define an asynchronous event dispatch boundary with cancellation and backpressure.
6. [x] Preserve event provenance and correlation into runtime decisions and executions.
7. [x] Support event filtering/routing without making the event subsystem a domain-specific message bus.
8. [x] Define persistence as optional and keep live queues/process-local handlers separate from durable event records.
9. [x] Ensure event delivery is safe under concurrent producers and consumers.
10. [x] Add deterministic Example verification for publishing, filtering, deduplication, expiration, bounded queues, cancellation, and correlation propagation.

## Verification result

The Example host reported success for all three event tests:

- Event envelope clone preservation, nested identity/context isolation, and scoped-event validation.
- Concurrent dispatch, type/source/scope filtering, correlation propagation, and identity propagation.
- Duplicate suppression, expiration rejection, publish cancellation, bounded configuration, and handler fault isolation.

## Architectural outcome

```text
Host / Provider / Tool / Runtime
            |
            v
      EventEnvelope
            |
      Event Dispatcher
       /           \
   reactive      cognitive
    handler       runtime
```

The subsystem provides generic event infrastructure; it does not become a replacement for a host's enterprise message broker.

## Phase 0.953 — Unified Policy Engine

## Status

**In progress — policy contracts, deterministic evaluation, precedence, provenance, cost guard, pre-transport runtime enforcement, effective-policy execution snapshots, canonical policy persistence, policy-gated tool invocation, policy-first host authorization composition, profile/runtime resource capability resolution, and typed learning-promotion policy transitions are implemented.**

## Goal

Unify HAgent's growing permission, capability, cost, learning, approval, resource, and execution rules behind a coherent provider-neutral policy model.

## Requirements

1. [x] Define generic policy, rule, scope, evaluation context, and decision contracts.
2. [x] Represent at least `Allow`, `Deny`, `RequireApproval`, `Defer/Wait`, and `NotApplicable` outcomes where meaningful.
3. [x] Support policy scopes such as system, tenant, user, workspace, agent, runtime, execution, resource, tool, and provider/target where applicable.
4. [x] Integrate existing permission/authorization concepts without replacing host-owned authorization.
5. [x] Integrate cost policy (`FreeOnly`, `FreePreferred`, `NoRestriction`) through the policy system at the evaluation boundary.
6. [x] Integrate typed learning-promotion policy and explicit candidate review/approval/promotion transitions into the policy boundary.
7. [x] Complete verification of capability/resource enablement and runtime tri-state overrides at the profile/runtime resource boundary, with execution snapshot capture and tool gating.
8. [x] Support explicit policy precedence and deterministic conflict resolution.
9. [x] Preserve policy provenance so diagnostics can explain which rule produced a decision.
10. [x] Make policy evaluation deterministic where inputs are deterministic and expose an explicit policy version for cache invalidation.
11. [x] Capture the full effective policy state in the execution snapshot, including the deep-cloned policy version/rules that govern the run.
12. [x] Prevent prompt content from serving as the policy enforcement mechanism.
13. [x] Complete deterministic Example verification for the newly added resource capability resolution, persistence, snapshot isolation, and tool gating in addition to the already verified policy/authorization scenarios.

## Implemented slices

The current implementation includes:

- `AiPolicySet` and `AiPolicyRule` for versioned, scoped rules;
- `AiPolicyEvaluationContext` for bounded identity/resource/execution inputs;
- `AiPolicyDecision` with outcome and provenance;
- `IAiPolicyEngine` and `DefaultAiPolicyEngine`;
- `GetPolicySnapshot()` as the explicit owned-policy capture boundary;
- deterministic precedence based on explicit priority, scope specificity, match specificity, outcome restrictiveness, and stable rule ID;
- empty rule constraint collections treated as unrestricted dimensions;
- built-in `FreeOnly` enforcement where `Paid` and `Unknown` cost states are denied;
- `AgentExecution.PolicyDecision` capture;
- `AgentExecutionSnapshot.EffectivePolicy` deep-cloned at execution creation;
- default runtime loading of persisted policy through `IAiStore` asynchronously, while explicit policy-engine injection remains available;
- File, SQL Server, and MySQL policy persistence through the canonical `IAiStore` contract;
- SQL Server/MySQL bootstrap creation of the policy table;
- runtime enforcement after execution-target selection and before provider transport;
- policy enforcement before executable tool handlers through `HAgentClient.ExecuteToolAsync`;
- policy decisions captured in `ToolExecutionResult` and one evaluator captured for each tool loop;
- `PolicyDataAccessAuthorizer` composition of HAgent policy with host `IDataAccessAuthorizer`, preserving host authority after policy evaluation;
- canonical identity propagation into `DataAuthorizationRequest` for policy composition;
- `AiResourceCapabilityPolicy` profile defaults with `Inherit` / `Enabled` / `Disabled` states;
- runtime-only `AgentRuntimeOverrides.ResourceCapabilityOverrides` with runtime-over-profile precedence;
- deterministic effective resolution with exact-resource precedence, resource-type fallback, `Inherit` fall-through, and default `Enabled` state;
- `AgentExecutionSnapshot.EffectiveResourceCapabilities` capturing resolved resource enablement for each execution;
- tool execution resource gating before executable handler side effects, including runtime-instance-specific overrides;
- `AiLearningPromotionRequest` for bounded typed candidate metadata and source identity;
- `AiLearningPromotionPolicy` routing learning promotion through the existing `IAiPolicyEngine` using operation `learning.promote` and resource type `learning-candidate`;
- `AiLearningCandidate` guarded `Proposed` / `PendingReview` / `Approved` / `Rejected` / `Promoted` transitions mapped from policy outcomes;
- deterministic Example verification in `MainForm.PolicyTests.cs`, `MainForm.ResourceCapabilityTests.cs`, and `MainForm.LearningPolicyTests.cs` for policy persistence, runtime enforcement, host authorization, resource capability resolution/persistence/snapshot isolation/tool gating, and learning promotion/review transitions.

Policy persistence, provider/tool/data authorization, and the resource capability boundary were locally verified by the user on 2026-09-08. The learning-promotion implementation and its new Example verification are ready for the next local run and must not be described as passing until that Example test succeeds.

## Remaining slices

1. Verify the learning-promotion policy/candidate transition slice locally and then treat it as complete.
2. Add bounded human approval/defer workflow integration.
3. Implement the policy management UI refinement in layers:
   - make rule editing human-oriented with semantic selectors for scope, agent, tool, resource, provider, operation, and outcome, while retaining advanced raw identifiers only where necessary;
   - make Effective Decisions a read-only diagnostic surface showing the evaluated context, selected rule, policy version, precedence/provenance, and built-in guard contribution;
   - make Agent Capabilities a per-agent capability/resource matrix showing persistent profile state, transient runtime override, and deterministic effective state, with the source of the effective value where useful;
   - source display names and selectable entities from existing HAgent configuration/runtime registries rather than creating duplicate UI-specific registries;
   - keep unresolved or stale identifiers visible and diagnosable rather than silently rewriting or deleting policy state;
   - preserve the core policy engine as the single decision/precedence authority. UI behavior must not introduce a second policy evaluator or alternative capability semantics.
4. Expand deterministic Example verification and backend-specific live verification where configured.

The detailed target behavior for the Policy page and its components is defined in `docs/architecture/09-policy.md` under **Policy configuration UI evolution** and should be treated as the architectural UI contract for subsequent refinements.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects. Policy may further restrict a host operation, but an HAgent policy `Allow` never grants application authorization.

Resource enablement is a separate configuration capability layer. It does not replace provider capability discovery or host authorization. An enabled resource must still pass any applicable policy and authorization boundaries before side effects occur.

Learning promotion uses the same policy boundary rather than a parallel learning authorization evaluator. A typed candidate may move to `Approved` only through an `Allow` policy decision or explicit review after `RequireApproval`/`Defer`. Promotion to authoritative storage is a separate operation and is not performed by policy evaluation itself.

## Phase 0.954 — Prompt and Instruction Governance

## Status

**Verified and complete on 2026-09-08.**

## Goal

Define trusted instruction layers and provenance so HAgent can safely combine system policy, agent instructions, Skills, Knowledge, Memory, tools, runtime context, user input, and externally retrieved content.

## Requirements

1. [x] Define normalized instruction/source records with source type, authority/trust level, provenance, scope, and lifecycle metadata.
2. [x] Define deterministic instruction composition and precedence rules.
3. [x] Distinguish trusted policy/instructions from untrusted retrieved content and ordinary user/model-generated text.
4. [x] Prevent lower-authority content from silently overriding higher-authority policy.
5. [x] Ensure prompts never substitute for authorization, permissions, approval, or other code-enforced controls.
6. [x] Track the instruction sources contributing to an execution snapshot.
7. [x] Support Skill, Knowledge, Memory, tool-description, runtime, and host-context instructions without creating provider-specific prompt formats in Core.
8. [x] Define handling for instruction conflicts, unsafe/invalid sources, disabled resources, and unavailable source content.
9. [x] Keep secrets and sensitive host data out of diagnostic instruction traces by default.
10. [x] Add deterministic Example verification for precedence, untrusted-content handling, conflicts, disabled resources, and execution-snapshot provenance.

## Verification

The user verified `COGNITION INSTRUCTIONS` on both .NET Framework 4.8.1 and .NET 9, covering source creation/validation, authority-vs-trust separation, precedence, explicit priority, conflict representation, provenance-preserving snapshot cloning, canonical additive composition, deterministic conflicts, disabled/invalid containment, sensitive-content exclusion, trusted-resource authority/trust, external-content boundaries, unavailable-source handling, lower-authority override resistance, effective snapshot capture, provider transport parity, caller-source mutation isolation, and execution/principal provenance.

The user also verified the deterministic runtime boundary examples on the supported targets:

- .NET 9: `RUNTIME TERMINAL STATE`, `RUNTIME CONCURRENCY`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and `RUNTIME SHUTDOWN`.
- .NET Framework 4.8.1: `RUNTIME INSTANCES`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and `RUNTIME SHUTDOWN`.

These Examples use local/in-memory provider infrastructure where deterministic behavior is required; configuration-driven `RUNTIME EXECUTION` remains a separate live-provider host example and is not used as a deterministic milestone gate.

## Architectural outcome

```text
System / Policy
      ↓
Agent instructions
      ↓
Skills / trusted resources
      ↓
Knowledge / Memory / tool descriptions
      ↓
Runtime + host context
      ↓
User / external content
      ↓
Provider request
```

The exact precedence rules are implementation-defined, but authority and provenance remain explicit. Execution captures the effective instruction snapshot before provider transport, and provider adapters receive the same composed effective instruction text rather than rebuilding instruction governance.

## Phase 0.955 — Context Engineering

## Status

**In progress — context contracts, acquisition, ranking, deterministic compaction, reusable caching, execution/provider integration, and bounded multi-resource retrieval are verified; policy/permission-aware assembly and the complete phase verification matrix remain.**

## Goal

Make context assembly a first-class HAgent subsystem that selects, ranks, bounds, compresses, and explains the information sent to an execution instead of treating prompt construction as string concatenation.

## Requirements

1. [x] Define provider-neutral context items with source, type, provenance, trust, importance, freshness, scope, and estimated size.
2. [x] Define context budgets for tokens/characters/items and other applicable resource dimensions.
3. [x] Separate context retrieval from context assembly and from cognitive attention.
4. [x] Support relevance ranking using goal relevance, attention, recency, importance, trust, redundancy, and estimated cost where available.
5. [ ] Support bounded memory, knowledge, skill, conversation, host-context, tool-description, and instruction retrieval.
6. [x] Support compaction, summarization, deduplication, and truncation strategies without silently discarding required policy or provenance.
7. [x] Preserve source/provenance metadata for assembled context and expose safe diagnostics explaining inclusion/exclusion.
8. [x] Support reusable and cacheable context components when configuration/version rules permit.
9. [x] Keep provider-specific tokenization behind optional adapters; Core must not require a particular tokenizer.
10. [ ] Ensure context assembly respects policy, permissions, disabled resources, and instruction authority.
11. [x] Capture the resulting bounded context in immutable execution snapshots.
12. [ ] Add deterministic Example verification for budgets, ranking, prioritization, compaction, source provenance, cache reuse, and policy-enforced exclusion.

## Verified slices

- Slice 2: provider-neutral context contracts and budgets — verified 2026-09-08 on .NET Framework 4.8.1 and .NET 9 Example execution.
- Slice 3: bounded acquisition and execution-owned context snapshots — verified 2026-09-08 with 20/20 HAgent.Tests and deterministic Example coverage.
- Slice 4: ranking, deterministic prioritization, and deduplication — verified 2026-09-08 with 24/24 HAgent.Tests and deterministic Example coverage.
- Slice 5: deterministic compaction/truncation and provenance-preserving diagnostics — verified 2026-09-09 with 29/29 HAgent.Tests and deterministic Example coverage.
- Slice 6: cache-safe reusable context components — verified 2026-09-09 with 34/34 HAgent.Tests and deterministic public-API Example coverage.
- Slice 7: execution/provider integration and request isolation — verified 2026-09-09 with 37/37 HAgent.Tests and deterministic public-API Context Execution Integration Example coverage.
- Slice 8: bounded multi-resource retrieval — verified 2026-09-09 with 41/41 HAgent.Tests and deterministic public-API Example coverage on .NET Framework 4.8.1 and .NET 9.

## Current implementation checkpoint

- Slice 9: policy/permission-aware context assembly — implementation checkpoint; local verification pending.
- The implementation composes the existing unified `IAiPolicyEngine` with the execution's effective `AiResourceCapabilitySnapshot`.
- Denied/approval-required/deferred sources are excluded before their source callback is queried; disabled sources are excluded before retrieval.
- Candidates are evaluated before global budget assembly so denied candidates cannot consume the bounded context budget.
- Admission decisions contain bounded metadata only and do not carry context payloads or translate policy into prompt text.

## Architectural outcome

```text
Available information
        ↓
Policy + permissions
        ↓
Attention / relevance
        ↓
Retrieval
        ↓
Policy-aware admission
        ↓
Ranking / deduplication
        ↓
Compression / compaction
        ↓
Bounded Context
        ↓
Execution Request
```

Context engineering remains distinct from cognitive decision making: cognition decides what matters; context engineering constructs the bounded evidence supplied to an execution.

## Phase 0.956 — Observability and Distributed Tracing

## Status

**Planned architectural foundation before capability-aware execution and persistent cognition.**

## Goal

Turn HAgent execution, resource use, policy decisions, cognition, tools, provider activity, and lifecycle changes into a coherent structured trace that can be correlated across operations and processes.

## Requirements

1. [ ] Define provider-neutral trace/span concepts for HAgent operations.
2. [ ] Correlate deployment, tenant, user/session, workspace, agent, runtime, execution, tool-call, provider-target, event, policy, and evaluation activity where applicable.
3. [ ] Represent operation start/end, duration, status, parent relationship, decision reason, and safe metadata.
4. [ ] Trace context assembly, resource retrieval, policy evaluation, candidate selection, admission, provider execution, tool execution, learning, and cognitive transitions.
5. [ ] Support configurable redaction of prompts, responses, arguments, host context, and other sensitive data.
6. [ ] Keep secrets, credentials, raw connection strings, and sensitive payloads out of traces by default.
7. [ ] Support local/in-memory tracing plus host-integrated sinks without forcing one telemetry vendor or transport.
8. [ ] Define bounded trace retention and sampling controls.
9. [ ] Preserve cross-process correlation for network/database-backed deployments where identity is available.
10. [ ] Make stale-result rejection, policy denial, fallback, waiting, retry, and recovery decisions observable.
11. [ ] Provide a safe human-readable diagnostic projection for management UI.
12. [ ] Add deterministic Example verification for trace hierarchy, correlation propagation, redaction, sampling, failures, cancellation, and fallback paths.

## Architectural outcome

```text
Event / Request
      ↓
Trace
 ├── Policy
 ├── Context
 ├── Planning
 ├── Admission
 ├── Provider
 ├── Tools
 ├── Memory/Knowledge
 └── Outcome
```

Tracing is observability, not authorization and not transcript storage.

## Phase 0.957 — Evaluation and Quality Measurement

## Status

**Planned architectural foundation for reliable agent behavior and later optimization.**

## Goal

Give HAgent a provider-neutral way to measure whether executions, tool use, plans, learning changes, and agent outcomes achieved their intended quality or task goals.

## Requirements

1. [ ] Define evaluation contracts independent of any specific LLM vendor or grading service.
2. [ ] Support evaluation targets including execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning candidate quality.
3. [ ] Support deterministic evaluators such as schema validity, required-field checks, policy compliance, tool success, latency, cost, and task completion signals.
4. [ ] Support externally supplied human/application ratings and labels.
5. [ ] Support model-assisted evaluators without treating evaluator-model output as unquestionable truth.
6. [ ] Preserve evaluation provenance, evaluator identity/type, input references, timestamp, and confidence where meaningful.
7. [ ] Correlate evaluations with execution/runtime/agent/goal/plan/trace identities.
8. [ ] Keep evaluation data separate from authoritative agent state; an evaluation does not automatically mutate configuration, memory, skill, or knowledge.
9. [ ] Support repeated test cases and regression suites for provider/model/agent comparisons.
10. [ ] Support aggregate metrics such as success rate, quality score, latency, cost, fallback frequency, tool success, and plan completion.
11. [ ] Add deterministic Example verification for evaluation creation, aggregation, human rating, failed evaluations, and comparison of alternative execution targets.

## Architectural outcome

```text
Execution / Goal / Plan
        ↓
    Evaluation
        ↓
 score / label / evidence
        ↓
  metrics / regression
```

Evaluation measures behavior; it does not become a hidden decision-maker for authorization.

## Phase 0.9575 — Knowledge, Skills, Memory Governance + Learning

## Status

Planned after Phase 0.957 Evaluation / Quality Measurement and before Phase 0.958 Agent Lifecycle / Health.

## Goal

Complete the **mature governance and learning layer** for HAgent's first-class Knowledge, Skills, Memory, and Learning resources.

The foundational resource model is established earlier through HAgent's internal storage/resource foundations. This phase does not introduce a second resource architecture and does not delay foundational resource contracts until this point. Instead, it makes those resources fully governed, reusable, scoped, observable, configurable, and administrable across profiles, runtime instances, executions, and learning workflows.

## Architectural position

HAgent treats these as four related but distinct first-class capabilities:

```text
Skills    = reusable executable capabilities/procedures
Knowledge = reusable retrievable information
Memory    = scoped experience/state
Learning  = governed process that turns experience into typed candidates
            and, when permitted, promotes them into Memory, Knowledge, or Skills
```

The earlier resource foundation establishes canonical identity, scope, provenance, versioning, storage, and provider-neutral contracts. This phase establishes the mature governance model around those resources.

```text
Resource Foundations
    ↓
Identity / Ownership / Scope
    ↓
Policy / Authorization
    ↓
Resource Governance
    ↓
Context / Instruction / Execution
    ↓
Evaluation / Experience
    ↓
Learning Candidates
    ↓
Validation / Approval / Promotion
    ↓
New resource version or scoped state
```

## Prerequisites

1. [ ] Resource foundations from the earlier internal-storage/resource layer are canonical and provider-neutral. The historical 0.8 repository/backend wiring obligation is explicitly deferred to this phase for the resource persistence that governance requires; see `docs/roadmap/20-data-access.md`.
2. [ ] Identity / tenancy / user context is available for ownership and scope decisions.
3. [ ] Unified policy enforcement is available for resource access and learning promotion decisions.
4. [ ] Prompt/instruction governance and context engineering expose trust/provenance boundaries needed when resources enter execution context.
5. [ ] Evaluation provides quality signals usable by learning and promotion decisions.
6. [ ] Runtime instances and execution snapshots provide stable runtime ownership and immutable effective state boundaries.

## Resource governance

7. [ ] Define one generic resource governance model shared by Skills, Knowledge/Wiki, Memory families/types, and future resource types.
8. [ ] Make resource scope explicit and authorization-aware rather than inferred from agent identity alone.
9. [ ] Support Global, Tenant, Domain, User, Agent, Runtime, and Execution scopes where applicable to the resource type.
10. [ ] Preserve owner/resource identity separately from runtime-instance identity.
11. [ ] Preserve resource provenance, lifecycle/status, version, source, and relationship metadata through retrieval and promotion.
12. [ ] Ensure shared/reusable resources are references to authoritative resources rather than private copies embedded in agents.
13. [ ] Prevent prompt text or model output from granting access to a disabled or unauthorized resource.

## Capability policy and inheritance

14. [ ] Add profile capability defaults for Skills, Knowledge/Wiki, Memory families/types, individual resources, and future resource types.
15. [ ] Add tri-state runtime override: `Inherit`, `Enabled`, `Disabled`.
16. [ ] Resolve system/host policy → agent profile → runtime override into one effective resource capability state.
17. [ ] Capture the effective capability/resource state in every execution snapshot that can observe or invoke those resources.
18. [ ] Enforce capability policy before retrieval, exposure, or invocation.
19. [ ] Ensure runtime-only overrides never mutate the persistent profile.
20. [ ] Surface the source and effective value so operators can distinguish inherited, explicitly configured, and overridden state.

## Knowledge and Wiki

21. [ ] Complete the provider-neutral knowledge resource/source contract and managed Wiki model over the earlier resource foundation.
22. [ ] Support identity, title/content, summary, metadata, tags/categories, provenance, lifecycle/status, versioning, relationships, and bounded retrieval metadata.
23. [ ] Keep retrieval independent from physical indexing: keyword, semantic, hybrid, relational, or future implementations remain replaceable.
24. [ ] Support reusable shared knowledge plus authorized agent/runtime scoped resources.
25. [ ] Prevent model-generated content from silently becoming authoritative knowledge.
26. [ ] Preserve source and promotion provenance when a candidate becomes authoritative knowledge.
27. [ ] Make knowledge retrieval bounded by policy, resource limits, and context budgets.

## Skills

28. [ ] Complete stable/versioned `SkillDefinition` and skill-set/reference semantics over the earlier resource foundation.
29. [ ] Keep executable handlers separate from persisted definitions and never serialize handlers.
30. [ ] Support required knowledge, required tools, input/output contracts, preconditions, procedure steps, constraints, and lifecycle/version metadata.
31. [ ] Allow reusable skills to be referenced by multiple agents without duplicating the definition.
32. [ ] Preserve execution snapshot semantics so in-flight executions continue using their captured skill version/state.
33. [ ] Support `SkillCandidate` → validation/evaluation → new skill version or explicit rejection.
34. [ ] Ensure skill invocation remains subject to policy and authorization.

## Memory

35. [ ] Normalize memory families including working, episodic, semantic, procedural, and future extensible types.
36. [ ] Make memory scope explicit: execution, runtime, logical agent, user, tenant, or other approved scope.
37. [ ] Preserve independent runtime-instance private-memory isolation.
38. [ ] Keep storage implementation separate from memory ownership and retrieval policy.
39. [ ] Make memory-type enable/disable state governable at profile and runtime levels.
40. [ ] Support bounded retrieval and retention policies appropriate to each memory family/type.
41. [ ] Preserve provenance and confidence/evidence metadata where available.
42. [ ] Keep Memory usable without GPU hardware, vector databases, embeddings, or large resident indexes.

## Learning modes and candidates

43. [ ] Define provider-neutral `LearningMode`: `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, `FullyAutomatic`.
44. [ ] Keep Learning Mode distinct from resource/capability enablement.
45. [ ] Define one learning policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization.
46. [ ] Support typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts.
47. [ ] Preserve source execution ID, runtime ID, agent/profile identity, scope, provenance, and evidence/confidence on candidates when available.
48. [ ] Support deterministic code-derived learning signals without requiring an LLM.
49. [ ] Allow optional model-assisted extraction/classification while keeping the model non-authoritative.
50. [ ] Keep candidate creation separate from candidate promotion.
51. [ ] Preserve published Skill/Knowledge versions when learning produces an improvement; never silently mutate an active version.

## Learning lifecycle

52. [ ] Establish the canonical lifecycle:

```text
Experience / Observation
    → Candidate
    → Validation / Evaluation
    → Policy decision
    → Approval where required
    → Promotion
    → New authoritative version or scoped state
```

53. [ ] Support candidate rejection without modifying the target resource.
54. [ ] Support candidate expiry/retention according to policy.
55. [ ] Preserve candidate provenance after rejection or promotion according to retention rules.
56. [ ] Require explicit authorization for promotion into authoritative Knowledge or published Skills.
57. [ ] Ensure a model cannot bypass candidate state transitions by emitting apparently authoritative content.
58. [ ] Make promotion decisions auditable and attributable to policy, operator, or automated governance rules.
59. [ ] Allow evaluation outcomes to block, approve, or condition promotion.

## Context and instruction integration

60. [ ] Integrate governed Skills, Knowledge, Memory, and externally retrieved content into the canonical context/instruction pipeline.
61. [ ] Carry resource provenance, trust, scope, and effective capability state into the execution snapshot where exposed.
62. [ ] Ensure disabled, unauthorized, unavailable, stale, or failed resources remain diagnosable without being exposed as authoritative context.
63. [ ] Keep prompt/instruction composition separate from authorization and enforcement.
64. [ ] Ensure lower-trust resource content cannot override higher-authority instructions or policy.

## Management UI

65. [ ] Add Learning Review management with pending candidates, inspection, provenance/evidence, source execution/runtime, target scope, approve, reject, and retention state.
66. [ ] Add Knowledge/Wiki Manager with New/Edit/Delete, search/filter, relationships, version/status, provenance, and used-by/accessed-by views.
67. [ ] Add Skill Manager with New/Edit/Delete, version/status, relationships, required dependencies, and used-by views.
68. [ ] Extend Agent Configuration so selecting an agent shows effective Skills, Knowledge/Wiki, Memory families, and generic future resource types.
69. [ ] Add profile-level controls for resource/capability enablement.
70. [ ] Add runtime-instance override controls using `Inherit` / `Enabled` / `Disabled`.
71. [ ] Show inherited, explicit, overridden, and effective states clearly.
72. [ ] Show the effective AI selection/cost policy established by Phase 0.96 without duplicating provider/model discovery.
73. [ ] Expose Learning Mode and make its relationship to Learning Policy explicit.
74. [ ] Keep known resource types specialized while unknown/future resource types remain visible through generic resource inventory.
75. [ ] Follow HAgent.WinForms conventions: shared `Header`, `HButton`, `HMessage`, and existing configuration composition boundaries.

## Storage

76. [ ] Complete HAgent-owned persistence for candidates, knowledge-resource relationships, skill versions/relationships, resource capability assignments/overrides, and extensible memory-type policy where still outstanding. This is the consuming completion point for the historical 0.8 Item 8 repository/backend wiring obligation for mature resources.
77. [ ] Keep File, SQL Server, and MySQL behavior aligned through versioned migrations.
78. [ ] Keep learning/review metadata secret-safe and bounded.
79. [ ] Preserve resource identity/ownership/scope directly in persistence rather than creating subsystem-specific ownership models.

## Runtime integration

80. [ ] Bind effective resource capability state into runtime execution snapshots.
81. [ ] Capture execution outcomes and observations as learning input without mutating runtime identity.
82. [ ] Preserve runtime isolation, execution correlation, cancellation, timeout, stale-result protection, and concurrent execution semantics.
83. [ ] Ensure runtime overrides remain transient and cannot write back into persistent profile configuration.
84. [ ] Make resource access and learning promotion visible through the same observability and policy boundaries as other runtime actions.

## Verification

85. [ ] Add deterministic Example verification for resource scope isolation, inherited/overridden capability state, memory families/types, knowledge retrieval, skill binding, and future resource types.
86. [ ] Add Example verification for `SuggestOnly` review, approval, rejection, and candidate retention.
87. [ ] Add tests that candidates cannot bypass authorization or directly mutate published Knowledge/Skills.
88. [ ] Add tests for resource version/snapshot isolation after profile/resource edits.
89. [ ] Add tests for promotion conflicts, stale candidates, contradictory evidence, and policy denial.
90. [ ] Add tests for runtime isolation across two independent runtime instances.
91. [ ] Add UI verification that Agent Configuration displays effective resource/capability state, Learning Mode, and inherited/overridden configuration correctly.
92. [ ] Verify all supported framework targets required by HAgent before declaring the phase complete.

## Exit criterion

Knowledge, Skills, Memory, and Learning are first-class HAgent resources with explicit identity, scope, ownership, provenance, policy, capability inheritance, runtime overrides, immutable execution snapshots, and provider-neutral contracts. Hosts can administer reusable Knowledge and Skills, isolate Memory correctly, and run governed learning from experience through typed candidates, evaluation, approval/policy, and safe promotion without allowing model output to become authoritative by itself.

## Phase 0.958 — Agent Lifecycle and Health Management

## Status

**Planned architectural foundation before persistent cognitive runtime.**

## Goal

Make agent/runtime lifecycle and health explicit, observable, recoverable, and controllable for both request-oriented and persistent agents.

## Requirements

1. [ ] Define normalized lifecycle states for runtime agents and persistent cognitive agents.
2. [ ] Distinguish lifecycle state from health state and execution state.
3. [ ] Support at least active, sleeping/idle, waiting, blocked, deliberating, executing, degraded, failed, retired, recovering, and shutdown semantics where applicable.
4. [ ] Define health/status reasons and safe transitions rather than exposing only a Boolean healthy flag.
5. [ ] Prevent retired/shutdown agents from originating new executions.
6. [ ] Support suspension/resume without deleting durable state.
7. [ ] Expose lifecycle and health changes through events and tracing.
8. [ ] Define heartbeat/progress or equivalent signals for long-running persistent runtimes where needed.
9. [ ] Detect stalled or repeatedly failing progress without confusing slow legitimate inference with failure.
10. [ ] Support operator-visible diagnostics explaining why an agent is blocked, waiting, degraded, or recovering.
11. [ ] Add deterministic Example verification for lifecycle transitions, suspension/resume, unhealthy/degraded states, stalled work, and shutdown safety.

## Architectural rule

Lifecycle state answers "what is the agent doing?" Health state answers "is the agent operating normally?" Execution state answers "what is this specific operation doing?" These concerns remain separate.

## Phase 0.9591 — Goal/Plan Persistence and Recovery

## Status

**Ordered before 0.959 Human-in-the-Loop / Intervention; planned foundation.**

This phase is intentionally moved ahead of 0.959 in the roadmap because durable goals, intentions, plans, plan-step revisions, checkpoints, and recovery state provide the persistent authority that later goal/plan intervention should govern safely.

## Goal

Make long-lived agent goals, intentions, plans, checkpoints, and recovery state durable without making transient executions or provider sessions part of persistent cognitive state.

## Requirements

1. [ ] Define durable Goal, Intention, Plan, PlanStep, checkpoint, and recovery metadata contracts.
2. [ ] Separate durable cognitive state from live execution tasks, cancellation tokens, provider sessions, HTTP state, and synchronization primitives.
3. [ ] Define plan revision/version semantics so stale executions cannot overwrite newer goals or plans.
4. [ ] Support partial plan execution and explicit step states.
5. [ ] Define checkpoint boundaries and durable progress records.
6. [ ] Define idempotency semantics for retried plan steps and externally observable actions.
7. [ ] Distinguish safe retry, unknown outcome, and completed outcome states.
8. [ ] Support recovery after process restart, crash, timeout, cancellation, or provider failure.
9. [ ] Reconcile in-flight executions during recovery and invalidate obsolete execution authority.
10. [ ] Support plan suspension, resumption, replacement, abandonment, and rollback/compensation metadata where applicable.
11. [ ] Keep host side effects authoritative; HAgent may persist intent and requested action state but must not claim external side effects occurred without evidence.
12. [ ] Support optional persistence backends through the HAgent storage abstraction.
13. [ ] Add deterministic Example verification for checkpoints, restart recovery, stale revisions, duplicate/retry handling, unknown outcomes, and plan supersession.

## Architectural outcome

```text
Goal / Intention
      ↓
     Plan
      ↓
 checkpoints / revisions
      ↓
 Execution
      ↓
 outcome evidence
      ↓
 durable progress / recovery state
```

Durability provides recovery semantics; it does not guarantee exactly-once execution of arbitrary host side effects.

## Phase 0.959 — Human-in-the-Loop and Intervention

## Status

**Ahead-of-roadmap implementation state — execution intervention slices 1 and 2 and the learning-candidate intervention slice are verified; the phase is not the current milestone.**

The repository intentionally retains intervention implementation that was built before the ordered foundational phases 0.954–0.958 and 0.9591 were completed. That implementation is treated as ahead-of-roadmap work, not as permission to skip the foundations.

The bounded approval/defer workflow from 0.953 is the starting point for this phase. Phase 0.959 generalizes that boundary into one provider-neutral intervention model rather than creating a parallel approval subsystem.

## Goal

Allow authorized humans or host applications to inspect, pause, resume, approve, reject, redirect, or otherwise intervene in agent behavior without bypassing the HAgent execution and policy model.

## Requirements

1. [x] Define a provider-neutral intervention/approval request and lifecycle model.
2. [ ] Support inspect, approve, reject, pause, resume, cancel, retire, shutdown, redirect, and defer actions where applicable.
3. [ ] Allow intervention at execution, tool, plan-step, goal, learning-candidate, and consequential-action boundaries. Execution and learning-candidate boundaries are implemented ahead of roadmap; plan-step, goal, and consequential-action coverage remain future work.
4. [x] Preserve who requested and who approved/rejected an intervention through identity and trace metadata.
5. [x] Make intervention policy-driven rather than prompt-driven.
6. [ ] Ensure an intervention cannot bypass permissions, authorization, budgets, capability requirements, or host-side validation at every intervention target.
7. [x] Define behavior when intervention arrives while work is executing, waiting, completing, or concurrently changing state.
8. [x] Support operator comments/reasons as bounded metadata without treating them as trusted executable instructions.
9. [ ] Expose intervention state through management UI and diagnostics.
10. [x] Add deterministic Example verification for the currently implemented approval, rejection, pause/resume, cancellation, concurrent intervention, stale intervention, and learning-candidate scenarios. Final phase-wide coverage remains pending for all supported target types.

## Verified ahead-of-roadmap slices

### Slice 1 — Execution intervention control

Verified through the public `HAgentClient` intervention API and deterministic local provider adapter:

- execution intervention requests capture target/action/correlation metadata;
- approved pause requests reach `Completed` only after the execution-control transition is applied;
- paused executions remain incomplete until explicit resume;
- approved resume requests restore execution;
- approved cancellation produces terminal `Cancelled` execution state;
- late provider responses cannot overwrite a terminal cancelled execution.

### Slice 2 — Concurrency and stale-state hardening

Verified through deterministic local execution scenarios:

- an intervention created before terminal completion becomes `Expired` rather than acting retroactively;
- target control state/version is captured when the request is created;
- conflicting concurrent intervention requests serialize so only one transition applies and the stale request expires;
- duplicate responder resolution cannot apply a second transition;
- a paused execution remains blocked until an explicit fresh resume intervention is approved.

### Slice 3 — Learning-candidate intervention

Verified in `HAgent.Example` through the public `HAgentClient` intervention API:

- approval moves `PendingReview -> Approved`;
- rejection moves `PendingReview -> Rejected`;
- target state and candidate revision are captured at request creation;
- an approval made stale by candidate rejection resolves as `Expired`;
- concurrent approve/reject requests produce one applied intervention and one stale request;
- a proposed candidate can be rejected through the same intervention lifecycle.

This does not make the whole 0.959 phase complete. Plan-step, goal, consequential-action, durable persistence, management UI, and final cross-target verification remain future work after the preceding roadmap phases are completed.

## Implementation sequence

1. Stabilize the Core intervention request/lifecycle contract using the current approval/defer implementation as its foundation.
2. Integrate intervention application at the execution and tool boundaries without introducing a second execution engine.
3. Add deterministic concurrency/stale-request handling before durable persistence.
4. Extend the same contract to plan steps, goals, learning candidates, and consequential actions.
5. Add durable persistence for intervention state.
6. Add management UI and diagnostics over the canonical intervention state.
7. Complete deterministic Example verification for all supported transitions and race-sensitive cases.

The detailed target lifecycle, target model, concurrency rules, persistence boundary, and management UI behavior are defined in `docs/architecture/92-human-intervention.md` and are authoritative for subsequent implementation work.

## Relationship to 0.9591 Goal/Plan Persistence and Recovery

The ordered roadmap places 0.9591 before 0.959. Durable goals, intentions, plans, plan-step revisions, checkpoints, and recovery state provide the persistent authority that later goal/plan intervention can govern safely. Execution-level intervention remains useful independently, so the existing ahead-of-roadmap execution intervention implementation can remain in source while the ordered roadmap catches up.

## Architectural outcome

```text
Agent Runtime
     ↕
Intervention Boundary
     ↕
Human / Authorized Host
```

Intervention controls agent operation; it does not become a second execution engine. Approval or intervention acceptance never by itself grants authorization, bypasses capability/budget checks, or resumes protected work without the owning runtime boundary applying the requested transition.

## Verification rule

A requirement becomes complete only when the implementation exists, deterministic Example verification passes locally, and the architecture/roadmap documentation reflects the verified behavior. Do not claim local build/test success without actually performing it.

## Phase 0.9592 — Provider Ecosystem and Adapter Lifecycle

## Status

**Planned provider-platform foundation before and alongside Phase 0.96.**

## Goal

Mature the provider adapter boundary so HAgent can support many providers, API variants, models, modalities, discovery mechanisms, and provider API versions without leaking provider-specific behavior into HAgent.Core.

## Requirements

1. [ ] Define a complete provider adapter lifecycle including registration, validation, initialization, refresh, health, disablement, replacement, and retirement.
2. [ ] Separate transport capability from discovery, usage, quota/rate, health, and other provider-specific data sources.
3. [ ] Define normalized adapter contracts for model discovery, capability discovery, usage, rate/quota information, health, and supported execution features where available.
4. [ ] Allow one provider integration to expose multiple models and task families without hard-coded model assumptions in Core.
5. [ ] Preserve provider-native identifiers, API versions, deployment identifiers, and endpoint metadata alongside normalized identities.
6. [ ] Support partial provider implementations: a provider may support execution while exposing incomplete discovery or quota telemetry.
7. [ ] Represent unavailable/unknown provider features explicitly instead of manufacturing defaults.
8. [ ] Define adapter version/compatibility metadata so provider API changes can be handled deliberately.
9. [ ] Support provider deprecation/retirement without corrupting persisted agent configuration or historical execution records.
10. [ ] Keep provider-specific retry, response, streaming, authentication, and error handling inside adapters where appropriate.
11. [ ] Ensure adapter instances are safe for concurrent use or explicitly scoped when they are not.
12. [ ] Ensure provider credentials are supplied through the current simple encrypted provider-configuration mechanism; this phase must not introduce a separate secret-vault architecture.
13. [ ] Add deterministic fake-provider verification for complete discovery, partial discovery, unsupported operations, provider/API version changes, adapter replacement, health changes, and concurrent usage.

## Architectural outcome

```text
Provider Configuration
        ↓
Provider Adapter
 ├── execution
 ├── discovery
 ├── capabilities
 ├── usage/quota
 ├── health
 └── provider-specific metadata
        ↓
Normalized HAgent contracts
        ↓
Execution Planner / Runtime
```

HAgent.Core remains provider-neutral; provider-specific knowledge stays behind adapter boundaries.

## Phase 0.96.x — Configuration, Storage, and Portability Evolution

## Status

**Required cross-cutting work for Phase 0.96 capability-aware execution and the later 0.97 persistent cognitive runtime.**

## Goal

Evolve HAgent persistence so the new provider/model selection architecture, capability-aware execution, agent policies, resource relationships, global settings, and configuration portability can be stored consistently across the File, SQL Server, and MySQL backends.

The storage design should remain deliberately simple. HAgent configuration is HAgent-owned data. Provider API keys are persisted with provider configuration and encrypted at rest; there is no separate secret-reference, vault, or centralized secret-provider architecture.

The same database-backed configuration can be consumed by multiple HAgent processes/machines, allowing a network deployment to share providers, models, agents, skills, knowledge, policies, and credentials without configuring each client independently.

## Storage architecture direction

```text
HAgent Configuration
├── General/system settings
├── Providers
│   ├── connection configuration
│   └── encrypted API key
├── Models
├── Concrete execution targets
├── Discovery metadata/evidence
├── Capability state
├── Constraints
├── Quota/rate/capacity state
├── Agents
│   ├── selection mode
│   ├── requirements/preferences
│   └── fallback/cost policy
├── Skills
├── Knowledge / Wiki
├── Memory configuration/policy
├── Learning configuration
├── Tools
├── Permissions
└── resource relationships

Configuration Portability
├── versioned export package
├── import/compatibility validation
├── explicit conflict handling
└── optional encrypted credential bundle
```

## Provider credentials

1. [ ] Replace the current conceptual requirement that provider credentials live only in a separate secret store with direct provider configuration persistence.
2. [ ] Add an `ApiKey`-style provider credential field to the authoritative provider configuration contract where the provider uses an API key.
3. [ ] Encrypt provider API keys at rest before writing them to File, SQL Server, or MySQL persistence.
4. [ ] Keep the encryption mechanism simple, documented, deterministic for the supported deployment model, and independent of provider-specific logic.
5. [ ] Ensure decrypted credentials are available to provider adapters only when constructing provider execution requests.
6. [ ] Redact provider credentials from diagnostics, logs, audits, discovery evidence, planner assessments, exceptions, and UI diagnostic output.
7. [ ] Support credential replacement/removal so revoking a provider credential only requires updating/removing the persisted configuration and refreshing active snapshots.
8. [ ] Remove the requirement for `SecretReference`-based provider persistence from the new architecture.
9. [ ] Retire or simplify `ISecretStore` usage as part of implementation; it must not remain an unnecessary parallel source of truth for provider credentials.
10. [ ] Preserve runtime-only handling of storage-server connection passwords where appropriate; do not place database connection passwords into ordinary provider configuration records.

## Global configuration persistence

11. [ ] Persist the system-wide `General` configuration described by the architecture, including at minimum Cost Policy, default AI selection mode, default fallback policy, default Learning Mode, and discovery/refresh defaults.
12. [ ] Support explicit inherit/override semantics for settings that may be overridden at Agent or runtime/host scope.
13. [ ] Persist effective policy inputs without mutating global defaults when an Agent or runtime override is applied.
14. [ ] Version configuration records so cache/snapshot invalidation can detect changes reliably.

## Provider and model persistence

15. [ ] Redesign the provider persistence model so Provider is independent from Model and concrete Execution Target.
16. [ ] Remove obsolete permanent Agent `ProviderId`/`ProviderIds` and model-binding storage from the new design rather than preserving legacy fields unnecessarily.
17. [ ] Persist normalized logical-model records where logical identity can be established.
18. [ ] Persist provider-native model identifiers separately from logical-model identity.
19. [ ] Persist concrete execution targets with provider, endpoint/account/project, deployment/model identity, version/revision where available, and routing/deployment identity.
20. [ ] Persist execution-target commercial state: `Free`, `FreeWithinQuota`, `Paid`, or `Unknown`.
21. [ ] Persist discovery metadata including verification time, source/provenance, confidence where applicable, and refresh/expiration information.
22. [ ] Persist capability evidence and normalized tri-state capability state: `Supported`, `Unsupported`, `Unknown`.
23. [ ] Persist normalized request/target constraints such as context limits, output limits, modality restrictions, schema limitations, and provider-specific values through extensible metadata where needed.
24. [ ] Persist operational state separately from capability: availability/health, quota, rate, concurrency/capacity, reset information, and observed remaining capacity.
25. [ ] Distinguish configured/manual overrides from provider-discovered/observed values so refresh does not silently erase administrator intent.
26. [ ] Permit unknown discovery data without requiring fake defaults. Unknown must remain a valid persisted state.
27. [ ] Preserve multiple execution targets for the same logical model across different providers/accounts/projects/endpoints.

## Agent policy persistence

28. [ ] Persist Agent AI selection mode: `Auto`, `Preferred`, or `Fixed`.
29. [ ] Persist preferred provider/model/execution-target settings without treating them as permanent execution bindings.
30. [ ] Persist fixed execution-target selection when the administrator intentionally chooses Fixed mode.
31. [ ] Persist capability requirements and preferences, including required/preferred/optional/forbidden semantics.
32. [ ] Persist fallback/degradation policy.
33. [ ] Persist Agent cost-policy inheritance/override and effective policy inputs.
34. [ ] Persist runtime tri-state capability overrides separately from the reusable Agent profile.
35. [ ] Ensure execution snapshots contain resolved configuration versions so changes after execution start cannot alter active work.

## Resource and relationship persistence

36. [ ] Extend persistence for Skills, Knowledge/Wiki, Memory policy, Learning configuration, Tools, Permissions, and their Agent/runtime relationships.
37. [ ] Support reusable Skill definitions and versions without embedding executable handlers in persistence.
38. [ ] Support Knowledge/Wiki resources independently from Skill storage while allowing explicit Agent access relationships.
39. [ ] Support extensible resource/type identity so future resource categories can be stored and inventoried without hard-coded Agent columns.
40. [ ] Persist Agent/resource relationships with explicit scope and enabled/disabled state where required.
41. [ ] Preserve Learning candidate provenance, source execution/runtime identity, target scope, and evidence/confidence.

## Runtime and cache coordination

42. [ ] Add change/version metadata sufficient for long-lived runtime configuration snapshots.
43. [ ] Support cache invalidation when provider configuration, model/discovery metadata, capabilities, permissions, global settings, or Agent configuration changes.
44. [ ] Avoid reloading unchanged Agent/provider configuration from persistence on every execution when a valid runtime snapshot exists.
45. [ ] Ensure database-backed HAgent instances can safely observe shared configuration changes across processes/machines.
46. [ ] Define a lightweight refresh/invalidation strategy appropriate for File, SQL Server, and MySQL without requiring a distributed cache service.
47. [ ] Prevent stale configuration snapshots from being used indefinitely after a relevant configuration revision changes.

## Configuration export/import

48. [ ] Define a versioned HAgent configuration package format independent of the physical storage backend.
49. [ ] Export all HAgent-owned configuration that can be recreated on another deployment, including General settings, Providers, Models, execution targets, Agents, Skills, Knowledge/Wiki, Memory configuration/policy, Learning configuration, Tools, Permissions, capability/resource relationships, and relevant metadata.
50. [ ] Exclude executable tool handlers, live runtime objects, active executions, synchronization primitives, transient provider sessions, raw HTTP state, and other process-local state from portable configuration.
51. [ ] Support normal export without provider credentials by default.
52. [ ] Support explicit credential-bearing export for administrators who choose to move credentials with the configuration.
53. [ ] Encrypt included API keys inside a credential-bearing export package.
54. [ ] Protect credential-bearing exports with a basic user-supplied password/encryption mechanism; do not introduce a separate secret-vault architecture.
55. [ ] Validate package format/version compatibility before import.
56. [ ] Provide explicit conflict behavior for existing IDs, names, providers, models, skills, knowledge resources, and other imported objects.
57. [ ] Ensure import restores credentials into the normal encrypted-at-rest provider configuration of the selected storage backend.
58. [ ] Ensure export/import preserves authoritative IDs and relationships when possible while providing deterministic remapping when conflicts require new IDs.
59. [ ] Support round-trip export/import verification with the File, SQL Server, and MySQL backends.

## Multi-machine database deployment

60. [ ] Treat SQL Server and MySQL configuration storage as centrally shared HAgent configuration for all authorized HAgent processes connected to that database.
61. [ ] Ensure provider API keys stored in the shared database are usable by authorized execution processes after decryption.
62. [ ] Do not require each machine to maintain a separate provider API-key copy when using shared database-backed HAgent configuration.
63. [ ] Ensure configuration refresh/version checks prevent one machine from continuing to use a revoked or replaced provider credential indefinitely.
64. [ ] Preserve HAgent database isolation: shared HAgent storage remains an HAgent-owned database and must not become a gateway into the host application's business database.

## File, SQL Server, and MySQL parity

65. [ ] Define one logical configuration/storage contract and maintain equivalent behavior across File, SQL Server, and MySQL implementations.
66. [ ] Add ordered schema migrations for SQL Server and MySQL covering the redesigned provider/agent/model configuration and new resource/policy records.
67. [ ] Keep provider-specific SQL differences isolated to storage implementation/migrations; HAgent.Core remains provider-neutral.
68. [ ] Add File persistence equivalents for the same authoritative configuration concepts so File mode does not become a second architecture.
69. [ ] Ensure the selected backend can persist the configuration required by Phase 0.96 and Phase 0.97 without depending on a host business database.

## UI implications

70. [ ] Update `Providers` UI to edit connection information and API key while exposing encryption/redaction behavior without exposing implementation details.
71. [ ] Update `Models` UI to display persisted/discovered model and execution-target metadata, capability evidence, limits, availability, cost state, and verification state.
72. [ ] Update `Agents` UI to edit the new selection policy instead of obsolete permanent ProviderId/Model fields.
73. [ ] Add configuration export/import management UI, including package type, credential-inclusion choice, password/protection flow, compatibility validation, conflict preview, and import result summary.
74. [ ] Make it clear in the UI that credential-bearing export is an explicit action and normal export does not include API keys.
75. [ ] Keep the user-facing UI organized around General, Providers, Models, Agents, Tools, Permissions, Storage, and related resource-management surfaces rather than exposing storage internals.

## Migration strategy

Because HAgent is still in active build/test and legacy configuration does not require preservation, this evolution should favor direct model replacement over a large backward-compatibility layer.

76. [ ] Remove obsolete Agent provider/model fields from the authoritative model and schema.
77. [ ] Remove obsolete provider-secret-reference assumptions from the new provider persistence path.
78. [ ] Add new schema versions/migrations as needed for the redesigned model without introducing compatibility tables solely for retired fields.
79. [ ] Update File, SQL Server, and MySQL serialization/persistence together so the backends remain behaviorally aligned.
80. [ ] Update Example verification and management UI against the new storage contracts before marking the architecture transition complete.

## Verification

81. [ ] File, SQL Server, and MySQL can persist and reload the same logical configuration model.
82. [ ] Two independent HAgent processes using one database observe the same provider, model, agent, skill, knowledge, and General configuration.
83. [ ] A stored API key is encrypted at rest and is not emitted by diagnostics/audit/logging paths.
84. [ ] Updating/removing a provider API key is reflected after configuration snapshot refresh/invalidation.
85. [ ] Same logical model with different provider/account cost, capability, quota, and health state remains represented as distinct execution targets.
86. [ ] Auto, Preferred, and Fixed Agent selection policies round-trip correctly through persistence.
87. [ ] General Cost Policy and Learning defaults round-trip correctly and preserve inherit/override semantics.
88. [ ] Export without credentials contains no API keys.
89. [ ] Credential-bearing export contains encrypted credentials and requires the export protection mechanism to import them.
90. [ ] Export/import round-trips providers, models, execution targets, agents, skills, knowledge/wiki, memory policy, learning configuration, tools, permissions, and relationships.
91. [ ] Import detects incompatible package versions and reports deterministic conflicts rather than silently overwriting unrelated configuration.
92. [ ] Running executions use immutable snapshots even when another process edits/deletes the underlying configuration.

## Architectural outcome

After this evolution, HAgent storage should conceptually look like:

```text
                 HAgent Configuration
                         │
          ┌──────────────┴──────────────┐
          │                             │
      File backend                Database backend
                                      │
                               SQL Server / MySQL
                                      │
                          shared by authorized HAgent
                              processes/machines

Provider
  ├── connection metadata
  └── encrypted API key

Model
  ├── logical identity
  └── provider-native identities

Execution Target
  ├── provider/account/project/endpoint
  ├── model/deployment
  ├── capabilities/evidence
  ├── constraints
  ├── quota/rate/capacity
  ├── health/availability
  └── cost state

Agent
  ├── selection policy
  ├── requirements/preferences
  ├── fallback
  ├── cost policy
  └── resource relationships

Export / Import
  └── versioned portable representation of the same authoritative configuration
```

The storage layer remains an implementation boundary. Provider routing, cognitive planning, and execution behavior consume normalized contracts rather than knowing whether the source was a JSON file, SQL Server, or MySQL.

## Phase 0.96 — Capability-Aware Execution

## Status

**Planned next — provider/runtime hardening before resuming Phase 0.10.**

## Goal

Make HAgent safe and useful across heterogeneous providers, accounts, deployments, models, modalities, quotas, rate limits, concurrency limits, and very different inference latencies without binding an agent profile to one provider/model.

Real provider testing exposed that a provider may expose many models with different capabilities and limits, while the same logical model may be available through multiple providers. HAgent must therefore reason about the **actual execution target** for each request rather than treating provider or model names as sufficient capability descriptions.

## Core model

```text
Agent Profile
    = what the agent is, what it requires, and what it prefers

Provider
    = a provider/service integration

Provider Account / Project / Endpoint
    = an operational execution environment

Model / Logical Model
    = provider-independent model identity where it can be established

Model Deployment / Execution Target
    = a concrete provider + account/project/endpoint + model deployment

Capability
    = what the execution target can do

Constraint
    = request/model limits such as context, output, image count, or schema size

Quota / Rate Limit
    = operational consumption limits over windows

Availability / Health
    = whether the target can accept work now

Cost / Billing Mode
    = free allocation, free-within-quota, paid, or unknown for the applicable provider/account/target

Execution Planner
    = selects the best currently compatible execution target
```

**Architectural terminology rule:** the **Execution Planner** is not the cognitive planner. The Execution Planner answers *where/how should an already-requested inference execute?* The cognitive **Planner** in Phase 0.97 answers *what should the agent do?* These layers must remain independent even when both perform candidate selection and scoring.

## Requirements

1. [ ] Remove permanent provider/model binding from reusable Agent profiles. Replace the obsolete binding with preference/requirement semantics in the new Agent model; do not add compatibility fields solely to preserve the retired design.
2. [ ] Preserve Agent profiles as first-class configuration containing identity, instructions, tools, memory/knowledge/skills policy, capability requirements, execution preferences, and fallback/degradation policy.
3. [ ] Represent Provider independently from Model and from concrete execution endpoint/account/project/deployment.
4. [ ] Introduce provider-independent logical model identity where reliably known, while preserving provider-native model identifiers and deployment identity.
5. [ ] Allow the same logical model to be exposed by multiple providers, including different endpoints/accounts/projects, without treating those executions as equivalent.
6. [ ] Define normalized execution-target identity covering provider, endpoint/account/project, model identifier, model version/revision where available, and relevant routing/deployment identity.
7. [ ] Extend the existing tri-state capability system to exact execution targets: `Supported`, `Unsupported`, `Unknown`.
8. [ ] Separate capability from operational state, account/project permission, quota, rate limiting, health, and request-specific constraints.
9. [ ] Model input/output modalities explicitly rather than using one generic vision/image flag. Support extensibility for text, image, audio, video, embeddings, generation, understanding, and future modalities.
10. [ ] Represent capability evidence, confidence, source, observation time, and expiration/refresh information.
11. [ ] Support capability evidence from provider metadata, provider documentation supplied through adapters, discovery APIs, controlled probes, successful executions, failures, and response metadata.
12. [ ] Cache capability knowledge without treating stale or undocumented capability data as authoritative.
13. [ ] Define request-side capability requirements independently from agent identity. Requirements must support at least required, preferred, optional, and forbidden semantics.
14. [ ] Support requirements for structured output, strict structured output, tool calling, reasoning, modalities, streaming, embeddings, and future capabilities.
15. [ ] Distinguish native capability from emulated/degraded behavior. Do not report prompt-based JSON fallback as equivalent to native constrained structured output.
16. [ ] Make fallback/degradation policy explicit: fail, wait, try another candidate, or use an explicitly permitted degraded mode.
17. [ ] Validate manually selected provider/model/execution targets against request and agent requirements before sending provider requests.
18. [ ] Introduce a capability-aware Execution Planner that evaluates candidate execution targets before transport.
19. [ ] Score/filter candidates by required capabilities, preferred logical model, preferred provider, explicit host/runtime selection, policy, availability, limits, expected latency, and other execution preferences without mutating the Agent profile.
20. [ ] Keep provider-native transport behind the existing `ProviderExecutionRequest` boundary.
21. [ ] Introduce normalized generic rate/quota dimensions rather than hard-coding only RPM/RPD/TPM/TPD.
22. [ ] Support at minimum request count, input tokens, output tokens, total tokens, concurrency, and future dimensions such as audio duration, image count, bytes, spend, or provider-specific units.
23. [ ] Support arbitrary windows including per-minute, per-day, and provider-specific/custom windows.
24. [ ] Support limits at the scope actually enforced by a provider, including account, organization, project, endpoint, model/deployment, or other documented scope.
25. [ ] Distinguish configured limits from observed remaining capacity and provider-reported reset information.
26. [ ] Parse provider rate-limit and retry metadata where available and reconcile observed state with HAgent's admission state.
27. [ ] Implement proactive rate/quota admission before provider transport so HAgent does not intentionally discover ordinary limits by sending doomed requests.
28. [ ] Implement atomic reservation/admission for concurrent executions so two requests cannot both consume the same remaining budget.
29. [ ] Reconcile reservations with actual provider usage after execution, including partial/unknown usage when provider telemetry is incomplete.
30. [ ] Support `Wait`, `TryNextCandidate`, `Fail`, and explicitly policy-controlled degraded behavior when capacity is insufficient.
31. [ ] Support maximum queue/admission wait so a theoretically available future target does not cause unbounded waiting.
32. [ ] Treat provider `429`, throttling, exhaustion, and quota failures as feedback for the operational state rather than as the only capability discovery mechanism.
33. [ ] Track request latency and execution duration separately from rate/quota state.
34. [ ] Model long-running inference targets where a single request may legitimately take minutes without treating slow response as provider failure.
35. [ ] Ensure caller cancellation, timeout, and late-result protection remain correct while a long-running request is waiting, executing, or being retried/fallback-routed.
36. [ ] Support explicit concurrency capacity such as one-at-a-time or bounded in-flight requests for providers/models that have limited serving capacity even when daily quota is high or unlimited.
37. [ ] Distinguish `quota available` from `execution capacity available`. A target may have abundant daily quota but still require serialization or waiting because inference is slow or concurrency-limited.
38. [ ] Provide target health/availability state and backoff hints without permanently blacklisting a provider because of transient failures.
39. [ ] Preserve provider neutrality: do not hard-code Groq, Cloudflare, NVIDIA, OpenRouter, or any other provider's model matrix into HAgent.Core.
40. [ ] Allow providers to supply provider-specific discovery/capability adapters while HAgent.Core consumes only normalized contracts.
41. [ ] Support arbitrary OpenAI-compatible endpoints whose capabilities may be partially known or completely unknown.
42. [ ] Handle providers with multiple task families and model catalogs, including text generation, image generation, image-to-text, embeddings, speech, classification, and future task types.
43. [ ] Preserve independent provider/model capability snapshots for multiple environments even when the logical model name is identical.
44. [ ] Expose enough planner diagnostics for a host/UI to explain why a candidate was accepted, rejected, delayed, or degraded.
45. [ ] Add deterministic Example verification for identical logical models exposed through multiple providers, required/preferred/optional capabilities, unknown capabilities, incompatible manual selection, structured-output native vs fallback behavior, proactive rate limiting, daily quota, token windows, atomic concurrent reservations, 429 feedback, long-running requests, cancellation, timeout, stale-result protection, and candidate fallback.
46. [ ] Update management UI targets so provider/model selection shows effective capabilities, constraints, quota/rate state, availability, and compatibility with the active request rather than only listing model names.
47. [ ] Ensure the Workspace/provider/model selection planned for Phase 0.10 consumes this capability planner rather than bypassing it.
48. [ ] Define provider/model discovery as **discovery-first**: users normally configure credentials, endpoint/account/project information, and optional provider-specific connection settings; HAgent discovers model catalogs, capabilities, constraints, operational limits, availability, and other metadata whenever the provider exposes them.
49. [ ] Support providers that expose complete model catalogs, partial catalogs, no catalog API, or arbitrary OpenAI-compatible endpoints. Discovery failure must degrade to explicit `Unknown` information rather than requiring a large mandatory manual metadata form.
50. [ ] Define model metadata provenance so each discovered fact can identify whether it came from provider metadata, discovery, documentation, controlled probe, successful execution, response metadata, host-supplied override, or unknown source.
51. [ ] Model commercial state separately from technical capability. Cost status must support at least `Free`, `FreeWithinQuota`, `Paid`, and `Unknown`, and must be associated with the applicable provider/account/plan/execution target rather than treated as an intrinsic property of a logical model.
52. [ ] Support a system-wide **Cost Policy** with at least `FreeOnly`, `FreePreferred`, and `NoRestriction` behavior. Cost policy is a selection/admission policy and must not be implemented as a simple model list filter.
53. [ ] Allow agents and approved runtime/host scopes to inherit or override cost policy without mutating the global default or persistent agent profile unexpectedly.
54. [ ] Ensure `FreeOnly` considers only targets whose applicable commercial state is known to be free/free-within-policy; `Unknown` cost must not silently qualify as free.
55. [ ] Define agent AI selection mode with at least `Auto`, `Preferred`, and `Fixed`.
56. [ ] In `Auto`, HAgent selects a compatible execution target according to capabilities, policy, availability, cost, quota/capacity, latency, and preferences.
57. [ ] In `Preferred`, HAgent attempts the configured provider/model/deployment preference but may use another compatible target according to the configured fallback policy.
58. [ ] In `Fixed`, HAgent uses the selected target when permitted and compatible; it must still enforce authorization, required capabilities, constraints, quota/capacity, and health. If unavailable or incompatible, behavior follows an explicit fallback policy and must never silently bypass enforcement.
59. [ ] Distinguish a specific fixed target from a preference such as highest quality, lowest latency, lowest cost, or balanced. Preference scoring remains planner-owned rather than being encoded as a hard provider/model dependency.
60. [ ] Add deterministic Example verification for provider discovery success/partial failure, unknown metadata, free-only selection, free-preferred fallback, paid-only targets under restriction, same model with different cost status across providers, Auto/Preferred/Fixed agent selection, fixed-target incompatibility, and explicit fallback behavior.

## Management UI direction

The HAgent.WinForms configuration surface should be organized around user responsibilities rather than internal planner terminology.

Top-level configuration tabs:

```text
Overview
General
Providers
Models
Agents
Tools
Permissions
Storage
Storage Test
About
```

### General

`General` contains system-wide defaults and policies that apply across providers and agents unless overridden by a more specific scope.

At minimum it should contain:

```text
Execution Defaults
    Cost Policy: FreeOnly / FreePreferred / NoRestriction
    Default AI Selection: Auto
    Default Fallback Policy
    Default timeout/concurrency policies where appropriate

Learning Defaults
    Default Learning Mode

Discovery
    Automatically discover models: On/Off
    Automatically refresh provider metadata: On/Off
```

Global settings are defaults, not forced values. Agent and approved runtime/host configuration can explicitly inherit or override them according to policy.

### Providers

The normal provider setup experience should be lightweight:

```text
Provider
    Name
    Provider/API type
    Base URL where applicable
    Credentials/secrets
    Account/project information where applicable

[ Test Connection ]
[ Save ]
```

A provider should not require the user to manually enter every model, capability, modality, quota, rate limit, or technical restriction when those values can be discovered or observed. Provider-specific advanced/manual overrides may exist for information that cannot be discovered, but these are exception paths.

The provider surface should also expose discovery status, last refresh, connection status, and a way to refresh/retest discovered metadata.

### Models

`Models` is a first-class top-level management surface between Providers and Agents. It displays the HAgent model catalog built from discovered provider information and normalized runtime observations.

The catalog should support at least:

```text
Model / logical model
Provider / execution target
Availability
Cost status: Free / FreeWithinQuota / Paid / Unknown
Capabilities
Constraints/limits
Quota/rate/capacity state where available
Last verified / evidence source
```

The UI should group the same logical model across multiple providers when their identities can be correlated, while keeping each concrete execution target distinct. A model may therefore appear as free at one provider and paid/unknown at another.

When a provider cannot expose complete metadata, the Models surface must show `Unknown` rather than inventing a value and offer appropriate refresh/probe/manual-override actions where supported.

### Agents

Agent Configuration must make agent intent clear without requiring the administrator to understand execution-planner internals.

The Agent Editor should include at least:

```text
Overview
General
AI
Skills
Knowledge
Memory
Learning
Advanced
```

#### AI selection

The AI section should support:

```text
Selection Mode
    Auto
    Preferred
    Fixed

Provider
    Auto or selected provider

Model
    Auto or selected logical/concrete model

Required capabilities
    capabilities this agent must have

Preferred capabilities/preferences
    capabilities/quality/latency/cost preferences

Fallback policy
    explicit behavior when preferred/fixed selection cannot run
```

`Auto` is the normal default. `Preferred` expresses a strong preference without turning the profile into a permanent transport binding. `Fixed` gives administrators explicit control when they intentionally want one concrete target, such as always using a particular high-end model. Fixed selection never bypasses capability, authorization, quota, capacity, or health enforcement.

The agent UI should describe Fixed as a deliberate override and make the failure/fallback behavior visible rather than silently substituting another model.

#### Effective configuration overview

The selected agent should show an understandable summary of the effective state:

```text
AI selection
Skills
Knowledge
Memory
Learning
Tools
Cost Policy
```

Where a value is inherited, the UI should show the source and effective value. For example:

```text
Cost Policy
    Global: FreePreferred
    Agent: Inherit
    Effective: FreePreferred
```

This same pattern should be usable for runtime overrides where runtime configuration is exposed.

## Cost policy and free-model behavior

Cost policy is global by default but may be overridden at agent/runtime/host scopes where policy permits.

The intended behavior is:

```text
Global Cost Policy
      ↓
Agent Cost Policy
      ↓
Runtime/Host Override
      ↓
Execution Planner
      ↓
compatible execution targets
```

`FreeOnly` does not mean "pick any model labeled free." HAgent must still enforce capability requirements, permissions, constraints, quota/capacity, health, and other policy. A free target that cannot perform the requested task is not eligible.

`FreePreferred` prefers free/free-within-policy targets but can use paid targets only when the active higher-level policy explicitly permits paid fallback. `NoRestriction` does not impose a cost filter.

Unknown commercial status is never treated as free implicitly.

## Same model, multiple providers

The same logical model may appear through different providers:

```text
Logical Model: M

Groq       -> Deployment A -> FreeWithinQuota
OpenRouter -> Deployment B -> Free
Local      -> Deployment C -> Unknown
```

The deployments are separate execution targets because they may differ in capabilities, limits, pricing, routing, permissions, latency, availability, and operational state. HAgent may use the logical model as a preference while selecting among compatible concrete deployments.

## Capability and limitation layers

HAgent must keep these dimensions separate:

```text
Capability
    Can it do the operation?

Constraint
    Can it do this size/shape/version of the operation?

Permission
    Is this account/project allowed to use it?

Quota / Rate Limit
    Is there remaining budget in the applicable window?

Concurrency / Capacity
    Can it accept another request now?

Availability / Health
    Is the target currently usable?

Cost
    Is this target free, free-within-policy, paid, or unknown?

Latency
    How long may the operation reasonably take?
```

A target may therefore be capable but temporarily unavailable, available but incompatible with a required feature, or free but unusable because the needed capability or quota is unavailable.

## Execution-target assessment

The planner should expose a normalized assessment for every candidate considered:

```text
ExecutionTargetAssessment
    Target identity
    Compatible / incompatible
    Capability evidence
    Constraint checks
    Permission state
    Quota state
    Capacity state
    Health / availability
    Cost state
    Estimated latency
    Wait-until (optional)
    Degradation available (optional)
    Score / ranking information
    Decision reason
```

This assessment is diagnostic data, not merely logging. It allows hosts and management UI to explain why an execution target was accepted, rejected, delayed, or degraded without knowing provider-specific implementation details.

## Rate limiting and admission

Rate limiting is proactive admission control, not merely retry logic.

The intended flow is:

```text
Execution Request
      |
      v
Capability requirements
      |
      v
Candidate discovery/filtering
      |
      v
Policy/preferences scoring
      |
      v
Admission / reservation
      |
      +---- available -> ProviderExecutionRequest
      |
      +---- wait -> bounded queue/wait
      |
      +---- unavailable -> next candidate / fail / permitted degradation
```

The planner and admission layer must remain provider-neutral while provider adapters may contribute provider-specific metadata needed to normalize limits and responses.

## Storage and configuration relationship

Phase 0.96 depends on the storage evolution defined in `docs/roadmap/38-configuration-storage-and-portability.md`.

The capability-aware planner requires persistence for providers, logical models, concrete execution targets, discovery metadata, capability evidence, constraints, operational quota/rate/capacity state, cost state, Agent selection policies, global cost policy, and configuration versioning. The storage model must use the new contracts directly rather than adding legacy compatibility columns for the retired Agent provider/model binding.

Provider credentials are persisted with provider configuration and encrypted at rest. HAgent does not require a separate secret-reference or secret-vault architecture.

Configuration export/import is part of the platform foundation: portable configuration must include the new model/target/policy/resource graph, while executable process state and active executions remain non-portable.

## Phase 0.97 — Persistent Cognitive Runtime

## Status

**Planned after Phase 0.96 and before resuming higher-level autonomous-agent features.**

## Goal

Add a provider-neutral, long-lived cognitive runtime above individual HAgent executions while preserving the existing execution engine as the reusable foundation.

A runtime agent instance identifies and owns the lifecycle of a live agent. The Persistent Cognitive Runtime adds the higher-level machinery that lets that agent continuously exist as an autonomous process: it receives environmental events, maintains cognitive state, manages attention, owns goals and intentions, maintains plans, chooses between reactive and deliberative behavior, and decides when and how probabilistic reasoning is warranted.

The runtime is generic. It must work for business applications, automation, research systems, simulations, games, and other hosts without embedding domain-specific world models, schedulers, persistence engines, UI frameworks, or side-effect authority.

This phase does **not** assume that any single cognitive architecture is the final or correct architecture for LLM-based agents. HAgent must provide a stable cognitive substrate that can host multiple cognitive strategies and evolve as research improves.

## Architectural position

HAgent must support both request-oriented execution and persistent cognition. Persistent cognition is a layer above, not a replacement for, `ExecuteAsync`.

```text
HAgent
│
├── Execution Engine
│   ├── provider/model execution
│   ├── structured output
│   ├── tools
│   ├── memory/context integration
│   ├── retries / timeout / cancellation
│   └── capability-aware execution planning
│
├── Runtime Agent Instance
│   ├── identity
│   ├── lifecycle
│   ├── runtime overrides
│   ├── revision / stale-result protection
│   └── private-memory ownership
│
└── Persistent Cognitive Runtime
    ├── Cognitive Kernel
    │   ├── belief state
    │   ├── working state
    │   ├── attention / global workspace
    │   ├── goals / intentions
    │   ├── plans / operators
    │   ├── cognitive actions
    │   └── revision / learning state
    │
    ├── Cognitive Strategy Layer
    │   ├── Adaptive Hybrid Cognition (initial)
    │   └── future / experimental strategies
    │
    ├── event intake / activation
    ├── reactive cognition
    ├── deliberative cognition
    ├── reasoning-requirement assessment
    ├── memory / knowledge / skill integration
    └── execution requests
             │
             ▼
       Existing Execution Engine
```

The execution engine remains usable directly:

```text
Host
  -> AgentExecutionRequest
  -> HAgentClient.ExecuteAsync(...)
  -> AgentExecution
```

A persistent cognitive host uses the higher layer:

```text
Environment
  -> Event / Observation
  -> Cognitive Kernel
  -> Cognitive Strategy
  -> Attention / Belief / Goal / Plan evaluation
  -> Reactive action OR reasoning requirement
  -> Execution Planner
  -> Execution Engine
  -> Outcome / Observation
  -> Cognitive revision / Memory / Learning
  -> Host-owned side effect or environment action
```

## Core principles

1. [ ] Preserve request-oriented `ExecuteAsync` as a first-class public API. It remains the simplest integration path for callers that do not need autonomous cognition.
2. [ ] Build persistent cognition as an optional higher-level runtime rather than changing the semantics of ordinary execution.
3. [ ] Separate environment/simulation time, cognitive scheduling time, and LLM execution latency. None may be conflated.
4. [ ] Make event-driven activation the default. The runtime must not require an LLM call on every host tick or every incoming event.
5. [ ] Allow deterministic/reactive cognition to resolve routine situations without model execution.
6. [ ] Allow salience, novelty, uncertainty, goal relevance, urgency, risk, policy, and available evidence to influence whether and how deliberation is activated.
7. [ ] Treat the LLM as a replaceable reasoning component inside the cognitive runtime, not as the cognitive runtime itself.
8. [ ] Keep the cognitive kernel provider-neutral and avoid coupling cognitive state to any one model, provider, or reasoning style.
9. [ ] Preserve the host boundary: HAgent may decide what it wants to do, but the host remains authoritative over domain state, authorization, scheduling policy, and side effects.
10. [ ] Keep runtime state provider-neutral and serializable where persistence is explicitly enabled.
11. [ ] Ensure cognitive state changes are versioned and concurrency-safe so stale asynchronous executions cannot overwrite newer intent, belief, goal, or plan state.
12. [ ] Make activation, cognition, planning, execution, memory retrieval, learning, tool usage, and outcomes observable without requiring prompt inspection.
13. [ ] Do not treat any researched cognitive architecture as canonical. HAgent should adapt proven principles and leave room for new strategies and research-derived implementations.

## Cognitive kernel and strategy separation

The persistent runtime must distinguish a stable **cognitive kernel** from pluggable **cognitive strategies**.

The cognitive kernel owns durable semantics and safety-critical state such as identity, belief state, working state, goals, intentions, plans, revisions, persistence, provenance, resource boundaries, and execution correlation. A cognitive strategy decides how that state should be interpreted and progressed.

```text
Stable Cognitive Kernel
        │
        ├── Beliefs
        ├── Goals
        ├── Intentions
        ├── Plans / Operators
        ├── Attention / Global Workspace
        ├── Memory / Knowledge / Skills
        ├── Cognitive Actions
        └── Revision / Provenance
                │
                ▼
        Cognitive Strategy
                │
        ┌───────┴─────────────────────┐
        │                             │
  deterministic / hybrid        LLM-backed reasoning
        │                             │
        └──────────────┬──────────────┘
                       ▼
                cognitive decision
```

Requirements:

14. [ ] Define a provider-neutral `ICognitiveStrategy` boundary independent from provider/model execution.
15. [ ] Treat cognitive strategy as replaceable configuration/implementation rather than a hard-coded property of the runtime kernel.
16. [ ] Define strategy lifecycle, state compatibility, versioning, diagnostics, and safe replacement semantics.
17. [ ] Permit multiple cognitive strategies to coexist in the codebase without duplicating the runtime identity, persistence, memory, authorization, or execution engine.
18. [ ] Allow a future research-derived strategy to be implemented under its own explicit name without redesigning the cognitive kernel.
19. [ ] Make strategy selection observable and reproducible for evaluation.
20. [ ] Prevent a cognitive strategy from bypassing shared HAgent security, authorization, budget, persistence, or stale-result boundaries.

### Initial strategy: Adaptive Hybrid Cognition (AHC)

The first strategy should be explicitly named **Adaptive Hybrid Cognition (AHC)**.

AHC is not declared to be the final cognitive architecture. It is the first HAgent strategy and an experimental baseline built around the following principle:

> Use deterministic cognition whenever sufficient; escalate to probabilistic reasoning when necessary; select reasoning capability according to uncertainty, novelty, consequence, available evidence, goal relevance, constraints, and required capability; then incorporate validated outcomes back into persistent cognitive state.

Conceptually:

```text
Situation
   ↓
Existing deterministic cognition sufficient?
   │
   ├── yes → act / continue plan
   │
   └── no
        ↓
   progressive assessment
        ↓
   lightweight reasoning if sufficient
        ↓
   deeper deliberation if required
        ↓
   execution / observation
        ↓
   validated learning and cognitive revision
```

Requirements:

21. [ ] Implement AHC as the initial concrete cognitive strategy.
22. [ ] Make AHC capable of deciding that no LLM/model execution is required.
23. [ ] Make AHC capable of escalating from deterministic processing to lightweight or deeper probabilistic reasoning when evidence is insufficient.
24. [ ] Base escalation on multiple signals rather than a single opaque "complexity score".
25. [ ] Preserve a provider-neutral reasoning requirement between cognition and execution planning.
26. [ ] Make AHC's assessment and escalation reasons observable.
27. [ ] Keep AHC replaceable so later strategies can be evaluated against it rather than modifying the kernel to accommodate every new research idea.

## Cognitive runtime model

A persistent cognitive runtime should conceptually maintain:

```text
Identity
  Agent profile + runtime instance

Belief state
  Current beliefs about the environment, entities, agent state, and relevant assumptions

Working state
  Current situation, active focus, temporary assumptions, cognitive scratch state

Global workspace / attention
  Bounded set of information currently made available to cognitive processing

Goals
  Desired persistent outcomes with priority/deadline/status

Intentions
  Goals currently committed to pursue

Plan
  Ordered or partially ordered intended steps and operators

Memory
  Episodic, semantic, procedural, and other HAgent-managed stores

Knowledge / Skills
  Resources selected according to policy and capability

Relationships / collaboration context
  Optional generic references to external participants/workspaces

Activation state
  Awake, waiting, sleeping, blocked, deliberating, executing, or retired

Execution state
  Current execution correlation, revision, cancellation and result authority
```

The exact storage contracts must remain modular. A host may choose an in-memory runtime, persistent runtime state, or another HAgent storage implementation without changing the cognitive semantics.

## Belief state and belief revision

Persistent cognition requires a first-class distinction between observations and the agent's current beliefs about what those observations mean. A belief is a cognitive state assertion, not a statement of ground truth. Observations remain attributable to their sources and the host remains authoritative over domain truth.

```text
Observation / Event
        ↓
Interpretation
        ↓
Belief candidate
        ↓
Belief validation / confidence / provenance
        ↓
Current Belief State
        ↓
Goal / attention / plan consequences
```

Requirements:

28. [ ] Define provider-neutral belief identity, content/reference, provenance, confidence/quality metadata, timestamps/validity, scope, and revision information.
29. [ ] Distinguish host observations/events from inferred beliefs.
30. [ ] Support belief addition, reinforcement, weakening, contradiction, expiry/staleness, and removal/retraction.
31. [ ] Record why a belief changed: new observation, execution outcome, contradiction, learned evidence, policy, or deliberate revision.
32. [ ] Propagate belief invalidation to dependent goals, intentions, plan assumptions, and working state where configured.
33. [ ] Ensure stale or low-confidence beliefs cannot silently override newer higher-authority evidence.
34. [ ] Preserve belief provenance through downstream decisions and execution metadata.
35. [ ] Keep host-authoritative state outside the belief store when the host provides authoritative domain truth.

## Working memory, global workspace, and cognitive actions

HAgent should distinguish durable memory from bounded cognitive working state. A research-derived **GlobalWorkspaceFrame** may represent the small set of currently attended information exposed to the selected cognitive strategy.

```text
Long-term resources
   │
   ├── Memory
   ├── Knowledge
   └── Skills
          │
          ▼
   retrieval / relevance
          │
          ▼
   GlobalWorkspaceFrame
          │
          ▼
   DecisionContext / cognitive strategy
```

Requirements:

36. [ ] Define a bounded `GlobalWorkspaceFrame` or equivalent provider-neutral attended-state contract.
37. [ ] Make attention/working-state selection explicit rather than treating the LLM prompt as the only working memory mechanism.
38. [ ] Keep the workspace bounded by item count, size, budget, or policy.
39. [ ] Allow cognitive strategies to replace or extend workspace-selection policies.
40. [ ] Define provider-neutral internal cognitive actions distinct from external host/application tools.
41. [ ] At minimum allow conceptual internal actions such as recall/retrieve, create/revise goal, adopt/revise intention, create/revise plan, mark belief stale, request deliberation, wait, sleep, wake, and emit observation.
42. [ ] Ensure internal cognitive actions cannot bypass shared authorization, budget, provenance, or revision semantics.
43. [ ] Do not expose a false claim of machine consciousness; `GlobalWorkspaceFrame` is an engineering abstraction for bounded attended state.

## Event and activation model

The runtime must accept host-owned events and observations without assuming what the host calls them.

```text
Host event
   |
   v
Event normalization
   |
   v
Belief / working-state update
   |
   v
Attention / salience evaluation
   |
   +---- ignore / retain
   +---- update working state
   +---- run reactive cognition
   +---- activate deliberation
             |
             v
      reasoning requirement
             |
             v
       execution request
```

Requirements:

44. [ ] Define provider-neutral cognitive event/observation contracts with stable identity, source, timestamp, correlation/causation metadata, optional importance, and bounded payload/context.
45. [ ] Support event deduplication and bounded queues.
46. [ ] Support event expiry and retention policy.
47. [ ] Support explicit wake-up, scheduled wake-up, and event-triggered wake-up.
48. [ ] Support sleeping/idle behavior so an agent can remain persistent without continuously consuming model resources.
49. [ ] Support backpressure and bounded activation work under event storms.
50. [ ] Preserve event provenance into cognitive decisions and execution metadata.

## Attention and salience

Attention is a policy layer, not an implicit LLM prompt convention.

Requirements:

51. [ ] Define a generic attention/salience evaluation contract.
52. [ ] Support at least urgency, novelty, goal relevance, uncertainty, relationship relevance, risk/consequence, and policy-defined importance as inputs where supplied by the host/runtime.
53. [ ] Produce a bounded attended set or an explicit `no-deliberation-needed` outcome.
54. [ ] Allow deterministic attention rules before model execution.
55. [ ] Allow host/runtime policy to cap attention processing cost and queue growth.
56. [ ] Make the reason an event was promoted to deliberation observable.

## Reactive versus deliberative cognition

The runtime must support at least two behavioral paths:

```text
Event
  |
  +--> Reactive cognition
  |       |
  |       +--> action / state transition
  |
  +--> Deliberative activation
          |
          +--> context + memory + goals + attention
          |
          +--> reasoning requirement
          |
          +--> selected model execution when justified
          |
          +--> decision / plan update
```

Requirements:

57. [ ] Define a host-neutral reactive cognition boundary for deterministic behavior.
58. [ ] Define a deliberation activation policy that can decide when an LLM/model execution is justified.
59. [ ] Support explicit triggers such as goal failure, novelty, ambiguity, blocked progress, high-priority event, social interaction, contradiction, elevated risk, or host-requested deliberation.
60. [ ] Allow deliberation to be skipped when an existing plan can safely continue.
61. [ ] Allow re-deliberation when execution results contradict beliefs, violate plan assumptions, or invalidate the current plan.
62. [ ] Avoid encoding repeated motor prompts as the canonical persistent-agent architecture.

## Reasoning requirement and adaptive model selection

Cognition must not directly choose a provider-specific model. Instead it produces a provider-neutral **reasoning requirement** describing what kind of inference is needed. The existing Phase 0.96 execution-planning system then maps that requirement to an executable target.

The runtime should use progressive assessment rather than pretending it can predict exact problem complexity:

```text
Need inference?
   ↓
Can deterministic cognition resolve it?
   │
   ├── yes → continue / act
   │
   └── no
        ↓
   assess signals:
     uncertainty
     novelty
     evidence availability
     goal relevance
     consequence / risk
     planning horizon
     conflicting beliefs
     tool / context requirements
     budget / latency policy
        ↓
   define ReasoningRequirement
        ↓
   Phase 0.96 Execution Planner
        ↓
   appropriate provider/model/target
```

Requirements:

63. [ ] Define a provider-neutral `ReasoningRequirement` contract separate from `AiExecutionTarget`.
64. [ ] Support required/preferred reasoning capabilities without requiring a single scalar complexity classification.
65. [ ] Allow reasoning requirements to express qualities such as depth, context capacity, tool-use needs, structured-output needs, latency tolerance, cost policy, and other capability constraints supported by HAgent.
66. [ ] Support staged escalation from deterministic cognition to lightweight reasoning to deeper deliberation when configured.
67. [ ] Allow multiple assessments to be combined before selecting the final reasoning requirement.
68. [ ] Reuse Phase 0.96 capability-aware execution planning for concrete provider/model selection, quota, health, capacity, cost, and routing.
69. [ ] Keep provider-specific model knowledge out of the cognitive kernel and cognitive strategies.
70. [ ] Record why a stronger or weaker reasoning path was selected and whether escalation succeeded.
71. [ ] Support cases where the correct choice is explicitly **no model execution**.

## Goals, intentions, plans, operators, and impasses

The runtime should incorporate strong ideas from established cognitive architectures without reproducing any one architecture literally.

### Goals and intentions

Requirements:

72. [ ] Define generic goal identity, status, priority, constraints, provenance, and lifecycle.
73. [ ] Support multiple concurrent goals with explicit priority and conflict policy.
74. [ ] Define intention state separately from goal state.
75. [ ] Allow new goals to originate from host input, environmental triggers, existing goal decomposition, learned candidates, policy, or model proposals subject to validation/admission.
76. [ ] Allow goals to be revised, suspended, completed, failed, abandoned, or superseded with explicit reasons.

### Plans and operators

Requirements:

77. [ ] Define a plan model that can represent ordered steps, dependencies, checkpoints, failure conditions, completion criteria, and assumptions.
78. [ ] Define a provider-neutral operator/action concept with preconditions, intended effects, provenance, and execution requirements where appropriate.
79. [ ] Allow deterministic progress against known plan/operator steps without requiring a new LLM call for every step.
80. [ ] Allow a plan to be partially executed without losing its identity or revision history.
81. [ ] Allow plan revision, replacement, suspension, resumption, and abandonment.
82. [ ] Record why a plan changed: new evidence, failed precondition, failed action, higher-priority goal, external event, policy change, belief revision, or deliberate reconsideration.
83. [ ] Support bounded planning depth, step count, context size, and deliberation budget.
84. [ ] Keep plan/operator execution separate from domain side-effect authority; the host decides whether a requested action is valid and executable.

### Impasse and bounded deliberation

An **Impasse** represents a bounded state where the current deterministic cognition cannot safely or confidently continue, such as a missing operator, conflicting goals, failed precondition, contradictory beliefs, insufficient evidence, or policy/resource blockage.

Requirements:

85. [ ] Define an explicit provider-neutral `Impasse` concept with reason, affected state, provenance, severity, and resolution status.
86. [ ] Distinguish routine progress from genuine impasse rather than invoking an LLM for every uncertainty.
87. [ ] Allow an impasse to trigger bounded deliberation, alternative strategy selection, additional retrieval, waiting, or escalation to the host.
88. [ ] Support nested/bounded deliberative substates where a strategy requires temporary focused reasoning.
89. [ ] Ensure impasse resolution cannot silently invalidate newer cognitive revisions.
90. [ ] Observe impasse frequency, causes, resolution path, and recurrence.

## Cognitive revision and compare-and-apply

LLM proposals, learned candidates, plan revisions, and belief changes must be treated as **proposals against a versioned cognitive state**, not as direct mutation commands.

```text
Current cognitive revision N
          │
          ▼
      reasoning
          │
          ▼
  proposed cognitive changes
          │
          ▼
   compare against revision N
          │
      ┌───┴────┐
      │        │
 compatible  stale/conflicting
      │        │
      ▼        ▼
 validate    reject/re-deliberate
      │
      ▼
 Commit revision N+1
```

Requirements:

91. [ ] Define a provider-neutral cognitive revision identifier/version.
92. [ ] Represent model-produced decisions and learned changes as proposals against a specific cognitive revision.
93. [ ] Validate proposals against current beliefs, goals, plan assumptions, permissions, budgets, and applicable policy before applying them.
94. [ ] Apply compatible changes atomically where practical.
95. [ ] Reject or rebase/re-deliberate stale proposals rather than allowing last-writer-wins mutation.
96. [ ] Preserve change provenance and causation for every committed cognitive revision.
97. [ ] Support rollback or compensating revision for learned/experimental cognitive artifacts where safe.

## Memory, knowledge, skills, learning, and experience evolution

Persistent cognition must consume HAgent's existing resource layers rather than duplicate them. Learning should allow an agent to become more efficient over time without silently modifying the cognitive kernel.

```text
Experience
   ↓
Memory record
   ↓
reflection / analysis / validation
   ↓
Candidate:
   Knowledge | Skill | Policy | Goal heuristic | Cognitive improvement
   ↓
Evaluation / governance
   ↓
Versioned adoption
   ↓
Future cognition becomes more efficient / deterministic
```

Requirements:

98. [ ] Define how working cognitive state differs from persistent Memory records, Knowledge, Skills, policies, and conversation context.
99. [ ] Represent execution outcomes and environmental outcomes as attributable experience where appropriate.
100. [ ] Retrieve episodic/semantic/procedural resources selectively based on current attention, goal, plan, and reasoning requirements.
101. [ ] Avoid rebuilding full memory, wiki, skill, or conversation stores into every LLM request.
102. [ ] Allow runtime policy to choose which memory/knowledge/skill resources are eligible for an execution.
103. [ ] Support bounded retrieval and context budgets.
104. [ ] Allow successful repeated experience to produce versioned skill/policy candidates that can reduce future model dependence when validated.
105. [ ] Do not assume that every successful model trace can or should become deterministic code; support partial proceduralization and continued model use for unresolved portions.
106. [ ] Require validation, provenance, versioning, rollback, and explicit governance before learned candidates become trusted deterministic behavior.
107. [ ] Never allow learning to silently rewrite the cognitive kernel itself.
108. [ ] Distinguish at least Experience, Memory, Skill, Policy, and Cognitive-Strategy evolution as separate concepts.
109. [ ] Integrate Phase 0.11 governance contracts when available without making governance logic a cognitive-runtime-specific exception.
110. [ ] Preserve private versus shared resource boundaries and authorization rules.

## Execution integration

Persistent cognition must use the existing generic execution boundary rather than inventing a second provider path.

Requirements:

111. [ ] Create deliberative work through the canonical `AgentExecutionRequest` boundary.
112. [ ] Reuse Phase 0.96 capability-aware target planning for model/provider selection.
113. [ ] Preserve host correlation, execution identity, runtime-instance identity, cancellation, timeout, and stale-result protection.
114. [ ] Associate an execution with the cognitive activation, attended event set, reasoning requirement, goal, intention, and plan revision that caused it.
115. [ ] Prevent an obsolete execution from mutating current beliefs, goals, intentions, plans, or working state after superseding state exists.
116. [ ] Support execution budgets across time, model calls, tokens, tools, and host-defined units where available.
117. [ ] Support independent concurrency limits for cognitive runtimes while allowing unrelated runtimes to continue.
118. [ ] Support long-running deliberation without blocking host event ingestion.

## Context efficiency

The persistent runtime exists partly to eliminate unnecessary repeated prompting and context transfer.

Requirements:

119. [ ] Maintain bounded persistent working context rather than reconstructing the entire cognitive state for every execution.
120. [ ] Prefer state deltas, event summaries, belief changes, plan changes, and relevant memory retrieval over repeatedly sending unchanged state.
121. [ ] Cache stable execution environment information where safe, including resolved profile/resource configuration, with explicit invalidation/version rules.
122. [ ] Avoid reloading unchanged agent/provider configuration from persistence for every cognitive activation when a valid runtime snapshot already exists.
123. [ ] Rebuild or refresh cached execution configuration when persistent configuration, capability data, permissions, or runtime overrides change.
124. [ ] Expose diagnostics for context size, retrieved resources, estimated token cost, activation frequency, deliberation frequency, escalation frequency, latency, and cache refreshes.

## Persistence and recovery

125. [ ] Define an optional persistence contract for cognitive state independent from HAgent provider transport state.
126. [ ] Persist identity, beliefs, goals, intentions, plan state, activation state, bounded working state, cognitive revision, and recovery metadata when enabled.
127. [ ] Do not persist live tasks, cancellation tokens, synchronization primitives, raw provider sessions, secrets, or transient HTTP state as cognitive state.
128. [ ] Support restart recovery with explicit reconciliation of in-flight executions.
129. [ ] On recovery, invalidate obsolete execution authority and rebuild the runtime from the latest durable cognitive revision.
130. [ ] Support optional checkpoints to bound recovery cost.

## Collaboration and social extensibility

The runtime should be usable by applications that have multiple agents without hard-coding social semantics.

131. [ ] Allow an environment to provide participant/reference information to cognition through provider-neutral contracts.
132. [ ] Allow cognitive plans to request communication/delegation through generic tools or collaboration APIs.
133. [ ] Preserve sender, recipient, correlation, causation, and ordering metadata where collaboration systems provide them.
134. [ ] Keep domain-specific social rules outside the generic cognitive runtime.
135. [ ] Allow HAgent Workspaces to use persistent cognitive runtimes without making workspace participation mandatory.

## Observability and cognitive evaluation

Observability must evaluate not only whether the agent succeeded, but how cognition was achieved and whether the architecture improves over time.

136. [ ] Add lifecycle events for cognitive activation, belief changes, attention selection, goal changes, intention changes, plan creation/update/completion/failure, impasses, sleep/wake, deliberation start/end, learning proposals, and strategy changes.
137. [ ] Expose a structured cognitive trace separate from raw provider prompt/response logging.
138. [ ] Track activation-to-execution relationships and end-to-end latency.
139. [ ] Track reactive versus deliberative decisions and the percentage of events handled without model execution.
140. [ ] Track reasoning escalation paths and whether escalation improved decision quality.
141. [ ] Expose budget consumption and reasons for skipped/delayed/failed deliberation.
142. [ ] Make stale-result rejection, cognitive revision conflicts, plan supersession, and belief invalidation observable.
143. [ ] Track repeated situations that become progressively more deterministic through validated skills/policies.
144. [ ] Provide evaluation hooks for comparing multiple cognitive strategies on the same event/goal scenarios.
145. [ ] Measure at least task success, recovery success, unnecessary model-call rate, model-call escalation rate, token/cost efficiency, latency, stale-rejection rate, and cognitive-state consistency where measurable.
146. [ ] Preserve enough provenance to reproduce why a strategy and reasoning requirement were selected without requiring storage of sensitive prompts/responses.

## Safety and policy

147. [ ] Ensure cognitive autonomy never bypasses HAgent permission or host authorization boundaries.
148. [ ] Require explicit policy for autonomous tool invocation and consequential actions.
149. [ ] Support host-controlled maximum deliberation frequency and budget.
150. [ ] Support suspension, retirement, and shutdown of a cognitive runtime independently from persistent state deletion.
151. [ ] Ensure sleeping/retired runtimes do not continue to originate new executions.
152. [ ] Treat model-produced beliefs, goals, intentions, plans, skills, and policies as untrusted proposals until validated against applicable policy.
153. [ ] Require explicit governance before a learned candidate can move from experimental or proposed status into trusted deterministic behavior.
154. [ ] Provide safe fallback when a cognitive strategy fails, is unavailable, or produces incompatible state transitions.

## Business and simulation neutrality

This phase must explicitly support both ordinary business agents and simulated/artificial-world agents.

Examples:

```text
Business
  Customer-support agent
    Event: customer reply
    Attention: unresolved issue
    Goal: resolve ticket within SLA
    Plan: inspect -> diagnose -> respond/escalate

Simulation
  World agent
    Event: nearby entity / danger / conversation
    Attention: threat or social opportunity
    Goal: survive / trade / reach destination
    Plan: navigate -> interact -> re-evaluate
```

The cognitive runtime sees both as events, beliefs, goals, plans, memory, skills, tools, and execution requests. The environment supplies the domain semantics.

## Explicit non-goals

- No HWorld types, physics, simulation clocks, actors, factions, or world rules.
- No CRM/ERP/helpdesk domain model.
- No renderer/UI dependency.
- No mandatory LLM usage for every event or tick.
- No requirement that cognition and execution share a thread.
- No replacement of the existing `AgentExecution` lifecycle.
- No provider-specific cognitive implementation.
- No claim that AHC or any other single strategy is the final architecture of intelligence.
- No direct reproduction of BDI, SOAR, ACT-R, CoALA, Global Workspace, ReAct, or other research architectures as a monolith.
- No automatic promotion of every model trace or successful experience into trusted deterministic behavior.
- No hidden self-modification of the cognitive kernel.

## Example verification

Add deterministic Example coverage for:

1. [ ] Request-oriented execution remains unchanged and does not require a cognitive runtime.
2. [ ] A persistent cognitive runtime can receive events while multiple executions run asynchronously.
3. [ ] Routine events are handled reactively without an LLM execution.
4. [ ] Novel/uncertain events trigger progressive reasoning assessment.
5. [ ] The system can explicitly decide that no model execution is needed.
6. [ ] A lightweight reasoning path can escalate to a stronger reasoning path when evidence or capability requirements are insufficient.
7. [ ] The execution planner selects a provider/model from a provider-neutral reasoning requirement.
8. [ ] Belief contradiction triggers explicit belief revision and, when required, plan/intention reconsideration.
9. [ ] Existing plans continue through deterministic operators without repeated model calls.
10. [ ] A missing precondition or blocked action produces an explicit impasse.
11. [ ] Impasse resolution is bounded and cannot overwrite newer cognitive revisions.
12. [ ] Plan failure triggers bounded re-deliberation.
13. [ ] Higher-priority goals can supersede lower-priority intentions.
14. [ ] A successful repeated experience can produce a versioned skill/policy candidate.
15. [ ] Unvalidated learned candidates cannot become trusted deterministic behavior.
16. [ ] Persistent state survives runtime restart where persistence is enabled.
17. [ ] Late/stale executions cannot overwrite newer cognitive revisions.
18. [ ] Execution configuration is reused safely and refreshed when invalidated.
19. [ ] Cognitive trace reports why an event was ignored, reacted to, escalated, or deliberated.
20. [ ] Two independent cognitive runtimes can operate concurrently without state or memory leakage.
21. [ ] Multiple cognitive strategies can be evaluated against the same scenario through the same kernel and execution boundary.
22. [ ] A future experimental strategy can be added without changing runtime identity, persistence, authorization, or execution semantics.
23. [ ] The same cognitive runtime model works with a business-style event host and a simulation-style event host using only public generic APIs.

## Architectural additions and explicit contracts

The following contracts are first-class architectural concepts even when their initial implementations are deliberately small:

### DecisionContext

`DecisionContext` is the bounded, immutable-at-decision-time cognitive input shared by reactive policies, planners, and LLM-backed policies. It should be assembled from current working state, current belief state, attended events, active goals/intentions, current plan state, relevant retrieved resources, available capabilities, constraints, and reasoning requirements.

```text
DecisionContext
├── current situation
├── current belief state / relevant beliefs
├── attended events / observations
├── active goals
├── active intention
├── current plan
├── relevant Memory resources
├── relevant Knowledge resources
├── applicable Skills
├── available capabilities
├── constraints / permissions
└── reasoning requirements
```

The context builder must not automatically include all stored memory, Wiki/Knowledge, Skills, or conversation history. Retrieval, workspace bounds, and context budgets determine the working set.

### DecisionPolicy

The cognitive runtime should expose a provider-neutral decision-policy boundary rather than embedding decision logic directly in the runtime loop.

Conceptually:

```text
IDecisionPolicy
├── ReactivePolicy
├── UtilityPolicy
├── LlmPolicy
├── HybridPolicy
└── future strategy-specific policies
```

A policy consumes `DecisionContext` and returns a provider-neutral decision result, an internal cognitive action, or a bounded request for deliberation. Policies remain host-neutral; domain-specific authority stays with the host.

### Cognitive strategy

A cognitive strategy is the higher-level policy/architecture that determines how a runtime reasons over its cognitive state. It is intentionally separate from provider/model selection.

```text
ICognitiveStrategy
├── AdaptiveHybridCognition
├── future research-derived strategies
└── experimental strategies
```

A strategy may choose among reactive cognition, deterministic planning, internal cognitive actions, retrieval, reflection, and one or more deliberative executions. It must express provider/model needs through `ReasoningRequirement` and let the execution planner select the concrete target.

### ReasoningRequirement

`ReasoningRequirement` is the bridge between cognition and execution planning.

```text
ReasoningRequirement
├── required capabilities
├── preferred capabilities
├── reasoning depth / quality needs
├── context requirements
├── tool-use / structured-output requirements
├── latency tolerance
├── cost/budget constraints
└── other provider-neutral constraints
```

It must not embed provider-specific model names or transport details. Phase 0.96 remains responsible for target selection and execution admission.

### Plan and operator

A `Plan` is state/data. A `Planner` creates or revises plans. An `Operator` or cognitive action represents a bounded transition that may have preconditions and intended effects. Keep these concepts separate so future planning strategies can be added without changing plan storage or execution semantics.

Conceptually:

```text
IPlanner
├── DeterministicPlanner
├── UtilityPlanner
├── LlmPlanner
├── HybridPlanner
└── future planner implementations
```

Initial implementations may be minimal. The architectural contract should exist before advanced planning is implemented.

### Impasse

`Impasse` represents inability of the current cognitive path to safely continue. Examples include missing operators, conflicting goals, failed preconditions, contradictory beliefs, inadequate evidence, or policy/resource blockage.

The runtime should be able to resolve an impasse by applying another deterministic strategy, retrieving more information, waiting, requesting LLM deliberation, switching strategy, or escalating to the host, subject to bounds and policy.

### Cognitive revision

A cognitive revision is the versioned state boundary for beliefs, goals, intentions, plans, working state, and adopted learned artifacts.

Model outputs and learning proposals are never authoritative state mutations. They are proposals against a specific revision and must pass validation before being committed as the next revision.

### Cognitive planning versus execution planning

These are deliberately separate:

```text
Cognitive Planner / Strategy
    Question: "What should the agent do, and what reasoning is required?"
    Input: DecisionContext + beliefs + goals + knowledge + skills + strategy rules
    Output: Decision / Plan / ReasoningRequirement

Execution Planner
    Question: "Where/how should required inference execute?"
    Input: AgentExecutionRequest + ReasoningRequirement / capability requirements
    Output: ExecutionTargetAssessment + selected target
```

The cognitive planner must never directly inspect provider-specific rate-limit or transport state. It expresses inference requirements; Phase 0.96 decides the concrete execution target.

### Retrieval and relevance

Resource retrieval must be a reusable subsystem rather than an LLM-only behavior.

Conceptually:

```text
DecisionContext / RetrievalQuery
        |
        v
Resource eligibility filtering
        |
        v
Candidate retrieval
        |
        v
Relevance ranking
        |
        v
Bounded top-K resources
        |
        v
GlobalWorkspaceFrame / DecisionContext
```

Relevance should remain composable. Initial ranking may use resource type, scope, tags, lexical similarity, goal/task metadata, recency, importance, belief relevance, and other deterministic signals. Optional semantic/vector retrieval or model-assisted ranking can be added later without making it mandatory for Core.

Attention and relevance are different operations: **attention determines what matters now; retrieval/relevance determines what information is useful about it.**

### Learning and proceduralization

Learning may produce new Knowledge, Skills, Policies, heuristics, or strategy proposals, but adoption must remain governed and versioned.

The intended progression is:

```text
new / unfamiliar situation
        ↓
probabilistic reasoning when required
        ↓
experience + outcome
        ↓
candidate skill / policy / knowledge
        ↓
validation + governance
        ↓
trusted reusable behavior
        ↓
future situation may require less or no model use
```

This is an optimization and adaptation mechanism, not a guarantee that every problem converges to deterministic code. Unknown portions must remain able to return to deliberation.

### Validation boundary

Decisions, beliefs, plans, skills, policies, and learned candidates must remain subject to explicit validation before consequential execution or trust promotion. A provider/model response, even when structurally valid, is not authorization. HAgent permissions, tool capability, budgets, cognitive revision checks, and host authorization remain enforcement boundaries.

## Exit criterion

A host can create a long-lived cognitive runtime for a runtime agent, feed it events/observations, maintain persistent beliefs, goals, intentions, and plans, selectively retrieve memory/knowledge/skills, handle routine situations without an LLM, progressively escalate reasoning only when justified, select an appropriate model through a provider-neutral reasoning requirement, encounter and resolve explicit impasses, learn versioned skills/policies from validated experience without silently mutating the cognitive kernel, recover safely after restart, compare multiple cognitive strategies through the same runtime substrate, and observe the full cognitive lifecycle — without HAgent gaining ownership of host-domain state or side effects.

Cognitive Runtime Workbench

Part of Phase 0.97.

HAgent.WinForms must add a top-level Cognitions view for active runtime instances. It provides complete inspection of current runtime cognition: beliefs, goals, intentions, plans, attention, working state, memory, knowledge, skills, events, executions, learning and history.

Cognitive Workbench Controls

Authorized users may insert, edit and invalidate beliefs; create, edit, reprioritize, suspend, resume and abandon goals; modify intentions where policy permits; request plan reconsideration; inject observations/events; request deliberation; and pause, resume, wake, sleep or retire a runtime.

The UI must use HAgent runtime state-transition APIs and never write directly to persistence. Every mutation is atomic, version-aware, authorized and auditable. Record operator identity, timestamp, reason, UI action, previous revision and new revision.

If the runtime revision changed since the UI read it, reject or refresh the mutation rather than silently merging it. Existing execution snapshots remain immutable and stale executions must not overwrite newer cognition.

Cognitive Workbench History and Learning

Show the complete cognitive timeline: events, belief changes, attention changes, goal and intention changes, plan revisions, impasses, deliberation, executions, outcomes, memory/experience creation, learning decisions, sleep/wake and recovery, including user interventions.

Show learning as Experience -> Memory -> Reflection/Learning -> candidate skill/knowledge/policy -> validation/governance -> published version. Distinguish candidates from authoritative versions so users can see when repeated LLM reasoning becomes reusable deterministic behavior.

Show the active cognitive strategy and version, such as Adaptive Hybrid Cognition (AHC). Future strategies must use the same generic workbench while allowing strategy-specific diagnostics. Historical state is initially read-only; future experimentation may branch from checkpoints without silently replacing live state.

## Phase 0.10 — Workspaces, Routing + Chat

**Status: PAUSED after the provider-neutral workspace routing and role-policy foundation.**

Phase 0.10 remains intentionally unfinished. The remaining workspace product work is deferred while earlier provider/runtime capability gaps are investigated and corrected.

## Goal
Provide an optional shared conversation where one authenticated host user and multiple runtime agents can visibly work together while every model request is routed deliberately and the user's workspace state survives application restarts.

## Steps

1. [x] Introduce a workspace abstraction independent of WinForms.
2. [x] Register users and runtime-agent participants with explicit lifecycle state.
3. [x] Define one workspace default recipient for unaddressed user messages.
4. [x] Define direct user-to-agent addressing.
5. [x] Define addressed agent-to-agent delegation and responses.
6. [x] Define coordinator/specialist behavior as a role/policy over generic runtime agents. `WORKSPACE ROLES` Example verification complete.
7. [ ] Allow specialists to represent whole domains, tables, subsystems, or other host responsibilities.
8. [x] Preserve sender, recipient, correlation, causation, ordering, and routing metadata.
9. [ ] Execute routed workspace messages through runtime agents and make agent-to-agent work visible in the workspace lobby when enabled.
10. [ ] Add configurable addressing syntax at the host/UI layer without making prompt text the authoritative routing mechanism.
11. [ ] Add loop protection and collaboration budgets.
12. [ ] Add optional persistent workspace state and explicit shared-memory policy.
13. [ ] Add the WinForms workspace surface and global agent selection.
14. [ ] Add stable host user identity input, including `UserId` and `IsAdmin`, with database-safe user/workspace partitioning.
15. [ ] Add create/open/show/hide/close workspace lifecycle APIs where UI close never destroys persisted workspace state.
16. [ ] Persist user-owned lobby chat, private-agent chat, participant membership/roles, approval state, safe statistics, and selected workspace UX state according to storage policy.
17. [ ] Add user-facing workspace configuration with a single `Enable Workspace` setting; workspace remains hidden until explicitly opened.
18. [ ] Add default manager/coordinator agent configuration.
19. [ ] Add default specialist agent configuration and specialist responsibility metadata.
20. [ ] Add workspace/private-chat provider and agent selection/override controls without mutating persistent agent profiles.
21. [ ] Add configurable default approval type/policy and integrate approval requests/resolution into workspace UI and conversation history.
22. [ ] Add professional modern WinForms UI with lobby chat, participant/agent list, private chats, approval presentation, and explicit open/close/show/hide controls.
23. [ ] Add Example controls and tests for create, open/show, hide, close UI, agent join/leave, lobby chat, private chat, approval flow, persistence across restart, and user/workspace restoration.
24. [ ] Verify SQL Server, MySQL, and File workspace persistence and user partitioning.

### Current foundation

The provider-neutral foundation contains `AgentWorkspace`, `WorkspaceParticipant`, `WorkspaceMessage`, `IWorkspaceRouter`, and `WorkspaceRouter`. Participants are either users or runtime agents and have explicit Active/Suspended/Retired state. An active default recipient may be defined for unaddressed user messages. Routing does not invoke providers, mutate agent profiles, or perform host side effects.

`IWorkspaceRolePolicy` and `WorkspaceRolePolicy` allow coordinator/specialist behavior to be expressed as policy over ordinary agent participants. `WorkspaceAgentRoleAssignment` describes `Participant`, `Coordinator`, or `Specialist` role, optional responsibility metadata, user-message eligibility, and allowed delegation target roles. No separate coordinator/specialist agent classes are introduced. The `WORKSPACE ROLES` Example verification confirms the policy works.

The workspace user identity contract must carry a stable `UserId` and an `IsAdmin` flag supplied by the host. The identity is an input to workspace authorization and persistence partitioning; `IsAdmin` does not itself grant tool, memory, data, or host-business permissions.

Phase 0.10 initially provides one default persisted workspace per user. The model should remain extensible to multiple named workspaces later without forcing a second workspace into the first implementation.

## Workspace lifecycle

The workspace UI is hidden until explicitly opened by the host. There is no automatic-show workspace behavior in Phase 0.10.

`Create`/ensure obtains the user's default workspace. `Open`/`Show` makes the UI visible. `Hide` hides it without changing workspace state. `Close` closes the UI without deleting workspace state. Destructive archive/deletion is a separate explicit operation and is not implied by closing the UI.

Application shutdown or computer shutdown must not cause user work to disappear. Reopening the application with the same stable `UserId` restores the persisted workspace state from the selected HAgent storage backend.

## Conversations

The workspace contains a shared Lobby conversation where the user and joined agents can visibly communicate. It also contains distinct Private Chats between the user and selected agents. Private chat content is not automatically exposed to other workspace participants.

Visible messages identify their author and role clearly. System and approval events are first-class workspace-visible events alongside ordinary conversation messages.

## Agent configuration

The host/application administrator can configure default manager/coordinator and specialist agents for the workspace. A specialist has descriptive responsibility metadata that can represent a domain, table, subsystem, process, capability, or other host-owned responsibility without requiring HAgent-specific domain classes.

The workspace UI can let a user switch the active provider/model or selected agent for an allowed conversation or private chat. These are execution/runtime selections and do not silently mutate the stored `AiAgent` profile.

The default approval type/policy is a workspace/host policy default. Approval handling remains subject to HAgent authorization and does not bypass permission checks.

## Routing rules

- Unaddressed user message: send only to the workspace default recipient.
- Explicitly addressed user message: send to that participant.
- Agent delegation: send only to the addressed participant unless an explicit role policy allows the sender's role to delegate to the recipient's role.
- Broadcast: explicit opt-in operation, never the default.
- Agent-to-agent routed work becomes visible in the Lobby when workspace execution policy permits it.

The authoritative routing decision is represented by workspace messages and routing APIs; human-friendly addressing syntax is a UI/host convenience.

## Persistence

Persisted workspace state is partitioned by host application identity and the stable `UserId`. File storage remains local to the host installation; SQL Server and MySQL must prevent users in the same host application from reading or mutating another user's workspace state unless an explicit host/admin policy permits it.

Persisted workspace state includes workspace metadata, participant membership/roles/lifecycle state, lobby and private-chat history, approval requests/resolution state, safe statistics/activity metadata, selected workspace UX state where appropriate, and explicit workspace/shared-memory records.

Provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, and temporary execution objects are not workspace persistence concerns; the generic runtime phase already establishes those boundaries.

Agent private memory remains private unless explicit shared-memory policy grants workspace visibility.

## Workspace UI

The WinForms workspace is a compact, professional collaboration surface rather than a large dashboard. It contains a Lobby, participant/agent selection, access to private chats, approval presentation, and message composition. It must provide clear authorship and state without exposing implementation details.

The host interacts with the workspace through a public HAgent workspace facade rather than direct manipulation of internal WinForms controls. The facade provides lifecycle and communication operations conceptually equivalent to create, open/show, hide, close, agent join/leave, lobby send, private-chat open/send, and workspace state observation.

## Example verification

The Example application will expose explicit controls for creating, showing/opening, hiding, and closing the workspace UI, plus controlled agent join/leave and communication tests. Verification must confirm that closing the UI and restarting the application preserve the user's lobby/private chats, membership, approvals, selected state, and safe statistics for File, SQL Server, and MySQL storage.

## HWorld boundary

HWorld remains an external consumer. It references HAgent normally and uses public runtime/workspace APIs. HAgent does not add an HWorld-specific dependency, adapter, world type, physics, simulation scheduling, or action authority.

## Exit criterion

A host can identify a user, create/open the user's persisted default workspace, show and close the UI without losing work, join runtime agents, conduct visible lobby and private conversations, configure/select permitted manager/specialist agents and provider/model overrides, present and resolve approvals, and restore the same state after application restart while routing remains bounded and traceable.

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

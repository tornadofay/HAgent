# HAgent Roadmap

> This file is generated from the complete active V1 roadmap source set. Do not edit it directly.
> V2/research-only material belongs in `roadmapv2.md` and is intentionally excluded.
> Source directory: `docs/roadmap`.

## HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, current implementation state belongs under `docs/plan/`, and this directory defines the ordered delivery sequence.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage and first-class resource foundations substantially implemented; remaining repository/backend obligations are consumed by later storage/resource phases
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration complete and verified on .NET Framework 4.8.1 and .NET 9
- 0.951 — Identity, Tenancy + User Context completed and verified
- 0.952 — First-Class Event Subsystem completed and verified
- 0.953 — Unified Policy Engine completed for its canonical policy boundary; UI/backend hardening is post-phase work
- 0.954 — Prompt + Instruction Governance completed and verified on .NET Framework 4.8.1 and .NET 9
- 0.955 — Context Engineering completed and verified on .NET Framework 4.8.1 and .NET 9
- 0.956 — Observability + Distributed Tracing completed through its verified slices
- 0.957 — Evaluation + Quality Measurement completed and verified
- 0.9575 — Knowledge, Skills, Memory Governance + Learning **current**; Slices 1–7 verified, Slice 8 in progress
- 0.9576 — Learned Resource Reliability + Adaptation **planned follow-on**
- 0.958 — Agent Lifecycle + Health **planned**
- 0.9591 — Goal/Plan Persistence + Recovery **planned and ordered before intervention**
- 0.959 — Human-in-the-Loop + Intervention **planned; execution/learning intervention exists ahead of roadmap**
- 0.9592 — Provider Ecosystem + Adapter Lifecycle **planned**
- 0.96.x — Configuration, Storage + Portability **cross-cutting foundation before 0.96**
- 0.96 — Capability-Aware Execution **planned major execution foundation**
- 0.97 — Persistent Cognitive Runtime **planned production V1 cognitive layer**
- 0.98 — Model Reasoning Engineering **planned bounded reasoning-engineering layer**
- 0.10 — Workspaces, Routing + Chat **deferred user-facing product surface after the generic runtime/execution/cognition foundations are sufficient**
- 1.0 — Collaboration + Workflows **deferred orchestration layer built on 0.10, 0.959, 0.9591, 0.96, 0.97, and 0.98**
- Later — extensibility, developer platform, release hardening, and other ecosystem work

## Ordered V1 dependency chain

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Events
        ↓
0.953 Unified Policy
        ↓
0.954 Prompt / Instruction Governance
        ↓
0.955 Context Engineering
        ↓
0.956 Observability / Tracing
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 Knowledge / Skills / Memory Governance + Learning
        ↓
0.9576 Learned Resource Reliability + Adaptation
        ↓
0.958 Agent Lifecycle + Health
        ↓
0.9591 Goal / Plan Persistence + Recovery
        ↓
0.959 Human Intervention
        ↓
0.9592 Provider Ecosystem + Adapters
        ↓
0.96.x Configuration / Storage / Portability
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
        ↓
0.98 Model Reasoning Engineering
        ↓
0.10 Workspaces / Routing / Chat
        ↓
1.0 Collaboration / Workflows
```

The ordering is dependency-driven. A feature may be implemented early as an ahead-of-roadmap experiment, but that does not mark its ordered phase complete until the phase's full V1 boundaries and verification requirements are satisfied.

## Phase ownership map

Each responsibility has one authoritative owner. Later phases consume the contract instead of recreating it.

| Responsibility | Authoritative phase | Later consumers |
|---|---|---|
| Identity, tenancy, user context | 0.951 | all identity-aware phases |
| Generic events | 0.952 | runtime, cognition, workspace, workflows |
| Authorization/policy/precedence | 0.953 | instruction, context, learning, intervention, execution, cognition |
| Instruction authority/provenance | 0.954 | context, execution, cognition, reasoning engineering |
| Context retrieval/assembly/bounds | 0.955 | execution, cognition, reasoning engineering |
| Tracing/observability | 0.956 | all runtime subsystems |
| Evaluation/quality evidence | 0.957 | learning, reliability, execution diagnostics, cognition, reasoning engineering |
| Skill/Knowledge/Memory governance + learning promotion | 0.9575 | reliability, cognition, workspace, reasoning engineering |
| Post-promotion reliability/applicability/forgetting | 0.9576 | lifecycle, cognition, reasoning engineering |
| Runtime-agent lifecycle + runtime health | 0.958 | durable recovery, intervention, cognition |
| Durable goals/plans/checkpoints/recovery | 0.9591 | intervention, cognition, workflows |
| Human/host intervention boundary | 0.959 | lifecycle, plans, cognition, workflows, reasoning engineering |
| Provider adapter/discovery/operational evidence | 0.9592 | 0.96 execution, reasoning engineering |
| Configuration/storage/portability | 0.96.x | execution, cognition, workspaces, workflows |
| Concrete execution-target selection/admission | 0.96 | all inference-capable subsystems |
| Persistent per-agent cognition | 0.97 | reasoning engineering, workspace, collaboration, workflows |
| Model reasoning responsibility / deterministic-before-inference discipline | 0.98 | 0.10, 1.0, host applications |
| User-facing workspace/routing/chat | 0.10 | collaboration/workflows |
| Multi-agent collaboration/workflow orchestration | 1.0 | host applications |

The table is an architectural ownership rule, not merely a planning convenience.

## First-class resource architecture

Knowledge, Skills, Memory, and Learning are one coherent resource architecture.

```text
0.8 resource foundations
    identity / ownership / scope
    provenance / lifecycle / version metadata
    Skill definitions/references
    Knowledge/Wiki contracts
    Memory family/type foundations
    storage substrate

0.9575 mature resource governance + learning
    capability inheritance + runtime overrides
    authorization and resource governance
    bounded retrieval + retention
    Learning Mode
    typed candidates
    lifecycle / review / promotion
    version-safe authoritative resource creation
    context/runtime integration

0.9576 post-promotion reliability
    applicability / validity
    outcome-based trust evidence
    stale / contradiction / drift handling
    revalidation / replacement
    archival / forgetting
```

No later phase may create a parallel Memory, Knowledge, Skill, or Learning model merely because an earlier phase has not yet completed its management surface.

## Runtime and execution separation

The roadmap maintains a strict separation:

```text
Persistent Cognitive Runtime (0.97)
    decides whether/what cognitive progress is required

Model Reasoning Engineering (0.98)
    defines the bounded reasoning responsibility once probabilistic reasoning is requested

Execution Planner (0.96)
    decides where/how the requested inference executes

Provider Adapter (0.9592)
    knows provider-specific transport/discovery details

Execution Engine
    performs the selected request
```

The cognitive layer must not become a second provider/model router. The 0.98 reasoning layer must not become a second cognitive runtime or execution planner.

## Single-owner runtime invariant

Persistent cognition uses one authoritative state owner per runtime agent instance.

```text
one runtime instance
    → one authoritative state owner
    → serialized state mutation
    → asynchronous work returns results/events

many independent runtime instances
    → concurrent operation
    → isolated state and identity
```

This removes the need for a general multi-writer cognitive merge architecture in production V1. Revision/stale-result protection remains mandatory.

## 0.96 scope

Phase 0.96 is the only concrete execution-selection layer. It handles:

- capabilities and constraints;
- provider/model/execution-target selection;
- cost policy;
- quota/rate limits;
- concurrency/capacity admission;
- health/availability of execution targets;
- latency and bounded waiting;
- fallback/degradation;
- target diagnostics.

Provider health evidence originates at 0.9592; 0.96 consumes it. Cognitive strategies emit provider-neutral reasoning requirements and consume 0.96 rather than naming providers/models.

## 0.97 production V1 scope

The Persistent Cognitive Runtime provides:

- authoritative per-agent cognitive state;
- observations and beliefs;
- bounded DecisionWorkspace selection;
- goals and intentions;
- deterministic/reactive decisions;
- bounded deliberation and provider-neutral reasoning requirements;
- plans and recovery integration;
- governed learning candidates;
- learned-resource reliability consumption;
- lifecycle, persistence, cancellation, shutdown, observability, and evaluation;
- a production management/diagnostic workbench built on runtime state-transition APIs rather than direct persistence mutation.

The three auxiliary roadmap documents `cognitive-workbench.md`, `cognitive-workbench-controls.md`, and `cognitive-workbench-learning.md` are subdocuments of 0.97, not separate roadmap phases.

## 0.98 scope

Phase 0.98 is the bounded engineering layer between persistent cognition and provider/model inference. It does not decide whether the agent should reason in the first place; 0.97 owns that decision. It does not select the provider/model; 0.96 owns that decision.

0.98 owns:

- deterministic-before-inference responsibility boundaries;
- bounded model reasoning responsibilities;
- decomposition of semantic reasoning work;
- explicit use of authoritative evidence/context;
- reasoning-result contracts and uncertainty representation;
- validation of semantic reasoning results before authoritative use;
- provider/model-neutral reasoning quality verification.

The canonical shape of a future `ReasoningTask` or equivalent contract is intentionally **not decided by this roadmap entry**.

## 0.10 vs 1.0 boundary

0.10 is the user-facing workspace/routing/chat product surface. It does not become a general multi-agent workflow engine.

1.0 is the later orchestration layer for bounded multi-agent collaboration and workflows. It consumes 0.10 routing, 0.959 intervention, 0.9591 durable planning/recovery, 0.96 execution selection, 0.97 persistent cognition, and 0.98 reasoning engineering. It must not recreate those lower-level authorities.

## Ahead-of-roadmap implementation rule

Existing implementation may appear before its ordered phase. Such work is retained when useful, but it is labeled as ahead-of-roadmap evidence and does not silently reorder the roadmap.

This rule applies especially to current execution intervention, learning-candidate intervention, runtime examples, resource foundations, and other experiments created while earlier foundations were still being completed.

## Cross-phase V1 verification scenarios

Phase exit criteria prove their own bounded contracts. V1 completion also requires a small set of concrete scenarios that exercise the seams between authoritative phases. These are integration scenarios, not replacements for phase-specific verification.

### Scenario 1 — Provider failure and recovery

A persistent cognitive decision in 0.97 produces a provider-neutral `ReasoningRequirement` → 0.96 selects an execution target → the selected provider fails or reaches a governed rate/capacity boundary → 0.96 admits a valid fallback target → execution completes → 0.97 continues the active goal/plan without creating a second provider-selection authority.

### Scenario 2 — Learning and restart recovery

0.97 captures a meaningful experience → 0.9575 admits a typed learning candidate through policy and Learning Mode → governed promotion creates authoritative resource state → the host/runtime restarts → 0.958/0.9591 recovery restores the durable cognitive state → the promoted resource remains authoritative, version-safe, and governed after restart.

### Scenario 3 — Intervention and stale asynchronous result

0.97 starts bounded asynchronous reasoning → 0.959 intervention changes the authoritative target state or lifecycle revision → the old reasoning result arrives → the owning runtime rejects it as stale/superseded → the runtime continues from the current authoritative state without corrupting newer state.

### Scenario 4 — Independent concurrent agents without a shared cognitive bottleneck

At least 10 independent runtime agents receive work concurrently → each agent serializes mutation only within its own state owner → each agent completes its work without waiting behind another agent's owner queue → each agent retains isolated identity, revision, and state → observed concurrency demonstrates that one agent's workload does not collapse all agents behind a shared cognitive queue.

These scenarios must use deterministic/fake infrastructure where possible and must report the concrete phase boundaries exercised. They do not require every execution to pass through every phase; each scenario tests a specific real cross-phase dependency.

## Roadmap maintenance rules

1. One phase owns each responsibility; later phases consume earlier contracts rather than recreating them.
2. Every substantial phase has a bounded V1 exit criterion.
3. Production V1 requirements are separated from V2/research work.
4. Architectural invariants outrank implementation convenience.
5. Historical foundation checklists are normalized when later implementation proves the item already exists; genuine missing work is assigned to the phase that consumes it.
6. If implementation proves a dependency wrong, update this ordered roadmap and the affected phase documents together before continuing.
7. Auxiliary documents under `docs/roadmap/` must name their parent phase and may not create an independent milestone or authority.
8. Generated root `roadmap.md` remains a view; authoritative ordering lives in `docs/roadmap/`.
9. Verification depth must match the risk and nature of the claim. Concurrency, cancellation/lifecycle, persistence/recovery, authorization/security, performance, and public API claims require verification appropriate to those claims rather than a blanket requirement that every feature use the same test type.
10. A production architecture mechanism that materially affects V1 scope must have a corresponding roadmap item or explicit scope reference; architecture and roadmap must not silently drift apart.

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

11. [x] Establish one provider-neutral resource identity contract that can represent Skills, Knowledge/Wiki resources, Memory families/types, Learning candidates, and future resource types without adding hard-coded resource properties to the agent model.
12. [x] Establish explicit resource scope/ownership metadata compatible with the canonical identity model: Global, Tenant, User, Workspace, Agent, Runtime, and Execution where applicable. `Workspace` is the canonical scope; historical `Domain` terminology is obsolete.
13. [x] Establish resource provenance/source metadata and lifecycle/status/version metadata as shared foundation concepts.
14. [x] Define the stable distinction between Skills, Knowledge/Wiki, Memory, and Learning candidates; do not collapse them into one persisted object model.
15. [x] Establish stable/versioned Skill definitions and references while keeping executable handlers outside persistence.
16. [x] Establish provider-neutral Knowledge/Wiki resource/source contracts and bounded retrieval semantics independent of keyword/vector/index implementation.
17. [x] Normalize Memory foundations around working, episodic, semantic, procedural, and future extensible types while keeping memory ownership separate from physical storage.
18. [x] Establish typed learning-candidate contracts and provenance fields needed for later governed promotion without making the model authoritative.
19. [x] Preserve immutable snapshot compatibility: resource definitions and references must be safe to capture into active execution snapshots without later configuration edits mutating running executions.
20. [x] Ensure the resource foundation remains usable without GPU hardware, vector databases, embeddings, or large resident indexes.

The resource-foundation obligations above are now historical foundation work. Any remaining gaps in mature persistence, governance, promotion, retention, administration, or management UI are owned by their consuming phases, especially 0.9575, 0.96.x, and 0.97. No later phase may create a parallel resource model to finish an incomplete surface.

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
9. [x] Provide runtime-state persistence/snapshot hooks needed by the current runtime contract without claiming durable cognitive goal/plan state or a second persistence model.
10. [x] Verify the runtime contract with deterministic Example coverage.
11. [x] Complete generic external-host execution boundary hardening in Phase 0.95.

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

Durable goal, intention, plan, checkpoint, and recovery state is deliberately later work in Phase 0.9591. Persistent cognitive ownership and long-lived cognition are later 0.97 work. The runtime-instance phase must not be treated as having already solved those later persistence problems merely because basic runtime-state snapshots exist.

## Exit criterion

A host can create, run, cancel, and retire multiple independent runtime agents from reusable profiles without identity, private-memory, or execution-state collisions. Later resource governance, durable goal/plan recovery, and persistent cognition can then build on the stable runtime and snapshot boundaries without weakening runtime isolation.

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

## Phase 0.96 — Capability-Aware Execution

## Status

**Planned major execution foundation after 0.96.x and before 0.97.**

## Purpose

Select and admit the best currently usable concrete execution target for each request across heterogeneous providers, models, accounts, endpoints, capabilities, quotas, rate limits, concurrency capacity, health, latency, and cost policy.

Phase 0.96 is the **single concrete execution-selection authority** in HAgent.

The cognitive layer may request reasoning requirements, but it must never choose provider/model names directly.

## Core separation

```text
Agent Profile
    = what the agent requires and prefers

Provider
    = provider/service integration

Logical Model
    = provider-independent identity when reliably known

Execution Target
    = concrete provider + account/project/endpoint + model/deployment

Capability
    = what the target can do

Constraint
    = limits on the requested operation

Operational State
    = quota / rate / concurrency / health / availability

Cost State
    = Free / FreeWithinQuota / Paid / Unknown

Execution Planner
    = selects and admits the best compatible target
```

## Delivery slices

### Slice 1 — Execution-target model

- Define normalized target identity.
- Separate Provider, Model, and concrete target.
- Preserve provider-native identifiers/deployment metadata.
- Support multiple targets for the same logical model.

### Slice 2 — Capability and constraint evaluation

- Normalize capabilities as `Supported`, `Unsupported`, or `Unknown`.
- Keep capability separate from permission, health, quota, capacity, and cost.
- Support request requirements with required/preferred/optional/forbidden semantics.
- Cover structured output, tool use, reasoning, modalities, streaming, embeddings, and extensible future capabilities.
- Distinguish native support from emulated/degraded behavior.
- Never treat unknown capability as supported by default.

### Slice 3 — Discovery evidence integration

- Consume provider adapter discovery from 0.9592.
- Preserve capability provenance, confidence, observation time, and expiration.
- Accept provider metadata, documentation evidence, controlled probes, successful execution evidence, response metadata, and explicit host overrides.
- Keep discovery evidence replaceable and provider-neutral.

### Slice 4 — Cost and policy selection

Support system/agent/runtime policy:

```text
FreeOnly
FreePreferred
NoRestriction
```

Support Agent selection mode:

```text
Auto
Preferred
Fixed
```

Rules:

- `FreeOnly` cannot treat `Unknown` cost as free.
- `FreePreferred` prefers eligible free targets and may use paid fallback only when policy permits it.
- `Fixed` still enforces capability, authorization, quota, capacity, health, and constraints.
- Agent preferences never become permanent provider bindings.

### Slice 5 — Rate, quota, and concurrency admission

Use generic resource dimensions rather than provider-specific hard-coded limits.

Minimum dimensions:

```text
request count
tokens in
tokens out
total tokens
concurrency
```

Support arbitrary provider-defined windows and enforcement scopes.

Implement:

- proactive admission;
- atomic reservations for concurrent requests;
- bounded waiting;
- `Wait`, `TryNextCandidate`, `Fail`, or policy-approved degradation;
- reconciliation after execution;
- partial/unknown usage accounting;
- provider 429/quota signals as operational feedback.

A target with quota available may still have no execution capacity.

### Slice 6 — Health, latency, fallback, and long-running execution

- Track availability/health separately from capability.
- Track latency separately from quota/rate state.
- Support legitimately long-running inference without false failure.
- Preserve cancellation, timeout, and stale-result protection while waiting, executing, or falling back.
- Avoid permanent blacklisting from transient failures.
- Define explicit fallback/degradation behavior.

### Slice 7 — Execution planning and diagnostics

For each candidate expose a normalized assessment:

```text
Target identity
Compatibility
Capability evidence
Constraint result
Permission state
Quota/rate state
Capacity state
Health/availability
Cost state
Estimated latency
Wait-until (optional)
Degradation option (optional)
Score/ranking data
Decision reason
```

The assessment is diagnostic data and should be consumable by hosts/UI without provider-specific knowledge.

### Slice 8 — Management UI and verification

The management surface must show effective target capability, limits, cost, quota/rate/capacity, availability, and compatibility with the active request.

Deterministic verification must cover:

- same logical model through multiple providers;
- required/preferred/optional/forbidden capabilities;
- unknown capability metadata;
- incompatible manual selection;
- native vs degraded structured output;
- proactive rate limiting;
- token/request windows;
- atomic concurrent reservations;
- 429 feedback;
- long-running requests;
- cancellation and timeout;
- stale-result protection;
- target fallback;
- Auto/Preferred/Fixed selection;
- FreeOnly/FreePreferred/NoRestriction behavior.

## Architectural rules

1. 0.96 is the sole concrete execution-selection layer.
2. 0.97 cognitive strategies never select providers/models directly.
3. Rate limiting is proactive admission, not only retry logic.
4. Capability is not permission, health, quota, capacity, or cost.
5. Unknown information stays unknown.
6. Provider-specific logic remains in adapters.
7. Agent profiles express intent and preferences, not transport bindings.
8. Every execution uses an immutable effective snapshot of relevant configuration.
9. Fallback never bypasses authorization or capability requirements.
10. Independent runtime agents may call the planner concurrently without sharing mutable runtime identity/state.

## Dependency chain

```text
0.9592 provider/adapters
        ↓
0.96.x configuration/storage
        ↓
0.96 capability-aware execution
        ↓
0.97 persistent cognition
```

## Exit criterion

For every execution request HAgent can deterministically identify compatible targets, enforce capability/policy/cost/quota/capacity/health constraints, reserve required capacity, choose or wait/fallback according to explicit rules, execute through provider-neutral boundaries, and explain the resulting selection or rejection without embedding provider logic into Core or cognition.

## Phase 0.97 — Persistent Cognitive Runtime

## Status

**Planned after 0.96 and before higher-level autonomous-agent features.**

## Purpose

Add an optional long-lived cognitive runtime above ordinary HAgent executions.

A Persistent Cognitive Runtime lets one runtime agent remain active over time: receive observations/events, maintain state, pursue goals, use plans and Skills, remember relevant experience, decide when deterministic behavior is sufficient, request bounded reasoning when necessary, execute through the existing execution engine, and incorporate validated outcomes.

It is a production mechanism layer, not an implementation of BDI, SOAR, ACT-R, Global Workspace Theory, LIDA, ReAct, Reflexion, MemGPT, Voyager, consciousness, or AGI.

## Core production invariant — single authoritative owner

Each runtime agent instance owns its authoritative cognitive state.

```text
Agent A runtime instance
    ├── authoritative state owner A
    ├── event queue A
    ├── asynchronous work A
    └── state mutations applied only by owner A

Agent B runtime instance
    ├── authoritative state owner B
    ├── event queue B
    ├── asynchronous work B
    └── state mutations applied only by owner B
```

Asynchronous LLM/tool/retrieval work may run concurrently, but it never writes cognitive state directly. It returns a typed result/event to the owning runtime, which validates lifecycle, revision, policy, and applicability before applying the mutation.

Per-agent state mutation is serialized. Independent runtime agents remain concurrently schedulable and must not become a single shared cognitive bottleneck.

This is the authoritative concurrency model for V1.

## Architecture boundary

```text
Host / Environment
        ↓
Events / Observations
        ↓
Persistent Cognitive Runtime
        ├── state ownership
        ├── decision workspace
        ├── goals / intentions
        ├── plans / methods
        ├── reactive decisions
        ├── bounded deliberation
        ├── learning integration
        └── runtime lifecycle
        ↓
Reasoning Requirement
        ↓
Phase 0.96 Execution Planner
        ↓
Existing Execution Engine
```

The host remains authoritative over domain truth, host scheduling policy, permissions, external side effects, and business state.

## Delivery slices

### Slice 1 — Single-owner cognitive state and revisions

- Define the authoritative cognitive-state contract owned by one runtime agent.
- Provide immutable/read-only snapshots for consumers.
- Define a monotonic cognitive revision.
- Serialize state mutation per runtime agent.
- Allow asynchronous work to return events/results to the owner.
- Reject stale, cancelled, retired, shutdown, or superseded results.
- Preserve source/cause/correlation metadata.
- Verify multiple independent runtime agents can mutate concurrently without sharing state.

**Important:** V1 does not require a general multi-writer proposal/merge engine. The owner is the sole state authority.

### Slice 1 architectural verification evidence — VERIFIED 2026-09-11

The owner/queue architecture spike was executed through the existing `AgentRuntimeInstance` type on both supported targets:

- .NET Framework 4.8.1 — **passed**;
- .NET 9 — **passed**.

The verified spike covered one-owner-per-agent semantics, serialized mutation order, stale older-revision rejection, cancellation, post-shutdown mutation rejection, independent agent isolation, and 12 independent runtime agents operating concurrently.

For the 12-agent concurrency case, all 12 owner loops overlapped and each agent's follow-up mutation remained serialized within its own state owner. This is recorded as **verified architectural evidence**, not as completion of the production Persistent Cognitive Runtime implementation.

The spike did not modify the production runtime architecture and did not introduce a shared cognitive queue.

### Next documentation checkpoint — Slice 2

Before production implementation begins, finalize the provider-neutral contract for:

- observations versus inferred beliefs;
- provenance, confidence/quality, freshness, validity, scope, and revision;
- explicit ambiguity and insufficient evidence states;
- bounded `DecisionWorkspace` selection;
- deterministic relevance signals and bounded multi-signal selection;
- workspace isolation from prompt construction and long-term memory.

The detailed mechanism authority for this checkpoint is `docs/architecture/17-cognitive-algorithms.md`, sections 4–5. Documentation must remain consistent with the V1 roadmap boundary before code is introduced.

### Slice 2 — Observations, beliefs, and bounded decision workspace

- Distinguish host observations/events from inferred beliefs.
- Preserve provenance, confidence/quality, scope, freshness, validity, and revision.
- Represent ambiguity and insufficient evidence explicitly.
- Define bounded `DecisionWorkspace` selection for the current decision.
- Use deterministic relevance signals such as urgency, novelty, goal relevance, uncertainty, risk, freshness, relationship relevance, and policy importance where supplied.
- Keep workspace selection separate from prompt construction.
- Prevent event storms and workspace growth from becoming unbounded.

### Slice 3 — Goals, intentions, and reconsideration

- Define durable Goal and Intention usage over the 0.9591 persistence contracts.
- Keep Goal, Intention, and Plan/Method distinct.
- Support multiple active goals with deterministic priority/constraint policy.
- Record why an intention was adopted, retained, revised, suspended, completed, failed, abandoned, or superseded.
- Define reconsideration triggers such as invalid assumptions, failure, changed constraints, higher-priority goals, resource/policy changes, deadlines, or host intervention.
- Add anti-thrashing limits such as cooldown, reconsideration budgets, repeated-proposal detection, or no-progress thresholds.

### Slice 4 — Reactive processing and deterministic fast path

The runtime should attempt deterministic processing before model reasoning.

```text
Event / current state
        ↓
Check current plan/operators/Skills
        ↓
Check preconditions + policy + resources
        ↓
Safe deterministic action?
    yes → apply
    no  → reasoning assessment
```

Deterministic behavior may advance a plan, update working state, emit an event, invoke a governed tool, mark a resource stale, wait/sleep/wake, or create an impasse.

Routine events should not consume an LLM merely because the runtime is active.

### V1 impasse boundary

An `Impasse` is a bounded state in which the runtime cannot safely continue the current deterministic path. V1 keeps impasse handling explicit and shallow rather than introducing a general recursive subproblem architecture.

Supported V1 resolution paths are limited to:

- deterministic recovery or alternate safe action;
- bounded reasoning/deliberation request;
- wait for a required condition or event;
- abandon or supersede the current path;
- escalate to an authorized host/intervention boundary.

V1 does not require nested impasse substates, recursive subproblem trees, or a separate general cognitive conflict/merge engine. Richer impasse decomposition remains an extension point, not a V1 delivery requirement.

### Slice 5 — Deliberation and Reasoning Requirement

- Define provider-neutral reasoning requirements.
- Support required/preferred capabilities, context needs, structured output, tools, latency tolerance, cost policy, and bounded reasoning depth.
- Support staged escalation from deterministic processing to bounded reasoning.
- Allow explicit `NoModelRequired` outcomes.
- Keep provider/model names out of cognition.
- Send concrete execution selection only through 0.96.
- Bound model calls, time, tokens/usage, retrieval, and recursion.
- Treat incomplete deliberation as a typed outcome rather than false success.

### Slice 6 — Plans, methods, execution, and recovery integration

- Consume durable Goal/Plan/Checkpoint/Recovery contracts from 0.9591.
- Support plan steps, assumptions, preconditions, expected effects, checkpoints, and explicit outcome states.
- Continue valid plans without unnecessary re-deliberation.
- Reconsider when assumptions/resources/policy change.
- Distinguish `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`.
- Propagate stale-result and lifecycle protection into plan execution.
- Use existing execution/runtime intervention boundaries rather than creating a second execution engine.

### Slice 7 — Experience, learning, and learned-resource reliability

- Capture bounded experience records from meaningful completed interactions.
- Keep Experience distinct from Memory, Knowledge, Skill, Policy, and raw execution logs.
- Produce procedural learning candidates conservatively from sufficient evidence.
- Preserve positive/negative evidence, applicability conditions, provenance, expected effects, and source execution/runtime identity.
- Send candidates through 0.9575 learning governance and promotion.
- Consume 0.9576 reliability/applicability results after promotion.
- Never make a single successful trajectory silently authoritative.

### Slice 8 — Lifecycle, persistence, observability, and production verification

- Consume 0.958 lifecycle/health semantics.
- Consume 0.9591 durable goals/plans/recovery.
- Support cancellation, suspension, retirement, shutdown, restart recovery, and post-retirement stale-result rejection.
- Preserve per-agent state ownership during all asynchronous work.
- Emit structured telemetry for event intake, workspace selection, deterministic actions, deliberation, reasoning requirements, plan progress, execution correlation, learning candidates, interventions, and recovery.
- Support evaluation of correctness, unnecessary LLM use, latency, cost, failure/recovery, and learning reliability.
- Verify concurrency with many independent runtime agents, not only one shared state under contention.

## Production V1 invariants

1. One runtime agent instance has one authoritative state owner.
2. Only the owner applies authoritative state mutations.
3. Background work returns results/events; it never writes owner state directly.
4. State mutation for one agent is serialized.
5. Different agent instances may operate concurrently without sharing mutable runtime state.
6. No stale asynchronous result may overwrite newer state.
7. Retirement and shutdown invalidate outstanding cognitive authority.
8. Request-oriented `ExecuteAsync` remains a first-class public API.
9. Deterministic processing is preferred when sufficient.
10. 0.96 is the only concrete provider/model execution-selection layer.
11. Learned resources are governed by 0.9575/0.9576 rather than a second learning architecture.
12. Host/domain truth and external side effects remain host-authoritative.
13. All queues, workspaces, deliberation, recursion, and resource growth are bounded.
14. Model output is evidence/request input, never authorization.
15. V1 impasse handling remains bounded to explicit recovery, reasoning, waiting, abandon/supersede, or authorized intervention; nested impasse/substate recursion is not required.

## Dependency graph

```text
0.9575 governed resources + learning
        ↓
0.9576 learned-resource reliability
        ↓
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
        ↓
0.959 intervention
        ↓
0.9592 provider/adapters
        ↓
0.96.x configuration/storage
        ↓
0.96 capability-aware execution
        ↓
0.97 persistent cognitive runtime
```

## Relationship to HWorld and other hosts

HWorld may host many independent runtime agents, for example one agent per NPC. The runtime architecture must support concurrent operation of those independent agents without embedding HWorld concepts into HAgent.

The same runtime also supports ordinary desktop applications, automation, analysis, or other hosts that need a persistent agent.

## Exit criterion

A host can create a long-lived runtime agent that independently owns its cognitive state, receives and processes events, uses deterministic behavior before unnecessary model calls, pursues persistent goals and plans, resolves bounded impasses through explicit V1 outcomes, requests bounded reasoning through 0.96, learns through governed candidates, survives cancellation/restart/lifecycle transitions, and operates concurrently with many other independent runtime agents without shared-state corruption or a shared cognitive bottleneck.

## Phase 0.96.x — Configuration, Storage, and Portability Evolution

## Status

**Cross-cutting foundation required before completion of 0.96 and before 0.97 consumes long-lived configuration.**

## Purpose

Provide one authoritative persistence/configuration model for providers, models, concrete execution targets, Agent selection policy, resources, learning configuration, permissions, and global defaults across the supported File, SQL Server, and MySQL backends.

This phase is infrastructure. It does not perform provider/model routing and does not become a second configuration architecture.

## V1 storage model

```text
General settings
Providers
Logical models (where identity can be established)
Concrete execution targets
Capabilities / evidence
Constraints
Operational quota/rate/capacity state
Cost state
Agents / selection policy
Skills
Knowledge / Wiki
Memory / learning configuration
Tools
Permissions / policy
Resource relationships
```

The same logical configuration model must exist regardless of storage backend.

## Delivery slices

### Slice 1 — Authoritative configuration model

- Replace obsolete permanent Agent provider/model binding with selection preferences and requirements.
- Keep Provider, logical Model, and concrete Execution Target distinct.
- Persist global defaults such as cost policy, default selection mode, fallback policy, learning defaults, and discovery/refresh defaults.
- Preserve explicit inherit/override semantics.
- Version configuration records so active runtime snapshots can detect relevant changes.

### Slice 2 — Provider credentials

- Persist provider API keys with provider configuration where applicable.
- Encrypt credentials at rest in File, SQL Server, and MySQL storage.
- Redact credentials from logs, diagnostics, tracing, audit records, Examples, and UI diagnostic output.
- Support credential replacement/removal and configuration refresh.
- Keep storage-server passwords outside ordinary provider configuration.
- Do not introduce a separate secret-vault architecture.

### Slice 3 — Resource and relationship persistence

- Persist Skills, Knowledge/Wiki, Memory policy, Learning configuration, Tools, Permissions, and explicit resource relationships.
- Preserve canonical scope/ownership, version, provenance, lifecycle, and relationship identity.
- Persist learning candidate state/provenance once the 0.9575 lifecycle requires it.
- Keep executable handlers and live runtime state out of persistence.

### Slice 4 — Runtime snapshots and invalidation

- Persist revisions/version metadata sufficient for long-lived execution snapshots.
- Avoid reloading unchanged configuration from persistence on every execution when a valid snapshot exists.
- Define lightweight refresh/invalidation suitable for File, SQL Server, and MySQL.
- Ensure revoked/changed configuration cannot remain effective indefinitely.
- Preserve immutable execution snapshots even when persistence changes during execution.

### Slice 5 — Configuration portability

- Define one versioned export/import package independent of the physical storage backend.
- Export HAgent-owned configuration, not process-local state.
- Exclude live runtimes, active executions, synchronization primitives, provider sessions, and executable handlers.
- Normal export excludes credentials.
- Optional credential-bearing export contains encrypted credentials protected by the package mechanism.
- Validate package compatibility and produce deterministic import-conflict results.
- Preserve IDs and relationships where possible and explicitly remap only when required.

### Slice 6 — Backend parity and multi-process behavior

- Keep File, SQL Server, and MySQL behavior logically aligned.
- Add ordered schema migrations where required.
- Support authorized processes sharing database-backed HAgent configuration.
- Ensure configuration revision/refresh semantics prevent one process from using revoked configuration forever.
- Keep HAgent storage isolated from host business databases.

### Slice 7 — Verification

Verify:

- backend round-trip parity;
- encrypted-at-rest credentials and redaction;
- configuration revision invalidation;
- shared-database visibility;
- Auto/Preferred/Fixed selection persistence;
- cost/fallback policy persistence;
- resource relationships;
- export/import with and without credentials;
- deterministic import conflicts;
- immutable active execution snapshots after configuration edits.

## Architectural rules

1. There is one authoritative HAgent configuration model.
2. Persistence is an implementation boundary, not a second domain model.
3. File, SQL Server, and MySQL are interchangeable storage implementations of the same logical contracts.
4. Active executions use immutable snapshots rather than mutable database records.
5. Provider credentials are encrypted at rest and never become diagnostic data.
6. Export/import never exports executable handlers or live runtime state.
7. HAgent storage never becomes an implicit gateway to a host application's business database.
8. Configuration changes invalidate or supersede affected snapshots deterministically.

## Dependency relationship

```text
0.9592 provider/adapters
        ↓
0.96.x configuration + storage
        ↓
0.96 execution planning
        ↓
0.97 persistent cognition
```

## Exit criterion

All configuration required by 0.96 and 0.97 can be represented in one authoritative model, persisted consistently by supported backends, refreshed without corrupting active snapshots, shared safely where database deployment is used, and exported/imported without carrying transient execution state.

## 0.97 Subdocument — Cognitive Runtime Workbench

This document is part of **Phase 0.97 — Persistent Cognitive Runtime**, primarily supporting Slice 8 management, diagnostics, observability, and production verification.

HAgent.WinForms must add a top-level Cognitions view for active runtime instances. It provides complete inspection of current runtime cognition: beliefs, goals, intentions, plans, attention, working state, memory, knowledge, skills, events, executions, learning, lifecycle, and history.

The workbench is a diagnostic/management surface over the authoritative runtime-agent state owner. It must not create a second cognitive-state store, second policy evaluator, second execution planner, or independent persistence model.

All edits flow through public HAgent runtime state-transition/intervention APIs. The UI never writes directly to persistence and must preserve owner/revision/stale-result rules.

## Future 0.97 verification obligation — automated concurrency test

The 2026-09-11 single-owner/runtime-concurrency spike is architectural evidence only. Before **0.97 Slice 1** can be considered production-complete, its concurrency invariant must be covered by repeatable automated tests in `HAgent.Tests`; the manual Example spike must not be the sole verification of the concurrency claim.

The automated verification must exercise, at minimum:

- many independent runtime agents operating concurrently (at least 10);
- overlap of independent owner loops without collapsing behind one shared cognitive queue;
- serialized authoritative mutation within each individual agent;
- isolated runtime identity/revision/state between agents;
- stale-result rejection;
- cancellation protection;
- retirement/shutdown protection;
- preservation of the single-owner rule under asynchronous completion races.

The focused tests must run on both supported targets required by the 0.97 slice, including .NET Framework 4.8.1 and .NET 9. The test suite must verify the production implementation, not merely reproduce the earlier Example-only spike.

A matching `HAgent.Example` scenario remains required for the completed production capability under the repository's Example-first rules. The future automated test and the Example have different purposes: automated tests provide repeatable regression/contract verification; the Example demonstrates the externally usable capability through public APIs.

This obligation does **not** advance the current implementation milestone. The active implementation remains **0.9575 Slice 8**, and the existing 0.97 spike remains classified as verified architectural evidence rather than production completion.

## 0.97 Subdocument — Cognitive Workbench Controls

This document is part of **Phase 0.97 — Persistent Cognitive Runtime** and supports its management/diagnostic workbench.

Authorized users may inspect and, where policy permits, insert or invalidate beliefs; create, edit, reprioritize, suspend, resume and abandon goals; modify intentions; request plan reconsideration; inject observations/events; request deliberation; and pause, resume, wake, sleep or retire a runtime.

The UI must use HAgent runtime state-transition APIs and the canonical 0.959 intervention boundary where an action is an intervention. It must never write directly to persistence. Every mutation is atomic, version-aware, authorized and auditable. Record operator identity, timestamp, reason, UI action, previous revision and new revision where the owning contract exposes those fields.

If the runtime revision changed since the UI read it, reject or refresh the mutation rather than silently merging it. Existing execution snapshots remain immutable and stale executions must not overwrite newer cognition.

The workbench is not a new lifecycle system, plan store, approval engine, policy evaluator, or cognitive-state owner.

## 0.97 Subdocument — Cognitive Workbench History and Learning

This document is part of **Phase 0.97 — Persistent Cognitive Runtime**, supporting its workbench and diagnostics.

Show the complete cognitive timeline: events, belief changes, attention changes, goal and intention changes, plan revisions, impasses, deliberation, executions, outcomes, memory/experience creation, learning decisions, sleep/wake and recovery, including user interventions.

Show learning as:

```text
Experience
   ↓
Memory / bounded evidence
   ↓
Reflection / learning input
   ↓
Typed candidate
   ↓
0.9575 validation / governance / promotion
   ↓
Authoritative resource version
   ↓
0.9576 reliability / applicability / forgetting
```

Candidates must remain visibly distinct from authoritative versions. The workbench must not imply that repeated LLM reasoning is automatically a Skill, Knowledge item, Memory record, or Policy change.

Show the active cognitive strategy and version, such as Adaptive Hybrid Cognition (AHC), when the runtime exposes such metadata. Future strategies must use the same generic workbench while allowing strategy-specific diagnostics.

Historical state is initially read-only. Future experimentation may branch from checkpoints, but a branch must not silently replace the live runtime's authoritative state.

## Phase 0.98 — Model Reasoning Engineering

## Status

**Planned after 0.97 Persistent Cognitive Runtime.**

## Purpose

Define and implement the bounded engineering boundary between HAgent's deterministic runtime knowledge and model-based reasoning.

This phase does **not** create a second cognitive runtime, replace Prompt/Instruction Governance, replace Context Engineering, or replace the 0.96 Execution Planner. It establishes how HAgent requests probabilistic reasoning without asking the model to rediscover facts that HAgent already knows deterministically and without giving model output authority over runtime state or external effects.

The phase is provider-neutral and model-neutral. The model remains a replaceable reasoning component.

## Core principle

> Do not ask the model to infer deterministic facts when HAgent can provide those facts directly, unless inferring them is itself the requested task.

The complementary rule is:

> Permit inference when inference is the declared task, the required evidence is available, and the result remains inside its explicit authority boundary.

## Responsibility boundary

```text
HAgent knows / derives deterministically
        ↓
HAgent supplies authoritative evidence and relevant context
        ↓
Model performs only the required interpretation / inference / planning / generation
        ↓
HAgent validates the returned contract and evidence relationship
        ↓
Policy / authorization / runtime commits decide what may actually happen
```

The model is not responsible for re-identifying execution/runtime facts, permissions, provenance, authority, or other deterministic facts already available to HAgent.

## Scope

### Slice 1 — Reasoning responsibility and deterministic-before-inference boundary

- Define the provider-neutral semantic distinction between deterministic runtime facts and model-derived reasoning.
- Establish that source identity, execution identity, runtime state, provenance, permissions, capability state, and other deterministic facts remain HAgent-owned when already available.
- Ensure cognition may choose deterministic progress/no-model behavior before requesting probabilistic reasoning.
- Ensure reasoning requests describe the semantic problem that actually requires model judgment rather than bundling unrelated deterministic classification work.

### Slice 2 — Reasoning task decomposition and bounded model responsibility

- Establish bounded reasoning responsibilities for interpretation, inference, planning, and generation.
- Prevent one model request from silently combining source classification, policy authorization, learning promotion, and other separate authorities merely because the prompt asks for them together.
- Keep authoritative facts explicit in structured context/contracts rather than depending on prompt rediscovery.
- Preserve provider/model neutrality while allowing different models to satisfy the same semantic requirement with different capability levels.

### Slice 3 — Reasoning result contract and uncertainty handling

- Define the contract required for a reasoning result to be useful to HAgent.
- Distinguish valid structured output from semantically acceptable reasoning.
- Support bounded outcomes such as successful result, insufficient evidence, ambiguity, conflict, and unsupported inference where the task requires them.
- Treat model-reported confidence as evidence rather than authoritative truth or calibrated probability.
- Preserve provenance/evidence references needed to evaluate the result.

### Slice 4 — Validation, authority boundary, and verification

- Validate reasoning results before they influence cognitive state, learning candidates, tool requests, or external execution.
- Keep policy/authorization/approval/budget enforcement outside model output.
- Reuse existing structured-output, policy, context, observability, and evaluation infrastructure rather than creating parallel validators or metric systems.
- Add deterministic tests and public-API Examples for representative reasoning responsibilities, including deterministic facts supplied directly and genuinely inferential tasks delegated to a model.
- Measure semantic correctness, unsupported inference, over-inference, and provider/model variance where the capability is introduced.

## Explicitly out of scope

- A new cognitive architecture or Cognitive Runtime v2.
- A replacement for Phase 0.954 Prompt + Instruction Governance.
- A replacement for Phase 0.955 Context Engineering.
- A replacement for Phase 0.957 Evaluation + Quality Measurement.
- A replacement for Phase 0.96 Capability-Aware Execution.
- A new authorization or policy engine.
- A universal model-orchestration framework.
- Mandatory multi-model voting, debate, or ensemble reasoning.
- A requirement that every reasoning task use an LLM.
- A requirement to expose internal model chain-of-thought as an HAgent architectural contract.

## Ownership boundary

0.97 owns persistent cognition and decides whether the agent needs deterministic progress, bounded probabilistic deliberation, information acquisition, or another cognitive action.

0.98 owns the engineering discipline and contracts for the model-based reasoning portion once such reasoning is requested.

0.96 owns concrete execution-target selection and admission.

0.954 owns instruction authority/provenance and composition.

0.955 owns context retrieval/assembly/bounds.

0.957 owns evaluation evidence and quality measurement.

Policy, authorization, approval, budgets, tools, host state, and external side effects remain governed by their existing authorities.

## Future contract question intentionally left open

The phase does not pre-decide whether the canonical implementation should introduce a `ReasoningTask` type. That design is a separate architectural decision to be resolved before implementation of the first 0.98 slice.

## Dependency chain

```text
0.954 Prompt / Instruction Governance
        ↓
0.955 Context Engineering
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 / 0.9576 governed learning + reliability
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
        ↓
0.98 Model Reasoning Engineering
        ↓
0.10 Workspaces / Routing / Chat
```

0.98 consumes earlier contracts; it must not recreate them.

## Exit criterion

HAgent has a provider-neutral, testable boundary for model reasoning in which deterministic facts are supplied by HAgent when already known, inference is requested only where it is actually needed, model results remain non-authoritative until validated and governed, uncertainty/unsupported inference can be represented without forcing fabricated answers, and reasoning behavior can be evaluated across providers/models without creating a second cognitive or execution architecture.

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

## Status

**Deferred until the 0.10 workspace surface and the 0.97 persistent runtime foundations are mature.**

## Goal

Turn basic workspace messaging into reliable multi-agent collaboration and then into bounded task/workflow execution, while reusing the existing runtime, intervention, policy, persistence, execution, and workspace contracts.

## Collaboration steps

1. First-class delegation/handoff operations over the existing workspace routing contracts.
2. Shared/private workspace context policies using the existing resource, identity, and authorization boundaries.
3. Parallel specialist work with bounded collaboration budgets and independent runtime-agent ownership.
4. Human intervention and approval points using the canonical 0.959 intervention contract; do not create a second approval engine.
5. Explicit runtime/participant lifecycle states by consuming the existing runtime/lifecycle contracts.
6. Cross-agent memory sharing only through explicit resource scope and policy.
7. Collaboration history, audit, and traceability through the existing observability/audit boundaries.

## Workflow steps

1. Task/job model and lifecycle over the durable goal/plan/recovery foundations established by 0.9591.
2. Planning, execution, and verification stages using the 0.97 cognitive planning contracts where cognition is involved.
3. Multi-step and branching workflows without replacing the persistent cognitive runtime's plan model.
4. Background execution and scheduling through the existing host-controlled runtime/scheduler boundaries.
5. Pause/resume and durable checkpoints through 0.9591 recovery contracts.
6. Event-triggered execution through the 0.952 event subsystem.
7. Per-step timeout, cancellation, retry, approval, intervention, and budget policies through existing runtime/policy/intervention contracts.

## Ownership boundaries

- **0.10** owns the host-facing workspace/routing/chat product surface.
- **0.959** owns the provider-neutral intervention boundary.
- **0.9591** owns durable goal/plan/checkpoint/recovery state.
- **0.96** owns concrete execution-target selection.
- **0.97** owns one runtime agent's persistent cognition and plan/goal use.
- **1.0** owns multi-agent collaboration and generic workflow orchestration built on those contracts.

No 1.0 feature may silently recreate a parallel runtime lifecycle, approval system, execution router, durable plan store, or cognitive-state ownership model.

## Boundary

These are generic orchestration facilities. HAgent does not become the authority for business rules, simulation state, or host-side side effects.

## Exit criterion

A host can coordinate multiple independent agents and long-running work with bounded execution, explicit authority, reusable intervention/policy boundaries, durable state where required, and observable collaboration without duplicating HAgent's lower-level runtime, execution, cognition, or authorization systems.

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

**Completed — the canonical policy boundary is implemented and verified.** Policy contracts, deterministic evaluation, precedence, provenance, cost guard, pre-transport runtime enforcement, effective-policy execution snapshots, canonical policy persistence, policy-gated tool invocation, policy-first host authorization composition, profile/runtime resource capability resolution, and typed learning-promotion policy transitions are established.

Remaining UI refinement and additional backend/live verification are ongoing hardening work and must not reopen or redefine the policy boundary.

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
13. [x] Complete deterministic Example verification for resource capability resolution, persistence, snapshot isolation, tool gating, and learning-policy transitions.

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

The policy/resource/learning-policy foundation is complete as a phase boundary. Later phases own the remaining resource lifecycle, candidate persistence/promotion, intervention, execution selection, and cognitive integration.

## Post-phase hardening

The following are explicitly post-phase refinements rather than missing policy architecture:

1. Human-oriented policy management UI refinement with semantic selectors for scope, agent, tool, resource, provider, operation, and outcome, while retaining advanced raw identifiers only where necessary.
2. A read-only Effective Decisions diagnostic surface showing evaluated context, selected rule, policy version, precedence/provenance, and built-in guard contribution.
3. An Agent Capabilities surface showing persistent profile state, transient runtime override, deterministic effective state, and source of the effective value where useful.
4. Additional configured-backend/live verification where the deployment has the corresponding environment.

The policy engine remains the single decision/precedence authority. These refinements must not introduce a second evaluator or alternative capability semantics.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects. Policy may further restrict a host operation, but an HAgent policy `Allow` never grants application authorization.

Resource enablement is a separate configuration capability layer. It does not replace provider capability discovery or host authorization. An enabled resource must still pass any applicable policy and authorization boundaries before side effects occur.

Learning promotion uses the same policy boundary rather than a parallel learning authorization evaluator. A typed candidate may move to `Approved` only through an `Allow` policy decision or explicit review after `RequireApproval`/`Defer`. Promotion to authoritative storage is a separate operation and is not performed by policy evaluation itself.

## Dependency boundary

```text
0.953 Unified Policy
    ↓ consumed by
0.954 Instruction Governance
0.955 Context Engineering
0.9575 Learning Governance
0.959 Intervention
0.96 Execution Selection
0.97 Persistent Cognition
```

None of those phases may recreate a policy evaluator merely because they expose a policy-related UI or use a specialized decision boundary.

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

**Verified and complete — the provider-neutral context pipeline, policy/capability admission, host authorization boundary, execution integration, and deterministic Example/test matrix are verified.**

## Goal

Make context assembly a first-class HAgent subsystem that selects, ranks, bounds, compresses, and explains the information sent to an execution instead of treating prompt construction as string concatenation.

## Requirements

1. [x] Define provider-neutral context items with source, type, provenance, trust, importance, freshness, scope, and estimated size.
2. [x] Define context budgets for tokens/characters/items and other applicable resource dimensions.
3. [x] Separate context retrieval from context assembly and from cognitive attention.
4. [x] Support relevance ranking using goal relevance, attention, recency, importance, trust, redundancy, and estimated cost where available.
5. [x] Support bounded memory, knowledge, skill, conversation, host-context, tool-description, and instruction retrieval through the provider-neutral multi-resource retrieval plan.
6. [x] Support compaction, summarization, deduplication, and truncation strategies without silently discarding required policy or provenance.
7. [x] Preserve source/provenance metadata for assembled context and expose safe diagnostics explaining inclusion/exclusion.
8. [x] Support reusable and cacheable context components when configuration/version rules permit.
9. [x] Keep provider-specific tokenization behind optional adapters; Core must not require a particular tokenizer.
10. [x] Ensure context assembly respects policy, permissions, disabled resources, and instruction authority. HAgent policy and resource-capability state are enforced by the canonical context admission boundary; protected data-backed context sources additionally compose the existing host-owned `IDataAccessAuthorizer`; instruction authority remains owned by the canonical 0.954 instruction-governance subsystem.
11. [x] Capture the resulting bounded context in immutable execution snapshots.
12. [x] Add deterministic Example verification for budgets, ranking, prioritization, compaction, source provenance, cache reuse, and policy-enforced exclusion through the verified Context Example matrix, including end-to-end assembly and host authorization.

## Verified slices

- Slice 2: provider-neutral context contracts and budgets — verified 2026-09-08 on .NET Framework 4.8.1 and .NET 9 Example execution.
- Slice 3: bounded acquisition and execution-owned context snapshots — verified 2026-09-08 with 20/20 HAgent.Tests and deterministic Example coverage.
- Slice 4: ranking, deterministic prioritization, and deduplication — verified 2026-09-08 with 24/24 HAgent.Tests and deterministic Example coverage.
- Slice 5: deterministic compaction/truncation and provenance-preserving diagnostics — verified 2026-09-09 with 29/29 HAgent.Tests and deterministic Example coverage.
- Slice 6: cache-safe reusable context components — verified 2026-09-09 with 34/34 HAgent.Tests and deterministic public-API Example coverage.
- Slice 7: execution/provider integration and request isolation — verified 2026-09-09 with 37/37 HAgent.Tests and deterministic public-API Context Execution Integration Example coverage.
- Slice 8: bounded multi-resource retrieval — verified 2026-09-09 with 41/41 HAgent.Tests and deterministic public-API Example coverage on .NET Framework 4.8.1 and .NET 9.
- Slice 9: policy/capability-aware context assembly admission — verified 2026-09-09 with 46/46 HAgent.Tests and deterministic public-API Context Policy Assembly Example coverage on .NET Framework 4.8.1 and .NET 9.
- Slice 10: end-to-end bounded context assembly pipeline — verified 2026-09-09 with 50/50 HAgent.Tests and deterministic public-API Context Assembly Example coverage on .NET Framework 4.8.1 and .NET 9.
- Slice 11: host authorization-aware context admission — verified 2026-09-09 with 56/56 HAgent.Tests and deterministic public-API Context Host Authorization Example coverage on .NET Framework 4.8.1 and .NET 9.

## Final implementation outcome

- The canonical context pipeline is provider-neutral and bounded:
  `retrieval plan → policy/capability admission → host authorization where required → ranking/deduplication → final compaction → ContextSnapshot`.
- Host authorization remains an explicit host-owned boundary. A policy allow does not imply host authorization, and protected sources fail closed when the host authorization boundary is unavailable or denies access.
- The context subsystem does not create a second policy engine or instruction precedence system. Instruction authority and conflict resolution remain owned by the canonical instruction-governance subsystem.
- Admission and compaction diagnostics remain metadata-only and preserve safe provenance without copying context payloads.
- The complete required deterministic Context Example/test matrix for 0.955 is covered by the verified slices above.

## Architectural outcome

```text
Available information
        ↓
Policy + permissions
        ↓
Attention / relevance
        ↓
Acquisition / retrieval
        ↓
Policy/capability admission
        ↓
Host authorization when the source requires it
        ↓
Ranking / deduplication
        ↓
Compression / compaction / truncation
        ↓
Bounded Context Snapshot
        ↓
Execution Request / Provider Adapter
```

Context engineering remains distinct from cognitive decision making: cognition decides what matters; context engineering constructs the bounded evidence supplied to an execution. Instruction authority remains owned by the canonical instruction-governance subsystem.

## Phase 0.956 — Observability and Distributed Tracing

## Status

**Completed and verified on .NET Framework 4.8.1 and .NET 9.**

## Goal

Turn HAgent execution, resource use, policy decisions, cognition, tools, provider activity, and lifecycle changes into a coherent structured trace that can be correlated across operations and processes.

## Verified outcome

All requirements in this phase are implemented and verified through the ordered Slice 1–8 sequence.

- Provider-neutral trace/span concepts and deterministic lifecycle semantics are implemented.
- Execution, runtime, host, event, policy, tool, provider, and other applicable identities remain distinct and correlatable.
- Start/end, duration, status, hierarchy, parentage, bounded metadata, redaction, sampling, retention, and sink boundaries are implemented.
- Context, policy, provider, tool, event, execution, and outcome boundaries emit trace information without duplicating or replacing their authoritative subsystems.
- Secrets, raw prompts/responses, provider payloads, tool payloads, host raw context, and arbitrary serialized objects remain excluded by default.
- Local tracing, bounded asynchronous sinks, diagnostic projection, sampling/retention, and cross-process propagation are implemented without forcing a telemetry vendor or transport.
- Failure, retry, retry-wait, recovery, fallback, cancellation, policy denial, and stale-result observability are represented from the subsystem that owns each decision. In particular, the execution runtime publishes authoritative outcome facts and tracing consumes them; provider adapters do not reconstruct execution state with `AsyncLocal`, call counting, or exception-message inspection.
- Observability remains diagnostic and cannot alter authorization, execution decisions, cancellation, retry, or terminal-state authority.

## Verification

**User verification — 2026-09-09:** `HAgent.Tests` completed with **89/89 tests passed**.

**User Example verification — .NET Framework 4.8.1, 2026-09-09 06:11:32:** `Observability Outcome Tracing` succeeded, verifying authoritative retry, retry-wait, recovery, fallback decision representation, stale-result rejection, absence of provider-side retry inference, unchanged execution terminal authority, no raw prompts/responses/provider payloads, no remote telemetry, and no real provider request.

**User Example verification — .NET 9, 2026-09-09 06:12:14:** same public-API scenario succeeded with the same checks.

## Architectural invariants

```text
Trace context                     Execution outcome state
-------------                     -----------------------
Where am I?                       What did runtime decide?
TraceId / ParentSpanId            Attempt / retry number
Sampled state                     Fallback transition
                                  Wait/backpressure decision
                                  Recovery
                                  Stale-result acceptance/rejection
```

- Trace context is ambient relationship state. It must not be used to infer authoritative runtime behavior.
- The logical execution runtime remains authoritative for retry classification, retry count, fallback selection, waits, cancellation/timeout, terminal completion, and stale-result acceptance/rejection.
- `IExecutionObservationSource` is the provider-neutral boundary through which an execution runtime publishes bounded facts that tracing may consume.
- `TracingProviderAdapter` records provider operation boundaries only. It does not reconstruct retry/fallback state with `AsyncLocal`, call counting, or exception-message inspection.
- Fallback is observable only when the execution runtime actually selects a fallback target; multiple provider spans alone do not prove fallback.
- Observability remains diagnostic and cannot change execution, authorization, cancellation, retry, or terminal-state behavior.

## Architectural outcome

```text
Event / Request
      ↓
Trace identity/context
      ↓
Root execution span
      │
      ├── Policy
      ├── Context
      ├── Planning
      ├── Provider invocation spans
      ├── Tools
      └── Outcome observations
             ↑
             │ explicit runtime facts
      Execution runtime
```

Tracing is observability, not authorization and not transcript storage.

## Phase 0.957 — Evaluation and Quality Measurement

## Status

**Verified through Slice 6 on 2026-09-09.**

## Goal

Give HAgent a provider-neutral way to measure whether executions, tool use, plans, learning changes, and agent outcomes achieved their intended quality or task goals.

## Requirements

1. [x] Define evaluation contracts independent of any specific LLM vendor or grading service.
2. [x] Support evaluation targets including execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning candidate quality.
3. [x] Support deterministic evaluators such as schema validity, required-field checks, policy compliance, tool success, latency, cost, and task completion signals.
4. [x] Support externally supplied human/application ratings and labels.
5. [x] Support model-assisted evaluators without treating evaluator-model output as unquestionable truth.
6. [x] Preserve evaluation provenance, evaluator identity/type, input references, timestamp, and confidence where meaningful.
7. [x] Correlate evaluations with execution/runtime/agent/goal/plan/trace identities.
8. [x] Keep evaluation data separate from authoritative agent state; an evaluation does not automatically mutate configuration, memory, skill, or knowledge.
9. [x] Support repeated test cases and regression suites for provider/model/agent comparisons.
10. [x] Support aggregate metrics such as success rate, quality score, latency, cost, fallback frequency, tool success, and plan completion.
11. [x] Add deterministic Example verification for evaluation creation, aggregation, human rating, failed evaluations, and comparison of alternative execution targets.

## Slice 1 — Provider-neutral evaluation contracts and evaluator boundary

**Verified on 2026-09-09.**

- Added provider-neutral target, request, evidence-reference, and result contracts with bounded validation and clone behavior.
- Added `IAiEvaluator` as the asynchronous evaluator boundary with explicit evaluator identity, kind, and version.
- Kept evaluation separate from authorization and authoritative state.
- Added focused `EvaluationContractsTests.cs` and public Example coverage.
- User verification: **96/96 tests passed** on .NET 9; required Example checks succeeded on .NET Framework 4.8.1 and .NET 9.

## Slice 2 — Deterministic evaluators and evaluation evidence

**Verified on 2026-09-09.**

- Added bounded `AiEvaluationObservation` values with explicit Boolean, Decimal, and Text value kinds.
- Added deterministic schema validity, required-field completeness, policy compliance, tool success, cost, latency, and task-completion rules.
- Added `AiDeterministicEvaluationEvaluator` with deterministic scoring, bounded evidence, provenance/correlation, threshold handling, cancellation, ambiguity rejection, and `Inconclusive` outcomes.
- Added focused `DeterministicEvaluationTests.cs` and matching public `Deterministic Evaluation` Example.
- User verification: **109/109 tests passed**; .NET Framework 4.8.1 and .NET 9 Example scenarios succeeded.

## Slice 3 — Human/application ratings and labeled evaluation evidence

**Verified on 2026-09-09.**

- Added bounded `AiEvaluationRating` for externally supplied outcome, score, confidence, label, reason, evidence references, and metadata.
- Added `AiSuppliedRatingEvaluator` through `IAiEvaluator`, accepting only `Human` or `Application` evaluator kinds.
- Preserved supplied rating values, evaluator provenance, correlation, bounded evidence/metadata, and owned clone isolation.
- Added focused `SuppliedEvaluationTests.cs` and matching public `Supplied Evaluation Ratings` Example.
- User verification: **115/115 tests passed** on .NET 9.
- User Example verification: `Supplied Evaluation Ratings` succeeded on both .NET Framework 4.8.1 and .NET 9.

## Slice 4 — Model-assisted evaluators and non-authoritative judge boundary

**Verified on 2026-09-09.**

- Added `IAiEvaluationJudge` as the provider-neutral asynchronous judge boundary.
- Added `AiEvaluationJudgeRequest` as a detached clone of `AiEvaluationRequest`, protecting active judging from caller mutation and preserving provider-neutral bounded inputs, observations, and criteria.
- Added `AiModelAssistedEvaluationEvaluator` implementing `IAiEvaluator` with explicit `ModelAssisted` provenance and bounded evaluator identity/version.
- Reused `AiEvaluationRating` as the bounded judge result instead of introducing a second evaluation-result model; the evaluator maps it into normal `AiEvaluation` evidence while retaining judge provenance.
- Explicitly records `evaluation.source=model-assisted` and `evaluation.authoritative=false`.
- Provider transport, credentials, model selection, retries, and host-specific evidence resolution remain outside Core in the injected judge implementation/owning subsystem.
- Cancellation is checked before judge invocation and after judge completion so a late judge result cannot become an evaluation after cancellation.
- Judge failure, null output, and invalid bounded rating are rejected rather than converted into fabricated evaluation evidence.
- Added focused `ModelAssistedEvaluationTests.cs` and public `Model-Assisted Evaluation` Example.
- User verification: **123/123 HAgent.Tests passed** and the Slice 4 Example succeeded on **.NET Framework 4.8.1 and .NET 9**.

## Slice 5 — Evaluation aggregation and alternative-target comparison

**Verified on 2026-09-09.**

- Added `AiEvaluationMetricKind` for success rate, quality score, latency, cost, fallback frequency, tool success, plan completion, and custom metrics.
- Added bounded `AiEvaluationMetric` with provider-neutral direction semantics; custom metrics require an explicit higher-is-better/lower-is-better declaration.
- Added `AiEvaluationSample` and `AiEvaluationAggregationRequest` with stable case/variant identity, bounded sample count, validation, and detached clone ownership.
- Added `AiEvaluationAggregate` / `AiEvaluationAggregateMetric` for outcome counts and average/minimum/maximum metric summaries.
- Added `AiEvaluationComparison` / `AiEvaluationMetricComparison` for left/right metric averages, left-minus-right deltas, and strictly preferred variants where both sides are comparable.
- Added `AiEvaluationAggregator` with cancellation checks, deterministic ordering, bounded input, detached aggregation snapshots, and no authoritative side effects.
- Added focused `EvaluationAggregationTests.cs` and public `Evaluation Aggregation` Example verification.
- User verification: **130/130 HAgent.Tests passed**, with 0 failed and 0 skipped.
- User Example verification on .NET Framework 4.8.1 and .NET 9 produced baseline success rate `0.333333`, candidate success rate `1`, candidate average quality `0.85`, candidate average latency `110 ms`, and no authoritative routing or authorization decision.

## Slice 6 — Evaluation regression suites and repeated target execution

**Verified on 2026-09-09.**

- Added `AiEvaluationRegressionCase` for bounded reusable test-case identity, input references, and host-defined parameters.
- Added `AiEvaluationRegressionTarget` for bounded alternative target identity, name, and metadata without hard-coding provider/model semantics into Core.
- Added `AiEvaluationRegressionSuite` for bounded case/target matrices, unique identifiers, `MaxConcurrency` from 1 to 32, and a maximum of 1024 case-target executions.
- Added `AiEvaluationRegressionCaseResult` and `AiEvaluationRegressionRun` with explicit completed/failed/canceled lifecycle states, bounded failure codes, deterministic ordering, validation, and completed-sample ownership.
- Added `IAiEvaluationRegressionExecutor` as the host-owned execution boundary. The runner does not select providers, credentials, authorization, or production routing.
- Added `AiEvaluationRegressionRunner.RunAsync` with detached suite snapshots, semaphore-bounded concurrency, complete case-target repetition, cancellation propagation, executor failure isolation, invalid/null sample rejection, case/variant identity protection, and late-result discard after cancellation.
- Added `AiEvaluationRegressionRun.CreateAggregationRequest()` to expose only completed, validated `AiEvaluationSample` evidence through the existing Slice 5 aggregation contract.
- Added focused `EvaluationRegressionTests.cs` covering suite validation, matrix execution, deterministic ordering, concurrency bounds, failure isolation, identity mismatch, cancellation/late-result protection, snapshot isolation, and aggregation handoff ownership.
- Added public `MainForm.EvaluationRegression.cs` and registered it as `Diagnostics → Evaluation → Evaluation Regression Suites`.
- User verification: **139/139 HAgent.Tests passed** with 0 failed and 0 skipped.
- User Example verification on **.NET Framework 4.8.1 and .NET 9** succeeded with 3 cases × 2 targets, 6 completed executions, maximum observed concurrency of 2, isolated failure handling, late-result cancellation protection, and successful-sample-only aggregation handoff.

## Architectural invariants

```text
Evaluation state                  Authoritative runtime/cognitive state
-----------------                 ------------------------------------
Outcome / score / label           Execution terminal state
Evaluator provenance              Authorization decision
Evidence references               Configuration / memory / knowledge
Diagnostic correlation            Cognitive revision / learning promotion
```

- Evaluation is evidence about behavior, not hidden authorization.
- Model-assisted evaluations are explicitly non-authoritative and retain evaluator provenance.
- Evaluation contracts use bounded references, observations, ratings, metrics, regression cases/targets, and metadata instead of raw prompts, responses, tool payloads, credentials, or arbitrary host objects.
- Evaluation must not mutate authoritative agent state merely because an evaluation passes.
- Deterministic rules evaluate explicit host-computed facts; they do not independently inspect or authorize host-domain state.
- Human/Application ratings are externally supplied evidence and never become authorization decisions by virtue of evaluator kind.
- Model-assisted judging stays behind an injected provider-neutral judge contract; Core does not become a hidden model router.
- Aggregation and comparison are measurement-only and do not route execution, authorize actions, alter configuration, promote learning, or mutate cognitive state.
- Regression suites orchestrate repeated measurement only through an injected host-owned executor; they do not become provider routers or production schedulers.
- Executor failures and cancellation are lifecycle outcomes of the regression run, not fabricated evaluation truth.

## Architectural outcome

```text
Execution / Response / Tool / Goal / Plan / Memory-Knowledge / Learning Candidate
        ↓
   AiEvaluationRequest
        ↓
      IAiEvaluator
        ↓
     AiEvaluation
        ↓
 outcome + score + label + evidence + provenance
        ↓
 bounded regression case × target execution
        ↓
 completed AiEvaluationSample evidence
        ↓
 bounded aggregation by target variant
        ↓
 alternative-target comparison evidence
```

Evaluation measures behavior; later policy-controlled subsystems may consume evaluation evidence, but evaluation itself does not become a decision-maker for authorization or execution routing.

The next ordered milestone is **0.9575 Knowledge, Skills, Memory Governance + Learning**. The active implementation work for that milestone is maintained in `docs/plan/20-active.md`.

## Phase 0.9575 — Knowledge, Skills, Memory Governance + Learning

## Status

**In progress — Slice 12 implementation.**

Slices 1–11 are verified. This roadmap is normalized against the current implementation so historical checklist entries that already leaked into the code are not treated as automatically missing work.

## Goal

Complete HAgent's production V1 governance and learning boundary for first-class Skills, Knowledge/Wiki, Memory, and Learning resources without introducing a parallel resource architecture.

The canonical model remains:

```text
Skills    = reusable executable capability definitions
Knowledge = reusable retrievable information
Memory    = scoped experience/state
Learning  = governed process that turns experience into typed candidates
            and, when permitted, promotes them into authoritative state
```

Model output is never an authority. Learning creates typed candidates before any authoritative promotion.

## Verified foundation

### Slice 1 — Resource capability governance — VERIFIED 2026-09-09

Canonical ownership, effective capability state, and unified policy authorization are composed through one reusable governance boundary.

Verification: user reported 146/146 tests and the required Example succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 2 — Knowledge/Wiki governed resource contract — VERIFIED 2026-09-09

Knowledge/Wiki resources have explicit identity, scope/ownership, provenance, lifecycle/versioning, bounded metadata, relationships, chunks, and provider/index-neutral retrieval.

Verification: user reported 153/153 tests and the required Example succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 3 — Versioned Skill definitions — VERIFIED 2026-09-09

Reusable versioned Skill definitions/reference semantics, explicit scope/ownership, lifecycle/provenance, bounded contracts, dependencies, constraints, snapshot semantics, governed resolution, and runtime-owned executable handlers are established.

Verification: user reported 158/158 tests, 0 failed, 0 skipped and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 4 — Memory family/type and provenance — VERIFIED 2026-09-09

The existing MemoryEntry contract now carries canonical family/type, provenance, expiration metadata, clone/validation behavior, and aligned File/SQL Server/MySQL persistence.

Verification: user reported 167/167 tests, 0 failed, 0 skipped and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 5 — Memory governance and retention — VERIFIED 2026-09-09

Memory governance reuses the generic resource capability model, adds deterministic bounded retrieval/expiration filtering, per-family/type retention caps, and the governed memory-store decorator.

Verification: user reported 175/175 tests, 0 failed, 0 skipped and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 6 — Learning Mode — VERIFIED 2026-09-09

`AiLearningMode` is provider-neutral and distinct from resource capability policy. Persistent profile state, runtime-only override, and immutable execution-snapshot capture are established.

Verification: user reported 181/181 tests, 0 failed, 0 skipped and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 7 — Learning Policy + Typed Candidates — VERIFIED 2026-09-11

One provider-neutral Learning Policy contract and typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` payloads now compose the single canonical `AiLearningCandidate` lifecycle.

Verification: user reported 187/187 tests, 0 failed, 0 skipped on .NET 9; the `HAgent.Example → Policy → Learning Policy` Example succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 8 — Canonical Learning Lifecycle Gate — VERIFIED 2026-09-11

`AiLearningLifecycleCoordinator` is the canonical admission gate between validated typed candidates and later authoritative promotion. It composes typed-candidate validation, Learning Policy, Learning Mode, and unified learning-promotion authorization; it does not publish or persist authoritative resources.

Verification: user reported `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` recorded **194/194 passed, 0 failed, 0 skipped** on .NET 9.

The authorization-sensitive identity boundary is fail-closed and requires canonical `AgentIdentityContext`.

## Remainder audit: historical checklist normalization

### Already implemented / leaked into the current architecture

- Canonical resource scopes are `Global`, `Tenant`, `User`, `Workspace`, `Agent`, `Runtime`, and `Execution`. Any historical wording that names `Domain` is obsolete; `Workspace` is the canonical scope.
- `AiAgent.ResourceCapabilities` already provides persistent profile capability defaults, and `AiResourceCapabilityPolicy` supports both resource-type and exact-resource entries with `Inherit`, `Enabled`, and `Disabled` states.
- Effective profile/runtime capability resolution already exists and is captured into `AgentExecutionSnapshot.EffectiveResourceCapabilities`.
- Runtime overrides remain transient and do not write back into profile configuration.
- Knowledge retrieval requests already impose explicit bounded query, result, character, chunk, metadata, and filter limits.
- Knowledge and Skill resources already have explicit scope/owner identity and governed resolution; reusable shared resources are represented by authoritative resource identity rather than private copies.
- Published Knowledge and published Skills are already rejected as typed learning candidate payloads.
- Runtime identity, correlation, cancellation, timeout, stale-result protection, and independent runtime-instance isolation already exist as runtime foundations.

### Partially implemented; integration still outstanding

- Shared reusable Knowledge exists at the resource-contract/governance level, but HAgent-owned persistence, management, relationships, and end-to-end reusable-resource administration remain incomplete.
- Retrieval is bounded at the contract level, and Slice 11 integrates governed learned-resource retrieval into the canonical context/instruction pipeline; broader reusable-resource administration remains outstanding.
- Runtime snapshots capture capability/learning configuration, and Slice 11 captures bounded execution observations as learning input; automatic observation-to-candidate analysis remains outstanding.

### Genuinely outstanding 0.9575 work

- Promotion audit/provenance/evaluation persistence beyond the structured promotion evidence produced by Slice 10.
- Broader governed learning-resource management/repository integration where authoritative Knowledge/Skill persistence is host/storage-owned.
- Knowledge/Wiki and Skill CRUD management UI.
- Effective Agent Configuration resource inventory/editing UI.
- Persistence and restart verification for the mature learning/resource layer beyond the candidate store and current in-memory observation reference store.

## Slice 9 — Candidate persistence, retention, and review — VERIFIED 2026-09-11

### Purpose

Make the existing canonical typed learning candidate durable without creating a second candidate lifecycle or authoritative resource model.

### Scope delivered

- provider-neutral durable candidate record;
- provider-neutral candidate-store boundary;
- deterministic InMemory and durable File stores;
- lifecycle status/revision persistence;
- retention/expiry and purge;
- typed payload serialization and restoration;
- persisted Learning Policy and promotion authorization provenance;
- Learning Review through the existing unified policy boundary;
- explicit reviewer identity and authorization evidence;
- optimistic revision-checked review updates;
- restart/recovery semantics;
- no authoritative resource mutation at the persistence/review boundary.

### Verification

User verified `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` on .NET Framework 4.8.1 and .NET 9.

Both Examples reported contract success, typed durable capture, restart persistence, PendingReview restoration, authorized review to Approved, revision 1 → 2, reviewer/policy evidence, and no authoritative resource publication.

Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

Architecture: `docs/architecture/88-learning-candidate-persistence.md`.

## Slice 10 — Authoritative promotion and version-safe resource creation — VERIFIED 2026-09-11

### Purpose

Convert an approved durable typed candidate into authoritative resource state through one canonical promotion boundary without creating parallel lifecycle or resource models.

### Scope delivered

- one provider-neutral promotion service;
- explicit `AgentIdentityContext` and fresh `learning.promote` evaluation through the existing unified `IAiPolicyEngine`;
- durable `Approved` status and expiry checks;
- typed payload validation/restoration before publication;
- Memory promotion through the existing `IMemoryStore` contract;
- Knowledge promotion through an explicit provider-neutral publication target with version conflict protection;
- Skill promotion through an explicit provider-neutral immutable publication target;
- deterministic equal/lower version conflict rejection;
- preservation of candidate/source execution/runtime/profile provenance and authorization evidence;
- transition to `Promoted` only after authoritative publication succeeds;
- structured promotion result/audit evidence for later observability integration.

### Verification

User verified `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` on .NET Framework 4.8.1 and .NET 9.

Both Examples reported contract success for Memory, Knowledge, Skill, fresh unified authorization, lifecycle transition after publication, provenance retention, and immutable version behavior.

Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

Architecture: `docs/architecture/89-learning-authoritative-promotion.md`.

## Slice 11 — Context, instruction, runtime, and observability integration — VERIFIED 2026-09-11

### Purpose

Connect governed learned resources to real execution without moving authorization into prompts or creating a parallel execution/context/observability architecture.

### Scope delivered

- prepare learned instruction sources through the existing `AgentExecutionRequest.InstructionSources` boundary;
- prepare learned context retrieval sources through the canonical policy/capability admission, ranking, and bounded context assembly pipeline;
- preserve execution-owned context snapshots and cloned instruction inputs;
- consume authoritative `IExecutionObservationSource` facts as bounded learning input without turning observations into candidates automatically;
- provide a bounded provider-neutral observation-store reference implementation;
- preserve separation between runtime authority, observability, Learning Policy, candidate lifecycle, and authoritative promotion.

### Verification

User verified `HAgent.Example → Cognition → Learning → Learning Execution Integration` on .NET Framework 4.8.1 and .NET 9.

Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.

Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

Architecture: `docs/architecture/90-learning-execution-integration.md`.

## Slice 12 — Management UI — IN PROGRESS

### Purpose

Add the production WinForms administration surface using existing HAgent configuration conventions without moving resource lifecycle or authorization into the UI shell.

### Current increment — Learning Review

- dedicated `LearningReviewPage` under `src/HAgent.WinForms/UI/Configuration/Learning/`;
- explicit reviewer user, tenant, and workspace identity fields;
- bounded PendingReview candidate list projection;
- Approve/Reject routed through `AiLearningCandidateReviewService` and the existing unified policy engine;
- durable candidate-store dependency exposed through `ConfigurationContext`;
- no authoritative Memory/Knowledge/Skill publication from the UI;
- explicit configuration-shell navigation entry in `AISettingsForm`;
- dedicated Slice 12 verification workflow building WinForms and Example on both supported targets and running the full .NET 9 test suite.

### Verification

**Required Example:** `HAgent.Example → Configuration → Learning Review` on .NET Framework 4.8.1 and .NET 9.

**Required tests:** full `HAgent.Tests` regression suite; WinForms UI behavior is verified through the supported Example host.

Architecture: `docs/architecture/93-learning-review-management-ui.md`.

## Slice 13 — Phase completion verification

Close the phase only after deterministic evidence exists for the real boundaries introduced by 0.9575, including both framework targets, promotion, persistence/recovery, resource version/snapshot isolation, authorization, runtime isolation, context integration, management UI, observability/audit coverage, and no model-output bypass of authoritative resource boundaries.

## Phase exit criterion

Knowledge, Skills, Memory, and Learning are first-class production V1 HAgent resources with explicit identity, scope, ownership, provenance, capability policy, runtime overrides, immutable execution snapshots, governed retrieval, typed learning candidates, durable lifecycle, version-safe authoritative promotion, context/runtime integration, management UI, and auditable policy boundaries.

## Phase 0.9576 — Learned Resource Reliability, Adaptation + Forgetting

## Status

**Planned immediately after 0.9575.**

## Purpose

Provide the post-promotion reliability layer for learned Skills, Knowledge, and other learned resources.

0.9575 governs how experience becomes a validated candidate and how that candidate is promoted. 0.9576 governs whether the promoted resource remains safe, applicable, and useful afterward.

This phase is deliberately practical: it provides lifecycle/reliability controls needed by production V1 without turning HAgent into a research system for large-scale knowledge consolidation or neural continual learning.

## V1 outcome

A promoted resource is never trusted forever merely because it was once approved.

HAgent distinguishes:

```text
Authorization      = may this resource be used?
Applicability      = does it fit this situation?
Reliability        = how much evidence supports continued trust?
Lifecycle          = is it active, stale, quarantined, archived, retired?
```

These dimensions remain separate.

## Delivery slices

### Slice 1 — Applicability and validity

- Define provider-neutral applicability results:
  - `Applicable`
  - `NotApplicable`
  - `Uncertain`
  - `Invalidated`
- Support bounded applicability conditions, preconditions, scope, and evidence references.
- Evaluate deterministic applicability before requesting model reasoning when sufficient evidence exists.
- Keep applicability independent from authorization/capability state.
- Preserve applicability decision/evidence in observability.

### Slice 2 — Reliability evidence and outcome feedback

- Define post-promotion reliability metadata without replacing resource version identity.
- Distinguish promotion evidence from operational outcome evidence.
- Reinforce reliability from validated successful outcomes where policy permits.
- Weaken or quarantine resources after failed outcomes, contradictions, invalid preconditions, or sustained degradation.
- Preserve execution/runtime provenance for reliability evidence.
- Keep reliability changes policy-controlled and revision-safe.

### Slice 3 — Staleness, contradiction, and revalidation

- Distinguish age-based staleness, observed degradation, contextual drift, and direct contradiction.
- Support bounded lifecycle states such as `Active`, `UnderReview`, `Quarantined`, and `Retired` where appropriate.
- Block automatic use of invalidated or contradictory learned behavior.
- Re-evaluate degraded/stale resources through existing Evaluation contracts.
- Produce replacement/revision as new typed candidates rather than mutating published resources in place.
- Preserve historical versions/provenance required to explain changes.

### Slice 4 — Forgetting and archival

- Define policy-governed utility/retention signals.
- Archive or retire stale, superseded, contradicted, or persistently low-utility resources.
- Preserve bounded provenance explaining retirement.
- Never remove a higher-authority resource merely because a lower-utility duplicate exists.
- Keep archival recovery possible where policy requires it.

### Slice 5 — Runtime integration and verification

- Expose reliability/applicability outcomes through the same resource/policy boundaries used elsewhere.
- Ensure execution snapshots capture the resource version/reliability state required for deterministic reproducibility.
- Reject stale asynchronous reliability updates against newer revisions.
- Fall back safely to deterministic behavior, another resource, bounded reasoning, or host escalation when learned behavior is uncertain or invalid.
- Verify supported framework targets and risk-sensitive boundaries.

## V1 safety rules

1. Reliability never grants authorization.
2. An authorized resource can still be inapplicable or invalidated.
3. `Uncertain` never means `Applicable`.
4. Published Skill and Knowledge versions are not silently mutated.
5. Reliability updates are revision-safe and auditable.
6. Learned-resource failure never forces an unsafe fallback.
7. Reliability remains usable without GPU, embeddings, or vector databases.
8. Reliability operates on already-promoted resource versions; it does not bypass the 0.9575 candidate and promotion boundary.
9. Replacement and adaptation produce new governed candidates/resources rather than hidden in-place mutation.

## Ownership boundary

0.9575 owns candidate creation, review, authorization, and authoritative promotion.

0.9576 owns post-promotion applicability, reliability evidence, degradation/quarantine, revalidation, forgetting, and replacement signals.

0.958 owns runtime lifecycle/health; it may consume reliability evidence but does not become the learned-resource evaluator.

0.97 consumes reliable learned behavior during cognition but does not create a second reliability or learning architecture.

## Dependency chain

```text
0.9575 governed learning + promotion
        ↓
0.9576 learned-resource reliability + adaptation
        ↓
0.958 lifecycle + health
        ↓
0.97 deterministic learned behavior + deliberation
```

## Exit criterion

HAgent can determine whether promoted learned behavior is applicable and trustworthy, refuse or quarantine stale/contradictory behavior, incorporate validated outcomes into reliability evidence, produce replacement candidates without mutating authoritative versions, and safely forget/archive learned resources without confusing reliability with authorization or bypassing governed promotion.

## Phase 0.958 — Agent Lifecycle and Health Management

## Status

**Planned after 0.9576 and before durable goal/plan recovery.**

## Purpose

Make the lifecycle and health of a live HAgent runtime explicit and observable without duplicating the runtime-instance identity and execution lifecycle already established by earlier phases.

The phase does **not** create a new runtime-agent class. It extends the existing runtime-instance foundation with the operational state needed by long-running agents and Persistent Cognitive Runtime.

## V1 outcome

HAgent distinguishes:

```text
Lifecycle state = whether the runtime may operate
Health state    = whether the runtime is operating normally
Execution state = what one specific execution is doing
```

These concerns remain separate.

### Lifecycle

The existing runtime foundation remains authoritative for `Active`, `Retired`, and `Shutdown`.

0.958 adds only the operational states needed for persistent operation:

```text
Active
Suspended
Recovering
Retired
Shutdown
```

`Suspended` preserves durable state while new work is prevented or host-controlled work is paused. It is not retirement.

### Health

Health is orthogonal:

```text
Healthy
Degraded
Failed
Unknown
```

Health is evidence, not authorization. Lifecycle and policy decide whether work may continue, wait, recover, or stop.

## Ownership boundary

0.958 owns **runtime-agent lifecycle and runtime health**.

The provider ecosystem phase 0.9592 owns provider/adapter lifecycle and provider/target operational evidence. The capability-aware execution phase 0.96 consumes that provider/target evidence for execution admission.

0.958 must not turn provider failures, rate limits, or target outages into a second provider health/routing authority. Likewise, 0.9592 must not create a second runtime-agent lifecycle model.

Human/host intervention is consumed through the canonical 0.959 intervention boundary; 0.958 owns the target transition itself.

## Delivery slices

### Slice 1 — Lifecycle state extension

- Extend the existing runtime lifecycle only where long-lived operation requires it.
- Define valid transitions and terminal behavior.
- Prevent suspended, retired, recovering, or shutdown runtimes from originating work that policy disallows.
- Preserve existing revision and stale-result protection.

### Slice 2 — Health state

- Define normalized health status and bounded reason metadata.
- Record the source of a health determination: runtime observation, provider failure, recovery failure, host signal, or equivalent evidence.
- Distinguish transient degradation from terminal failure.
- Do not classify slow but valid inference as failed merely because it is long-running.

### Slice 3 — Progress and recovery signals

- Provide bounded progress/heartbeat metadata where a host needs it.
- Detect clearly stalled work only when configured evidence supports that conclusion.
- Support transition into `Recovering` without deleting durable state.
- Make recovery outcome explicit.

### Slice 4 — Observability and verification

- Emit lifecycle and health transitions through existing event/tracing boundaries.
- Expose diagnostics explaining why a runtime is active, suspended, recovering, degraded, failed, retired, or shutdown.
- Verify valid/invalid transitions, suspension/resume, degradation, recovery, stall handling, intervention, and shutdown safety.

## Architectural rules

1. Do not duplicate `AgentRuntimeInstance` identity or execution identity.
2. Lifecycle state is not health state.
3. Health is evidence, not authorization.
4. Recovery never makes obsolete asynchronous work authoritative again.
5. Suspension and recovery preserve durable state.
6. Host lifecycle/scheduling policy remains authoritative where the host controls runtime admission.
7. Provider/model-specific lifecycle semantics do not belong in Core; provider health evidence is normalized by 0.9592 and consumed by 0.96.
8. Runtime intervention uses 0.959; this phase does not create a second approval/intervention mechanism.

## Dependencies

```text
0.9575 governed resources + learning
        ↓
0.9576 learned-resource reliability
        ↓
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
```

## Exit criterion

A long-lived HAgent runtime has explicit lifecycle and runtime-health state, safe suspension/recovery semantics, observable progress/failure reasons, and deterministic protection against work becoming authoritative after retirement, shutdown, recovery invalidation, or newer revisions, while provider health remains owned by the adapter/execution layers.

## Phase 0.9591 — Goal/Plan Persistence and Recovery

## Status

**Planned after 0.958 and before 0.959 intervention.**

## Purpose

Make long-lived agent goals, intentions, plans, checkpoints, and recovery state durable without persisting transient execution machinery.

This phase establishes the durable authority that later intervention and Persistent Cognitive Runtime consume.

## V1 boundary

Persist:

```text
Goal
Intention
Plan
PlanStep
Checkpoint
Recovery record
Revision metadata
Outcome evidence
```

Do not persist as cognitive state:

```text
live Tasks
CancellationToken / synchronization primitives
HTTP clients
provider sessions
active sockets
in-process delegates
live runtime objects
```

## Delivery slices

### Slice 1 — Durable goal/intention contracts

- Define stable IDs, status, priority, constraints, provenance, timestamps, and revision metadata.
- Keep goal identity separate from intention identity.
- Record why an intention was adopted, suspended, revised, completed, failed, abandoned, or superseded.
- Preserve host-supplied goal state without pretending inferred belief is host truth.

### Slice 2 — Durable plans and steps

- Define plan identity/version and ordered or explicitly related steps.
- Capture preconditions, assumptions, expected effects, dependencies, status, and provenance.
- Define step states sufficient for partial progress.
- Record plan revisions without mutating history invisibly.

### Slice 3 — Checkpoints and outcome semantics

- Define explicit checkpoint boundaries.
- Persist durable progress at safe points.
- Distinguish `Completed`, `Failed`, `UnknownOutcome`, `Cancelled`, and `Superseded`.
- Never convert timeout/provider failure into success without evidence.

### Slice 4 — Retry and idempotency

- Define stable operation/step identity for retry correlation.
- Distinguish safe retry from unknown external outcome.
- Record whether an action was requested, observed completed, or remains unknown.
- Keep external side effects host-authoritative; HAgent cannot claim exactly-once execution of arbitrary host actions.

### Slice 5 — Restart and recovery

- Recover the latest durable goal/plan revision after process restart or crash.
- Invalidate in-flight authority belonging to the previous process/runtime execution.
- Reconcile incomplete steps into safe states such as retryable, unknown, blocked, or requiring host review.
- Preserve enough evidence to explain recovery decisions.

### Slice 6 — Persistence backends and verification

- Reuse the existing HAgent storage abstraction.
- Keep File, SQL Server, and MySQL behavior aligned where each backend is supported by the current milestone.
- Verify checkpoint creation, restart recovery, stale revisions, duplicate retries, unknown outcomes, plan supersession, cancellation, and crash-safe recovery.

## Architectural rules

1. Durable state is owned by the runtime agent; storage is only the persistence mechanism.
2. A newer goal/plan revision invalidates stale asynchronous work.
3. Recovery never revives obsolete provider/execution authority.
4. Persistence does not guarantee exactly-once external side effects.
5. Recovery decisions are attributable to evidence and policy.
6. Do not introduce a second plan model for intervention or cognition.
7. Keep the phase host-neutral; host side effects remain outside Core.

## Dependency order

```text
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
        ↓
0.959 intervention
        ↓
0.97 persistent cognition consumes these contracts
```

## Exit criterion

Goals, intentions, plans, steps, checkpoints, and recovery state survive process restart through the supported storage boundary, stale work cannot overwrite newer durable revisions, unknown external outcomes remain explicit, and recovery produces a safe self-consistent state without claiming unsupported exactly-once guarantees.

## Phase 0.959 — Human-in-the-Loop and Intervention

## Status

**Planned after 0.9591; some execution and learning-candidate intervention already exists ahead of roadmap.**

## Purpose

Provide one provider-neutral intervention boundary through which an authorized human or host application can inspect and change agent operation without bypassing execution ownership, policy, authorization, capability, budget, lifecycle, or host-side validation.

Existing ahead-of-roadmap execution and learning-candidate intervention code is evidence to consume, not a second subsystem.

## V1 intervention model

```text
Host / Operator
      ↓
Intervention Request
      ↓
Identity + Policy + Target-State Validation
      ↓
Owning Runtime / Durable Boundary
      ↓
Applied | Rejected | Expired | Superseded
```

Approval is never itself authorization. An approved intervention still has to pass the owning boundary's enforcement rules.

## Supported action families

V1 covers only actions that have a clear owning boundary:

```text
Inspect
Approve
Reject
Pause
Resume
Cancel
Retire
Shutdown
Defer
Redirect where explicitly supported by the target
```

Not every action applies to every target.

## Target boundaries

The same intervention model may target:

```text
Execution
Tool invocation
Learning candidate
Plan step
Goal / intention
Consequential host action
Runtime lifecycle
```

The target owner remains responsible for applying the transition.

## 2026-09-11 use-case audit

The intended desktop-application and HWorld use cases justify the complete **intervention boundary**, but not a requirement that every action family be implemented against every target.

| Intervention surface | Use-case result | V1 decision |
|---|---|---|
| Inspect | Needed for desktop diagnostics and operator visibility into active execution/resource state. | Keep |
| Approve / Reject / Defer | Needed for governed learning candidates and host/operator approval boundaries. | Keep |
| Pause / Resume | Needed for controllable long-running execution and future durable plan/goal control. | Keep, target-specific |
| Cancel | Needed for execution control and host cancellation. | Keep |
| Retire / Shutdown | Needed for explicit runtime lifecycle control. | Keep |
| Redirect | Useful for plan/host actions where the owning boundary explicitly supports redirection, but not a generic provider/model routing mechanism. | Keep as optional target capability |
| Execution target | Directly required by current execution-intervention behavior and host control. | Keep |
| Tool invocation | Required when host tools have meaningful side effects or can run for significant time. | Keep |
| Learning candidate | Directly required by the governed learning lifecycle. | Keep |
| Plan step | Not an immediate HWorld requirement, but required for later durable plan intervention. | Keep for 0.959 V1 |
| Goal / intention | Not an immediate HWorld requirement, but required for later cognitive control and recovery. | Keep for 0.959 V1 |
| Consequential host action | Needed because HAgent must participate in approval/control without owning host or world side effects. | Keep |
| Runtime lifecycle | Directly relevant to desktop/HWorld runtime ownership. | Keep |

The audit therefore **does not remove an intervention target family**, but establishes that action applicability is target-owned and incremental. In particular, `Redirect` must not become a second provider/model selection path; provider/execution-target selection remains owned by Phase 0.96.

The audit also confirms that HWorld does not require 0.10 workspace/chat features before using intervention. HWorld remains authoritative over world state and side effects, while HAgent supplies the generic control boundary.

## Delivery slices

### Slice 1 — Canonical intervention contract

- Normalize request identity, target identity, action, reason, requester identity, correlation, expected target revision/state, and expiry.
- Keep intervention state separate from target state.
- Define deterministic outcomes such as applied, denied, expired, superseded, invalid, and failed.

### Slice 2 — Execution and learning boundaries

- Consume existing execution intervention behavior.
- Consume existing learning-candidate intervention behavior.
- Verify stale intervention requests cannot act on newer target revisions.
- Verify concurrent intervention requests serialize at the owning target.

### Slice 3 — Durable goal/plan intervention

- Add intervention at plan-step and goal/intention boundaries after 0.9591 durable contracts exist.
- Preserve durable revision semantics.
- Pause/resume/reject/redirect only through the owning durable state boundary.
- Do not mutate a plan by editing an unrelated intervention record.

### Slice 4 — Consequential-action boundary

- Allow host-defined consequential actions to request intervention using a generic HAgent contract.
- Keep the host authoritative over actual side effects.
- HAgent records requested/approved state but never fabricates completion evidence.

### Slice 5 — Persistence, UI, diagnostics

- Persist intervention records where the owning target is durable.
- Add management UI and diagnostics using existing WinForms conventions.
- Preserve requester/approver identity, reason, correlation, target revision, and outcome.
- Verify restart behavior and stale intervention expiry.

## Architectural rules

1. There is one intervention model, not separate approval engines.
2. Intervention never grants authority that policy did not already grant.
3. Target ownership remains authoritative.
4. Intervention requests become stale when their target revision/state changes.
5. A rejected or expired intervention never mutates the target.
6. Host side effects remain host-authoritative.
7. Intervention state does not replace lifecycle, plan, execution, or learning state.
8. Redirect is never a provider/model routing shortcut; execution-target selection remains owned by Phase 0.96.

## Dependency order

```text
0.958 lifecycle + health
        ↓
0.9591 durable goals/plans/recovery
        ↓
0.959 intervention
        ↓
0.96 execution planning
```

## Existing ahead-of-roadmap evidence

Execution intervention and learning-candidate intervention already exist and have deterministic Examples/tests. They remain in source while the ordered roadmap catches up. They do not mark the entire 0.959 phase complete.

## Exit criterion

Authorized operators/hosts can safely inspect and intervene at supported execution, learning, plan, goal, lifecycle, and consequential-action boundaries through one policy-governed contract, with durable target revisions, stale-request protection, persistence where appropriate, diagnostics, and no bypass of host authority.

## Phase 0.9592 — Provider Ecosystem and Adapter Lifecycle

## Status

**Planned after 0.959 Human Intervention and before 0.96 capability-aware execution.**

The ordered roadmap position is:

```text
0.9591 Goal / Plan Persistence + Recovery
        ↓
0.959 Human Intervention
        ↓
0.9592 Provider Ecosystem + Adapter Lifecycle
        ↓
0.96.x Configuration / Storage / Portability
        ↓
0.96 Capability-Aware Execution
```

This ordering is a delivery dependency for the V1 roadmap. 0.9592 does not need goal/plan persistence or intervention APIs as implementation inputs merely because it follows them in the ordered sequence; it is placed here so the provider boundary is complete before 0.96 consumes it.

## Purpose

Provide a clean provider-adapter boundary so HAgent can use multiple providers, API variants, models, and discovery sources without leaking provider-specific behavior into `HAgent.Core` or making provider integration larger than necessary.

This phase is a provider-platform foundation, not a provider marketplace.

## V1 provider model

```text
Provider Configuration
        ↓
Provider Adapter
 ├── execution transport
 ├── model / target discovery (when available)
 ├── capability evidence (when available)
 ├── quota / rate / usage telemetry (when available)
 ├── health / availability evidence
 └── provider-native metadata
        ↓
Normalized HAgent contracts
        ↓
0.96 Execution Planner
```

Unknown information remains `Unknown`; adapters do not invent capabilities, limits, costs, quotas, health, or compatibility claims.

## 2026-09-11 use-case audit

The intended desktop-application and HWorld use cases justify the core of 0.9592. HWorld explicitly intends to support different providers/models in one world, while HAgent's host architecture requires provider/model discovery and normalized execution-target information before 0.96 can make capability-aware selections. The current provider-limit/rate-capacity problems also make operational evidence a concrete requirement rather than a hypothetical platform feature.

| 0.9592 surface | Use-case result | 0.9592 decision |
|---|---|---|
| Multiple provider adapters | Required when different agents/actors use different providers/models concurrently. | Keep |
| Adapter registration/use/disable/retirement | Required for desktop configuration and for hosts that need to remove an unavailable provider without deleting historical identity. | Keep |
| Model/target discovery | Required to avoid hard-coding every provider's model catalog and to feed 0.96 target assessment. Partial/no discovery must remain supported. | Keep |
| Capability discovery/evidence | Required for 0.96 capability-aware selection; unknown capability must remain unknown. | Keep |
| Provider-native identities/metadata | Required because the same logical model may exist through multiple providers/accounts/deployments. | Keep |
| Quota/rate/usage telemetry | Directly justified by provider limits, shared capacity, and 429/rate-limit behavior encountered in HWorld-oriented execution. | Keep |
| Health/availability evidence | Needed to avoid repeatedly selecting unavailable execution targets. | Keep |
| Adapter/API compatibility and replacement | Needed for deliberate adapter replacement while preserving historical provider/target identity and configuration semantics. | Keep, bounded |
| Cost information | Needed by 0.96's FreeOnly/FreePreferred/NoRestriction policy; 0.9592 only supplies evidence, it does not select. | Keep as evidence |
| Provider marketplace/ecosystem catalog | Not required by the intended use cases and not part of the ordered 0.9592 responsibility. | Exclude from this phase |
| Distributed provider-control/rate-limit service | Not required; HAgent should normalize local/provider-reported evidence and leave host/distributed infrastructure outside Core. | Exclude from this phase |
| Autonomous provider-routing logic | Not required here; target selection belongs exclusively to 0.96. | Exclude from this phase |
| Vendor-specific compatibility matrix as a second rules engine | Not required; adapter-specific behavior stays behind adapter contracts and normalized evidence. | Exclude from this phase |

The audit therefore **keeps 0.9592 substantially intact but confirms its boundary**: the phase supplies adapters, discovery, provider-native evidence, and operational observations. It does not become a marketplace, billing system, distributed provider-control plane, or second routing engine.

## Delivery slices

### Slice 1 — Adapter contract and lifecycle

- Define provider adapter identity/version metadata.
- Support registration/creation, validation, use, refresh, disablement, replacement, and retirement.
- Keep adapter instances concurrency-safe or explicitly scoped.
- Keep transport/authentication/retry/provider-specific error handling inside adapters where appropriate.

### Slice 2 — Discovery and normalized metadata

- Support providers with complete, partial, or absent discovery APIs.
- Normalize models/execution targets without forcing logical-model correlation when it cannot be established reliably.
- Preserve provider-native model IDs, deployments, endpoints, accounts/projects, API versions, and provenance.
- Normalize capability evidence, constraints, cost information, and refresh timestamps.

### Slice 3 — Operational telemetry

- Normalize provider-reported rate/quota/usage information where available.
- Normalize health and availability evidence.
- Support incomplete telemetry without manufacturing defaults.
- Keep observed operational state separate from technical capability.

### Slice 4 — Adapter compatibility and change handling

- Record adapter/provider API compatibility metadata.
- Support deliberate adapter replacement without corrupting persisted agent configuration or historical execution records.
- Mark retired/deprecated targets unavailable without deleting historical identity.

### Slice 5 — Verification

Use deterministic fake providers to verify:

- complete discovery;
- partial discovery;
- unknown metadata;
- unsupported operations;
- API/adapter version changes;
- replacement/retirement;
- concurrent adapter use;
- quota/rate/health telemetry.

## Architectural rules

1. Core remains provider-neutral.
2. A provider describes transport/service integration, not agent behavior.
3. Model names are not sufficient execution identity; concrete targets remain distinct.
4. Unknown capability/cost/quota/health information remains unknown.
5. Adapter lifecycle does not become runtime-agent lifecycle.
6. Provider-specific behavior stays behind adapter boundaries.
7. 0.96 owns execution-target selection; this phase does not create a routing engine.
8. Provider credentials use the repository's simple encrypted provider-configuration mechanism; no separate secret-vault architecture is introduced.
9. Provider marketplace, distributed provider-control, and autonomous provider-routing responsibilities remain outside this phase.
10. 0.9592 publishes normalized provider evidence; it does not silently reinterpret that evidence into execution policy.
11. Configuration/storage portability remains a cross-cutting concern consumed through existing/future 0.96.x contracts; provider adapters must not create a parallel configuration or persistence subsystem.

## Dependencies and handoff

```text
Existing provider-neutral runtime/execution contracts
        ↓
0.9592 provider adapters / discovery / operational evidence
        ↓
0.96.x configuration, storage, and portability foundations
        ↓
0.96 capability-aware execution
```

The phase is intentionally ordered after 0.9591 and 0.959 in the roadmap, but its direct technical purpose is to complete the provider boundary before 0.96 consumes it. 0.96 must depend on normalized 0.9592 contracts rather than provider-specific APIs.

## Exit criterion

HAgent can register and use multiple provider adapters, preserve provider-native identities and metadata, consume complete or partial discovery/operational information through normalized contracts, represent unknowns honestly, preserve adapter lifecycle/history, and hand all execution-target selection to Phase 0.96 without creating a parallel routing or configuration authority.

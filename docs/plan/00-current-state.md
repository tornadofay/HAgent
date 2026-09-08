# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.953 Unified Policy Engine — CURRENT; 0.954–0.9592 remain foundational hardening ahead of 0.96 Capability-Aware Execution.**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage foundations are substantially implemented and manually verified across supported storage backends; remaining internal repository parity is intentionally deferred.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.952 First-Class Event Subsystem is completed and verified. 0.953 Unified Policy Engine is the current implementation milestone. Its core evaluator, runtime enforcement, and effective-policy execution-snapshot architecture are implemented; the new snapshot behavior still requires the matching local Example verification before being considered verified. The remaining foundational sequence is 0.954 Prompt/Instruction Governance; 0.955 Context Engineering; 0.956 Observability/Distributed Tracing; 0.957 Evaluation/Quality Measurement; 0.958 Agent Lifecycle/Health; 0.959 Human-in-the-Loop/Intervention; 0.9591 Goal/Plan Persistence/Recovery; and 0.9592 Provider Ecosystem/Adapter Lifecycle.

The configuration/storage evolution defined by `docs/roadmap/38-configuration-storage-and-portability.md` is ordered as a cross-cutting foundation before 0.96 because capability-aware execution and persistent cognition both depend on its provider/model/target separation, credential persistence, global configuration, resource relationships, shared-database behavior, snapshot invalidation, and portability contracts.

0.96 Capability-Aware Execution follows these foundations and addresses heterogeneous capabilities, the same logical model exposed by multiple providers, provider/account/project restrictions, model/task-specific constraints, quotas, rate limits and future quota dimensions, concurrency capacity, operational availability, long-running inference, capability-aware candidate selection, fallback/degradation, and proactive admission control.

0.97 Persistent Cognitive Runtime builds the long-lived cognitive layer above the execution engine after the generic event, identity, policy, context, lifecycle, recovery, provider, and configuration foundations are established. It now includes an extensible Cognitive Kernel plus pluggable Cognitive Strategies, with Adaptive Hybrid Cognition (AHC) as the first reference strategy, explicit belief state/revision, attention/global workspace, goals/intentions/plans/operators/impasses, reasoning requirements, adaptive model escalation including the ability to use no LLM, experience-driven proceduralization, and a live Cognition Workbench.

0.10 Workspaces, Routing + Chat has a verified routing and role-policy foundation and remains intentionally paused until the generic runtime/capability/cognitive foundations are mature.

The later Knowledge + Skills + Memory Governance + Learning layer remains planned as a platform feature layer and must consume the new policy, identity, event, context, lifecycle, evaluation, and persistent-runtime contracts rather than create parallel project-specific systems. The 0.97 cognitive runtime may consume existing resource primitives before all 0.11 governance work is complete.

## Verified implementation

The repository currently contains verified foundations for:

- provider/agent configuration and routing;
- execution lifecycle, timeout, cancellation, retries, diagnostics, and failure reporting;
- memory, persistent sessions, context budgeting, automatic/episodic/task memory;
- capability discovery and response normalization;
- streaming contracts and live streaming;
- tool definitions, registry, schema validation, provider transport, bounded tool loops, persistence, and per-agent assignment;
- WinForms UI Context with Form/UserControl attachment;
- semantic and bound/native data-source discovery;
- CurrencyManager/current-item/source relationships;
- control-to-source relationship discovery;
- convention-based custom control adaptation;
- bounded application-object discovery;
- provider-neutral structured data projection/query contracts;
- HAgent-owned storage configuration for File, SQL Server, and MySQL backends;
- application-specific File storage layout;
- HAgent-owned SQL Server/MySQL database bootstrap foundations;
- bounded internal inventory, memory, conversation, and execution-audit read tools;
- automatic payload-free execution auditing with configurable bounded retention;
- runtime-instance identity, scope, runtime-only overrides, independent memory ownership, concurrent execution, stale-result protection, host-controlled scheduling, shutdown semantics, and optional runtime-state persistence;
- provider-neutral workspace participants, message metadata, default-recipient routing, and coordinator/specialist role policy;
- generic host execution requests with multiple messages, host correlation identity, bounded host context, provider-facing request isolation, native structured-output transport/fallback, terminal-state protection, runtime snapshot isolation, verified external-consumer coverage on both supported target frameworks, and verified runtime-instance + canonical-request composition;
- unified policy contracts, deterministic scoped evaluation, cost guards, policy provenance, pre-transport runtime enforcement, and implementation of deep-cloned effective policy state in execution snapshots (new snapshot verification pending local execution).

## Foundational architecture hardening before 0.96

The next architectural work establishes common infrastructure required by both capability-aware execution and persistent cognition:

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Event subsystem
        ↓
0.953 Unified Policy Engine
        ↓
0.954 Prompt / Instruction Governance
        ↓
0.955 Context Engineering
        ↓
0.956 Observability / Tracing
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.958 Agent Lifecycle / Health
        ↓
0.959 Human-in-the-Loop / Intervention
        ↓
0.9591 Goal / Plan Persistence / Recovery
        ↓
0.9592 Provider Ecosystem / Adapter Lifecycle
        ↓
0.96.x Configuration / Storage / Portability Evolution
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
```

These phases are architectural foundations, not commitments that every feature must be fully completed before any implementation can begin. A later phase may consume a stable contract from an earlier phase while implementation continues iteratively.

### Storage implications

The foundational phases consume the storage evolution defined in `docs/roadmap/38-configuration-storage-and-portability.md`. The persistence model must directly support the new configuration architecture rather than preserve retired Agent provider/model fields.

Provider API keys are persisted with provider configuration and encrypted at rest. There is no separate provider secret-reference or vault architecture. Shared SQL Server/MySQL configuration can therefore be used by multiple authorized HAgent processes/machines. Configuration export/import is planned as a versioned portable representation with optional encrypted credential inclusion.

### Deferred exclusions

The roadmap does not introduce a separate distributed-coordination subsystem and does not introduce a sophisticated external secret-management architecture. Distributed behavior is handled through the existing storage/runtime contracts where required, while provider credentials use the project's intentionally simple fixed encryption/decryption mechanism.

## Paused Workspace target

Phase 0.10 initially provides one default persisted workspace per host user. The host supplies a stable `UserId`, display identity, and `IsAdmin` identity. Database-backed persistence is partitioned by host application identity and user identity; File storage remains local to the host installation.

Workspace visibility is always explicit: the workspace is hidden until the host opens it. `Create`, `Open/Show`, `Hide`, and `Close` are separate lifecycle operations, and closing the UI never destroys persisted workspace state. The model remains extensible to multiple named workspaces later.

The workspace product target includes a shared Lobby, distinct user-to-agent Private Chats, agent join/leave, coordinator/specialist defaults, permitted provider/agent/model selection and runtime overrides, integrated approval requests/resolution, safe activity/statistics, unread/last-seen state, bounded presentation of tables/charts/graphs and popup/detail results, and modern WinForms presentation through a public host-facing workspace facade.

Provider secrets, connection strings, live provider tasks, live `CancellationToken` state, runtime synchronization primitives, raw HTTP requests, raw provider payloads, and temporary execution objects remain outside persisted workspace state; these exclusions were established by 0.95.

## Phase 0.96 capability-aware execution target

Reusable Agent profiles remain first-class. They describe what the agent is and what capabilities it requires or prefers rather than permanently binding the agent to one provider/model transport target.

```text
Agent Profile
    identity / instructions / tools / memory / knowledge / skills
    required capabilities
    preferred capabilities
    preferred logical model (optional)
    preferred provider (optional)
    fallback / degradation policy

Provider
    service integration

Provider account / project / endpoint
    operational execution environment

Model / logical model
    provider-independent identity when reliably known

Model deployment / execution target
    concrete provider + account/project/endpoint + model deployment

Execution Planner
    selects the best currently compatible target
```

The same logical model may be exposed by Groq, OpenRouter, a direct vendor endpoint, or a local OpenAI-compatible server. These are separate execution targets because capability, provider policy, limits, quota, health, latency, routing behavior, and permissions may differ.

Capability and operational state remain separate. Effective capability state is `Supported`, `Unsupported`, or `Unknown`. Separate records represent request/model constraints, account/project permission, quota/rate capacity, concurrency capacity, current availability/health, and expected latency.

The capability model supports explicit input/output modalities rather than a single vision flag, including text, image, audio, video, embeddings, generation, understanding, and future task types. Native and emulated/degraded behavior remain distinguishable, especially for structured output.

Execution requests express capability requirements independently from agent identity, using required/preferred/optional/forbidden semantics. A manually selected provider/model target passes the same compatibility and policy checks as an automatically selected candidate.

The Execution Planner evaluates candidate targets against requirements, preferences, policy, current availability, limits, and operational capacity before transport. Provider adapters may supply provider-specific discovery and telemetry, but HAgent.Core must consume normalized contracts and must not hard-code Groq, Cloudflare, NVIDIA, OpenRouter, or other provider model matrices.

### Admission and quotas

HAgent provides proactive admission control rather than relying on retries after ordinary limit failures. Rate/quota dimensions are generic and extensible. Minimum dimensions include request count, input tokens, output tokens, total tokens, and concurrent requests, with future dimensions such as audio duration, image count, bytes, spend, or provider-specific units.

Windows may be per-minute, per-day, or provider-specific. Limits may apply at organization, account, project, endpoint, model/deployment, or another provider-defined scope.

Concurrent executions require atomic reservation before provider transport and reconciliation with actual observed usage after execution. Provider headers, retry metadata, 429/throttling responses, and other telemetry are feedback that updates operational state rather than the sole capability-discovery mechanism.

Capacity decisions may be `Wait`, `TryNextCandidate`, `Fail`, or an explicitly policy-permitted degraded path. Admission waiting has a bounded maximum wait.

### Long-running providers

A provider can have abundant or effectively unlimited daily quota while still having low concurrency capacity and multi-minute inference latency. HAgent must not equate quota availability with execution capacity. Long-running requests remain asynchronous, respect cancellation and timeout semantics, and do not block unrelated runtime executions. Slow targets may be valid candidates when the request's latency policy permits them.

## Management UI target for 0.96

Provider/model administration should eventually show execution-target identity, capabilities, constraints, quota/rate state, availability, latency observations, and compatibility with the current request. Workspace provider/model selection must consume the same planner rather than bypass it.

## Boundaries

0.96 is provider-neutral runtime hardening. It does not add provider-specific model matrices to Core, replace host scheduling policy, or become the source of billing truth. It supplies generic discovery, compatibility, admission, and planning primitives that provider adapters and hosts can use.

## Active implementation

The active implementation plan remains `docs/plan/20-active.md`. The architectural foundation phases 0.951–0.9592 now precede Phase 0.96 in the roadmap. Configuration/storage evolution is ordered as a cross-cutting pre-0.96 dependency even though its historical source filename is `38-configuration-storage-and-portability.md`. Phase 0.10 remains paused while the generic foundations are hardened.

## Verification rule

A capability becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the project documentation reflects the result.

Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — non-negotiable engineering and repository rules.
- `docs/architecture/` — stable architectural design and boundaries.
- `docs/plan/` — master direction, current state, and active implementation only.
- `docs/roadmap/` — ordered path from completed foundations and future phases.
- `docs/storage.md` — storage-specific details.

The root `plan.md` and `roadmap.md` are generated from their source directories. They are views, not independent sources of truth.

# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.953 Unified Policy Engine
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete policy contracts, deterministic evaluation, cost guarding, runtime enforcement, persistence/effective snapshots, authorization integration, tool/resource policy, learning-promotion policy, approvals, UI, and verification defined by the active implementation plan.

## Current checkpoint

Policy persistence, runtime provider enforcement, tool policy enforcement, policy-first host data authorization, the profile/runtime resource capability boundary, learning-promotion policy/candidate transitions, and the bounded approval/defer workflow were verified locally by the user on 2026-09-08. The implementation now also contains a policy management WinForms surface for rule editing, effective-decision inspection, and agent resource-capability inspection. The UI has not yet been locally verified.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the active scope is explicitly changed.

## Current blockers

None recorded. All runtime policy slices completed so far are verified. The remaining 0.953 checkpoint is local verification of the new policy management UI on the supported WinForms targets, followed by any backend-specific verification appropriate to the configured environment.

## Next checkpoint

Build and run the HAgent WinForms configuration application. Verify the **Policy** configuration surface opens, loads the persisted policy, adds/edits/deletes rules without invalid states, evaluates a request with correct outcome/provenance, and displays effective agent resource-capability state. Repeat on both `net481` and `net9.0-windows` where available.

## Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.959 Human-in-the-Loop / Intervention — CURRENT; 0.9591–0.9592 remain foundational hardening ahead of 0.96 Capability-Aware Execution.**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage foundations are substantially implemented and manually verified across supported storage backends; remaining internal repository parity is intentionally deferred.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.952 First-Class Event Subsystem is completed and verified. 0.953 Unified Policy Engine is completed for its verified runtime/persistence/resource/learning-policy foundation. 0.959 Human-in-the-Loop / Intervention is the current implementation milestone. Its canonical provider-neutral intervention request/lifecycle contract, bounded in-memory workflow, tool approval/defer integration, and deterministic approval/defer verification are present. Runtime application, concurrency hardening, broader targets, persistence, management UI, expanded Example coverage, and final framework/backend verification remain in the run-sized active plan.

The remaining foundational sequence is 0.9591 Goal/Plan Persistence/Recovery and 0.9592 Provider Ecosystem/Adapter Lifecycle, followed by 0.96 Capability-Aware Execution. The configuration/storage evolution defined by `docs/roadmap/38-configuration-storage-and-portability.md` remains a cross-cutting foundation before 0.96 because capability-aware execution and persistent cognition both depend on its provider/model/target separation, credential persistence, global configuration, resource relationships, shared-database behavior, snapshot invalidation, and portability contracts.

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
- unified policy contracts, deterministic scoped evaluation, cost guards, policy provenance, pre-transport runtime enforcement, deep-cloned effective policy state in execution snapshots, and canonical File/SQL Server/MySQL policy persistence;
- policy-gated tool invocation before executable handlers, including `Deny`, `RequireApproval`, and `Defer` enforcement and policy provenance in `ToolExecutionResult`;
- policy-first composition with host `IDataAccessAuthorizer` for structured data operations, preserving host authorization as the final authority;
- canonical resource capability profiles with runtime `Inherit` / `Enabled` / `Disabled` overrides, deterministic effective-state snapshots, resource persistence, and tool gating before executable handlers;
- typed learning-promotion requests evaluated through the unified policy engine, plus explicit learning-candidate `Proposed` / `PendingReview` / `Approved` / `Rejected` / `Promoted` transition rules;
- canonical provider-neutral human-intervention requests, bounded approval/defer workflow, and deterministic Example verification for the current intervention foundation.

## Foundational architecture hardening before 0.96

The foundational sequence is now:

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

The unified policy set is now a canonical HAgent-owned configuration record exposed through `IAiStore`. File storage persists it with the main settings document; SQL Server and MySQL persist it in `HAgentPolicies`, and their HAgent database bootstrap paths create that table. The default runtime loads the current persisted policy asynchronously at execution creation when no explicitly injected policy engine is supplied, then captures the effective policy in the execution snapshot.

Agent profile resource capability defaults are now part of the canonical `AiAgent` configuration and persist through the normal agent storage path. Runtime capability overrides remain transient and resolve above profile defaults into `AgentExecutionSnapshot.EffectiveResourceCapabilities`.

Learning-promotion decisions are now represented as normal `AiPolicyDecision` outcomes on `learning.promote`, with typed candidate metadata carried as bounded policy attributes. This keeps learning governed by the single policy engine rather than adding a parallel learning authorization mechanism. Candidate persistence/target repositories and human intervention remain separate phases.

Human intervention is now a canonical provider-neutral workflow boundary built on the existing approval/defer policy boundary. Runtime execution control, persistence, and management UI are being added through the ordered run-sized plan in `docs/plan/20-active.md` rather than through parallel approval or execution subsystems.

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

The active implementation plan is `docs/plan/20-active.md`. It is currently organized into seven run-sized 0.959 intervention slices; only one slice is current at a time and each must reach a verified checkpoint before the next begins. The first slice is the execution control boundary.

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

## Architectural Decisions

This file contains only durable decisions needed to preserve project direction across sessions. It is not a conversation log.

## D-001 — Complete known architecture before implementation

**Status:** Active

Substantial features must be implemented against the complete intended architecture that can reasonably be derived from the repository, roadmap, architecture documents, and requirements. Do not deliberately create simplified temporary implementations when the required design is already understood.

Testing is primarily for verification, defect discovery, and genuinely unforeseen interactions. It must not be used as a substitute for architectural analysis that should have happened before implementation.

## D-002 — Repository documents are persistent project memory

**Status:** Active

Small purpose-specific Markdown documents preserve durable project state so development can resume without relying on conversation history. They contain compressed conclusions, current state, active work, invariants, and durable decisions—not transcripts or unrestricted reasoning.

## D-003 — No duplicate sources of truth

**Status:** Active

Each durable fact should have one authoritative source. Other documents should reference that source rather than copying the same architectural decision. Generated root documents remain generated views.

## D-004 — Active work is state, not history

**Status:** Active

Current unfinished work is represented by the compact active-work document. When work advances, update the current state instead of appending a chronological diary. Completed work is removed from active state once it is reflected in the appropriate authoritative project document.

## D-005 — Unforeseen discoveries may refine architecture

**Status:** Active

Testing and implementation may reveal requirements or interactions that could not reasonably be known beforehand. Such discoveries should produce an explicit architectural decision or update to the authoritative source rather than an undocumented workaround.

## HAgent Master Plan

## Purpose

HAgent is a general-purpose, provider-neutral cognition and execution library that makes connecting software to LLMs practical. Its goal is to provide reusable infrastructure for any software project that needs LLM-driven behavior without importing host-specific domain models into HAgent.Core.

A host may be a conversational program, business software, service, game, simulation, automation system, developer tool, or another environment.

## End-state goal

A host should be able to add HAgent and choose how much intelligence it wants to expose. HAgent should provide generic infrastructure for model invocation, context, tools, memory, reusable skills, knowledge/Wiki, controlled learning, structured output, multi-agent coordination, and asynchronous execution.

The host remains authoritative over real domain state, lifecycle, scheduling, host persistence, authorization, and side effects.

## Core model

```text
Provider profile
    -> connection/account/endpoint configuration

Model catalog
    -> discovered logical models and concrete execution targets
    -> capabilities / constraints / availability / cost metadata

Agent profile
    -> reusable behavior + capability policy defaults
    -> AI requirements/preferences/selection mode

Runtime agent instance
    -> one live agent identity created from a profile
    -> runtime-only capability/memory overrides

Execution request
    -> host input/context + correlation + execution requirements

Skills
    -> reusable executable capabilities/procedures

Knowledge
    -> reusable retrievable information
    -> Wiki is a managed persistent knowledge source

Memory
    -> scoped experience/state
    -> working / episodic / semantic / procedural / future families

Learning
    -> execution experience -> typed candidates -> policy -> promotion

Execution
    -> bounded asynchronous model/tool work with lifecycle and correlation
```

Provider and model configuration is discovery-first. Users normally configure credentials, endpoints, and provider-specific connection information; HAgent discovers model catalogs and technical metadata when the provider exposes them. Manual metadata overrides are exception paths for unknown/unavailable information, not mandatory setup fields.

Commercial state is treated separately from technical capability. A concrete execution target may be `Free`, `FreeWithinQuota`, `Paid`, or `Unknown` depending on provider/account/plan. The same logical model can therefore be free through one provider and paid or unknown through another.

The distinction between persistent profiles and runtime instances remains fundamental. One profile can produce many independent runtime instances. Shared resources are referenced; private runtime state is not copied across instances.

## Cost policy and AI selection

HAgent provides system-wide policy defaults and more specific overrides.

```text
Global General settings
    -> Agent profile
        -> Runtime/host override
            -> Execution Planner
```

The global **Cost Policy** is at least:

```text
FreeOnly
FreePreferred
NoRestriction
```

`FreeOnly` does not simply filter a model list. The selected target must still satisfy capabilities, permissions, constraints, quota/capacity, health, and other policy. Unknown cost is never treated as free implicitly.

Agents have an AI selection mode:

```text
Auto
Preferred
Fixed
```

`Auto` lets HAgent select a compatible target according to policy, capabilities, cost, availability, capacity, latency, and preferences. `Preferred` expresses a strong model/provider preference while allowing explicit fallback according to policy. `Fixed` lets an administrator deliberately select a concrete target, such as always using a particular high-end model, while still enforcing capability, permission, quota, capacity, health, and fallback rules.

Preference policies such as highest quality, lowest latency, lowest cost, or balanced are distinct from Fixed target selection and remain planner-owned.

## Knowledge, Skills, Memory, and Learning

HAgent must keep the following distinctions explicit:

```text
Skill     = reusable executable capability/procedure
Knowledge = reusable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = controlled transformation of experience into candidates
```

Knowledge and Skills are reusable resources with explicit scope and authorization rather than private copies owned by every agent. Memory has explicit ownership/scope and may be private to a runtime instance, shared at logical-agent/user/tenant level, or execution-local.

Learning is not model-weight training. It may use deterministic code, LLM reasoning, or both. Code controls candidate typing, provenance, policy, authorization, retention, and promotion.

### Learning modes

```text
Disabled
SuggestOnly
AutomaticWithPolicy
FullyAutomatic
```

`SuggestOnly` is the recommended governance mode. `AutomaticWithPolicy` permits promotion only under explicit policy. `FullyAutomatic` is an explicit advanced opt-in and never follows merely from enabling learning.

Learning candidates are typed (`MemoryCandidate`, `KnowledgeCandidate`, `SkillCandidate`) and preserve source execution/runtime identity, proposed scope, provenance, and evidence/confidence where available.

### Promotion rules

LLM output must never write authoritative Wiki/knowledge or mutate a published Skill directly merely because it was generated. Normal promotion is:

```text
experience
  -> candidate
  -> validation / policy / authorization
  -> memory, managed knowledge, or new skill version
```

Skill improvements produce new versions; already-running executions use their immutable skill/configuration snapshots.

## Capability policy

Agent profiles establish reusable capability defaults. Runtime instances inherit them and can override them without mutating the profile.

The effective state for each capability/resource is tri-state:

```text
Inherit
Enabled
Disabled
```

The policy must support at least:

- skills and individual skill resources;
- Wiki/knowledge and individual knowledge resources;
- memory and individual memory families/types;
- future resource types by stable type/resource identifiers.

Capability enforcement occurs before retrieval or invocation. Prompt instructions are not authorization.

## Runtime and memory target

A host may keep a runtime instance alive and execute against it repeatedly for an arbitrary lifetime. Private runtime memory must remain independent across runtime instances created from the same profile.

Working memory is execution-local. Long-term memory ownership is explicit and may be runtime-, agent-, user-, tenant-, or another host-approved scope. The physical store may be shared when its contract is concurrency-safe.

Effective profile/runtime capability policy and memory access are captured in execution snapshots so configuration changes cannot alter already-running work.

## Generic execution request

The canonical execution boundary accepts generic host input/context, host correlation identity, execution options, and optional structured-output requirements. Plain strings remain convenience APIs.

## System-prompt model

System prompts are additive layers. Lower layers can add narrower instructions/restrictions but cannot erase higher layers. Prompt layering is behavioral composition, not a security boundary.

## Context target

Context remains bounded, generic, and host-supplied. HAgent may normalize/project/serialize host context but does not assign domain meaning.

## Structured output target

A host may define its own schema. HAgent carries it through provider invocation, validates the returned structure, and exposes validation metadata. Valid JSON alone does not prove schema compliance.

## Tool target

Tool definitions describe what may be requested; trusted runtime handlers define what executes. Handler delegates are never serialized. Tool execution preserves execution/runtime/host correlation for authorization and telemetry.

## Configuration and management UI target

`HAgent.WinForms` is organized around user responsibilities rather than execution-planner terminology.

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

System-wide defaults and policies live here, including at minimum:

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

These are defaults, not forced values. More specific configuration may inherit or override them where policy permits.

### Providers

Provider setup is intentionally lightweight. A normal provider form asks for only what is necessary to connect:

```text
Provider name/type
Base URL where applicable
Credentials/secrets
Account/project information where applicable

[ Test Connection ]
[ Save ]
```

HAgent should discover model catalogs, capabilities, modalities, constraints, operational limits, availability, and cost metadata when possible. Providers that expose only partial information remain valid; unknown values are shown as unknown rather than forcing a large manual form.

### Models

Models is a first-class configuration tab between Providers and Agents. It displays the discovered HAgent model catalog.

It should surface:

```text
Logical model / display name
Provider / execution target
Availability
Cost: Free / FreeWithinQuota / Paid / Unknown
Capabilities
Constraints/limits
Quota/rate/capacity state where available
Last verified / evidence source
```

The UI may group matching logical models across providers while keeping concrete execution targets separate. It should make it obvious that the same model can have different capabilities, cost, quota, or availability depending on provider/deployment.

### Agents

Agent Configuration should answer practical questions: what is this agent, which AI should it use, what can it access, what does it know, what does it remember, and how does it learn?

The Agent Editor should include:

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

The AI section includes `Auto`, `Preferred`, and `Fixed` selection modes plus requirements/preferences and explicit fallback behavior. The selected agent overview shows effective AI selection, Skills, Knowledge, Memory, Learning, Tools, and Cost Policy with inherited/overridden/effective values where applicable.

The Knowledge section shows accessible Wiki/knowledge resources and relationships. The Skills section shows assigned/inherited/disabled skills and usage relationships. The Memory section shows enabled memory families/types and effective scope. Learning exposes Learning Mode and links to Learning Review. Future/unknown resource types remain visible through the generic resource inventory.

The configuration model is reference-based: resources remain in their own stores and are not copied into the agent profile merely to appear in the overview.

## Learning and management UI target

`HAgent.WinForms` must provide:

```text
Learning Review
    pending candidates
    inspect provenance/evidence/source
    approve / reject

Wiki / Knowledge Manager
    new / edit / delete
    search/filter
    relationships
    which agents use/access it

Skill Manager
    new / edit / delete
    version/status
    relationships
    which agents use it

Agent Configuration
    selected agent -> effective AI selection + cost policy
                     -> effective skills
                     -> knowledge/wiki access
                     -> memory families
                     -> learning mode/policy
                     -> any future resource types
    profile enable/disable
    runtime-instance overrides
```

The agent knowledge overview is based on a generic resource inventory. Known types may have specialized panels, but unknown/new types remain visible without adding a new hard-coded Agent property.

## Generic external-host requirement

HAgent must be capable of serving as the generic LLM cognition/execution layer for different project types. Host state, lifecycle, scheduling, persistence, authorization, and side effects remain host-owned.

## Security target

No model instruction is an authorization boundary. Retrieval, memory recall, skill use, learning, and learning promotion are independently enforceable policy boundaries where meaningful.

## Development principles

- Keep Core provider-neutral and dependency-light.
- Preserve .NET Framework 4.8.1 compatibility where targeted and support .NET 9 where supported.
- Design for low RAM and no GPU assumption.
- Keep runtime work cancellable, bounded, correlated, concurrent, and safe against stale results.
- Keep persistent configuration separate from live runtime state.
- Use generic contracts for future extensibility rather than hard-coded host concepts.
- Verify completed capabilities through `HAgent.Example` before marking them complete.
- Keep authoritative documentation synchronized with implementation.

## What success looks like

A developer can start with:

```csharp
await ai.SendAsync("assistant", "Hello");
```

and later grow the integration into:

```text
host
  -> generic execution/context requests
  -> multiple runtime agent instances
  -> private/shared memory
  -> reusable skills
  -> scoped Wiki/knowledge
  -> controlled learning
  -> authorized tools
  -> structured model output
  -> workspace routing
  -> asynchronous background work
```

without replacing HAgent or introducing application-specific types into `HAgent.Core`.

## Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.959 Human-in-the-Loop and Intervention — CURRENT

Phase 0.959 is the current intervention foundation. HAgent now has a canonical provider-neutral intervention request/lifecycle contract built on the bounded approval/defer boundary from 0.953. Execution intervention control and execution-target concurrency/stale-state hardening are implemented; the hardening slice remains unverified in this connected environment because there is no executable repository build/test workflow and no local checkout.

### Objective

Allow authorized humans or host applications to inspect and control active HAgent work without creating a bypass around execution, policy, permissions, authorization, capabilities, budgets, cancellation, or host validation.

### Completed in this milestone so far

- Canonical `AiInterventionRequest` with request identity, target kind, requested action, lifecycle status, execution/resource context, HAgent/host correlation, requester/responder identity, policy reason, target state/version evidence, and resolution metadata.
- `IAiInterventionWorkflow` and bounded in-memory implementation with cloned request boundaries and terminal-state protection.
- Intervention lifecycle requires `Pending -> Approved -> Completed` for an accepted intervention; stale application may terminate as `Approved -> Expired`; terminal requests cannot be resolved again.
- Tool execution creates canonical intervention requests for `RequireApproval` and `Defer`, with explicit tool target and requested-action semantics.
- Tool execution results expose the intervention request.
- Deterministic Example approval/defer verification uses the canonical intervention API.
- Obsolete approval-only contract/facade files were removed in favor of the intervention model.
- `DefaultAgentRuntime` owns the canonical execution intervention coordinator and links intervention cancellation into the existing execution cancellation path.
- Execution pause/resume/cancel requests use the shared `HAgentClient` intervention workflow and do not introduce a second execution engine.
- `HAgentClient.ExecutionChanged` and execution intervention APIs expose the host-facing control boundary through public APIs.
- The `EXECUTION INTERVENTION` Example passed local verification on 2026-09-08, covering pause/resume/cancel lifecycle, public execution events, terminal cancellation, and late provider response protection.
- Execution intervention requests now capture the observed control state and monotonic control-state version.
- Competing execution interventions are serialized per execution and stale requests resolve deterministically to `Expired`.
- Duplicate responder resolution is protected by request lifecycle state, and a stale request remains queryable with its responder and stale reason.
- A deterministic `INTERVENTION HARDENING` Example scenario has been added for terminal staleness, conflicting concurrent requests, paused-state blocking, and duplicate responders.

### Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **Complete — Execution control boundary**
   - Scope: Integrate intervention application into the existing canonical runtime/execution lifecycle for pause, resume, and cancellation; preserve the existing execution engine and terminal-state rules.
   - Entry: Canonical intervention workflow and execution lifecycle contracts exist.
   - Implementation state: Complete in source; focused Example verification added.
   - Verification state: **VERIFIED** — user executed the `EXECUTION INTERVENTION` Example on 2026-09-08 and all expected lifecycle, cancellation, and late-response assertions passed.
   - Completion: Controlled execution can be paused/resumed/cancelled through the intervention boundary without a second execution engine, and focused deterministic verification passes in an executable environment.

2. **CURRENT — Concurrency and stale-state hardening**
   - Scope: Make intervention state transitions deterministic under concurrent requests, duplicate responders, late provider completion, retirement/shutdown teardown, and already-terminal executions.
   - Entry: Slice 1 passes its focused lifecycle verification.
   - Implementation state: Complete in source; deterministic Example verification added.
   - Verification state: **BLOCKED** — no executable repository build/test workflow is available through the connected environment, and local repository checkout is unavailable in this session.
   - Completion: concurrency/stale-request tests pass and late results cannot overwrite terminal outcomes.
   - Next smallest step after verification: additional intervention targets.

3. **Additional intervention targets**
   - Scope: Extend the same canonical intervention mechanism to plan steps, goals, learning candidates, and consequential actions where defined by the architecture.
   - Entry: lifecycle/concurrency semantics are stable.
   - Completion: each supported target/action pair has explicit authorization/policy semantics and focused deterministic verification.

4. **Durable intervention persistence**
   - Scope: Persist intervention lifecycle/history through the existing canonical storage architecture without creating a parallel persistence model.
   - Entry: lifecycle and target semantics are stable.
   - Completion: persistence/reload, ownership, and terminal-state behavior are verified against the supported storage contracts.

5. **Management UI and diagnostics**
   - Scope: Expose pending/history intervention state through the designated configuration/management surfaces and diagnostics while keeping UI as a consumer of the canonical contracts.
   - Entry: persistence and lifecycle contracts are stable.
   - Completion: UI opens/loads, displays correct state, issues authorized controls, and handles stale/completed requests safely in the supported WinForms targets.

6. **Example coverage expansion**
   - Scope: Add deterministic public-API Example scenarios for pause/resume, cancellation, concurrency, stale requests, target/action transitions, persistence, and failure boundaries.
   - Entry: implementation and UI contracts are stable enough to exercise end-to-end.
   - Completion: all required scenarios are reproducible and the Example host remains organized by feature.

7. **Final framework/backend verification**
   - Scope: Run the supported .NET Framework 4.8.1 and .NET 9 verification plus backend-specific live verification where configured.
   - Entry: all implementation slices and Example verification are complete.
   - Completion: actual builds/tests/examples have been executed and the authoritative documentation records the verified milestone state.

### Architectural boundaries

The intervention boundary is provider-neutral and does not authenticate principals or replace host authorization. Policy decides when an intervention/approval boundary is required; intervention state records and applies the authorized control through the owning runtime boundary.

Approval or intervention acceptance never directly executes a protected tool/provider call, silently resumes work, grants host authorization, or creates capabilities. The target runtime must still enforce policy, permissions, capability, budget, cancellation, and host-side validation.

The canonical lifecycle, target/action semantics, concurrency rules, persistence boundary, and management UI requirements are defined in `docs/architecture/92-human-intervention.md`.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example or focused test verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

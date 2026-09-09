# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.956 Observability and Distributed Tracing
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Build the ordered 0.956 observability/tracing foundation after verified completion of 0.955 Context Engineering.

## Current checkpoint

0.956 Slice 6 safe human-readable diagnostic projection is complete and verified on both supported Example targets. The provider-neutral bounded projection exposes diagnostic trace structure without exposing raw trace storage, prompts, provider responses, tool payloads, host context, secrets, or arbitrary objects.

## Current run

**0.956 Slice 7 cross-process trace context and correlation boundary — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING.**

Slice 7 now provides a bounded provider-neutral host/transport boundary for moving `TraceContext` and existing correlation identities across process boundaries. It does not implement a wire transport or remote telemetry service.

## Implemented in Slice 7

- Added bounded `TracePropagationCarrier`, `TracePropagationImportOptions`, `TracePropagationImportStatus`, and `TracePropagationImportResult` contracts.
- Extended `TracePropagation` with deterministic export/import of trace context and bounded deployment, tenant, principal, user, session, workspace, agent-profile, runtime, execution, execution-correlation, host-correlation, event, and causation identities.
- Preserved distinct trace, execution, host, event, and causation identities rather than collapsing them into one cross-process value.
- Added explicit host trust acceptance for incoming trace context; the default rejects untrusted incoming trace context.
- Added safe handling for missing, malformed, incomplete, unsampled, and oversized incoming propagation values.
- Kept Core independent of HTTP headers, W3C/OpenTelemetry types, vendor SDKs, message buses, and remote telemetry delivery.
- Added focused `tests/HAgent.Tests/ObservabilityTracePropagationTests.cs` covering round-trip propagation, identity separation, unsampled state, missing context, explicit trust rejection, malformed trace input, invalid sampled state, oversized correlation input, and carrier clone/bounds behavior.
- Added `src/HAgent.Example/MainForm.ObservabilityTracePropagation.cs` and registered it under `Diagnostics → Observability → Observability Trace Propagation`.
- Updated `docs/architecture/22-observability.md` with the cross-process propagation boundary and its trust/validation semantics.

## Slice 7 verification boundary

- Build the solution after pulling the current branch.
- Run the full `HAgent.Tests` suite.
- Run `HAgent.Example → Diagnostics → Observability → Observability Trace Propagation` on .NET Framework 4.8.1.
- Run the same Example on .NET 9.
- Confirm no real network transport, remote telemetry service, or real provider request is contacted.
- Do not mark Slice 7 verified until all required local results are supplied.
- Do not begin Slice 8 in the same run.

## Current blockers

No known architecture blocker. Local .NET/WinForms execution remains user-side verification for connected implementation runs.

## Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.956 Observability and Distributed Tracing — CURRENT.**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage now explicitly includes the first-class resource foundation for Knowledge, Skills, Memory, and Learning candidates. Its storage/resource primitives are substantially implemented; mature resource governance and learning promotion remain intentionally later work.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.952 First-Class Event Subsystem is completed and verified. 0.953 Unified Policy Engine is completed for its verified runtime/persistence/resource/learning-policy foundation. 0.954 Prompt and Instruction Governance is completed and verified on .NET Framework 4.8.1 and .NET 9. 0.955 Context Engineering is completed and verified on .NET Framework 4.8.1 and .NET 9, including provider-neutral context contracts, bounded multi-resource retrieval, deterministic ranking/compaction/caching, policy/capability admission, host authorization for protected data-backed sources, execution/provider integration, and end-to-end Context Example verification.

The remaining ordered foundation includes 0.956 Observability/Tracing, 0.957 Evaluation/Quality Measurement, 0.9575 Knowledge/Skills/Memory Governance + Learning, 0.958 Agent Lifecycle/Health, 0.9591 Goal/Plan Persistence/Recovery, and 0.959 Human-in-the-Loop/Intervention. Phase 0.9591 remains intentionally ordered before 0.959 because durable goal/plan revisions, checkpoints, and recovery state provide the persistent authority for later goal/plan intervention. Execution-level intervention remains valid independently. 0.9592 Provider Ecosystem/Adapter Lifecycle follows these foundations, then 0.96 Capability-Aware Execution.

## First-class Knowledge / Skills / Memory / Learning architecture

HAgent no longer treats the old Phase 0.11 block as one late feature layer. The architecture is split across the roadmap according to dependency:

```text
0.8 Data Access / Internal Storage + Resource Foundations
    ↓
0.951–0.953 Identity / Events / Policy
    ↓
0.954–0.957 Instruction / Context / Observability / Evaluation
    ↓
0.9575 Mature Resource Governance + Learning
    ↓
0.958+ Lifecycle / Recovery / Intervention / Provider / Execution foundations
    ↓
0.97 Persistent Cognitive Runtime
```

The 0.8 resource foundation provides canonical provider-neutral resource identity, scope/ownership metadata, provenance, lifecycle/versioning, Skill definitions/references, Knowledge/Wiki contracts, Memory family/type foundations, typed learning-candidate foundations, and HAgent-owned persistence substrate. It is deliberately foundational rather than a complete management/governance feature.

Phase 0.9575 completes mature governance: resource authorization, capability inheritance, runtime tri-state overrides, effective execution resource snapshots, bounded retrieval/retention, resource management UI, Learning Mode, learning policy, evaluation-aware candidate validation, approval/promotion, version/conflict handling, and safe promotion into authoritative resources.

Knowledge, Skills, Memory, and Learning remain distinct. Skills are reusable executable capabilities; Knowledge is reusable retrievable information; Memory is scoped experience/state; Learning is the governed transformation of experience into typed candidates and, where permitted, promoted authoritative resource state.

Existing ahead-of-roadmap implementation may already provide pieces of this architecture. Such code remains useful implementation evidence but does not make 0.9575 complete until its full requirements and verification are reached.

## Foundational architecture hardening before 0.96

The ordered sequence is now:

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Event subsystem
        ↓
0.953 Unified Policy Engine
        ↓
0.954 Prompt / Instruction Governance — verified
        ↓
0.955 Context Engineering — verified
        ↓
0.956 Observability / Tracing — current
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

The numbered roadmap is dependency-driven, not permanently locked. When architectural understanding reveals a real dependency change, the roadmap may be reordered deliberately and the authoritative roadmap/current-state documents must be updated together. Ahead-of-roadmap implementation remains code evidence rather than milestone completion.

## 0.955 Context Engineering completion boundary

0.955 Context Engineering is complete and verified. The canonical context subsystem now provides provider-neutral bounded context items and snapshots; separate retrieval planning; deterministic ranking, deduplication, compaction, and provenance-safe diagnostics; reusable cache components; execution/provider context propagation; policy/capability admission; host authorization composition for protected data-backed sources; and one canonical end-to-end assembly boundary. The context subsystem remains distinct from cognitive decision making and does not duplicate policy, host authorization, or instruction authority. Its complete verified Example/test matrix was exercised on .NET Framework 4.8.1 and .NET 9 through the ordered Context slices.

## Storage implications

The configuration/storage evolution defined by `docs/roadmap/38-configuration-storage-and-portability.md` remains a cross-cutting foundation before 0.96. It must directly support authoritative resource relationships, scoped ownership, capability state, and portable configuration rather than creating a second resource configuration model.

The unified policy set is now a canonical HAgent-owned configuration record exposed through `IAiStore`. File storage persists it with the main settings document; SQL Server and MySQL persist it in `HAgentPolicies`, and their HAgent database bootstrap paths create that table. The default runtime loads the current persisted policy asynchronously at execution creation when no explicitly injected policy engine is supplied, then captures the effective policy in the execution snapshot.

Agent profile resource capability defaults are now part of the canonical `AiAgent` configuration and persist through the normal agent storage path. Runtime capability overrides remain transient and resolve above profile defaults into `AgentExecutionSnapshot.EffectiveResourceCapabilities`.

Learning-promotion decisions are now represented as normal `AiPolicyDecision` outcomes on `learning.promote`, with typed candidate metadata carried as bounded policy attributes. This keeps learning governed by the single policy engine rather than adding a parallel learning authorization mechanism. Mature candidate/target repositories and broader human intervention remain ordered roadmap work.

Human intervention is implemented ahead of its ordered milestone only as a coherent canonical workflow boundary. Execution-level intervention and concurrency/stale-state hardening have deterministic local Example verification; durable persistence, management UI, broader target support, and remaining lifecycle controls are not treated as complete.

Provider API keys are persisted with provider configuration and encrypted at rest. There is no separate provider secret-reference or vault architecture. Shared SQL Server/MySQL configuration can therefore be used by multiple authorized HAgent processes/machines. Configuration export/import is planned as a versioned portable representation with optional encrypted credential inclusion.

### Deferred exclusions

The roadmap does not introduce a separate distributed-coordination subsystem and does not introduce a sophisticated external secret-management architecture. Distributed behavior is handled through the existing storage/runtime contracts where required, while provider credentials use the project's intentionally simple fixed encryption/decryption mechanism.

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

## 0.956 Observability and Distributed Tracing — CURRENT

Phase 0.955 Context Engineering is complete and verified on .NET Framework 4.8.1 and .NET 9. The next ordered foundational milestone is 0.956 Observability and Distributed Tracing.

### 0.955 Context Engineering — VERIFIED

The complete 0.955 implementation and verification sequence is complete. Verified work includes provider-neutral context contracts, bounded acquisition, deterministic ranking/deduplication, compaction, reusable caching, execution/provider integration, bounded multi-resource retrieval, policy/capability admission, host authorization for protected data-backed sources, and end-to-end assembly.

### 0.956 Run-sized execution plan

1. **Observability contract inventory and architecture review — VERIFIED**
   - Reconciled existing execution/correlation identity, identity context, execution audit, lifecycle state, event correlation/causation, policy decisions, tool-call context, context/admission diagnostics, and provider boundaries against the 0.956 requirements.
   - Added the authoritative provider-neutral observability architecture in `docs/architecture/22-observability.md`.
   - Established that trace/span identity is a new observability relationship and does not replace `ExecutionId`, `AgentExecution.CorrelationId`, `HostCorrelationId`, runtime identity, event IDs, or causation IDs.
   - Established bounded default-deny trace metadata, explicit redaction before sinks, vendor-neutral sampling/retention semantics, and the separation between tracing, event dispatch, authorization, execution audit, and transcript/payload storage.
   - Identified the exact first implementation boundary: **0.956 Slice 2 — Trace identity and span lifecycle contracts**, with focused `HAgent.Tests` and matching public-API `HAgent.Example` verification.
   - No tracing implementation, exporter, persistence, UI, or broad runtime instrumentation was started in this slice.

2. **Trace identity and span lifecycle contracts — VERIFIED**
   - Implemented the provider-neutral Core trace context/span contracts and in-memory recorder boundary defined by `docs/architecture/22-observability.md`.
   - Added focused `HAgent.Tests` coverage for hierarchy, correlation propagation, bounded redacted/omitted metadata, terminal completion protection, and deterministic ordering.
   - Added and classified the matching public-API `HAgent.Example` scenario under `Diagnostics → Observability → Observability Tracing`.
   - **User verification — 2026-09-09:** solution build succeeded after pull; `HAgent.Tests` completed with **59/59 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 01:37:** `Observability Tracing` succeeded, verifying hierarchy, execution/host/runtime/event correlation, redaction/omitted metadata, terminal statuses/late completion rejection, deterministic recorder order, and no provider request.
   - **User Example verification — .NET 9, 2026-09-09 01:38:** same public-API scenario succeeded with the same contract checks.
   - No Slice 3 implementation was started in this run.

3. **Trace-producing runtime instrumentation and propagation — VERIFIED**
   - Added provider-neutral tracing producers/decorators for the canonical execution lifecycle, policy evaluation, provider invocation, tool execution, context assembly, and event publication/handling boundaries.
   - Added `TracePropagation` as the explicit host/internal propagation scope and `EventEnvelope.TraceContext` as the optional provider-neutral event propagation field; neither replaces existing correlation/causation identity.
   - Kept runtime lifecycle as the sole execution producer so policy/context tracing is not duplicated when their dedicated tracing boundaries are composed.
   - Preserved default-deny payload handling: tool arguments, event payload/context, prompts, provider responses, and raw host payloads are not copied into trace metadata.
   - Hardened nested execution restoration so an outer trace context/correlation is restored after a traced execution completes or fails.
   - Added focused `HAgent.Tests` coverage for execution/policy/provider hierarchy, tool/context/event parent propagation, correlation preservation, payload exclusion, failure and cancellation terminal statuses, and nested ambient-context restoration.
   - Added and classified the matching public-API `HAgent.Example` scenario under `Diagnostics → Observability → Observability Runtime Instrumentation`; it exercises success, provider failure, cancellation, policy/provider hierarchy, tool/context/event propagation, event handler parentage, correlation preservation, and payload omission using only deterministic in-process fakes.
   - **User verification — 2026-09-09:** `HAgent.Tests` completed with **63/63 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 03:49:21:** `Observability Runtime Instrumentation` succeeded, verifying execution/policy/provider hierarchy, distinct execution/host correlation, tool/context/event propagation, event publication→handler parentage, sensitive payload omission, failure terminal status, cancellation terminal status, deterministic fake provider transport, and no real provider request.
   - **User Example verification — .NET 9, 2026-09-09 03:50:11:** same public-API scenario succeeded with the same checks.
   - A final Slice 3 test correction was required for ambient correlation propagation through tool/context/event boundaries; the resulting full suite passed 63/63.

4. **Sampling and bounded retention controls — VERIFIED**
   - Added provider-neutral `TraceSamplingOptions`, `ITraceSampler`, and `TraceRetentionOptions` contracts.
   - Added deterministic root sampling through `DeterministicTraceSampler`; sampled state is inherited by child spans through `TraceContext` and unsampled spans remain lifecycle-capable without being retained.
   - Extended `InMemoryTraceRecorder` with optional sampling and bounded retention while preserving the existing no-argument recorder behavior.
   - Added retention bounds for maximum trace count, maximum span count, maximum spans per trace, aggregate metadata characters, and maximum age; eviction operates on whole older traces where possible and avoids evicting the active trace merely to admit a child span.
   - Sampling/retention do not alter execution, authorization, event delivery, or span completion semantics, and existing default-deny metadata behavior remains unchanged.
   - Added focused `HAgent.Tests/ObservabilitySamplingRetentionTests.cs` covering deterministic sampling, unsampled inheritance/suppression, retention bounds, per-trace limits, aggregate metadata limits, and lifecycle independence.
   - Added and classified the matching public-API `HAgent.Example/MainForm.ObservabilitySamplingRetention.cs` scenario under `Diagnostics → Observability → Observability Sampling & Retention`.
   - **User verification — .NET 9, 2026-09-09:** full `HAgent.Tests` completed with **67/67 tests passed**.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 04:16:56:** `Observability Sampling & Retention` succeeded, verifying deterministic sampling stability, unsampled child inheritance/suppression, lifecycle independence from sampling/retention, maximum retained traces/spans, per-trace bounds, aggregate metadata bound, no provider transport, and no real provider request.
   - **User Example verification — .NET 9, 2026-09-09 04:17:25:** same public-API scenario succeeded with the same checks.
   - The supplied latest verification did not separately restate a solution-build result; therefore only the explicitly supplied verification results are recorded here.

5. **Integrated trace sinks and safe export boundary — VERIFIED**
   - Added provider-neutral `ITraceSink` and bounded `TraceSinkOptions` contracts.
   - Added `TraceSinkDispatcher` with a bounded non-blocking enqueue boundary, FIFO processing, asynchronous sink delivery, flush support for deterministic tests, and isolated sink-failure accounting.
   - Integrated sink dispatch into `InMemoryTraceRecorder` only after a sampled span completes and only while that span remains retained by the recorder; sampled-out and retention-rejected spans never cross the sink boundary.
   - Preserved the existing trace metadata/redaction contract; sinks receive the canonical bounded `TraceSpan` rather than prompts, provider payloads, tool payloads, host raw context, or arbitrary serialized objects.
   - Sink latency, queue saturation, and sink exceptions remain telemetry concerns and do not alter span lifecycle completion or execution correctness. One failing sink does not prevent other registered sinks from receiving the same span.
   - Added focused `tests/HAgent.Tests/ObservabilitySinksTests.cs` covering FIFO delivery, sink-failure isolation, slow asynchronous sink behavior, bounded queue saturation, sampled-out suppression, and retention-boundary suppression.
   - Added and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilitySinks.cs` scenario under `Diagnostics → Observability → Observability Sinks`.
   - **User verification — 2026-09-09:** full `HAgent.Tests` completed with **72/72 tests passed**.
   - **User Example verification — .NET 9, 2026-09-09 04:59:10:** `Observability Sinks` succeeded, verifying retained sampled span delivery order, sink-failure isolation, slow-sink non-blocking span completion, asynchronous flush behavior, bounded queue saturation/drop behavior, sampled-out suppression, retention-rejected suppression, no remote telemetry transport, and no real provider request.
   - **User Example verification — .NET Framework 4.8.1, 2026-09-09 04:59:52:** same public-API scenario succeeded with the same checks.
   - The prior Example-only duplicate-helper compiler error was fixed before these successful runs.

6. **Safe human-readable diagnostic projection — VERIFIED**
   - Added provider-neutral `TraceDiagnosticProjectionOptions`, `TraceDiagnosticMetadataItem`, `TraceDiagnosticSpan`, and `TraceDiagnosticProjection` contracts.
   - Added `TraceDiagnosticProjector` as the bounded management/UI projection boundary. It consumes retained sampled spans only, orders them deterministically by trace sequence/identity, and bounds result counts, identifiers, operation/kind text, metadata entries, metadata keys, and metadata values.
   - The projector allowlists diagnostic metadata namespaces (`admission`, `agent`, `context`, `event`, `evaluation`, `execution`, `failure`, `knowledge`, `learning`, `lifecycle`, `memory`, `outcome`, `planning`, `policy`, `provider`, `resource`, `runtime`, `sampling`, and `tool`) plus the explicit `decision` key. Unknown/custom metadata is omitted and counted rather than rendered.
   - Existing `[Redacted]`/`[Omitted]` markers are preserved only for metadata that is otherwise safe to expose; the projection never exposes raw prompts, responses, tool payloads, host context, credentials, connection strings, or arbitrary serialized objects.
   - The projection exposes bounded trace/span identity, operation/kind, start/duration/status, parent relationship, execution correlation, host correlation, and runtime identity suitable for diagnostic UI consumption.
   - Added focused `tests/HAgent.Tests/ObservabilityDiagnosticProjectionTests.cs` covering deterministic ordering, span/text bounds, safe metadata allowlisting, omission accounting, explicit redaction markers, parent/correlation visibility, status/duration, and unsampled suppression.
   - Added and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilityDiagnosticProjection.cs` under `Diagnostics → Observability → Observability Diagnostic Projection`.
   - **User verification — 2026-09-09 05:08:** .NET Framework 4.8.1 Example succeeded, verifying deterministic ordering, bounds, correlation/parent relationship, status/duration, metadata allowlisting, redaction, omission accounting, unsampled suppression, and no provider/remote telemetry transport or real provider request.
   - **User verification — 2026-09-09 05:09:** .NET 9 Example succeeded with the same checks.
   - **User verification — 2026-09-09:** full `HAgent.Tests` completed with **77/77 tests passed** after the Slice 6 Example helper correction.

7. **Cross-process trace context and correlation boundary — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING**
   - Added provider-neutral `TracePropagationCarrier`, `TracePropagationImportOptions`, `TracePropagationImportStatus`, and `TracePropagationImportResult` contracts with bounded key/value counts and lengths.
   - Extended `TracePropagation` to export `TraceContext` plus the existing bounded correlation identities into the carrier and import them deterministically without coupling Core to HTTP, message buses, OpenTelemetry, or vendor-specific telemetry.
   - Preserved the distinction between `TraceId`, `ParentSpanId`, sampled state, and `ExecutionId`, `ExecutionCorrelationId`, `HostCorrelationId`, `EventId`, `CausationId`, plus the other bounded identity dimensions.
   - Incoming trace context is explicitly rejected as untrusted by default; missing context returns `Missing`, malformed trace data returns `InvalidTraceContext`, malformed correlation data returns `InvalidCorrelation`, and accepted unsampled state remains unsampled.
   - Correlation values remain diagnostic identity only and do not become authentication or authorization authority. Host trust remains outside Core.
   - Added focused `tests/HAgent.Tests/ObservabilityTracePropagationTests.cs` covering round-trip identity preservation, unsampled propagation, missing context, explicit trust rejection, malformed context, invalid sampling, oversized correlation input, and carrier clone/bounds behavior.
   - Added and classified the matching public-API `src/HAgent.Example/MainForm.ObservabilityTracePropagation.cs` under `Diagnostics → Observability → Observability Trace Propagation`.
   - **Local verification required:** after pull, build the solution, run the full `HAgent.Tests` suite, then run the exact Example scenario on .NET Framework 4.8.1 and .NET 9. Do not begin Slice 8 in the same run.

### Verification rule

A slice becomes complete only after its implementation exists, matching deterministic or focused verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless actually performed.

### Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

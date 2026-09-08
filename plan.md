# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.959 Human-in-the-Loop / Intervention
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the canonical intervention lifecycle, execution control, concurrency/stale-state hardening, additional targets, durable persistence, management UI, expanded Example verification, and final framework/backend verification defined by the active implementation plan.

## Current checkpoint

Slice 1 execution intervention control was verified locally by the user on 2026-09-08 through the `EXECUTION INTERVENTION` Example, including pause/resume/cancel lifecycle and late provider response protection. Slice 2 concurrency/stale-state hardening is implemented in source with target state/version evidence, per-execution resolution serialization, explicit stale expiry, teardown-race handling, and a deterministic `INTERVENTION HARDENING` Example, but that new Example has not yet been locally executed in this connected environment.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the active scope is explicitly changed.

## Current blockers

Local verification of the new intervention-hardening slice is not available through the connected environment because the repository has no executable code build/test workflow exposed here and there is no local checkout in this session. The user must run the updated Example after pulling the latest commits before Slice 2 can be marked verified and Slice 3 can begin.

## Next checkpoint

Build and run HAgent Example, then execute **INTERVENTION HARDENING → Run intervention concurrency/stale-state test**. Verify terminal stale requests resolve as `Expired`, target state/version evidence is captured, conflicting concurrent requests produce one applied transition and one stale request, paused work remains blocked until a fresh resume intervention, and duplicate responder resolution cannot apply a second transition.

## Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.954 Prompt and Instruction Governance — CURRENT.**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage now explicitly includes the first-class resource foundation for Knowledge, Skills, Memory, and Learning candidates. Its storage/resource primitives are substantially implemented; mature resource governance and learning promotion remain intentionally later work.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.952 First-Class Event Subsystem is completed and verified. 0.953 Unified Policy Engine is completed for its verified runtime/persistence/resource/learning-policy foundation. 0.954 Prompt and Instruction Governance is the current ordered foundational milestone. Its authoritative plan is `docs/plan/20-active.md` and its detailed requirements are in `docs/roadmap/954-prompt-instruction-governance.md`.

The remaining ordered foundation now includes 0.955 Context Engineering, 0.956 Observability/Tracing, 0.957 Evaluation/Quality Measurement, 0.9575 Knowledge/Skills/Memory Governance + Learning, 0.958 Agent Lifecycle/Health, 0.9591 Goal/Plan Persistence/Recovery, and 0.959 Human-in-the-Loop/Intervention. Phase 0.9591 remains intentionally ordered before 0.959 because durable goal/plan revisions, checkpoints, and recovery state provide the persistent authority for later goal/plan intervention. 0.9592 Provider Ecosystem/Adapter Lifecycle follows these foundations, then 0.96 Capability-Aware Execution.

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

## 0.954 Prompt and Instruction Governance — CURRENT

Phase 0.954 is the next ordered foundational milestone after the verified 0.953 Unified Policy Engine. The repository roadmap defines 0.954 through 0.958 as foundations that precede 0.9591 Goal/Plan Persistence and Recovery and 0.959 Human-in-the-Loop / Intervention. Work that was implemented ahead of this order in 0.959 remains in source but is not treated as the current milestone or as evidence that the intervening phases are complete.

### Pre-phase Example UI prerequisite — CURRENT

Before beginning 0.954 implementation, the manual verification host must be reorganized according to the Example UI rules in `AGENTS.md`: architecture-level top-level feature tabs, with multiple examples represented by nested focused tabs instead of an ever-growing flat tab list.

- Entry: existing Example host contains a growing flat tab collection and the roadmap has been reset to the ordered 0.954 milestone.
- Scope: reorganize the existing Example UI into feature groups without changing the underlying Example test behavior; restore the existing `LEARNING INTERVENTION` example to the visible host; keep Example code split into focused partial files.
- Implementation: `src/HAgent.Example/MainForm.ExampleOrganization.cs` creates the feature-group/nested-tab presentation during `MainForm.OnLoad` and adds the already-implemented learning intervention example before grouping.
- Verification: user builds/runs `HAgent.Example`, confirms the top-level feature groups and nested examples are usable, confirms `LEARNING INTERVENTION` is present under `Cognition`, and confirms existing examples remain independently runnable.
- Completion: the UI is locally verified and no Example test capability is lost or duplicated.

### 0.954 Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **Instruction source and authority contracts**
   - Scope: Define the provider-neutral normalized instruction/source model, source type, authority/trust level, provenance, scope, lifecycle metadata, deterministic precedence, conflict representation, and execution-snapshot provenance.
   - Entry: Verified Example UI prerequisite and verified 0.953 Unified Policy Engine; existing prompt construction and execution snapshot boundaries identified.
   - Completion: Core contracts can represent trusted and untrusted instruction sources with deterministic authority/precedence semantics and evidence suitable for execution snapshots.
   - Verification: Add deterministic Example coverage for source creation/validation, precedence, authority separation, and provenance.

2. **Instruction composition and conflict handling**
   - Scope: Integrate the contracts into canonical prompt/instruction assembly, preserve higher-authority layers, handle conflicts/invalid or unavailable sources, and keep secrets/sensitive host data out of diagnostics by default.
   - Entry: Slice 1 verified.
   - Completion: canonical instruction composition produces a deterministic provider-neutral snapshot and rejects/contains invalid authority transitions.

3. **Resource and external-content boundaries**
   - Scope: Integrate Skills, Knowledge, Memory, tool descriptions, runtime context, host context, user content, and externally retrieved content with explicit trust/provenance semantics while keeping authorization outside prompt text.
   - Entry: composition semantics verified.
   - Completion: lower-authority/untrusted content cannot erase higher-authority policy or instruction layers, and disabled/unavailable sources remain diagnosable.

4. **Execution integration**
   - Scope: Feed the effective instruction snapshot into the existing execution boundary without creating a second execution/prompt engine and preserve active-execution snapshot isolation.
   - Entry: instruction model and composition are stable.
   - Completion: running executions retain immutable effective instruction state even when source configuration changes.

5. **Example coverage and framework verification**
   - Scope: Add deterministic public-API Example scenarios for precedence, conflicts, untrusted content, disabled resources, provenance, snapshot isolation, cancellation/failure boundaries, and supported framework targets.
   - Entry: implementation is stable.
   - Completion: the new Example coverage passes locally on the supported targets and the authoritative phase documents record verification.

6. **Advance to 0.955 Context Engineering**
   - Scope: Only after 0.954 is verified, update the active plan to the next roadmap phase.

### Relationship to ahead-of-roadmap intervention work

A prior run advanced into 0.959 before 0.954–0.958 were completed. That was an ordering mistake, not a reason to redefine the roadmap. The existing 0.959 intervention code is retained as ahead-of-roadmap work in the source tree, but it is not considered a completed project milestone until the ordered foundational phases and their required verification are reached.

The roadmap now places 0.9591 Goal/Plan Persistence and Recovery before 0.959 Human-in-the-Loop / Intervention so durable goal/plan revision and recovery authority exists before later intervention can govern those durable targets. Execution-level intervention remains valid independently and is preserved in source.

### Architectural boundaries

Prompt/instruction governance is an authority and provenance boundary, not an authorization mechanism. Prompt text cannot grant permissions, bypass policy, approve protected operations, or elevate untrusted content. Provider adapters receive provider-neutral effective instructions and remain responsible only for transport-specific representation.

The authoritative requirements for this phase are in `docs/roadmap/954-prompt-instruction-governance.md` and the broader instruction architecture. The roadmap order is normative unless an explicit architectural decision records a dependency-driven exception.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example or focused test verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

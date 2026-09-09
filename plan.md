# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 1 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Establish the mature resource-governance boundary over the existing 0.8 resource foundations: explicit capability policy, effective runtime resource state, governed access to Skills/Knowledge/Memory families, and typed learning-candidate admission without introducing a parallel resource model.

## Completed milestone

0.957 Evaluation and Quality Measurement is **verified through Slice 6** on 2026-09-09. The user verified the required Example scenarios on .NET Framework 4.8.1 and .NET 9 and reported **139/139 HAgent.Tests passed**.

The completed evaluation path is:

- Slice 1 — provider-neutral evaluation contracts and evaluator boundary.
- Slice 2 — deterministic evaluators and evaluation evidence.
- Slice 3 — human/application ratings and labeled evaluation evidence.
- Slice 4 — model-assisted evaluators and non-authoritative judge boundary.
- Slice 5 — aggregation and alternative-target comparison.
- Slice 6 — repeated regression cases and bounded alternative-target execution.

Evaluation remains measurement-only. Aggregation and regression results do not authorize, route production execution, mutate configuration, promote learning, or become cognitive authority.

**Latest verification evidence:** `HAgent.Tests` 139/139 passed; `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` succeeded on .NET Framework 4.8.1 and .NET 9.

## Current Slice 1 — Mature resource governance foundation

The next work is Phase 0.9575, consuming the canonical resource foundations established in 0.8 and the identity/policy/evaluation boundaries already verified. Do not create a second Skill/Knowledge/Memory/Learning model.

The first slice should establish the provider-neutral governance boundary for effective resource capability state and governed access decisions while keeping authoritative resource definitions separate from runtime snapshots.

### Verification checkpoint

Implementation and matching Example/test coverage are pending for Phase 0.9575 Slice 1.

**Example to run:** `HAgent.Example → Cognition → Resource Governance` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/ResourceGovernanceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

No design blocker is known. Before coding, read the authoritative resource/identity/policy architecture documents and inspect the existing resource capability, runtime snapshot, and learning-policy implementation so the new governance boundary composes existing contracts instead of duplicating them.

## Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.957 Evaluation and Quality Measurement — CURRENT.**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage now explicitly includes the first-class resource foundation for Knowledge, Skills, Memory, and Learning candidates. Its storage/resource primitives are substantially implemented; mature resource governance and learning promotion remain intentionally later work.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.95 Generic External Host Integration is complete and verified on .NET Framework 4.8.1 and .NET 9, including canonical generic execution requests, provider-facing request isolation, structured-output validation/native transport, terminal-state protection, runtime snapshot isolation, external-consumer verification, and composition of long-lived runtime instances with canonical execution requests.

0.952 First-Class Event Subsystem is completed and verified. 0.953 Unified Policy Engine is completed for its verified runtime/persistence/resource/learning-policy foundation. 0.954 Prompt and Instruction Governance is completed and verified on .NET Framework 4.8.1 and .NET 9. 0.955 Context Engineering is completed and verified on .NET Framework 4.8.1 and .NET 9. 0.956 Observability and Distributed Tracing is completed and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9, including authoritative execution-outcome observations consumed by tracing without provider-side state inference.

0.957 Slice 1 (provider-neutral evaluation contracts and evaluator boundary) is verified. Slice 2 (deterministic evaluators and evaluation evidence) is verified with 109/109 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification. Slice 3 (human/application ratings and labeled evaluation evidence) is verified with 115/115 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification. Slice 4 (model-assisted evaluators and non-authoritative judge boundary) is verified with 123/123 HAgent.Tests and successful Example verification on .NET Framework 4.8.1 and .NET 9.

Slice 5 (evaluation aggregation and alternative-target comparison) is **verified** from user-run results on 2026-09-09: **130/130 HAgent.Tests passed**, and `Diagnostics → Evaluation → Evaluation Aggregation` succeeded on **.NET Framework 4.8.1 and .NET 9**. The verified scenario produced baseline success rate `0.333333`, candidate success rate `1`, candidate average quality `0.85`, candidate average latency `110 ms`, and no authoritative routing or authorization decision.

Slice 6 (evaluation regression suites and repeated target execution) is the current implementation checkpoint. It adds provider-neutral bounded case/target suite contracts, host-owned execution through `IAiEvaluationRegressionExecutor`, bounded concurrency, deterministic case-target results, failure isolation, cancellation/late-result protection, and direct handoff of completed samples into the existing Slice 5 aggregation/comparison contracts. Automated build/test verification and manual Example execution for Slice 6 are pending.

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

## Foundational architecture hardening before 0.96

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
0.956 Observability / Tracing — verified
        ↓
0.957 Evaluation / Quality Measurement — current
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

The numbered roadmap is dependency-driven, not permanently locked. When architectural understanding reveals a real dependency change, the roadmap and current-state documents must be updated together. Ahead-of-roadmap implementation remains code evidence rather than milestone completion.

## 0.957 Evaluation completion boundary

0.957 remains in progress. Slices 1–5 are verified. Slice 4 provides the model-assisted judging boundary through injected `IAiEvaluationJudge` and remains explicitly non-authoritative. Slice 5 provides bounded provider-neutral aggregation and alternative-target comparison over completed evaluation evidence. Slice 6 currently provides repeated case-target regression execution over host-owned executors and reuses Slice 5 samples for aggregation/comparison.

Regression execution is measurement orchestration, not production routing. The suite does not choose providers or credentials, authorize actions, persist authoritative state, promote learning, or mutate cognitive state. Failed or canceled case-target executions remain visible as regression execution results but are excluded from evaluation aggregation unless a valid `AiEvaluationSample` was produced before cancellation.

## Storage implications

The configuration/storage evolution defined by `docs/roadmap/38-configuration-storage-and-portability.md` remains a cross-cutting foundation before 0.96. It must directly support authoritative resource relationships, scoped ownership, capability state, and portable configuration rather than creating a second resource configuration model.

The unified policy set is a canonical HAgent-owned configuration record exposed through `IAiStore`. File storage persists it with the main settings document; SQL Server and MySQL persist it in `HAgentPolicies`, and their HAgent database bootstrap paths create that table. The default runtime loads the current persisted policy asynchronously at execution creation when no explicitly injected policy engine is supplied, then captures the effective policy in the execution snapshot.

Agent profile resource capability defaults are part of the canonical `AiAgent` configuration and persist through the normal agent storage path. Runtime capability overrides remain transient and resolve above profile defaults into `AgentExecutionSnapshot.EffectiveResourceCapabilities`.

Learning-promotion decisions are represented as normal `AiPolicyDecision` outcomes on `learning.promote`, with typed candidate metadata carried as bounded policy attributes. Mature candidate/target repositories and broader human intervention remain ordered roadmap work.

Provider API keys are persisted with provider configuration and encrypted at rest. There is no separate provider secret-reference or vault architecture. Shared SQL Server/MySQL configuration can therefore be used by multiple authorized HAgent processes/machines. Configuration export/import remains planned as a versioned portable representation with optional encrypted credential inclusion.

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

## D-006 — Model-assisted evaluation uses an injected judge boundary

**Status:** Active

Model-assisted evaluation must not make `HAgent.Core` a provider router or a model-specific grading client. `IAiEvaluationJudge` is the provider-neutral boundary for model judging, and `AiModelAssistedEvaluationEvaluator` is the `IAiEvaluator` adapter that maps a bounded judge rating into `AiEvaluation` evidence.

The evaluator owns detached per-call request snapshots, validation, cancellation checks, evidence ownership, and evaluator/judge provenance. Provider transport, credentials, model selection, retries, and host-specific evidence resolution remain inside the injected judge implementation or its owning subsystem.

Model-assisted output is explicitly non-authoritative. It can produce `Passed`, `Failed`, `Inconclusive`, or `NeedsReview`, but it never grants authorization, changes configuration, promotes learning, or mutates authoritative cognitive state by itself.

## D-007 — Evaluation aggregation is measurement-only

**Status:** Active

Evaluation aggregation and alternative-target comparison remain provider-neutral measurement operations over bounded evaluation evidence. They may group samples, compute outcome/quality/operational metrics, compute left-minus-right deltas, and identify a strictly preferred variant according to an explicit metric direction.

A `PreferredVariantId` is comparative evidence, not a routing instruction, authorization decision, policy decision, configuration change, learning promotion, or authoritative cognitive mutation. Aggregation does not invoke providers, resolve credentials, persist authoritative state, or execute regression suites. Later regression-suite orchestration must consume these contracts rather than introduce a parallel metric/result model.

## D-008 — Regression suites orchestrate host-owned execution and evaluation only

**Status:** Active

Evaluation regression suites are provider-neutral orchestration contracts. A suite owns bounded case definitions, alternative target descriptors, and a concurrency limit; an injected `IAiEvaluationRegressionExecutor` owns how a specific case is executed for a specific target and returns the existing `AiEvaluationSample` evidence contract.

The regression runner may repeat every case across every configured target, bound concurrent executions, isolate executor failures as case results, propagate cancellation, discard late results after cancellation, and deterministically hand successful samples to the existing aggregation/comparison layer. It does not route production execution, authorize actions, select providers or credentials, persist authoritative state, or introduce a second metric/result model.

Failure and cancellation are kept distinct from evaluation outcomes: an execution that cannot produce a valid evaluation is recorded as a regression case failure/cancellation and does not become a fabricated `AiEvaluation`. Regression results remain measurement evidence only.

## D-009 — Resource governance composes canonical ownership, capability, and policy

**Status:** Active

Mature resource admission must compose the existing canonical `AgentResourceOwnership`, effective `AiResourceCapabilitySnapshot`, and unified `IAiPolicyEngine` rather than creating subsystem-specific ownership or authorization models. Non-global resource requests require the authoritative resource owner ID and must match the identity-derived owner; capability state is evaluated from an immutable profile/runtime snapshot; policy remains the final authorization boundary.

The generic `AiResourceGovernanceEvaluator` is reusable across Skills, Knowledge/Wiki, Memory families/types, and future resource types. Disabled capability, ownership mismatch, policy denial, approval requirement, deferral, and non-applicable policy all result in non-admitted resource access. Effective capability source (`Default`, `Profile`, `RuntimeOverride`) is provenance for configuration diagnostics, not authority.

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

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 Evaluation and Quality Measurement is complete through Slice 6. The provider-neutral evaluation architecture now covers evaluation contracts, deterministic evaluators, human/application ratings, model-assisted judging, aggregation/comparison, and repeated regression-suite execution.

### Verification

- Slice 1 — provider-neutral evaluation contracts and evaluator boundary — verified.
- Slice 2 — deterministic evaluators and evaluation evidence — verified; user reported 109/109 tests and both supported Examples succeeded.
- Slice 3 — human/application ratings and labeled evaluation evidence — verified; user reported 115/115 tests and both supported Examples succeeded.
- Slice 4 — model-assisted evaluator/judge boundary — verified; user reported 123/123 tests and both supported Examples succeeded.
- Slice 5 — aggregation and alternative-target comparison — verified; user reported 130/130 tests and both supported Examples succeeded.
- Slice 6 — regression suites and repeated target execution — verified by user on 2026-09-09: **139/139 HAgent.Tests passed**, `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` succeeded on **.NET Framework 4.8.1 and .NET 9**, with 3 cases × 2 targets, bounded concurrency of 2, isolated failures, late cancellation-result discard, and successful-sample-only aggregation handoff.

Requirement 9 (repeated test cases/regression suites) and requirement 10 (aggregate metrics) are therefore verified. Requirement 11 is verified through the matching public Examples for evaluation creation/ratings, aggregation/comparison, failure handling, and regression execution.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource foundations established in 0.8 and the identity, policy, context, observability, and evaluation boundaries already completed. It must not introduce a parallel resource model.

### Slice 1 — Mature resource capability governance foundation

**Objective:** establish one provider-neutral governance boundary for effective resource capability state and governed resource access decisions, while keeping authoritative resource definitions separate from runtime snapshots.

### Required architecture to inspect before coding

- `docs/architecture/05-identity.md` — canonical ownership, scope, tenancy, principal, and resource identity semantics.
- `docs/architecture/16-cognitive-runtime.md` — persistent cognition/resource relationship and learning boundaries.
- `docs/architecture/23-evaluation-quality.md` — evaluation evidence boundary consumed by later learning governance.
- Existing resource capability contracts and runtime execution snapshot implementation.
- Existing policy engine/resource-policy integration and learning-promotion policy decision implementation.
- Existing Skill, Knowledge/Wiki, Memory, and Learning candidate foundation models from Phase 0.8.

### Slice 1 boundary

The first implementation slice should provide reusable effective resource capability/admission semantics for Skills, Knowledge/Wiki, Memory families/types, and future resource types without hard-coding resource-specific authorization into the agent runtime.

It should address inheritance/profile defaults, runtime tri-state overrides, explicit ownership/scope context, fail-closed admission where required, immutable execution snapshots, deterministic decisions, and provider-neutral public contracts. Persistent resource stores, full management UI, candidate promotion workflows, retention governance, and broader learning lifecycle remain later slices of 0.9575 unless they are required as part of this boundary.

**Example to run:** `HAgent.Example → Cognition → Resource Governance` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/ResourceGovernanceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

No design blocker is known. Start by inspecting the authoritative identity/cognitive/resource architecture and existing capability/policy/snapshot implementation. Do not create duplicate capability, resource identity, ownership, or policy models merely because the mature governance phase is not yet complete.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

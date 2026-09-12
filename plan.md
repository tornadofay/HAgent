# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9576 Learned Resource Reliability + Adaptation
- **Status:** Slice 1 in progress — Applicability and validity
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Establish a provider-neutral applicability/validity boundary for learned resource versions before reliability, staleness, forgetting, or runtime adaptation work.

## 0.9575 checkpoint closed

The 0.9575 Knowledge, Skills, Memory Governance + Learning phase is complete through the Learning Review and Authoritative Resource Inventory management increments.

User verification completed on 2026-09-12:

- `HAgent.Example → Authoritative Resource Inventory` succeeded on .NET Framework 4.8.1.
- `HAgent.Example → Authoritative Resource Inventory` succeeded on .NET 9.
- The Example verified unified Memory / Knowledge / Skill inventory projection, real `InMemoryMemoryStore` adaptation, provider-neutral Knowledge enumeration, authoritative-only filtering, deterministic ordering/paging, filtering, and readable resource details.

The remaining 0.9575 work that intentionally stays deferred includes storage-specific Skill enumeration and later governed editing/lifecycle operations. Reliability/adaptation is now the active 0.9576 phase.

## Current Slice 1 boundary

`0.9576 Slice 1 — Applicability and validity` establishes:

- `AiApplicabilityOutcome`: `Applicable`, `NotApplicable`, `Uncertain`, `Invalidated`;
- bounded provider-neutral applicability conditions and preconditions;
- bounded applicability evidence references and per-condition evidence availability;
- resource identity including resource type, resource ID, version, and scope;
- explicit invalidation state without mutating published resource versions;
- deterministic evaluation through `IAiApplicabilityEvaluator` and `AiDeterministicApplicabilityEvaluator`;
- scope-aware applicability that remains separate from authorization/capability state;
- missing required evidence produces `Uncertain`, never `Applicable`;
- applicability decisions retain bounded condition results and evidence references for later observability/audit integration;
- no model invocation, embeddings, vector database, or GPU dependency.

## Files and verification

- Architecture: `docs/architecture/97-learned-resource-applicability.md`.
- Tests: `tests/HAgent.Tests/LearnedResourceApplicabilityTests.cs` / `LearnedResourceApplicabilityTests`.
- Example: `src/HAgent.Example/MainForm.LearnedResourceApplicability.cs` / `Learned Resource Applicability`.

**Example to run:** `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceApplicabilityTests.cs` focused first, then the full `HAgent.Tests` regression suite.

## Do not advance

Do not advance to 0.9576 reliability evidence, staleness/contradiction, forgetting/archival, or runtime integration until this Slice 1 Example succeeds on both supported targets and the focused/full tests remain green.

The durable candidate boundary is now closed. It remains separate from authoritative resource publication.

### Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified Memory promotion, creation of a new published Knowledge version, creation of a new immutable Skill version, fresh unified promotion authorization, publication-before-lifecycle transition, provenance evidence, and no mutation of existing published Knowledge or Skill versions.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

The authoritative promotion boundary is now closed.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

The execution-integration boundary is now closed.

### Slice 12 — Learning Review management UI — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 real WinForms configuration flow succeeded.
- .NET 9 real WinForms configuration flow succeeded.
- Durable candidate-store reopen was verified on both targets.
- Manual workflow verified: `Learning Review Seed` → `Configuration → Learning Review` → inspect/filter → Approve → Promote → `Learning Review Verify`.
- Final candidate status was `Promoted` with lifecycle revision `3` on both targets.
- Reviewer identity evidence persisted.
- Review/promotion authorization evidence persisted with outcome `Allow`.
- The management surface reused the existing provider-neutral `AiLearningPromotionService`; WinForms did not directly publish authoritative resources.

The complete Learning Review management boundary is now closed.

### 0.9575 Resource Inventory + Detail Inspection — VERIFIED

Verified by user on 2026-09-12 on both .NET Framework 4.8.1 and .NET 9:

- `HAgent.Example → Authoritative Resource Inventory` succeeded on both targets.
- Unified Memory / Knowledge / Skill inventory projection succeeded.
- Real `InMemoryMemoryStore` → Memory inventory source projection succeeded.
- Provider-neutral Knowledge source → inventory projection succeeded.
- Authoritative-only, resource-type, text, lifecycle, version, updated-time, owner, deterministic ordering, and bounded paging behavior succeeded.
- Readable Memory / Knowledge / Skill detail inspection succeeded.
- Skill storage-specific enumeration remains deferred because its current contract exposes lookup but not generic authoritative enumeration.

Architecture: `docs/architecture/95-authoritative-resource-inventory.md`, `docs/architecture/96-resource-detail-inspection.md`.

The 0.9575 management inventory increment is closed.

## Current phase — 0.9576 Learned Resource Reliability + Adaptation

### Slice 1 — Applicability and validity — IN PROGRESS

Implemented on `master`:

- `AiApplicabilityOutcome`: `Applicable`, `NotApplicable`, `Uncertain`, `Invalidated`;
- bounded provider-neutral preconditions and deterministic condition operators;
- explicit resource version/scope identity and invalidation state;
- bounded host-supplied applicability facts and evidence references;
- `IAiApplicabilityEvaluator` and deterministic `AiDeterministicApplicabilityEvaluator`;
- missing evidence yields `Uncertain`, never `Applicable`;
- applicability remains separate from authorization/capability state;
- applicability decisions retain bounded condition results and evidence references for later observability/audit integration;
- focused tests and dedicated Example verification host.

Architecture: `docs/architecture/97-learned-resource-applicability.md`.

**Tests to run:** `HAgent.Tests → LearnedResourceApplicabilityTests.cs` focused, then the full `HAgent.Tests` suite.

**Example to run:** `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9.

## Remaining 0.9576 work

- reliability evidence and validated outcome feedback;
- staleness, contradiction, drift, and revalidation;
- quarantine, retirement, archival, and forgetting;
- governed replacement candidates without in-place mutation;
- runtime integration and end-to-end verification.

## Architectural Decisions

This file contains only durable decisions needed to preserve project direction across sessions. It is not a conversation log.

## D-001 — Complete known architecture before implementation

**Status:** Active

Substantial features must be implemented against the complete intended architecture that can reasonably be derived from the repository, roadmap, architecture documents, and requirements. Do not deliberately create simplified temporary implementations when the required design is already understood.

Testing is primarily for verification, defect discovery, and genuinely unforeseen interactions. It must not be used as a substitute for architectural analysis that should have happened before implementation.

## D-002 — Repository documents are persistent project memory

**Status:** Active

Small purpose-specific Markdown documents preserve durable project state so development can resume without relying on conversation history. They contain compressed conclusions, current state, active work, and durable decisions—not transcripts or unrestricted reasoning.

## D-003 — No duplicate sources of truth

**Status:** Active

Each durable fact should have one authoritative source. Other documents should reference that source rather than copying the same architectural decision. Generated root documents remain generated views.

## D-004 — Active work is state, not history

**Status:** Active

Current unfinished work is represented by the compact active-work document. When work advances, update the current state instead of appending a chronological diary. Completed work is removed from active state once it is reflected in the appropriate authoritative project document.

## D-005 — Unforeseen discoveries may refine architecture

**Status:** Active

Testing and implementation may reveal requirements or interactions that could not reasonably have been known beforehand. Such discoveries should produce an explicit architectural decision or update to the authoritative source rather than an undocumented workaround.

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

## D-010 — Skills are versioned descriptive resources with governed execution snapshots

**Status:** Active

`AiSkillDefinition` is the canonical provider-neutral representation of one reusable Skill version. Its stable identity is `SkillId + Version`, with explicit `AgentResourceScope`, owner, lifecycle state, provenance, bounded input/output contracts, preconditions, ordered procedure steps, required Knowledge/Tool dependencies, constraints, metadata, and relationships.

`AiSkillReference` identifies a reusable definition by Skill ID plus explicit scope/owner and an optional version pin. `AiSkillSet` is a bounded collection of these references and may be reused by multiple agent profiles without duplicating Skill definitions. Agent profiles own the references through `AiAgent.Skills`; the references do not themselves grant authorization.

Skill access must reuse `AiResourceGovernanceEvaluator`. The canonical `skill.invoke` resource boundary is evaluated before the definition source is accessed, using the reference's explicit scope/owner, identity, profile/runtime/execution context, and effective capability state. There is no parallel Skill authorization engine.

`IAiSkillDefinitionSource` is a provider/storage-neutral read boundary. `AiGovernedSkillResolver` composes that source with the canonical governance evaluator and admits only definitions whose ID, scope, owner, optional pinned version, and published lifecycle state match the requested reference.

Executable handlers are deliberately outside persisted Skill definitions and Skill sets. No provider SDK object, delegate, callback, or executable handler is serializable as Skill configuration. `AiSkillExecutionSnapshot` deep-clones admitted bindings, and `AgentExecutionSnapshot.Skills` owns that snapshot for the execution lifetime so later profile/source mutations cannot change the bound Skill version/state.

Slice 3 does not introduce a Skill-specific persistence backend, execution handler registry, learning-promotion workflow, management UI, or context-budget integration; those remain later roadmap boundaries.

## D-011 — Memory families use one extensible MemoryEntry contract

**Status:** Active

Memory remains one provider-neutral `MemoryEntry` persistence contract. The entry identifies its broad semantic family with `AiMemoryFamily` (`Working`, `Episodic`, `Semantic`, `Procedural`, or `Custom`) and its stable concrete type with a bounded `TypeId` namespace. Built-in families use reserved family prefixes; custom types use application-specific namespaces. Future memory types therefore do not require a new persisted class or provider-specific model.

`AiMemoryProvenance` carries source kind, source identity, optional source URI/creator information, originating execution/runtime IDs, evidence, and bounded confidence. Provenance is descriptive evidence only; model-generated memory is not automatically authoritative.

`MemoryEntry.ExpiresAt` is optional metadata and `IsExpired(...)` is a deterministic point-in-time helper. Retention enforcement remains a later governance concern. Historical records may have `OccurredAt` before `CreatedAt`.

`MemoryEntry.Validate()` owns structural bounds and family/type consistency but does not authorize access. `MemoryEntry.Clone()` deep-copies mutable metadata and provenance. Existing memory stores continue to use `MemoryEntry` as their single representation; no parallel memory repository is introduced by this slice.

## D-012 — Memory governance composes generic capability state with one provider-neutral policy boundary

**Status:** Active

Memory family/type access must reuse the existing `AiResourceCapabilityPolicy` and immutable `AiResourceCapabilitySnapshot`. Canonical Memory capability resource types are `memory`, `memory.family`, and `memory.type`; no Memory-specific authorization system is introduced.

`MemoryQuery` owns explicit family/type and expiration filters plus the caller's requested result bound. `AiMemoryGovernancePolicy` owns deterministic global/family/type retrieval limits and retention caps. Exact TypeId rules take precedence over family rules, and policy limits are bounded to 1000.

`AiMemoryGovernanceEvaluator` performs capability checks, while `AiGovernedMemoryStore` composes capability and policy enforcement around the existing `IMemoryStore` boundary. The decorator clones writes, applies the maximum retention expiration without extending a shorter explicit expiration, filters expired/unauthorized records during recall, and returns detached records. Physical storage remains the existing File/SQL Server/MySQL/InMemory implementations.

Memory governance is therefore separated into capability authorization, retrieval/retention policy, and storage. Runtime execution snapshot binding and management UI remain later integration slices.

## D-013 — Learning policy and typed candidates compose the existing lifecycle

**Status:** Active

`AiLearningCandidate` remains the single canonical learning lifecycle. `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` are typed payload contracts that compose an existing `AiLearningCandidate`; they do not define parallel status transitions or promotion state.

`AiLearningPolicy` is a deterministic, provider-neutral candidate admission/evaluation contract. Its rules may constrain candidate type/scope, minimum confidence, evidence, provenance, contradiction state, evaluation state, retention class, and the required promotion-authorization classification. An unmatched rule denies the candidate by default.

The learning policy is not a replacement for authorization. External promotion remains behind the existing `IAiPolicyEngine` / `AiLearningPromotionPolicy` boundary. Model-generated content may propose a candidate but cannot make Knowledge or Skills authoritative by itself.

Knowledge and Skill candidate payloads must remain non-authoritative (`Draft`). Candidate payloads are cloned at construction so caller mutation does not alter the captured proposal. Source execution/runtime/profile identity remains on the canonical candidate lifecycle object.

## D-014 — Learning candidate persistence remains one lifecycle envelope with revision-checked review

**Status:** Active

Slice 9 persists the existing `AiLearningCandidate` lifecycle through one provider-neutral `AiLearningCandidateRecord` and `IAiLearningCandidateStore`. Persistence stores the canonical lifecycle status/revision, typed payload, provenance/evaluation metadata, retention information, Slice 8 policy/authorization provenance, and review evidence; it does not create a second candidate state machine or authoritative resource model.

Retention is policy metadata over the durable candidate record. Expiry may remove a candidate from normal reads and be purged deterministically, but an unmapped retention class does not invent a default expiry. Review remains an authorization-sensitive host operation using the existing unified `IAiPolicyEngine` with operation `learning.review`; reviewer identity is explicit and never substituted by an anonymous identity.

Review updates use an expected lifecycle revision so stale reviewers cannot overwrite newer candidate state. Store implementations return detached records and must preserve the same contracts across persistence backends. Authoritative Memory/Knowledge/Skill promotion remains the later Slice 10 boundary.

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

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CLOSED

Phase 0.9575 is closed through Learning Review and the Authoritative Resource Inventory + Detail Inspection management increment.

- Learning Review was user-verified on both supported targets.
- Authoritative Resource Inventory was user-verified on 2026-09-12 on .NET Framework 4.8.1 and .NET 9.
- The inventory Example verified real `InMemoryMemoryStore` projection, provider-neutral Knowledge enumeration, authoritative-only filtering, deterministic ordering/paging, filtering, and readable Memory/Knowledge/Skill details.
- Skill storage enumeration remains deferred until its existing source contract gains a supported authoritative enumeration boundary.
- Governed resource editing/version-creation/lifecycle workflows remain later management work.

## 0.9576 Learned Resource Reliability + Adaptation — CURRENT

### Slice 1 — Applicability and validity — CURRENT

This slice establishes the provider-neutral decision boundary that determines whether a promoted resource version is applicable to a bounded context, or whether evidence is insufficient or the resource is invalidated.

Implemented:

- `AiApplicabilityOutcome`: `Applicable`, `NotApplicable`, `Uncertain`, `Invalidated`;
- `AiApplicabilityTarget` with resource type/ID/version/scope and explicit invalidation state;
- `AiApplicabilityCondition` with bounded `Exists`, `Equals`, `NotEquals`, and `OneOf` operators;
- `AiApplicabilityContext` with bounded host-supplied facts and optional scope;
- `AiApplicabilityEvidenceReference` plus condition-level evidence references;
- `AiApplicabilityDecision` retaining outcome, version identity, bounded condition results, evidence references, reason, and evaluation timestamp;
- provider-neutral `IAiApplicabilityEvaluator`;
- deterministic `AiDeterministicApplicabilityEvaluator` that evaluates available deterministic evidence before any model reasoning;
- missing deterministic evidence yields `Uncertain` and never proves applicability;
- applicability remains independent from authorization/capability policy;
- invalidation is terminal for this evaluator and does not mutate the published resource;
- focused tests and a dedicated manual Example.

Architecture: `docs/architecture/97-learned-resource-applicability.md`.

**Example to run:** `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `HAgent.Tests → LearnedResourceApplicabilityTests.cs` focused first, then the full `HAgent.Tests` regression suite.

## Run rule

Complete Slice 1 and record user verification before starting 0.9576 Slice 2 reliability evidence. Do not combine numbered 0.9576 slices in one run.

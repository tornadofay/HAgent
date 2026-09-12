# HAgent Development Plan

> This file is generated from smaller source documents. Do not edit it directly.
> Source directory: `docs/plan`.

## Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 12 in progress — Management UI
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Add production WinForms administration surfaces using the existing configuration shell and provider-neutral learning/resource contracts.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

0.9575 Slice 7 — Learning Policy + Typed Candidates — **verified by user** 2026-09-11: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

0.9575 Slice 8 — Canonical Learning Lifecycle Gate — **verified by user** 2026-09-11: `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was **194/194 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 9 — Learning Candidate Persistence, Retention + Review — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified durable recovery, PendingReview revision restoration, authorized review, revision 1 → 2, reviewer/policy evidence, and absence of authoritative publication.
- Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified Memory promotion, new published Knowledge/Skill versions, fresh unified authorization, publication-before-lifecycle transition, provenance evidence, and immutable version behavior.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 11 — Context, Instruction, Runtime, and Observability Integration — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Execution Integration` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified learned instruction, policy/capability-gated context, bounded snapshots, authoritative runtime observations, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

## Current Slice 12 boundary

The first management-UI increment is the Learning Review surface.

- `LearningReviewPage` lives under `src/HAgent.WinForms/UI/Configuration/Learning/` and follows the shared header/action-bar/content layout.
- Candidate listing is limited to durable, non-expired `PendingReview` records and exposes bounded metadata only.
- Reviewer user identity is explicit; tenant/workspace are optional structured identity fields.
- Approve and Reject use `AiLearningCandidateReviewService`, which re-evaluates `learning.review` through `IAiPolicyEngine` and persists reviewer/policy evidence through the existing candidate store.
- UI actions do not publish authoritative Memory, Knowledge, or Skill resources.
- `ConfigurationContext` exposes the durable File candidate-store adapter used by the current reference WinForms composition.

**Architecture:** `docs/architecture/93-learning-review-management-ui.md`.

**Example to run:** `HAgent.Example → Configuration → Learning Review` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** full `HAgent.Tests` regression suite; WinForms configuration UI is primarily verified through the supported Example host.

## Do not advance

Do not advance beyond this management-UI increment until the Learning Review Example succeeds on both supported targets and the full test suite remains green.

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

### Current Slice 12 management increment — Authoritative Resource Inventory + Detail Inspection

This increment remains inside 0.9575 and establishes the shared inventory foundation, a scalable master-detail WinForms inventory surface, and provider-neutral read-only detail inspection.

### Implemented

- `IAiResourceInventorySource` for host/provider/storage-specific enumeration;
- `IAiResourceInventory` for the shared management-facing read boundary;
- bounded `AiResourceInventoryQuery` and `AiResourceInventoryItem` contracts;
- lifecycle, exact-version, updated-time, owner/agent, scope, type, search, and authoritative-only query filters;
- deterministic aggregation, filtering, authoritative-only selection, logical-resource deduplication, highest-version selection, ordering, and `SkipResults` + `MaxResults` paging;
- extensible string resource types so future resource families do not require central Agent model changes;
- provider-neutral `AiMemoryResourceInventorySource` adapting the existing `IMemoryStore.SearchAsync` contract without adding storage-specific enumeration logic;
- no SQL Server/MySQL enumeration implementation for Knowledge/Wiki/Skill, whose current contracts do not expose generic authoritative enumeration;
- focused unit coverage in `HAgent.Tests/ResourceInventoryTests.cs` including filter and paging contracts;
- focused Memory-source coverage in `HAgent.Tests/MemoryResourceInventorySourceTests.cs`;
- dedicated Example scenario `HAgent.Example → Authoritative Resource Inventory` now exercises a real provider-neutral `InMemoryMemoryStore` through the Memory inventory adapter plus deterministic Knowledge/Skill projections;
- WinForms `Configuration → Authoritative Resources` page consuming the inventory and detail contracts;
- persistent master resource list that remains visible while inspecting a selected resource;
- user-resizable SplitContainer starting near a 55/45 list/detail balance rather than a fixed detail width;
- filter/action region for Search, Resource type, Agent/Owner, Scope, Lifecycle, Version, Updated window, Authoritative-only, Apply/Reset/Refresh, and bounded page navigation;
- paging controls placed beneath the master list and omitted when the result set fits on one page;
- Overview and Content tabs confined to the selected-resource detail pane rather than replacing the master list;
- stale asynchronous detail-result protection so an earlier selection cannot overwrite the current selection;
- edit/delete/publish/archive/retire operations remain outside the read-only detail boundary.

### Remaining work

- connect real Knowledge/Wiki and Skill authoritative sources where their existing contracts can support inventory and detail enumeration without inventing provider-specific behavior;
- expose effective agent-resource visibility without duplicating authoritative resource models;
- extend the management surface with governed resource-specific editing/version-creation workflows;
- add appropriate governed lifecycle operations rather than unconditional CRUD/delete behavior;
- consider provider-side paging/virtualization optimizations only when a concrete storage source requires them;
- keep reliability/adaptation separate for 0.9576.

User verification already reported for this management slice: full `HAgent.Tests` **218/218 passed, 0 failed, 0 skipped** on .NET 9, and the WinForms master-detail resource management surface was visually reviewed as correct after the paging-control polish. The new Memory inventory source increment is not yet locally verified by the user.

Architecture: `docs/architecture/93-learning-review-management-ui.md`, `docs/architecture/94-learning-review-candidate-details.md`, `docs/architecture/95-authoritative-resource-inventory.md`, and `docs/architecture/96-resource-detail-inspection.md`.

**Tests to run:** `HAgent.Tests → MemoryResourceInventorySourceTests.cs` (focused), then the full `HAgent.Tests` suite at the management-slice checkpoint.

**Example to run:** `HAgent.Example → Authoritative Resource Inventory` on .NET Framework 4.8.1 and .NET 9; then open `Configuration → Authoritative Resources` on both targets, resize the master/detail splitter, exercise the filters and page navigation, and select Memory, Knowledge, and Skill resources to verify the aligned Overview and readable Content views.

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

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slices 1–10 — VERIFIED

Slices 1–6 were verified on 2026-09-09. Slice 7, Slice 8, Slice 9, and Slice 10 were verified by the user on 2026-09-11. Their recorded test counts are 146, 153, 158, 167, 175, 181, 187, 194, 200, and 205 respectively; all required Examples succeeded on both supported frameworks.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — VERIFIED

Verified by user on 2026-09-11.

- `HAgent.Example → Cognition → Learning → Learning Execution Integration` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

Architecture: `docs/architecture/90-learning-execution-integration.md`.

Slice 11 is closed.

### Slice 12 — Management UI — CURRENT

The management surface is being added under the existing WinForms configuration architecture. `AISettingsForm` remains a composition shell; feature behavior lives under `src/HAgent.WinForms/UI/Configuration/`.

Current increment:

- `Learning Review` configuration page for durable `PendingReview` candidates;
- explicit reviewer identity fields;
- Approve/Reject actions routed through `AiLearningCandidateReviewService` and the unified policy engine;
- bounded candidate metadata projection without copying candidate payload into the list;
- durable candidate store dependency exposed through `ConfigurationContext`;
- no authoritative Memory/Knowledge/Skill publication from the UI.

Architecture: `docs/architecture/93-learning-review-management-ui.md`.

**Example to run:** `HAgent.Example → Configuration → Learning Review`.

**Tests to run:** full `HAgent.Tests` remains the phase regression gate; Slice 12 UI verification is primarily the supported WinForms Example on .NET Framework 4.8.1 and .NET 9.

## Run rule

Complete the current implementation increment and record its verification before moving to the next management-UI increment. Do not combine multiple numbered slices in one run.

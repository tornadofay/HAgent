# Phase 0.9575 — Knowledge, Skills, Memory Governance + Learning

## Status

**In progress — Slice 8 implementation.**

Slices 1–7 are verified. This roadmap has been normalized against the current implementation so historical checklist entries that already leaked into the code are no longer treated as automatically missing work.

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

Verification: user reported 158/158 tests and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 4 — Memory family/type and provenance — VERIFIED 2026-09-09

The existing MemoryEntry contract now carries canonical family/type, provenance, expiration metadata, clone/validation behavior, and aligned File/SQL Server/MySQL persistence.

Verification: user reported 167/167 tests and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 5 — Memory governance and retention — VERIFIED 2026-09-09

Memory governance reuses the generic resource capability model, adds deterministic bounded retrieval/expiration filtering, per-family/type retention caps, and the governed memory-store decorator.

Verification: user reported 175/175 tests and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 6 — Learning Mode — VERIFIED 2026-09-09

`AiLearningMode` is provider-neutral and distinct from resource capability policy. Persistent profile state, runtime-only override, and immutable execution-snapshot capture are established.

Verification: user reported 181/181 tests and the required Examples succeeded on .NET Framework 4.8.1 and .NET 9.

### Slice 7 — Learning Policy + Typed Candidates — VERIFIED 2026-09-11

One provider-neutral Learning Policy contract and typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` payloads now compose the single canonical `AiLearningCandidate` lifecycle.

Verification: user reported 187/187 tests, 0 failed, 0 skipped on .NET 9; the `HAgent.Example → Policy → Learning Policy` Example succeeded on .NET Framework 4.8.1 and .NET 9.

## Remainder audit: historical checklist normalization

The older 0.9575 checklist mixed genuine missing work with requirements that had already been implemented elsewhere. The following distinctions are now authoritative.

### Already implemented / leaked into the current architecture

- Canonical resource scopes are `Global`, `Tenant`, `User`, `Workspace`, `Agent`, `Runtime`, and `Execution`. Any historical wording that names `Domain` is obsolete; `Workspace` is the canonical scope.
- `AiAgent.ResourceCapabilities` already provides persistent profile capability defaults, and `AiResourceCapabilityPolicy` supports both resource-type and exact-resource entries with `Inherit`, `Enabled`, and `Disabled` states.
- Effective profile/runtime capability resolution already exists and is captured into `AgentExecutionSnapshot.EffectiveResourceCapabilities`.
- Runtime overrides remain transient and do not write back into profile configuration.
- Knowledge retrieval requests already impose explicit bounded query, result, character, chunk, metadata, and filter limits.
- Knowledge and Skill resources already have explicit scope/owner identity and governed resolution; reusable shared resources are represented by authoritative resource identity rather than private copies.
- Published Knowledge and published Skills are already rejected as typed learning candidate payloads.
- Runtime identity, correlation, cancellation, timeout, stale-result protection, and independent runtime-instance isolation already exist as runtime foundations.

These items remain subject to the final phase verification matrix, but they are not to be reimplemented as duplicate mechanisms.

### Partially implemented; integration still outstanding

- Shared reusable Knowledge exists at the resource-contract/governance level, but HAgent-owned persistence, management, relationships, and end-to-end reusable-resource administration remain incomplete.
- Retrieval is bounded at the contract level, but integration with context budgets and canonical context assembly is still outstanding.
- Learning policy already carries retention/evaluation/authorization classifications, but candidate retention/expiry persistence and lifecycle audit storage remain outstanding.
- Typed candidates reject authoritative payloads, but there is not yet a resource-specific promotion service that creates new authoritative versions.
- Runtime snapshots capture capability/learning configuration, but learning outcomes are not yet captured into a complete runtime-to-learning pipeline.

### Genuinely outstanding 0.9575 work

- Canonical candidate lifecycle admission/review/promotion gate.
- Candidate persistence, retention, expiry, rejection/promotion provenance, and durable review state.
- Authoritative Memory promotion.
- Knowledge promotion that creates a new authoritative version rather than mutating a published record.
- Skill promotion that creates a new immutable version rather than mutating a published definition.
- Promotion audit/provenance/evaluation records.
- Governed learning-resource integration into the canonical context/instruction pipeline.
- Runtime capture of execution outcomes/observations as learning input.
- Resource access/promotion observability through the same policy and audit boundaries as other runtime actions.
- Learning Review, Knowledge/Wiki, Skill, and Agent Configuration management UI.
- Persistence and restart verification for the mature learning/resource layer.

## Slice 8 — Canonical Learning Lifecycle Gate — IN PROGRESS

### Purpose

Close the missing boundary between typed candidate validation and later authoritative promotion.

### Scope

- Deterministic typed-candidate validation.
- `AiLearningPolicy` evaluation.
- `AiLearningMode` semantics.
- Existing `IAiPolicyEngine` promotion authorization for automatic paths.
- Explicit routing to `Rejected`, `PendingReview`, or `Approved`.
- Candidate-identity/state checks when applying lifecycle decisions.
- No model call is required.
- Approval is not publication.

### Verification

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Lifecycle` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `tests/HAgent.Tests/LearningLifecycleTests.cs` during the checkpoint.

### Architecture

See `docs/architecture/87-learning-lifecycle.md`.

## Slice 9 — Candidate persistence, retention, and review

The candidate itself becomes durable without creating a second candidate model.

Scope:

- provider-neutral candidate store boundary;
- durable lifecycle status/revision;
- retention/expiry policy;
- rejected/pending/approved provenance retention according to policy;
- durable evaluation outcomes;
- Learning Review read/update workflow;
- restart/recovery semantics;
- authorization/audit evidence for review actions.

No authoritative Knowledge/Skill mutation is performed directly by the candidate store.

## Slice 10 — Authoritative promotion and version-safe resource creation

Convert an approved typed candidate into authoritative resource state through one canonical promotion boundary.

Scope:

- Memory promotion using its existing memory ownership/store contracts;
- Knowledge promotion creates a new authoritative version and never silently edits a published version;
- Skill promotion creates a new immutable version and preserves the active version;
- promotion conflicts and stale candidates are rejected deterministically;
- promotion provenance/source execution/runtime identity is preserved;
- unified policy authorization is required;
- promotion is auditable.

## Slice 11 — Context, instruction, runtime, and observability integration

Connect governed learned resources to real execution without moving authorization into prompts.

Scope:

- integrate Skills, Knowledge, Memory, and approved external learning content through the canonical context/instruction pipeline;
- carry provenance, scope, trust, and effective capability state in execution snapshots where exposed;
- prevent disabled/unauthorized/stale resources from becoming authoritative context;
- capture execution outcomes and observations as learning input;
- expose resource access and promotion through existing observability/audit/policy boundaries.

Context composition and authorization remain separate concerns.

## Slice 12 — Management UI

Add the production WinForms administration surface using existing HAgent conventions.

Scope:

- Learning Review with pending candidates, provenance/evidence, approval, rejection, and retention state;
- Knowledge/Wiki Manager with CRUD, relationships, version/status, provenance, and usage views;
- Skill Manager with CRUD, version/status, dependencies, relationships, and usage views;
- Agent Configuration effective resource/capability state;
- profile defaults and runtime `Inherit`/`Enabled`/`Disabled` overrides;
- Learning Mode and Learning Policy visibility;
- generic inventory for future resource types.

## Slice 13 — Phase completion verification

Close the phase only after the implementation has deterministic evidence for the real boundaries introduced by 0.9575.

Required evidence includes:

- both supported framework targets;
- lifecycle and review behavior;
- persistence/restart/recovery;
- resource version/snapshot isolation;
- candidate authorization and stale/conflict handling;
- independent runtime isolation;
- context integration;
- management UI behavior;
- observability/audit coverage;
- no model-output bypass of authoritative resource boundaries.

Risk-based verification remains mandatory: concurrency, cancellation/lifecycle, persistence/recovery, security/authorization, performance claims, and public API behavior receive tests appropriate to the claim rather than a blanket requirement that every feature use the same test type.

## Phase exit criterion

Knowledge, Skills, Memory, and Learning are first-class production V1 HAgent resources with explicit identity, scope, ownership, provenance, capability policy, runtime overrides, immutable execution snapshots, governed retrieval, typed learning candidates, durable lifecycle, version-safe authoritative promotion, context/runtime integration, management UI, and auditable policy boundaries.

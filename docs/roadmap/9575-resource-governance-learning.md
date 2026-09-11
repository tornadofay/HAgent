# Phase 0.9575 — Knowledge, Skills, Memory Governance + Learning

## Status

**In progress — Slice 11 implementation.**

Slices 1–10 are verified. This roadmap is normalized against the current implementation so historical checklist entries that already leaked into the code are not treated as automatically missing work.

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
- Retrieval is bounded at the contract level, but integration with context budgets and canonical context assembly is now established for governed learned-resource inputs; broader reusable-resource administration remains outstanding.
- Runtime snapshots capture capability/learning configuration, and Slice 11 now captures bounded execution observations as learning input; complete automatic observation-to-candidate analysis remains outstanding.

### Genuinely outstanding 0.9575 work

- Promotion audit/provenance/evaluation persistence beyond the structured promotion evidence produced by Slice 10.
- Broader governed learning-resource management/repository integration where authoritative Knowledge/Skill persistence is host/storage-owned.
- Learning Review, Knowledge/Wiki, Skill, and Agent Configuration management UI.
- Persistence and restart verification for the mature learning/resource layer beyond the candidate store and current in-memory observation reference store.

## Slice 9 — Candidate persistence, retention, and review — VERIFIED 2026-09-11

### Purpose

Make the existing canonical typed learning candidate durable without creating a second candidate lifecycle or authoritative resource model.

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
- explicit `AgentIdentityContext` and re-evaluation of `learning.promote` through the existing unified `IAiPolicyEngine`;
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

After the final test-contract correction, full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

Architecture: `docs/architecture/89-learning-authoritative-promotion.md`.

## Slice 11 — Context, instruction, runtime, and observability integration — CURRENT

### Purpose

Connect governed learned resources to real execution without moving authorization into prompts or creating a parallel execution/context/observability architecture.

### Scope

- prepare learned instruction sources through the existing `AgentExecutionRequest.InstructionSources` boundary;
- prepare learned context retrieval sources through the canonical policy/capability admission, ranking, and bounded context assembly pipeline;
- preserve execution-owned context snapshots and cloned instruction inputs;
- consume authoritative `IExecutionObservationSource` facts as bounded learning input without turning observations into candidates automatically;
- provide a bounded provider-neutral observation store reference implementation;
- preserve the separation between runtime authority, observability, Learning Policy, candidate lifecycle, and authoritative promotion.

### Verification

**Required Example:** `HAgent.Example → Cognition → Learning → Learning Execution Integration` on .NET Framework 4.8.1 and .NET 9.

**Required tests:** focused `tests/HAgent.Tests/LearningExecutionIntegrationTests.cs`; full `HAgent.Tests` at the Slice 11 checkpoint.

Architecture: `docs/architecture/90-learning-execution-integration.md`.

## Slice 12 — Management UI

Add the production WinForms administration surface using existing HAgent conventions.

## Slice 13 — Phase completion verification

Close the phase only after deterministic evidence exists for the real boundaries introduced by 0.9575, including both framework targets, promotion, persistence/recovery, resource version/snapshot isolation, authorization, runtime isolation, context integration, management UI, observability/audit coverage, and no model-output bypass of authoritative resource boundaries.

## Phase exit criterion

Knowledge, Skills, Memory, and Learning are first-class production V1 HAgent resources with explicit identity, scope, ownership, provenance, capability policy, runtime overrides, immutable execution snapshots, governed retrieval, typed learning candidates, durable lifecycle, version-safe authoritative promotion, context/runtime integration, management UI, and auditable policy boundaries.

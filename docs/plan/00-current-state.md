# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT.**

0.957 Evaluation and Quality Measurement is verified through Slice 6. User verification on 2026-09-09 recorded **139/139 HAgent.Tests passed** and the required evaluation Example scenarios succeeded on .NET Framework 4.8.1 and .NET 9.

## First-class resource architecture

The architecture is dependency-driven:

```text
0.8 Resource Foundations
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

Knowledge, Skills, Memory, and Learning remain distinct. Skills are reusable executable capabilities; Knowledge is reusable retrievable information; Memory is scoped experience/state; Learning is the governed transformation of experience into typed candidates and, where permitted, promoted authoritative resource state.

## 0.9575 verified slices

### Slice 1 — Resource capability governance

Verified on 2026-09-09. The generic admission boundary composes canonical ownership, effective capability state, and unified policy authorization. User reported 146/146 full tests and the required Example succeeded on both supported frameworks.

### Slice 2 — Knowledge/Wiki

Verified on 2026-09-09. Managed Knowledge/Wiki resources provide identity, scope/ownership, provenance, lifecycle/versioning, bounded metadata, relationships, chunk evidence, and provider/index-neutral governed retrieval. User reported 153/153 full tests and the required Example succeeded on both supported frameworks.

### Slice 3 — Skills

Verified by user on 2026-09-09.

- `.NET Framework 4.8.1` Example `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded.
- `.NET 9` Example `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded.
- Full `HAgent.Tests`: **158/158 passed, 0 failed, 0 skipped** on .NET 9.

The Skill contract provides reusable versioned definitions/references, explicit scope/ownership, lifecycle/provenance, bounded input/output contracts, preconditions, ordered procedure steps, Knowledge/Tool dependencies, constraints, relationships, governed resolution, and deep-cloned execution snapshots. Executable handlers remain runtime-owned and outside persisted definitions.

## Current 0.9575 Slice 4 — Memory family/type and provenance contract

Implementation is in progress and verification is pending.

The canonical `MemoryEntry` now carries:

- `AiMemoryFamily`: `Working`, `Episodic`, `Semantic`, `Procedural`, `Custom`.
- bounded stable `TypeId`, with reserved built-in namespaces and application-specific custom namespaces;
- `AiMemoryProvenance` containing bounded source/evidence/execution/runtime metadata and confidence 0..1;
- optional `ExpiresAt` plus deterministic `IsExpired(...)` evaluation;
- deep cloning and structural validation.

The existing in-memory and File stores validate/isolate records. SQL Server and MySQL preserve the new fields using the existing `HAgentMemoryEntries` representation and schema upgrade paths. Existing episodic/task memory creation is aligned with the episodic family/type model.

Architecture source: `docs/architecture/83-memory-contracts.md`.
Durable decision: D-011.
Focused tests: `tests/HAgent.Tests/MemoryFamilyTests.cs`.
Public Example: `HAgent.Example → Memory → Memory Families`.
Verification workflow: `.github/workflows/verify-phase-0-9575-slice-4.yml`.

The current Slice 4 boundary deliberately does not include memory authorization orchestration, retention-policy enforcement, profile/runtime capability UI, learning candidates, promotion, Knowledge/Skill management UI, or a second storage architecture.

## Storage implications

The configuration/storage evolution remains a cross-cutting foundation before 0.96. HAgent-owned resource state must retain canonical identity, scope, ownership, provenance, and version information without creating subsystem-specific ownership models.

SQL Server and MySQL memory persistence now use explicit family/type/provenance/expiration columns while retaining the existing memory table and versioned bootstrap migration approach. File and in-memory persistence use the same `MemoryEntry` contract. Broader mature-resource persistence for learning candidates and management remains later 0.9575 work.

### Deferred exclusions

The roadmap does not introduce a separate distributed-coordination subsystem or sophisticated external secret-management architecture. Distributed behavior is handled through existing storage/runtime contracts where required, while provider credentials use the existing fixed encryption/decryption mechanism.

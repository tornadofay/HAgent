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

### Slice 4 — Memory family/type and provenance contract

Verified by user on 2026-09-09.

- `.NET Framework 4.8.1` Example `HAgent.Example → Memory → Memory Families` succeeded.
- `.NET 9` Example `HAgent.Example → Memory → Memory Families` succeeded.
- Full `HAgent.Tests`: **167/167 passed, 0 failed, 0 skipped** on .NET 9.

The canonical `MemoryEntry` now carries explicit family/type, bounded provenance/evidence/confidence, optional expiration metadata, deterministic validation, and deep clone isolation. Existing File/SQL Server/MySQL/InMemory stores retain the same representation.

## Current 0.9575 Slice 5 — Memory governance and retention

Implementation is in progress and verification is pending.

The current slice reuses the existing generic tri-state resource capability system with canonical Memory resource types:

- `memory` — whole Memory capability;
- `memory.family` — one `AiMemoryFamily`;
- `memory.type` — one stable `MemoryEntry.TypeId`.

`MemoryQuery` now supports family/type and expiration filters. `AiMemoryGovernancePolicy` provides bounded retrieval limits plus per-family/type retention caps. `AiMemoryGovernanceEvaluator` evaluates the existing effective capability snapshot, and `AiGovernedMemoryStore` composes capability/policy enforcement over the existing `IMemoryStore` boundary without introducing provider-specific storage.

Architecture source: `docs/architecture/84-memory-governance.md`.
Durable decision: D-012.
Focused tests: `tests/HAgent.Tests/MemoryGovernanceTests.cs`.
Public Example: `HAgent.Example → Memory → Memory Governance`.

The current Slice 5 boundary deliberately does not include Learning candidates/promotion, Knowledge/Skill management UI, persistence redesign, or runtime execution-snapshot binding of memory policy. Those remain later slices.

## Storage implications

The configuration/storage evolution remains a cross-cutting foundation before 0.96. HAgent-owned resource state must retain canonical identity, scope, ownership, provenance, and version information without creating subsystem-specific ownership models.

SQL Server and MySQL memory persistence use explicit family/type/provenance/expiration columns while retaining the existing memory table and versioned bootstrap migration approach. File and in-memory persistence use the same `MemoryEntry` contract. Broader mature-resource persistence for learning candidates and management remains later 0.9575 work.

### Deferred exclusions

The roadmap does not introduce a separate distributed-coordination subsystem or sophisticated external secret-management architecture. Distributed behavior is handled through existing storage/runtime contracts where required, while provider credentials use the existing fixed encryption/decryption mechanism.

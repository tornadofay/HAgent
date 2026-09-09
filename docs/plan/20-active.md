# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slice 1 — Resource capability governance — VERIFIED

Verified on 2026-09-09. Resource governance composes canonical ownership, effective capability state, and unified policy authorization. User reported 146/146 tests passed and the required Example succeeded on both supported frameworks.

### Slice 2 — Knowledge/Wiki governed resource contract — VERIFIED

Verified on 2026-09-09. Managed Knowledge/Wiki resources provide explicit identity, scope/ownership, provenance, lifecycle/versioning, bounded metadata, relationships, chunks, and provider/index-neutral governed retrieval. User reported 153/153 tests passed and the required Example succeeded on both supported frameworks.

### Slice 3 — Stable/versioned Skill definition and reference contract — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 158/158 passed, 0 failed, 0 skipped on .NET 9.

The Skill architecture includes versioned reusable definitions/references, explicit scope/ownership, lifecycle/provenance, bounded contracts, preconditions, ordered procedure steps, Knowledge/Tool dependencies, constraints, execution snapshot isolation, governed resolution, and runtime-owned executable handlers outside persisted definitions.

Slice 3 is closed.

### Slice 4 — Memory family/type and provenance contract — CURRENT

**Objective:** normalize the canonical `MemoryEntry` contract around explicit reusable memory families (`Working`, `Episodic`, `Semantic`, `Procedural`, `Custom`) and stable type identifiers, while preserving bounded provenance/evidence/confidence metadata and optional expiration metadata.

**Entry condition:** Slice 3 verified and closed.

**Complete within this slice:**

- Extend the canonical `MemoryEntry`; do not introduce a second persisted memory model.
- Add explicit family and stable type identifier with future/custom type support.
- Add bounded provenance and confidence/evidence metadata.
- Add optional expiration metadata with deterministic validation.
- Provide deep cloning/validation for mutable contract state.
- Keep existing File/SQL Server/MySQL/InMemory stores on the same `MemoryEntry` representation.
- Add focused `tests/HAgent.Tests/MemoryFamilyTests.cs`.
- Add matching public Example `HAgent.Example → Memory → Memory Families`.
- Add the authoritative memory contract architecture document and durable decision as required.

**Out of scope:** memory governance/authorization orchestration, retention policy enforcement, profile/runtime memory capability UI, learning candidates, promotion, Knowledge Manager, Skill Manager, or broader storage redesign.

**Completion checkpoint:** affected projects build; focused tests and full .NET 9 tests pass; Example succeeds on .NET Framework 4.8.1 and .NET 9.

## Run rule

Complete the current slice and record its verification before selecting the next slice. Do not combine multiple numbered slices in one run.

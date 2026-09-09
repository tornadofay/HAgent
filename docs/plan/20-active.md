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

### Slice 4 — Memory family/type and provenance contract — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Memory → Memory Families` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 167/167 passed, 0 failed, 0 skipped on .NET 9.

Slice 4 is closed.

### Slice 5 — Memory governance and retention — CURRENT

**Objective:** reuse the existing generic tri-state resource capability snapshot for Memory family/type access and add provider-neutral bounded retrieval and retention policy over the existing `IMemoryStore` / `MemoryEntry` foundation.

**Complete within this slice:**

- canonical Memory capability resource types: `memory`, `memory.family`, `memory.type`;
- family/type/expiration filters on `MemoryQuery`;
- deterministic global and family/type retrieval limits capped at 1000;
- optional expiration exclusion during recall;
- per-family/type retention caps that never extend a shorter explicit expiration;
- `AiMemoryGovernancePolicy` validation and deep clone semantics;
- `AiMemoryGovernanceEvaluator` over the existing `AiResourceCapabilitySnapshot`;
- `AiGovernedMemoryStore` over the existing `IMemoryStore` boundary;
- focused tests and matching public Example.

**Out of scope:** Learning candidates/promotion, Knowledge Manager, Skill Manager, management UI, persistence redesign, or a second authorization system.

**Completion checkpoint:** affected projects build; focused tests and full .NET 9 tests pass; Example succeeds on .NET Framework 4.8.1 and .NET 9.

**Example to run:** `HAgent.Example → Memory → Memory Governance` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/MemoryGovernanceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Run rule

Complete the current slice and record its verification before selecting the next slice. Do not combine multiple numbered slices in one run.

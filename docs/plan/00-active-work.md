# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 5 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Govern Memory family/type access through the existing tri-state resource capability model and add provider-neutral bounded retrieval and retention policy over the existing `IMemoryStore` / `MemoryEntry` foundation.

## Completed milestone

0.957 Evaluation and Quality Measurement is **verified through Slice 6** on 2026-09-09. The user verified the required Example scenarios on .NET Framework 4.8.1 and .NET 9 and reported **139/139 HAgent.Tests passed**.

## Completed current-phase slices

0.9575 Slice 1 — Mature resource capability governance foundation — **verified** 2026-09-09.

- .NET Framework 4.8.1 Example: `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- .NET 9 Example: `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- Full `.NET 9` tests: **146/146 passed, 0 failed, 0 skipped**.

0.9575 Slice 2 — Knowledge/Wiki governed resource contract — **verified** 2026-09-09.

- .NET Framework 4.8.1 Example: `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` succeeded.
- .NET 9 Example: `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` succeeded.
- Full `.NET 9` tests: **153/153 passed, 0 failed, 0 skipped**.

0.9575 Slice 3 — Stable/versioned Skill definition and reference contract — **verified by user** 2026-09-09.

- .NET Framework 4.8.1 Example: `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded.
- .NET 9 Example: `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded.
- Full `HAgent.Tests`: **158/158 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 4 — Memory family/type and provenance contract — **verified by user** 2026-09-09.

- .NET Framework 4.8.1 Example: `HAgent.Example → Memory → Memory Families` succeeded.
- .NET 9 Example: `HAgent.Example → Memory → Memory Families` succeeded.
- Full `HAgent.Tests`: **167/167 passed, 0 failed, 0 skipped** on .NET 9.

Slice 4 is closed.

## Current Slice 5 — Memory governance and retention

**Objective:** reuse the existing generic capability snapshot for Memory, add explicit family/type-aware retrieval, and enforce bounded retrieval/retention through one provider-neutral `IMemoryStore` decorator.

**Complete within this slice:**

- canonical Memory capability resource keys: `memory`, `memory.family`, `memory.type`;
- family/type/expiration filters on `MemoryQuery`;
- deterministic global and family/type retrieval limits capped at 1000;
- optional expiration exclusion during recall;
- per-family/type retention caps that never extend an existing shorter expiration;
- `AiMemoryGovernancePolicy` validation/clone semantics;
- `AiMemoryGovernanceEvaluator` over the existing `AiResourceCapabilitySnapshot`;
- `AiGovernedMemoryStore` over the existing `IMemoryStore` boundary;
- focused tests for policy precedence, retention, capability denial, filtering, bounds, and cloning;
- matching public Example.

**Out of scope:** Learning candidates/promotion, Knowledge Manager, Skill Manager, management UI, persistence redesign, or replacing the existing generic authorization model.

**Example to run:** `HAgent.Example → Memory → Memory Governance` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/MemoryGovernanceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Verification status

Slice 5 implementation is not yet verified. Do not close it until the affected projects build, focused tests pass, the full .NET 9 suite passes, and the matching Example succeeds on both supported frameworks.

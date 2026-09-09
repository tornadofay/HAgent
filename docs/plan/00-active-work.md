# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 4 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Normalize the canonical provider-neutral Memory family/type and provenance/evidence contract over the existing `MemoryEntry` foundation. Keep storage/provider implementations unchanged in this slice except where the new contract is naturally serialized by existing stores.

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

The verified Skill boundary provides reusable versioned definitions/references, explicit scope/ownership, lifecycle/provenance, bounded input/output contracts, preconditions, ordered procedures, Knowledge/Tool dependencies, constraints, execution snapshot isolation, and governance denial before source access. Executable handlers remain runtime-owned and outside persisted Skill contracts.

## Current Slice 4 — Memory family/type and provenance contract

**Objective:** normalize Memory around explicit reusable families (`Working`, `Episodic`, `Semantic`, `Procedural`, and extensible `Custom`) and stable type identifiers, while preserving provenance/evidence/confidence metadata. This slice is a contract/foundation change only; governed retrieval, retention policy, profile/runtime memory capability enforcement, and candidate promotion remain later slices.

**Entry condition:** Slice 3 is verified and closed.

**Completion condition:**

- Canonical `MemoryEntry` carries explicit family/type metadata and bounded provenance/evidence/confidence state.
- The contract supports future memory types without adding one persisted class per type.
- Contract validation and cloning are deterministic and deep-copy mutable metadata/provenance.
- Existing memory stores serialize/restore the new fields through their existing `MemoryEntry` representation without introducing a second persistence model.
- Focused tests cover every supported family, custom type extensibility, malformed/bounded provenance, cloning isolation, expiration metadata, and invalid combinations.
- Matching public Example demonstrates all four core families plus a custom type and provenance preservation.

**Example to run:** `HAgent.Example → Memory → Memory Families` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/MemoryFamilyTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

Slice 4 implementation is the active run. Do not begin memory governance/retention or learning work until this slice reaches its verification checkpoint.

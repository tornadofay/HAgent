# Active Work

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

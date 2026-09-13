# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9576 Learned Resource Reliability + Adaptation
- **Status:** Slice 2 verified — checkpoint closed
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Record host-validated post-promotion reliability evidence against exact resource versions, with policy control and revision-safe updates, without mutating the authoritative resource.

## 0.9575 checkpoint closed

The 0.9575 Knowledge, Skills, Memory Governance + Learning phase is complete through the Learning Review and Authoritative Resource Inventory + Detail Inspection management increment.

User verification completed on 2026-09-12, followed by 0.9576 Slice 1 verification and a 234/234 full-suite result reported by the user.

## 0.9576 Slice 1 checkpoint closed

User verified `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9. The reported full .NET 9 regression result was 234/234 passed, 0 failed, 0 skipped.

## 0.9576 Slice 2 checkpoint closed

User verified `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13.

The user also reported the full `.NET 9` `HAgent.Tests` result: **242/242 passed, 0 failed, 0 skipped**.

Verified boundary:

- `AiResourceReliabilityIdentity` keyed by resource type, resource ID, published version, and scope;
- distinct promotion evidence and operational outcome evidence;
- host-validated outcomes only;
- bounded reliability score and outcome counters;
- review and quarantine recommendations as evidence-derived state;
- revision-safe provider-neutral reliability persistence;
- policy-controlled reliability updates;
- success reinforcement, failure weakening, invalid-precondition weakening, and contradiction weakening;
- execution/runtime/agent/evaluation provenance preservation;
- no mutation of the promoted resource version.

Example organization was also corrected: `Learned Resource Applicability` and `Learned Resource Reliability` are under `Cognition → Learning`, and unknown feature/subgroup classification now fails closed instead of silently falling into Diagnostics.

## Files and verification

- Architecture: `docs/architecture/98-learned-resource-reliability.md`.
- Tests: `tests/HAgent.Tests/LearnedResourceReliabilityTests.cs` / `LearnedResourceReliabilityTests`.
- Example: `src/HAgent.Example/MainForm.LearnedResourceReliability.cs` / `Learned Resource Reliability`.
- Example organization: `src/HAgent.Example/MainForm.ExampleOrganization.cs`.

**Example verified:** `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

**Tests verified:** `HAgent.Tests → LearnedResourceReliabilityTests.cs` and full suite; full suite reported 242/242 passed on .NET 9.

## Do not advance

Do not begin 0.9576 Slice 3 staleness/drift/revalidation in this run. A subsequent run may explicitly start Slice 3.

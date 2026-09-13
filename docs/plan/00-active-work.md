# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9576 Learned Resource Reliability + Adaptation
- **Status:** Slice 3 implementation complete — user verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Determine whether already-promoted learned resources remain current, detect staleness/degradation/drift/contradiction, govern lifecycle transitions, and create replacement candidates without mutating authoritative resource versions.

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

Example organization was also corrected: learned-resource capability scenarios are explicitly classified, and unknown feature/subgroup classification fails closed instead of silently falling into Diagnostics.

## 0.9576 Slice 3 implementation checkpoint

Implemented and not yet user-verified:

- `AiLearnedResourceLifecycleStatus`: `Active`, `UnderReview`, `Quarantined`, `Retired`;
- deterministic conditions: `Current`, `Stale`, `Degraded`, `Drifted`, `Contradicted`;
- explicit `IsAutomaticallyUsable` lifecycle gate requiring `Active + Current`;
- exact-version lifecycle records with bounded transition history;
- provider-neutral lifecycle persistence with compare-and-swap revisions;
- policy-controlled `resource.lifecycle.revalidate` and `resource.lifecycle.propose-replacement` operations;
- age-based staleness, observed degradation, contextual drift, and direct contradiction detection;
- terminal `Retired` preservation during revalidation;
- quarantine on direct contradiction/invalidation;
- governed recovery to `Active` after clean revalidation;
- replacement proposals emitted as normal typed learning candidates rather than authoritative in-place mutation;
- focused Slice 3 tests and matching manual Example.

Runtime consumption/integration remains deferred to 0.9576 Slice 5.

## Files and verification

- Architecture: `docs/architecture/99-learned-resource-lifecycle.md`.
- Tests: `tests/HAgent.Tests/LearnedResourceAdaptationTests.cs` / `LearnedResourceAdaptationTests`.
- Example: `src/HAgent.Example/MainForm.LearnedResourceAdaptation.cs` / `LEARNED RESOURCE ADAPTATION`.
- Lifecycle store: `src/HAgent.Core/Abstractions/IAiLearnedResourceLifecycleStore.cs` and `src/HAgent.Core/Runtime/InMemoryAiLearnedResourceLifecycleStore.cs`.
- Lifecycle service: `src/HAgent.Core/Runtime/AiLearnedResourceLifecycleService.cs`.

**User verification pending:** run the focused Slice 3 tests and the new Example on .NET Framework 4.8.1 and .NET 9 before closing the slice.

## Do not advance

Do not begin 0.9576 Slice 4 until Slice 3 is user-verified and explicitly closed.

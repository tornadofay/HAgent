# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 9 in progress — Learning Candidate Persistence, Retention + Review
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Continue the unfinished 0.9575 learning boundary. Slice 8 is verified and closed; Slice 9 makes the canonical typed learning candidate durable without creating a second candidate lifecycle or authoritative resource model.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

0.9575 Slice 7 — Learning Policy + Typed Candidates — **verified by user** 2026-09-11: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

0.9575 Slice 8 — Canonical Learning Lifecycle Gate — **verified by user** 2026-09-11: `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was **194/194 passed, 0 failed, 0 skipped** on .NET 9. The lifecycle identity boundary is fail-closed and requires canonical `AgentIdentityContext`.

## Current Slice 9 boundary

`AiLearningCandidateRecord` is the canonical durable envelope for the existing `AiLearningCandidate` lifecycle. `IAiLearningCandidateStore` provides provider-neutral persistence, bounded query, optimistic revision checks, and expiry cleanup. `AiLearningCandidateReviewService` uses the existing unified `IAiPolicyEngine` with operation `learning.review`; it never publishes Memory, Knowledge, or Skills.

The current implementation includes deterministic InMemory and durable File stores, typed payload round-trip, retention expiry, review authorization evidence, and stale-revision protection. Candidate persistence remains storage of a proposal/lifecycle record, not authoritative resource publication.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/LearningCandidatePersistenceTests.cs` (focused); full `HAgent.Tests` is required at the Slice 9 checkpoint.

## Verification status

Slice 9 implementation is in progress. The Slice 8 checkpoint is closed from user-provided verification. Do not advance to Slice 10 until the focused Slice 9 tests, both supported Example targets, persistence/restart behavior, retention behavior, review authorization, and stale-revision handling are actually verified.

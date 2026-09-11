# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 8 in progress — Canonical Learning Lifecycle Gate
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Continue the unfinished 0.9575 learning boundary before moving to 0.958. Do not treat unchecked historical checklist items as automatically missing; the remainder is being audited against the current architecture and implementation.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

0.9575 Slice 7 — Learning Policy + Typed Candidates — **verified by user** 2026-09-11: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

## Current Slice 8 boundary

`AiLearningLifecycleCoordinator` is the canonical gate from a validated typed learning candidate to `Rejected`, `PendingReview`, or `Approved`. It composes Learning Policy, Learning Mode, and existing unified learning-promotion authorization. It does not publish or persist authoritative Memory, Knowledge, or Skills.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Lifecycle` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/LearningLifecycleTests.cs` (focused); full `HAgent.Tests` is not required until the slice checkpoint.

## Remainder audit direction

The historical 0.9575 checklist contains both genuine remaining work and items that have already leaked into the implementation under different names or with a newer architecture. The audit must classify each remaining item as implemented, stale/obsolete, partially implemented, or genuinely outstanding before planning 0.958.

Known example: the current canonical resource scope is `Global`, `Tenant`, `User`, `Workspace`, `Agent`, `Runtime`, and `Execution`; the older checklist wording that says `Domain` is stale.

## Verification status

Slice 8 implementation is in progress. Do not select the next numbered slice until the focused tests and both supported Example targets have been run and the result recorded here.

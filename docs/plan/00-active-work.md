# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 12 in progress — Management UI
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Add production WinForms administration surfaces using the existing configuration shell and provider-neutral learning/resource contracts.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

0.9575 Slice 7 — Learning Policy + Typed Candidates — **verified by user** 2026-09-11: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

0.9575 Slice 8 — Canonical Learning Lifecycle Gate — **verified by user** 2026-09-11: `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was **194/194 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 9 — Learning Candidate Persistence, Retention + Review — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified durable recovery, PendingReview revision restoration, authorized review, revision 1 → 2, reviewer/policy evidence, and absence of authoritative publication.
- Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified Memory promotion, new published Knowledge/Skill versions, fresh unified authorization, publication-before-lifecycle transition, provenance evidence, and immutable version behavior.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 11 — Context, Instruction, Runtime, and Observability Integration — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Execution Integration` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified learned instruction, policy/capability-gated context, bounded snapshots, authoritative runtime observations, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

## Current Slice 12 boundary

The first management-UI increment is the Learning Review surface.

- `LearningReviewPage` lives under `src/HAgent.WinForms/UI/Configuration/Learning/` and follows the shared header/action-bar/content layout.
- Candidate listing is limited to durable, non-expired `PendingReview` records and exposes bounded metadata only.
- Reviewer user identity is explicit; tenant/workspace are optional structured identity fields.
- Approve and Reject use `AiLearningCandidateReviewService`, which re-evaluates `learning.review` through `IAiPolicyEngine` and persists reviewer/policy evidence through the existing candidate store.
- UI actions do not publish authoritative Memory, Knowledge, or Skill resources.
- `ConfigurationContext` exposes the durable File candidate-store adapter used by the current reference WinForms composition.

**Architecture:** `docs/architecture/93-learning-review-management-ui.md`.

**Example to run:** `HAgent.Example → Configuration → Learning Review` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** full `HAgent.Tests` regression suite; WinForms configuration UI is primarily verified through the supported Example host.

## Do not advance

Do not advance beyond this management-UI increment until the Learning Review Example succeeds on both supported targets and the full test suite remains green.

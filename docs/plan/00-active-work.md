# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 10 in progress — Authoritative Promotion + Version-Safe Resource Creation
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Convert an approved, durable typed learning candidate into authoritative resource state through one canonical promotion boundary. Reuse the existing Memory store contract and explicit publication targets for Knowledge and Skill.

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

## Current Slice 10 boundary

Promotion starts only from a durable candidate in `Approved` state, with retention still valid and explicit identity supplied. The promotion boundary must re-authorize `learning.promote` through the existing unified policy engine, validate the typed payload, and preserve candidate/source provenance.

- Memory promotion uses existing `IMemoryStore`.
- Knowledge promotion creates a new published version through an explicit provider-neutral publication target; an existing published version is never edited in place.
- Skill promotion creates a new published immutable version through an explicit provider-neutral publication target; an existing published definition is never edited in place.
- Equal/lower candidate versions are conflicts/stale candidates and are rejected deterministically.
- Candidate lifecycle transitions to `Promoted` only after authoritative publication succeeds.
- Promotion result records authoritative resource identity/version and promotion authorization/audit evidence.
- No prompt text or model output is itself authoritative.

**Verification gate:** focused Slice 10 tests plus a dedicated Example on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` at the Slice 10 checkpoint.

## Do not advance

Do not start Slice 11 until Memory/Knowledge/Skill promotion, version conflict handling, authorization, provenance/audit evidence, candidate lifecycle transition, and both supported Example targets are actually verified.

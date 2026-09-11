# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT. Slice 10 is in progress.**

0.957 Evaluation and Quality Measurement is verified through Slice 6. User verification on 2026-09-09 recorded **139/139 HAgent.Tests passed** and the required evaluation Example scenarios succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 verified slices

### Slice 1 — Resource capability governance
Verified on 2026-09-09. User reported 146/146 full tests and the required Example succeeded on both supported frameworks.

### Slice 2 — Knowledge/Wiki
Verified on 2026-09-09. User reported 153/153 full tests and the required Example succeeded on both supported frameworks.

### Slice 3 — Skills
Verified by user on 2026-09-09. Both required Examples succeeded; full `HAgent.Tests` was 158/158 passed, 0 failed, 0 skipped on .NET 9.

### Slice 4 — Memory family/type and provenance
Verified by user on 2026-09-09. Both required Examples succeeded; full `HAgent.Tests` was 167/167 passed, 0 failed, 0 skipped on .NET 9.

### Slice 5 — Memory governance and retention
Verified by user on 2026-09-09. Both required Examples succeeded; full `HAgent.Tests` was 175/175 passed, 0 failed, 0 skipped on .NET 9.

### Slice 6 — Learning Mode foundation
Verified by user on 2026-09-09. Both required Examples succeeded; full `HAgent.Tests` was 181/181 passed, 0 failed, 0 skipped on .NET 9.

### Slice 7 — Learning Policy + Typed Candidates — VERIFIED
Verified by user on 2026-09-11. Both required Examples succeeded; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

### Slice 8 — Canonical Learning Lifecycle Gate — VERIFIED
Verified by user on 2026-09-11. Both required Examples succeeded; full `HAgent.Tests` was 194/194 passed, 0 failed, 0 skipped on .NET 9.

### Slice 9 — Learning Candidate Persistence, Retention + Review — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` succeeded.
- .NET 9 Example `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` succeeded.
- Both Examples verified provider-neutral durable capture, persistence across restart, PendingReview status/revision restoration, authorized Learning Review, revision 1 → 2, reviewer identity/policy evidence, and absence of authoritative publication.
- Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

The durable candidate boundary is now closed. It remains separate from authoritative resource publication.

## Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — IN PROGRESS

Promotion is the canonical boundary that turns an approved durable typed candidate into authoritative state.

Current architectural requirements:

- re-authorize `learning.promote` through the existing unified policy engine with explicit identity;
- reject expired, rejected, promoted, or non-approved candidates;
- restore and validate the typed payload before publication;
- publish Memory through the existing `IMemoryStore` contract;
- publish Knowledge through an explicit provider-neutral target that creates a new authoritative version without mutating a published version;
- publish Skill through an explicit provider-neutral target that creates a new immutable version;
- reject stale/equal Knowledge or Skill versions deterministically;
- preserve source/candidate provenance and policy authorization evidence;
- mark the candidate `Promoted` only after successful authoritative publication;
- produce structured promotion/audit evidence without creating a second lifecycle.

**Required Example:** `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` on .NET Framework 4.8.1 and .NET 9.

**Required tests:** `tests/HAgent.Tests/LearningCandidatePromotionTests.cs`; full `HAgent.Tests` at the Slice 10 checkpoint.

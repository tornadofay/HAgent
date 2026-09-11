# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT. Slice 12 is in progress.**

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

### Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` succeeded.
- .NET 9 Example `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` succeeded.
- Both Examples verified Memory promotion, creation of a new published Knowledge version, creation of a new immutable Skill version, fresh unified promotion authorization, publication-before-lifecycle transition, provenance evidence, and no mutation of existing published Knowledge or Skill versions.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

The authoritative promotion boundary is now closed.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example `HAgent.Example → Cognition → Learning → Learning Execution Integration` succeeded.
- .NET 9 Example `HAgent.Example → Cognition → Learning → Learning Execution Integration` succeeded.
- Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

The execution-integration boundary is now closed.

## Slice 12 — Management UI — IN PROGRESS

The first management-UI increment adds a production configuration surface for Learning Review using the existing WinForms configuration-shell conventions.

Current implementation:

- `Learning Review` page under `src/HAgent.WinForms/UI/Configuration/Learning/`;
- explicit reviewer user, tenant, and workspace identity fields;
- Approve/Reject routed through `AiLearningCandidateReviewService` and the existing unified policy engine;
- bounded PendingReview candidate list projection;
- durable candidate-store dependency exposed through `ConfigurationContext`;
- no authoritative Memory/Knowledge/Skill publication from the UI.

Architecture: `docs/architecture/93-learning-review-management-ui.md`.

**Example to run:** `HAgent.Example → Configuration → Learning Review` on .NET Framework 4.8.1 and .NET 9.

**Verification workflow:** `.github/workflows/verify-phase-0-9575-slice-12.yml`.

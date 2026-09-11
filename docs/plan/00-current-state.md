# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT. Slice 12 is verified; the next management increment is authoritative resource inventory and management.**

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

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified provider-neutral durable capture, persistence across restart, PendingReview status/revision restoration, authorized Learning Review, revision 1 → 2, reviewer identity/policy evidence, and absence of authoritative publication.
- Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

The durable candidate boundary is now closed. It remains separate from authoritative resource publication.

### Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified Memory promotion, creation of a new published Knowledge version, creation of a new immutable Skill version, fresh unified promotion authorization, publication-before-lifecycle transition, provenance evidence, and no mutation of existing published Knowledge or Skill versions.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

The authoritative promotion boundary is now closed.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example succeeded.
- .NET 9 Example succeeded.
- Both Examples verified provider-neutral learned instruction, policy/capability-gated learned context, bounded execution context snapshots, authoritative runtime outcome observation capture, non-creation of candidates from observations, and non-authoritative prompt text.
- Full `HAgent.Tests`: **208/208 passed, 0 failed, 0 skipped** on .NET 9.

The execution-integration boundary is now closed.

### Slice 12 — Learning Review management UI — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 real WinForms configuration flow succeeded.
- .NET 9 real WinForms configuration flow succeeded.
- Durable candidate-store reopen was verified on both targets.
- Manual workflow verified: `Learning Review Seed` → `Configuration → Learning Review` → inspect/filter → Approve → Promote → `Learning Review Verify`.
- Final candidate status was `Promoted` with lifecycle revision `3` on both targets.
- Reviewer identity evidence persisted.
- Review/promotion authorization evidence persisted with outcome `Allow`.
- The management surface reused the existing provider-neutral `AiLearningPromotionService`; WinForms did not directly publish authoritative resources.

The complete Learning Review management boundary is now closed.

## Next Slice 12 management increment — Authoritative Resource Inventory

The next increment remains inside 0.9575 and will establish management surfaces for authoritative Memory, Knowledge, and Skill resources.

Intended scope:

- one provider-neutral authoritative resource inventory boundary;
- Memory inventory using the existing memory store contract;
- Knowledge/Wiki inventory using provider-neutral resource-source/query contracts;
- Skill inventory using provider-neutral definition-source/query contracts;
- bounded read-only inventory metadata and scope filtering;
- effective agent-resource visibility without duplicating authoritative resource models;
- focused WinForms management pages under `UI/Configuration/`;
- matching `HAgent.Tests` contract/boundary tests and dedicated Example verification;
- no SQL Server/MySQL implementation work in this increment;
- no resource reliability/adaptation work, which remains 0.9576.

Architecture must keep the management UI dependent on provider-neutral resource contracts so later WPF/ASP configuration surfaces can consume the same configuration/resource boundaries.

Architecture: `docs/architecture/93-learning-review-management-ui.md`, `docs/architecture/94-learning-review-candidate-details.md`, and the resource inventory architecture to be established with the next increment.

**Verification workflow for the next increment must preserve an exact Example title/path and focused test class in this document before user verification begins.**

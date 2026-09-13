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

### 0.9575 Resource Inventory + Detail Inspection — VERIFIED

Verified by user on 2026-09-12 on both .NET Framework 4.8.1 and .NET 9:

- `HAgent.Example → Authoritative Resource Inventory` succeeded on both targets.
- Unified Memory / Knowledge / Skill inventory projection succeeded.
- Real `InMemoryMemoryStore` → Memory inventory source projection succeeded.
- Provider-neutral Knowledge source → inventory projection succeeded.
- Authoritative-only, resource-type, text, lifecycle, version, updated-time, owner, deterministic ordering, and bounded paging behavior succeeded.
- Readable Memory / Knowledge / Skill detail inspection succeeded.
- Skill storage-specific enumeration remains deferred because its current contract exposes lookup but not generic authoritative enumeration.

Architecture: `docs/architecture/95-authoritative-resource-inventory.md`, `docs/architecture/96-resource-detail-inspection.md`.

The 0.9575 management inventory increment is closed.

## Current phase — 0.9576 Learned Resource Reliability + Adaptation

### Slice 1 — Applicability and validity — VERIFIED

User verified `HAgent.Example → Learned Resource Applicability` on .NET Framework 4.8.1 and .NET 9 on 2026-09-12. The user subsequently reported the full .NET 9 regression suite at **234/234 passed, 0 failed, 0 skipped**.

The applicability boundary is closed: deterministic `Applicable`, `NotApplicable`, `Uncertain`, and `Invalidated` outcomes; bounded evidence; scope-aware evaluation; and separation from authorization.

Architecture: `docs/architecture/97-learned-resource-applicability.md`.

### Slice 2 — Reliability evidence and outcome feedback — VERIFIED

User verified `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13 and reported the full .NET 9 regression suite at **242/242 passed, 0 failed, 0 skipped**.

The reliability boundary is closed: post-promotion reliability is version-scoped, policy-controlled, revision-safe, provenance-preserving, and separate from both promotion evidence and authoritative resource mutation.

Architecture: `docs/architecture/98-learned-resource-reliability.md`.

Example organization was also hardened so unclassified examples no longer fall through to Diagnostics; feature and subgroup classification now fails closed and requires an explicit architecture mapping.

**Tests:** `HAgent.Tests → LearnedResourceReliabilityTests.cs`, plus the full regression suite reported at 242/242 on .NET 9.

**Example:** `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

## Remaining 0.9576 work

- staleness, contradiction, drift, and revalidation;
- quarantine, retirement, archival, and forgetting;
- governed replacement candidates without in-place mutation;
- runtime integration and end-to-end verification.

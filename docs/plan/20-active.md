# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slices 1–8 — VERIFIED

Slices 1–6 were verified on 2026-09-09. Slice 7 and Slice 8 were verified by the user on 2026-09-11. Their recorded test counts are 146, 153, 158, 167, 175, 181, 187, and 194 respectively; all required Examples succeeded on both supported frameworks.

### Slice 9 — Learning Candidate Persistence, Retention + Review — VERIFIED

Verified by user on 2026-09-11.

- `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` succeeded on .NET Framework 4.8.1 and .NET 9.
- The Example verified provider-neutral durable capture, restart/recovery, PendingReview restoration, authorized review, revision 1 → 2, reviewer/policy evidence, and no authoritative resource publication.
- Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

Slice 9 is closed.

### Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — CURRENT

Promotion is the only path from an approved learning candidate to authoritative resource state.

Scope:

- one canonical provider-neutral promotion service;
- re-evaluate `learning.promote` through the existing unified `IAiPolicyEngine` with explicit `AgentIdentityContext`;
- require durable candidate status `Approved` and reject expired/rejected/promoted candidates;
- validate and restore the typed candidate payload before publication;
- Memory publication through existing `IMemoryStore`;
- Knowledge publication through an explicit provider-neutral target that creates a new published version and never edits an existing published version;
- Skill publication through an explicit provider-neutral target that creates a new immutable published version;
- deterministic stale/equal version conflict rejection;
- preserve candidate, source execution/runtime/profile, provenance, and authorization evidence;
- transition the candidate to `Promoted` only after authoritative publication succeeds;
- return a structured promotion result suitable for later observability/audit integration.

### Verification

**Required Example:** `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` on .NET Framework 4.8.1 and .NET 9.

**Required tests:** focused `LearningCandidatePromotionTests.cs`; full `HAgent.Tests` at the Slice 10 checkpoint.

## Run rule

Complete the current slice and record its verification before selecting the next slice. Do not combine multiple numbered slices in one run.

# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slice 1 — Resource capability governance — VERIFIED

Verified on 2026-09-09. Resource governance composes canonical ownership, effective capability state, and unified policy authorization. User reported 146/146 tests passed and the required Example succeeded on both supported frameworks.

### Slice 2 — Knowledge/Wiki governed resource contract — VERIFIED

Verified on 2026-09-09. Managed Knowledge/Wiki resources provide explicit identity, scope/ownership, provenance, lifecycle/versioning, bounded metadata, relationships, chunks, and provider/index-neutral governed retrieval. User reported 153/153 tests passed and the required Example succeeded on both supported frameworks.

### Slice 3 — Stable/versioned Skill definition and reference contract — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 158/158 passed, 0 failed, 0 skipped on .NET 9.

The Skill architecture includes versioned reusable definitions/references, explicit scope/ownership, lifecycle/provenance, bounded contracts, preconditions, ordered procedure steps, Knowledge/Tool dependencies, constraints, execution snapshot isolation, governed resolution, and runtime-owned executable handlers outside persisted definitions.

Slice 3 is closed.

### Slice 4 — Memory family/type and provenance contract — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Memory → Memory Families` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 167/167 passed, 0 failed, 0 skipped on .NET 9.

Slice 4 is closed.

### Slice 5 — Memory governance and retention — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Memory → Memory Governance` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 175/175 passed, 0 failed, 0 skipped on .NET 9.

Slice 5 is closed.

### Slice 6 — Learning Mode foundation — VERIFIED

Verified by user on 2026-09-09.

- Example: `HAgent.Example → Cognition → Learning → Learning Mode` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 181/181 passed, 0 failed, 0 skipped on .NET 9.

`AiLearningMode` remains distinct from resource capability enablement, with persistent profile state, runtime-only override, immutable execution-snapshot capture, and the existing single learning-candidate contract.

Slice 6 is closed.

### Slice 7 — Learning Policy + Typed Candidates — VERIFIED

Verified by user on 2026-09-11.

- Example: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: 187/187 passed, 0 failed, 0 skipped on .NET 9.

The slice defines one provider-neutral Learning Policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization. It adds typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` payload contracts while reusing the canonical `AiLearningCandidate` lifecycle and promotion boundary.

Slice 7 is closed.

### Slice 8 — Canonical Learning Lifecycle Gate — VERIFIED

Verified by user on 2026-09-11.

- Example: `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded on .NET Framework 4.8.1 and .NET 9.
- Full `HAgent.Tests`: **194/194 passed, 0 failed, 0 skipped** on .NET 9.

The lifecycle gate composes typed-candidate validation, deterministic Learning Policy, Learning Mode, and the existing unified learning-promotion authorization. It routes candidates to `Rejected`, `PendingReview`, or `Approved` without publishing or persisting authoritative resources. The authorization-sensitive boundary now requires a non-null canonical `AgentIdentityContext` and clones it into the promotion request.

Slice 8 is closed.

### Slice 9 — Learning Candidate Persistence, Retention + Review — IN PROGRESS

Slice 9 makes the existing canonical candidate lifecycle durable without creating a second candidate model.

Scope:

- provider-neutral `AiLearningCandidateRecord` durable envelope;
- provider-neutral `IAiLearningCandidateStore` boundary;
- InMemory and File store implementations;
- durable lifecycle status/revision;
- retention/expiry policy and purge;
- typed payload serialization and round-trip restoration;
- persisted Learning Policy and promotion-authorization provenance;
- Learning Review through the existing unified `IAiPolicyEngine` using `learning.review`;
- reviewer identity and authorization evidence;
- optimistic revision-checked review updates;
- restart/recovery semantics;
- deterministic focused verification plus matching Example on both supported targets.

No authoritative Knowledge/Skill mutation or Memory publication is performed by the candidate store/review boundary.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `tests/HAgent.Tests/LearningCandidatePersistenceTests.cs` (focused); full `HAgent.Tests` is required at the Slice 9 checkpoint.

### Ahead-of-roadmap architectural evidence

The Phase 0.97 single-owner/runtime-concurrency spike was verified by the user on 2026-09-11 on both .NET Framework 4.8.1 and .NET 9. It is recorded in the 0.97 roadmap and architecture documents as verified architectural evidence.

This evidence does **not** advance the current implementation milestone, does not mark any 0.97 slice complete, and does not authorize production runtime implementation. Future-phase documentation may be updated when new architectural evidence or scope decisions require it, while the active implementation remains 0.9575 Slice 9.

## Run rule

Complete the current slice and record its verification before selecting the next slice. Do not combine multiple numbered slices in one run.

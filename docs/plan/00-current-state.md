# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT. Slice 9 is in progress.**

0.957 Evaluation and Quality Measurement is verified through Slice 6. User verification on 2026-09-09 recorded **139/139 HAgent.Tests passed** and the required evaluation Example scenarios succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 verified slices

### Slice 1 — Resource capability governance

Verified on 2026-09-09. User reported 146/146 full tests and the required Example succeeded on both supported frameworks.

### Slice 2 — Knowledge/Wiki

Verified on 2026-09-09. User reported 153/153 full tests and the required Example succeeded on both supported frameworks.

### Slice 3 — Skills

Verified by user on 2026-09-09.

- .NET Framework 4.8.1 Example `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded.
- .NET 9 Example `HAgent.Example → Cognition → Skills → Skill Definitions` succeeded.
- Full `HAgent.Tests`: **158/158 passed, 0 failed, 0 skipped** on .NET 9.

### Slice 4 — Memory family/type and provenance contract

Verified by user on 2026-09-09.

- .NET Framework 4.8.1 Example `HAgent.Example → Memory → Memory Families` succeeded.
- .NET 9 Example `HAgent.Example → Memory → Memory Families` succeeded.
- Full `HAgent.Tests`: **167/167 passed, 0 failed, 0 skipped** on .NET 9.

The canonical `MemoryEntry` carries explicit family/type, bounded provenance/evidence/confidence, optional expiration metadata, deterministic validation, and deep clone isolation. Existing stores retain the same representation.

### Slice 5 — Memory governance and retention

Verified by user on 2026-09-09.

- .NET Framework 4.8.1 Example `HAgent.Example → Memory → Memory Governance` succeeded.
- .NET 9 Example `HAgent.Example → Memory → Memory Governance` succeeded.
- Full `HAgent.Tests`: **175/175 passed, 0 failed, 0 skipped** on .NET 9.

The slice reuses the generic tri-state capability snapshot through `memory`, `memory.family`, and `memory.type`, with deterministic bounded retrieval, expiration filtering, per-family/type retention caps, and the provider-neutral `AiGovernedMemoryStore` decorator.

### Slice 6 — Learning Mode foundation

Verified by user on 2026-09-09.

- .NET Framework 4.8.1 Example `HAgent.Example → Cognition → Learning → Learning Mode` succeeded.
- .NET 9 Example `HAgent.Example → Cognition → Learning → Learning Mode` succeeded.
- Full `HAgent.Tests`: **181/181 passed, 0 failed, 0 skipped** on .NET 9.

`AiLearningMode` is a provider-neutral profile setting with four values:

- `Disabled` — no learning candidates;
- `SuggestOnly` — candidates are produced and require review;
- `AutomaticWithPolicy` — promotion may occur only through explicit policy;
- `FullyAutomatic` — explicitly permits unreviewed promotion.

`AgentRuntimeOverrides.LearningMode` is nullable runtime-only override state and never mutates the persistent profile. `AgentExecutionSnapshot.LearningMode` captures the effective value for one execution. `AiLearningModePolicy` provides deterministic interpretation and validation helpers.

Learning Mode is intentionally separate from resource/capability enablement. Existing learning-candidate and promotion-policy contracts remain the single candidate model; this slice did not introduce a duplicate candidate architecture.

Architecture source: `docs/architecture/85-learning-mode.md`.
Focused tests: `tests/HAgent.Tests/LearningModeTests.cs`.
Public Example: `HAgent.Example → Cognition → Learning → Learning Mode`.

### Slice 7 — Learning Policy + Typed Candidates — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example `HAgent.Example → Policy → Learning Policy` succeeded.
- .NET 9 Example `HAgent.Example → Policy → Learning Policy` succeeded.
- Full `HAgent.Tests`: **187/187 passed, 0 failed, 0 skipped** on .NET 9.

The slice defines one provider-neutral learning policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization. It adds typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts while reusing the existing canonical `AiLearningCandidate` lifecycle and promotion boundary.

The Example verified typed learning-promotion requests, candidate type/scope matching, confidence/evidence/provenance policy matching, contradiction handling, policy decision provenance, canonical lifecycle transitions, typed Memory/Knowledge/Skill validation, and rejection of published Knowledge/Skill payloads.

Architecture source: `docs/architecture/86-learning-policy.md`.
Focused tests: `tests/HAgent.Tests/LearningPolicyTests.cs`.
Public Example: `HAgent.Example → Policy → Learning Policy`.

### Slice 8 — Canonical Learning Lifecycle Gate — VERIFIED

Verified by user on 2026-09-11.

- .NET Framework 4.8.1 Example `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded.
- .NET 9 Example `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded.
- Full `HAgent.Tests`: **194/194 passed, 0 failed, 0 skipped** on .NET 9.

`AiLearningLifecycleCoordinator` is the canonical gate from a validated typed candidate to `Rejected`, `PendingReview`, or `Approved`. It composes Learning Policy, Learning Mode, and the existing unified learning-promotion authorization and now fails closed when canonical identity is missing. Approval remains eligibility for later promotion, not publication.

Architecture source: `docs/architecture/87-learning-lifecycle.md`.
Focused tests: `tests/HAgent.Tests/LearningLifecycleTests.cs`.
Public Example: `HAgent.Example → Cognition → Learning → Learning Lifecycle`.

## Slice 9 — Learning Candidate Persistence, Retention + Review — IN PROGRESS

The current implementation adds one provider-neutral durable candidate record around the canonical `AiLearningCandidate` lifecycle. It preserves typed payload JSON, lifecycle status/revision, retention metadata, provenance/evaluation state, Slice 8 policy provenance, and review authorization evidence.

Implemented so far:

- `AiLearningCandidateRecord` durable envelope;
- `IAiLearningCandidateStore` provider-neutral contract;
- `InMemoryAiLearningCandidateStore` deterministic runtime/test store;
- `FileLearningCandidateStore` durable JSONL-backed store with atomic rewrite;
- retention expiry calculation and purge;
- typed Memory/Knowledge/Skill payload capture and restoration;
- `AiLearningCandidateReviewService` using unified `IAiPolicyEngine` operation `learning.review`;
- explicit reviewer identity and persisted review authorization evidence;
- optimistic expected-revision updates preventing stale review overwrites;
- dedicated tests and Example registration;
- Slice 9 architecture document `docs/architecture/88-learning-candidate-persistence.md`.

### Deferred exclusions

Slice 9 does not perform authoritative Memory publication, Knowledge version creation, Skill version creation, production Learning Review UI, context/instruction integration, runtime learning-input capture, or the broader 0.9575 completion verification.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `tests/HAgent.Tests/LearningCandidatePersistenceTests.cs` (focused); full `HAgent.Tests` at the Slice 9 checkpoint.

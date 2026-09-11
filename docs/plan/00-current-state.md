# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT.**

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

## Current 0.9575 Slice 7 — Learning Policy + Typed Candidates

Implementation is in progress and verification is pending.

The slice will define one provider-neutral learning policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization. It will also add typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts while reusing the existing canonical `AiLearningCandidate` lifecycle.

The slice must preserve source execution/runtime/profile identity, proposed scope, provenance, and evidence/confidence where available; support deterministic code-derived learning signals without requiring an LLM; keep model-assisted extraction/evaluation optional and non-authoritative; and keep candidate creation separate from promotion.

Architecture source: the Learning Policy/Ty​ped Candidate slice documentation to be added with the implementation.
Focused tests: `tests/HAgent.Tests/LearningPolicyTests.cs`.
Public Example: `HAgent.Example → Cognition → Learning → Learning Policy`.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Policy` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `tests/HAgent.Tests/LearningPolicyTests.cs` (focused), then the full `HAgent.Tests` suite on .NET 9.

## Storage implications

File/in-memory agent persistence uses the canonical `AiAgent` representation. SQL Server explicitly persists `LearningMode` and includes a versioned schema migration. MySQL runtime persistence code now persists the explicit field; its managed bootstrap migration still requires alignment before this slice is considered storage-complete.

### Deferred exclusions

Slice 7 does not implement candidate persistence, promotion orchestration, Learning Review UI, Knowledge/Skill management UI, context integration, or the broader 0.9576 roadmap restructuring.

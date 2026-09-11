# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 7 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Define one provider-neutral Learning Policy contract and typed Memory/Knowledge/Skill candidate contracts while reusing the existing canonical learning-candidate lifecycle.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

## Current Slice 7 — Learning Policy + Typed Candidates

**Objective:** define one provider-neutral learning policy contract and typed Memory/Knowledge/Skill candidate payload contracts while reusing the existing canonical `AiLearningCandidate` lifecycle and promotion boundary.

**Complete within this slice:**

- learning policy covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization;
- typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` contracts;
- source execution/runtime/profile identity, proposed scope, provenance, and evidence/confidence preservation where available;
- deterministic code-derived learning signals without requiring an LLM;
- optional model-assisted extraction/evaluation that remains non-authoritative;
- candidate creation kept separate from candidate promotion.

**Out of scope:** candidate persistence, promotion orchestration, Learning Review UI, Knowledge/Skill management UI, context integration, and the broader 0.9576 roadmap restructuring.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Policy` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/LearningPolicyTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Verification status

Slice 7 implementation is in progress and verification is pending. Do not close it until the affected projects build, the focused tests pass, the full .NET 9 suite passes, and the matching Example succeeds on both supported frameworks.

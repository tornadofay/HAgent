# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 7 verified — next slice not yet started
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Slice 7 Learning Policy + Typed Candidates is complete; preserve the repository checkpoint before selecting the next numbered slice.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

0.9575 Slice 7 — Learning Policy + Typed Candidates — **verified by user** 2026-09-11: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

### Slice 7 completed boundary

The slice defines one provider-neutral Learning Policy contract covering candidate type, scope, confidence/evidence, provenance, contradiction checks, retention, evaluation requirements, and promotion authorization. It adds typed `MemoryCandidate`, `KnowledgeCandidate`, and `SkillCandidate` payload contracts while reusing the existing canonical `AiLearningCandidate` lifecycle and promotion boundary.

**Example verified:** `HAgent.Example → Policy → Learning Policy` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests verified:** `tests/HAgent.Tests/LearningPolicyTests.cs` plus full `HAgent.Tests` on **.NET 9** — 187/187 passed.

## Verification status

Slice 7 is verified complete. Do not begin the next numbered slice until the next active slice is explicitly selected and recorded here.

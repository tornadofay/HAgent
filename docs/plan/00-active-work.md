# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 6 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Establish provider-neutral Learning Mode semantics at profile/runtime/execution-snapshot boundaries without creating a duplicate learning-candidate model.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

## Current Slice 6 — Learning Mode foundation

**Objective:** define `Disabled`, `SuggestOnly`, `AutomaticWithPolicy`, and `FullyAutomatic` as a provider-neutral learning lifecycle setting; keep it separate from resource capability enablement; allow runtime-only override and immutable execution snapshot capture; preserve the existing single learning-candidate contract.

**Complete within this slice:**

- `AiLearningMode` enum and deterministic interpretation helpers;
- persistent `AiAgent.LearningMode` defaulting to `Disabled`;
- nullable runtime-only `AgentRuntimeOverrides.LearningMode`;
- effective `AgentExecutionSnapshot.LearningMode` captured from profile/runtime state;
- validation of Learning Mode values in promotion requests;
- profile/runtime/snapshot isolation tests;
- matching public Example.

**Out of scope:** candidate storage, promotion orchestration, Learning Review UI, model-assisted extraction, context integration, or a second candidate architecture.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Mode` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/LearningModeTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Verification status

Slice 6 implementation is not yet verified. Do not close it until the affected projects build, focused tests pass, the full .NET 9 suite passes, and the matching Example succeeds on both supported frameworks.

# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** Slice 11 in progress — Context, Instruction, Runtime, and Observability Integration
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Connect governed learned resources to real execution through the existing context/instruction boundaries and capture authoritative runtime observations as bounded learning input.

## Completed current-phase slices

0.9575 Slice 1 — Resource capability governance — **verified** 2026-09-09: user reported 146/146 tests and both required Examples succeeded.

0.9575 Slice 2 — Knowledge/Wiki — **verified** 2026-09-09: user reported 153/153 tests and both required Examples succeeded.

0.9575 Slice 3 — Skills — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 158/158 passed.

0.9575 Slice 4 — Memory family/type and provenance — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 167/167 passed.

0.9575 Slice 5 — Memory governance and retention — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 175/175 passed.

0.9575 Slice 6 — Learning Mode — **verified by user** 2026-09-09: both required Examples succeeded and full `HAgent.Tests` was 181/181 passed.

0.9575 Slice 7 — Learning Policy + Typed Candidates — **verified by user** 2026-09-11: `HAgent.Example → Policy → Learning Policy` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was 187/187 passed, 0 failed, 0 skipped on .NET 9.

0.9575 Slice 8 — Canonical Learning Lifecycle Gate — **verified by user** 2026-09-11: `HAgent.Example → Cognition → Learning → Learning Lifecycle` succeeded on .NET Framework 4.8.1 and .NET 9; full `HAgent.Tests` was **194/194 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 9 — Learning Candidate Persistence, Retention + Review — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Candidate Persistence` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified durable recovery, PendingReview revision restoration, authorized review, revision 1 → 2, reviewer/policy evidence, and absence of authoritative publication.
- Full `HAgent.Tests`: **200/200 passed, 0 failed, 0 skipped** on .NET 9.

0.9575 Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — **verified by user** 2026-09-11:
- `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified Memory promotion, new published Knowledge/Skill versions, fresh unified authorization, publication-before-lifecycle transition, provenance evidence, and immutable version behavior.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9.

## Current Slice 11 boundary

Learned resources must enter execution only through the canonical resource/context/instruction contracts already owned by HAgent.

- `AiLearningExecutionPreparation` clones learned instruction sources and returns them for `AgentExecutionRequest.InstructionSources`.
- Learned context retrieval sources are processed through the existing `ContextAssembler`, including capability/policy admission, bounded retrieval, ranking/deduplication, and compaction.
- `AiLearningExecutionObservationCollector` consumes the authoritative `IExecutionObservationSource` boundary and stores bounded facts through `IAiLearningObservationStore`.
- The in-memory observation store is bounded, thread-safe, filtered, and detached-copy based.
- Observation capture does not create candidates, approve promotion, or mutate authoritative resources.
- Prompt text remains non-authoritative; no second policy or tracing mechanism is introduced.

**Architecture:** `docs/architecture/90-learning-execution-integration.md`.

**Example to run:** `HAgent.Example → Cognition → Learning → Learning Execution Integration` on .NET Framework 4.8.1 and .NET 9.

**Tests to run:** `tests/HAgent.Tests/LearningExecutionIntegrationTests.cs`; full `HAgent.Tests` at the Slice 11 checkpoint.

## Do not advance

Do not start Slice 12 until learned-resource context/instruction preparation, policy/capability admission, bounded execution context, authoritative runtime observation capture, and both supported Example targets are actually verified.

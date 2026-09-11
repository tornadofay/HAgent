# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 is complete through Slice 6. User verification on 2026-09-09 recorded 139/139 tests passed and the required evaluation Example succeeded on .NET Framework 4.8.1 and .NET 9.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource, identity, policy, instruction, context, observability, evaluation, and runtime foundations. It must not introduce a parallel resource architecture.

### Slices 1–9 — VERIFIED

Slices 1–6 were verified on 2026-09-09. Slice 7, Slice 8, and Slice 9 were verified by the user on 2026-09-11. Their recorded test counts are 146, 153, 158, 167, 175, 181, 187, 194, and 200 respectively; all required Examples succeeded on both supported frameworks.

### Slice 10 — Authoritative Promotion + Version-Safe Resource Creation — VERIFIED

Verified by user on 2026-09-11.

- `HAgent.Example → Cognition → Learning → Learning Candidate Promotion` succeeded on .NET Framework 4.8.1 and .NET 9.
- Both Examples verified Memory promotion, new Knowledge version, new immutable Skill version, fresh unified promotion authorization, publication-before-lifecycle transition, provenance preservation, and no mutation of existing published versions.
- Full `HAgent.Tests`: **205/205 passed, 0 failed, 0 skipped** on .NET 9 after correcting the test implementation to the current identity/policy contracts.

Slice 10 is closed.

### Slice 11 — Context, Instruction, Runtime, and Observability Integration — CURRENT

Connect governed learned resources to real execution while retaining the existing canonical context, instruction, policy, runtime-observation, and tracing boundaries.

Scope:

- `AiLearningExecutionPreparation` validates/clones learned instruction sources for the existing `AgentExecutionRequest.InstructionSources` path;
- learned context retrieval sources flow through the existing `ContextAssembler`, including capability/policy admission, bounded retrieval, ranking/deduplication, and final compaction;
- execution receives an isolated `ContextSnapshot`, not a mutable resource object;
- authoritative runtime observations from `IExecutionObservationSource` become bounded learning-input records through `AiLearningExecutionObservationCollector`;
- `IAiLearningObservationStore` provides the provider-neutral observation persistence boundary, with a bounded thread-safe in-memory reference implementation;
- observations remain non-authoritative and do not create candidates or promotion decisions by themselves;
- no prompt text is an authorization mechanism and no new policy/context/tracing architecture is introduced.

### Verification

**Required Example:** `HAgent.Example → Cognition → Learning → Learning Execution Integration` on .NET Framework 4.8.1 and .NET 9.

**Required tests:** focused `LearningExecutionIntegrationTests.cs`; full `HAgent.Tests` at the Slice 11 checkpoint.

Architecture: `docs/architecture/90-learning-execution-integration.md`.

## Run rule

Complete the current slice and record its verification before selecting the next slice. Do not combine multiple numbered slices in one run.

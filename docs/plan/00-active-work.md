# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.957 Evaluation and Quality Measurement
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Build the provider-neutral evaluation and quality measurement foundation after verified completion of 0.956 Observability and Distributed Tracing.

## Completed prerequisite

0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime is authoritative for outcome facts; tracing observes those facts without reconstructing execution state.

## Current run

**0.957 Slice 1 — Provider-neutral evaluation contracts and evaluator boundary — VERIFIED.**

## Implemented and verified in Slice 1

- Added `AiEvaluationTargetKind` for execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning-candidate evaluation targets.
- Added provider-neutral `AiEvaluationRequest`, `AiEvaluationInputReference`, and `AiEvaluation` contracts with bounded validation and owned clone semantics.
- Added outcome, score, label, confidence, evidence, evaluator identity/type/version, timestamp, and bounded execution/runtime/agent/goal/plan/trace correlation fields.
- Added `IAiEvaluator` as the asynchronous provider-neutral evaluator boundary using `CancellationToken`.
- Added focused `tests/HAgent.Tests/EvaluationContractsTests.cs` covering target identity, clone isolation, score/confidence bounds, evaluator provenance, correlation preservation, and bounded collections.
- Added `src/HAgent.Example/MainForm.EvaluationContracts.cs` as the public API deterministic Example verification for evaluation creation and provenance/correlation preservation.
- Added `docs/architecture/23-evaluation-quality.md` defining the evaluation ownership boundary and its separation from authorization and authoritative agent state.
- User verification — **2026-09-09:** full `HAgent.Tests` completed with **96/96 tests passed** on .NET 9.
- User Example verification — **.NET Framework 4.8.1, 2026-09-09 06:26:48:** `Evaluation Contracts` succeeded.
- User Example verification — **.NET 9, 2026-09-09 06:26:18:** `Evaluation Contracts` succeeded.
- Example checks verified provider-neutral target kinds, outcome/score/label representation, evaluator provenance, execution/runtime/goal/plan/trace correlation, bounded input/evidence references, owned clone isolation, no agent-state mutation or authorization side effect, no remote grading/provider transport, and no real provider request.

## Next checkpoint

**0.957 Slice 2 — Deterministic evaluators and evaluation evidence — NOT STARTED.**

Do not implement the next slice until a new run begins.

## Current blockers

None. Slice 1 is fully verified and the repository is intentionally paused at the Slice 2 boundary.

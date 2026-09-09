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

**0.957 Slice 1 — Provider-neutral evaluation contracts and evaluator boundary — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING.**

## Implemented in Slice 1

- Added `AiEvaluationTargetKind` for execution, response, tool outcome, goal outcome, plan outcome, memory/knowledge usefulness, and learning-candidate evaluation targets.
- Added provider-neutral `AiEvaluationRequest`, `AiEvaluationInputReference`, and `AiEvaluation` contracts with bounded validation and owned clone semantics.
- Added outcome, score, label, confidence, evidence, evaluator identity/type/version, timestamp, and bounded execution/runtime/agent/goal/plan/trace correlation fields.
- Added `IAiEvaluator` as the asynchronous provider-neutral evaluator boundary using `CancellationToken`.
- Added focused `tests/HAgent.Tests/EvaluationContractsTests.cs` covering target identity, clone isolation, score/confidence bounds, evaluator provenance, correlation preservation, and bounded collections.
- Added `src/HAgent.Example/MainForm.EvaluationContracts.cs` as the public API deterministic Example verification for evaluation creation and provenance/correlation preservation.
- Added `docs/architecture/23-evaluation-quality.md` defining the evaluation ownership boundary and its separation from authorization and authoritative agent state.

## Verification boundary

- Build the solution after pulling the current branch.
- Run the full `HAgent.Tests` suite.
- Run `HAgent.Example → Diagnostics → Other Diagnostics → Evaluation Contracts` on .NET Framework 4.8.1.
- Run the same Example on .NET 9.
- Confirm the Example uses only an in-process deterministic evaluator and performs no provider transport, model grading service, remote telemetry, or agent-state mutation.
- Do not mark Slice 1 verified until all required local results are supplied.
- Do not begin Slice 2 in the same run.

## Current blockers

No known architecture blocker. Slice 1 implementation is complete; local .NET/WinForms execution remains user-side verification.

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

**0.957 Slice 2 — Deterministic evaluators and evaluation evidence — IMPLEMENTATION CHECKPOINT; LOCAL VERIFICATION PENDING.**

## Implemented in Slice 2

- Added bounded `AiEvaluationObservation` values with explicit Boolean, Decimal, and Text value kinds for host-computed evaluation facts.
- Added `AiDeterministicEvaluationRuleKind` covering schema validity, required-field completeness, policy compliance, tool success, latency, cost, and task completion.
- Added `AiDeterministicEvaluationEvaluator` implementing `IAiEvaluator` with deterministic pass/fail scoring, evidence references, evaluator provenance, correlation preservation, threshold handling, ambiguity rejection, and inconclusive outcomes for missing/mismatched signals.
- Extended `AiEvaluationRequest` with bounded, owned `Observations` and clone/validation support.
- Added focused `tests/HAgent.Tests/DeterministicEvaluationTests.cs` covering observation bounds, clone isolation, all deterministic rule families, pass/fail behavior, missing/mismatched evidence, threshold validation, duplicate-signal rejection, and cancellation.
- Added `src/HAgent.Example/MainForm.DeterministicEvaluation.cs` as the public API Example for the deterministic rule set, including explicit failure, missing-evidence, and ambiguity cases without provider/model transport.
- Explicitly classified the new Example as `Diagnostics → Evaluation → Deterministic Evaluation`.
- Updated `docs/architecture/23-evaluation-quality.md` to make bounded host observations and deterministic evaluator semantics authoritative.
- Fixed deterministic threshold/boolean inconclusive-path return types so `EvaluateAsync` consistently returns `Task<AiEvaluation>`.

## Verification boundary

The repository implementation and matching Example/test coverage are present, but this run has not executed the supported repository build/test/WinForms Example sequence from this connected session. Do not mark Slice 2 verified yet.

### Exact user verification commands/targets

**Tests to run:** `tests/HAgent.Tests/DeterministicEvaluationTests.cs` (`DeterministicEvaluationTests`) and then the full `HAgent.Tests` suite as required by the slice.

**Example to run:** `HAgent.Example → Diagnostics → Evaluation → Deterministic Evaluation` on **.NET Framework 4.8.1** and **.NET 9**.

- Build the affected solution/projects after pulling the current branch.
- Run the focused deterministic evaluation tests and the full `HAgent.Tests` suite.
- Run the exact Example above on .NET Framework 4.8.1.
- Run the same Example on .NET 9.
- Confirm deterministic evidence remains provider-neutral, bounded, cancellation-aware, and non-authoritative.

## Current blockers

Local repository execution is not available from this connected session, so build/test/Example success cannot be claimed here. The code remains at an implementation checkpoint pending actual local verification; no later 0.957 slice should begin until this checkpoint is verified.

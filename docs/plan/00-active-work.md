# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.957 Evaluation and Quality Measurement
- **Status:** In progress — Slice 4 implementation complete, verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Add the provider-neutral model-assisted evaluation boundary without coupling Core to a provider/model transport or making evaluation authoritative.

## Completed prerequisite

0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime is authoritative for outcome facts; tracing observes those facts without reconstructing execution state.

## Completed evaluation slices

0.957 Slice 1 — provider-neutral evaluation contracts and evaluator boundary is verified.

0.957 Slice 2 — deterministic evaluators and evaluation evidence is verified with 109/109 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification.

0.957 Slice 3 — human/application ratings and labeled evaluation evidence is verified with 115/115 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification.

## Current Slice 4 — Model-assisted evaluators and non-authoritative judge boundary

- Added `IAiEvaluationJudge` as the provider-neutral model-judge boundary.
- Added detached `AiEvaluationJudgeRequest` snapshots so asynchronous judge calls cannot observe caller mutation.
- Added `AiModelAssistedEvaluationEvaluator` behind `IAiEvaluator` with evaluator/judge provenance, bounded evidence/metadata ownership, explicit `evaluation.authoritative=false`, cancellation checks before and after judging, and fail-closed null/invalid/failure handling.
- Kept provider selection, credentials, model transport, retries, and host-specific evidence resolution outside `HAgent.Core` in the injected judge implementation/owning subsystem.
- Added focused `tests/HAgent.Tests/ModelAssistedEvaluationTests.cs` covering provenance, NeedsReview, detached snapshots, concurrency, cancellation, late cancellation, judge failure/null output, and bounded evaluator identity.
- Added matching public `src/HAgent.Example/MainForm.ModelAssistedEvaluation.cs`.
- Registered and classified the Example as `HAgent.Example → Diagnostics → Evaluation → Model-Assisted Evaluation`.
- Updated `docs/architecture/23-evaluation-quality.md` with the authoritative model-assisted boundary and `docs/plan/00-decisions.md` with decision D-006.

**Example to run:** `HAgent.Example → Diagnostics → Evaluation → Model-Assisted Evaluation` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/ModelAssistedEvaluationTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9** before marking Slice 4 verified.

## Verification checkpoint

A Windows GitHub Actions verification workflow was added for this branch to build HAgent.Core and HAgent.Example on .NET Framework 4.8.1/.NET 9 Windows targets and run the focused model-assisted tests. Manual Example execution is still required because `HAgent.Example` is a WinForms developer host rather than a headless test runner.

Until the actual build/test workflow result and both Example targets are confirmed, Slice 4 remains a **verified checkpoint/blocker**, not a completed slice.

## Current blocker

No implementation blocker is known. Verification is pending because the current execution environment cannot directly run the Windows solution or the WinForms Example UI. The next safe step is to consume the branch CI result, fix any compiler/test defect within Slice 4 if present, then run the exact Example path above on both supported targets and record the results.

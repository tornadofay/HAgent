# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.957 Evaluation and Quality Measurement
- **Status:** In progress — Slice 6 implementation checkpoint, verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Add provider-neutral repeated regression cases and alternative target execution with bounded concurrency, failure isolation, cancellation/late-result protection, and direct handoff to the existing evaluation aggregation/comparison contracts.

## Completed prerequisite

0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime remains authoritative for outcome facts; tracing observes those facts without reconstructing execution state.

## Completed evaluation slices

0.957 Slice 1 — provider-neutral evaluation contracts and evaluator boundary is verified.

0.957 Slice 2 — deterministic evaluators and evaluation evidence is verified with 109/109 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification.

0.957 Slice 3 — human/application ratings and labeled evaluation evidence is verified with 115/115 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification.

0.957 Slice 4 — model-assisted evaluators and non-authoritative judge boundary is verified. The user verified **123/123 HAgent.Tests**, plus the Slice 4 Example on **.NET Framework 4.8.1 and .NET 9**.

0.957 Slice 5 — evaluation aggregation and alternative-target comparison is verified by user execution on **2026-09-09**: **130/130 HAgent.Tests passed**, and the `Evaluation Aggregation` Example succeeded on **.NET Framework 4.8.1 and .NET 9**. The verified scenario produced baseline success rate `0.333333`, candidate success rate `1`, candidate average quality `0.85`, and candidate average latency `110 ms`, with no authoritative routing or authorization decision.

## Current Slice 6 — Evaluation regression suites and repeated target execution

- Added `AiEvaluationRegressionCase` for bounded reusable test-case identity, input references, and host-defined parameters.
- Added `AiEvaluationRegressionTarget` for bounded alternative target identity, name, and metadata without hard-coding provider/model semantics into Core.
- Added `AiEvaluationRegressionSuite` for bounded case/target matrices, uniqueness validation, a maximum of 1024 case-target executions, and `MaxConcurrency` from 1 to 32.
- Added `AiEvaluationRegressionCaseResult` and `AiEvaluationRegressionRun` with explicit execution status, completed evaluation samples, bounded failure codes, deterministic result ordering, and validation.
- Added `IAiEvaluationRegressionExecutor` as the host-owned execution boundary. The runner does not select providers, credentials, authorization, or production routing.
- Added `AiEvaluationRegressionRunner.RunAsync` with detached suite snapshots, semaphore-bounded concurrency, complete case-target repetition, cancellation propagation, failure isolation, invalid/null sample rejection, case/variant identity protection, and late-result discard after cancellation.
- Added `AiEvaluationRegressionRun.CreateAggregationRequest()` to expose only completed, validated `AiEvaluationSample` evidence through the existing Slice 5 aggregation contract.
- Added focused `tests/HAgent.Tests/EvaluationRegressionTests.cs` covering suite validation, matrix execution, deterministic ordering, concurrency bounds, executor failure isolation, identity mismatch, cancellation/late-result protection, snapshot isolation, and aggregation handoff ownership.
- Added matching public `src/HAgent.Example/MainForm.EvaluationRegression.cs`.
- Registered/classified the Example as `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites`.
- Added `.github/workflows/verify-phase-0-957-slice-6.yml` for supported-target builds, focused regression tests, and the full .NET 9 test suite. It runs from `master`.

**Example to run:** `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/EvaluationRegressionTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Verification checkpoint

Slice 6 implementation and matching Example/test coverage are present directly on `master`. Automated build/test verification and manual Example execution are still pending for this slice. Do not mark requirement 9 or Slice 6 verified until the supported-target builds, focused/full tests, and both Example targets have actually been confirmed.

## Current blocker

No design blocker is known. Slice 6 is intentionally limited to regression-suite orchestration. Persistence, regression history storage, management UI, production provider selection, and authoritative decision-making remain outside this slice.

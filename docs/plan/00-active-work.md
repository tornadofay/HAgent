# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.957 Evaluation and Quality Measurement
- **Status:** In progress — Slice 5 implementation checkpoint, verification pending
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Add provider-neutral aggregation and comparison over bounded evaluation samples, including success/quality/latency/cost/fallback/tool-success/plan-completion metrics, without adding routing, authorization, persistence, regression-suite orchestration, or management UI.

## Completed prerequisite

0.956 Observability and Distributed Tracing is complete and verified through Slice 8 on .NET Framework 4.8.1 and .NET 9. The execution runtime remains authoritative for outcome facts; tracing observes those facts without reconstructing execution state.

## Completed evaluation slices

0.957 Slice 1 — provider-neutral evaluation contracts and evaluator boundary is verified.

0.957 Slice 2 — deterministic evaluators and evaluation evidence is verified with 109/109 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification.

0.957 Slice 3 — human/application ratings and labeled evaluation evidence is verified with 115/115 tests and successful .NET Framework 4.8.1 and .NET 9 Example verification.

0.957 Slice 4 — model-assisted evaluators and non-authoritative judge boundary is verified. The user verified **123/123 HAgent.Tests**, plus the Slice 4 Example on **.NET Framework 4.8.1 and .NET 9**.

## Current Slice 5 — Evaluation aggregation and alternative-target comparison

- Added `AiEvaluationMetricKind` and bounded `AiEvaluationMetric` contracts with provider-neutral higher/lower comparison semantics and explicit direction for custom metrics.
- Added `AiEvaluationSample` and `AiEvaluationAggregationRequest` with bounded sample counts, owned cloning, and validation.
- Added `AiEvaluationAggregate` / `AiEvaluationAggregateMetric` for per-variant outcome and measurement summaries.
- Added `AiEvaluationComparison` / `AiEvaluationMetricComparison` for left/right metric averages, deltas, and strictly comparable preferred variants.
- Added `AiEvaluationAggregator.Aggregate` and `.Compare` with cancellation checks, deterministic ordering, bounded input, detached aggregation snapshots, and no authoritative side effects.
- Added focused `tests/HAgent.Tests/EvaluationAggregationTests.cs` covering validation, grouping, outcome counts, success/quality averages, explicit metrics, comparison direction, one-sided metrics, cancellation, and detached snapshots.
- Added matching public `src/HAgent.Example/MainForm.EvaluationAggregation.cs`.
- Registered/classified the Example as `HAgent.Example → Diagnostics → Evaluation → Evaluation Aggregation`.
- Added a Windows CI workflow to build Core/Example on .NET Framework 4.8.1 and .NET 9 and run focused/full tests.

**Example to run:** `HAgent.Example → Diagnostics → Evaluation → Evaluation Aggregation` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/EvaluationAggregationTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Verification checkpoint

The Slice 5 implementation is committed to `phase-0.957-slice-5-evaluation-aggregation`. CI/build/test verification and manual Example execution remain pending. Do not mark Slice 5 verified until the focused tests, full suite, supported-target builds, and both Example targets are actually confirmed.

## Current blocker

No design blocker is known. The repository currently requires the Slice 5 focused test/full-suite results and manual Example execution on both targets. Regression-suite orchestration remains deliberately outside this slice and is the next distinct implementation objective only after Slice 5 verification.

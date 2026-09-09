# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.957 Evaluation and Quality Measurement — VERIFIED

Phase 0.957 Evaluation and Quality Measurement is complete through Slice 6. The provider-neutral evaluation architecture now covers evaluation contracts, deterministic evaluators, human/application ratings, model-assisted judging, aggregation/comparison, and repeated regression-suite execution.

### Verification

- Slice 1 — provider-neutral evaluation contracts and evaluator boundary — verified.
- Slice 2 — deterministic evaluators and evaluation evidence — verified; user reported 109/109 tests and both supported Examples succeeded.
- Slice 3 — human/application ratings and labeled evaluation evidence — verified; user reported 115/115 tests and both supported Examples succeeded.
- Slice 4 — model-assisted evaluator/judge boundary — verified; user reported 123/123 tests and both supported Examples succeeded.
- Slice 5 — aggregation and alternative-target comparison — verified; user reported 130/130 tests and both supported Examples succeeded.
- Slice 6 — regression suites and repeated target execution — verified by user on 2026-09-09: **139/139 HAgent.Tests passed**, `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` succeeded on **.NET Framework 4.8.1 and .NET 9**, with 3 cases × 2 targets, bounded concurrency of 2, isolated failures, late cancellation-result discard, and successful-sample-only aggregation handoff.

Requirement 9 (repeated test cases/regression suites) and requirement 10 (aggregate metrics) are therefore verified. Requirement 11 is verified through the matching public Examples for evaluation creation/ratings, aggregation/comparison, failure handling, and regression execution.

## 0.9575 Knowledge, Skills, Memory Governance + Learning — CURRENT

Phase 0.9575 consumes the canonical resource foundations established in 0.8 and the identity, policy, context, observability, and evaluation boundaries already completed. It must not introduce a parallel resource model.

### Slice 1 — Mature resource capability governance foundation

**Objective:** establish one provider-neutral governance boundary for effective resource capability state and governed resource access decisions, while keeping authoritative resource definitions separate from runtime snapshots.

### Required architecture to inspect before coding

- `docs/architecture/05-identity.md` — canonical ownership, scope, tenancy, principal, and resource identity semantics.
- `docs/architecture/16-cognitive-runtime.md` — persistent cognition/resource relationship and learning boundaries.
- `docs/architecture/23-evaluation-quality.md` — evaluation evidence boundary consumed by later learning governance.
- Existing resource capability contracts and runtime execution snapshot implementation.
- Existing policy engine/resource-policy integration and learning-promotion policy decision implementation.
- Existing Skill, Knowledge/Wiki, Memory, and Learning candidate foundation models from Phase 0.8.

### Slice 1 boundary

The first implementation slice should provide reusable effective resource capability/admission semantics for Skills, Knowledge/Wiki, Memory families/types, and future resource types without hard-coding resource-specific authorization into the agent runtime.

It should address inheritance/profile defaults, runtime tri-state overrides, explicit ownership/scope context, fail-closed admission where required, immutable execution snapshots, deterministic decisions, and provider-neutral public contracts. Persistent resource stores, full management UI, candidate promotion workflows, retention governance, and broader learning lifecycle remain later slices of 0.9575 unless they are required as part of this boundary.

**Example to run:** `HAgent.Example → Cognition → Resource Governance` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/ResourceGovernanceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

No design blocker is known. Start by inspecting the authoritative identity/cognitive/resource architecture and existing capability/policy/snapshot implementation. Do not create duplicate capability, resource identity, ownership, or policy models merely because the mature governance phase is not yet complete.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

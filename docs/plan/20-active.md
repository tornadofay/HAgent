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

### Slice 1 — Mature resource capability governance foundation — VERIFIED

The first slice established one provider-neutral admission boundary over the existing resource capability, identity/ownership, and unified policy contracts. Effective capability state is captured with `Default`, `Profile`, or `RuntimeOverride` source provenance; non-global requests require authoritative owner identity; ownership mismatch and disabled resources fail before policy evaluation; policy outcomes remain explicit and fail-closed.

#### Verification

User verification on 2026-09-09:

- `.NET Framework 4.8.1` Example `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- `.NET 9` Example `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- Full `HAgent.Tests` on .NET 9: **146/146 passed, 0 failed, 0 skipped**.

Slice 1 is closed. Its implementation, focused tests, matching Example, and roadmap evidence are complete.

### Slice 2 — Knowledge/Wiki governed resource contract

**Objective:** complete the provider-neutral Knowledge/Wiki resource contract on top of the verified generic governance boundary without creating a second authorization, ownership, or persistence model.

The slice should establish the canonical knowledge resource/reference shape and bounded retrieval-facing metadata needed by later context and learning slices. It must preserve resource identity, scope/ownership, provenance, lifecycle/status, version, source, relationships, and bounded retrieval metadata. Authoritative resource content must remain distinct from candidate/model-generated content.

The implementation should reuse `AiResourceGovernanceEvaluator` for admission and the existing resource/storage foundations for persistence boundaries. Do not implement the full Knowledge Manager UI, semantic/vector indexing, learning promotion, or broad retrieval orchestration in this slice unless required to establish the contract itself.

**Example to run:** `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki`

**Tests to run:** `tests/HAgent.Tests/KnowledgeResourceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

No design blocker is known. Slice 1 is verified. Slice 2 should begin by inspecting the existing Knowledge/Wiki foundation models and storage contracts, then compose the verified resource-governance boundary rather than duplicating ownership, capability, policy, or persistence concerns.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

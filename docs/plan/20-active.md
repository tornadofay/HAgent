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

### Slice 2 — Knowledge/Wiki governed resource contract — IMPLEMENTATION CHECKPOINT

**Objective:** complete the provider-neutral Knowledge/Wiki resource contract on top of the verified generic governance boundary without creating a second authorization, ownership, or persistence model.

#### Implemented in this checkpoint

- Added `AiKnowledgeResource` with explicit `Knowledge` / `Wiki` kind, scope/owner, title/content/summary, lifecycle status, version, source, timestamps, tags/categories, bounded metadata, relationships, and provenance.
- Added `AiKnowledgeProvenance` preserving source kind, source identity, URI/creator, source execution/runtime provenance, evidence, and bounded confidence.
- Added `AiKnowledgeChunk` as bounded retrieval evidence without coupling Core to a physical index or search engine.
- Added `AiKnowledgeRetrievalRequest`, `AiKnowledgeRetrievalCandidate`, `AiKnowledgeRetrievalResult`, and `IAiKnowledgeRetriever` as provider/index-neutral retrieval contracts with explicit result bounds.
- Added `AiGovernedKnowledgeRetriever`, which applies the verified `AiResourceGovernanceEvaluator` before allowing resource IDs into the underlying retriever and excludes drafts unless explicitly requested.
- Added matching `tests/HAgent.Tests/KnowledgeResourceTests.cs` for authority/lifecycle, owner requirements, cloning/provenance, bounded request state, authorized retrieval, cross-owner/draft rejection, and cancellation.
- Added matching public `src/HAgent.Example/MainForm.KnowledgeWiki.cs` and registered it under `Cognition → Knowledge/Wiki`.
- Added `.github/workflows/verify-phase-0-9575-slice-2.yml` for Core/Example builds on .NET Framework 4.8.1 and .NET 9 plus focused/full tests.

The model-generated path defaults to `Draft`, so generated content is not authoritative merely because it exists. `Published` is an explicit lifecycle state for authoritative knowledge; promotion/approval remains outside this slice.

**Example to run:** `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/KnowledgeResourceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

### Slice 2 boundary

This slice establishes the provider-neutral Knowledge/Wiki contract and governed retrieval boundary. It does not add Knowledge Manager CRUD UI, persistent resource repositories, semantic/vector indexing, candidate promotion, retention governance, or broader learning lifecycle workflows.

## Current blocker

Implementation is complete for the bounded Slice 2 scope, but verification is pending. Do not start Slice 3 until the affected projects build, `KnowledgeResourceTests` and the full .NET 9 test suite pass, and the matching Example succeeds on both required framework targets.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

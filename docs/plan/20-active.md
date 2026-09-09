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

### Slice 2 — Knowledge/Wiki governed resource contract — VERIFIED

The second slice completed the provider-neutral Knowledge/Wiki contract over the verified generic governance boundary. It provides managed `Knowledge` / `Wiki` resource identity, explicit scope/ownership, provenance, lifecycle and versioning, source metadata, bounded tags/categories/metadata, typed relationships, chunk evidence, and bounded provider/index-neutral retrieval contracts. `AiGovernedKnowledgeRetriever` reuses the same governance boundary before forwarding admitted resource IDs to a retrieval implementation.

#### Verification

User verification on 2026-09-09:

- `.NET Framework 4.8.1` Example `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` succeeded.
- `.NET 9` Example `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` succeeded.
- Full `HAgent.Tests` on .NET 9: **153/153 passed, 0 failed, 0 skipped**.

The verification demonstrated managed Wiki representation, explicit scope/owner boundaries, published version and provenance preservation, non-authoritative model-generated drafts, exclusion of another user's resource before retrieval, and bounded provider/index-independent retrieval.

Slice 2 is closed. Retrieval-facing bounds are verified; the separate context-budget integration requirement remains open for a later context/runtime slice.

### Slice 3 — Stable/versioned Skill definition and reference contract — IMPLEMENTED, VERIFICATION PENDING

**Objective:** complete the provider-neutral reusable Skill definition/reference model over the existing resource and governance foundations. The slice should preserve stable identity/version/lifecycle metadata, input/output contracts, preconditions, procedure steps, required knowledge/tools, constraints, and reusable references while keeping executable handlers outside persistence and subjecting skill access to the same generic governance boundary.

**Implemented:**

- `AiSkillDefinition` with stable ID/version, explicit scope/owner, lifecycle, provenance, bounded input/output contracts, preconditions, ordered procedure steps, Knowledge/Tool dependencies, constraints, metadata, and relationships.
- `AiSkillReference` and `AiSkillSet` for reusable version-pinnable references without duplicating definitions.
- `AiAgent.Skills` as the canonical profile-owned skill reference set.
- `IAiSkillDefinitionSource` plus `AiGovernedSkillResolver`, reusing `AiResourceGovernanceEvaluator` for `skill.invoke` before definition-source access.
- `AiSkillBinding` / `AiSkillExecutionSnapshot` plus `AgentExecutionSnapshot.Skills` for deep-cloned in-flight Skill version/state isolation.
- Matching `tests/HAgent.Tests/SkillResourceTests.cs` covering lifecycle/contracts, cloning, reference scope/owner/version, execution snapshot isolation, governed version resolution, and denial before source access.
- Matching public Example at `src/HAgent.Example/MainForm.Skills.cs`, registered as `HAgent.Example → Cognition → Skills → Skill Definitions`.
- Authoritative architecture in `docs/architecture/82-skills.md`, durable decision D-010, and dedicated `.github/workflows/verify-phase-0-9575-slice-3.yml`.

**Focused verification target:** `tests/HAgent.Tests/SkillResourceTests.cs`, then the full `HAgent.Tests` suite on .NET 9; both supported framework builds are required for the Core and Example projects.

**Current verification state:** GitHub Actions run for Slice 3 has been queued from `master`. No test/build result is recorded here until an actual run completes. The public Example remains a manual WinForms verification and has not been claimed as executed in this environment.

## Current blocker

No design blocker is known. Slice 3 implementation is present and the remaining checkpoint is execution of the focused/full verification plus the required public Example on both supported frameworks.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

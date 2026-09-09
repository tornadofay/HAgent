# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 2 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the provider-neutral Knowledge/Wiki resource contract over the verified generic resource-governance boundary without introducing a parallel authorization, ownership, or persistence model.

## Completed milestone

0.957 Evaluation and Quality Measurement is **verified through Slice 6** on 2026-09-09. The user verified the required Example scenarios on .NET Framework 4.8.1 and .NET 9 and reported **139/139 HAgent.Tests passed**.

The completed evaluation path is:

- Slice 1 — provider-neutral evaluation contracts and evaluator boundary.
- Slice 2 — deterministic evaluators and evaluation evidence.
- Slice 3 — human/application ratings and labeled evaluation evidence.
- Slice 4 — model-assisted evaluators and non-authoritative judge boundary.
- Slice 5 — aggregation and alternative-target comparison.
- Slice 6 — repeated regression cases and bounded alternative-target execution.

Evaluation remains measurement-only. Aggregation and regression results do not authorize, route production execution, mutate configuration, promote learning, or become cognitive authority.

**Latest evaluation verification evidence:** `HAgent.Tests` 139/139 passed; `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` succeeded on .NET Framework 4.8.1 and .NET 9.

## Completed current-phase slice

0.9575 Slice 1 — Mature resource capability governance foundation — is **verified** on 2026-09-09.

User verification evidence:

- `.NET Framework 4.8.1` `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- `.NET 9` `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- Full `.NET 9` `HAgent.Tests`: **146/146 passed, 0 failed, 0 skipped**.

The verified boundary composes canonical ownership, effective capability source (`Default` / `Profile` / `RuntimeOverride`), and unified policy authorization. Cross-owner and disabled-resource access fail before policy evaluation; approval remains explicitly non-admitted.

## Current Slice 2 — Knowledge/Wiki governed resource contract

The next work consumes the verified governance boundary and existing 0.8 Knowledge/Wiki foundation. The goal is to establish the canonical provider-neutral resource/reference shape and bounded retrieval metadata needed by later context and learning slices.

The slice must preserve resource identity, explicit scope/ownership, provenance, lifecycle/status, version, source, relationships, and bounded retrieval metadata. Authoritative Knowledge content must remain distinct from candidate/model-generated content. Reuse `AiResourceGovernanceEvaluator` for admission and existing resource/storage foundations for persistence boundaries.

Do not implement the full Knowledge Manager UI, semantic/vector indexing, learning promotion, or broad retrieval orchestration merely as part of this contract slice.

### Verification checkpoint

Implementation and matching Example/test coverage are pending for Phase 0.9575 Slice 2.

**Example to run:** `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki`

**Tests to run:** `tests/HAgent.Tests/KnowledgeResourceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

No design blocker is known. Before coding, inspect the existing Knowledge/Wiki foundation models, resource/storage contracts, provenance/version metadata, and Example organization so the new contract composes established boundaries instead of duplicating them.

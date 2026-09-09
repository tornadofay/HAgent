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

The bounded implementation checkpoint now contains:

- `AiKnowledgeResource`, `AiKnowledgeProvenance`, and `AiKnowledgeRelationship` for explicit identity, kind, scope/owner, content, lifecycle/status, version, source, provenance, metadata, tags/categories, and relationships.
- `AiKnowledgeChunk` for bounded retrieval evidence independent of any physical index.
- `AiKnowledgeRetrievalRequest`, `AiKnowledgeRetrievalCandidate`, `AiKnowledgeRetrievalResult`, and `IAiKnowledgeRetriever` for provider/index-neutral asynchronous retrieval with explicit limits and cancellation.
- `AiGovernedKnowledgeRetriever` composing the verified resource-governance boundary so only explicitly admitted resources enter the underlying retriever and drafts remain excluded unless requested.
- Focused tests in `tests/HAgent.Tests/KnowledgeResourceTests.cs`.
- Matching public Example in `src/HAgent.Example/MainForm.KnowledgeWiki.cs`, registered as `Cognition → Knowledge/Wiki → Knowledge/Wiki`.
- CI workflow `.github/workflows/verify-phase-0-9575-slice-2.yml` covering Core/Example builds for .NET Framework 4.8.1 and .NET 9 plus focused/full tests.

Do not implement the Knowledge Manager UI, semantic/vector indexing, learning promotion, or broader learning lifecycle in this slice.

### Verification checkpoint

Implementation is complete for the bounded Slice 2 scope, but local verification is still pending.

**Example to run:** `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/KnowledgeResourceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

Verification is the only blocker. If compilation or tests expose a defect, correct it within Slice 2 and repeat the necessary verification; do not begin Slice 3 until this checkpoint is verified.

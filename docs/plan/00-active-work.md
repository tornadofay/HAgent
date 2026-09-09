# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 1 implementation checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Establish the mature resource-governance boundary over the existing 0.8 resource foundations: explicit capability policy, effective runtime resource state, governed access to Skills/Knowledge/Memory families, and typed learning-candidate admission without introducing a parallel resource model.

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

**Latest verification evidence:** `HAgent.Tests` 139/139 passed; `HAgent.Example → Diagnostics → Evaluation → Evaluation Regression Suites` succeeded on .NET Framework 4.8.1 and .NET 9.

## Current Slice 1 — Mature resource governance foundation

The next work is Phase 0.9575, consuming the canonical resource foundations established in 0.8 and the identity/policy/evaluation boundaries already verified. Do not create a second Skill/Knowledge/Memory/Learning model.

The first slice should establish the provider-neutral governance boundary for effective resource capability state and governed access decisions while keeping authoritative resource definitions separate from runtime snapshots.

### Verification checkpoint

Implementation and matching Example/test coverage are pending for Phase 0.9575 Slice 1.

**Example to run:** `HAgent.Example → Cognition → Resource Governance` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/ResourceGovernanceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

No design blocker is known. Before coding, read the authoritative resource/identity/policy architecture documents and inspect the existing resource capability, runtime snapshot, and learning-policy implementation so the new governance boundary composes existing contracts instead of duplicating them.

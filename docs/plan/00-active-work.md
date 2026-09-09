# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.9575 Knowledge, Skills, Memory Governance + Learning
- **Status:** In progress — Slice 3 verification checkpoint
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the provider-neutral reusable Skill definition/reference model over the verified generic resource-governance boundary. Keep executable handlers outside persisted provider-neutral Skill data and capture admitted Skill versions in execution snapshots.

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

## Completed current-phase slices

0.9575 Slice 1 — Mature resource capability governance foundation — is **verified** on 2026-09-09.

User verification evidence:

- `.NET Framework 4.8.1` `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- `.NET 9` `HAgent.Example → Cognition → Resource Governance → Resource Governance` succeeded.
- Full `.NET 9` `HAgent.Tests`: **146/146 passed, 0 failed, 0 skipped**.

0.9575 Slice 2 — Knowledge/Wiki governed resource contract — is **verified** on 2026-09-09.

User verification evidence:

- `.NET Framework 4.8.1` `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` succeeded.
- `.NET 9` `HAgent.Example → Cognition → Knowledge/Wiki → Knowledge/Wiki` succeeded.
- Full `.NET 9` `HAgent.Tests`: **153/153 passed, 0 failed, 0 skipped**.

The verified Knowledge/Wiki boundary provides managed resource identity, scope/ownership, lifecycle/versioning, provenance, bounded metadata/tags/categories, typed relationships, chunk evidence, and provider/index-neutral governed retrieval.

## Current Slice 3 — Stable/versioned Skill definition and reference contract

Implemented in `HAgent.Core` and the public Example surface:

- `AiSkillDefinition` — versioned reusable definition with explicit scope/owner/lifecycle/provenance, bounded input/output contracts, preconditions, ordered procedure steps, Knowledge/Tool dependencies, constraints, metadata, and relationships.
- `AiSkillReference` / `AiSkillSet` — reusable, version-pinnable, explicitly scoped/owned references without duplicating Skill definitions.
- `AiAgent.Skills` — canonical profile-owned reference set.
- `IAiSkillDefinitionSource` and `AiGovernedSkillResolver` — asynchronous provider/storage-neutral resolution through the existing `AiResourceGovernanceEvaluator` using `skill.invoke` before source access.
- `AiSkillBinding` / `AiSkillExecutionSnapshot` and `AgentExecutionSnapshot.Skills` — deep-cloned execution-owned Skill state/version isolation.
- Executable handlers, delegates, provider SDK objects, and host callbacks are not part of persisted Skill definition/reference contracts.

Verification assets:

- Focused tests: `tests/HAgent.Tests/SkillResourceTests.cs`.
- Public Example: `HAgent.Example → Cognition → Skills → Skill Definitions`.
- CI workflow: `.github/workflows/verify-phase-0-9575-slice-3.yml` builds Core/Example for both supported frameworks, runs focused Skill tests, and runs the full .NET 9 suite.
- Architecture: `docs/architecture/82-skills.md`.
- Durable decision: D-010 in `docs/plan/00-decisions.md`.

### Verification checkpoint

The Slice 3 implementation is complete for its bounded scope. The GitHub Actions verification workflow has been queued from `master`, but no completed build/test result is recorded yet. The public WinForms Example has not been executed in this environment, so Slice 3 must not be marked verified or closed until the exact Example is run successfully on `.NET Framework 4.8.1` and `.NET 9`, and the focused/full tests complete.

**Example to run:** `HAgent.Example → Cognition → Skills → Skill Definitions` on **.NET Framework 4.8.1** and **.NET 9**.

**Tests to run:** `tests/HAgent.Tests/SkillResourceTests.cs` (focused), then the full `HAgent.Tests` suite on **.NET 9**.

## Current blocker

Verification is the only blocker. Do not begin Slice 4 or broader Skills/learning/persistence work until this Slice 3 checkpoint is verified.

# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.954 Prompt and Instruction Governance
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the ordered 0.954 slices only: Example UI prerequisite, instruction source/authority contracts, deterministic instruction composition/conflict handling, resource/external-content boundaries, execution integration, and final Example/framework verification.

## Current checkpoint

The current 0.954 entry prerequisite is implemented in `HAgent.Example` through `src/HAgent.Example/MainForm.ExampleOrganization.cs`: architecture-level feature tabs are created, Context and Runtime use nested focused sub-tabs, and the ahead-of-roadmap Learning candidate intervention example is restored to the visible Cognition group. The prerequisite has not been locally verified in this connected environment.

## Current run

**Verified checkpoint/blocker — Example UI prerequisite awaiting local verification.**

- Implementation present: `MainForm.ExampleOrganization.cs`.
- Required verification: build/run `HAgent.Example`; confirm top-level feature grouping and nested examples are usable, confirm `LEARNING INTERVENTION` is visible under `Cognition`, and confirm existing examples remain independently runnable.
- Environment limitation: this session has GitHub repository access only; there is no local checkout or executable code build/test workflow available here. The repository's only GitHub Actions workflow is documentation generation, so it cannot substitute for the required local Example verification.

## Current blockers

The 0.954 implementation cannot begin until the Example UI prerequisite is locally verified, as required by `docs/plan/20-active.md`. No claim of local build/test success is made in this run.

## Next checkpoint

After local Example verification succeeds, mark the prerequisite complete and make **0.954 slice 1 — Instruction source and authority contracts** the sole current slice. Do not begin slice 1 before that prerequisite is verified.

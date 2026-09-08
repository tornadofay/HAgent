# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.954 Prompt and Instruction Governance
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the ordered 0.954 slices only: Example UI prerequisite, instruction source/authority contracts, deterministic instruction composition/conflict handling, resource/external-content boundaries, execution integration, and final Example/framework verification.

## Current checkpoint

The 0.954 Example UI prerequisite was verified by the user on 2026-09-08 through successful `LEARNING INTERVENTION`, `CONTEXT BUDGET`, and `RUNTIME INSTANCES` examples in the reorganized host.

Slice 1 — Instruction source and authority contracts — was verified by the user on 2026-09-08 through the `COGNITION INSTRUCTIONS` Example. The result verified source creation/validation, authority-vs-trust separation, higher-authority precedence, equal-authority explicit priority, conflict representation, and provenance-preserving snapshot cloning.

## Current run

**Current slice — 0.954 Slice 2: Instruction composition and conflict handling.**

- Entry condition: Slice 1 verified.
- Scope: Integrate the canonical instruction contracts into one provider-neutral composition boundary; preserve higher-authority layers; handle conflicts, invalid sources, and unavailable/disabled sources deterministically; keep sensitive instruction/provenance diagnostics bounded and secret-safe.
- Expected files/assemblies: `HAgent.Core` instruction/composition implementation and focused `HAgent.Example` verification only; no unrelated UI or later roadmap work.
- Completion: canonical composition produces one deterministic provider-neutral instruction snapshot and does not allow lower-authority content to replace/erase higher-authority layers; invalid/unavailable sources are contained and diagnosable.
- Verification: add deterministic public-API Example coverage for ordered composition, conflict handling, disabled/invalid/unavailable sources, and secret-safe diagnostics.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. Implementation work may proceed from the verified Slice 1 boundary, but this run may only end as a verified checkpoint/blocker until the new Slice 2 Example is executed locally. No local build/test success is claimed.

## Next checkpoint

After the Slice 2 Example passes locally, update this file and `docs/plan/20-active.md` to mark Slice 2 complete, then make only **0.954 Slice 3 — Resource and external-content boundaries** current. Do not start Slice 3 before Slice 2 verification.

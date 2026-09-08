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

Slice 2 — Instruction composition and conflict handling — is implemented but not yet locally verified. `AiInstructionComposer` is now the canonical provider-neutral composition boundary; the existing `SystemPromptComposer` delegates to it. The Example now verifies additive non-conflicting composition, deterministic conflict resolution, disabled/invalid source containment, and secret-safe diagnostics.

## Current run

**Verified checkpoint/blocker — 0.954 Slice 2 awaiting local Example verification.**

- Core implementation: `src/HAgent.Core/Runtime/AiInstructionComposer.cs`.
- Existing runtime composition façade: `src/HAgent.Core/Runtime/SystemPromptComposer.cs`.
- Example verification: `src/HAgent.Example/MainForm.InstructionContractsTests.cs`.
- Architecture: `docs/architecture/11-instruction-governance.md`.
- Required verification: build/run `HAgent.Example` and execute **COGNITION INSTRUCTIONS → Run instruction contract test** again.
- Expected additional result: canonical composition preserves eligible non-conflicting sources, conflicts resolve by deterministic precedence, disabled and invalid sources are contained and diagnosable, and sensitive instruction content is excluded from diagnostics.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. No local build/test success is claimed. Slice 2 remains a verified checkpoint/blocker until the updated Example is run successfully by the user.

## Next checkpoint

After the updated `COGNITION INSTRUCTIONS` result passes locally, update this file and `docs/plan/20-active.md` to mark Slice 2 complete, then make only **0.954 Slice 3 — Resource and external-content boundaries** current. Do not start Slice 3 before Slice 2 verification.

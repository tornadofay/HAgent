# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.954 Prompt and Instruction Governance
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the ordered 0.954 slices only: Example UI prerequisite, instruction source/authority contracts, deterministic instruction composition/conflict handling, resource/external-content boundaries, execution integration, and final Example/framework verification.

## Current checkpoint

The 0.954 Example UI prerequisite was verified by the user on 2026-09-08 through successful `LEARNING INTERVENTION`, `CONTEXT BUDGET`, and `RUNTIME INSTANCES` examples in the reorganized host.

Slice 1 — Instruction source and authority contracts — was verified by the user on 2026-09-08 through `COGNITION INSTRUCTIONS`, covering source creation/validation, authority-vs-trust separation, precedence, equal-authority priority, conflict representation, and provenance-preserving snapshot cloning.

Slice 2 — Instruction composition and conflict handling — was verified by the user on 2026-09-08 through `COGNITION INSTRUCTIONS`, covering canonical additive composition, deterministic conflict resolution, disabled/invalid source containment, and sensitive-content exclusion from diagnostics.

Slice 3 — Resource and external-content boundaries — is implemented but not yet locally verified. The implementation adds canonical source factories for Skills, Knowledge, Memory, tool descriptions, runtime/host context, user input, and external content, plus explicit source availability state. The Example now verifies trusted-resource authority, untrusted external origin, disabled/unavailable containment, and resistance to lower-authority override.

## Current run

**Verified checkpoint/blocker — 0.954 Slice 3 awaiting local Example verification.**

- Resource source factory: `src/HAgent.Core/Models/AiInstructionSourceFactory.cs`.
- Availability state: `src/HAgent.Core/Models/AiInstructionContracts.cs`.
- Composition boundary: `src/HAgent.Core/Runtime/AiInstructionComposer.cs`.
- Example verification: `src/HAgent.Example/MainForm.InstructionContractsTests.cs`.
- Architecture: `docs/architecture/11-instruction-governance.md` and `docs/architecture/80-knowledge-memory-learning.md`.
- Required verification: build/run `HAgent.Example` and execute **COGNITION INSTRUCTIONS → Run instruction contract test**.
- Expected additional result: resource sources use trusted-resource authority/trust, external content remains lower-authority/untrusted, disabled/unavailable sources stay diagnosable without entering effective instructions, and lower-authority resource/external/user content cannot override trusted resource instructions.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. No local build/test success is claimed. Slice 3 remains a verified checkpoint/blocker until the updated Example is run successfully by the user.

## Next checkpoint

After the updated `COGNITION INSTRUCTIONS` result passes locally, update this file and `docs/plan/20-active.md` to mark Slice 3 complete, then make only **0.954 Slice 4 — Execution integration** current. Do not start Slice 4 before Slice 3 verification.

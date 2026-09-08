# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.954 Prompt and Instruction Governance
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete the ordered 0.954 slices only: Example UI prerequisite, instruction source/authority contracts, deterministic instruction composition/conflict handling, resource/external-content boundaries, execution integration, and final Example/framework verification.

## Current checkpoint

The 0.954 Example UI prerequisite was verified by the user on 2026-09-08 through successful `LEARNING INTERVENTION`, `CONTEXT BUDGET`, and `RUNTIME INSTANCES` examples in the reorganized host.

Slice 1 — Instruction source and authority contracts — is implemented but not yet locally verified. The implementation adds `AiInstructionSource`, authority/trust metadata, canonical generic scope/lifecycle/provenance, conflict representation, deterministic precedence, `AiInstructionSnapshot`, and `AgentExecutionSnapshot.InstructionSnapshot`. A deterministic public-API Example named `COGNITION INSTRUCTIONS` was added under the Cognition feature group.

## Current run

**Verified checkpoint/blocker — 0.954 Slice 1 awaiting local Example verification.**

- Implementation: `src/HAgent.Core/Models/AiInstructionContracts.cs`.
- Execution snapshot contract: `src/HAgent.Core/Models/AgentExecutionSnapshot.cs`.
- Example verification: `src/HAgent.Example/MainForm.InstructionContractsTests.cs` and registration in `MainForm.cs`.
- Architecture: `docs/architecture/11-instruction-governance.md`.
- Required verification: build/run `HAgent.Example` and execute **COGNITION INSTRUCTIONS → Run instruction contract test**.
- Expected result: source creation/validation, authority-vs-trust separation, higher-authority precedence, equal-authority explicit priority, conflict representation, and provenance-preserving snapshot cloning all report `verified`.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. No local build/test success is claimed. Slice 1 remains a verified checkpoint/blocker until the user runs the new Example successfully.

## Next checkpoint

After `COGNITION INSTRUCTIONS` passes locally, update this file and the authoritative phase plan to mark Slice 1 complete, then make only **0.954 Slice 2 — Instruction composition and conflict handling** current. Do not start Slice 2 before Slice 1 verification.

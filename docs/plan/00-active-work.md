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

Slice 3 — Resource and external-content boundaries — was verified by the user on 2026-09-08 through the updated `COGNITION INSTRUCTIONS` result, covering trusted-resource authority/trust, lower-authority external/user content, disabled/unavailable source handling, and resistance to lower-authority override.

Slice 4 — Execution integration — is now implemented but not yet locally verified. The execution request accepts provider-neutral instruction sources; `DefaultAgentRuntime` composes them through the canonical composer after target selection, captures a cloned effective instruction snapshot before the execution enters `Running`, and passes the same composed text to provider transport.

## Current run

**Verified checkpoint/blocker — 0.954 Slice 4 awaiting local Example verification.**

- Execution request contract: `src/HAgent.Core/Models/AgentExecutionRequest.cs`.
- Execution snapshot capture: `src/HAgent.Core/Models/AgentExecutionSnapshot.cs` and `src/HAgent.Core/Models/AgentExecution.cs`.
- Runtime integration: `src/HAgent.Core/Runtime/DefaultAgentRuntime.cs`.
- Example verification: `src/HAgent.Example/MainForm.InstructionContractsTests.cs`.
- Required verification: build/run `HAgent.Example` and execute **COGNITION INSTRUCTIONS → Run instruction contract test**.
- Expected result: effective instruction snapshot is captured before provider transport, provider receives the same composed instruction set, caller mutation after capture does not alter the execution snapshot/provider prompt, lower-authority external content is excluded, and execution/principal provenance is preserved.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. No local build/test success is claimed. Slice 4 remains a verified checkpoint/blocker until the updated Example is run successfully by the user.

## Next checkpoint

After the updated `COGNITION INSTRUCTIONS` result passes locally, update this file and `docs/plan/20-active.md` to mark Slice 4 complete, then continue only with **0.954 Slice 5 — Example coverage and framework verification**. Do not advance to 0.955 until the complete 0.954 milestone is verified.

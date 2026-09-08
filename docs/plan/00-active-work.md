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

Slice 4 — Execution integration — was verified by the user on 2026-09-08 through the updated `COGNITION INSTRUCTIONS` result, covering effective instruction snapshot capture before provider transport, provider transport parity, caller-source mutation isolation, lower-authority external exclusion, and execution/principal provenance.

## Current run

**Verified checkpoint/blocker — 0.954 Slice 5 awaiting final local Example/framework verification.**

- Execution request contract: `src/HAgent.Core/Models/AgentExecutionRequest.cs`.
- Execution snapshot capture: `src/HAgent.Core/Models/AgentExecutionSnapshot.cs` and `src/HAgent.Core/Models/AgentExecution.cs`.
- Runtime integration: `src/HAgent.Core/Runtime/DefaultAgentRuntime.cs`.
- Example verification: `src/HAgent.Example/MainForm.InstructionContractsTests.cs` plus existing deterministic execution/cancellation/failure scenarios.
- Required next verification: run the complete 0.954 Example verification set on the supported targets, including the updated **COGNITION INSTRUCTIONS** scenario and relevant existing runtime cancellation/failure boundary scenarios.

## Current blockers

The user has verified the current instruction integration scenario successfully. This connected session still cannot execute the local .NET/WinForms build or complete the supported-target matrix itself. No framework-wide local build/test success is claimed until the final Slice 5 verification is run by the user.

## Next checkpoint

After the complete 0.954 Example/framework verification passes on the supported targets, update this file and `docs/plan/20-active.md` to mark Slice 5 complete and the entire 0.954 milestone verified. Only then advance to **0.955 Context Engineering**.

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

Slice 4 — Execution integration — was verified by the user on 2026-09-08 through the updated `COGNITION INSTRUCTIONS` result, covering effective snapshot capture before provider transport, provider transport parity, caller-source mutation isolation, lower-authority external exclusion, and execution/principal provenance.

Slice 5 — Example coverage and framework verification — is current. Deterministic runtime examples that were coupled to selected-agent UI state have been corrected to self-contained local scenarios.

## Current run

**Verified checkpoint — 0.954 Slice 5 deterministic runtime coverage verified on both supported frameworks for the currently exercised scenarios; final framework matrix completion remains.**

- .NET 9: `RUNTIME TERMINAL STATE`, `RUNTIME CONCURRENCY`, `RUNTIME OVERRIDES`, and `RUNTIME STALE RESULTS` passed as deterministic local scenarios.
- .NET Framework 4.8.1: `RUNTIME SHUTDOWN`, `RUNTIME INSTANCES`, `RUNTIME OVERRIDES`, and `RUNTIME STALE RESULTS` passed as deterministic local scenarios.
- `RUNTIME SHUTDOWN` on .NET 9 initially exposed the same selected-agent coupling as the other corrected runtime examples. `src/HAgent.Example/MainForm.RuntimeLifecycleTests.cs` has now been corrected to provision an in-memory provider/agent and local adapter instead of requiring selected-agent UI state.
- `RUNTIME EXECUTION` remains a configuration-driven live example using the selected configured agent/provider and is not a deterministic Slice 5 gate by itself.
- `COGNITION INSTRUCTIONS` has already been verified by the user on .NET Framework 4.8.1 and .NET 9.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. The .NET 9 `RUNTIME SHUTDOWN` correction is committed but not locally verified. No local build/test success is claimed from this session.

## Next checkpoint

Run corrected `RUNTIME SHUTDOWN` on .NET 9. Then, if it passes, complete any remaining framework-matrix scenarios required by the active plan and record the exact successful target/framework results. Do not advance to 0.955 until Slice 5 completion is evidenced locally and the authoritative phase documents are updated.

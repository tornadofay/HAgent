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

Slice 5 — Example coverage and framework verification — is current. The first attempted runtime checks exposed Example-host setup coupling: `RUNTIME TERMINAL STATE` incorrectly depended on a manually selected agent even though it is a deterministic local verification, while `RUNTIME EXECUTION` is an intentionally configuration-driven live execution example.

## Current run

**Verified checkpoint/blocker — 0.954 Slice 5 Example setup correction awaiting local verification.**

- Deterministic terminal example: `src/HAgent.Example/MainForm.RuntimeTerminalStateTests.cs` now provisions an in-memory agent/provider and no longer requires selected-agent UI state.
- Existing concurrency example: `src/HAgent.Example/MainForm.RuntimeConcurrencyTests.cs` is already self-contained with local store/adapter coverage.
- Existing stale-result and runtime-override examples remain configuration/selected-agent dependent and should be assessed separately before being treated as deterministic Slice 5 gates.
- `RUNTIME EXECUTION` intentionally uses the selected configured agent/provider and must be run only after an enabled agent is selected/configured.
- Framework instruction verification is already complete on .NET Framework 4.8.1 and .NET 9 through `COGNITION INSTRUCTIONS`.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. The runtime-terminal Example setup defect is corrected but not locally verified. No local build/test success is claimed.

## Next checkpoint

Run the corrected `RUNTIME TERMINAL STATE` example first, then `RUNTIME CONCURRENCY`. Do not treat configuration-driven `RUNTIME EXECUTION` as a deterministic 0.954 gate when it only exercises the live selected-agent/provider path. Continue with additional Slice 5 coverage only where it directly verifies a remaining 0.954 requirement.

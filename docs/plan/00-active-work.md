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

**Verified checkpoint — 0.954 Slice 5 deterministic runtime coverage verified for the reported runs; final framework verification remains.**

- `RUNTIME TERMINAL STATE` passed after correction: caller cancellation and timeout completed before late provider completion, and late responses did not overwrite terminal state.
- `RUNTIME CONCURRENCY` passed: two independent instances overlapped successfully with distinct execution/correlation identities and instance isolation after retirement.
- `RUNTIME OVERRIDES` passed after correction: runtime overrides applied to the execution snapshot without mutating the reusable profile, and independent memory ownership was verified.
- `RUNTIME STALE RESULTS` passed after correction: execution revisions advanced from 1 to 2, the older result became non-current, the newer result remained current until retirement, and retirement invalidated current-result status.
- `RUNTIME EXECUTION` remains a configuration-driven live example using the selected configured agent/provider and is not a deterministic Slice 5 gate by itself.
- The four runtime results above were supplied by the user, but the supported framework target(s) used for those runs were not explicitly identified; do not treat them as complete framework-matrix evidence yet.
- `COGNITION INSTRUCTIONS` has already been verified by the user on .NET Framework 4.8.1 and .NET 9.

## Current blockers

This connected session cannot execute the local .NET/WinForms build or Example. No local build/test success is claimed from this session.

## Next checkpoint

Run the remaining deterministic 0.954 coverage required by the active plan, especially `RUNTIME SHUTDOWN` if it is part of the cancellation/failure boundary set, and repeat the newly gated runtime scenarios on each supported target needed for framework verification. Record the exact target/framework for each successful run. Do not advance to 0.955 until Slice 5 completion is evidenced locally and the authoritative phase documents are updated.

# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.953 Unified Policy Engine
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete policy contracts, deterministic evaluation, cost guarding, runtime enforcement, persistence/effective snapshots, authorization integration, tool/resource policy, learning-promotion policy, approvals, UI, and verification defined by the active implementation plan.

## Current checkpoint

Policy persistence, runtime provider enforcement, tool policy enforcement, policy-first host data authorization, the profile/runtime resource capability boundary, and learning-promotion policy/candidate transitions were verified locally by the user on 2026-09-08. The implementation now also contains a bounded process-local approval/defer workflow integrated with policy-gated tool execution. The matching approval workflow Example verification is the current local checkpoint.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the scope is explicitly changed.

## Current blockers

None recorded. Resource capabilities and learning-promotion policy are verified. The bounded approval/defer workflow implementation is present; only its local Example verification remains before marking that slice verified.

## Next checkpoint

Run `Approval Workflow → Run approval workflow test` after pulling current `master`. It must verify pending approval/deferral requests, policy provenance, correlation and requester identity propagation, explicit resolution, terminal-state protection, and that approval/defer decisions never execute the protected tool handler.

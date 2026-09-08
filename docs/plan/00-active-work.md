# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.953 Unified Policy Engine
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete policy contracts, deterministic evaluation, cost guarding, runtime enforcement, persistence/effective snapshots, authorization integration, tool/resource policy, learning-promotion policy, approvals, UI, and verification defined by the active implementation plan.

## Current checkpoint

Policy persistence/default-runtime integration is implemented and was verified locally by the user on 2026-09-08. The next enforcement slice is implemented in source and includes policy-gated tool invocation plus policy-first composition with host data authorization. Matching deterministic Example verification is ready and is the next local checkpoint.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the scope is explicitly changed.

## Current blockers

None recorded.

## Next checkpoint

Run the `Unified Policy` Example contract test after pulling the current master. It must verify the newly added tool denial/approval/allow cases and the policy-before-host-authorization data-access case in addition to the already passing persistence and runtime policy checks.

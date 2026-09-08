# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.953 Unified Policy Engine
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete policy contracts, deterministic evaluation, cost guarding, runtime enforcement, persistence/effective snapshots, authorization integration, tool/resource policy, learning-promotion policy, approvals, UI, and verification defined by the active implementation plan.

## Current checkpoint

Policy persistence, runtime provider enforcement, tool policy enforcement, and policy-first host data authorization were verified locally by the user on 2026-09-08. The current implementation also adds canonical profile resource capability state, runtime `Inherit` / `Enabled` / `Disabled` overrides, effective execution snapshots, resource persistence, and tool capability gating. The matching `HAgent.Example` resource capability test is the current local verification checkpoint.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the scope is explicitly changed.

## Current blockers

None recorded. The resource-capability slice is implemented; only local Example verification remains before it can be marked verified.

## Next checkpoint

Run `Resource Capabilities → Run resource capability test` after pulling current `master`. It must verify profile/runtime tri-state resolution, exact-resource precedence, default-enabled behavior, execution snapshot isolation, profile persistence, disabled-tool gating, and runtime re-enabling/disabling of the tool capability.

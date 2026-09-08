# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.953 Unified Policy Engine
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete policy contracts, deterministic evaluation, cost guarding, runtime enforcement, persistence/effective snapshots, authorization integration, tool/resource policy, learning-promotion policy, approvals, UI, and verification defined by the active implementation plan.

## Current checkpoint

Policy persistence, runtime provider enforcement, tool policy enforcement, policy-first host data authorization, the profile/runtime resource capability boundary, learning-promotion policy/candidate transitions, and the bounded approval/defer workflow were verified locally by the user on 2026-09-08. The implementation now also contains a policy management WinForms surface for rule editing, effective-decision inspection, and agent resource-capability inspection. The UI has not yet been locally verified.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the active scope is explicitly changed.

## Current blockers

None recorded. All runtime policy slices completed so far are verified. The remaining 0.953 checkpoint is local verification of the new policy management UI on the supported WinForms targets, followed by any backend-specific verification appropriate to the configured environment.

## Next checkpoint

Build and run the HAgent WinForms configuration application. Verify the **Policy** configuration surface opens, loads the persisted policy, adds/edits/deletes rules without invalid states, evaluates a request with correct outcome/provenance, and displays effective agent resource-capability state. Repeat on both `net481` and `net9.0-windows` where available.

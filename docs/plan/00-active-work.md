# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.953 Unified Policy Engine
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Complete policy contracts, deterministic evaluation, cost guarding, runtime enforcement, persistence/effective snapshots, authorization integration, tool/resource policy, learning-promotion policy, approvals, UI, and verification defined by the active implementation plan.

## Current checkpoint

Policy persistence, runtime provider enforcement, tool policy enforcement, policy-first host data authorization, and the profile/runtime resource capability boundary were verified locally by the user on 2026-09-08. The implementation now also contains typed learning-promotion policy requests and explicit learning-candidate review/promotion transitions. The matching learning-policy Example verification is the current local checkpoint.

## Work ownership

The active implementation plan is the authoritative scope for the current task. Do not start a parallel implementation of the same capability unless the scope is explicitly changed.

## Current blockers

None recorded. Resource capabilities are verified. Learning-promotion policy and candidate transition implementation is present; only its local Example verification remains before marking that slice verified.

## Next checkpoint

Run `Learning Policy → Run learning promotion test` after pulling current `master`. It must verify typed candidate/scope/evidence/provenance/contradiction policy matching, policy provenance, `Allow`/`RequireApproval`/`Deny` outcomes, and guarded `Proposed` → `PendingReview`/`Approved` → `Promoted` and rejection transitions.

# Phase 0.953 — Unified Policy Engine

## Status

**Planned architectural foundation before capability-aware execution, persistent cognition, and autonomous features.**

## Goal

Unify HAgent's growing permission, capability, cost, learning, approval, resource, and execution rules behind a coherent provider-neutral policy model.

## Requirements

1. [ ] Define generic policy, rule, scope, evaluation context, and decision contracts.
2. [ ] Represent at least `Allow`, `Deny`, `RequireApproval`, `Defer/Wait`, and `NotApplicable` outcomes where meaningful.
3. [ ] Support policy scopes such as system, tenant, user, workspace, agent, runtime, execution, resource, tool, and provider/target where applicable.
4. [ ] Integrate existing permission/authorization concepts without replacing host-owned authorization.
5. [ ] Integrate cost policy (`FreeOnly`, `FreePreferred`, `NoRestriction`) through the policy system.
6. [ ] Integrate learning promotion policy and approval requirements.
7. [ ] Integrate capability/resource enablement and runtime tri-state overrides.
8. [ ] Support explicit policy precedence and conflict resolution.
9. [ ] Preserve policy provenance so diagnostics can explain which rule produced a decision.
10. [ ] Make policy evaluation deterministic where inputs are deterministic and safe to cache when policy versions permit.
11. [ ] Capture effective policy state in execution/runtime snapshots.
12. [ ] Prevent prompt content from serving as the policy enforcement mechanism.
13. [ ] Add deterministic Example verification for policy precedence, inherited settings, denial, approval, cost restrictions, learning promotion, resource access, and runtime overrides.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects.
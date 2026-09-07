# Phase 0.953 — Unified Policy Engine

## Status

**In progress — policy contracts, deterministic evaluation, precedence, provenance, and cost guard implemented.**

## Goal

Unify HAgent's growing permission, capability, cost, learning, approval, resource, and execution rules behind a coherent provider-neutral policy model.

## Requirements

1. [x] Define generic policy, rule, scope, evaluation context, and decision contracts.
2. [x] Represent at least `Allow`, `Deny`, `RequireApproval`, `Defer/Wait`, and `NotApplicable` outcomes where meaningful.
3. [x] Support policy scopes such as system, tenant, user, workspace, agent, runtime, execution, resource, tool, and provider/target where applicable.
4. [ ] Integrate existing permission/authorization concepts without replacing host-owned authorization.
5. [x] Integrate cost policy (`FreeOnly`, `FreePreferred`, `NoRestriction`) through the policy system at the evaluation boundary.
6. [ ] Integrate learning promotion policy and approval requirements into runtime learning workflows.
7. [ ] Integrate capability/resource enablement and runtime tri-state overrides.
8. [x] Support explicit policy precedence and deterministic conflict resolution.
9. [x] Preserve policy provenance so diagnostics can explain which rule produced a decision.
10. [x] Make policy evaluation deterministic where inputs are deterministic and expose an explicit policy version for cache invalidation.
11. [ ] Capture effective policy state in execution/runtime snapshots.
12. [x] Prevent prompt content from serving as the policy enforcement mechanism.
13. [x] Add deterministic Example verification for policy precedence, denial, approval outcome, cost restrictions, resource/tool/provider matching, and deterministic conflict resolution. Learning/resource override coverage remains for later slices.

## Implemented contract slice

The current implementation adds:

- `AiPolicySet` and `AiPolicyRule` for versioned, scoped rules;
- `AiPolicyEvaluationContext` for bounded identity/resource/execution inputs;
- `AiPolicyDecision` with outcome and provenance;
- `IAiPolicyEngine` and `DefaultAiPolicyEngine`;
- deterministic precedence based on priority, scope specificity, match specificity, outcome restrictiveness, and stable rule ID;
- built-in `FreeOnly` enforcement where `Paid` and `Unknown` cost states are denied;
- deterministic Example verification in `MainForm.PolicyTests.cs`.

Runtime enforcement, persistent policy storage, learning promotion controls, resource tri-state integration, human approval workflow, and effective-policy snapshot capture remain subsequent slices.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects.

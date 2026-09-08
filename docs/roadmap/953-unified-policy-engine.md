# Phase 0.953 — Unified Policy Engine

## Status

**In progress — policy contracts, deterministic evaluation, precedence, provenance, cost guard, pre-transport runtime enforcement, effective-policy execution snapshots, and canonical policy persistence implemented; local verification of the new persistence/default-runtime path is pending.**

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
11. [x] Capture the full effective policy state in the execution snapshot, including the deep-cloned policy version/rules that govern the run.
12. [x] Prevent prompt content from serving as the policy enforcement mechanism.
13. [x] Add deterministic Example verification for policy precedence, denial, approval outcome, cost restrictions, resource/tool/provider matching, deterministic conflict resolution, pre-transport runtime denial, and effective-policy snapshot isolation.

## Implemented slices

The current implementation includes:

- `AiPolicySet` and `AiPolicyRule` for versioned, scoped rules;
- `AiPolicyEvaluationContext` for bounded identity/resource/execution inputs;
- `AiPolicyDecision` with outcome and provenance;
- `IAiPolicyEngine` and `DefaultAiPolicyEngine`;
- `GetPolicySnapshot()` as the explicit owned-policy capture boundary;
- deterministic precedence based on explicit priority, scope specificity, match specificity, outcome restrictiveness, and stable rule ID;
- empty rule constraint collections treated as unrestricted dimensions;
- built-in `FreeOnly` enforcement where `Paid` and `Unknown` cost states are denied;
- `AgentExecution.PolicyDecision` capture;
- `AgentExecutionSnapshot.EffectivePolicy` deep-cloned at execution creation;
- default runtime loading of persisted policy through `IAiStore` asynchronously, while explicit policy-engine injection remains available;
- File, SQL Server, and MySQL policy persistence through the canonical `IAiStore` contract;
- SQL Server/MySQL bootstrap creation of the policy table;
- runtime enforcement after execution-target selection and before provider transport;
- deterministic Example verification in `MainForm.PolicyTests.cs`, including policy persistence round-trip and runtime effective-policy capture.

The new persistence/default-runtime-path verification has not yet been run locally in this session and must not be described as passing until the user executes the Example or an equivalent test.

## Remaining slices

1. Integrate existing host permission/authorization concepts without replacing host ownership.
2. Apply policy to tool invocation and resource access before side effects.
3. Integrate capability/resource enablement and runtime tri-state overrides.
4. Integrate learning-promotion policy, review requirements, and typed approval transitions.
5. Add bounded human approval/defer workflow integration.
6. Add policy management UI for rules, scopes, precedence, provenance, and effective decisions.
7. Expand deterministic Example verification and backend-specific live verification where configured.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects.

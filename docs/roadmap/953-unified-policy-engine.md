# Phase 0.953 — Unified Policy Engine

## Status

**In progress — policy contracts, deterministic evaluation, precedence, provenance, cost guard, pre-transport runtime enforcement, effective-policy execution snapshots, canonical policy persistence, policy-gated tool invocation, and policy-first host authorization composition are implemented.**

## Goal

Unify HAgent's growing permission, capability, cost, learning, approval, resource, and execution rules behind a coherent provider-neutral policy model.

## Requirements

1. [x] Define generic policy, rule, scope, evaluation context, and decision contracts.
2. [x] Represent at least `Allow`, `Deny`, `RequireApproval`, `Defer/Wait`, and `NotApplicable` outcomes where meaningful.
3. [x] Support policy scopes such as system, tenant, user, workspace, agent, runtime, execution, resource, tool, and provider/target where applicable.
4. [x] Integrate existing permission/authorization concepts without replacing host-owned authorization.
5. [x] Integrate cost policy (`FreeOnly`, `FreePreferred`, `NoRestriction`) through the policy system at the evaluation boundary.
6. [ ] Integrate learning promotion policy and approval requirements into runtime learning workflows.
7. [ ] Integrate capability/resource enablement and runtime tri-state overrides.
8. [x] Support explicit policy precedence and deterministic conflict resolution.
9. [x] Preserve policy provenance so diagnostics can explain which rule produced a decision.
10. [x] Make policy evaluation deterministic where inputs are deterministic and expose an explicit policy version for cache invalidation.
11. [x] Capture the full effective policy state in the execution snapshot, including the deep-cloned policy version/rules that govern the run.
12. [x] Prevent prompt content from serving as the policy enforcement mechanism.
13. [x] Add deterministic Example verification for policy precedence, denial, approval outcome, cost restrictions, resource/tool/provider matching, deterministic conflict resolution, pre-transport runtime denial, effective-policy snapshot isolation, persistence, tool enforcement, and policy-before-host-authorization behavior.

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
- policy enforcement before executable tool handlers through `HAgentClient.ExecuteToolAsync`;
- policy decisions captured in `ToolExecutionResult` and one evaluator captured for each tool loop;
- `PolicyDataAccessAuthorizer` composition of HAgent policy with host `IDataAccessAuthorizer`, preserving host authority after policy evaluation;
- canonical identity propagation into `DataAuthorizationRequest` for policy composition;
- deterministic Example verification in `MainForm.PolicyTests.cs` for policy persistence, runtime enforcement, tool denial/approval/allow, and policy decisions that stop a data operation before the host authorization callback.

The persistence/default-runtime path was locally verified by the user on 2026-09-08. The newly added tool/data authorization verification has not yet been run locally and must not be described as passing until the next Example execution succeeds.

## Remaining slices

1. Integrate capability/resource enablement and runtime tri-state overrides.
2. Integrate learning-promotion policy, review requirements, and typed approval transitions.
3. Add bounded human approval/defer workflow integration.
4. Add policy management UI for rules, scopes, precedence, provenance, and effective decisions.
5. Expand deterministic Example verification and backend-specific live verification where configured.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects. Policy may further restrict a host operation, but an HAgent policy `Allow` never grants application authorization.

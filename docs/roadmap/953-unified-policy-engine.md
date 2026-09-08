# Phase 0.953 — Unified Policy Engine

## Status

**In progress — policy contracts, deterministic evaluation, precedence, provenance, cost guard, pre-transport runtime enforcement, effective-policy execution snapshots, canonical policy persistence, policy-gated tool invocation, policy-first host authorization composition, profile/runtime resource capability resolution, and typed learning-promotion policy transitions are implemented.**

## Goal

Unify HAgent's growing permission, capability, cost, learning, approval, resource, and execution rules behind a coherent provider-neutral policy model.

## Requirements

1. [x] Define generic policy, rule, scope, evaluation context, and decision contracts.
2. [x] Represent at least `Allow`, `Deny`, `RequireApproval`, `Defer/Wait`, and `NotApplicable` outcomes where meaningful.
3. [x] Support policy scopes such as system, tenant, user, workspace, agent, runtime, execution, resource, tool, and provider/target where applicable.
4. [x] Integrate existing permission/authorization concepts without replacing host-owned authorization.
5. [x] Integrate cost policy (`FreeOnly`, `FreePreferred`, `NoRestriction`) through the policy system at the evaluation boundary.
6. [x] Integrate typed learning-promotion policy and explicit candidate review/approval/promotion transitions into the policy boundary.
7. [x] Complete verification of capability/resource enablement and runtime tri-state overrides at the profile/runtime resource boundary, with execution snapshot capture and tool gating.
8. [x] Support explicit policy precedence and deterministic conflict resolution.
9. [x] Preserve policy provenance so diagnostics can explain which rule produced a decision.
10. [x] Make policy evaluation deterministic where inputs are deterministic and expose an explicit policy version for cache invalidation.
11. [x] Capture the full effective policy state in the execution snapshot, including the deep-cloned policy version/rules that govern the run.
12. [x] Prevent prompt content from serving as the policy enforcement mechanism.
13. [x] Complete deterministic Example verification for the newly added resource capability resolution, persistence, snapshot isolation, and tool gating in addition to the already verified policy/authorization scenarios.

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
- `AiResourceCapabilityPolicy` profile defaults with `Inherit` / `Enabled` / `Disabled` states;
- runtime-only `AgentRuntimeOverrides.ResourceCapabilityOverrides` with runtime-over-profile precedence;
- deterministic effective resolution with exact-resource precedence, resource-type fallback, `Inherit` fall-through, and default `Enabled` state;
- `AgentExecutionSnapshot.EffectiveResourceCapabilities` capturing resolved resource enablement for each execution;
- tool execution resource gating before executable handler side effects, including runtime-instance-specific overrides;
- `AiLearningPromotionRequest` for bounded typed candidate metadata and source identity;
- `AiLearningPromotionPolicy` routing learning promotion through the existing `IAiPolicyEngine` using operation `learning.promote` and resource type `learning-candidate`;
- `AiLearningCandidate` guarded `Proposed` / `PendingReview` / `Approved` / `Rejected` / `Promoted` transitions mapped from policy outcomes;
- deterministic Example verification in `MainForm.PolicyTests.cs`, `MainForm.ResourceCapabilityTests.cs`, and `MainForm.LearningPolicyTests.cs` for policy persistence, runtime enforcement, host authorization, resource capability resolution/persistence/snapshot isolation/tool gating, and learning promotion/review transitions.

Policy persistence, provider/tool/data authorization, and the resource capability boundary were locally verified by the user on 2026-09-08. The learning-promotion implementation and its new Example verification are ready for the next local run and must not be described as passing until that Example test succeeds.

## Remaining slices

1. Verify the learning-promotion policy/candidate transition slice locally and then treat it as complete.
2. Add bounded human approval/defer workflow integration.
3. Implement the policy management UI refinement in layers:
   - make rule editing human-oriented with semantic selectors for scope, agent, tool, resource, provider, operation, and outcome, while retaining advanced raw identifiers only where necessary;
   - make Effective Decisions a read-only diagnostic surface showing the evaluated context, selected rule, policy version, precedence/provenance, and built-in guard contribution;
   - make Agent Capabilities a per-agent capability/resource matrix showing persistent profile state, transient runtime override, and deterministic effective state, with the source of the effective value where useful;
   - source display names and selectable entities from existing HAgent configuration/runtime registries rather than creating duplicate UI-specific registries;
   - keep unresolved or stale identifiers visible and diagnosable rather than silently rewriting or deleting policy state;
   - preserve the core policy engine as the single decision/precedence authority. UI behavior must not introduce a second policy evaluator or alternative capability semantics.
4. Expand deterministic Example verification and backend-specific live verification where configured.

The detailed target behavior for the Policy page and its components is defined in `docs/architecture/09-policy.md` under **Policy configuration UI evolution** and should be treated as the architectural UI contract for subsequent refinements.

## Architectural rule

The policy engine decides what HAgent is permitted or configured to do. It does not become an authentication provider or replace host authority over application side effects. Policy may further restrict a host operation, but an HAgent policy `Allow` never grants application authorization.

Resource enablement is a separate configuration capability layer. It does not replace provider capability discovery or host authorization. An enabled resource must still pass any applicable policy and authorization boundaries before side effects occur.

Learning promotion uses the same policy boundary rather than a parallel learning authorization evaluator. A typed candidate may move to `Approved` only through an `Allow` policy decision or explicit review after `RequireApproval`/`Defer`. Promotion to authoritative storage is a separate operation and is not performed by policy evaluation itself.

# Unified Policy Engine

## Purpose

HAgent's policy engine provides one provider-neutral decision boundary for rules that affect what HAgent may do or what configuration applies. It is an enforcement and configuration layer, not an authentication system and not a replacement for host authorization.

## Contracts

The canonical contracts are:

- `AiPolicySet` — versioned collection of policy rules.
- `AiPolicyRule` — scoped rule with explicit outcome, priority, match constraints, and provenance text.
- `AiPolicyEvaluationContext` — deterministic input describing operation, identity, agent/runtime/execution, resource, tool, provider, target, cost state, and bounded attributes.
- `AiPolicyDecision` — normalized result including outcome, policy version, selected rule, scope, priority, reason, and built-in status.
- `IAiPolicyEngine` — provider-neutral evaluator boundary. It exposes an owned clone of the effective policy through `GetPolicySnapshot()`.
- `AgentExecutionSnapshot.EffectivePolicy` — the deep-cloned policy state captured for the lifetime of one execution.
- `IAiStore.GetPolicySetAsync` / `SavePolicySetAsync` — the canonical persistence boundary for the current HAgent policy set.
- `AiResourceCapabilityPolicy` — canonical profile/runtime resource enablement state using `Inherit`, `Enabled`, and `Disabled`.
- `AiResourceCapabilitySnapshot` — effective resource state resolved for one execution.
- `AiLearningPromotionRequest` — typed learning-promotion context carrying candidate type, proposed scope, evidence/confidence, provenance, contradiction, retention, source identities, and learning mode.
- `AiLearningPromotionPolicy` — adapter that evaluates a learning-promotion request through the unified `IAiPolicyEngine`; it does not create a second policy evaluator.
- `AiLearningCandidate` — provider-neutral candidate lifecycle state with explicit review and promotion transitions.

Supported policy outcomes are `NotApplicable`, `Allow`, `Deny`, `RequireApproval`, and `Defer`.

## Scope

Rules may be scoped to system, tenant, user, workspace, agent, runtime, execution, resource, tool, provider, or concrete execution target.

Identity matching uses the host-supplied `AgentIdentityContext`. HAgent does not authenticate the identity; it evaluates the identity presented to it.

## Matching and precedence

A rule may constrain operation, resource type/id, tool, provider, execution target, and bounded string attributes. An empty constraint collection means that dimension is unrestricted.

When multiple rules match, precedence is deterministic:

1. higher explicit `Priority`;
2. more specific policy scope;
3. more specific rule constraints;
4. more restrictive outcome;
5. stable rule ID ordering.

This makes repeated evaluation deterministic for deterministic inputs while preserving explicit policy precedence rather than relying on declaration order.

## Cost boundary

`FreeOnly` is enforced as a built-in policy guard. Only `Free` and `FreeWithinQuota` targets satisfy it. `Paid` and `Unknown` cost are denied. `FreePreferred` does not itself deny non-free targets; it remains an execution-selection preference.

Cost policy is therefore orthogonal to capability matching and provider identity.

## Authorization boundary

The policy engine does not replace host authorization. A host may still supply a dedicated authorization decision for a data operation or side effect. `PolicyDataAccessAuthorizer` composes these two authorities for structured data access: HAgent policy is evaluated first, and a deny/approval/defer outcome prevents the host callback from being invoked. A policy allow or not-applicable result only permits evaluation to continue to the host `IDataAccessAuthorizer`; the host decision remains authoritative.

`DataAuthorizationRequest` carries the canonical `AgentIdentityContext` in addition to host runtime context so policy composition can evaluate the same identity presented at the data boundary. Host authorization callbacks remain runtime-owned and are never persisted as policy/configuration.

## Resource capability boundary

Resource enablement is separate from policy authorization, provider capability discovery, and host authorization. It represents whether an HAgent resource or resource family is enabled for a profile/runtime.

Profile configuration is the default layer. Runtime-only overrides use `Inherit`, `Enabled`, and `Disabled` and never mutate the persistent profile. Resolution is deterministic:

1. exact runtime resource state;
2. runtime resource-type state;
3. exact profile resource state;
4. profile resource-type state;
5. default `Enabled`.

An `Inherit` entry does not become an effective state; it continues resolution to the lower layer. `AiResourceCapabilitySnapshot` contains only effective `Enabled`/`Disabled` entries and is captured in `AgentExecutionSnapshot.EffectiveResourceCapabilities`.

This state is configuration gating, not authorization. An enabled resource still must pass any applicable HAgent policy and host authorization boundaries before side effects occur.

## Tool enforcement

Tool invocation is an HAgent-owned side-effect boundary. `HAgentClient.ExecuteToolAsync` resolves the effective resource capability state before invoking a registered executable handler and evaluates the unified policy with operation `tool.invoke`, resource type `tool`, the concrete tool ID, the agent profile ID, and the effective identity.

A `Disabled` resource capability state blocks the tool before its executable handler runs. `Deny`, `RequireApproval`, and `Defer` policy outcomes likewise prevent the handler from running. The resulting `ToolExecutionResult` preserves the `AiPolicyDecision`, effective resource capability snapshot, and resolved resource state.

Tool execution for a live `AgentRuntimeInstance` applies that instance's runtime-only capability overrides over the persistent profile. Direct tool execution therefore uses the same tri-state semantics as execution snapshots. A tool loop retains one effective policy evaluator for the loop; runtime-instance-specific tool execution uses the instance's current runtime capability overrides for each invocation.

## Learning promotion boundary

Learning promotion uses the same unified policy evaluator rather than introducing a parallel learning-specific authorization engine. `AiLearningPromotionRequest` maps typed learning metadata into the normal policy operation `learning.promote` and resource type `learning-candidate`.

The promotion context carries candidate type, proposed target scope, confidence/evidence state, provenance state, contradiction state, retention class, source execution/runtime/agent identity, optional learning mode, and canonical `AgentIdentityContext`. Policy rules can therefore require explicit evidence/provenance, restrict candidate type or scope, deny unresolved contradictions, and require review for sensitive promotion paths.

`AiLearningPromotionPolicy.Evaluate` returns the normal `AiPolicyDecision`. `Allow` means policy permits the candidate to enter the approved state; `RequireApproval` and `Defer` move a candidate into `PendingReview`; `Deny` moves it to `Rejected`; `NotApplicable` is never treated as promotion authority.

`AiLearningCandidate` enforces typed lifecycle transitions independently of prompts or model output:

```text
Proposed
   ├── policy Allow ───────────────> Approved ──> Promoted
   ├── policy RequireApproval/Defer -> PendingReview -> Approved -> Promoted
   └── policy Deny ────────────────> Rejected
```

Explicit rejection is allowed from proposed, pending-review, or approved state. Promoted and rejected candidates are terminal. Actual repository mutation, candidate persistence, and the broader human-intervention workflow remain separate boundaries; an approved candidate does not itself mutate authoritative knowledge or skills.

## Approval and deferral

`RequireApproval` and `Defer` are first-class decisions. They must not be represented as instructions hidden inside prompts. The later human-intervention and admission layers consume these outcomes and determine how the request proceeds.

## Enforcement boundary

Prompt content is never the security or policy enforcement mechanism. A model can request an operation, but executable tools, provider calls, data access, learning promotion, and other side effects must pass the appropriate host/runtime enforcement boundary.

## Versioning, snapshots, and caching

A policy set has an explicit version. `DefaultAiPolicyEngine` snapshots the supplied policy when constructed. `GetPolicySnapshot()` returns another deep clone, so callers cannot mutate engine-owned policy state.

When an execution begins, `DefaultAgentRuntime` obtains the effective policy before creating `AgentExecutionSnapshot`. An explicitly configured policy engine supplies its owned snapshot; otherwise the runtime loads the canonical policy from `IAiStore` asynchronously and creates an immutable-for-the-run evaluator from that snapshot. `AgentExecutionSnapshot.EffectivePolicy` then retains a deep clone of the exact policy state and version that govern the execution.

The same execution boundary resolves profile resource capabilities plus runtime overrides into `AgentExecutionSnapshot.EffectiveResourceCapabilities`. Both policy and resource capability state are therefore execution snapshots; later persisted configuration or runtime-source mutations do not modify an already-created execution.

The default runtime path uses persisted HAgent policy configuration, while hosts may inject an explicit evaluator for deliberately isolated policy composition or tests. No synchronous database or network call is used to load policy.

Because the policy is captured at execution creation, later persistence changes do not modify an active run. Tool loops likewise capture the evaluator used for their complete loop so a configuration edit cannot change the policy mid-loop.

## Persistence

Policy persistence is backend-neutral at `IAiStore` and currently represented by one HAgent-owned policy set per configuration store. The File backend stores the policy with the HAgent settings document. SQL Server and MySQL use an HAgent-owned `HAgentPolicies` table containing the explicit policy version and serialized canonical policy set.

Agent resource capability defaults are part of the canonical `AiAgent` configuration and persist through the existing agent storage models. Runtime capability overrides are transient runtime configuration and are not persisted as profile state.

Learning candidate persistence and promotion targets remain governed by the later Knowledge/Skills/Memory storage phase; the 0.953 policy boundary must decide whether a promotion is permitted but does not invent a second candidate repository contract.

The SQL Server/MySQL HAgent bootstrap paths create the policy table during normal HAgent database provisioning. Missing persisted policy resolves to the valid empty policy set; malformed persisted policy is rejected rather than silently replaced.

## Current implementation

Phase 0.953 currently implements the core policy contracts, deterministic evaluator, unrestricted-dimension matching, scoped matching, precedence, provenance, the built-in cost guard, runtime pre-transport enforcement, effective-policy execution snapshots, policy persistence through the HAgent File/SQL Server/MySQL configuration stores, policy-gated tool invocation, policy-first composition with host data authorization, canonical profile/runtime resource capability resolution with execution snapshots and tool gating, and typed learning-promotion policy evaluation plus candidate review/promotion transitions. Full candidate repositories, actual promotion targets, human approval workflow, policy management UI, and full cross-backend live verification remain subsequent slices.

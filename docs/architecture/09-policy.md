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

Supported outcomes are `NotApplicable`, `Allow`, `Deny`, `RequireApproval`, and `Defer`.

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

## Tool enforcement

Tool invocation is an HAgent-owned side-effect boundary. `HAgentClient.ExecuteToolAsync` evaluates the unified policy with operation `tool.invoke`, resource type `tool`, the concrete tool ID, the agent profile ID, and the effective identity before calling the registered executable handler.

`Deny`, `RequireApproval`, and `Defer` therefore prevent the executable handler from running. The resulting `ToolExecutionResult` preserves the `AiPolicyDecision` and its provenance. `Allow` and `NotApplicable` permit the handler to execute normally.

A tool loop resolves one effective policy engine at loop start and reuses it for all tool invocations in that loop, preserving execution-level policy consistency even if persisted configuration changes while the loop is running.

## Approval and deferral

`RequireApproval` and `Defer` are first-class decisions. They must not be represented as instructions hidden inside prompts. The later human-intervention and admission layers consume these outcomes and determine how the request proceeds.

## Enforcement boundary

Prompt content is never the security or policy enforcement mechanism. A model can request an operation, but executable tools, provider calls, data access, learning promotion, and other side effects must pass the appropriate host/runtime enforcement boundary.

## Versioning, snapshots, and caching

A policy set has an explicit version. `DefaultAiPolicyEngine` snapshots the supplied policy when constructed. `GetPolicySnapshot()` returns another deep clone, so callers cannot mutate engine-owned policy state.

When an execution begins, `DefaultAgentRuntime` obtains the effective policy before creating `AgentExecutionSnapshot`. An explicitly configured policy engine supplies its owned snapshot; otherwise the runtime loads the canonical policy from `IAiStore` asynchronously and creates an immutable-for-the-run evaluator from that snapshot. `AgentExecutionSnapshot.EffectivePolicy` then retains a deep clone of the exact policy state and version that govern the execution.

The default runtime path therefore uses persisted HAgent policy configuration, while hosts may inject an explicit evaluator for deliberately isolated policy composition or tests. No synchronous database or network call is used to load policy.

Because the policy is captured at execution creation, later persistence changes do not modify an active run. Tool loops likewise capture the evaluator used for their complete loop so a configuration edit cannot change the policy mid-loop.

## Persistence

Policy persistence is backend-neutral at `IAiStore` and currently represented by one HAgent-owned policy set per configuration store. The File backend stores the policy with the HAgent settings document. SQL Server and MySQL use an HAgent-owned `HAgentPolicies` table containing the explicit policy version and serialized canonical policy set.

The SQL Server/MySQL HAgent bootstrap paths create the policy table during normal HAgent database provisioning. Missing persisted policy resolves to the valid empty policy set; malformed persisted policy is rejected rather than silently replaced.

## Current implementation

Phase 0.953 currently implements the core contracts, deterministic evaluator, unrestricted-dimension matching, scoped matching, precedence, provenance, the built-in cost guard, runtime pre-transport enforcement, effective-policy execution snapshots, policy persistence through the HAgent File/SQL Server/MySQL configuration stores, policy-gated tool invocation, and policy-first composition with host data authorization. Runtime tri-state integration, learning-promotion rules, human approval workflow, policy management UI, and full cross-backend live verification remain subsequent slices.

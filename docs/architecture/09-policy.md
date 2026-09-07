# Unified Policy Engine

## Purpose

HAgent's policy engine provides one provider-neutral decision boundary for rules that affect what HAgent may do or what configuration applies. It is an enforcement and configuration layer, not an authentication system and not a replacement for host authorization.

## Contracts

The canonical contracts are:

- `AiPolicySet` — versioned collection of policy rules.
- `AiPolicyRule` — scoped rule with explicit outcome, priority, match constraints, and provenance text.
- `AiPolicyEvaluationContext` — deterministic input describing operation, identity, agent/runtime/execution, resource, tool, provider, target, cost state, and bounded attributes.
- `AiPolicyDecision` — normalized result including outcome, policy version, selected rule, scope, priority, reason, and built-in status.
- `IAiPolicyEngine` — provider-neutral evaluator boundary.

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

The policy engine does not replace host authorization. A host may still supply a dedicated authorization decision for a data operation or side effect. Policy can express HAgent-level rules and requirements; host authorization remains authoritative for application-owned resources and side effects.

## Approval and deferral

`RequireApproval` and `Defer` are first-class decisions. They must not be represented as instructions hidden inside prompts. The later human-intervention and admission layers consume these outcomes and determine how the request proceeds.

## Enforcement boundary

Prompt content is never the security or policy enforcement mechanism. A model can request an operation, but executable tools, provider calls, data access, learning promotion, and other side effects must pass the appropriate host/runtime enforcement boundary.

## Versioning and caching

A policy set has an explicit version. Evaluators snapshot the policy set when constructed so an in-flight evaluation cannot observe mutation from an external policy editor. The policy version is carried in every decision for provenance and future cache invalidation.

## Current implementation

Phase 0.953 currently implements the core contracts, deterministic evaluator, scoped matching, precedence, provenance, and the built-in cost guard. Runtime enforcement, persistent policy storage, learning-promotion rules, resource tri-state integration, approval workflow, and effective-policy execution snapshots remain subsequent slices.

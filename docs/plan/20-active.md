# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.953 Unified Policy Engine — CURRENT

Phase 0.953 is the current foundational hardening milestone. HAgent now has provider-neutral policy contracts, deterministic evaluation, cost guarding, pre-transport runtime enforcement, effective-policy execution snapshots, canonical persistence, policy-gated tool invocation, policy-first composition with host data authorization, and profile/runtime resource capability resolution.

### Objective

Create one coherent policy boundary for HAgent decisions without making the model, prompt, authentication provider, or host business authorization responsible for enforcement.

### Completed in this milestone so far

- Versioned `AiPolicySet` and scoped `AiPolicyRule` contracts.
- `Allow`, `Deny`, `RequireApproval`, `Defer`, and `NotApplicable` outcomes.
- Evaluation contexts carrying identity, tenant/workspace, agent/runtime/execution, resource/tool, provider/target, cost, and bounded attributes.
- Deterministic rule precedence and conflict resolution.
- Empty rule constraint collections treated as unrestricted dimensions.
- Decision provenance including policy version and selected rule.
- Built-in `FreeOnly` cost enforcement; unknown cost is never assumed free.
- `IAiPolicyEngine.GetPolicySnapshot()` as the owned policy capture boundary.
- `AgentExecution.PolicyDecision` capture.
- `AgentExecutionSnapshot.EffectivePolicy` deep-cloned into the execution snapshot.
- Runtime policy enforcement after execution-target selection and before provider transport.
- Canonical policy persistence through File, SQL Server, and MySQL `IAiStore` implementations.
- Default runtime loading of the persisted policy when no explicit evaluator is injected.
- Tool invocation policy enforcement before executable handlers, with policy decision/provenance captured in `ToolExecutionResult`.
- Tool loops capture one effective policy evaluator for the lifetime of the loop.
- `PolicyDataAccessAuthorizer` composes HAgent policy with the host `IDataAccessAuthorizer`, ensuring policy restrictions are evaluated before host authorization while preserving host authority.
- `DataAuthorizationRequest` carries canonical `AgentIdentityContext` for policy composition.
- Canonical profile-level `AiResourceCapabilityPolicy` with `Inherit` / `Enabled` / `Disabled` states.
- Runtime-only `AgentRuntimeOverrides.ResourceCapabilityOverrides` layered above profile defaults.
- Deterministic effective resource resolution with exact-resource precedence, type-level fallback, runtime-over-profile precedence, and default `Enabled` behavior.
- `AgentExecutionSnapshot.EffectiveResourceCapabilities` captures the resolved resource state for an execution.
- Tool execution consumes the effective resource capability snapshot and blocks disabled tool resources before the executable handler.
- Deterministic Example coverage for engine behavior, persistence, runtime provider-call prevention, tool denial/approval/allow, policy-before-host-authorization, and resource capability resolution/persistence/tool gating.

### Remaining implementation slices

1. Integrate learning-promotion policy, review requirements, and typed approval transitions.
2. Add bounded human approval/defer workflow integration.
3. Add management UI for policy rules, scope, precedence, provenance, effective decisions, and resource capability state.
4. Expand deterministic Example verification and backend-specific live verification where configured.

### Architectural boundaries

The policy engine is provider-neutral and deterministic. It evaluates HAgent policy state; it does not authenticate principals, own host business authorization, or directly perform application side effects.

Resource enablement is a separate configuration capability layer. It determines whether an HAgent-owned or explicitly governed resource is enabled for a profile/runtime; it is not equivalent to provider capability discovery and it never grants host authorization.

Profile resource configuration is the default layer. Runtime `Inherit` / `Enabled` / `Disabled` overrides are runtime-only and do not mutate the persistent profile. Effective state is captured into the execution snapshot so later profile/runtime edits cannot alter an already-created execution.

Prompt/instruction text is never a policy enforcement mechanism. A model may request an action, but the appropriate runtime enforcement boundary must independently decide whether the action can occur.

For structured data access, HAgent policy can restrict a request before the host authorization callback is consulted, but a policy allow never grants application authorization. The host callback remains authoritative.

For tool execution, disabled resource capability state and `Deny`, `RequireApproval`, or `Defer` policy outcomes are enforced before the registered handler runs. A tool handler is never treated as an authorization boundary by itself.

## Verification rule

A 0.953 slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the result. Do not claim local build/test success unless it was actually performed.

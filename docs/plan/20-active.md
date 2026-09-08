# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.953 Unified Policy Engine — CURRENT

Phase 0.953 is the current foundational hardening milestone. HAgent now has provider-neutral policy contracts, deterministic evaluation, cost guarding, pre-transport runtime enforcement, effective-policy execution snapshots, canonical persistence, policy-gated tool invocation, and policy-first composition with host data authorization.

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
- Deterministic Example verification for engine behavior, snapshot isolation, persistence, runtime provider-call prevention, tool denial/approval/allow, and policy-before-host-authorization behavior.

### Remaining implementation slices

1. Integrate resource enablement and runtime `Inherit` / `Enabled` / `Disabled` semantics.
2. Integrate learning-promotion policy, review requirements, and typed approval transitions.
3. Add bounded human approval/defer workflow integration.
4. Add management UI for policy rules, scope, precedence, provenance, and effective decisions.
5. Expand deterministic Example verification and backend-specific live verification where configured.

### Architectural boundaries

The policy engine is provider-neutral and deterministic. It evaluates HAgent policy state; it does not authenticate principals, own host business authorization, or directly perform application side effects.

Prompt/instruction text is never a policy enforcement mechanism. A model may request an action, but the appropriate runtime enforcement boundary must independently decide whether the action can occur.

For structured data access, HAgent policy can restrict a request before the host authorization callback is consulted, but a policy allow never grants application authorization. The host callback remains authoritative.

For tool execution, `Deny`, `RequireApproval`, and `Defer` are enforced before the registered handler runs. A tool handler is never treated as an authorization boundary by itself.

## Verification rule

A 0.953 slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the result. Do not claim local build/test success unless it was actually performed.

# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.953 Unified Policy Engine — CURRENT

Phase 0.953 is the current foundational hardening milestone. HAgent now has provider-neutral policy contracts, deterministic evaluation, cost guarding, pre-transport runtime enforcement, and effective-policy execution snapshots. The remaining work completes policy integration across persistence, authorization, resources, learning, approvals, and UI.

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
- Deterministic Example verification for engine behavior, snapshot isolation, runtime policy capture, and provider-call prevention under denial.

### Next implementation slices

1. Persist policy configuration through File, SQL Server, and MySQL without mixing it into provider/agent transport configuration.
2. Integrate host authorization callbacks at HAgent enforcement boundaries without replacing host authority.
3. Apply policy to tool invocation and resource access before side effects occur.
4. Integrate resource enablement and runtime `Inherit` / `Enabled` / `Disabled` semantics.
5. Add learning-promotion policy, review requirements, and typed approval transitions.
6. Add bounded human approval/defer workflow integration.
7. Add management UI for policy rules, scope, precedence, provenance, and effective decisions.
8. Expand deterministic Example coverage for persistence, authorization callbacks, tool denial, resource gating, inheritance, runtime overrides, approval, and recovery.

### Architectural boundaries

The policy engine is provider-neutral and deterministic. It evaluates HAgent policy state; it does not authenticate principals, own host business authorization, or directly perform application side effects.

Prompt/instruction text is never a policy enforcement mechanism. A model may request an action, but the appropriate runtime enforcement boundary must independently decide whether the action can occur.

## Verification rule

A 0.953 slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the result. Do not claim local build/test success unless it was actually performed.

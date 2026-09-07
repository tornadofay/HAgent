# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.953 Unified Policy Engine — CURRENT

Phase 0.953 is the current foundational hardening milestone. HAgent now has provider-neutral policy contracts, deterministic evaluation, cost guarding, and pre-transport runtime enforcement. The remaining work completes policy integration across persistence, authorization, resources, learning, approvals, and effective snapshots.

### Objective

Create one coherent policy boundary for HAgent decisions without making the model, prompt, authentication provider, or host business authorization responsible for enforcement.

### Completed in this milestone so far

- Versioned `AiPolicySet` and scoped `AiPolicyRule` contracts.
- `Allow`, `Deny`, `RequireApproval`, `Defer`, and `NotApplicable` outcomes.
- Evaluation contexts carrying identity, tenant/workspace, agent/runtime/execution, resource/tool, provider/target, cost, and bounded attributes.
- Deterministic rule precedence and conflict resolution.
- Decision provenance including policy version and selected rule.
- Built-in `FreeOnly` cost enforcement; unknown cost is never assumed free.
- `AgentExecution.PolicyDecision` capture.
- Runtime policy enforcement after execution-target selection and before provider transport.
- Deterministic Example verification for engine behavior and provider-call prevention under denial.

### Next implementation slices

1. Capture an immutable effective policy state/version in the execution snapshot, including the policy inputs that governed the run.
2. Persist policy configuration through File, SQL Server, and MySQL without mixing it into provider/agent transport configuration.
3. Integrate host authorization callbacks at HAgent enforcement boundaries without replacing host authority.
4. Apply policy to tool invocation and resource access before side effects occur.
5. Integrate resource enablement and runtime `Inherit` / `Enabled` / `Disabled` semantics.
6. Add learning-promotion policy, review requirements, and typed approval transitions.
7. Add bounded human approval/defer workflow integration.
8. Add management UI for policy rules, scope, precedence, provenance, and effective decisions.
9. Expand deterministic Example coverage for inheritance, resource access, runtime overrides, persistence, tool denial, approval, and recovery.

### Architectural boundaries

The policy engine is provider-neutral and deterministic. It evaluates HAgent policy state; it does not authenticate principals, own host business authorization, or directly perform application side effects.

Prompt/instruction text is never a policy enforcement mechanism. A model may request an action, but the appropriate runtime enforcement boundary must independently decide whether the action can occur.

## Verification rule

A 0.953 slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the result. Do not claim local build/test success unless it was actually performed.

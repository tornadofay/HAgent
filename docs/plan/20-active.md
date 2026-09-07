# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.953 Unified Policy Engine — CURRENT

Phase 0.953 is the current foundational slice. HAgent now has provider-neutral policy contracts and deterministic evaluation; subsequent work will connect policy to runtime enforcement, persistence, learning, resource enablement, and human approval.

### Objective

Create one coherent policy boundary for HAgent decisions without making the model, prompt, authentication provider, or host business authorization responsible for enforcement.

### Implemented

- Versioned `AiPolicySet` and scoped `AiPolicyRule` contracts.
- `Allow`, `Deny`, `RequireApproval`, `Defer`, and `NotApplicable` outcomes.
- Evaluation contexts carrying identity, tenant/workspace, agent/runtime/execution, resource/tool, provider/target, cost, and bounded attributes.
- Deterministic rule precedence and conflict resolution.
- Decision provenance including policy version and selected rule.
- Built-in `FreeOnly` cost enforcement; unknown cost is never assumed free.
- Deterministic Example verification for precedence, approval, denial, scope matching, cost restrictions, provenance, and tie-breaking.

### Next slices

- Integrate host authorization callbacks at HAgent enforcement boundaries without replacing host authority.
- Add effective policy state to execution/runtime snapshots.
- Persist policy sets through HAgent File, SQL Server, and MySQL storage.
- Apply policy to tool invocation and provider/execution-target admission.
- Add resource enablement and runtime `Inherit` / `Enabled` / `Disabled` policy integration.
- Add learning-promotion policy and approval requirements.
- Add bounded human approval/defer workflow integration.
- Add management UI for policy scope, precedence, provenance, and effective decisions.
- Add deterministic Example coverage for inherited policy, resource access, runtime overrides, persisted policies, tool denial, approval flow, and runtime enforcement.

### Architectural boundaries

The policy engine is provider-neutral and deterministic. It evaluates HAgent policy state; it does not authenticate principals, own host business authorization, or directly perform application side effects.

Prompt/instruction text is never a policy enforcement mechanism. A model may request an action, but the appropriate runtime enforcement boundary must independently decide whether the action can occur.

## Verification rule

A 0.953 slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the result. Do not claim local build/test success unless it was actually performed.

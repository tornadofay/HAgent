# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.953 Unified Policy Engine — CURRENT

Phase 0.953 is the current foundational hardening milestone. HAgent now has provider-neutral policy contracts, deterministic evaluation, cost guarding, pre-transport runtime enforcement, effective-policy execution snapshots, canonical persistence, policy-gated tool invocation, policy-first composition with host data authorization, profile/runtime resource capability resolution, typed learning-promotion policy evaluation with explicit candidate approval transitions, and a bounded approval/defer workflow.

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
- Default runtime loading of the persisted policy when no explicitly injected evaluator is supplied.
- Tool invocation policy enforcement before executable handlers, with policy decision/provenance captured in `ToolExecutionResult`.
- Tool loops capture one effective policy evaluator for the lifetime of the loop.
- `PolicyDataAccessAuthorizer` composes HAgent policy with the host `IDataAccessAuthorizer`, ensuring policy restrictions are evaluated before host authorization while preserving host authority.
- `DataAuthorizationRequest` carries canonical `AgentIdentityContext` for policy composition.
- Canonical profile-level `AiResourceCapabilityPolicy` with `Inherit` / `Enabled` / `Disabled` states.
- Runtime-only `AgentRuntimeOverrides.ResourceCapabilityOverrides` layered above profile defaults.
- Deterministic effective resource resolution with exact-resource precedence, type-level fallback, runtime-over-profile precedence, and default `Enabled` behavior.
- `AgentExecutionSnapshot.EffectiveResourceCapabilities` captures the resolved resource state for an execution.
- Tool execution consumes the effective resource capability snapshot and blocks disabled tool resources before the executable handler.
- `AiLearningPromotionRequest` maps candidate type, proposed scope, confidence/evidence, provenance, contradiction, retention, source identity, and optional learning mode into the unified `learning.promote` policy operation.
- `AiLearningPromotionPolicy` evaluates learning promotion through the existing `IAiPolicyEngine`; no parallel learning authorization evaluator exists.
- `AiLearningCandidate` provides explicit `Proposed`, `PendingReview`, `Approved`, `Rejected`, and `Promoted` states with guarded transitions. `Allow`, `RequireApproval`, `Defer`, and `Deny` map to the corresponding promotion lifecycle states.
- `AiApprovalRequest` and `IAiApprovalWorkflow` provide bounded process-local approval/deferral state for policy outcomes requiring review.
- Approval requests preserve operation/resource identity, HAgent and host correlation, agent/runtime/execution identity where known, requester identity, policy reason, and terminal responder metadata.
- Approval resolution is explicit and terminal; resolving an approval never executes the protected operation or silently resumes an execution.
- Deterministic Example coverage for engine behavior, persistence, runtime provider-call prevention, tool denial/approval/allow, policy-before-host-authorization, resource capability resolution/persistence/tool gating, learning promotion/review transitions, and approval/deferral workflow lifecycle.
- WinForms policy management surface with policy-rule editing, explicit precedence fields, effective-decision inspection, and agent resource-capability inspection.
- WinForms configuration UI was structurally refactored so `AISettingsForm` is a small composition shell and feature-owned pages live under `UI/Configuration/<Area>/`.
- Providers, Agents, Tools, Policy, Overview, and About are independently owned configuration pages; shared dependencies/state flow through `ConfigurationContext`.
- List-oriented configuration pages use a shared three-row layout separating header, action bar, and content to prevent docking overlap.
- Legacy reflection/control-tree navigation injection was removed. Permissions, Storage, Storage Test, and Policy are now explicitly composed by the configuration shell.

### Remaining implementation slices

1. Verify the refactored configuration UI locally on .NET Framework 4.8.1 and .NET 9.
2. Expand deterministic Example verification and backend-specific live verification where configured.

### Architectural boundaries

The policy engine is provider-neutral and deterministic. It evaluates HAgent policy state; it does not authenticate principals, own host business authorization, or directly perform application side effects.

Resource enablement is a separate configuration capability layer. It determines whether an HAgent-owned or explicitly governed resource is enabled for a profile/runtime; it is not equivalent to provider capability discovery and it never grants host authorization.

Profile resource configuration is the default layer. Runtime `Inherit` / `Enabled` / `Disabled` overrides are runtime-only and do not mutate the persistent profile. Effective state is captured into the execution snapshot so later profile/runtime edits cannot alter an already-running execution.

Learning promotion is another enforcement use of the same policy boundary. Typed learning metadata is supplied as policy context; policy may permit, require review, defer, or deny promotion. Candidate lifecycle transitions are explicit and terminal states cannot be bypassed. Approved candidates still require a separate repository/promotion operation; no candidate object directly mutates authoritative Knowledge or Skills.

Approval and deferral are explicit review-state boundaries. `RequireApproval` and `Defer` may create bounded pending requests, but approval state is not authorization by itself and does not silently resume execution. Full durable intervention, pause/resume, cancellation, and operator lifecycle semantics remain owned by the later human-intervention foundation.

The WinForms configuration shell is intentionally separate from feature implementation. `AISettingsForm` composes pages and navigation; feature-specific behavior belongs to its feature directory. See `docs/architecture/91-winforms-configuration-maintenance.md` for the maintenance protocol.

Prompt/instruction text is never a policy enforcement mechanism. A model may request an action, but the appropriate runtime enforcement boundary must independently decide whether the action can occur.

For structured data access, HAgent policy can restrict a request before the host authorization callback is consulted, but a policy allow never grants application authorization. The host callback remains authoritative.

For tool execution, disabled resource capability state and `Deny`, `RequireApproval`, or `Defer` policy outcomes are enforced before the registered handler runs. A tool handler is never treated as an authorization boundary by itself.

## Verification rule

A 0.953 slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the result. Do not claim local build/test success unless it was actually performed.

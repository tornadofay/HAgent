# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.959 Human-in-the-Loop and Intervention — CURRENT

Phase 0.959 is the current intervention foundation. HAgent now has a canonical provider-neutral intervention request/lifecycle contract built on the bounded approval/defer boundary from 0.953, but runtime application of pause/resume/cancel and broader intervention targets remains to be implemented.

### Objective

Allow authorized humans or host applications to inspect and control active HAgent work without creating a bypass around execution, policy, permissions, authorization, capabilities, budgets, cancellation, or host validation.

### Completed in this milestone so far

- Canonical `AiInterventionRequest` with request identity, target kind, requested action, lifecycle status, execution/resource context, HAgent/host correlation, requester/responder identity, policy reason, and resolution metadata.
- `IAiInterventionWorkflow` and bounded in-memory implementation with cloned request boundaries and terminal-state protection.
- Tool execution creates canonical intervention requests for `RequireApproval` and `Defer`, with explicit tool target and requested-action semantics.
- Tool execution results expose the intervention request.
- Deterministic Example approval/defer verification uses the canonical intervention API.
- Obsolete approval-only contract/facade files were removed in favor of the intervention model.

### Remaining implementation slices

1. Integrate intervention application at execution and tool boundaries without creating a second execution engine.
2. Add deterministic concurrency-safe state transitions and stale-request handling.
3. Extend intervention to plan steps, goals, learning candidates, and consequential actions.
4. Add durable intervention persistence after lifecycle semantics stabilize.
5. Expose pending/historical intervention state through management UI and diagnostics.
6. Expand deterministic Example verification for pause/resume, cancellation, concurrency, stale requests, and all supported target/action transitions.
7. Complete .NET Framework 4.8.1 and .NET 9 verification plus backend-specific live verification where configured.

### Architectural boundaries

The intervention boundary is provider-neutral and does not authenticate principals or replace host authorization. Policy decides when an intervention/approval boundary is required; intervention state records and applies the authorized control through the owning runtime boundary.

Approval or intervention acceptance never directly executes a protected tool/provider call, silently resumes work, grants host authorization, or creates capabilities. The target runtime must still enforce policy, permissions, capability, budget, cancellation, and host-side validation.

The canonical lifecycle, target/action semantics, concurrency rules, persistence boundary, and management UI requirements are defined in `docs/architecture/92-human-intervention.md`.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

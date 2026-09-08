# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.959 Human-in-the-Loop and Intervention — CURRENT

Phase 0.959 is the current intervention foundation. The canonical provider-neutral intervention contract has now been integrated with the runtime execution boundary, durable file persistence, management UI, and deterministic lifecycle verification. Final build/test/Example verification remains the release gate for this slice.

### Objective

Allow authorized humans or host applications to inspect and control active HAgent work without creating a bypass around execution, policy, permissions, authorization, capabilities, budgets, cancellation, or host validation.

### Implemented in this milestone

- Canonical `AiInterventionRequest` now carries target revision and optimistic-concurrency version metadata in addition to request identity, lifecycle status, execution/resource context, HAgent/host correlation, requester/responder identity, policy reason, and resolution metadata.
- `IAiInterventionWorkflow` now supports search, explicit resolution, expiration, completion, and asynchronous waiting, with both in-memory and store-backed implementations.
- Runtime-owned `AiInterventionCoordinator` applies approved interventions through target handlers and exposes the extension point for non-execution targets.
- Execution targeting supports inspect, pause, resume, and cancel through the canonical runtime, with control-revision stale protection and first-terminal-state protection.
- Policy `RequireApproval` and `Defer` now suspend execution through the same intervention boundary; an approved gate is consumed without recursively requesting the same approval, while a later deny remains authoritative.
- Intervention cancellation is linked to provider execution cancellation so late provider results cannot become authoritative after the runtime has been cancelled.
- Host authorization remains a callback boundary; intervention metadata, handlers, and authorization callbacks are not persisted.
- `IAiInterventionStore` and `StoreBackedAiInterventionWorkflow` separate durable state from runtime executable handlers. `FileAiInterventionStore` supplies the local durable implementation used by the WinForms configuration host.
- WinForms configuration now includes an **Interventions** management/diagnostics surface reading the canonical workflow state.
- `HAgent.Example` now exposes `--verify-intervention` for deterministic approval, pause/resume, cancellation, stale-state, lifecycle, and durable-persistence verification.
- A phase-specific GitHub Actions workflow verifies solution build, unit tests, and Example intervention verification.

### Verification checkpoint

The implementation has been committed on `codex/phase-0.959-intervention` and is under pull request verification. No build/test/Example result is considered verified until the corresponding GitHub Actions run completes successfully.

### Remaining implementation slices

1. Complete build/test/Example verification on .NET 9 and the solution's supported targets.
2. Fix any defects revealed by verification before marking 0.959 complete.
3. After this slice is verified, close 0.959 and advance the active plan to 0.9591 Goal/Plan Persistence/Recovery.
4. Backend-specific live verification remains environment-dependent; no provider-specific intervention logic belongs in Core.

### Architectural boundaries

The intervention boundary is provider-neutral and does not authenticate principals or replace host authorization. Policy decides when an intervention/approval boundary is required; intervention state records and applies the authorized control through the owning runtime boundary.

Approval or intervention acceptance never directly executes a protected tool/provider call, silently resumes work without the owning runtime, grants host authorization, or creates capabilities. The target runtime must still enforce policy, permissions, capability, budget, cancellation, and host-side validation.

Tool approvals remain gates on the existing tool execution path rather than a second tool engine. Plan-step, goal, learning-candidate, and consequential-action targets use the same handler extension point and are not duplicated in UI or Core runtime logic.

The canonical lifecycle, target/action semantics, concurrency rules, persistence boundary, and management UI requirements are defined in `docs/architecture/92-human-intervention.md`.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example verification passes, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

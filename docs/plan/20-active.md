# Active implementation plan

Only the current implementation milestone belongs here. Completed implementation history is recorded in the ordered roadmap under `docs/roadmap/`; future work does not belong here.

## 0.959 Human-in-the-Loop and Intervention — CURRENT

Phase 0.959 is the current intervention foundation. HAgent now has a canonical provider-neutral intervention request/lifecycle contract built on the bounded approval/defer boundary from 0.953. Execution intervention control and execution-target concurrency/stale-state hardening are implemented; the hardening slice remains unverified in this connected environment because there is no executable repository build/test workflow and no local checkout.

### Objective

Allow authorized humans or host applications to inspect and control active HAgent work without creating a bypass around execution, policy, permissions, authorization, capabilities, budgets, cancellation, or host validation.

### Completed in this milestone so far

- Canonical `AiInterventionRequest` with request identity, target kind, requested action, lifecycle status, execution/resource context, HAgent/host correlation, requester/responder identity, policy reason, target state/version evidence, and resolution metadata.
- `IAiInterventionWorkflow` and bounded in-memory implementation with cloned request boundaries and terminal-state protection.
- Intervention lifecycle requires `Pending -> Approved -> Completed` for an accepted intervention; stale application may terminate as `Approved -> Expired`; terminal requests cannot be resolved again.
- Tool execution creates canonical intervention requests for `RequireApproval` and `Defer`, with explicit tool target and requested-action semantics.
- Tool execution results expose the intervention request.
- Deterministic Example approval/defer verification uses the canonical intervention API.
- Obsolete approval-only contract/facade files were removed in favor of the intervention model.
- `DefaultAgentRuntime` owns the canonical execution intervention coordinator and links intervention cancellation into the existing execution cancellation path.
- Execution pause/resume/cancel requests use the shared `HAgentClient` intervention workflow and do not introduce a second execution engine.
- `HAgentClient.ExecutionChanged` and execution intervention APIs expose the host-facing control boundary through public APIs.
- The `EXECUTION INTERVENTION` Example passed local verification on 2026-09-08, covering pause/resume/cancel lifecycle, public execution events, terminal cancellation, and late provider response protection.
- Execution intervention requests now capture the observed control state and monotonic control-state version.
- Competing execution interventions are serialized per execution and stale requests resolve deterministically to `Expired`.
- Duplicate responder resolution is protected by request lifecycle state, and a stale request remains queryable with its responder and stale reason.
- A deterministic `INTERVENTION HARDENING` Example scenario has been added for terminal staleness, conflicting concurrent requests, paused-state blocking, and duplicate responders.

### Run-sized execution plan

Only one slice is **CURRENT** at a time. Each slice must reach a verified checkpoint before the next slice begins.

1. **Complete — Execution control boundary**
   - Scope: Integrate intervention application into the existing canonical runtime/execution lifecycle for pause, resume, and cancellation; preserve the existing execution engine and terminal-state rules.
   - Entry: Canonical intervention workflow and execution lifecycle contracts exist.
   - Implementation state: Complete in source; focused Example verification added.
   - Verification state: **VERIFIED** — user executed the `EXECUTION INTERVENTION` Example on 2026-09-08 and all expected lifecycle, cancellation, and late-response assertions passed.
   - Completion: Controlled execution can be paused/resumed/cancelled through the intervention boundary without a second execution engine, and focused deterministic verification passes in an executable environment.

2. **CURRENT — Concurrency and stale-state hardening**
   - Scope: Make intervention state transitions deterministic under concurrent requests, duplicate responders, late provider completion, retirement/shutdown teardown, and already-terminal executions.
   - Entry: Slice 1 passes its focused lifecycle verification.
   - Implementation state: Complete in source; deterministic Example verification added.
   - Verification state: **BLOCKED** — no executable repository build/test workflow is available through the connected environment, and local repository checkout is unavailable in this session.
   - Completion: concurrency/stale-request tests pass and late results cannot overwrite terminal outcomes.
   - Next smallest step after verification: additional intervention targets.

3. **Additional intervention targets**
   - Scope: Extend the same canonical intervention mechanism to plan steps, goals, learning candidates, and consequential actions where defined by the architecture.
   - Entry: lifecycle/concurrency semantics are stable.
   - Completion: each supported target/action pair has explicit authorization/policy semantics and focused deterministic verification.

4. **Durable intervention persistence**
   - Scope: Persist intervention lifecycle/history through the existing canonical storage architecture without creating a parallel persistence model.
   - Entry: lifecycle and target semantics are stable.
   - Completion: persistence/reload, ownership, and terminal-state behavior are verified against the supported storage contracts.

5. **Management UI and diagnostics**
   - Scope: Expose pending/history intervention state through the designated configuration/management surfaces and diagnostics while keeping UI as a consumer of the canonical contracts.
   - Entry: persistence and lifecycle contracts are stable.
   - Completion: UI opens/loads, displays correct state, issues authorized controls, and handles stale/completed requests safely in the supported WinForms targets.

6. **Example coverage expansion**
   - Scope: Add deterministic public-API Example scenarios for pause/resume, cancellation, concurrency, stale requests, target/action transitions, persistence, and failure boundaries.
   - Entry: implementation and UI contracts are stable enough to exercise end-to-end.
   - Completion: all required scenarios are reproducible and the Example host remains organized by feature.

7. **Final framework/backend verification**
   - Scope: Run the supported .NET Framework 4.8.1 and .NET 9 verification plus backend-specific live verification where configured.
   - Entry: all implementation slices and Example verification are complete.
   - Completion: actual builds/tests/examples have been executed and the authoritative documentation records the verified milestone state.

### Architectural boundaries

The intervention boundary is provider-neutral and does not authenticate principals or replace host authorization. Policy decides when an intervention/approval boundary is required; intervention state records and applies the authorized control through the owning runtime boundary.

Approval or intervention acceptance never directly executes a protected tool/provider call, silently resumes work, grants host authorization, or creates capabilities. The target runtime must still enforce policy, permissions, capability, budget, cancellation, and host-side validation.

The canonical lifecycle, target/action semantics, concurrency rules, persistence boundary, and management UI requirements are defined in `docs/architecture/92-human-intervention.md`.

## Verification rule

A slice becomes complete only after the implementation exists, matching deterministic Example or focused test verification passes locally, and the authoritative architecture/roadmap documentation reflects the verified result. Do not claim local build/test success unless it was actually performed.

## Run rule

Do not implement multiple numbered slices in one run merely because they are related. Finish the current slice, verify it, update this file, and only then select the next slice.

# Phase 0.96 Implementation — Capability-Aware Execution Foundation

## Current slice

This slice establishes the provider-neutral decision boundary needed before capability-aware transport admission is integrated into the runtime.

### Implemented

- `AiExecutionTarget` as the concrete provider/account/project/endpoint/model deployment identity.
- `LogicalModelId` so multiple concrete targets can represent one logical model without becoming equivalent executions.
- `CapabilityRequirementStrength` with Required / Preferred / Optional / Forbidden semantics.
- `AiCapabilityRequirements` as request/agent-side requirements independent of provider transport.
- `AiExecutionSelectionPolicy` with Auto / Preferred / Fixed selection modes.
- Explicit Fail / TryNextCandidate / Wait fallback policy values.
- Independent FreeOnly / FreePreferred / NoRestriction cost policy.
- `IExecutionPlanner` and deterministic `DefaultExecutionPlanner`.
- `AiExecutionPlan` plus per-target diagnostics for acceptance/rejection and score reasons.
- Generic quota/rate dimensions for request count, token usage, concurrency, audio duration, image count, bytes, and spend.
- `AiQuotaLimit` / `AiQuotaPolicy` for arbitrary rolling windows.
- `InMemoryAiQuotaAdmission` with atomic target-scoped reservations and release/actual-usage reconciliation.
- Deterministic Example verification for execution planning and quota admission.

## Deliberate non-goals for the current slice

Provider transport, provider discovery, persistent/shared admission, provider-enforced scope reconciliation, operational 429 feedback, long-running execution policy, and capability-aware runtime integration are not yet wired into the execution path.

The obsolete provider/model properties on `AiAgent` are also not being preserved through compatibility wrappers. They will be replaced as part of the single coherent runtime/configuration integration step.

## Next slice

Integrate the planner with the canonical `AgentExecutionRequest` boundary, replace reusable `AiAgent` provider/model binding with execution-selection policy plus capability requirements, and make runtime/provider transport consume the selected `AiExecutionTarget`. Then connect admission to that path and add provider discovery, capability evidence refresh, operational capacity, and 429 feedback.

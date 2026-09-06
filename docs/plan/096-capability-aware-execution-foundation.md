# Phase 0.96 Implementation — Capability-Aware Execution Foundation

## Current slice

This slice establishes and begins integrating the provider-neutral execution decision boundary before capability-aware transport admission is fully wired into provider discovery and operational state.

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
- `AgentExecutionRequest` support for execution selection and capability requirements.
- `AgentExecutionSnapshot` cloning of execution policy and capability requirements so runtime execution does not share mutable policy state.
- `DefaultAgentRuntime` planner invocation before provider transport.
- Structured-output requests automatically requiring the `StructuredOutput` capability.
- `InMemoryAiStore` cloning of execution selection and capability requirements.
- Deterministic Example verification for execution planning, quota admission, and runtime concurrency through the new execution-planner path.

### Verified by user

- Execution target planning contract test passed.
- Quota admission contract test passed.
- Runtime instance lifecycle/isolation contract test passed.

## Current correction

The original runtime concurrency Example was configuration-dependent: it selected the currently configured UI agent and assumed its legacy primary provider fields remained the execution source. That is no longer a valid verification strategy for the redesigned runtime.

The test has been changed to construct an explicit in-memory provider and agent and execute two independent runtime instances concurrently through `DefaultAgentRuntime` and the new planner boundary. This keeps the test deterministic and independent of user configuration.

## Remaining 0.96 work

Provider discovery, capability evidence refresh, concrete target catalog persistence, operational permission/capacity state, proactive admission integration into the real provider execution path, provider 429 feedback, long-running request policy, stale-result handling across planner retries/fallbacks, complete removal of obsolete reusable-agent provider/model binding, and management UI integration remain to be completed.

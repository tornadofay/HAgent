# Phase 0.96 Implementation — Execution Planning Foundation

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
- Example verification covering multiple providers for one logical model, required/preferred/forbidden capabilities, known free/paid/unknown cost, Preferred selection, and Fixed capability enforcement.

## Deliberate non-goals for this slice

This slice does not yet send provider requests, reserve quota, infer rate limits, perform provider discovery, or remove the obsolete provider/model properties from `AiAgent`. Those changes will be made as one coherent runtime/configuration integration rather than through compatibility shims.

## Next slice

Integrate the planner with the canonical `AgentExecutionRequest` boundary, replace the reusable `AiAgent` provider/model binding with `AiExecutionSelectionPolicy` plus requirements, and make runtime/provider transport consume the selected `AiExecutionTarget`. Then add normalized quota/rate/capacity admission before transport.

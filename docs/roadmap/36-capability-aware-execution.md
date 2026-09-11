# Phase 0.96 — Capability-Aware Execution

## Status

**Planned major execution foundation after 0.96.x and before 0.97.**

## Purpose

Select and admit the best currently usable concrete execution target for each request across heterogeneous providers, models, accounts, endpoints, capabilities, quotas, rate limits, concurrency capacity, health, latency, and cost policy.

Phase 0.96 is the **single concrete execution-selection authority** in HAgent.

The cognitive layer may request reasoning requirements, but it must never choose provider/model names directly.

## Core separation

```text
Agent Profile
    = what the agent requires and prefers

Provider
    = provider/service integration

Logical Model
    = provider-independent identity when reliably known

Execution Target
    = concrete provider + account/project/endpoint + model/deployment

Capability
    = what the target can do

Constraint
    = limits on the requested operation

Operational State
    = quota / rate / concurrency / health / availability

Cost State
    = Free / FreeWithinQuota / Paid / Unknown

Execution Planner
    = selects and admits the best compatible target
```

## Delivery slices

### Slice 1 — Execution-target model

- Define normalized target identity.
- Separate Provider, Model, and concrete target.
- Preserve provider-native identifiers/deployment metadata.
- Support multiple targets for the same logical model.

### Slice 2 — Capability and constraint evaluation

- Normalize capabilities as `Supported`, `Unsupported`, or `Unknown`.
- Keep capability separate from permission, health, quota, capacity, and cost.
- Support request requirements with required/preferred/optional/forbidden semantics.
- Cover structured output, tool use, reasoning, modalities, streaming, embeddings, and extensible future capabilities.
- Distinguish native support from emulated/degraded behavior.
- Never treat unknown capability as supported by default.

### Slice 3 — Discovery evidence integration

- Consume provider adapter discovery from 0.9592.
- Preserve capability provenance, confidence, observation time, and expiration.
- Accept provider metadata, documentation evidence, controlled probes, successful execution evidence, response metadata, and explicit host overrides.
- Keep discovery evidence replaceable and provider-neutral.

### Slice 4 — Cost and policy selection

Support system/agent/runtime policy:

```text
FreeOnly
FreePreferred
NoRestriction
```

Support Agent selection mode:

```text
Auto
Preferred
Fixed
```

Rules:

- `FreeOnly` cannot treat `Unknown` cost as free.
- `FreePreferred` prefers eligible free targets and may use paid fallback only when policy permits it.
- `Fixed` still enforces capability, authorization, quota, capacity, health, and constraints.
- Agent preferences never become permanent provider bindings.

### Slice 5 — Rate, quota, and concurrency admission

Use generic resource dimensions rather than provider-specific hard-coded limits.

Minimum dimensions:

```text
request count
tokens in
tokens out
total tokens
concurrency
```

Support arbitrary provider-defined windows and enforcement scopes.

Implement:

- proactive admission;
- atomic reservations for concurrent requests;
- bounded waiting;
- `Wait`, `TryNextCandidate`, `Fail`, or policy-approved degradation;
- reconciliation after execution;
- partial/unknown usage accounting;
- provider 429/quota signals as operational feedback.

A target with quota available may still have no execution capacity.

### Slice 6 — Health, latency, fallback, and long-running execution

- Track availability/health separately from capability.
- Track latency separately from quota/rate state.
- Support legitimately long-running inference without false failure.
- Preserve cancellation, timeout, and stale-result protection while waiting, executing, or falling back.
- Avoid permanent blacklisting from transient failures.
- Define explicit fallback/degradation behavior.

### Slice 7 — Execution planning and diagnostics

For each candidate expose a normalized assessment:

```text
Target identity
Compatibility
Capability evidence
Constraint result
Permission state
Quota/rate state
Capacity state
Health/availability
Cost state
Estimated latency
Wait-until (optional)
Degradation option (optional)
Score/ranking data
Decision reason
```

The assessment is diagnostic data and should be consumable by hosts/UI without provider-specific knowledge.

### Slice 8 — Management UI and verification

The management surface must show effective target capability, limits, cost, quota/rate/capacity, availability, and compatibility with the active request.

Deterministic verification must cover:

- same logical model through multiple providers;
- required/preferred/optional/forbidden capabilities;
- unknown capability metadata;
- incompatible manual selection;
- native vs degraded structured output;
- proactive rate limiting;
- token/request windows;
- atomic concurrent reservations;
- 429 feedback;
- long-running requests;
- cancellation and timeout;
- stale-result protection;
- target fallback;
- Auto/Preferred/Fixed selection;
- FreeOnly/FreePreferred/NoRestriction behavior.

## Architectural rules

1. 0.96 is the sole concrete execution-selection layer.
2. 0.97 cognitive strategies never select providers/models directly.
3. Rate limiting is proactive admission, not only retry logic.
4. Capability is not permission, health, quota, capacity, or cost.
5. Unknown information stays unknown.
6. Provider-specific logic remains in adapters.
7. Agent profiles express intent and preferences, not transport bindings.
8. Every execution uses an immutable effective snapshot of relevant configuration.
9. Fallback never bypasses authorization or capability requirements.
10. Independent runtime agents may call the planner concurrently without sharing mutable runtime identity/state.

## Dependency chain

```text
0.9592 provider/adapters
        ↓
0.96.x configuration/storage
        ↓
0.96 capability-aware execution
        ↓
0.97 persistent cognition
```

## Exit criterion

For every execution request HAgent can deterministically identify compatible targets, enforce capability/policy/cost/quota/capacity/health constraints, reserve required capacity, choose or wait/fallback according to explicit rules, execute through provider-neutral boundaries, and explain the resulting selection or rejection without embedding provider logic into Core or cognition.

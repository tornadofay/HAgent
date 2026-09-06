# Phase 0.9592 — Provider Ecosystem and Adapter Lifecycle

## Status

**Planned provider-platform foundation before and alongside Phase 0.96.**

## Goal

Mature the provider adapter boundary so HAgent can support many providers, API variants, models, modalities, discovery mechanisms, and provider API versions without leaking provider-specific behavior into HAgent.Core.

## Requirements

1. [ ] Define a complete provider adapter lifecycle including registration, validation, initialization, refresh, health, disablement, replacement, and retirement.
2. [ ] Separate transport capability from discovery, usage, quota/rate, health, and other provider-specific data sources.
3. [ ] Define normalized adapter contracts for model discovery, capability discovery, usage, rate/quota information, health, and supported execution features where available.
4. [ ] Allow one provider integration to expose multiple models and task families without hard-coded model assumptions in Core.
5. [ ] Preserve provider-native identifiers, API versions, deployment identifiers, and endpoint metadata alongside normalized identities.
6. [ ] Support partial provider implementations: a provider may support execution while exposing incomplete discovery or quota telemetry.
7. [ ] Represent unavailable/unknown provider features explicitly instead of manufacturing defaults.
8. [ ] Define adapter version/compatibility metadata so provider API changes can be handled deliberately.
9. [ ] Support provider deprecation/retirement without corrupting persisted agent configuration or historical execution records.
10. [ ] Keep provider-specific retry, response, streaming, authentication, and error handling inside adapters where appropriate.
11. [ ] Ensure adapter instances are safe for concurrent use or explicitly scoped when they are not.
12. [ ] Ensure provider credentials are supplied through the current simple encrypted provider-configuration mechanism; this phase must not introduce a separate secret-vault architecture.
13. [ ] Add deterministic fake-provider verification for complete discovery, partial discovery, unsupported operations, provider/API version changes, adapter replacement, health changes, and concurrent usage.

## Architectural outcome

```text
Provider Configuration
        ↓
Provider Adapter
 ├── execution
 ├── discovery
 ├── capabilities
 ├── usage/quota
 ├── health
 └── provider-specific metadata
        ↓
Normalized HAgent contracts
        ↓
Execution Planner / Runtime
```

HAgent.Core remains provider-neutral; provider-specific knowledge stays behind adapter boundaries.
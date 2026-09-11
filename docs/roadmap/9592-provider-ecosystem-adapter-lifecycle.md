# Phase 0.9592 — Provider Ecosystem and Adapter Lifecycle

## Status

**Planned before 0.96 capability-aware execution.**

## Purpose

Provide a clean provider-adapter boundary so HAgent can use multiple providers, API variants, models, and discovery sources without leaking provider-specific behavior into `HAgent.Core` or making provider integration larger than necessary.

This phase is a provider-platform foundation, not a provider marketplace.

## V1 provider model

```text
Provider Configuration
        ↓
Provider Adapter
 ├── execution
 ├── model discovery (when available)
 ├── capability discovery (when available)
 ├── quota/rate/usage telemetry (when available)
 ├── health/availability
 └── provider-native metadata
        ↓
Normalized HAgent contracts
        ↓
0.96 Execution Planner
```

Unknown information remains `Unknown`; adapters do not invent capabilities or limits.

## 2026-09-11 use-case audit

The intended desktop-application and HWorld use cases justify the core of 0.9592. HWorld explicitly intends to support different providers/models in one world, while HAgent's host architecture requires provider/model discovery and normalized execution-target information before 0.96 can make capability-aware selections. The current provider-limit/rate-capacity problems also make operational evidence a concrete requirement rather than a hypothetical platform feature.

| 0.9592 surface | Use-case result | V1 decision |
|---|---|---|
| Multiple provider adapters | Required when different agents/actors use different providers/models concurrently. | Keep |
| Adapter registration/use/disable/retirement | Required for desktop configuration and for hosts that need to remove an unavailable provider without deleting historical identity. | Keep |
| Model/target discovery | Required to avoid hard-coding every provider's model catalog and to feed 0.96 target assessment. Partial/no discovery must remain supported. | Keep |
| Capability discovery/evidence | Required for 0.96 capability-aware selection; unknown capability must remain unknown. | Keep |
| Provider-native identities/metadata | Required because the same logical model may exist through multiple providers/accounts/deployments. | Keep |
| Quota/rate/usage telemetry | Directly justified by real provider limits, shared capacity, and 429/rate-limit behavior encountered in HWorld-oriented execution. | Keep |
| Health/availability evidence | Needed to avoid repeatedly selecting unavailable execution targets. | Keep |
| Adapter/API compatibility and replacement | Needed for deliberate adapter replacement while preserving historical provider/target identity and configuration semantics. | Keep, bounded |
| Cost information | Needed by 0.96's FreeOnly/FreePreferred/NoRestriction policy; 0.9592 only supplies evidence, it does not select. | Keep as evidence |
| Provider marketplace/ecosystem catalog | Not required by HAgent's intended use cases. | Not part of V1 |
| Distributed provider-control/rate-limit service | Not required; HAgent should normalize local/provider-reported evidence and leave host/distributed infrastructure outside Core. | Not part of V1 |
| Autonomous provider-routing logic | Not required here; target selection belongs exclusively to 0.96. | Not part of V1 |
| Vendor-specific compatibility matrix as a second rules engine | Not required; adapter-specific behavior stays behind adapter contracts and normalized evidence. | Not part of V1 |

The audit therefore **keeps 0.9592 substantially intact but confirms its boundary**: the phase supplies adapters, discovery, provider-native evidence, and operational observations. It does not become a marketplace, billing system, distributed provider-control plane, or second routing engine.

## Delivery slices

### Slice 1 — Adapter contract and lifecycle

- Define provider adapter identity/version metadata.
- Support registration/creation, validation, use, refresh, disablement, replacement, and retirement.
- Keep adapter instances concurrency-safe or explicitly scoped.
- Keep transport/authentication/retry/provider-specific error handling inside adapters where appropriate.

### Slice 2 — Discovery and normalized metadata

- Support providers with complete, partial, or absent discovery APIs.
- Normalize models/execution targets without forcing logical-model correlation when it cannot be established reliably.
- Preserve provider-native model IDs, deployments, endpoints, accounts/projects, API versions, and provenance.
- Normalize capability evidence, constraints, cost information, and refresh timestamps.

### Slice 3 — Operational telemetry

- Normalize provider-reported rate/quota/usage information where available.
- Normalize health and availability evidence.
- Support incomplete telemetry without manufacturing defaults.
- Keep observed operational state separate from technical capability.

### Slice 4 — Adapter compatibility and change handling

- Record adapter/provider API compatibility metadata.
- Support deliberate adapter replacement without corrupting persisted agent configuration or historical execution records.
- Mark retired/deprecated targets unavailable without deleting historical identity.

### Slice 5 — Verification

Use deterministic fake providers to verify:

- complete discovery;
- partial discovery;
- unknown metadata;
- unsupported operations;
- API/adapter version changes;
- replacement/retirement;
- concurrent adapter use;
- quota/rate/health telemetry.

## Architectural rules

1. Core remains provider-neutral.
2. A provider describes transport/service integration, not agent behavior.
3. Model names are not sufficient execution identity; concrete targets remain distinct.
4. Unknown capability/cost/quota information remains unknown.
5. Adapter lifecycle does not become runtime-agent lifecycle.
6. Provider-specific behavior stays behind adapter boundaries.
7. 0.96 owns execution-target selection; this phase does not create a routing engine.
8. Provider credentials use the repository's simple encrypted provider-configuration mechanism; no separate secret-vault architecture is introduced.
9. Provider marketplace, distributed provider-control, and autonomous provider-routing responsibilities remain outside this phase.

## Dependencies

```text
0.9592 provider/adapters
        ↓
0.96.x configuration/storage
        ↓
0.96 capability-aware execution
```

The adapter contracts may be implemented incrementally, but 0.96 cannot depend on provider-specific APIs directly.

## Exit criterion

HAgent can register and use multiple provider adapters, preserve provider-native identities and metadata, consume complete or partial discovery/operational information through normalized contracts, represent unknowns honestly, and hand all execution-target selection to Phase 0.96.

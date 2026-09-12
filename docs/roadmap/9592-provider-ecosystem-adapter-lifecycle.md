# Phase 0.9592 — Provider Ecosystem and Adapter Lifecycle

## Status

**Planned after 0.959 Human Intervention and before 0.96 capability-aware execution.**

The ordered roadmap position is:

```text
0.9591 Goal / Plan Persistence + Recovery
        ↓
0.959 Human Intervention
        ↓
0.9592 Provider Ecosystem + Adapter Lifecycle
        ↓
0.96.x Configuration / Storage / Portability
        ↓
0.96 Capability-Aware Execution
```

This ordering is a delivery dependency for the V1 roadmap. 0.9592 does not need goal/plan persistence or intervention APIs as implementation inputs merely because it follows them in the ordered sequence; it is placed here so the provider boundary is complete before 0.96 consumes it.

## Purpose

Provide a clean provider-adapter boundary so HAgent can use multiple providers, API variants, models, and discovery sources without leaking provider-specific behavior into `HAgent.Core` or making provider integration larger than necessary.

This phase is a provider-platform foundation, not a provider marketplace.

## V1 provider model

```text
Provider Configuration
        ↓
Provider Adapter
 ├── execution transport
 ├── model / target discovery (when available)
 ├── capability evidence (when available)
 ├── quota / rate / usage telemetry (when available)
 ├── health / availability evidence
 └── provider-native metadata
        ↓
Normalized HAgent contracts
        ↓
0.96 Execution Planner
```

Unknown information remains `Unknown`; adapters do not invent capabilities, limits, costs, quotas, health, or compatibility claims.

## 2026-09-11 use-case audit

The intended desktop-application and HWorld use cases justify the core of 0.9592. HWorld explicitly intends to support different providers/models in one world, while HAgent's host architecture requires provider/model discovery and normalized execution-target information before 0.96 can make capability-aware selections. The current provider-limit/rate-capacity problems also make operational evidence a concrete requirement rather than a hypothetical platform feature.

| 0.9592 surface | Use-case result | 0.9592 decision |
|---|---|---|
| Multiple provider adapters | Required when different agents/actors use different providers/models concurrently. | Keep |
| Adapter registration/use/disable/retirement | Required for desktop configuration and for hosts that need to remove an unavailable provider without deleting historical identity. | Keep |
| Model/target discovery | Required to avoid hard-coding every provider's model catalog and to feed 0.96 target assessment. Partial/no discovery must remain supported. | Keep |
| Capability discovery/evidence | Required for 0.96 capability-aware selection; unknown capability must remain unknown. | Keep |
| Provider-native identities/metadata | Required because the same logical model may exist through multiple providers/accounts/deployments. | Keep |
| Quota/rate/usage telemetry | Directly justified by provider limits, shared capacity, and 429/rate-limit behavior encountered in HWorld-oriented execution. | Keep |
| Health/availability evidence | Needed to avoid repeatedly selecting unavailable execution targets. | Keep |
| Adapter/API compatibility and replacement | Needed for deliberate adapter replacement while preserving historical provider/target identity and configuration semantics. | Keep, bounded |
| Cost information | Needed by 0.96's FreeOnly/FreePreferred/NoRestriction policy; 0.9592 only supplies evidence, it does not select. | Keep as evidence |
| Provider marketplace/ecosystem catalog | Not required by the intended use cases and not part of the ordered 0.9592 responsibility. | Exclude from this phase |
| Distributed provider-control/rate-limit service | Not required; HAgent should normalize local/provider-reported evidence and leave host/distributed infrastructure outside Core. | Exclude from this phase |
| Autonomous provider-routing logic | Not required here; target selection belongs exclusively to 0.96. | Exclude from this phase |
| Vendor-specific compatibility matrix as a second rules engine | Not required; adapter-specific behavior stays behind adapter contracts and normalized evidence. | Exclude from this phase |

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
4. Unknown capability/cost/quota/health information remains unknown.
5. Adapter lifecycle does not become runtime-agent lifecycle.
6. Provider-specific behavior stays behind adapter boundaries.
7. 0.96 owns execution-target selection; this phase does not create a routing engine.
8. Provider credentials use the repository's simple encrypted provider-configuration mechanism; no separate secret-vault architecture is introduced.
9. Provider marketplace, distributed provider-control, and autonomous provider-routing responsibilities remain outside this phase.
10. 0.9592 publishes normalized provider evidence; it does not silently reinterpret that evidence into execution policy.
11. Configuration/storage portability remains a cross-cutting concern consumed through existing/future 0.96.x contracts; provider adapters must not create a parallel configuration or persistence subsystem.

## Dependencies and handoff

```text
Existing provider-neutral runtime/execution contracts
        ↓
0.9592 provider adapters / discovery / operational evidence
        ↓
0.96.x configuration, storage, and portability foundations
        ↓
0.96 capability-aware execution
```

The phase is intentionally ordered after 0.9591 and 0.959 in the roadmap, but its direct technical purpose is to complete the provider boundary before 0.96 consumes it. 0.96 must depend on normalized 0.9592 contracts rather than provider-specific APIs.

## Exit criterion

HAgent can register and use multiple provider adapters, preserve provider-native identities and metadata, consume complete or partial discovery/operational information through normalized contracts, represent unknowns honestly, preserve adapter lifecycle/history, and hand all execution-target selection to Phase 0.96 without creating a parallel routing or configuration authority.

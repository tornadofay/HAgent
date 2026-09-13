# Learned Resource Reliability + Adaptation

## Scope

Phase 0.9576 defines the post-promotion reliability and adaptation boundary for learned resource versions. Slice 2 records validated operational outcomes against the exact promoted resource identity without changing the published Skill, Knowledge, or Memory resource itself. Slice 3 adds lifecycle assessment, revalidation, quarantine/recovery, and replacement-candidate creation without mutating the published resource.

Reliability, applicability, lifecycle, and authorization remain separate dimensions. A resource may be authorized, applicable, and still have low reliability; reliability evidence never grants permission to use a resource. A non-current lifecycle state is not automatically usable merely because the resource remains authorized.

## Boundary

```text
promoted resource version
        |
        +--> promotion evidence
        |
        +--> operational reliability evidence
        |
        v
IAiResourceReliabilityStore          IAiLearnedResourceLifecycleStore
        ^                                      ^
        |                                      |
AiResourceReliabilityService        AiLearnedResourceLifecycleService
        ^                                      ^
        |                                      |
validated operational outcome        applicability + reliability + evaluation
        |                                      |
IAiPolicyEngine ---------------- policy-controlled lifecycle mutation
                                               |
                                               +--> typed replacement candidate
```

Reliability owns evidence aggregation and revision-safe metadata updates. Lifecycle owns post-promotion condition assessment and lifecycle transitions. Neither subsystem publishes or silently mutates the canonical resource definition.

## Canonical identity

`AiResourceReliabilityIdentity` identifies the resource by:

- resource type;
- resource ID;
- optional published version;
- explicit resource scope.

Reliability and lifecycle records use that identity. This prevents an outcome or revalidation result for one resource version from silently modifying state for another version.

## Evidence separation

Reliability metadata contains two explicitly separate evidence collections:

1. **Promotion evidence** — evidence that the resource passed the 0.9575 governed promotion boundary.
2. **Operational outcome evidence** — later host-validated observations about how the already-promoted version performed.

Operational evidence preserves bounded execution, runtime, agent-profile, and evaluation-reference provenance when supplied by the host.

Model output is not accepted as reliability evidence merely because it asserts success or failure. `AiValidatedResourceOutcome.IsValidated` must be true and a bounded host validation method/evidence summary must be supplied.

## Deterministic reliability update

`AiResourceReliabilityService` applies bounded deterministic deltas:

- `Success`: +0.05;
- `Failure`: -0.10;
- `InvalidPrecondition`: -0.15;
- `Contradiction`: -0.25.

Scores are clamped to `[0, 1]`.

These deltas are implementation defaults for Slice 2, not absolute truth. The policy boundary may allow or deny an update, and later lifecycle policies may add thresholds or decay behavior.

A score below `0.50` requests review. A contradiction or score at/below `0.25` produces `QuarantineRecommended`. Slice 2 records the recommendation; Slice 3 is responsible for actual lifecycle/quarantine transitions.

## Applicability and revalidation

Slice 3 consumes the existing provider-neutral applicability and evaluation contracts rather than creating a second evaluator. Revalidation is performed against the exact resource identity and bounded request inputs:

- previous and current applicability decisions;
- current reliability evidence;
- optional `AiEvaluation` result;
- evaluation timestamp and maximum age;
- explicit policy identity.

Condition precedence is deterministic:

`Contradicted → Drifted → Degraded → Stale → Current`.

`Stale` is age-based, `Degraded` is reliability evidence indicating review/quarantine concern, `Drifted` is a changed applicability result, and `Contradicted` is direct contradiction evidence. Missing or insufficient evidence does not silently become a positive current state.

## Lifecycle

Slice 3 provides bounded lifecycle states:

- `Active` — currently valid for normal use;
- `UnderReview` — requires revalidation/review before normal confidence is restored;
- `Quarantined` — blocked from automatic use after contradiction or quarantine-worthy evidence;
- `Retired` — explicitly terminal and not automatically reactivated.

`AiLearnedResourceLifecycleRecord` is provider-neutral, revisioned, and retains bounded transition history. Lifecycle mutation is controlled through `IAiPolicyEngine` and compare-and-swap persistence via `IAiLearnedResourceLifecycleStore`.

`IsAutomaticallyUsable` is true only for an `Active` resource whose last lifecycle condition is `Current`. `UnderReview`, `Quarantined`, and `Retired` therefore cannot be silently treated as normal usable learned behavior.

A clean revalidation may recover an `UnderReview` or `Quarantined` resource to `Active`. A retired resource remains retired until a future explicit governed replacement/reinstatement architecture exists.

## Policy boundary

Operational reliability updates are evaluated through `IAiPolicyEngine` using the provider-neutral operation `resource.reliability.record-outcome`.

Lifecycle revalidation is separately evaluated through `IAiPolicyEngine` using the provider-neutral operation `resource.lifecycle.revalidate`. Policy denial prevents lifecycle mutation and leaves the prior lifecycle record unchanged.

Policy remains an authorization/enforcement boundary. Reliability and lifecycle are evidence/state dimensions, not substitute authorization systems.

## Revision safety

Reliability records carry a monotonically increasing revision. `IAiResourceReliabilityStore.TryUpdateAsync` is compare-and-swap style: the update succeeds only when the stored revision still equals the caller's expected revision.

Lifecycle records use the same compare-and-swap principle through `IAiLearnedResourceLifecycleStore`. A stale lifecycle update is rejected rather than overwriting a newer assessment.

## Replacement and adaptation

Slice 3 never edits a published Skill, Knowledge, or Memory definition in place. When revalidation indicates drift or another replacement-worthy condition, `AiLearnedResourceLifecycleService` creates a new typed `AiLearningCandidate` through the existing governed candidate boundary.

The candidate carries bounded scope, evidence, provenance, confidence, candidate type, and source execution/runtime/agent identity when available. Candidate promotion remains owned by the 0.9575 learning governance boundary. Creating a replacement candidate does not publish it and does not change the original resource identity or version.

## Storage boundary

`IAiResourceReliabilityStore` and `IAiLearnedResourceLifecycleStore` are provider-neutral persistence boundaries. Their in-memory reference implementations provide deterministic storage for tests and manual Example verification.

The canonical resource subsystem remains the owner of Memory/Knowledge/Skill definitions. Reliability and lifecycle storage contain metadata, evidence references, and transition history—not executable handlers or duplicate resource definitions.

## Deferred responsibilities

Later 0.9576 slices continue to own:

- policy-governed utility/retention decay;
- archival and retirement workflows beyond the bounded Slice 3 lifecycle contract;
- runtime execution fallback and execution-snapshot integration;
- broader resource-management UI and storage-specific lifecycle implementations.

## Governance invariants

1. Reliability never grants authorization.
2. Promotion evidence remains distinct from operational outcome evidence.
3. Only host-validated outcomes may update reliability.
4. Operational outcome provenance remains bounded and preserved when supplied.
5. Reliability or lifecycle updates cannot mutate the published resource version.
6. Reliability and lifecycle transitions are policy-controlled.
7. Reliability and lifecycle updates are revision-safe; stale asynchronous updates cannot overwrite newer state.
8. Non-current lifecycle states are not automatically usable.
9. Replacement/adaptation produces new governed candidates rather than hidden in-place mutation.
10. Reliability, applicability, lifecycle, and authorization remain independently observable dimensions.

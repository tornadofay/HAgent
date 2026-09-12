# Learned Resource Reliability

## Scope

Phase 0.9576 Slice 2 defines the post-promotion reliability evidence boundary for learned resource versions. It records validated operational outcomes against the exact promoted resource identity without changing the published Skill, Knowledge, or Memory resource itself.

Reliability is separate from authorization and applicability. A resource may be authorized, applicable, and still have low reliability; reliability evidence never grants permission to use a resource.

## Boundary

```text
promoted resource version
        |
        +--> promotion evidence
        |
        v
IAiResourceReliabilityStore
        ^
        |
AiResourceReliabilityService
        ^
        |
validated operational outcome + source provenance
        |
IAiPolicyEngine  ---- policy-controlled reliability update
```

The reliability service owns evidence aggregation and revision-safe metadata updates. It does not publish, mutate, authorize, quarantine, retire, archive, or replace the canonical resource version.

## Canonical identity

`AiResourceReliabilityIdentity` identifies the resource by:

- resource type;
- resource ID;
- optional published version;
- explicit resource scope.

Reliability records are keyed by that identity. This prevents an outcome for one resource version from silently modifying reliability state for another version.

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

These deltas are implementation defaults for this slice, not absolute truth. The policy boundary may allow or deny an update, and later slices may introduce additional policy-defined thresholds or decay behavior.

A score below `0.50` requests review. A contradiction or score at/below `0.25` produces `QuarantineRecommended`. Slice 2 records the recommendation; actual lifecycle/quarantine state transitions belong to Slice 3.

## Policy boundary

Every operational reliability update is evaluated through `IAiPolicyEngine` using the provider-neutral operation `resource.reliability.record-outcome`.

The policy context includes the resource version/scope, outcome kind, validation marker, current score, and proposed score delta. A denied policy decision leaves the stored reliability record unchanged.

Reliability state is therefore evidence consumed by policy, not a second authorization system.

## Revision safety

Reliability records carry a monotonically increasing revision. `IAiResourceReliabilityStore.TryUpdateAsync` is compare-and-swap style: the update succeeds only when the stored revision still equals the caller's expected revision.

A stale update is rejected rather than overwriting newer reliability evidence. Implementations may retry from the latest record; Slice 2 does not hide such conflicts behind an unconditional last-write-wins update.

## Storage boundary

`IAiResourceReliabilityStore` is the provider-neutral persistence boundary. `InMemoryAiResourceReliabilityStore` is the deterministic reference implementation used by tests and Example verification.

The resource itself remains owned by the canonical Memory/Knowledge/Skill subsystem. Reliability storage contains only reliability metadata and evidence references, not executable Skill handlers or a duplicate resource definition.

## Deferred responsibilities

Slice 2 does not implement:

- age-based staleness or contextual drift;
- direct contradiction detection beyond an explicit validated outcome signal;
- actual quarantine/retirement/archive lifecycle transitions;
- automatic replacement candidate creation;
- reliability decay over time;
- runtime execution fallback or execution-snapshot integration.

Those remain later 0.9576 slices.

## Governance invariants

1. Reliability never grants authorization.
2. Promotion evidence remains distinct from operational outcome evidence.
3. Only host-validated outcomes may update reliability.
4. Operational outcome provenance remains bounded and preserved when supplied.
5. Reliability changes cannot mutate the published resource version.
6. Reliability updates are policy-controlled.
7. Reliability updates are revision-safe and stale asynchronous updates cannot overwrite newer state.
8. Reliability remains provider-neutral and usable without GPU, embeddings, or vector databases.

# Learned Resource Lifecycle and Revalidation

## Purpose

Phase 0.9576 Slice 3 adds a provider-neutral post-promotion lifecycle boundary for learned resources. It evaluates age, reliability, applicability, and validated evaluation evidence without mutating the authoritative Skill, Knowledge, Memory, or other published resource version.

## Identity

Lifecycle state is keyed by the existing `AiResourceReliabilityIdentity`:

- resource type;
- resource ID;
- published resource version;
- explicit resource scope.

A lifecycle record never becomes a replacement resource identity.

## Lifecycle states

`AiLearnedResourceLifecycleStatus` contains:

- `Active` — current evidence supports automatic use;
- `UnderReview` — freshness, reliability, evaluation, or contextual evidence requires governed review;
- `Quarantined` — direct contradiction or explicit invalidation prevents automatic use;
- `Retired` — terminal state owned by later forgetting/archival governance.

`AiLearnedResourceLifecycleRecord.IsAutomaticallyUsable` is true only when status is `Active` and the last condition is `Current`.

Lifecycle state is not authorization. A resource must still pass the existing authorization/policy boundary.

## Conditions

`AiLearnedResourceCondition` distinguishes:

- `Current`;
- `Stale` — the configured maximum age since promotion has elapsed;
- `Degraded` — reliability requests review, quarantine is recommended, or evaluation produces a failed/needs-review result, including uncertain applicability;
- `Drifted` — a previously applicable resource is now not applicable in the current bounded context;
- `Contradicted` — current applicability is explicitly invalidated or validated reliability evidence records a contradiction.

Condition detection is deterministic and ordered by safety precedence:

```text
Contradicted → Drifted → Degraded → Stale → Current
```

Missing or uncertain evidence never upgrades a resource to `Current` automatically.

## Revalidation

`AiLearnedResourceRevalidationRequest` carries:

- the exact resource identity;
- current reliability record;
- previous and current applicability decisions;
- optional existing `AiEvaluation` output;
- evaluation time and bounded maximum age;
- policy identity.

`AiLearnedResourceLifecycleService.RevalidateAsync`:

1. reads the latest lifecycle revision;
2. preserves `Retired` as terminal;
3. determines the condition deterministically;
4. derives the target lifecycle state;
5. evaluates the existing `IAiPolicyEngine` boundary before persistence;
6. records bounded lifecycle history and evidence references;
7. increments the lifecycle revision and commits with compare-and-swap;
8. returns an auditable result.

Contradiction transitions to `Quarantined`. Stale, degraded, and drifted resources transition to `UnderReview`. Clean current evidence can restore a non-terminal quarantined or review state to `Active`.

## Replacement and revision

`AiLearnedResourceLifecycleService.CreateReplacementCandidateAsync` creates an `AiLearningCandidate` rather than modifying a published resource.

The candidate retains:

- replacement provenance;
- evidence explaining why replacement is proposed;
- source execution/runtime/agent identifiers;
- candidate type and proposed scope.

The normal 0.9575 candidate review/approval/promotion lifecycle remains the only route from candidate to authoritative learned resource.

## Persistence and concurrency

`IAiLearnedResourceLifecycleStore` is provider-neutral and exposes:

- read by exact resource identity;
- create-if-absent;
- compare-and-swap update by expected revision.

`InMemoryAiLearnedResourceLifecycleStore` is the deterministic reference implementation used by tests and Example verification.

History is bounded to 64 lifecycle transitions. Reads and writes use owned clones so callers cannot mutate store state without an explicit update.

## Policy boundary

Lifecycle mutation uses the existing `IAiPolicyEngine` with explicit operations:

- `resource.lifecycle.revalidate`;
- `resource.lifecycle.propose-replacement`.

Policy denial prevents lifecycle mutation. Reliability and lifecycle evidence do not grant authorization.

## Safety boundary

The Slice 3 lifecycle layer blocks automatic use through its lifecycle state contract, not by silently changing the authoritative resource or bypassing runtime authorization.

Runtime execution consumption/integration remains owned by 0.9576 Slice 5.

## Tests and Example

Focused tests:

`tests/HAgent.Tests/LearnedResourceAdaptationTests.cs`

The suite covers stale, degraded, drifted, contradiction/quarantine, clean recovery, retirement preservation, policy denial, compare-and-swap revision safety, replacement candidate creation, and cancellation.

Matching manual Example:

`src/HAgent.Example/MainForm.LearnedResourceAdaptation.cs`

Example title:

`LEARNED RESOURCE ADAPTATION`

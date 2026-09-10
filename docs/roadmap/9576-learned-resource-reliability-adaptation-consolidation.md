# Phase 0.9576 — Learned Resource Reliability, Adaptation + Consolidation

## Status

**Planned — follows Phase 0.9575.**

## Goal

Provide the post-promotion reliability layer for learned Skills, Knowledge, and other learned resources so that learning remains adaptive, reversible, bounded, and operationally trustworthy after a candidate has been promoted.

Phase 0.9575 governs how experience becomes a validated, policy-authorized learning candidate and how that candidate may be promoted. Phase 0.9576 governs what happens **after promotion**: whether the learned resource remains applicable, how evidence changes its trust, how environmental or contextual drift is detected, and how accumulated learned resources are consolidated or retired.

This phase must not introduce a second resource architecture. It consumes the canonical resource, learning, policy, evaluation, provenance, versioning, retention, and capability contracts established by earlier phases.

## Architectural position

```text
Experience / Observation
        ↓
0.9575 Learning Candidate
        ↓
validation / evaluation / policy / approval
        ↓
Promotion → authoritative resource version
        ↓
0.9576 Post-promotion reliability
        ├── applicability assessment
        ├── trust / confidence revision
        ├── outcome feedback
        ├── drift / contradiction detection
        ├── revalidation / revision / retirement
        └── consolidation / deduplication / forgetting
```

The promoted resource remains versioned and authoritative only within its current validity and policy boundaries. Learned behavior must never become permanently trusted merely because it was once approved.

## Core principles

1. [ ] Treat promoted learned resources as versioned, evidence-backed hypotheses whose continued validity can change over time.
2. [ ] Keep resource confidence/trust distinct from current applicability to a particular situation.
3. [ ] Never require an LLM to determine whether a learned resource is applicable when deterministic evidence and policy can establish the answer.
4. [ ] Allow an applicable learned resource to execute deterministically without requiring model reasoning.
5. [ ] Allow `Uncertain` or otherwise insufficient applicability to escalate to deliberation/reasoning rather than silently applying the learned behavior.
6. [ ] Preserve provenance and evaluation evidence across the post-promotion lifecycle.
7. [ ] Make failures, contradictions, environmental drift, and other outcome evidence capable of weakening trust or invalidating a learned resource.
8. [ ] Keep reliability updates versioned, auditable, policy-controlled, and concurrency-safe.
9. [ ] Do not silently mutate a published Skill version or authoritative Knowledge version in place.
10. [ ] Keep consolidation provider-neutral and implementation-pluggable; clustering, rule merging, symbolic generalization, statistical methods, or other techniques are implementation choices rather than architectural requirements.
11. [ ] Keep resource growth bounded through consolidation, archival, expiration, and utility-aware forgetting according to policy.
12. [ ] Preserve compatibility with low-RAM/no-GPU operation and do not require vector databases or embeddings.

## Applicability and validity

13. [ ] Define a provider-neutral applicability/validity contract for learned resources where applicable.
14. [ ] Support explicit applicability outcomes such as `Applicable`, `NotApplicable`, `Uncertain`, and `Invalidated`.
15. [ ] Allow a learned resource to define or reference bounded applicability conditions, preconditions, context features, scope, and evidence requirements without embedding host-domain semantics into Core.
16. [ ] Distinguish resource validity from authorization/capability state. An authorized resource may still be inapplicable or invalidated.
17. [ ] Support deterministic applicability checks before model execution where sufficient evidence exists.
18. [ ] Define a safe fallback path for `Uncertain`, unsupported, conflicting, or invalidated learned behavior to existing deliberative/reasoning mechanisms.
19. [ ] Preserve the applicability decision and its evidence in observability/evaluation metadata where applicable.
20. [ ] Keep applicability bounded by context/resource budgets and policy.

## Trust, confidence, and outcome feedback

21. [ ] Define provider-neutral post-promotion trust/evidence state without replacing the canonical resource version contract.
22. [ ] Distinguish at minimum static promotion evidence from subsequent operational outcome evidence.
23. [ ] Support reinforcement from validated successful outcomes where policy permits.
24. [ ] Support weakening from failed outcomes, contradictory evidence, invalid preconditions, or other reliability signals.
25. [ ] Support policy-defined confidence/trust decay or review requirements over time and usage where appropriate.
26. [ ] Preserve outcome provenance including source execution/runtime identity and relevant evaluation evidence where available.
27. [ ] Ensure trust changes cannot by themselves grant authorization or bypass capability policy.
28. [ ] Allow configurable thresholds for automatic continued use, review, revalidation, quarantine, or retirement.

## Drift, staleness, contradiction, and revalidation

29. [ ] Distinguish age-based staleness, contextual/environmental drift, observed performance degradation, and direct contradiction.
30. [ ] Detect or accept deterministic signals indicating that the current situation differs materially from the learned resource's validated conditions.
31. [ ] Support explicit stale/under-review/quarantined/retired lifecycle states where appropriate to the resource type.
32. [ ] Allow contradiction or drift evidence to block automatic application pending revalidation.
33. [ ] Support re-evaluation of degraded or stale learned resources using the existing Evaluation/Quality Measurement contracts.
34. [ ] Allow re-learning/revision to produce a new typed candidate rather than mutating the existing authoritative version in place.
35. [ ] Preserve historical versions and provenance needed to explain why a learned resource was strengthened, weakened, revised, quarantined, or retired.
36. [ ] Make revalidation triggers and outcomes auditable and observable.

## Consolidation and redundancy control

37. [ ] Define a provider-neutral consolidation boundary for learned resources and candidates.
38. [ ] Detect substantially overlapping, duplicate, equivalent, or contradictory learned resources without requiring one fixed similarity/indexing technology.
39. [ ] Support safe merge/generalization proposals as candidates rather than silently replacing authoritative resources.
40. [ ] Preserve provenance and source history when resources are merged or superseded.
41. [ ] Define conflict-resolution requirements for overlapping learned resources with different scope, authority, evidence, confidence, or versions.
42. [ ] Prevent consolidation from crossing authorization, ownership, tenant, or other scope boundaries unless explicitly permitted.
43. [ ] Support consolidation scheduling independently from request-time inference so routine agent execution does not become blocked by maintenance work.
44. [ ] Keep consolidation bounded by configurable time, memory, candidate count, and storage budgets.

## Forgetting, archival, and utility

45. [ ] Define policy-governed utility signals for learned resources, such as validated use, outcome quality, recency, redundancy, scope, and maintenance cost where applicable.
46. [ ] Support archival or retirement of stale, redundant, contradicted, superseded, or persistently low-utility learned resources.
47. [ ] Preserve enough provenance/retention metadata to explain retirement without requiring indefinite storage of discarded content.
48. [ ] Ensure forgetting cannot remove a higher-authority resource merely because a lower-utility duplicate exists.
49. [ ] Keep forgetting reversible where required by policy through retained version/archival metadata.

## Runtime integration

50. [ ] Allow Persistent Cognitive Runtime and other consumers to request applicability/reliability evaluation without implementing a parallel learned-resource system.
51. [ ] Integrate `Uncertain`/`Invalidated` learned-resource outcomes with the existing deterministic-versus-deliberative escalation model.
52. [ ] Ensure runtime execution snapshots capture the exact resource version and reliability state required for deterministic reproducibility.
53. [ ] Ensure post-execution outcome feedback cannot overwrite newer resource/reliability revisions through stale asynchronous work.
54. [ ] Keep reliability evaluation separate from host side-effect authorization; the host remains authoritative over external effects.

## Evaluation and observability

55. [ ] Extend existing evaluation contracts so learned-resource applicability and operational reliability can be measured deterministically where possible.
56. [ ] Measure false application, missed application, invalidation, contradiction, successful reuse, and escalation-to-reasoning outcomes where the host can supply suitable evidence.
57. [ ] Make resource reliability trends inspectable without exposing secrets or unnecessary sensitive payloads.
58. [ ] Preserve correlation among resource version, learning candidate, execution, runtime, outcome, and revalidation/revision event where applicable.
59. [ ] Ensure reliability metrics cannot be mistaken for absolute truth; they remain evidence used by policy and evaluation.

## Verification

60. [ ] Add deterministic tests for applicability outcomes and safe fallback behavior.
61. [ ] Add tests for trust reinforcement, weakening, decay/review thresholds, and invalidation.
62. [ ] Add tests for stale, contradicted, and drifted resources and creation of replacement candidates.
63. [ ] Add tests for duplicate/overlapping resource detection and policy-safe consolidation proposals.
64. [ ] Add tests for scope/ownership boundaries during consolidation and forgetting.
65. [ ] Add tests for version/snapshot isolation when reliability state changes during active execution.
66. [ ] Add Example verification for learned-resource applicability, fallback to reasoning, reliability revision, and lifecycle transitions.
67. [ ] Add Example verification for consolidation/retirement behavior through public HAgent APIs.
68. [ ] Verify all supported framework targets required by HAgent before declaring the phase complete.

## Relationship to adjacent phases

- **0.9575** defines governed learning intake, candidate validation, evaluation, approval, promotion, and resource governance.
- **0.9576** defines the continued reliability, adaptation, consolidation, and forgetting lifecycle of already-promoted learned resources.
- **0.958** consumes reliable resource state as part of agent health/lifecycle management where appropriate.
- **0.96** provides capability-aware execution and routing primitives that can be used when learned behavior escalates to reasoning.
- **0.97** consumes this phase so persistent cognition can use deterministic learned behavior without treating old learned rules as permanently correct.

## Exit criterion

Promoted learned resources remain versioned, provenance-preserving, policy-governed, and operationally observable after promotion. HAgent can determine when learned behavior is applicable, avoid applying it when applicability is uncertain or invalid, fall back to reasoning when necessary, revise trust from validated outcomes, detect staleness/drift/contradiction, create replacement candidates without mutating authoritative versions in place, and control long-term resource growth through safe consolidation, archival, and forgetting.

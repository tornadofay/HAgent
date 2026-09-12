# Phase 0.9576 — Learned Resource Reliability, Adaptation + Forgetting

## Status

**In progress — Slice 2: Reliability evidence and outcome feedback.**

## Purpose

Provide the post-promotion reliability layer for learned Skills, Knowledge, and other learned resources.

0.9575 governs how experience becomes a validated candidate and how that candidate is promoted. 0.9576 governs whether the promoted resource remains safe, applicable, and useful afterward.

This phase is deliberately practical: it provides lifecycle/reliability controls needed by production V1 without turning HAgent into a research system for large-scale knowledge consolidation or neural continual learning.

## V1 outcome

A promoted resource is never trusted forever merely because it was once approved.

HAgent distinguishes:

```text
Authorization      = may this resource be used?
Applicability      = does it fit this situation?
Reliability        = how much evidence supports continued trust?
Lifecycle          = is it active, stale, quarantined, archived, retired?
```

These dimensions remain separate.

## Delivery slices

### Slice 1 — Applicability and validity — VERIFIED

Verified by the user on 2026-09-12 on .NET Framework 4.8.1 and .NET 9.

- provider-neutral applicability results: `Applicable`, `NotApplicable`, `Uncertain`, `Invalidated`;
- bounded applicability conditions, preconditions, scope, and evidence references;
- deterministic evaluation before model reasoning when sufficient evidence exists;
- applicability independent from authorization/capability state;
- applicability decisions preserve resource-version identity, condition results, and evidence references;
- dedicated tests and Example verification.

### Slice 2 — Reliability evidence and outcome feedback — CURRENT

- Define post-promotion reliability metadata without replacing resource version identity.
- Distinguish promotion evidence from operational outcome evidence.
- Reinforce reliability from host-validated successful outcomes where policy permits.
- Weaken reliability after validated failures and invalid preconditions.
- Mark contradiction evidence with a quarantine recommendation without performing lifecycle mutation in this slice.
- Preserve execution/runtime/agent provenance and bounded evaluation references.
- Keep reliability updates policy-controlled and revision-safe.

**Architecture:** `docs/architecture/98-learned-resource-reliability.md`.

**Tests to run:** `HAgent.Tests → LearnedResourceReliabilityTests.cs` focused first, then the full `HAgent.Tests` suite.

**Example to run:** `HAgent.Example → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

Verification is pending user execution.

### Slice 3 — Staleness, contradiction, and revalidation

- Distinguish age-based staleness, observed degradation, contextual drift, and direct contradiction.
- Support bounded lifecycle states such as `Active`, `UnderReview`, `Quarantined`, and `Retired` where appropriate.
- Block automatic use of invalidated or contradictory learned behavior.
- Re-evaluate degraded/stale resources through existing Evaluation contracts.
- Produce replacement/revision as new typed candidates rather than mutating published resources in place.
- Preserve historical versions/provenance required to explain changes.

### Slice 4 — Forgetting and archival

- Define policy-governed utility/retention signals.
- Archive or retire stale, superseded, contradicted, or persistently low-utility resources.
- Preserve bounded provenance explaining retirement.
- Never remove a higher-authority resource merely because a lower-utility duplicate exists.
- Keep archival recovery possible where policy requires it.

### Slice 5 — Runtime integration and verification

- Expose reliability/applicability outcomes through the same resource/policy boundaries used elsewhere.
- Ensure execution snapshots capture the resource version/reliability state required for deterministic reproducibility.
- Reject stale asynchronous reliability updates against newer revisions.
- Fall back safely to deterministic behavior, another resource, bounded reasoning, or host escalation when learned behavior is uncertain or invalid.
- Verify supported framework targets and risk-sensitive boundaries.

## V1 safety rules

1. Reliability never grants authorization.
2. An authorized resource can still be inapplicable or invalidated.
3. `Uncertain` never means `Applicable`.
4. Published Skill and Knowledge versions are not silently mutated.
5. Reliability updates are revision-safe and auditable.
6. Learned-resource failure never forces an unsafe fallback.
7. Reliability remains usable without GPU, embeddings, or vector databases.
8. Reliability operates on already-promoted resource versions; it does not bypass the 0.9575 candidate and promotion boundary.
9. Replacement and adaptation produce new governed candidates/resources rather than hidden in-place mutation.

## Ownership boundary

0.9575 owns candidate creation, review, authorization, and authoritative promotion.

0.9576 owns post-promotion applicability, reliability evidence, degradation/quarantine, revalidation, forgetting, and replacement signals.

0.958 owns runtime lifecycle/health; it may consume reliability evidence but does not become the learned-resource evaluator.

0.97 consumes reliable learned behavior during cognition but does not create a second reliability or learning architecture.

## Dependency chain

```text
0.9575 governed learning + promotion
        ↓
0.9576 learned resource reliability + adaptation
        ↓
0.958 lifecycle + health
        ↓
0.97 deterministic learned behavior + deliberation
```

## Exit criterion

HAgent can determine whether promoted learned behavior is applicable and trustworthy, refuse or quarantine stale/contradictory behavior, incorporate validated outcomes into reliability evidence, produce replacement candidates without mutating authoritative versions, and safely forget/archive learned resources without confusing reliability with authorization or bypassing governed promotion.

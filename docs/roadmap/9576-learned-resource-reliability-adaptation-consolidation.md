# Phase 0.9576 — Learned Resource Reliability, Adaptation + Forgetting

## Status

**In progress — Slice 4: Forgetting and archival.**

## Purpose

Provide the post-promotion reliability layer for learned Skills, Knowledge, and other learned resources.

0.9575 governs how experience becomes a validated candidate and how that candidate is promoted. 0.9576 governs whether the promoted resource remains safe, applicable, useful, and appropriately retained afterward.

This phase is deliberately practical: it provides lifecycle/reliability controls needed by production V1 without turning HAgent into a research system for large-scale knowledge consolidation or neural continual learning.

## V1 outcome

A promoted resource is never trusted forever merely because it was once approved.

HAgent distinguishes:

```text
Authorization      = may this resource be used?
Applicability      = does it fit this situation?
Reliability        = how much evidence supports continued trust?
Lifecycle          = is it active, stale, quarantined, archived, retired?
Retention          = should it remain available, be archived, or be retired?
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

### Slice 2 — Reliability evidence and outcome feedback — VERIFIED

Verified by the user on 2026-09-13 on .NET Framework 4.8.1 and .NET 9. The user also reported the full `.NET 9` `HAgent.Tests` regression result as 242/242 passed, 0 failed, 0 skipped.

- `AiResourceReliabilityIdentity` keys evidence to exact resource type, ID, published version, and scope;
- promotion evidence remains distinct from operational outcome evidence;
- only host-validated outcomes can change reliability;
- bounded reliability score and outcome counts;
- review and quarantine recommendations remain separate from lifecycle mutation;
- provider-neutral compare-and-swap persistence;
- policy-controlled reliability updates;
- success/failure/invalid-precondition/contradiction evidence with deterministic score deltas;
- execution/runtime/agent/evaluation provenance preserved;
- no authoritative resource version mutation.

**Architecture:** `docs/architecture/98-learned-resource-reliability.md`.

**Example:** `HAgent.Example → Cognition → Learning → Learned Resource Reliability` on .NET Framework 4.8.1 and .NET 9.

**Tests:** `HAgent.Tests → LearnedResourceReliabilityTests.cs` plus full regression.

### Slice 3 — Staleness, contradiction, and revalidation — VERIFIED

User verified `HAgent.Example → Cognition → Learning → LEARNED RESOURCE ADAPTATION` on .NET Framework 4.8.1 and .NET 9 on 2026-09-13. The user also reported the full `.NET 9` `HAgent.Tests` regression suite at **252/252 passed, 0 failed, 0 skipped**.

Implemented and verified:

- deterministic distinction between age-based staleness, observed degradation, contextual drift, and direct contradiction;
- bounded lifecycle states `Active`, `UnderReview`, `Quarantined`, and `Retired`;
- explicit automatic-use gate requiring `Active + Current`;
- existing Evaluation and Applicability contracts are consumed rather than duplicated;
- lifecycle transitions are policy-controlled and compare-and-swap revision-safe;
- clean revalidation can recover review/quarantine state while retired resources remain retired;
- replacement/revision is represented as a new typed learning candidate;
- lifecycle history, reliability/applicability/evaluation signals, and resource-version identity remain bounded and preserved;
- published resource versions remain unchanged.

**Architecture:** `docs/architecture/99-learned-resource-lifecycle.md`, with consolidated reliability/adaptation rules in `docs/architecture/98-learned-resource-reliability.md`.

**Example:** `HAgent.Example → Cognition → Learning → LEARNED RESOURCE ADAPTATION` on .NET Framework 4.8.1 and .NET 9.

**Tests:** `HAgent.Tests → LearnedResourceAdaptationTests.cs`; full suite reported 252/252 passed on .NET 9.

### Slice 4 — Forgetting and archival — CURRENT

Entry condition: Slice 3 is verified complete.

Implement the provider-neutral retention/utility boundary for already-promoted learned resources.

- Define bounded utility and retention signals without conflating retention with authorization or applicability.
- Determine when stale, superseded, contradicted, or persistently low-utility resources become eligible for archival/retirement.
- Preserve bounded provenance explaining retention decisions and state changes.
- Preserve higher-authority resources when lower-utility competing resources exist.
- Keep archival recovery possible where policy requires it.
- Keep retention decisions policy-controlled and revision-safe.
- Keep authoritative resource definitions unchanged; retention state belongs to the lifecycle boundary.
- Register the matching Example through the existing Learning tab registration path.

**Architecture:** `docs/architecture/100-learned-resource-retention.md`.

**Focused tests:** `HAgent.Tests → LearnedResourceRetentionTests.cs`.

**Example:** `HAgent.Example → Cognition → Learning → LEARNED RESOURCE RETENTION` on .NET Framework 4.8.1 and .NET 9.

### Slice 5 — Runtime integration and verification

- Expose reliability/applicability/retention outcomes through the same resource/policy boundaries used elsewhere.
- Ensure execution snapshots capture the resource version/reliability state required for deterministic reproducibility.
- Reject stale asynchronous reliability updates against newer revisions.
- Fall back safely to deterministic behavior, another resource, bounded reasoning, or host escalation when learned behavior is uncertain or invalid.
- Verify supported framework targets and risk-sensitive boundaries.

## V1 safety rules

1. Reliability never grants authorization.
2. An authorized resource can still be inapplicable or invalidated.
3. `Uncertain` never means `Applicable`.
4. Published Skill and Knowledge versions are not silently mutated.
5. Reliability and retention updates are revision-safe and auditable.
6. Learned-resource failure never forces an unsafe fallback.
7. Reliability remains usable without GPU, embeddings, or vector databases.
8. Reliability operates on already-promoted resource versions; it does not bypass the 0.9575 candidate and promotion boundary.
9. Replacement, adaptation, retention, and retirement produce governed state/candidates rather than hidden in-place mutation.
10. Retention protects higher-authority resources when lower-utility duplicates exist.

## Ownership boundary

0.9575 owns candidate creation, review, authorization, and authoritative promotion.

0.9576 owns post-promotion applicability, reliability evidence, degradation/quarantine, revalidation, forgetting, archival, and replacement signals.

0.958 owns runtime lifecycle/health; it may consume reliability evidence but does not become the learned-resource evaluator.

0.97 consumes reliable learned behavior during cognition but does not create a second reliability or learning architecture.

## Dependency chain

```text
0.9575 governed learning + promotion
        ↓
0.9576 learned resource reliability + adaptation + retention
        ↓
0.958 lifecycle + health
        ↓
0.97 deterministic learned behavior + deliberation
```

## Exit criterion

HAgent can determine whether promoted learned behavior is applicable and trustworthy, refuse or quarantine stale/contradictory behavior, incorporate validated outcomes into reliability evidence, produce replacement candidates without mutating authoritative versions, and safely forget/archive learned resources without confusing reliability with authorization or bypassing governed promotion.
